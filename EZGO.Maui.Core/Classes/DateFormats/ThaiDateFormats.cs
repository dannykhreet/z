using System;
namespace EZGO.Maui.Core.Classes.DateFormats
{
    public class ThaiDateFormats : BaseDateFormats
    {
        private const string dateFormat = "dd/MM/yyyy";
        private const string dateTimeFormat = "dd/MM/yyyy, HH:mm";
        private const string dateTimeFirstFormat = "HH:mm, dd/MM/yyyy";

        protected override string GetDisplayFormat()
        {
            return dateFormat;
        }

        protected override string GetDateTimeDisplayFormat()
        {
            return dateTimeFormat;
        }

        protected override string GetShortDisplayDateFormat()
        {
            return dateFormat;
        }

        protected override string GetFullMonthDisplayDateTimeFormat()
        {
            return dateTimeFormat;
        }

        protected override string GetDayFirstShortMonthDateTimeFormat()
        {
            return dateTimeFormat;
        }

        protected override string GetDayDateFormat()
        {
            return dateFormat;
        }

        protected override string GetShiftDateFormat()
        {
            return dateFormat;
        }

        protected override string GetWeekDateFormat()
        {
            return dateFormat;
        }

        protected override string GetDateTimeMonthShortNameFormat()
        {
            return dateTimeFirstFormat;
        }
    }
}
