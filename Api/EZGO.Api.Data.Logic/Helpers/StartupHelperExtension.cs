using EZGO.Api.Data;
using EZGO.Api.Data.Reporting;
using EZGO.Api.Data.Users;
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
        /// <summary>
        /// Add data services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        public static void AddLogicDataServices(this IServiceCollection services)
        {
            services.AddScoped<EZGO.Api.Interfaces.Data.IExportingDataManager, ExportingDataManager>();
            services.AddScoped<EZGO.Api.Interfaces.Data.IUserDataManager, UserDataManager>();
        }
    }
}
