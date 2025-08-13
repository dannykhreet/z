using System.Threading.Tasks;
using Autofac;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.HealthCheck;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Services
{
    public class LanguageChangeService
    {
        private readonly string languageTag;
        public LanguageChangeService(string languageTag)
        {
            this.languageTag = languageTag;
        }

        public async Task PerformLanguageChange()
        {
            using var scope = App.Container.CreateScope();
            var healthService = scope.ServiceProvider.GetService<IHealthCheckService>();

            if (await InternetHelper.HasInternetConnection() && await healthService.ValidateTokenAsync(Settings.Token))
            {
                Settings.UpdateDates();
                var cachingService = DependencyService.Get<ICachingService>();
                cachingService.ClearCache();

                var syncService = scope.ServiceProvider.GetService<ISyncService>();
                await syncService.GetLocalDataAsync();
            }

            Settings.CurrentLanguageTag = languageTag;
        }
    }
}
