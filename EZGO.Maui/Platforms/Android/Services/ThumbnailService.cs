using Android.Graphics;
using Android.Media;
using EZGO.Maui.Core.Interfaces.File;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class ThumbnailService : IThumbnailService
    {
        public byte[] GenerateThumbnail(string filePath)
        {
            MediaMetadataRetriever mediaMetadataRetriever = new MediaMetadataRetriever();
            mediaMetadataRetriever.SetDataSource(filePath);

            Bitmap bitmap = mediaMetadataRetriever.GetFrameAtTime(1);

            byte[] bitmapData = null;

            if (bitmap != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, memoryStream);
                bitmapData = memoryStream.ToArray();
            }

            return bitmapData;
        }
    }
}