using Amazon.S3;
using Amazon.S3.Model;
using Autofac;
using Autofac.Core;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.Utils.AWS;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Delegating handler which gets media items from aws s3.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.Classes.HttpDelegatingHandlerBase" />
    public class AWSS3MediaDelegatingHandler : HttpDelegatingHandlerBase
    {
        protected readonly ISettingsService settingsService;

        public AWSS3MediaDelegatingHandler()
        {
            using (var scope = App.Container.CreateScope())
            {
                settingsService = scope.ServiceProvider.GetService<ISettingsService>();
            }
        }

        /// <summary>Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.</summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> was <see langword="null" />.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            string requestUri = request.RequestUri.AbsoluteUri;

            Amazon.S3.Util.AmazonS3Uri s3URI;
            if (!Amazon.S3.Util.AmazonS3Uri.TryParseAmazonS3Uri(requestUri, out s3URI))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(Constants.WrongAWSS3MediaUrl) };
            }

            try
            {
                GetObjectRequest s3Request = new GetObjectRequest
                {
                    BucketName = s3URI.Bucket,
                    Key = s3URI.Key
                };

                //dont dispose the client
                var client = await AWSHelper.GetAmazonS3Client();

                using (GetObjectResponse response = await client.GetObjectAsync(s3Request))
                using (Stream responseStream = response.ResponseStream)
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await responseStream.CopyToAsync(memoryStream);
                    result = new HttpResponseMessage() { Content = new ByteArrayContent(memoryStream.ToArray()) };
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(ex.Message) };
            }
            return result;
        }
    }
}
