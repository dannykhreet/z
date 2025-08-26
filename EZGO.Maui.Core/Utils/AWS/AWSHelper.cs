using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Autofac;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Data;

namespace EZGO.Maui.Core.Utils.AWS
{
    public static class AWSHelper
    {
        private static readonly SemaphoreSlim HandleCredentialsSemaphore = new SemaphoreSlim(1, 1);

        private static AmazonS3Client S3Client;

        private async static Task HandleAWSCredentials()
        {
            using (var scope = App.Container.CreateScope())
            {
                var settingsService = scope.ServiceProvider.GetService<ISettingsService>();

                bool lockTaken = false;
                await HandleCredentialsSemaphore.WaitAsync();
                lockTaken = true;
                try
                {
                    var credentialsRefreshed = await settingsService.HandleAWSCredentials();
                    if (credentialsRefreshed || S3Client == null)
                    {
                        DebugService.WriteLine("Refreshing Amazon s3 client", "AWSHelper");
                        S3Client = new AmazonS3Client(region: Settings.AWSSettings.AWSRegion, awsAccessKeyId: Settings.AWSSettings.AccessKeyId, awsSecretAccessKey: Settings.AWSSettings.SecretAccessKey, awsSessionToken: Settings.AWSSettings.SessionToken);
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        HandleCredentialsSemaphore.Release();
                    }
                }
            }
        }

        public static Amazon.S3.Util.AmazonS3Uri GetParsedS3Uri(string requestUri)
        {
            Amazon.S3.Util.AmazonS3Uri s3URI;
            Amazon.S3.Util.AmazonS3Uri.TryParseAmazonS3Uri(requestUri, out s3URI);
            return s3URI;
        }

        public static async Task<AmazonS3Client> GetAmazonS3Client()
        {
            await HandleAWSCredentials();
            return S3Client;
        }
    }
}
