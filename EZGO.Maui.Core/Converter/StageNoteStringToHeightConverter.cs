using System;
using System.Globalization;
using EZGO.Maui.Core.Interfaces.Utils;
using Syncfusion.Maui.ListView;

namespace EZGO.Maui.Core.Converter;

public class StageNoteStringToHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var notes = (value as string);
            var sfList = parameter as SfListView;
            var width = sfList.Width;
            var textMetterService = DependencyService.Get<ITextMeter>();
            double result = 0;
            var fontSize = 14;

            var textSize = textMetterService.MeasureTextSize(notes, width - 20, fontSize, "RobotoRegular").Item2;

            result += textSize;
            result += 20;//padding

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
