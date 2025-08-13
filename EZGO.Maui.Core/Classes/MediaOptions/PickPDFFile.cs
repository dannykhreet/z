using System;
using System.IO;
using System.Threading.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.MediaOptions
{
    public class PickPDFFile : IMediaItemPicker
    {
        public async Task<MediaItem> PickMediaItem(Page page)
        {
            try
            {
                MediaItem mediaItem = MediaItem.Empty();
                var pickerOptions = new PickOptions() { FileTypes = FilePickerFileType.Pdf };
                var result = await FilePicker.PickAsync(pickerOptions);
                if (result != null)
                {
                    mediaItem.IsFile = true;
                    mediaItem.FileUrl = result.FullPath;
                    mediaItem.IsLocalFile = true;

                    var stream = await result.OpenReadAsync();
                    mediaItem.FileStream = stream;
                    mediaItem.MediaFile = new Plugin.Media.Abstractions.MediaFile(result.FullPath, () => stream);
                }

                return mediaItem;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
                return MediaItem.Empty();
            }
        }
    }
}