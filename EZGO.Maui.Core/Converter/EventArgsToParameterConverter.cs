using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class EventArgsToParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs selectionChanged)
            {
                return selectionChanged.AddedItem;
            }
            else if (value is Syncfusion.Maui.Inputs.SelectionChangedEventArgs selection)
            {
                return selection.AddedItems;
            }
            else if (value is Syncfusion.Maui.Picker.PickerSelectionChangedEventArgs changed)
            {
                return changed.NewValue;
            }
            else if (value is Syncfusion.Maui.Inputs.SelectionChangedEventArgs sel)
            {
                return sel.AddedItems;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
