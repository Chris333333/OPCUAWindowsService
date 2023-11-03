using Serilog;

namespace Application
{
    /// <summary>
    /// Main worker setting main setup. 
    /// </summary>
    public class OPCUAServiceWorker : IDisposable
    {
        private OPCUAConnectorSetup _setup;

        public void OPCUAServiceWorkerStart()
        {
            _setup = new OPCUAConnectorSetup(StaticData.OPCUANotifNames.testList);
            
            _setup.RemadeTable();
            _setup.StartCollection();
        }
        public void Dispose()
        {
            if(_setup != null)
            {
                _setup.Dispose();
            }
            
        }
    }
}