using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Utils.Logging;
using EZGO.Api.Utils.Media;
using EZGO.Api.Utils.Security;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.AspNetCore.Helpers
{
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add utils services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection</param>
        public static void AddUtilAspNetCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IMediaUploader, MediaUploader>(); //TODO move interface to utils.
            services.AddHttpClient<IApiConnectorAnalytics, ApiConnectorAnalytics>();
            services.AddScoped<IAwsSecurityTokenStore, AwsSecurityTokenStore>(); //TODO move interface to utils.
        }
    }
}
