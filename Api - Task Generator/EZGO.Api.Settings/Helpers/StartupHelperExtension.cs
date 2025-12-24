using EZGO.Api.Interfaces.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings.Helpers
{
    /// <summary>
    /// StartupHelperExtension; Helper created for managing startup services (project startup).
    /// </summary>
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add settings services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection</param>
        public static void AddSettingServices(this IServiceCollection services)
        {
            services.AddScoped<IConfigurationHelper, ConfigurationHelper>();

        }
    }
}
