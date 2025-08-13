using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ScoreToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                int.TryParse(value.ToString(), out int result);
                if (result <= 33)
                {
                    if (result == 0)
                        return ResourceHelper.GetApplicationResource<Color>("GreyColor");
                    return ResourceHelper.GetApplicationResource<Color>("RedColor");
                }
                else if (result <= 66)
                {
                    return ResourceHelper.GetApplicationResource<Color>("SkippedColor");
                }
                else
                {
                    return ResourceHelper.GetApplicationResource<Color>("GreenColor");
                }
            }
            return ResourceHelper.GetApplicationResource<Color>("GreyColor");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
