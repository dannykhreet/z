using System;
using System.Collections.Generic;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Core.Utils.Environments
{
    public abstract class BaseEnvironment
    {
        protected Dictionary<string, string> data;
        private const string mediaBaseUrl = "MediaBaseUrl";
        private const string videoBaseUrl = "VideoBaseUrl";
        private const string passwordValidationScheme = "PasswordValidationScheme";

        public BaseEnvironment()
        {
            data = PopulateEnvironment();
        }

        public virtual Dictionary<string, string> PopulateEnvironment()
        {
            var dic = new Dictionary<string, string>(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>( mediaBaseUrl, $"{CompanyFeatures.ImageStorageBaseUrl}" + "{0}"),
                new KeyValuePair<string, string>( videoBaseUrl, $"{CompanyFeatures.VideoStorageBaseUrl}" + "{0}"),
                new KeyValuePair<string, string>( passwordValidationScheme, CompanyFeatures.PasswordValidationRegEx),
            });

            return dic;
        }

        public virtual string GetValue(string key)
        {
            return data.GetValueOrDefault(key, string.Empty);
        }

        public void UpdateEnv()
        {
            if (!data.TryAdd(mediaBaseUrl, $"{CompanyFeatures.ImageStorageBaseUrl}" + "{0}"))
            {
                data[mediaBaseUrl] = $"{CompanyFeatures.ImageStorageBaseUrl}" + "{0}";
            }
            if (!data.TryAdd(videoBaseUrl, $"{CompanyFeatures.VideoStorageBaseUrl}" + "{0}"))
            {
                data[videoBaseUrl] = $"{CompanyFeatures.VideoStorageBaseUrl}" + "{0}";
            }
            if (!data.TryAdd(passwordValidationScheme, $"{CompanyFeatures.PasswordValidationRegEx}" + "{0}"))
            {
                data[passwordValidationScheme] = $"{CompanyFeatures.PasswordValidationRegEx}" + "{0}";
            }
        }
    }
}
