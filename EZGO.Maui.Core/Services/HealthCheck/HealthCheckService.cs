using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.HealthCheck;
using EZGO.Maui.Core.Utils;
using System.IdentityModel.Tokens.Jwt;

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
                //Token cannot be empty
                if (string.IsNullOrWhiteSpace(jwtToken))
                    return false;

                //Try to parse the token
                JwtSecurityToken token;
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    token = handler.ReadJwtToken(jwtToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ValidateTokenAsync] Invalid token format: {ex.Message}");
                    return false;
                }

                //Check expiration date
                var expiration = token.Claims.FirstOrDefault(x => x.Type.Equals("exp"))?.Value;
                if (!string.IsNullOrEmpty(expiration) && long.TryParse(expiration, out long exp))
                {
                    var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp);
                    if (expirationDate.UtcDateTime < DateTime.UtcNow)
                    {
                        Console.WriteLine("[ValidateTokenAsync] Token has expired.");
                        return false;
                    }
                }

                //Check internet connection
                if (await InternetHelper.HasInternetAndApiConnectionIgnoreTokenAsync())
                {
                    //Validate token against the API
                    try
                    {
                        var result = await httpClient.GetAsync("health/userconnection");
                        return result.IsSuccessStatusCode;
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"[ValidateTokenAsync] HTTP request failed: {ex.Message}");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ValidateTokenAsync] Network error: {ex.Message}");
                        return false;
                    }
                }

                // We don't have the Internet but the token is not yet expired.
                // At this point it's impossible to determine it's the token is valid for the API
                // We can assume it is and later when the connection is restored it will be automatically validated against 
                // the API.
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ValidateTokenAsync] Unexpected error: {ex}");
                return false;
            }
        }
    }
}
