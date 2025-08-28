using System;
using CoreGraphics;
using System.Diagnostics;
using UIKit;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class MediaResizer : IMediaResizer
    {

        public byte[] ResizeImage(byte[] arr, int width, int height)
        {
            UIImage originalImage = ImageFromByteArray(arr);
            var sourceSize = originalImage.Size;
            var maxResizeFactor = Math.Min(width / sourceSize.Width, height / sourceSize.Height);
            if (maxResizeFactor > 1) return originalImage.AsJPEG().ToArray();

            var newWidth = maxResizeFactor * sourceSize.Width;
            var newHeight = maxResizeFactor * sourceSize.Height;

            UIGraphics.BeginImageContext(new CGSize(newWidth, newHeight));

            originalImage.Draw(new CGRect(0, 0, newWidth, newHeight), CGBlendMode.Normal, 1f);

            var resultImage = UIGraphics.GetImageFromCurrentImageContext();

            UIGraphics.EndImageContext();

            return resultImage.AsJPEG(0.65f).ToArray();
        }

        public static UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            UIImage image;
            try
            {
                image = new UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }
    }
}

