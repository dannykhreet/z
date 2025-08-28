using Autofac;
using EZGO.Maui.Core.Classes.LanguageResources;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Login;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    public static class UserStatusManager
    {

        public static async Task SignOffAsync(INavigationService navigationService)
        {
            using var scope = App.Container.CreateScope();
            var syncService = scope.ServiceProvider.GetService<ISyncService>();
            var fileService = DependencyService.Get<IFileService>();
            var cachingService = DependencyService.Get<ICachingService>();
            var loginService = scope.ServiceProvider.GetService<ILoginService>();

            await loginService.SignOutAsync(UserSettings.Username);
            var currentLanguageTag = Settings.CurrentLanguageTag;

            Preferences.Clear();
            // Important, always call LoadDeviceInfoAsync() after Preferences.Clear()
            DeviceSettings.LoadDeviceInfo();
            fileService.ClearInternalStorageFolder(Constants.SessionDataDirectory);
            cachingService.ClearCache();
            syncService.StopMediaDownload();

            Settings.CurrentLanguageTag = currentLanguageTag;
            await navigationService.NavigateAsync<LoginViewModel>(noHistory: true, animated: false);
        }
    }
}
