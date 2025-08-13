using EZGO.Maui.Core.Classes.DeviceFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Plugin.DeviceInfo;

namespace EZGO.Maui.Core.Classes
{
    public static class DeviceSettings
    {

        #region Keys

        private const string userAgentHttpHeader = "UserAgentHttpHeader";
        private const string deviceInfoHttpHeader = "DeviceInfoHttpHeader";
        private const string deviceIdHttpHeader = "DeviceIdHttpHeader";

        #endregion

        public static string UserAgentHttpHeader
        {
            get => Preferences.Get(userAgentHttpHeader, string.Empty);
            set => Preferences.Set(userAgentHttpHeader, EncodingHelpers.ForceUTF8(value));
        }

        public static string DeviceInfoHttpHeader
        {
            get => Preferences.Get(deviceInfoHttpHeader, string.Empty);
            set => Preferences.Set(deviceInfoHttpHeader, EncodingHelpers.ForceUTF8(value));
        }

        public static string DeviceIdHttpHeader
        {
            get => Preferences.Get(deviceIdHttpHeader, string.Empty);
            set => Preferences.Set(deviceIdHttpHeader, EncodingHelpers.ForceUTF8(value));
        }

        public static double ScreenHeight { get; set; }
        public static double ScreenHeightInUnits { get; set; }
        public static double ScreenWidth { get; set; }
        public static double ScreenDencity { get; set; }
        public static BaseFormat DeviceFormat { get; set; }
        public static bool PhoneViewsEnabled { get; internal set; } = false;

        /// <summary>
        /// Loads current device information.
        /// </summary>
        /// <remarks>Must be called from a UI thread.</remarks>
        public static void LoadDeviceInfo()
        {
            // e.g. EZ-GO APP (iPad; iOS13.1; en-us) EZ-GO.APP.XAM/2.1.2365
            UserAgentHttpHeader = string.Format("EZ-GO APP ({0}/{1}; {2}{3}; {4}) {5}/{6}.{7}",
                DeviceInfo.Manufacturer,
                DeviceInfo.Name,
                DeviceInfo.Platform,
                DeviceInfo.VersionString,
                CultureInfo.CurrentCulture.IetfLanguageTag,
                AppInfo.PackageName,
                AppInfo.VersionString,
                AppInfo.BuildString);

            // When accessing DeviceDisplay it must be done from the UI thread, otherwise
            // on iOS you will get UI Kit consistency exception
            // e.g. 1600x1200;Landscape;8.4Gb;1.3Gb;2Gb;A64;122;
            DeviceInfoHttpHeader = string.Format("{0}x{1};{2};",
                DeviceDisplay.MainDisplayInfo.Width,
                DeviceDisplay.MainDisplayInfo.Height,
                DeviceDisplay.MainDisplayInfo.Orientation);

            DeviceIdHttpHeader = "placeholder";//CrossDeviceInfo.Current.Id;
            ScreenHeightInUnits = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        }
    }
}
