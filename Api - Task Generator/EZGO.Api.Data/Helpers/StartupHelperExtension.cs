using EZGO.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Data.Helpers
{
    /// <summary>
    /// StartupHelperExtension; Helper created for managing startup services (project startup).
    /// </summary>
    public static class StartupHelperExtension
    {
        private const string READER_WRITER_KEY = "AppSettings:UseReaderWriterConnections";
        /// <summary>
        /// Add data services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        /// <param name="configuration">Configuration to get connections strings from that are used when initiating the ConnectionHelper and EZGoContext.</param>
        public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Check if we would use one connection for reading and writing, or multiple ones.
            if (configuration.GetSection(READER_WRITER_KEY) != null
                && !string.IsNullOrEmpty(configuration.GetSection(READER_WRITER_KEY).Value)
                && Convert.ToBoolean(configuration.GetSection(READER_WRITER_KEY).Value))
            {
                services.AddSingleton<EZGO.Api.Interfaces.Data.IConnectionHelper, ConnectionHelper>(connhelper => new ConnectionHelper(connectionStringReader: Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT_READER) != null ?
                                                                                                                                       Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT_READER) :
                                                                                                                                       configuration.GetConnectionString(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_READER),
                                                                                                                                       connectionStringWriter: Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT_WRITER) != null ?
                                                                                                                                       Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT_WRITER) :
                                                                                                                                       configuration.GetConnectionString(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_WRITER)));
            }
            else
            {
                services.AddSingleton<EZGO.Api.Interfaces.Data.IConnectionHelper, ConnectionHelper>(connhelper => new ConnectionHelper(connectionString: Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT) != null ?
                                                                                                                       Environment.GetEnvironmentVariable(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME_ENVIRONMENT) :
                                                                                                                       configuration.GetConnectionString(ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME)));
            }


            services.AddScoped<EZGO.Api.Interfaces.Data.IDatabaseAccessHelper, DatabaseAccessHelper>();
            services.AddScoped<EZGO.Api.Interfaces.Data.IDatabaseLogWriter, DatabaseLogWriter>();
        }
    }
}
