using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Networking;

namespace EZGO.Maui.Core.Classes
{
    public class ImageHelpers
    {
        private static string _imageUrl;

        private static string _imageKey;

        private static readonly HttpClient httpClient = Statics.AWSS3MediaHttpClient;

        public ImageHelpers()
        {
            httpClient.Timeout = TimeSpan.FromSeconds(100);
        }

        public static string GetFullImageUrlFromName(string imageName)
        {
            if (File.Exists(imageName))
                return ConvertLocalImageToBase64(imageName);

            if (!string.IsNullOrWhiteSpace(imageName))
                _imageUrl = Constants.MediaBaseUrl.Format(imageName).Replace("/media/media", "/media");
            else
                return Constants.PlaceholderImage;

            if (Connectivity.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.None)
            {
                //_imageKey = ImageService.Instance.Config.MD5Helper.MD5(_imageUrl);
                try
                {
                    return Task.Run(async () => await ConvertCachedFileIntoBase64Async(_imageKey)).Result;
                }
                catch
                {
                    return Constants.PlaceholderImage;
                }
            }
            else if (Connectivity.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet)
            {
                return Task.Run(async () => await ConvertOnlineImageIntoBase64Async(_imageUrl)).Result;
            }

            return _imageUrl;
        }

        private static async Task<string> ConvertOnlineImageIntoBase64Async(string imageUrl)
        {
            var response = await httpClient.GetAsync(imageUrl);

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();

                var extension = Path.GetExtension(imageUrl);

                var base64 = Convert.ToBase64String(bytes);

                return FormatBase64ToHtmlImgBase64String(base64, extension);
            }

            return Constants.PlaceholderImage;
        }

        public static string ConvertLocalImageToBase64(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);

            var extension = Path.GetExtension(imagePath);

            var base64 = Convert.ToBase64String(bytes);

            return FormatBase64ToHtmlImgBase64String(base64, extension);
        }

        public static async Task<string> ConvertCachedFileIntoBase64Async(string imageKey)
        {
            //var imagePath = await ImageService.Instance.Config.DiskCache.GetFilePathAsync(imageKey);
            var imagePath = "";

            if (!File.Exists(imagePath))
                return Constants.PlaceholderImage;

            var bytes = File.ReadAllBytes(imagePath);

            //var extension = FFImageLoading.Helpers.FileHeader.GetImageType(bytes);

            var extension = "";

            var base64 = Convert.ToBase64String(bytes);

            return FormatBase64ToHtmlImgBase64String(base64, extension.ToString());
        }

        private static string FormatBase64ToHtmlImgBase64String(string base64, string extension)
        {
            return $"data:image/{extension.ToLower()};base64, " + base64;
        }
    }
}
