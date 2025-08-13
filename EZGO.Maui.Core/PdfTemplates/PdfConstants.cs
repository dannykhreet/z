using EZGO.Maui.Core.Classes.DateFormats;

namespace EZGO.Maui.Core.PdfTemplates
{
    public static class PdfConstants
    {
        public static string GetDateTimeFormat()
        {
            return BaseDateFormats.FullMonthDisplayDateTimeFormat;
        }

        public static string GetDateFormat()
        {
            return BaseDateFormats.DisplayDateFormat;
        }
    }
}
