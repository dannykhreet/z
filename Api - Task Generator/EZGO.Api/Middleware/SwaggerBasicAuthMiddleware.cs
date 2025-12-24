using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using EZGO.Api.Settings;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Elastic.Apm.Api;
using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace EZGO.Api.Middleware
{
    /// <summary>
    /// SwaggerBasicAuthMiddleware; Swagger authentication layer based on basic auth, ip restrictions and environment restrictions.
    /// </summary>
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private IConfiguration _configuration;

        public SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this._configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //Check if we are loading ezfdocs.
            if (context.Request.Path.StartsWithSegments("/ezfdocs"))
            {
                if (_configuration.GetSection("AppSettings:EnvironmentConfig").Value == "development" || 
                    _configuration.GetSection("AppSettings:EnvironmentConfig").Value == "localdevelopment" || 
                    (_configuration.GetSection("AppSettings:EnvironmentConfig").Value == "test") && this.GetIsValidSourceForConnection(context: context))
                {
                    string authHeader = context.Request.Headers["Authorization"];
                    if (authHeader != null && authHeader.StartsWith("Basic "))
                    {
                        // Get auth key
                        var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                        // Decode
                        var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                        // Split for user and password
                        var username = decodedUsernamePassword.Split(':', 2)[0];
                        var password = decodedUsernamePassword.Split(':', 2)[1];

                        // Check if login is correct
                        if (IsAuthorized(username, password))
                        {
                            await next.Invoke(context);
                            return;
                        }
                    }

                    // Return authentication type (causes browser to show login dialog)
                    context.Response.Headers["WWW-Authenticate"] = "Basic";
                }

                // Return unauthorized
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await next.Invoke(context);
            }
        }

        /// <summary>
        /// IsAuthorized; Is authorized.
        /// </summary>
        /// <param name="username">incoming user name</param>
        /// <param name="password">incoming password</param>
        /// <returns>true/false depending on outcome</returns>
        public bool IsAuthorized(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return false;

            // check that username and password are correct
            return username.Equals(_configuration.GetSection("AppSettings:EZDocsUser").Value, StringComparison.InvariantCultureIgnoreCase)
                    && password.Equals(_configuration.GetSection("AppSettings:EZDocsKey").Value);
        }

        /// <summary>
        /// Enable / Add to logic above when fully implemented
        /// </summary>
        /// <param name="context">Context of current request</param>
        /// <returns>true/false depending on outcome</returns>
        private bool GetIsValidSourceForConnection(HttpContext context)
        {
            const string IP_KEY = "X-Forwarded-For";

            //check multiple header parameters (due to http 1.0,1.1,2.0,2.1 handling header names differently)
            if (context.Request.Headers.Keys.Contains(IP_KEY) || context.Request.Headers.Keys.Contains(IP_KEY.ToLower()) || context.Request.Headers.Keys.Contains(IP_KEY.ToUpper()))
            {
                Microsoft.Extensions.Primitives.StringValues ipHeader = new Microsoft.Extensions.Primitives.StringValues();
                if (context.Request.Headers.Keys.Contains(IP_KEY))
                {
                    ipHeader = context.Request.Headers[IP_KEY];
                }
                else if (context.Request.Headers.Keys.Contains(IP_KEY.ToLower()))
                {
                    ipHeader = context.Request.Headers[IP_KEY.ToLower()];
                }
                else if (context.Request.Headers.Keys.Contains(IP_KEY.ToUpper()))
                {
                    ipHeader = context.Request.Headers[IP_KEY.ToUpper()];
                }

                if (!ipHeader.Any()) return false;

                var configuredValidIps = _configuration.GetSection("AppSettings:ValidIpForValidation").Value;

                if (!string.IsNullOrEmpty(configuredValidIps))
                {
                    bool ipValid = false;
                    foreach (var ip in configuredValidIps.Split(',').ToList())
                    {
                        //if ip header has valid ip then set ipValid to true,
                        if (ipHeader.Contains(ip)) ipValid = true;
                    }

                    if (!ipValid)
                    {
                        return false;
                    }

                    return ipValid;
                }
            }
            return false;
        }
    }
    public static class SwaggerAuthorizeExtensions
    {
        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerBasicAuthMiddleware>();
        }
    }

    public class AddRequiredHeaderParameter : IOperationFilter
    {

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {

            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "User-Agent",
                In = ParameterLocation.Header,
                Description = "User-Agent Field, must contain one or more valid tags for accessing the application [EZ-GO APP][EZ-GO WEBAPP][MY EZ-GO][EZ-GO DASHBOARD]. Connected user must have access to one of these apps.",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Description = "String value, must contain one or more valid tags for accessing the application [EZ-GO APP][EZ-GO WEBAPP][MY EZ-GO][EZ-GO DASHBOARD]. Connected user must have access to one of these apps."
                }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "User-Agent-EZ",
                In = ParameterLocation.Header,
                Description = "User-Agent-EZ, must contain one or more valid tags for accessing the application [EZ-GO APP][EZ-GO WEBAPP][MY EZ-GO][EZ-GO DASHBOARD].  Connected user must have access to one of these apps. Can be used for web applications that can not modify normal user agent information.",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Description = "String value, must contain one or more valid tags for accessing the application [EZ-GO APP][EZ-GO WEBAPP][MY EZ-GO][EZ-GO DASHBOARD].  Connected user must have access to one of these apps. Can be used for web applications that can not modify normal user agent information."
                }
            });
        }
    }
}
