using System;
using System.Threading.Tasks;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using Plugin.Media;
using Plugin.Media.Abstractions;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.MediaOptions
{
    public class PickGalleryPhoto : IMediaItemPicker
    {
        private readonly string securityPermissionPhotos;
        private readonly string securityPermissionOk;

        public PickGalleryPhoto()
        {
            securityPermissionPhotos = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionPhotos);
            securityPermissionOk = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionOk);
        }

        public async Task<MediaItem> PickMediaItem(Page page)
        {
            if (await PermissionsHelper.CheckAndRequestPermissionAsync<Permissions.Photos>())
            {
                var mediaFile = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions()
                {
                    SaveMetaData = true,
                    RotateImage = true
                });

                if (mediaFile == null)
                    return null;

                return new MediaItem
                {
                    IsVideo = false,
                    IsLocalFile = true,
                    MediaFile = mediaFile,
                    PictureUrl = mediaFile.Path
                };
            }
            else
                await page.DisplayAlert(securityPermissionPhotos, securityPermissionPhotos, securityPermissionOk);

            return null;
        }
    }
}
