using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Maui.Core.Extensions;

namespace EZGO.Maui.Core.Converter.MultiBinding
{
    public class TranslateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || (values[0] == null && values[1] == null))
                return "";

            var translationKey = (string)values[0];
            var propertyName = (string)values[1];

            var translation = Extensions.TranslateExtension.GetValueFromDictionary(translationKey);
            return translation.IsNullOrEmpty() ? propertyName : translation;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}