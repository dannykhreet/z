using System;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using UIKit;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class DeviceSizeService : IDeviceSizeService
    {
        public DeviceSizeService()
        {
        }

        public double CalculateDeviceSizeInInches()
        {
            var bounds = UIScreen.MainScreen.Bounds;

            nfloat scale = UIScreen.MainScreen.Scale;

            nfloat ppi = scale * 132;

            nfloat width = bounds.Width * scale;
            nfloat height = bounds.Height * scale;

            nfloat horizontal = width / ppi, vertical = height / ppi;

            return Math.Sqrt(Math.Pow((double)horizontal, 2) + Math.Pow((double)vertical, 2));
        }

        public void SetDeviceSize()
        {
            DeviceSettings.ScreenWidth = UIScreen.MainScreen.Bounds.Width;
            DeviceSettings.ScreenHeight = UIScreen.MainScreen.Bounds.Height;
        }
    }
}

