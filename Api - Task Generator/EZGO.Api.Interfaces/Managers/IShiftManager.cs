using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IShiftManager, Interface for use with the ShiftManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IShiftManager
    {
        Task<List<Shift>> GetShiftsAsync(int companyId, ShiftFilters? filters = null);
        Task<Shift> GetShiftAsync(int companyId, int shiftId);
        Task<int> AddShiftAsync(int companyId, int userId, Shift shift);
        Task<bool> ChangeShiftAsync(int companyId, int userId, int shiftId, Shift shift);
        Task<bool> SetShiftActiveAsync(int companyId, int userId, int shiftId, bool isActive = true);
        Task<Shift> GetShiftByTimestampAsync(int companyId, DateTime? timestamp);
        List<Exception> GetPossibleExceptions();
        Task<ShiftTimestamps> GetShiftTimestampsByOffsetAsync(int companyId, DateTime? timestamp, int shiftOffset);
        Task<ShiftDayWeekTimestamps> GetShiftDayWeekTimesByTimestamp(int companyId, DateTime? timestamp);
    }
}
