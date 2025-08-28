using System.Threading.Tasks;
using Autofac;
using EZGO.Maui.Core.Interfaces.Shifts;

namespace EZGO.Maui.Core.Classes.ShiftChecks
{
    public static class ShiftChanged
    {
        public static async Task<bool> PerformChangeAsync()
        {
            using var scope = App.Container.CreateScope();
            var shifts = scope.ServiceProvider.GetService<IShiftService>();
            var currentShift = await shifts.GetCurrentShiftAsync().ConfigureAwait(false);
            shifts.Dispose();

            // If the previous shift doesn't exist yet
            if (Settings.LastCheckedShiftId == -1)
            {
                // Set it to current one
                Settings.LastCheckedShiftId = currentShift.Id;
                return false;
            }

            // If the shift changed since the last check
            if (currentShift.Id != Settings.LastCheckedShiftId)
            {
                if (OnlineShiftCheck.IsShiftChangeAllowed)
                {
                    // Store the last checked shift
                    Settings.LastCheckedShiftId = currentShift.Id;
                    return true;
                }
                else
                    return true;

            }

            return false;
        }
    }
}
