using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Models.Requests;
using EZGO.Maui.Core.Services.Api;
using EZGO.Maui.Core.Utils;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.ApiRequestHandler
{
    public class ApiRequestHandler : IApiRequestHandler, IDisposable
    {
        private IApiClient apiClient;
        private IApiClient onlineApiClient;

        private static readonly Dictionary<string, int> Calls = new();

        public ApiRequestHandler() : this(Statics.RetrieveFromCacheHttpClient) { }

        public ApiRequestHandler(HttpClient httpClient)
        {
            apiClient = new ApiClient(httpClient, false);
            onlineApiClient = new ApiClient(Statics.SaveToCacheHttpClient, true);
        }

        public async Task<T> HandleRequest<T>(string uri, bool refresh = false, bool isFromSyncService = false)
        {
            CheckForSynchronization(uri, ref isFromSyncService);

            var result = await AsyncAwaiter.AwaitResultAsync($"{nameof(HandleRequest)}_{uri}", async () =>
            {
                var localResult = await apiClient.GetAsync<T>(uri).ConfigureAwait(false);

                if ((localResult == null || refresh || isFromSyncService) && await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    var apiResult = await onlineApiClient.GetAsync<T>(uri).ConfigureAwait(false);
                    localResult = apiResult ?? localResult;
                }

                return localResult ?? (T)Activator.CreateInstance(typeof(T));
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<List<T>> HandleListRequest<T>(string uri, bool refresh = false, bool isFromSyncService = false)
        {
            CheckForSynchronization(uri, ref isFromSyncService);

            var result = await AsyncAwaiter.AwaitResultAsync($"{nameof(HandleListRequest)}_{uri}", async () =>
            {
                var localResult = await apiClient.GetAllAsync<T>(uri).ConfigureAwait(false);

                if ((localResult == null || refresh || isFromSyncService) && await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    var apiResult = await onlineApiClient.GetAllAsync<T>(uri).ConfigureAwait(false);
                    localResult = apiResult ?? localResult;
                }

                return localResult ?? new List<T>();
            }).ConfigureAwait(false);

            return result;
        }

        private void CheckForSynchronization(string uri, ref bool isFromSyncService)
        {
            if (Statics.SynchronizationRunning)
            {
                if (Calls.ContainsKey(uri))
                {
                    var value = Calls[uri];
                    if (value >= 1)
                        isFromSyncService = false;

                    Calls[uri] = ++value;
                }
                else
                {
                    Calls.TryAdd(uri, 1);
                }
            }
            else if (Calls.Count > 0)
            {
                Calls.Clear();
            }
        }

        public async Task<HttpResponseMessage?> HandlePostRequest<T>(string uri, T objectToPost, bool ignoreNullValues = false, bool saveRequest = true)
        {
            var convertedObj = Convert(objectToPost, ignoreNullValues);

            try
            {
                if (await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    return await onlineApiClient.PostAsync(uri, convertedObj).ConfigureAwait(false);
                }

                if (saveRequest)
                {
                    var content = await convertedObj.ReadAsStringAsync().ConfigureAwait(false);
                    var postRequest = new RequestModel
                    {
                        ContentAsString = content,
                        Uri = uri
                    };

                    await RequestHelper.Instance().AddRequest(uri, postRequest).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiRequestHandler] Post failed for {uri}: {ex.Message}");
            }

            return null;
        }

        private StringContent Convert<T>(T objectToConvert, bool ignoreNullValues = false)
        {
            var strData = JsonSerializer.Serialize(objectToConvert, ignoreNullValues: ignoreNullValues);
            Debug.WriteLine($"[ApiRequestHandler] JSON Payload:\n{strData}");
            return new StringContent(strData);
        }

        public void Dispose()
        {
            apiClient?.Dispose();
            onlineApiClient?.Dispose();
            apiClient = null;
            onlineApiClient = null;
        }
    }
}
