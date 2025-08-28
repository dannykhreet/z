using System.Threading.Tasks;
using Autofac;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using Plugin.Media;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.MediaOptions
{
    public class PickVideoGallery : IMediaItemPicker
    {
        private readonly IThumbnailGenerator generator;

        private readonly string securityPermissionPhotos;
        private readonly string securityPermissionOk;

        public PickVideoGallery()
        {
            using var scope = App.Container.CreateScope();
            generator = scope.ServiceProvider.GetService<IThumbnailGenerator>();

            securityPermissionPhotos = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionPhotos);
            securityPermissionOk = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionOk);
        }

        public async Task<MediaItem> PickMediaItem(Page page)
        {
            bool hasVideoGalleryPermission = await PermissionsHelper.CheckAndRequestPermissionAsync<Permissions.Photos>();

            if (hasVideoGalleryPermission)
            {
                var mediaFile = await CrossMedia.Current.PickVideoAsync();

                if (mediaFile == null)
                    return null;

                return new MediaItem
                {
                    IsLocalFile = true,
                    IsVideo = true,
                    MediaFile = mediaFile,
                    VideoUrl = mediaFile.Path,
                    PictureUrl = generator.GenerateThumbnail(mediaFile.Path),
                };
            }
            else
                await page.DisplayAlert(securityPermissionPhotos, securityPermissionPhotos, securityPermissionOk);

            return null;
        }
    }
}
