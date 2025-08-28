using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class IsVisibleToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value?.ToString(), out bool res))
            {
                if (res)
                {
                    var paramWidth = double.Parse(parameter?.ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    return new GridLength(paramWidth, GridUnitType.Star);
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

