using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Http delegating handler.
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    public class HttpDelegatingHandler : HttpDelegatingHandlerBase
    {
        /// <summary>Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.</summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was <see langword="null" />.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage result = await base.SendAsync(request, cancellationToken);

            return result;
        }
    }
}
