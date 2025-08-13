using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ValueToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myvalue = System.Convert.ToInt32(value);

            if (parameter != null && myvalue > 0)
            {
                if (parameter is Label parametervalue)
                {
                    var strTotal = parametervalue.Text;
                    if (strTotal != null)
                    {
                        int.TryParse(strTotal, out int total);
                        if (total > 0)
                        {
                            var result = Math.Round((((double)((double)myvalue / total)) * 100), 2);
                            return new GridLength(result, GridUnitType.Star);
                        }
                    }
                }
            }
            return new GridLength(myvalue, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new GridLength(0, GridUnitType.Star);
        }
    }
}
