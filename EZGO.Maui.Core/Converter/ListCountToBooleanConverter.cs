using System;
using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ListCountToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;

            if (parameter != null && parameter is string countString && int.TryParse(countString, out int countParameter))
                count = countParameter;

            if (value != null && value is IList list)
                return list.Count > count;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
