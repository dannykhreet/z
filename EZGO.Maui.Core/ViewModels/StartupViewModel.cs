using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.HealthCheck;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;

namespace EZGO.Maui.Core.ViewModels
{
    public class StartupViewModel : BaseViewModel
    {
        public StartupViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        public static async Task NavigateToStartupPage()
        {
            using var scope = App.Container.CreateScope();
            var navigationService = scope.ServiceProvider.GetService<INavigationService>();
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = await StartupHelper.NavigateToStartupPage();
                await navigationService.PushAsync(page, true);
            });
            await SynchroniseLocalData().ConfigureAwait(false);
        }

        private static async Task SynchroniseLocalData()
        {
            using var scope = App.Container.CreateScope();
            var healthService = scope.ServiceProvider.GetService<IHealthCheckService>();

            if (await InternetHelper.HasInternetConnection() && await healthService.ValidateTokenAsync(Settings.Token))
            {
                Settings.UpdateDates();
                var cachingService = DependencyService.Get<ICachingService>();
                cachingService.ClearCache();

                var syncService = scope.ServiceProvider.GetService<ISyncService>();
                await Task.Run(syncService.GetLocalDataAsync);
            }
        }
    }
}
