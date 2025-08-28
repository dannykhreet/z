using System;
using Android.Graphics;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class MediaResizer : IMediaResizer
    {
        public byte[] ResizeImage(byte[] imageData, int width, int height)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();// Create object of bitmapfactory's option method for further option use
            options.InPurgeable = true; // inPurgeable is used to free up memory while required
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length, options);
            var maxResizeFactor = Math.Min(width / originalImage.Width, height / originalImage.Height);

            Bitmap resizedImage;
            if (maxResizeFactor <= 0 || maxResizeFactor >= 1)
            {
                resizedImage = Bitmap.CreateBitmap(originalImage);
            }
            else
            {
                var newWidth = maxResizeFactor * originalImage.Width;
                var newHeight = maxResizeFactor * originalImage.Height;
                resizedImage = Bitmap.CreateScaledBitmap(originalImage, newWidth, newHeight, true);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 65, ms);

                resizedImage.Recycle();

                if (originalImage != null)
                    originalImage.Recycle();

                return ms.ToArray();
            }
        }
    }
}

