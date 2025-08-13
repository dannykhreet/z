using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Tags;
using Syncfusion.Maui.ListView;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter.MultiBinding
{
    public class TagTableBackgroundColorConverter : IMultiValueConverter
    {
        private Color BackgroundColor => ResourceHelper.GetApplicationResource<Color>("BackgroundColor");

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var isExpanded = (bool)values[2];
                if (!isExpanded)
                    return BackgroundColor;

                var isHeader = (bool)values[3];
                var listview = values[0] as SfListView;
                var tagModel = values[1] as TagModel;
                var index = listview.DataSource.DisplayItems.IndexOf(tagModel);
                var lumDelta = isHeader ? -0.02f : 0;

                if (index % 2 == 0)
                    return Color.FromArgb("e2e0e0").AddLuminosity(lumDelta);
                return Color.FromArgb("e7e6e6").AddLuminosity(lumDelta);
            }
            catch (Exception e)
            {
            }
            return default(Color);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
