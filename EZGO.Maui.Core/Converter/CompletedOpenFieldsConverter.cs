using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class CompletedOpenFieldsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var obj = value as Tuple<double, double>;
            var convert = bool.Parse(parameter.ToString());
            if (obj != null)
            {
                if (convert)
                {
                    var divider = (obj.Item2 % 2 == 0 ? obj.Item2 : obj.Item2 + 1) / 2;
                    if (divider == 0)
                        return obj.Item1;

                    var itemHeight = obj.Item1 / divider;
                    return itemHeight * obj.Item2;
                }
                else
                {
                    return obj.Item1;
                }
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
