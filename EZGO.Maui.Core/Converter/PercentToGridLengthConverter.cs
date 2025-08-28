using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class PercentToGridLengthConverter : IValueConverter
    {
        private double percentage = 0;
        public double Percentage
        {
            get => percentage;
            set
            {
                if (value > 100)
                    percentage = 100;
                else if (value < 0)
                    percentage = 0;
                else
                    percentage = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Percentage = 0;
            if (value != null)
            {
                try
                {
                    Percentage = System.Convert.ToDouble(value);
                }
                catch { Percentage = 0; }
                if (parameter != null)
                {
                    bool.TryParse((string)parameter, out bool paramresult);
                    if (!paramresult)
                    {
                        Percentage = (100 - Percentage);
                    }
                }

            }
            return new GridLength(Percentage, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
