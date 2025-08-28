using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class FlexLayoutDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Settings.IsRightToLeftLanguage)
                return FlexDirection.RowReverse;
            return FlexDirection.Row;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
