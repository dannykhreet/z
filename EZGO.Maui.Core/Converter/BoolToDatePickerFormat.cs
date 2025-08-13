using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class BoolToDatePickerFormat : IValueConverter
    {
        // This is a work-around that allows to clear date picker input field when there's no date to display
        // Reason: date picker doesn't support nullable dates, so it always displays something in the text input
        // When the value if is true returns new line to trick the ToString method to display nothing

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool valueBool && valueBool == true) ? "\r\n" : parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string valueStr && valueStr == "\r\n";

        }   
    }
}
