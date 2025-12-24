using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// TimespanTypeEnum; TimespanEnum contains timespans based on days.
    /// With the exception of Today and ThisYear must be dynamically converted.
    /// </summary>
    public enum TimespanTypeEnum
    {
        LastDay = 1,
        LastTwelveDays = 12,
        LastSevenDays = 7,
        LastThirtyDays = 30,
        LastTwelveWeeks = 84,
        LastTwelveMonths = 366,
        LastYear = 365,
        ThisYear = 9999
    }

}
