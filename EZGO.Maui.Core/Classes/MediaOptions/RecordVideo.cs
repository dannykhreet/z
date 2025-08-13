using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using Plugin.Media;
using Plugin.Media.Abstractions;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace EZGO.Maui.Core.Classes.MediaOptions
{
    public class RecordVideo : IMediaItemPicker
    {
        private readonly IThumbnailGenerator generator;

        private readonly string securityPermissionCamera;
        private readonly string securityPermissionOk;


        public RecordVideo()
        {
            using var scope = App.Container.CreateScope();
            generator = scope.ServiceProvider.GetService<IThumbnailGenerator>();

            securityPermissionCamera = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionCamera);
            securityPermissionOk = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionOk);
        }

        public async Task<MediaItem> PickMediaItem(Page page)
        {
            bool hasVideoPermission = await PermissionsHelper.CheckAndRequestPermissionAsync<Camera>();
            bool hasMicrophonePermission = await PermissionsHelper.CheckAndRequestPermissionAsync<Microphone>();

            if (hasVideoPermission && hasMicrophonePermission)
            {
                var mediaFile = await CrossMedia.Current.TakeVideoAsync(new StoreVideoOptions
                {
                    Directory = "media",
                    SaveToAlbum = false,
                    Name = $"video_{DateTime.Now:yyyyMMdd_HHmmss}.mp4"
                });

                if (mediaFile == null)
                    return null;

                return new MediaItem
                {
                    IsVideo = true,
                    IsLocalFile = true,
                    MediaFile = mediaFile,
                    VideoUrl = mediaFile.Path,
                    PictureUrl = generator.GenerateThumbnail(mediaFile.Path),
                    ShouldRemoveAfterUpload = true
                };
            }
            else
                await page.DisplayAlert(securityPermissionCamera, securityPermissionCamera, securityPermissionOk);

            return null;
        }
    }
}
