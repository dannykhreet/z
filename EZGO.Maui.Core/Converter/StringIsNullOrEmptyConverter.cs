using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class StringIsNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            if (bool.TryParse((string)parameter, out bool result))
            {
                if (result)
                {
                    return string.IsNullOrEmpty(stringValue);
                }
                else
                {
                    return !string.IsNullOrEmpty(stringValue);
                }
            }
            else
            {
                return string.IsNullOrEmpty(stringValue);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
