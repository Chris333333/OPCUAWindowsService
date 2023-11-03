using Application.DTOs;
using Data.DTOs;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Serilog;
using System.Text;

namespace Application.Clients
{
    /// <summary>
    /// Overall main OPCUA client that sets up main logic, notifications and certificates
    /// </summary>
    public abstract class OPCUAClient : IDisposable
    {
        protected string ServerAddress { get; set; } = string.Empty;
        protected string ServerPortNumber { get; set; } = string.Empty;
        protected bool SecurityEnabled { get; set; }
        protected string MyApplicationName { get; set; } = string.Empty;
        protected Session? OPCSession { get; set; } = null!;
        public Dictionary<NodeId, OPCUATag> TagList { get; set; } = null!;
        protected bool SessionRenewalRequired { get; set; }
        protected double SessionRenewalPeriodMins { get; set; }
        protected DateTime LastTimeSessionRenewed { get; set; }
        protected DateTime LastTimeOPCServerFoundAlive { get; set; }
        protected bool ClassDisposing { get; set; }
        protected Thread RenewerTHread { get; set; } = null!;


        protected OPCUAClient(OPCUASpecDTO opcuaSpec)
        {
            ServerAddress = opcuaSpec.ServerAddres;
            ServerPortNumber = opcuaSpec.ServerPort;
            MyApplicationName = "OPCUAClient";
            TagList = opcuaSpec.TagList;
            SessionRenewalRequired = opcuaSpec.SessionRenewalRequired;
            SessionRenewalPeriodMins = opcuaSpec.SessionRenewalMinutes;
            LastTimeOPCServerFoundAlive = DateTime.Now;
        }

        /// <summary>
        /// Starting client with error returns.
        /// </summary>
        /// <returns>Error DTO</returns>
        public ErrorLogDTO StartClient()
        {
            if (ServerAddress == string.Empty)
                return new ErrorLogDTO(true, "OPCUA Server Address is empty");
            if (ServerPortNumber == string.Empty)
                return new ErrorLogDTO(true, "OPCUA Port Number is empty");
            if (SessionRenewalPeriodMins <= 0)
                return new ErrorLogDTO(true, "Session Renewal Period Minutes set 0 or below");

            if (TagList.Count == 0)
                return new ErrorLogDTO(true, "Tag List doesn't have any values");

            ErrorLogDTO initLog = new(false);

            try
            {
                initLog = InitializeOPCUAClient();

                if (SessionRenewalRequired && ClassDisposing != true)
                {
                    LastTimeSessionRenewed = DateTime.Now;
                    RenewerTHread = new Thread(RenewSessionThread);
                    RenewerTHread.Start();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new();
                sb.AppendLine("OPCUAService: OPCUAService: exception when constructing OPCUA client: ");
                sb.Append(" Exception: ");
                sb.Append(ex.ToString());
                return new ErrorLogDTO(true, sb.ToString());
            }

            return initLog;
        }

        /// <summary>
        /// Renewing Opcua session thread after disconnecting session
        /// </summary>
        private void RenewSessionThread()
        {
            while (!ClassDisposing)
            {
                if ((DateTime.Now - LastTimeSessionRenewed).TotalMinutes > SessionRenewalPeriodMins || (DateTime.Now - LastTimeOPCServerFoundAlive).TotalSeconds > 60)
                {
                    try
                    {
                        if (OPCSession != null)
                        {
                            OPCSession.Close();
                            OPCSession.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        StringBuilder sb = new();
                        sb.AppendLine("OPCUAService: renewSessionThread: OPCSession.Close() exception: ");
                        sb.Append(" Exception: ");
                        sb.Append(ex.ToString());
                        Log.Error(sb.ToString());
                    }
                    InitializeOPCUAClient();
                    LastTimeSessionRenewed = DateTime.Now;

                }
                Thread.Sleep(2000);


            }

        }
        private ErrorLogDTO InitializeOPCUAClient()
        {
            var config = new ApplicationConfiguration()
            {
                ApplicationName = MyApplicationName,
                ApplicationUri = Utils.Format(@"urn:{0}:" + MyApplicationName + "", ServerAddress),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = Utils.Format(@"CN={0}, DC={1}", MyApplicationName, ServerAddress) },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                    AutoAcceptUntrustedCertificates = true,
                    AddAppCertToTrustedStore = true
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };
            try
            {
                config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                }
            }
            catch(Exception ex)
            {
                StringBuilder sb = new();
                sb.AppendLine("OPCUAService: InitializeOPCUAClient: exception when validating OPCUA client: ");
                sb.Append(" Exception: ");
                sb.Append(ex.ToString());
                return new ErrorLogDTO(true, sb.ToString());
            }

            var application = new ApplicationInstance
            {
                ApplicationName = MyApplicationName,
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };

            try
            {
                application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new();
                sb.AppendLine("OPCUAService: InitializeOPCUAClient: exception when checking OPCUA client instance certificate: ");
                sb.Append(" Exception: ");
                sb.Append(ex.ToString());
                return new ErrorLogDTO(true, sb.ToString());
            }

            try
            {
                var selectedEndpoint = CoreClientUtils.SelectEndpoint("opc.tcp://" + ServerAddress + ":" + ServerPortNumber + "", useSecurity: SecurityEnabled, 15000);

                OPCSession = Session.Create(config, new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)), false, "", 60000, null, null).GetAwaiter().GetResult();
                {

                    var subscription = new Subscription(OPCSession.DefaultSubscription) { PublishingInterval = 1000 };

                    var list = new List<MonitoredItem> { };
                    list.Add(new MonitoredItem(subscription.DefaultItem) { DisplayName = "ServerStatusCurrentTime", StartNodeId = "i=2258" });

                    foreach (KeyValuePair<NodeId, OPCUATag> td in TagList)
                    {
                        if (td.Value != null)
                            list.Add(new MonitoredItem(subscription.DefaultItem) { DisplayName = td.Value.DisplayName, StartNodeId = td.Key });
                    }

                    list.ForEach(i => i.Notification += OnTagValueChange);
                    subscription.AddItems(list);

                    OPCSession.AddSubscription(subscription);
                    subscription.Create();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new();
                sb.AppendLine("OPCUAService: InitializeOPCUAClient: exception creating session in OPCUA client: ");
                sb.Append(" Exception: ");
                sb.Append(ex.ToString());
                return new ErrorLogDTO(true, sb.ToString());
            }

            return new ErrorLogDTO(false);
        }

        /// <summary>
        /// Main event that happenes when a notification is triggered. This mostly is used in child class depending on a system
        /// </summary>
        /// <param name="item"></param>
        /// <param name="e"></param>
        protected abstract void OnTagValueChange(MonitoredItem item, MonitoredItemNotificationEventArgs e);

        /// <summary>
        /// Disposing of the OPCUA client and all the components 
        /// </summary>
        public void Dispose()
        {
            ClassDisposing = true;
            try
            {
                if(OPCSession != null)
                {
                    OPCSession.Close();
                    OPCSession.Dispose();
                }

                if(RenewerTHread != null)
                    RenewerTHread.Interrupt();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new();
                sb.AppendLine("OPCUAService: Dispose: exception when disposing OPCUA client: ");
                sb.Append(" Exception: ");
                sb.Append(ex.ToString());
                Log.Error(sb.ToString());
            }
        }

    }
}
