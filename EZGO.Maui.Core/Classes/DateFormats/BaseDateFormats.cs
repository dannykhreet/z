using System;
using System.Globalization;

namespace EZGO.Maui.Core.Classes.DateFormats
{
    public abstract class BaseDateFormats
    {
        public static string DisplayDateFormat => Instance().GetDisplayFormat();

        public static string DisplayDateTimeFormat => Instance().GetDateTimeDisplayFormat();

        public static string ShortDisplayDateFormat => Instance().GetShortDisplayDateFormat();

        public static string FullMonthDisplayDateTimeFormat => Instance().GetFullMonthDisplayDateTimeFormat();

        public static string DayFirstShortMonthDateTimeFormat => Instance().GetDayFirstShortMonthDateTimeFormat();

        public static string ShiftDateFormat => Instance().GetShiftDateFormat();

        public static string DayDateFormat => Instance().GetDayDateFormat();

        public static string WeekDateFormat => Instance().GetWeekDateFormat();

        public static string DateTimeMonthShortNameFormat => Instance().GetDateTimeMonthShortNameFormat();

        protected abstract string GetDayDateFormat();
        protected abstract string GetShiftDateFormat();
        protected abstract string GetWeekDateFormat();
        protected abstract string GetDayFirstShortMonthDateTimeFormat();
        protected abstract string GetFullMonthDisplayDateTimeFormat();
        protected abstract string GetShortDisplayDateFormat();
        protected abstract string GetDateTimeDisplayFormat();
        protected abstract string GetDisplayFormat();
        protected abstract string GetDateTimeMonthShortNameFormat();

        private static BaseDateFormats baseDateFormats;

        public static BaseDateFormats Instance()
        {
            if(baseDateFormats == null)
            {
                if (CultureInfo.CurrentCulture.Calendar.GetType() == typeof(ThaiBuddhistCalendar))
                    return new ThaiDateFormats();
                else
                    return new StandardDateFormats();
            }

            return baseDateFormats;
        }

        protected BaseDateFormats()
        {
        }
    }
}
