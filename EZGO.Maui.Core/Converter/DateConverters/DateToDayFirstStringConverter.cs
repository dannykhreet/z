using System;
using System.Globalization;
using EZGO.Maui.Core.Classes.DateFormats;
using NodaTime;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class DateToDayFirstStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (LocalDateTime)value;
            return val.ToString(BaseDateFormats.DayFirstShortMonthDateTimeFormat, CultureInfo.CurrentUICulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

