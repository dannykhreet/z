using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Maui.Controls.Lists;

namespace EZGO.Maui.Converters
{
    public class PullToRefreshListLinearIndextToColorConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var listview = parameter as dynamic;
            var index = listview.List.DataSource.DisplayItems.IndexOf(value);

            if (index % 2 == 0)
                return Color.FromArgb("edfcf2");
            return Color.FromArgb("ffffff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}