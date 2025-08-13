using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Cache;
using Plugin.DeviceInfo;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    public abstract class HttpDelegatingHandlerBase : DelegatingHandler
    {
        protected readonly ICachingService cachingService;

        /// <summary>
        /// Gets or sets a value indicating whether the token should be sent with the request.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the token should be sent with the request; otherwise, <c>false</c>.
        /// </value>
        public static bool SendToken { get; set; } = true;

        protected HttpDelegatingHandlerBase() : base(new HttpClientHandler())
        {
            cachingService = DependencyService.Get<ICachingService>();
        }

        protected static bool IsMediaRequest(string absoluteUri)
        {
            return absoluteUri.Contains("/media/");
        }

        /// <summary>Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.</summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was <see langword="null" />.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (SendToken && !string.IsNullOrWhiteSpace(Settings.Token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Settings.Token);

            request.Headers.AddEzgoHeaders();
 
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
