using System;
using System.Globalization;

namespace EZGO.Maui.Converters
{
    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var returnValue = -1;

            if (value is Enum)
            {
                try
                {
                    returnValue = (int)System.Convert.ChangeType(value, typeof(int));
                }
                catch { }
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValue = default(Enum);
            if (parameter is Type enumType)
            {
                try
                {
                    enumValue = (Enum)Enum.Parse(enumType, value.ToString());
                }
                catch { }
            }
            return enumValue;
        }
    }
}

