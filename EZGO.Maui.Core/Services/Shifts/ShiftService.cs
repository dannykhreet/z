using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Shifts;
using EZGO.Maui.Core.Models.Shifts;

namespace EZGO.Maui.Core.Services.Shifts
{
    public class ShiftService : IShiftService
    {
        private readonly IApiRequestHandler _apiRequestHandler;

        public ShiftService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
        }

        public async Task<List<ShiftModel>> GetShiftsAsync(bool refresh = false, bool isFromSyncService = false)
        {
            const string uri = "shifts";
            List<ShiftModel> result = await _apiRequestHandler.HandleListRequest<ShiftModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            return result;
        }

        public async Task<ShiftModel> GetCurrentShiftAsync(bool refresh = false)
        {
            return await GetCurrentShiftInternalAsync(refresh).ConfigureAwait(false) ?? new ShiftModel();
        }

        public async Task<ShiftModel> GetCurrentShiftOrNullAsync(bool refresh = false)
        {
            return await GetCurrentShiftInternalAsync(refresh).ConfigureAwait(false);
        }

        private async Task<ShiftModel> GetCurrentShiftInternalAsync(bool refresh)
        {
            List<ShiftModel> shifts = await GetShiftsAsync(refresh).ConfigureAwait(false);

            DateTime dt = DateTime.Now;
            int weekDay = GetWeekDay(dt);
            List<ShiftModel> subresult = ShiftServiceHelper.FilterShifts(shifts, dt);

            // fri 22:00 - sat 6:00
            // sun 22:00 - mon 6:00
            // sun 2:00

            ShiftModel result = ShiftServiceHelper.GetShiftModel(dt, weekDay, subresult);

            return result;
        }

        private int GetWeekDay(DateTime dt)
        {
            int weekDay = 0;

            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    weekDay = 0;
                    break;
                case DayOfWeek.Tuesday:
                    weekDay = 1;
                    break;
                case DayOfWeek.Wednesday:
                    weekDay = 2;
                    break;
                case DayOfWeek.Thursday:
                    weekDay = 3;
                    break;
                case DayOfWeek.Friday:
                    weekDay = 4;
                    break;
                case DayOfWeek.Saturday:
                    weekDay = 5;
                    break;
                case DayOfWeek.Sunday:
                    weekDay = 6;
                    break;
            }

            return weekDay;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }
    }
}
