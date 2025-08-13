using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ScoreTypeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScoreTypeEnum scoretype)
            {
                bool result = true;
                if (bool.TryParse((string)parameter, out bool paramresult))
                {
                    result = paramresult;
                }

                switch (scoretype)
                {
                    case ScoreTypeEnum.Thumbs:
                        return result ? false : true;
                    case ScoreTypeEnum.Score:
                        return result ? true : false;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
