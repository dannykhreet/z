using System;
using EZGO.Maui.Core.Interfaces.Utils;
using Foundation;
using UIKit;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class OrientationService : IOrientationService
    {
        private static UIDeviceOrientation OriginalOrientation = UIDeviceOrientation.LandscapeRight;
        public void SaveOriginalOrientation()
        {
            if (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeLeft || UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeRight)
                OriginalOrientation = UIDevice.CurrentDevice.Orientation;
        }

        public void RestoreOriginalOrientation()
        {
            if (UIDevice.CurrentDevice.Orientation != OriginalOrientation)
                UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)OriginalOrientation), new NSString("orientation"));
        }
    }
}

