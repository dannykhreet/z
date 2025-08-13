using System;
using System.Diagnostics;
using System.Globalization;
using EZGO.Maui.Core.Classes.DateFormats;
using NodaTime;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            if (value != null)
            {
                LocalDateTime dateTime = ((LocalDateTime)value);

                result = dateTime.ToString(BaseDateFormats.DayFirstShortMonthDateTimeFormat, CultureInfo.CurrentUICulture);
                //result = dateTime.ToString("MMM' 'dd', 'yyyy', '", null) + dateTime.ToString(shortTimePattern, null);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
