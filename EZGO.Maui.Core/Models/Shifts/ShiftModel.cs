using EZGO.Api.Models;
using System;

namespace EZGO.Maui.Core.Models.Shifts
{
    public class ShiftModel : Shift
    {
        public TimeSpan StartTime => GetTimeSpan(Start);

        public TimeSpan EndTime => GetTimeSpan(End);

        private static TimeSpan GetTimeSpan(string time)
        {
            TimeSpan result = TimeSpan.Zero;

            if (TimeSpan.TryParse(time, out TimeSpan timeSpan))
                result = timeSpan;

            return result;
        }

        public DayOfWeek DayOfWeek => (DayOfWeek)(Weekday + 1 < 7 ? Weekday + 1 : 0);

        /// <summary>
        /// Gets whether or not this shift is an overnight shift, meaning if starts on one day and ends on the other one.
        /// </summary>
        public bool IsOvernight => StartTime > EndTime;
    }
}
