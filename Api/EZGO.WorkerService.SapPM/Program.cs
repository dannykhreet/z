using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Helpers;
using EZGO.WorkerService.SapPM.Data;
using EZGO.Api.Logic.SapPmConnector.Helpers;
using EZGO.Api.Logic.Helpers;
using EZGO.Api.TaskGeneration.Helpers;
using EZGO.Api.Data.Helpers;
using EZGO.Api.Utils.Logging;
using EZGO.Api.Interfaces.Data;
using EZGO.WorkerService.SapPM.Utils;


namespace EZGO.WorkerService.SapPM
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
               services.AddUtilServices(); //UtilServices, contains DI managers that are needed for the application. If a manager that is used for DI is added within the Util library, add it to the services in this method.
               services.AddSingleton<IConfigurationHelper, ConfigurationHelper>();
               services.AddMemoryCache();
               services.AddSapPmDataServices(configuration);
               services.AddSapPMConnectionServices();
               services.AddLogicServices();
               services.AddTaskGenerationServices();
               services.AddLogicDataServices();
           }).ConfigureLogging((hostContext, logger) => {
               logger.ClearProviders();
               logger.AddConsole();
               logger.AddDebug();
               logger.AddProvider(new WorkerServiceDatabaseLoggerProvider(hostContext.Configuration));

           });

    }
}
