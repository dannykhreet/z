using System;
using Syncfusion.Maui.ListView;
using System.Globalization;

namespace EZGO.Maui.Converters
{
    public class BoolToLoadMoreOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueBool && valueBool == true)
            {
                return parameter ?? LoadMoreOption.Manual;
            }
            return LoadMoreOption.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LoadMoreOption valueOption)
            {
                if (parameter is LoadMoreOption paramValueOption)
                {
                    return valueOption == paramValueOption;
                }

            }

            return false;
        }
    }
}

