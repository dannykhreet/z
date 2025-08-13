using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class StaticResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var someValue = (string)value;
            if (someValue != null && someValue == "DotsinCircle")
            {
                return Application.Current.Resources["CloseCross"];
            }
            return new object(); //TODO
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Application.Current.Resources["DotsinCircle"];
        }
    }
}
