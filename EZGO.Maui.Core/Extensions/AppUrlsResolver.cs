using System;
using System.Threading.Tasks;
using Amazon.S3.Model;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.Utils.AWS;

namespace EZGO.Maui.Core.Extensions
{
    public static class AppUrlsResolver
    {
        public static async Task<string> Video(string videoPath)
        {
            var url = "";
            if (videoPath.StartsWith("http"))
                url = videoPath;
            else
                url = Constants.VideoBaseUrl.Format(videoPath);

            var s3URI = AWSHelper.GetParsedS3Uri(url);
            if (s3URI != null)
            {
                var client = await AWSHelper.GetAmazonS3Client();

                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
                {
                    BucketName = s3URI.Bucket,
                    Key = s3URI.Key,
                    Expires = DateTime.Now.AddMinutes(1),
                    Verb = Amazon.S3.HttpVerb.GET
                };

                url = client.GetPreSignedURL(request);
            }

            return url;
        }
    }
}
