using Android.App;
using Android.Runtime;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Platforms.Android.Services;

namespace EZGO.Maui;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        DependencyService.Register<IFileService, FileService>();
        DependencyService.Register<ICachingService, CachingService>();
        DependencyService.Register<IStatusBarService, StatusBarService>();
        DependencyService.Register<ITextMeter, TextMeterService>();
        DependencyService.Register<IDeviceSizeService, DeviceSizeService>();
        DependencyService.Register<IOrientationService, OrientationService>();
        DependencyService.Register<IPdfService, PdfService>();
        DependencyService.Register<IMediaResizer, MediaResizer>();
        DependencyService.Register<IThumbnailService, ThumbnailService>();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

