using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter.TaskProperties
{
    /// <summary>
    /// Converts <see cref="PropertyValueTypeEnum"/> to corresponding number of decimal places available
    /// </summary>
    public class NumericPropertyValueTypeToDecimalPlacesConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PropertyValueTypeEnum valueEnum)
            {
                switch (valueEnum)
                {
                    case PropertyValueTypeEnum.Integer:
                        return 0;
                    case PropertyValueTypeEnum.Decimal:
                        return 2;
                    default:
                        return 0;
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
