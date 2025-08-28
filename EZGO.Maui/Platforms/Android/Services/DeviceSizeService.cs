using System;
using Android.Util;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class DeviceSizeService : IDeviceSizeService
    {
        public DeviceSizeService()
        {
        }

        public double CalculateDeviceSizeInInches()
        {
            DisplayMetrics dm = MainActivity.DisplayMetrics;
            int width = dm.WidthPixels;
            int height = dm.HeightPixels;
            double xa = Math.Pow(width, 2);
            double ya = Math.Pow(height, 2);
            double diagonal = Math.Sqrt(xa + ya);

            return diagonal / (double)dm.DensityDpi;
        }

        public void SetDeviceSize()
        {
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            DeviceSettings.ScreenWidth = mainDisplayInfo.Width;
            DeviceSettings.ScreenHeight = mainDisplayInfo.Height;
        }
    }
}

