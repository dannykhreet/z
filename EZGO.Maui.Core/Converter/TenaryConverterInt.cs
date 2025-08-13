using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class TenaryConverterInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ParameterToValues(parameter, out var ifTrue, out var ifFalse);

            return (value is bool valueBool && valueBool == true) ? ifTrue : ifFalse;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ParameterToValues(parameter, out var ifTrue, out var _);

            return (value is int valueInt) && valueInt == ifTrue;

        }

        private void ParameterToValues(object parameter, out int ifTure, out int ifFalse)
        {
            ifTure = ifFalse = default;
            if (parameter == null || parameter is string == false)
                return;

            var paramString = parameter as string;
            var valuesStr = paramString.Split(":");

            if (valuesStr.Length != 2)
                return;


            if (!int.TryParse(valuesStr[0], out var _ifTrue))
                return;

            if (!int.TryParse(valuesStr[1], out var _ifFalse))
                return;

            ifTure = _ifTrue;
            ifFalse = _ifFalse;
        }
    }
}
