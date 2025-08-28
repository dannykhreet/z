using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Data;
using System.Diagnostics;

namespace EZGO.Maui.Classes.Timers
{
    /// <summary>
    /// Performs performance data syncing every 60 seconds.
    /// Starts at the minute mark and continues at consistent 1-minute intervals.
    /// </summary>
    public class DeviceOneMinuter
    {
        private const int intervalInMinutes = 1;
        private static DeviceTimer Timer;

        private DeviceOneMinuter() { }

        public static DeviceTimer Instance()
        {
            if (Timer == null)
            {
                Timer = new DeviceTimer(PerformOneMinuteSync, TimeSpan.FromMinutes(intervalInMinutes));
            }

            return Timer;
        }

        private static async Task PerformOneMinuteSync()
        {
            if (Settings.WorkAreaId <= 0) return;

            bool hasInternet = await InternetHelper.HasInternetConnection();
            if (!hasInternet) return;

            if (Settings.HasCrashed)
            {
                Settings.HasCrashed = false;
                return;
            }

            await UpdateLocalDataAsync();
        }

        private static async Task UpdateLocalDataAsync()
        {
            try
            {
                var syncService = Application.Current.MainPage?.Handler?.MauiContext?.Services?.GetService<ISyncService>();

                if (syncService == null)
                {
                    // Optionally log: "SyncService could not be retrieved."
                    return;
                }
                Debug.WriteLine($"CheckForUpdated every minute");

                await syncService.UpdateLocalDataAsync();
                syncService.Dispose();
            }
            catch (Exception ex)
            {
                // Optionally log exception
            }
        }
    }
}
