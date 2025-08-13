using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class NullableIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int valueInt)
            {
                return valueInt.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string valueStr)
            {
                if (string.IsNullOrEmpty(valueStr))
                    return null;

                if (int.TryParse(valueStr, out int result))
                    return (int?)result;

                return null;
            }
            return null;
        }
    }
}
