using EZGO.Maui.Core.Models.Shifts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Shifts
{
    public interface IShiftService : IDisposable
    {
        Task<List<ShiftModel>> GetShiftsAsync(bool refresh = false, bool isFromSyncService = false);

        Task<ShiftModel> GetCurrentShiftAsync(bool refresh = false);
        Task<ShiftModel> GetCurrentShiftOrNullAsync(bool refresh = false);
    }
}
