using System.Globalization;
using System;
using EZGO.CMS.LIB.Extensions;
using Microsoft.Extensions.Configuration;

namespace WebApp.Logic
{
    /// <summary>
    /// Extension methods for DateTime
    /// This class is currently not in EZGO.CMS.LIB.Extensions because it relies on the AppSettings to determine if it should be active at the moment
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns the DateTime as a string with long date and short time as it is in the given locale.
        /// If the provided locale is not supported or does not exist, server locale will be used.
        /// Examples: Monday, July 1, 2024 5:02 PM (en-US), maandag 1 juli 2024 17:02 (nl-NL)
        /// </summary>
        /// <param name="dateTime">The DateTime to convert to a string</param>
        /// <param name="locale">The locale to translate to</param>
        /// <returns>String with long notation of date and short notation of time in specified locale (or server locale if provided locale is not supported)</returns>
        public static string ToLocaleFullDateShortTimeString(this DateTime dateTime, string locale)
        {
            bool useDateTimeLocaleTranslation = Startup.EzgoConfig.GetSection("AppSettings").GetValue<bool>("EnableDateLocaleTransaltions");
            if (locale.IsNullOrEmpty() || !useDateTimeLocaleTranslation)
            {
                return dateTime.ToString("f");
            }
            CultureInfo cultureInfo = null;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(locale);
            }
            catch (Exception ex)
            {
                return dateTime.ToString("f");
            }
            return dateTime.ToString("f", cultureInfo);
        }

        /// <summary>
        /// Returns the DateTime as a string with long date notation as it is in the given locale.
        /// If the provided locale is not supported or does not exist, server locale will be used.
        /// Examples: Tuesday, June 4, 2024 (en-US), dinsdag 4 juni 2024 (nl-NL)
        /// </summary>
        /// <param name="dateTime">The DateTime to convert to a string</param>
        /// <param name="locale">The locale to translate to</param>
        /// <returns>String with long notation of date in specified locale (or server locale if provided locale is not supported)</returns>
        public static string ToLocaleLongDateString(this DateTime dateTime, string locale)
        {
            bool useDateTimeLocaleTranslation = Startup.EzgoConfig.GetSection("AppSettings").GetValue<bool>("EnableDateLocaleTransaltions");
            if (locale.IsNullOrEmpty() || !useDateTimeLocaleTranslation)
            {
                return dateTime.ToLongDateString();
            }
            CultureInfo cultureInfo = null;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(locale);
            }
            catch (Exception ex)
            {
                return dateTime.ToLongDateString();
            }
            return dateTime.ToString("D", cultureInfo);
        }

        /// <summary>
        /// Returns the DateTime as a string with short date notation as it is in the given locale.
        /// If the provided locale is not supported or does not exist, server locale will be used.
        /// Examples: 6/15/2009 (en-US), 15-9-2009 (nl-NL)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="locale"></param>
        /// <returns>String with short notation of date in specified locale (or server locale if provided locale is not supported)</returns>
        public static string ToLocaleShortDateString(this DateTime dateTime, string locale)
        {
            bool useDateTimeLocaleTranslation = Startup.EzgoConfig.GetSection("AppSettings").GetValue<bool>("EnableDateLocaleTransaltions");
            if (locale.IsNullOrEmpty() || !useDateTimeLocaleTranslation)
            {
                return dateTime.ToShortDateString();
            }
            CultureInfo cultureInfo = null;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(locale);
            }
            catch (Exception ex)
            {
                return dateTime.ToShortDateString();
            }
            return dateTime.ToString("d", cultureInfo);
        }

        /// <summary>
        /// Returns the DateTime as a string with the general short date and short time notation as it is in the given locale.
        /// If the provided locale is not supported or does not exist, server locale will be used.
        /// Examples: 7/2/2024 1:34 PM (en-US), 2-7-2024 13:34 (nl-NL)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="locale"></param>
        /// <returns>String with general short notation of date and time in specified locale (or server locale if provided locale is not supported)</returns>
        public static string ToLocaleGeneralDateShortTimeString(this DateTime dateTime, string locale)
        {
            bool useDateTimeLocaleTranslation = Startup.EzgoConfig.GetSection("AppSettings").GetValue<bool>("EnableDateLocaleTransaltions");
            if (locale.IsNullOrEmpty() || !useDateTimeLocaleTranslation)
            {
                return dateTime.ToString("g");
            }
            CultureInfo cultureInfo = null;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(locale);
            }
            catch (Exception ex)
            {
                return dateTime.ToString("g");
            }
            return dateTime.ToString("g", cultureInfo);
        }

        /// <summary>
        /// Returns the DateTime as a string with the general short date and long time notation as it is in the given locale.
        /// If the provided locale is not supported or does not exist, server locale will be used.
        /// Examples: 7/2/2024 1:34:28 PM (en-US), 2-7-2024 13:34:28 (nl-NL)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="locale"></param>
        /// <returns>String with general short notation of date and long time in specified locale (or server locale if provided locale is not supported)</returns>
        public static string ToLocaleGeneralDateLongTimeString(this DateTime dateTime, string locale)
        {
            bool useDateTimeLocaleTranslation = Startup.EzgoConfig.GetSection("AppSettings").GetValue<bool>("EnableDateLocaleTransaltions");
            if (locale.IsNullOrEmpty() || !useDateTimeLocaleTranslation)
            {
                return dateTime.ToString("G");
            }
            CultureInfo cultureInfo = null;
            try
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(locale);
            }
            catch (Exception ex)
            {
                return dateTime.ToString("G");
            }
            return dateTime.ToString("G", cultureInfo);
        }
    }
}
