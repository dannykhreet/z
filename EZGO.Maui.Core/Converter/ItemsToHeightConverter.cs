using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZGO.Maui.Core.Models.Assessments;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ItemsToHeightConverter : IValueConverter
    {
        public ItemsToHeightConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var list = (value as IEnumerable<object>).ToList();
                int.TryParse(parameter as string, out int result);
                return list.Count * result;
            }
            catch
            {
                return 250;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
