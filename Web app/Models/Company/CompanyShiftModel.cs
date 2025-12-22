using System;

namespace WebApp.Models.Company
{
    public class CompanyShiftModel
    {
        public int Id { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public int Day { get; set; } // 1 t/m 7 -> zondag t/m zaterdag

        public int Weekday { get; set; } // 0 t/m 6 -> maandag t/m zondag

        public int ShiftNr { get; set; }

        public int? AreaId { get; set; }

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
