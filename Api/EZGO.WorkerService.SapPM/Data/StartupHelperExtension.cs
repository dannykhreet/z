
namespace EZGO.WorkerService.SapPM.Data
{
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add data services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">Configuration to get connections strings from that are used when initiating the ConnectionHelper and EZGoContext.</param>
        public static void AddSapPmDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<Api.Interfaces.Data.IConnectionHelper, ConnectionHelper>(connhelper => new ConnectionHelper(Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT) != null ?
                                                                                                                                   Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT) :
                                                                                                                                   configuration.GetConnectionString(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME)));
            services.AddScoped<Api.Interfaces.Data.IDatabaseAccessHelper, DatabaseAccessHelper>();
            services.AddScoped<Api.Interfaces.Data.IDatabaseLogWriter, DatabaseLogWriter>();
        }
    }
}
