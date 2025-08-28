using System;
using System.Globalization;
using EZGO.Maui.Core.Classes.DateFormats;
using NodaTime;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            string result = null;

            //var datetime = (LocalDateTime)value;

            //var timepattern = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            var format = parameter == null ? BaseDateFormats.DayFirstShortMonthDateTimeFormat : parameter.ToString();

            if (value is LocalDateTime localDate)
            {
                result = localDate.ToString(format, CultureInfo.CurrentUICulture);
            }
            else if (value is DateTime externalDate)
            {
                result = externalDate.ToString(format, CultureInfo.CurrentUICulture);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
