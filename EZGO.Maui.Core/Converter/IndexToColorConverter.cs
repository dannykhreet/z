using System;
using System.Globalization;
using EZGO.Maui.Core.Models.Tags;
using Syncfusion.Maui.ListView;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class IndexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var listview = parameter as SfListView;
            var index = listview.DataSource.DisplayItems.IndexOf(value);
            var item = value as TagModel;
            var lumDelta = item.IsSystemTag ? -0.5f : 0;

            if (index % 2 == 0)
                return Color.FromArgb("e2e0e0").AddLuminosity(lumDelta);
            return Color.FromArgb("e7e6e6").AddLuminosity(lumDelta);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
