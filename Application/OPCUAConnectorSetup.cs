using Application.Clients;
using Application.Secrets;
using Data.DTOs;
using Data;
using Opc.Ua;
using Serilog;
using Timer = System.Timers.Timer;

namespace Application
{
    /// <summary>
    /// OPCUA connector setup 
    /// </summary>
    public class OPCUAConnectorSetup: IDisposable
    {
        private OPCUAConnector _connector;
        private readonly Dictionary<NodeId, OPCUATag> TestDictionary = new();
        private Timer _timer;

        /// <summary>
        /// Constructor that setsup new Connectore and a TagList in Dictionary
        /// </summary>
        /// <param name="list"></param>
        public OPCUAConnectorSetup( List<OPCUATag> list)
        {

            foreach (var OPCUATag in list)
            {
                TestDictionary.Add(OPCUATag.NodeID, OPCUATag);
            }

            _connector = new OPCUAConnector(new DTOs.OPCUASpecDTO(
                    ServerAddres: OPCUAConnectionSecrets.ServerAddress,
                    ServerPort: OPCUAConnectionSecrets.ServerPort,
                    TagList: TestDictionary,
                    SessionRenewalRequired: true,
                    SessionRenewalMinutes: 30
                ));

            
        }

        /// <summary>
        /// Timer event to update the whole table in database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            using (MySqlDataSender mysqlSender = new())
            {
                mysqlSender.MySQLUpdateWholeTableInDatabase(_connector.TagList);
            }
            
        }

        /// <summary>
        /// Seting up colection via starting a OPCUA client and setting up the timer to make a update cycle to the database
        /// </summary>
        public void StartCollection()
        {
            _connector.StartClient();

            // Create a timer with a two second interval.
            _timer = new Timer(2000);
            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        /// <summary>
        /// Function to remade the whole table with default values
        /// </summary>
        public void RemadeTable()
        {
            using (MySqlDataSender sender = new())
            {
                sender.MySQLRemadeTableInDatabase(_connector.TagList);
            }
        }

        /// <summary>
        /// Dispose fuction of a OPCUA connector setup
        /// </summary>
        public void Dispose()
        {
            if (_connector != null)
            {
                _connector.Dispose();
            }
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }
    }
}
