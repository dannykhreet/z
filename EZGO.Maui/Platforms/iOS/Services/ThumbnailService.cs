using AVFoundation;
using CoreGraphics;
using CoreMedia;
using EZGO.Maui.Core.Interfaces.File;
using Foundation;
using UIKit;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class ThumbnailService : IThumbnailService
    {
        public byte[] GenerateThumbnail(string filePath)
        {
            AVAsset asset = AVAsset.FromUrl(NSUrl.FromFilename(filePath));

            AVAssetImageGenerator imageGenerator = AVAssetImageGenerator.FromAsset(asset);
            imageGenerator.AppliesPreferredTrackTransform = true;

            CMTime actualTime = asset.Duration;
            CMTime cmTime = new CMTime(1, 1000000);
            NSError error;

            CGImage cgImage = imageGenerator.CopyCGImageAtTime(cmTime, out actualTime, out error);

            byte[] bytes = new UIImage(cgImage).AsPNG().ToArray();

            return bytes;
        }
    }
}