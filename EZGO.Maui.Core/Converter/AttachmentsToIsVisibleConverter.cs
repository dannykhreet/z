using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using EZGO.Maui.Core.Classes;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class AttachmentsToIsVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<MediaItem> mediaItems)
            {
                var anyMediaItems = mediaItems.Any(x => !x.IsEmpty);
                if (anyMediaItems)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

