using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NodaTime;

namespace EZGO.Maui.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="DateTime"/>
    /// </summary>
    public static class DateTimeExtensionMethods
    {
        /// <summary>
        /// Gets the week number in a year
        /// </summary>
        /// <param name="time">The time to get the week number from</param>
        /// <returns>Number of the week in a year</returns>
        public static int GetWeekNumber(this DateTime time)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }
    }
}
