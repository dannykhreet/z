using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Utils.Crypto;
using EZGO.Api.Utils.Data;
using EZGO.Api.Utils.Security;
using EZGO.Api.Utils.Logging;

namespace EZGO.Api.Utils.Helpers
{
    /// <summary>
    /// StartupHelperExtension; Helper created for managing startup services (project startup).
    /// </summary>
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add utils services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection</param>
        public static void AddUtilServices(this IServiceCollection services)
        {
            services.AddScoped<ICryptography, Cryptography>(); //TODO move interface to utils.
            services.AddScoped<IDataAuditing, DataAuditing>(); //TODO move interface to utils.
         

        }
    }
}
