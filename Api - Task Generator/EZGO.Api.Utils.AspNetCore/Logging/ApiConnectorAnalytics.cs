using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models.Tools;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Logging
{
    public class ApiConnectorAnalytics : IApiConnectorAnalytics
    {
        private readonly HttpClient _apiClient;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _applicationName;
        private string _applicationVersion;

        public ApiConnectorAnalytics(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper)
        {
            _configurationHelper = configurationHelper;
            _httpContextAccessor = httpContextAccessor;
            _apiClient = httpClient;

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
                if(!string.IsNullOrEmpty(_configurationHelper.GetValueAsString("AppSettings:AnalyticsESBaseUri")))
                {
                    _apiClient.BaseAddress = new Uri(_configurationHelper.GetValueAsString("AppSettings:AnalyticsESBaseUri"));
                    _applicationName = _configurationHelper.GetValueAsString("AppSettings:ApplicationName");
                    _applicationVersion = _configurationHelper.GetValueAsString("AppSettings:ApplicationVersion");

                    HttpResponseMessage result;
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        AddAuthenticationHeaders(requestMessage: requestMessage);
                        //AddClientHeaderInformation(requestMessage: requestMessage);
                        requestMessage.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                        result = await _apiClient.SendAsync(requestMessage);
                    }

                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                    }
                    else if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        //TODO add logging when signing out.
                        return new ApiResponse { StatusCode = HttpStatusCode.Unauthorized, Message = await result.Content.ReadAsStringAsync() };
                    }
                    else if (result.StatusCode == HttpStatusCode.BadRequest || result.StatusCode == HttpStatusCode.Conflict || result.StatusCode == HttpStatusCode.MethodNotAllowed)
                    {
                        return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };
                    }
                    else
                    {
                        //Throw exception, API returned something that not can be handled properly, Application needs to handle the error it self.
                        throw new Exception(string.Concat("Api error: ", result.StatusCode), new Exception(await result.Content.ReadAsStringAsync()));
                    }
                } else
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.ServiceUnavailable, Message = "" };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the API: ", ex.Message) };
            }
        }


        /// <summary>
        /// AddAuthenticationHeaders; Add authentication headers to API calls based on the SID which should contain the API jwt token for logging in on the API.
        /// </summary>
        /// <param name="requestMessage">Message where the headers need to be added.</param>
        private void AddAuthenticationHeaders(HttpRequestMessage requestMessage)
        {
            if (_httpContextAccessor.HttpContext?.User != null && _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Sid).Any())
            {
                var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString().Replace("Bearer ", "");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
