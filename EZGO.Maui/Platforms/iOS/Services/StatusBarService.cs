using System;
using EZGO.Maui.Core.Interfaces.Utils;
using UIKit;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class StatusBarService : IStatusBarService
    {
        public void HideStatusBar()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.StatusBarHidden = true;
            });
        }
    }
}

