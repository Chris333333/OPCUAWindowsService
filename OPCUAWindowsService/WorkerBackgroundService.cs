
using Application;
using Serilog;

namespace OPCUAWindowsService
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        private readonly OPCUAServiceWorker _opcuaServiceWorker;
        private readonly ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService(OPCUAServiceWorker opcuaServiceWorker, ILogger<WindowsBackgroundService> logger) => (_opcuaServiceWorker, _logger) = (opcuaServiceWorker, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _opcuaServiceWorker.OPCUAServiceWorkerStart();
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                Environment.Exit(1);
            }
        }
    }
}