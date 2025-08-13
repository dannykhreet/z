using System;
using System.Globalization;
using EZGO.Maui.Core.Models.Actions;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class FilterSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is FilterModel selectedFilter && parameter is Binding binding)
            {
                if(binding.Source is FilterModel currentFilter)
                { 
                    return selectedFilter.Name == currentFilter.Name;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
