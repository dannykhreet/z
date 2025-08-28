using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Utils;
using System;
using System.Net.Http.Headers;
using System.Text;


namespace EZGO.Maui.Core.Extensions
{
    /// <summary>
    /// Extension for <see cref="HttpRequestHeaders"/>
    /// </summary>
    public static class HttpRequestHeadersExtensions
    {
        /// <summary>
        /// Adds Ezgo request headers to the collection.
        /// </summary>
        /// <param name="headers">Collection of headers to add the Ezgo headers to.</param>
        public static void AddEzgoHeaders(this HttpRequestHeaders headers)
        {
            // Time zone
            headers.Add(Constants.EzgoTimeZoneHttpHeader, EncodingHelpers.ForceUTF8(TimeZoneInfo.Local.Id.ToLower()));

            // Company Id
            if (UserSettings.CompanyId != 0)
            {
                headers.Add(Constants.EzgoCompanyIdHttpHeader, UserSettings.CompanyId.ToString());
            }

            // User agent
            var userAgentHeader = DeviceSettings.UserAgentHttpHeader;
            if (!string.IsNullOrWhiteSpace(userAgentHeader))
            {
                headers.Add(Constants.EzgoUserAgentHttpHeader, userAgentHeader);
            }

            // Device information
            var deviceInfoHeader = DeviceSettings.DeviceInfoHttpHeader;
            if (!string.IsNullOrWhiteSpace(deviceInfoHeader))
            {
                headers.Add(Constants.EzgoDeviceInformationHttpHeader, deviceInfoHeader);
            }

            // Device ID
            var deviceId = DeviceSettings.DeviceIdHttpHeader;
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                headers.Add(Constants.EzgoDeviceIdHttpHeader, deviceId);
            }

            // Language
            string languageTag = Settings.CurrentLanguageTag;
            if (!string.IsNullOrWhiteSpace(languageTag))
            {
                headers.Add(Constants.EzgoLanguageHttpHeader, languageTag);
            }
        }
    }
}
