using System;
using System.Threading.Tasks;
using EZGO.Api.Models.Settings;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.SettingsPreferences.AWSSettings;

namespace EZGO.Maui.Core.Interfaces.Data
{
    public interface ISettingsService : IDisposable
    {
        Task<CompanySettings> GetApplicationSettingsAsync();
        Task<AvailableLanguages> GetAvailableLanguagesAsync();
        Task<bool> HandleAWSCredentials();
    }
}
