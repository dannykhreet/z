using EZGO.Maui.Classes;
using EZGO.Maui.Classes.Timers;
using EZGO.Maui.Core;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DateFormats;
using EZGO.Maui.Core.Classes.DeviceFormats;
using EZGO.Maui.Core.Classes.LanguageResources;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.HealthCheck;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Navigation;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels;
using EZGO.Maui.Views;
using FFImageLoading;
using Newtonsoft.Json;
using System.Diagnostics;

namespace EZGO.Maui;

public partial class MauiEZGOApp : EZGO.Maui.Core.App
{
    private static DeviceTimer OneMinuter;
    private static DeviceTimer FiveteenSeconds;

    private IServiceProvider Container { get; set; }

    public MauiEZGOApp(IServiceProvider serviceProvider)
    {
        Container = serviceProvider;

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH1edHRSRWdcV011V0s=");
        //27.2.4 - Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH1edHRSRWdcV011V0s=
        //26.1.38 - MzM0MDI5NEAzMjM2MmUzMDJlMzBnMVJZT0k0M2llMU5lYkNHWFRVQmRtWmNZR1RKQ3VCRW15OUVoQ3JSeXFzPQ==

        MainPage = new NavigationPage(new StartupPage());

        InitializeComponent();

        Config.SetEnv();
        SetApiUrl();
    }

    protected override async void OnStart()
    {
#if DEBUG
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
#endif
        base.OnStart();

        DeviceSettings.LoadDeviceInfo();

        SetDeviceSettings();

        SetJsonConvertSettings();

        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            await GetSupportedLanguages();
        }

        await LoadLanguage();

        BaseDateFormats.Instance();

        Connectivity.ConnectivityChanged += Connectivity_OnConnectivityChanged;

        AppDomain.CurrentDomain.UnhandledException += Exception_OnUnhandledException;

        ViewManager.RegisterAllViews();

        Page startupPage = ViewFactory.CreateView<StartupViewModel>();

        MainPage = new MainNavigationPage(startupPage);

        await StartupViewModel.NavigateToStartupPage();

        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            Settings.UpdateDates();

        var ffConfig = new FFImageLoading.Config.Configuration
        {
            HttpClient = Statics.AWSS3MediaHttpClient
        };
        var imageService = Container.GetService<IImageService>();
        imageService?.Initialize(ffConfig);

        OneMinuter = DeviceOneMinuter.Instance();
        FiveteenSeconds = DeviceFiveteenSecunder.Instance();

        HideStatusBar();
#if DEBUG
        stopwatch.Stop();
        Debug.WriteLine($"[FormsApp::OnStart]:: App components have been loaded. It took {stopwatch.ElapsedMilliseconds} ms");
#endif
    }

    private async Task GetSupportedLanguages()
    {
        using (var scope = App.Container.CreateScope())
        {
            ISettingsService settingsService = scope.ServiceProvider.GetService<ISettingsService>();
            await settingsService.GetAvailableLanguagesAsync();
        }
    }

    //TODO Move to DeviceSettings Class
    private void SetDeviceSettings()
    {
        var deviceSizeService = DependencyService.Get<IDeviceSizeService>();
        deviceSizeService.SetDeviceSize();
        DeviceSettings.ScreenDencity = deviceSizeService.CalculateDeviceSizeInInches();
        DeviceSettings.DeviceFormat = SetDeviceFormatByDencity(DeviceSettings.ScreenDencity);
    }

    private BaseFormat SetDeviceFormatByDencity(double dencity)
    {
        if (dencity < 8)
        {
            return new EightInchFormat();
        }
        else
        {
            return new BaseFormat();
        }
    }

    private async Task LoadLanguage()
    {
        //if (UserSettings.PreferredLanguage.Length > 0)
        //    Settings.CurrentLanguageTag = UserSettings.PreferredLanguage;

        Language.SetCultureInfo();
        var language = new Language();
        await language.GetResourcesAsync(false);
    }

    private async Task SynchroniseLocalData()
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
    }

    private void Exception_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Settings.HasCrashed = true;
    }

    protected override void OnSleep()
    {
        OneMinuter?.Stop();
        FiveteenSeconds?.Stop();
        base.OnSleep();
    }

    protected override void OnResume()
    {
        base.OnResume();
        OneMinuter?.Restart();
        FiveteenSeconds?.Restart();
        HideStatusBar();
        //LoadLanguage();
    }

    private void HideStatusBar()
    {
        IStatusBarService statusBarService = DependencyService.Get<IStatusBarService>();
        statusBarService.HideStatusBar();
    }

    /// <summary>
    /// Clears the out of date cache.
    /// </summary>
    private static void ClearOutOfDateCache()
    {
        Task.Run(InternetHelper.HasInternetAndApiConnectionIgnoreTokenAsync).ContinueWith(task =>
        {
            if (task.Result)
            {
                ICachingService cachingService = DependencyService.Get<ICachingService>();
                cachingService.ClearOutOfDateCache();
            }
        });
    }

    /// <summary>
    /// Handles the OnConnectivityChanged event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ConnectivityChangedEventArgs"/> instance containing the event data.</param>
    private static async void Connectivity_OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        using (var scope = App.Container.CreateScope())
        {
            IMessageService messageService = scope.ServiceProvider.GetService<IMessageService>();
            ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();
            IHealthCheckService healthService = scope.ServiceProvider.GetService<IHealthCheckService>();

            if (e.NetworkAccess.Equals(NetworkAccess.Internet) && await healthService.ValidateTokenAsync(Settings.Token))
            {
                if (!Statics.SynchronizationRunning)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => messageService.SendMessage(Core.Models.Messaging.Message.Info(Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.syncLocalData), isClosable: false, spinner: true)));
                    await Task.Run(syncService.UploadLocalDataAsync);
                }

                if (RequestHelper.Instance().HasAny())
                {
                    await MainThread.InvokeOnMainThreadAsync(() => messageService.SendMessage(Core.Models.Messaging.Message.Info(Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.syncLocalData), isClosable: false, spinner: true)));
                    await Task.Run(syncService.UploadUnpostedData);
                }

                await MainThread.InvokeOnMainThreadAsync(() => messageService.SendMessage(Core.Models.Messaging.Message.Info(Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.syncLocalDataFinished))));

                if (Settings.DownloadMedia)
                    syncService.StartMediaDownload();

                await OnlineShiftCheck.CheckCycleChange();
            }
            else
            {
                syncService.StopMediaDownload();

                messageService.SendMessage("No internet", Colors.Red, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.Connection);
            }
        }
    }

    /// <summary>
    /// Registers the dependencies.
    /// </summary>
    protected override void RegisterDependencies()
    {
        App.Container = Container;
    }

    private static void SetJsonConvertSettings()
    {
        JsonConvert.DefaultSettings = () =>
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };

            return settings;
        };
    }
}

