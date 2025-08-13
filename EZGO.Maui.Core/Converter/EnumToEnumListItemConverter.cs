using EZGO.Maui.Core.Classes;
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    /// <summary>
    /// Converts an Enum value to <see cref="EZGO.Maui.Core.Classes.EnumListItem"/>
    /// </summary>
    public class EnumToEnumListItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Enum))
                return null;

            try
            {
                var instance = Activator.CreateInstance(typeof(EnumListItem<>).MakeGenericType(value.GetType()), value, true);
                return instance;
            }
            catch { }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var valueProp = value.GetType().GetProperty("Value");
                var enumValue = valueProp.GetValue(value);
                return enumValue;
            }
            catch { }

            return null;
        }
    }
}
