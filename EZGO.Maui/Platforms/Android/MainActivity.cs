using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using AndroidX.Core.View;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.LanguageResources;
using MediaManager;

namespace EZGO.Maui;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Landscape)]
public class MainActivity : MauiAppCompatActivity
{
    public static Activity CurrentActivity { get; private set; }

    public static DisplayMetrics DisplayMetrics { get; private set; }

    private static bool isResumed = false;

    protected override void OnStart()
    {
        SetCurrentLanguage();

        base.OnStart();
    }

    protected override void OnResume()
    {
        SetCurrentLanguage();
        base.OnResume();

    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        DisplayMetrics = Resources.DisplayMetrics;

        WindowCompat.SetDecorFitsSystemWindows(this.Window, false);
        WindowInsetsControllerCompat windowInsetsController = new WindowInsetsControllerCompat(this.Window, this.Window.DecorView);
        // Hide system bars
        windowInsetsController.Hide(WindowInsetsCompat.Type.SystemBars());
        windowInsetsController.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        CrossMediaManager.Current.Init(this);
    }

    private void SetCurrentLanguage()
    {
        //if (Settings.CurrentLanguageTag == string.Empty)
        //{
        Java.Util.Locale locale = Java.Util.Locale.GetDefault(Java.Util.Locale.Category.Format);
        string tag = locale.ToLanguageTag().ToLower();
        Settings.CurrentLanguageTag = Settings.CurrentLanguageTag == "" ? tag : Settings.CurrentLanguageTag.ToLower();

        if (!tag.Contains(Settings.CurrentLanguageTag))
        {
            Settings.CurrentLanguageTag = tag;
            Settings.DefaultDeviceLanguageTag = tag;
            //}
            Language.SetCultureInfo();

            var language = new Language();
            Task.Run(() => language.GetResourcesAsync(false)).Wait();
        }
        isResumed = false;
    }

    protected override void OnStop()
    {
        isResumed = true;
        base.OnStop();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }
}

