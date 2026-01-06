using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Raw;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Logic.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Templates.Helpers
{
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add logic services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection.</param>
        public static void AddLogicTemplatesServices(this IServiceCollection services)
        {
            services.AddScoped<ISharedTemplateManager, SharedTemplateManager>();
        }
    }
}
