using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class BooleanToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value.ToString(), out bool result))
            {
                var binding = parameter as Binding;
                dynamic viewCell = binding.Source;
                return result ? viewCell?.CardPropertyWidth ?? DeviceSettings.DeviceFormat.PropertyWidth : 0;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
