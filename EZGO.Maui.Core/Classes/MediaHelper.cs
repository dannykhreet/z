using Autofac;
using EZGO.Maui.Core.Classes.MediaOptions;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Utils;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Media helper.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.Interfaces.Utils.IMediaHelper" />
    public class MediaHelper : IMediaHelper
    {
        private readonly INavigationService navigationService;
        private readonly IStatusBarService statusBarService;
        private readonly IOrientationService orientationService;

        private string photoOptionTitle;
        private string photoGalleryOptionTitle;
        private string videoGalleryOptionTitle;
        private string videoOptionTitle;
        private string removeMediaOptionTitle;
        private string pdfOptionTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaHelper"/> class.
        /// </summary>
        public MediaHelper()
        {
            using var scope = App.Container.CreateScope();
            navigationService = scope.ServiceProvider.GetService<INavigationService>();

            statusBarService = DependencyService.Get<IStatusBarService>();
            orientationService = DependencyService.Get<IOrientationService>();

            InitializeTranslations();
        }

        private void InitializeTranslations()
        {
            photoOptionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuItemPhoto);
            photoGalleryOptionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuItemPhotoGallery);
            videoGalleryOptionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuItemVideoGallery);
            videoOptionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuItemVideo);
            removeMediaOptionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuItemRemoveMedia);
            pdfOptionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuItemPDF);
        }

        /// <summary>
        /// Lets the user pick media asynchronous.
        /// </summary>
        /// <param name="mediaOptions">The media options the user get's to choose from.</param>
        /// <returns>
        /// Chosen media file or null if remove media is chosen or when the right permissions are not given.
        /// </returns>
        public async Task<DialogResult<MediaItem>> PickMediaAsync(IEnumerable<MediaOption> mediaOptions, bool generateVideoThumbnail = true)
        {
            orientationService.SaveOriginalOrientation();
            var dialogResult = await AsyncAwaiter.AwaitResultAsync(nameof(PickMediaAsync), async () =>
             {
                 MediaItem mediaItem = null;
                 Page page = navigationService.GetCurrentPage();

                 string mediaDialogTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuChooseMediaDialogTitle);
                 string mediaDialogCancelTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuChooseMediaDialogCancel);

                 if (!CrossMedia.IsSupported)
                     await DisplaySecurityPermissionAlert(page);
                 else
                 {
                     List<string> dialogOptions = new List<string>();

                     foreach (MediaOption mediaOption in mediaOptions)
                     {
                         AddMediaOption(dialogOptions, mediaOption);
                     }

                     string dialogResult;

                     if (dialogOptions.Count == 1)
                         dialogResult = dialogOptions.SingleOrDefault();
                     else
                         dialogResult = await page.DisplayActionSheet(mediaDialogTitle, mediaDialogCancelTitle, null, dialogOptions.ToArray());

                     IMediaItemPicker picker = InitializeMediaPicker(dialogResult);

                     if (picker == null && dialogResult == mediaDialogCancelTitle) return DialogResult<MediaItem>.Canceled();
                     else if (picker == null)
                     {
                         statusBarService.HideStatusBar();

                         if (dialogResult == removeMediaOptionTitle)
                             return DialogResult<MediaItem>.Removed();

                         return DialogResult<MediaItem>.Success<MediaItem>(null);
                     }
                     try
                     {
                         mediaItem = await picker.PickMediaItem(page);
                     }
                     catch (Exception ex)
                     {
                         Debug.WriteLine($"An Error has occured error msg: {ex.Message}", "[MediaHelper]\n\t");
                         //Crashes.TrackError(ex);
                         await DisplaySecurityPermissionAlert(page);
                         mediaItem = null;
                         orientationService.RestoreOriginalOrientation();
                     }
                 }

                 statusBarService.HideStatusBar();

                 if (mediaItem == null)
                     return DialogResult<MediaItem>.Canceled();

                 return DialogResult<MediaItem>.Success(mediaItem);
             });

            orientationService.RestoreOriginalOrientation();
            return dialogResult;
        }

        private IMediaItemPicker InitializeMediaPicker(string dialogResult)
        {
            if (dialogResult == photoOptionTitle) return new TakePhoto();
            else if (dialogResult == photoGalleryOptionTitle) return new PickGalleryPhoto();
            else if (dialogResult == videoOptionTitle) return new RecordVideo();
            else if (dialogResult == videoGalleryOptionTitle) return new PickVideoGallery();
            else if (dialogResult == pdfOptionTitle) return new PickPDFFile();
            else return null;
        }

        private async Task DisplaySecurityPermissionAlert(Page page)
        {
            string securityPermissionNotSupported = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionNotSupported);
            string securityPermissionNotSupportedDecs = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionNotSupportedDecs);
            string securityPermissionOk = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionOk);

            await page.DisplayAlert(securityPermissionNotSupported, securityPermissionNotSupportedDecs, securityPermissionOk);
        }

        private void AddMediaOption(List<string> dialogOptions, MediaOption mediaOption)
        {
            switch (mediaOption)
            {
                case MediaOption.TakePhoto:
                    dialogOptions.Add(photoOptionTitle);
                    break;
                case MediaOption.PhotoGallery:
                    dialogOptions.Add(photoGalleryOptionTitle);
                    break;
                case MediaOption.VideoGallery:
                    dialogOptions.Add(videoGalleryOptionTitle);
                    break;
                case MediaOption.Video:
                    dialogOptions.Add(videoOptionTitle);
                    break;
                case MediaOption.RemoveMedia:
                    dialogOptions.Add(removeMediaOptionTitle);
                    break;
                case MediaOption.PDF:
                    dialogOptions.Add(pdfOptionTitle);
                    break;
            }
        }

        public async Task<DialogResult<MediaItem>> PickMediaAsync(MediaOption mediaOption, bool generateVideoThumbnail = true)
        {
            var result = await PickMediaAsync(new[] { mediaOption }, generateVideoThumbnail);

            return result;
        }

        public async Task<DialogResult<FileItem>> PickPdfFileAsync()
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions() { FileTypes = FilePickerFileType.Pdf });

            if (file == null || file.FileName.EndsWith(".pdf") == false)
                return DialogResult<FileItem>.Canceled();

            return DialogResult<FileItem>.Success(FileItem.FromLocalFile(file.FullPath));
        }
    }
}
