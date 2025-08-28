using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class IntToGridProportionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int valueInt)
                return new GridLength(valueInt, GridUnitType.Star);
            else if (value != null && value is string valueStr)
            {
                if (int.TryParse(valueStr, out var number))
                {
                    return new GridLength(number, GridUnitType.Star);
                }
            }

            return new GridLength(0, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
            {
                return gridLength.Value.ToString();
            }
            return null;
        }
    }
}
