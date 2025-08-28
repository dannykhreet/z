using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DateFormats;
using NodaTime;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class DateTimeToGregorianDateStringConverter : IValueConverter
    {
        public DateTimeToGregorianDateStringConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var objVal = (DateTime)value;

            LocalDateTime localDateTime = Settings.ConvertDateTimeToLocal(objVal);

            //"M dd','yyyy"
            var result = localDateTime.ToString(BaseDateFormats.ShortDisplayDateFormat, CultureInfo.CurrentUICulture);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
