using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Exporting.Helpers;
using EZGO.Api.Settings.Helpers;
using EZGO.WorkerService.Exporter.Data;
using EZGO.WorkerService.Exporter.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.WorkerService.Exporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args).ConfigureHostConfiguration(configHost =>
           {
               configHost.AddEnvironmentVariables();
           }).ConfigureServices((hostContext, services) =>
           {
               IConfiguration configuration = hostContext.Configuration;
               services.AddHostedService<Worker>();
               services.AddExportDataServices(configuration); //Data Services, contains DI managers that are needed for the application. If a manager that is used for DI is added within the data library, add it to the services in this method.
               services.AddScoped<IConfigurationHelper, ConfigurationHelper>();
               services.AddExportingServices();
           }).ConfigureLogging((hostContext, logger) => {
                logger.ClearProviders();
                logger.AddConsole();
                logger.AddDebug();
                logger.AddProvider(new WorkerServiceDatabaseLoggerProvider(hostContext.Configuration));
            });

    }
}
