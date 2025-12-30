using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    /// <summary>
    /// DateTimeExtensions; Extensions to convert datetimes/timestamps to certain formats.
    /// </summary>
    public static class DateTimeExtensions
    {
        public static string ToTimeDisplay(this DateTime dt)
        {
            return dt.ToString("HH:mm");
        }

        public static string ToTimeWithSecondsDisplay(this DateTime dt)
        {
            return dt.ToString("HH:mm");
        }

        public static string ToTimeDisplay(this TimeSpan ts)
        {
            return ts.ToString("hh:mm");
        }

        public static string ToTimeWithSecondsDisplay(this TimeSpan ts)
        {
            return ts.ToString("hh:mm:ss");
        }

        public static string ToDateDisplay(this DateTime dt)
        {
            return dt.ToString("dd-MM-yyyy");
        }
    }
}
