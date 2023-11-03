using Application;
using Serilog;
using System.Reflection;

namespace OPCUAWindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            IHost host = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "OPCUAService";
            })
            .ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                configBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange:  true)
                    .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
                    .Build();
            })
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<OPCUAServiceWorker>();
                services.AddHostedService<WindowsBackgroundService>();
            })
            .Build();

            host.Run();
        }
    }
}