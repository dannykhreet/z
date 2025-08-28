using System;
using System.Globalization;
using Syncfusion.Maui.ListView;

namespace EZGO.Maui.Core.Converter;

public class IncompletedChecklistLinearIndexToColorConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var listview = parameter as SfListView;
            var index = listview.DataSource.DisplayItems.IndexOf(value);

            if (index % 2 == 0)
                return Color.FromArgb("edfcf2");
            return Color.FromArgb("ffffff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
}
