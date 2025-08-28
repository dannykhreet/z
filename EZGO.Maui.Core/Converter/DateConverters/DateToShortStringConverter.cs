using System;
using System.Globalization;
using EZGO.Maui.Core.Classes.DateFormats;
using NodaTime;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Core.Converter
{
    public class DateToShortStringConverter : IValueConverter
    {
        public DateToShortStringConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            if (value != null)
            {
                LocalDateTime dateTime;

                if (value is DateTime dt)
                    dateTime = Settings.ConvertDateTimeToLocal(dt.ToLocalTime());
                else
                    dateTime = (LocalDateTime)value;

                result = dateTime.ToString(BaseDateFormats.DisplayDateTimeFormat, CultureInfo.CurrentUICulture);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
