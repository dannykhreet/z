using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.SettingsPreferences.AWSSettings;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Services.Data
{
    public class SettingsService : ISettingsService
    {
        private readonly IApiRequestHandler _apiRequestHandler;

        public SettingsService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }

        public async Task<CompanySettings> GetApplicationSettingsAsync()
        {
            CompanySettings result = await _apiRequestHandler.HandleRequest<CompanySettings>("app/settings");

            if (result != null) result.ToSettings();

            Config.UpdateSettings();

            return result;
        }


        private async Task GetAWSSettingsAsync()
        {
            if (!InternetHelper.HasNetworkAccess)
                return;

            var response = await _apiRequestHandler.HandlePostRequest<string>("authentication/fetchmediatoken", "");
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                AWSSettings settings = JsonSerializer.Deserialize<AWSSettings>(json);
                settings.ToSettings();
            }
        }

        public async Task<bool> HandleAWSCredentials()
        {
            if (Settings.AWSSettings.Expiration > DateTime.Now)
            {
                DebugService.WriteLine("Credentials not expired: " + Settings.AWSSettings.Expiration.ToLongTimeString() + "; Current time: " + DateTime.Now.ToLongTimeString(), "HandleAWSCredentials");
                return false;
            }

            DebugService.WriteLine("Credentials expired", "HandleAWSCredentials");
            await GetAWSSettingsAsync();
            return true;
        }

        public async Task<AvailableLanguages> GetAvailableLanguagesAsync()
        {
            Settings.AvailableLanguages = new List<string>() { "en-gb" }; //default

            var availableLanguages = new AvailableLanguages() { SupportedLanguages = new Dictionary<string, string>() };

            var result = await _apiRequestHandler.HandleRequest<AvailableLanguages>("tools/resources/language/cultures");

            if (result?.SupportedLanguages != null && result?.SupportedLanguages.Keys != null)
            {
                foreach (var key in result.SupportedLanguages)
                {
                    availableLanguages.SupportedLanguages.Add(key.Key.Replace("_", "-"), key.Value);
                }

                //var listOfValues = new List<string>(result.SupportedLanguages.Keys.Select(k => k.Replace('_', '-')));
                var listOfValues = availableLanguages.SupportedLanguages.Keys.ToList();
                Settings.AvailableLanguages = listOfValues;
            }

            return availableLanguages;
        }
    }
}
