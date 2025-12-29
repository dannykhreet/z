using Amazon.S3;
using Amazon.S3.Model;
using Elastic.Apm.Api;
using EZGO.CMS.LIB.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebOptimizer;

namespace WebApp.Logic.Services
{
    public class MediaService : IMediaService
    {
        private AmazonS3Client _s3;
        private readonly IApiConnector _connector;
        private DateTime _expirationDate;
        private bool _apiConnectionBroken = false;
        public MediaService(IApiConnector connector)
        {
            _connector = connector;
            _expirationDate = DateTime.UtcNow.AddSeconds(-1);
        }

        public async Task<bool> Initialize()
        {
            var response = await _connector.PostCall(url: "/v1/authentication/fetchmediatoken", body: "".ToJsonFromObject());

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                MediaToken token = response.Message.ToObjectFromJson<MediaToken>();

                if (token != null && !string.IsNullOrEmpty(token.AccessKeyId) && !string.IsNullOrEmpty(token.SecretAccessKey) && !string.IsNullOrEmpty(token.SessionToken))
                {
                    _s3 = new AmazonS3Client(token.AccessKeyId, token.SecretAccessKey, token.SessionToken, Amazon.RegionEndpoint.EUCentral1);
                    _expirationDate = token.Expiration;
                    return true;
                }
            }
            else
            {
                _apiConnectionBroken = true;
            }

            return false;
        }

        public async Task<string> GetMediaAsBase64(string url)
        {
            if (_s3 == null || DateTime.UtcNow > _expirationDate)
            {
                if (_apiConnectionBroken || !await Initialize())
                {
                    return "/assets/img/normal_unavailable_image.png";
                }
            }

            if (url.StartsWith("blob:") || url.StartsWith("/"))
            {
                return url;
            }

            if (url == string.Empty)
            {
                return "/assets/img/normal_unavailable_image.png";
            }

            var tempUrl = url.Replace("https://", "");
            var bucketname = tempUrl.Split(".s3")[0];
            var startIndex = tempUrl.IndexOf('/') + 1;
            var key = tempUrl.Substring(startIndex, tempUrl.Length - startIndex);
            var request = new GetObjectRequest() { BucketName = bucketname, Key = key };

            if (key == "" || bucketname == "")
            {
                return "/assets/img/normal_unavailable_image.png";
            }

            var getObjectResponse = await _s3.GetObjectAsync(request);
            if (getObjectResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                using (Stream responseStream = getObjectResponse.ResponseStream)
                {
                    var mediaBytes = ReadStream(responseStream);
                    var mimeType = getObjectResponse.Headers.ContentType;
                    var base64Media = Convert.ToBase64String(mediaBytes);
                    getObjectResponse.ResponseStream.Dispose();

                    //base64 example:
                    //"data:image/png;base64, iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg=="
                    var imageSrc = string.Format("data:{0};base64, {1}", mimeType, base64Media);
                    return imageSrc;
                }
            }
            else
            {
                if (_apiConnectionBroken || !await Initialize())
                {
                    return "/assets/img/normal_unavailable_image.png";
                }
                return await GetMediaAsBase64(url);
            }

        }

        public async Task<string> GetPreSignedURL(string url)
        {
            if (_s3 == null || DateTime.UtcNow > _expirationDate)
            {
                if (_apiConnectionBroken || !await Initialize())
                {
                    return "/assets/img/normal_unavailable_image.png";
                }
            }

            if (url.StartsWith("blob:") || url.StartsWith("/"))
            {
                return url;
            }

            if (url == string.Empty)
            {
                return "/assets/img/normal_unavailable_image.png";
            }

            var tempUrl = url.Replace("https://", "");
            var bucketname = tempUrl.Split(".s3")[0];
            var startIndex = tempUrl.IndexOf('/') + 1;
            var key = tempUrl.Substring(startIndex, tempUrl.Length - startIndex);
            var request = new GetPreSignedUrlRequest() { BucketName = bucketname, Key = key };

            if (key == "" || bucketname == "")
            {
                return "/assets/img/normal_unavailable_image.png";
            }

            var preSignedURL = _s3.GetPreSignedURL(request);
            return preSignedURL;
        }

        public static byte[] ReadStream(Stream responseStream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
