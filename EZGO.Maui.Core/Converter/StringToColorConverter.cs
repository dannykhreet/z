using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class StringToColorConverter : IValueConverter
    {
        private static Color ResourcesBackgroundColor => ResourceHelper.GetApplicationResource<Color>("GreyColor");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var defaultColor = ResourcesBackgroundColor;
            var stringValue = value as string;
            if (stringValue.IsNullOrEmpty())
                return defaultColor;

            return Color.FromArgb(stringValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
