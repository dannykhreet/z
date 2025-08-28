using System;
namespace EZGO.Maui.Core.Classes.DateFormats
{
    public class StandardDateFormats : BaseDateFormats
    {
        private const string monthShortNameDateFormat = "MMM dd, yyyy";
        private const string monthShortNameDateTimeFormat = "MMM dd, yyyy, HH:mm";
        private const string monthFullNameDateFormat = "MMMM d, yyyy";
        private const string monthFullNameDateTimeFormat = "MMMM d, yyyy, HH:mm";
        private const string dayFirstShortMonthDateTimeFormat = "d MMM yyyy, HH:mm";
        private const string shiftDateFormat = "dddd, dd MMMM yyyy";
        private const string weekDateFormat = "MMM dd";
        private const string dateTimeMonthShortNameFormat = "HH:mm, MMM dd, yyyy";

        public StandardDateFormats()
        {
        }

        protected override string GetDateTimeDisplayFormat()
        {
            return monthShortNameDateTimeFormat;
        }

        protected override string GetDayDateFormat()
        {
            return shiftDateFormat;
        }

        protected override string GetDayFirstShortMonthDateTimeFormat()
        {
            return dayFirstShortMonthDateTimeFormat;
        }

        protected override string GetDisplayFormat()
        {
            return monthFullNameDateFormat;
        }

        protected override string GetFullMonthDisplayDateTimeFormat()
        {
            return monthFullNameDateTimeFormat;
        }

        protected override string GetShiftDateFormat()
        {
            return shiftDateFormat;
        }

        protected override string GetShortDisplayDateFormat()
        {
            return monthShortNameDateFormat;
        }

        protected override string GetWeekDateFormat()
        {
            return weekDateFormat;
        }

        protected override string GetDateTimeMonthShortNameFormat()
        {
            return dateTimeMonthShortNameFormat;
        }
    }
}
