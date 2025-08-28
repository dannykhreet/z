using System;
using System.Globalization;
using System.Linq;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ColorToBoolConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color selectedColor)
            {
                if (parameter is Color secondColor)
                {
                    //var color = ResourceHelper.GetApplicationResource<Color>(colorName);
                    //var type = typeof(Color);
                    //var name = type.GetEnumValues();
                    //int index = Array.BinarySearch(name, colorName);
                    //.FirstOrDefault(x => x.ToLowerInvariant() == colorName.ToLowerInvariant());
                    return selectedColor == secondColor ? selectedColor : Colors.Transparent;
                }
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Colors.Transparent;
        }
    }
}
