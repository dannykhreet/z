using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// RecurrencyTypeEnum; RecurrencyType is the type of recurrency (DB: tasks_taskrecurrency.type).
    /// When using for submitting to the EZGO Api, always use the value (int) for posting.
    /// </summary>
    public enum RecurrencyTypeEnum
    {
        NoRecurrency = 0,
        Week = 1,
        Month = 2,
        Shifts = 3,
        PeriodDay = 4,
        PeriodMinute = 5,
        PeriodHour = 6,
        DynamicDay = 7,
        DynamicHour = 8,
        DynamicMinute = 9

    }
}
