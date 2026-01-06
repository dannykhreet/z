using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Logic.Processor.Helpers;
using EZGO.Api.Data.Helpers;
using EZGO.WorkerService.Processor;
using EZGO.WorkerService.Processor.Data;
using EZGO.WorkerService.Processor.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EZGO.Api.Logic.Helpers;
using EZGO.Api.Utils.Helpers;
using EZGO.Api.TaskGeneration.Helpers;
//using EZGO.Api.Utils.Helpers;

namespace EZGO.WorkerService.Processor
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
               services.AddMemoryCache();
               services.AddProcessorDataServices(configuration);
               services.AddScoped<IConfigurationHelper, ConfigurationHelper>();
               services.AddLogicDataServices(); 
               services.AddProcessorServices(); //Data Services, contains DI managers that are needed for the application. If a manager that is used for DI is added within the data library, add it to the services in this method.
               services.AddUtilServices(); //UtilServices, contains DI managers that are needed for the application. If a manager that is used for DI is added within the Util library, add it to the services in this method.
               services.AddTaskGenerationServices();
               services.AddLogicServices(); //Logic Services, contains DI managers that are needed for the application. If a manager that is used for DI is added within the logic library, add it to the services in this method.

           }).ConfigureLogging((hostContext, logger) => {
                logger.ClearProviders();
                logger.AddConsole();
                logger.AddDebug();
                logger.AddProvider(new WorkerServiceDatabaseLoggerProvider(hostContext.Configuration));
            });

    }
}
