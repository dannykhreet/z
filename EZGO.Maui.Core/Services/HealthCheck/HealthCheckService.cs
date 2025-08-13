using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.HealthCheck;
using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Services.HealthCheck
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly HttpClient httpClient;

        public HealthCheckService()
        {
            httpClient = Statics.AppHttpClient;
            httpClient.Timeout = TimeSpan.FromSeconds(3);
            httpClient.BaseAddress = new Uri(Constants.ApiBaseUrl);
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        public async Task<bool> ValidateTokenAsync(string jwtToken)
        {
            try
            {
                // If there is not token then it's not valid
                if (string.IsNullOrWhiteSpace(jwtToken))
                    return false;

                // Get the expiration date from it first
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);

                string expiration = token.Claims.FirstOrDefault(x => x.Type.Equals("exp"))?.Value;

                if (!string.IsNullOrEmpty(expiration))
                {
                    if (long.TryParse(expiration, out long exp))
                    {
                        var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp);
                        var expired = expirationDate.UtcDateTime < DateTime.UtcNow;

                        // If the token is expired there's not point checking it against the API 
                        if (expired)
                            return false;
                    }
                }

                // If we have Internet
                if (await InternetHelper.HasInternetAndApiConnectionIgnoreTokenAsync())
                {

                    // Validate the token against the API
                    var result = await httpClient.GetAsync("health/userconnection");

                    return result.IsSuccessStatusCode;
                }

                // We don't have the Internet but the token is not yet expired.
                // At this point it's impossible to determine it's the token is valid for the API
                // We can assume it is and later when the connection is restored it will be automatically validated against 
                // the API.
                return true;
            }
            catch (Exception)
            {
                Debugger.Break();
                return false;
            }
        }

    }
}
