using EZGO.Api.Security;
using EZGO.Api.Security.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Security.Helpers
{
    /// <summary>
    /// StartupHelperExtension; Helper created for managing startup services (project startup).
    /// </summary>
    public static class StartupHelperExtension
    {
        /// <summary>
        /// Add security services to service collection.
        /// This extension can be executed from startup and will initiate all DI managers that are going to be used.
        /// </summary>
        /// <param name="services">The application services collection</param>
        public static void AddSecurityServices(this IServiceCollection services)
        {
            services.AddScoped<IApplicationUser, ApplicationUser>(); //application user always add scoped, needs to be initiated on every request for security reason. Do NOT add as singleton or other persistent type of service.
            services.AddScoped<IObjectRights, ObjectRights>(); //objectrights checker.
            services.AddDataProtection(); //data protection api implementation for encrypting and decrypting values.
        }
    }
}
