using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class IntToGridLengthConverter : IValueConverter
    {
        private int length = 0;
        public int Length
        {
            get => length;
            set
            {
                if (value > 100)
                    length = 100;
                else if (value < 0)
                    length = 0;
                else
                    length = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Length = System.Convert.ToInt32(value);
                if (parameter != null)
                {
                    bool.TryParse((string)parameter, out bool paramresult);
                    if (!paramresult)
                    {
                        Length = (100 - Length);
                    }
                }
            }
            return new GridLength(Length, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
