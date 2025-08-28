using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Utils;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace EZGO.Maui.Core.Services.Api
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient httpClient;
        private readonly IMessageService messageService;
        private readonly bool _onlineRequest = false;

        public ApiClient(HttpClient httpClient, bool onlineRequest)
        {
            this.httpClient = httpClient;
            _onlineRequest = onlineRequest;

            this.httpClient.Timeout = TimeSpan.FromSeconds(30);
            this.httpClient.BaseAddress = new Uri(Statics.ApiUrl);

            using (var scope = App.Container.CreateScope())
            {
                messageService = scope.ServiceProvider.GetService<IMessageService>();
            }
        }

        /// <summary>
        /// Prepare HttpClient with BaseAddress and settings
        /// </summary>
        public ApiClient()
        {
            _onlineRequest = true;
            this.httpClient = Statics.AppHttpClient;

            this.httpClient.Timeout = TimeSpan.FromSeconds(30);
            this.httpClient.BaseAddress = new Uri(Statics.ApiUrl);

            using (var scope = App.Container.CreateScope())
            {
                messageService = scope.ServiceProvider.GetService<IMessageService>();
            }
        }

        /// <summary>
        /// Async get returns List of objects (T)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<List<T>> GetAllAsync<T>(string action)
        {
            List<T> result = null;

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(action);
                await PrintCall(action, "GET", response);
                if (response.IsSuccessStatusCode)
                {
                    string strJson = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(strJson))
                        result = JsonSerializer.Deserialize<List<T>>(strJson, dateTimeZoneHandling: Newtonsoft.Json.DateTimeZoneHandling.Utc);
                }
                else
                {
                    await HandleErrorInMessagingCenter(response, action);
                }

            }
            catch (Exception ex)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Async returns a single object, like UserInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string action)
        {
            try
            {
                string token = Settings.Token;
                if (!string.IsNullOrWhiteSpace(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response = await httpClient.GetAsync(action);
                await PrintCall(action, "GET", response);
                if (response.IsSuccessStatusCode)
                {
                    string strJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(strJson))
                    {
                        var result = JsonSerializer.Deserialize<T>(strJson, dateTimeZoneHandling: Newtonsoft.Json.DateTimeZoneHandling.Utc);
                        return result;
                    }
                }
                else
                {
                    await HandleErrorInMessagingCenter(response, action);
                }

            }
            catch (Exception ex)
            {
            }
            return default(T);
        }

        public async Task<HttpResponseMessage> PostAsync(string action, StringContent content)
        {
            HttpResponseMessage result = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound };
            try
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await httpClient.PostAsync(action, content);
#if DEBUG
                await PrintCall(action, "POST", response, content);
#endif
                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorInMessagingCenter(response, action);
                }
                return response;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string action, T data)
        {
            string strData = JsonSerializer.Serialize(data);

            var content = new StringContent(strData);

            return await PostAsync(action, content);
        }

        private async Task PrintCall(string url, string type, HttpResponseMessage response, StringContent content = null)
        {
            if (_onlineRequest)
            {
                var message = $"\n\tDate: {DateTimeHelper.Now}:\n\tCall to url: {url}\n\tType: {type},\n\tStatuscode: {response.StatusCode.ToString()}";
                if (content != null)
                    message += $"\n\tContent: " + await content?.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string reason = await response.Content.ReadAsStringAsync();
                    if (!reason.IsNullOrEmpty())
                        message += $"\n\tReason: {reason}";
                }
                Debug.WriteLine(message, "[API Calls]");
            }
        }

        private async Task HandleErrorInMessagingCenter(HttpResponseMessage response, string uri = null)
        {
            var message = await response.Content.ReadAsStringAsync();
            message ??= string.Empty;
            // Server responses
            /// <summary>
            /// MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN; Message when token expired, is not available and/or possible login on other device.
            /// </summary>
            string MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN = "Unauthorized. User logged in on other device or user session has expired.";
            /// <summary>
            /// MESSAGE_UNAUTHORIZED_ACCESS; Message when token expired or is not available .
            /// </summary>
            string MESSAGE_UNAUTHORIZED_ACCESS = "Unauthorized. User session is not valid or expired.";
            /// <summary>
            /// MESSAGE_FORBIDDEN_OBJECT; Message when user tries to access a object that is not part of the users company or has rights to
            /// </summary>
            string MESSAGE_FORBIDDEN_OBJECT = "Forbidden. User has no rights to access object.";

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    // when setting the Settings.WorkAreaId to 0, syncing is not performed
                    Settings.WorkAreaId = 0;
                    if (message.Contains(MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN))
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.SignedOff); });
                    }
                    else if (message.Contains(MESSAGE_UNAUTHORIZED_ACCESS))
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.TokenExpired); });
                    }
                    else
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.LogOff); });
                    }
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    if (message.Contains(MESSAGE_FORBIDDEN_OBJECT))
                    {
                        messageService.SendMessage(message, Colors.Red, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.General);
                    }
                    break;
                case System.Net.HttpStatusCode.NotFound:
                    if (message == Constants.NotFoundInCacheError)
                    {
                        // If this happens, no panic: we may just have requested this a first time.
                        string WHERE_DID_WE_WANT_TO_GO = uri;
                    }
                    else
                    {
                        messageService.SendMessage(response.ReasonPhrase, Colors.Red, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.General);
                        // Check if we have internet & health
                        if (!await InternetHelper.HasInternetConnection())
                        {
                            messageService.SendMessage("No internet", Colors.Red, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.Connection);
                        }
                    }
                    break;
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    if (!await InternetHelper.HasInternetConnection())
                    {
                        messageService.SendMessage("No internet", Colors.Red, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.Connection);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}