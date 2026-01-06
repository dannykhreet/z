using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.Solvace.Helpers
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
        public static void AddSolvaceConnectorServices(this IServiceCollection services)
        {
            //TODO maybe remove Depency Injection to make it more standalone later on.
            // services.AddScoped<ISolvaceConnector, SolvaceConnector>();
        }
    }
}
