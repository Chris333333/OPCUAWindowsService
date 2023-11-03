using Application.Clients;
using Application.DTOs;
using Opc.Ua.Client;

namespace Application.Tests.ObsoleteClasses
{
    /// <summary>
    /// Class for tests
    /// </summary>
    public class OPCUAVoidConnector : OPCUAClient
    {
        public OPCUAVoidConnector(OPCUASpecDTO opcuaSpec) : base(opcuaSpec)
        {
        }

        protected override void OnTagValueChange(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            
        }
    }
}
