using System;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes.SettingsPreferences.AWSSettings
{
    public class AWSSettings : IAWSSettings
    {
        static IAWSSettings awsSettings;

        private const string accessKeyId = "accessKeyId";
        private const string expiration = "expiration";
        private const string secretAccessKey = "secretAccessKey";
        private const string sessionToken = "sessionToken";

        private AWSSettings()
        {
        }

        public static IAWSSettings Instance()
        {
            if (awsSettings == null)
            {
                awsSettings = new AWSSettings();
            }

            return awsSettings;
        }

        public string AccessKeyId { get => Preferences.Get(accessKeyId, string.Empty); set => Preferences.Set(accessKeyId, value); }
        public DateTime Expiration { get => Preferences.Get(expiration, DateTime.MinValue); set => Preferences.Set(expiration, value); }
        public string SecretAccessKey { get => Preferences.Get(secretAccessKey, string.Empty); set => Preferences.Set(secretAccessKey, value); }
        public string SessionToken { get => Preferences.Get(sessionToken, string.Empty); set => Preferences.Set(sessionToken, value); }

        public Amazon.RegionEndpoint AWSRegion => Amazon.RegionEndpoint.EUCentral1;

        public void ToSettings()
        {
            Settings.AWSSettings.AccessKeyId = AccessKeyId;
            Settings.AWSSettings.Expiration = Expiration;
            Settings.AWSSettings.SecretAccessKey = SecretAccessKey;
            Settings.AWSSettings.SessionToken = SessionToken;
        }
    }
}
