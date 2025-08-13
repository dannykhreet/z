using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    public static class ShiftServiceHelper
    {
        public static List<ShiftModel> FilterShifts(List<ShiftModel> shifts, DateTime dt)
        {
            return shifts
                .Where(item => dt.IsTimeBetween(item.StartTime, item.EndTime))
                .OrderBy(item => item.Weekday)
                .ThenBy(x => x.StartTime).ToList();
        }

        public static ShiftModel GetShiftModel(DateTime dt, int weekDay, List<ShiftModel> subresult)
        {
            ShiftModel result;
            var areaResult0 = subresult.Where(item => item.AreaId == Settings.WorkAreaId);

            var possibleAreaShift = areaResult0.Where(x => x.DayOfWeek == dt.DayOfWeek).FirstOrDefault();
            if (possibleAreaShift == null || possibleAreaShift.StartTime > dt.TimeOfDay)
            {
                var prevWeekday = weekDay == 0 ? 6 : weekDay - 1;
                var suspectedshift = areaResult0.Where(item => item.Weekday == prevWeekday).FirstOrDefault();
                result = suspectedshift;
            }
            else
                result = possibleAreaShift;

            if (result == null)
            {
                areaResult0 = subresult.Where(item => item.AreaId == null);
                var possibleCompanyShift = areaResult0.Where(x => x.DayOfWeek == dt.DayOfWeek).FirstOrDefault();
                if (possibleCompanyShift == null || possibleCompanyShift.StartTime > dt.TimeOfDay)
                {
                    var prevWeekday = weekDay == 0 ? 6 : weekDay - 1;
                    var suspectedshift = areaResult0.Where(item => item.Weekday == prevWeekday).FirstOrDefault();
                    result = suspectedshift;
                }
                else
                    result = possibleCompanyShift;
            }

            return result;
        }

    }
}
