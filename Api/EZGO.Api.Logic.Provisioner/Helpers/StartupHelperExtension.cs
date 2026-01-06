using EZGO.Api.Interfaces.Provisioner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Provisioner.Helpers
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
        public static void AddProvisionerServices(this IServiceCollection services)
        {
            services.AddScoped<IProvisionerManager, ProvisionerManager>();
        }
    }
}
