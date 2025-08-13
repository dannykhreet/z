using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Feed;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class FeedCommentsItemsToHeightConverter : IValueConverter
    {
        public FeedCommentsItemsToHeightConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var stackLayout = parameter as StackLayout;
                var textMetterService = DependencyService.Get<ITextMeter>();
                double result = 0;
                // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                var fontSize = (DeviceInfo.Platform == DevicePlatform.Android) ? 16 : 15;

                var list = (value as IEnumerable<FeedMessageItemModel>).ToList();
                foreach (var item in list)
                {
                    var width = stackLayout.Width - 30 - 60; //margin - first column width
                    var textSize = textMetterService.MeasureTextSize(item.Description, width, fontSize, "RobotoRegular");
                    var userNameSize = textMetterService.MeasureTextSize(item.Username, width, fontSize, "RobotoRegular");
                    var fullTextSize = textSize.Item2 + userNameSize.Item2 + 6; //spacing
                    if (item.IsModified)
                    {
                        result += 15; //padding
                        result += userNameSize.Item2; // modified by username text
                    }
                    result += fullTextSize + 25; //padding + line brake + row spacing
                    if (item.MediaItems.Count > 0)
                        result += 301;
                }

                return result;
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
