using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface IMediaService
    {
        Task<string> UploadPictureAsync(Stream media, MediaStorageTypeEnum mediaStorageType, int id, string filename);

        Task UploadMediaItemAsync(MediaItem mediaItem, MediaStorageTypeEnum mediaStorageType, int id, bool hasWatermark = false);

        Task UploadMediaItemsAsync(IEnumerable<MediaItem> mediaItems, MediaStorageTypeEnum mediaStorageType, int id);
    }
}
