using System;
using Android.App;
using Android.Views;
using EZGO.Maui.Core.Interfaces.Utils;
using Plugin.CurrentActivity;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class StatusBarService : IStatusBarService
    {
        public void HideStatusBar()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Activity activity = CrossCurrentActivity.Current.Activity;

                if (activity != null)
                {
                    int uiOptions = (int)activity.Window.DecorView.SystemUiVisibility;

                    uiOptions |= (int)SystemUiFlags.LowProfile;
                    uiOptions |= (int)SystemUiFlags.Fullscreen;
                    uiOptions |= (int)SystemUiFlags.HideNavigation;
                    uiOptions |= (int)SystemUiFlags.ImmersiveSticky;

                    activity.Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);
                    activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;

                    WindowManagerLayoutParams attributes = activity.Window.Attributes;
                    attributes.Flags |= WindowManagerFlags.Fullscreen;
                    activity.Window.Attributes = attributes;
                }
            });
        }
    }
}

