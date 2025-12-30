using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// RetrievalTypeEnum; 
    /// </summary>
    public enum RetrievalTypeEnum
    {
        ThisMonth,
        ThisWeek,
        ThisQuarter,
        ThisYear,
        Today,
        OverDue,
        ShiftType,
        MonthType,
        WeekType,
        OneTimeOnlyType,
        DailyDynamicType,
        DailyPeriodType,
        Default,
        ThisShift,
        ByWeekNr,
        ByMonthNr,
        ByYearNr,
        ByDayOfYearNr,
        ByDayOfMonthNr

    }
}
