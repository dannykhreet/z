using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Claims;
using Elastic.Apm.Api.Kubernetes;
using System.Security.Cryptography;
using System.Text;

namespace WebApp.Logic
{
    public class ApiDatawarehouseConnector : IApiDatawarehouseConnector
    {

            private readonly HttpClient _apiClient;
            private readonly IConfiguration _configuration;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IConfigurationHelper _configurationHelper;
            private string _applicationName;
            private string _applicationVersion;

            public ApiDatawarehouseConnector(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IConfigurationHelper configurationHelper)
            {
                _configurationHelper = configurationHelper;
                _configuration = configuration;
                _httpContextAccessor = httpContextAccessor;
                _apiClient = httpClient;
                _apiClient.BaseAddress = new Uri(_configurationHelper.GetValueAsString("AppSettings:ApiDwUri"));
                _applicationName = _configurationHelper.GetValueAsString("AppSettings:ApplicationName");
                _applicationVersion = _configurationHelper.GetValueAsString("AppSettings:ApplicationVersion");
            }

            public async Task<ApiResponse> GetCall(string url, string userName, string password, string appid)
            {
                try
                {
                    HttpResponseMessage result;
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        AddAuthenticationHeaders(requestMessage: requestMessage, userName: userName, password: password, appId: appid);
                        AddClientHeaderInformation(requestMessage: requestMessage);
                        result = await _apiClient.SendAsync(requestMessage);
                    }

                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                    }
                    else
                    {
                        //API returned something that not can be handled properly, Application needs to handle the error it self.
                        return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };
                    }
                }
                catch (Exception ex)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the DW API: ", ex.Message) };
                }


            }

            public async Task<ApiResponse> PostCall(string url, string jsonBody, string userName, string password, string appid)
            {
                try
                {
                    HttpResponseMessage result;
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        AddAuthenticationHeaders(requestMessage: requestMessage, userName: userName, password: password, appId:appid);
                        AddClientHeaderInformation(requestMessage: requestMessage);
                        requestMessage.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                        result = await _apiClient.SendAsync(requestMessage);
                    }

                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        return new ApiResponse { StatusCode = HttpStatusCode.OK, Message = await result.Content.ReadAsStringAsync() }; //return content string, parse on implementing caller.
                    }
                    else
                    {
                        //API returned something that not can be handled properly, Application needs to handle the error it self.
                        return new ApiResponse { StatusCode = result.StatusCode, Message = await result.Content.ReadAsStringAsync() };
                    }
                }
                catch (Exception ex)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Message = string.Concat("An error occurred while connecting or getting data from the DW API: ", ex.Message) };
                }

            }

            /// <summary>
            /// AddAuthenticationHeaders; Add authentication headers to API calls based on the SID which should contain the API jwt token for logging in on the API.
            /// </summary>
            /// <param name="requestMessage">Message where the headers need to be added.</param>
            private void AddAuthenticationHeaders(HttpRequestMessage requestMessage, string userName, string password, string appId)
            {
                if (_httpContextAccessor.HttpContext?.User != null && _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Sid).Any())
                {
                    //add headers if available. For internal tests not needed.
                    var authHeaderValue = $"{userName}:{password}";
                    var authHeaderByteArray = Encoding.ASCII.GetBytes(authHeaderValue);
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authHeaderByteArray));
                    requestMessage.Headers.Add("AppId", appId);
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

        }

}
