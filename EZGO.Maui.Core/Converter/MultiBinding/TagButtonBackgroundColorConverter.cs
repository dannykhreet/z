using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter.MultiBinding
{
    public class TagButtonBackgroundColorConverter : IMultiValueConverter
    {
        private static Color ResourcesBackgroundColor => ResourceHelper.GetApplicationResource<Color>("GreyColor");
        private static Color GreenColor => ResourceHelper.GetApplicationResource<Color>("GreenColor");

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isExpanded = false;
            var isActive = false;

            if (values[0] is bool)
                isExpanded = (bool)values[0];

            if (values[1] is bool)
                isActive = (bool)values[1];

            return isActive ? GreenColor : (isExpanded ? Colors.White : ResourcesBackgroundColor);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
