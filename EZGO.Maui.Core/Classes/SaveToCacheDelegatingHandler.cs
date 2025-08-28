using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Delegating handler which saves requests to the cache.
    /// </summary>
    public class SaveToCacheDelegatingHandler : HttpDelegatingHandlerBase
    {
        private readonly TimeSpan cacheTimeSpan = TimeSpan.FromDays(1);

        /// <summary>
        /// Gets or sets a value indicating whether to save requests to the cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if requests should be saved to the cache; otherwise, <c>false</c>.
        /// </value>
        public static bool SaveToCache { get; set; } = true;

        /// <summary>Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.</summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was <see langword="null" />.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage result = await base.SendAsync(request, cancellationToken);

            if (SaveToCache && cachingService != null && request.Method == HttpMethod.Get && result.IsSuccessStatusCode)
            {
                string requestUri = request.RequestUri.AbsoluteUri;

                bool isMediaRequest = IsMediaRequest(requestUri);
                try
                {
                    if (isMediaRequest)
                    {
                        byte[] responseBytes = await result.Content.ReadAsByteArrayAsync();

                        cachingService.CacheRequest(requestUri, responseBytes, cacheTimeSpan);
                    }
                    else
                    {
                        string responseContent = await result.Content.ReadAsStringAsync();

                        cachingService.CacheRequest(requestUri, responseContent, cacheTimeSpan);
                    }
                }
                catch (Exception ex){
                    Debug.WriteLine(ex.Message);
                    //Crashes.TrackError(ex, new Dictionary<string, string>() { { "SaveToCache", requestUri } });
                }
            }

            return result;
        }
    }
}
