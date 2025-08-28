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
    public class TakePhoto : IMediaItemPicker
    {
        private readonly string securityPermissionCamera;
        private readonly string securityPermissionOk;
        private readonly string takePhotoTitle;

        public TakePhoto()
        {
            securityPermissionCamera = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionCamera);
            securityPermissionOk = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionOk);
            takePhotoTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.takePhotoTitle);
        }

        public async Task<MediaItem> PickMediaItem(Page page)
        {
            if (await PermissionsHelper.CheckAndRequestPermissionAsync<Camera>())
            {
                MediaFile mediaFile;

                // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var file = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                    {
                        Title = takePhotoTitle
                    });

                    if (file == null)
                    return null;

                    var stream = await file.OpenReadAsync();
                    var localPath = Path.Combine(FileSystem.CacheDirectory, file.FileName);

                    mediaFile = new Plugin.Media.Abstractions.MediaFile(localPath, () => stream);

                    using (var localFile = File.OpenWrite(localPath))
                    await stream.CopyToAsync(localFile);
                }
                else
                    mediaFile = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
                    {
                        SaveToAlbum = false,
                        Directory = "media",
                        PhotoSize = PhotoSize.MaxWidthHeight,
                        DefaultCamera = CameraDevice.Rear,
                        CompressionQuality = 92,
                        MaxWidthHeight = 1920
                    });

                if (mediaFile == null)
                    return null;

                return new MediaItem
                {
                    IsVideo = false,
                    IsLocalFile = true,
                    MediaFile = mediaFile,
                    PictureUrl = mediaFile.Path,
                    ShouldRemoveAfterUpload = true
                };
            }
            else
                await page.DisplayAlert(securityPermissionCamera, securityPermissionCamera, securityPermissionOk);

            return null;
        }
    }
}
