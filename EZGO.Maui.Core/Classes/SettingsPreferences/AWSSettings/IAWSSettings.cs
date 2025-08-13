using System;
namespace EZGO.Maui.Core.Classes.SettingsPreferences.AWSSettings
{
    public interface IAWSSettings
    {
        public string AccessKeyId { get; set; }
        public DateTime Expiration { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
        Amazon.RegionEndpoint AWSRegion => Amazon.RegionEndpoint.EUCentral1;
    }
}
