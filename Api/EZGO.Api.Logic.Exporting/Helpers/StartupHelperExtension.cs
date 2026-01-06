using EZGO.Api.Interfaces.Reporting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Logic.Exporting.Helpers
{
    /// <summary>
    /// StartupHelperExtension; Helper created for managing startup services (project startup).
    /// </summary>
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add logic services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        public static void AddExportingServices(this IServiceCollection services)
        {
            services.AddScoped<IAutomatedExportingManager, AutomatedExportingManager>();
            services.AddScoped<IExportingManager, ExportingManager>();
        }
    }
}
