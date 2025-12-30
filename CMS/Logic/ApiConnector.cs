using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebApp.Logic.Interfaces;
using WebApp.Models;

namespace WebApp.Logic
{
    //TODO: Add a function to Get as a stream within the ApiConnector class.
    public class ApiConnector : IApiConnector
    {
        private readonly HttpClient _apiClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfigurationHelper _configurationHelper;
        private string _applicationName;
        private string _applicationVersion;

        public ApiConnector(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IConfigurationHelper configurationHelper)
        {
            _configurationHelper = configurationHelper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(_configurationHelper.GetValueAsString("AppSettings:ApiUri"));
            _apiClient.Timeout = new TimeSpan(0,2,0);
            _applicationName = _configurationHelper.GetValueAsString("AppSettings:ApplicationName");
            _applicationVersion = _configurationHelper.GetValueAsString("AppSettings:ApplicationVersion");
        }

        /// <summary>
        /// GetCall; Get a specific call by Url.
        /// </summary>
        /// <param name="url">Url (minus base url, seeing that in configured in settings AppSettings:ApiUri </param>
        /// <returns>Depending on status, returned a Json string with status when 200, else return what the API gives back. When its a 401 status, auto logout the user from the CMS</returns>
        public async Task<ApiResponse> GetCall(string url)
        {
            try
            {
                HttpResponseMessage result;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddHeaders(requestMessage: requestMessage);
                    result = await _apiClient.SendAsync(requestMessage);
                }

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //Signout out of webapp when client is signedout from API
                    await SignOut();
                    //TODO add logging when signing out.
                    return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, Message = await result.Content.ReadAsStringAsync() };
                }
                else
                {
                    //API returned something that not can be handled properly, Application needs to handle the error it self.
                    return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the API: ", ex.Message) };
            }


        }

        /// <summary>
        /// GetCall; Get a specific call by Url.
        /// </summary>
        /// <param name="url">Url (minus base url, seeing that in configured in settings AppSettings:ApiUri </param>
        /// <returns>Depending on status, returned a Json string with status when 200, else return what the API gives back. When its a 401 status, auto logout the user from the CMS</returns>
        public async Task<ApiResponse> GetCall(string url, string token)
        {
            try
            {
                HttpResponseMessage result;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddHeaders(requestMessage: requestMessage, token: token);
                    result = await _apiClient.SendAsync(requestMessage);
                }

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //Signout out of webapp when client is signedout from API
                    await SignOut();
                    //TODO add logging when signing out.
                    return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, Message = await result.Content.ReadAsStringAsync() };
                }
                else
                {
                    //API returned something that not can be handled properly, Application needs to handle the error it self.
                    return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the API: ", ex.Message) };
            }

        }

        /// <summary>
        /// GetCall; Get call based on a stream, can be used for getting files from API.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stream"></param>
        /// <returns>ApiResponse and fills stream with data.</returns>
        public async Task<ApiResponse> GetCall(string url, Stream stream)
        {
            try
            {
                HttpResponseMessage result;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddHeaders(requestMessage: requestMessage);
                    result = await _apiClient.SendAsync(requestMessage);
                    result.EnsureSuccessStatusCode();

                    //copy content to supplied stream
                    await using var ms = await result.Content.ReadAsStreamAsync();
                    ms.Seek(0, SeekOrigin.Begin); //set to 0 stream cuz of possible issues;
                    await ms.CopyToAsync(stream); //copy every thing to stream
                }

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = "Succes" };
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //Signout out of webapp when client is signedout from API
                    await SignOut();
                    //TODO add logging when signing out.
                    return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, Message = await result.Content.ReadAsStringAsync() };
                }
                else
                {
                    //API returned something that not can be handled properly, Application needs to handle the error it self.
                    return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the API: ", ex.Message) };
            }


        }

        /// <summary>
        /// PostCall; Make a post call the API.
        /// </summary>
        /// <param name="url">API url part</param>
        /// <param name="jsonBody">The serialized JSON object</param>
        /// <returns>Depending on status, returned a Json string with status when 200, else return what the API gives back. When its a 401 status, auto logout the user from the CMS</returns>
        public async Task<ApiResponse> PostCall(string url, string jsonBody)
        {
            try
            {
                HttpResponseMessage result;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    AddHeaders(requestMessage: requestMessage);
                    requestMessage.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                    result = await _apiClient.SendAsync(requestMessage);
                }

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //Signout out of webapp when client is signedout from API
                    await SignOut();
                    //TODO add logging when signing out.
                    return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, Message = await result.Content.ReadAsStringAsync() };
                }
                else if (result.StatusCode == HttpStatusCode.BadRequest || result.StatusCode == HttpStatusCode.Conflict || result.StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };
                }
                else if (result.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return new ApiResponse
                    {
                        StatusCode = HttpStatusCode.TooManyRequests,
                        Message = await result.Content.ReadAsStringAsync()
                    };
                }

                else
                {
                    //Throw exception, API returned something that not can be handled properly, Application needs to handle the error it self.
                    throw new Exception(string.Concat("Api error: ", result.StatusCode), new Exception(await result.Content.ReadAsStringAsync()));
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the API: ", ex.Message) };
            }
        }

        /// <summary>
        /// Sends a POST request to the specified URL with the provided token and JSON body, and returns the API
        /// response.
        /// </summary>
        /// <remarks>- If the API returns a 200 (OK) status code, the response message is returned in the
        /// <see cref="ApiResponse.Message"/> property. - If the API returns a 401 (Unauthorized) status code, the user
        /// is signed out, and the response message is returned. - If the API returns a 400 (Bad Request) or 409
        /// (Conflict) status code, the response message is returned without additional processing. - For any other
        /// status code, an exception is thrown. - If an exception occurs during the request, an <see
        /// cref="ApiResponse"/> with a 500 (Internal Server Error) status code is returned.</remarks>
        /// <param name="url">The URL to which the POST request will be sent. This cannot be null or empty.</param>
        /// <param name="token">The authentication token to include in the request headers. This cannot be null or empty.</param>
        /// <param name="jsonBody">The JSON-formatted string to include in the request body. If null or empty, the token will be serialized to
        /// JSON and used as the body instead.</param>
        /// <returns>An <see cref="ApiResponse"/> object containing the HTTP status code and the response message from the API.</returns>
        public async Task<ApiResponse> PostTokenCall(string url, string token, string jsonBody = null)
        {
            try
            {
                HttpResponseMessage result;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    AddHeaders(requestMessage: requestMessage, token: token);
                    requestMessage.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                    result = await _apiClient.SendAsync(requestMessage);
                }

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //Signout out of webapp when client is signedout from API
                    await SignOut();
                    //TODO add logging when signing out.
                    return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, Message = await result.Content.ReadAsStringAsync() };

                }
                else if (result.StatusCode == HttpStatusCode.BadRequest || result.StatusCode == HttpStatusCode.Conflict)
                {

                    return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };

                }
                else
                {
                    //Throw exception, API returned something that not can be handled properly, Application needs to handle the error it self.
                    throw new Exception(string.Concat("Api error: ", result.StatusCode), new Exception(await result.Content.ReadAsStringAsync()));
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the API: ", ex.Message) };
            }
        }


        /// <summary>
        /// PostCall; Make a post call the API.
        /// </summary>
        /// <param name="url">API url part</param>
        /// <param name="jsonBody">The serialized JSON object</param>
        /// <returns>Depending on status, returned a Json string with status when 200, else return what the API gives back. When its a 401 status, auto logout the user from the CMS</returns>
        public async Task<ApiResponse> PostCall(string url, MultipartFormDataContent objectBody)
        {
            try
            {
                HttpResponseMessage result;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    AddHeaders(requestMessage: requestMessage);
                    requestMessage.Content = objectBody;
                    result = await _apiClient.SendAsync(requestMessage);
                }

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                }
                else if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //Signout out of webapp when client is signedout from API
                    await SignOut();
                    //TODO add logging when signing out.
                    return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, Message = await result.Content.ReadAsStringAsync() };
                }
                else
                {
                    //Throw exception, API returned something that not can be handled properly, Application needs to handle the error it self.
                    throw new Exception(string.Concat("Api error: ", result.StatusCode), new Exception(await result.Content.ReadAsStringAsync()));
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the API: ", ex.Message) };
            }
        }

        /// <summary>
        /// Specific function for checking if API calls can be made with this specific user; Can be used before actually posting information to the API that consists of multiple stages.
        /// Method will check based on acceptation key (implementation later on for multitenent) and user authentication token for API.
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCode> CheckConnectorAuthorized()
        {
            var activityAuthenticationCheck = await PostCall("v1/authentication/check", "");
            return activityAuthenticationCheck.StatusCode;
        }

        /// <summary>
        /// SignOut(); Signout from application when token not valid anymore on APP
        /// </summary>
        private async Task SignOut()
        {
            //Signout out of webapp when client is signed-out from API
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private void AddHeaders(HttpRequestMessage requestMessage, string token = null)
        {
            if (token.IsNullOrEmpty())
            {
                AddAuthenticationHeaders(requestMessage);
            }
            else
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            AddClientHeaderInformation(requestMessage);
            AddGuidHeaderInformation(requestMessage);
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableLanguageHeader"))
                AddLanguageHeaderInformation(requestMessage);
        }

        /// <summary>
        /// AddAuthenticationHeaders; Add authentication headers to API calls based on the SID which should contain the API jwt token for logging in on the API.
        /// </summary>
        /// <param name="requestMessage">Message where the headers need to be added.</param>
        private void AddAuthenticationHeaders(HttpRequestMessage requestMessage)
        {
            if (_httpContextAccessor.HttpContext?.User != null && _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Sid).Any())
            {
                var token = _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Sid).FirstOrDefault().Value;
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// AddClientHeaderInformation; Add client header (user agent) to posts to API request.
        /// </summary>
        /// <param name="requestMessage">Message where the headers need to be added.</param>
        private void AddClientHeaderInformation(HttpRequestMessage requestMessage)
        {
            try
            {
                if (_httpContextAccessor.HttpContext?.Request?.Headers != null)
                {
                    if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()))
                    {
                        var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
                        var userAgentString = string.Format("{0} ({1}) {0}/{2}", _applicationName, userAgent, _applicationVersion);
                        requestMessage.Headers.Add("User-Agent", userAgentString);
                    }

                    if (_httpContextAccessor.HttpContext?.User != null)
                    {
                        var company = _httpContextAccessor.HttpContext?.User.GetProfile()?.Company;
                        if (company != null)
                        {
                            var cid = company.Id;
                            requestMessage.Headers.Add("Ez-Cid", cid.ToString());
                        }
                    }
                }
#pragma warning disable CS0168 // Do not catch general exception types
            }
            catch (Exception ex)
            {
                //ignore it
            }
#pragma warning restore CS0168 // Do not catch general exception types
        }

        /// <summary>
        /// AddGuidHeaderInformation; Add Guids to messages for extra checks on API.
        /// </summary>
        /// <param name="requestMessage">Message where the headers need to be added.</param>
        private void AddGuidHeaderInformation(HttpRequestMessage requestMessage)
        {
            try
            {
                if (_httpContextAccessor.HttpContext?.Request?.Headers != null)
                {
                    if (_httpContextAccessor.HttpContext?.User != null && _httpContextAccessor.HttpContext.User.Claims.Any())
                    {
                        if (_httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == "syncguid").Any())
                        {
                            var syncGuid = _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == "syncguid").FirstOrDefault().Value;
                            requestMessage.Headers.Add("Sync-GUID", syncGuid);
                        }

                        if (_httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == "guid").Any())
                        {
                            var userGuid = _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == "guid").FirstOrDefault().Value;
                            requestMessage.Headers.Add("User-GUID", userGuid);
                        }
                    }
                }
#pragma warning disable CS0168 // Do not catch general exception types
            }
            catch (Exception ex)
            {
                //ignore it
            }
#pragma warning restore CS0168 // Do not catch general exception types
        }

        private void AddLanguageHeaderInformation(HttpRequestMessage requestMessage)
        {
            if (_httpContextAccessor.HttpContext?.Request?.Headers != null)
            {
                var locale = GetCookie(Constants.General.LANGUAGE_COOKIE_STORAGE_KEY) ?? "en-US";
                requestMessage.Headers.Add("Language", locale);
            }
        }

        /// <summary>
        /// GetCookie; Get cookie value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetCookie(string key)
        {
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request != null && _httpContextAccessor.HttpContext.Request.Cookies != null)
            {
                return _httpContextAccessor.HttpContext.Request.Cookies[key];
            }
            else return string.Empty;
        }
    }
}
