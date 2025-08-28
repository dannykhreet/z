using EZGO.Maui.Core.Utils;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Delegating handler which retrieves requests from the cache.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.Classes.HttpDelegatingHandlerBase" />
    public class RetrieveFromCacheDelegatingHandler : HttpDelegatingHandlerBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether get requests should be retrieved from the cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if get requests should be retrieved from the cache; otherwise, <c>false</c>.
        /// </value>
        public static bool RetrieveFromCache { get; set; } = true;

        /// <summary>Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.</summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was <see langword="null" />.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage result;

            string requestUri = request.RequestUri.AbsoluteUri;

            bool isMediaRequest = IsMediaRequest(requestUri);

            if (RetrieveFromCache && cachingService != null && request.Method == HttpMethod.Get)
            {
                if (isMediaRequest)
                {
                    byte[] resultBytes = cachingService.GetResponseAsBytes(requestUri);

                    if (resultBytes != null)
                        result = new HttpResponseMessage { Content = new ByteArrayContent(resultBytes) };
                    else
                        result = new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(Constants.NotFoundInCacheError) };
                }
                else
                {
                    string resultContent = cachingService.GetResponseAsString(requestUri);

                    if (!string.IsNullOrWhiteSpace(resultContent))
                        result = new HttpResponseMessage { Content = new StringContent(resultContent) };
                    else
                        result = new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(Constants.NotFoundInCacheError) };
                }
            }
            else
                result = await base.SendAsync(request, cancellationToken);

            return result;
        }
    }
}
