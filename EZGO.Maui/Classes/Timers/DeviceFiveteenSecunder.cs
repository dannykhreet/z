using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Classes.Timers
{
    /// <summary>
    /// Performs syncing every 15 seconds time period
    /// </summary>
    public class DeviceFiveteenSecunder
    {
        private const int intervalInSeconds = 15;
        private static DeviceTimer Timer;

        private DeviceFiveteenSecunder()
        {
        }

        public static DeviceTimer Instance()
        {
            if (Timer == null)
            {
                Timer = new DeviceTimer(PerformFiveteenSecondsSync, TimeSpan.FromSeconds(intervalInSeconds));
            }

            return Timer;
        }

        private static async Task PerformFiveteenSecondsSync()
        {
            var workAreaSelected = Settings.WorkAreaId > 0;
            if (workAreaSelected && await InternetHelper.HasInternetConnection())
            {
                MessagingCenter.Send(Application.Current, Constants.QuickTimer);

                await OnlineShiftCheck.CheckCycleChange();
            }
        }
    }
}
