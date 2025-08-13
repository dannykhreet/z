using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Utils;
using NodaTime;
using NodaTime.TimeZones;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Extensions
{
    public struct DateTimeHelper
    {

        public static LocalDateTime UtcNow => Settings.ConvertDateTimeToLocal(DateTime.UtcNow);

        public static LocalDateTime Now => Settings.ConvertDateTimeToLocal(DateTime.Now);

        public static LocalDateTime MinValue => Settings.ConvertDateTimeToLocal(DateTime.MinValue);

        public static LocalDateTime MaxValue => Settings.ConvertDateTimeToLocal(DateTime.MaxValue);

        public static LocalDateTime Today => Settings.ConvertDateTimeToLocal(DateTime.Today);

        public static LocalDateTime FromFileTimeUtc(long fileTime)
        {
            return LocalDateTime.FromDateTime(DateTime.FromFileTimeUtc(fileTime));
        }
    }

    public static class LocalDateTimeExtensions
    {
        public static long ToFileTime(this LocalDateTime localDateTime)
        {
            return localDateTime.ToDateTimeUnspecified().ToFileTimeUtc(); ;
        }

        public static long ToFileTimeUtc(this LocalDateTime localDateTime)
        {
            return localDateTime.ToDateTimeUnspecified().ToFileTimeUtc(); ;
        }

        public static LocalDateTime AddDays(this LocalDateTime localDateTime, int value)
        {
            return localDateTime.PlusDays(value);
        }

        public static LocalDateTime AddMinutes(this LocalDate localDateTime, long value)
        {
            var date = localDateTime.Plus(Period.FromMinutes(value));
            return Settings.ConvertDateTimeToLocal(date.ToDateTimeUnspecified());
        }

        public static LocalDateTime AddSeconds(this LocalDateTime localDateTime, long value)
        {
            return localDateTime.PlusSeconds(value);
        }

        public static LocalDateTime AddDays(this LocalDate localDateTime, double value)
        {
            var date = localDateTime.ToDateTimeUnspecified();
            date = date.AddDays(value);
            return Settings.ConvertDateTimeToLocal(date);
        }

        public static LocalDateTime AddMonths(this LocalDate localDateTime, int value)
        {
            var date = localDateTime.ToDateTimeUnspecified();
            date = date.AddMonths(value);
            return Settings.ConvertDateTimeToLocal(date);
        }

        public static LocalDateTime AddMinutes(this LocalDate localDateTime, double value)
        {
            var date = localDateTime.ToDateTimeUnspecified();
            date = date.AddMinutes(value);
            return Settings.ConvertDateTimeToLocal(date);
        }

        public static LocalDateTime AddSeconds(this LocalDate localDateTime, double value)
        {
            var date = localDateTime.ToDateTimeUnspecified();
            date = date.AddSeconds(value);
            return Settings.ConvertDateTimeToLocal(date);
        }
    }
}
