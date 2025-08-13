using EZGO.Maui.Core.Classes;
using Foundation;
using UIKit;

namespace EZGO.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        SetCurrentLanguage();

        return base.FinishedLaunching(application, launchOptions);
    }

    private static void SetCurrentLanguage()
    {
        var localeIdentifier = NSLocale.PreferredLanguages[0].ToLower();
        Settings.CurrentLanguageTag = localeIdentifier;
        Settings.DefaultDeviceLanguageTag = localeIdentifier;
    }
}