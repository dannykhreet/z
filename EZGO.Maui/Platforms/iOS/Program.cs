using System.Diagnostics;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Platforms.iOS.Services;
using ObjCRuntime;
using UIKit;

namespace EZGO.Maui;

public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
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
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}

