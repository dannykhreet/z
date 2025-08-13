using System;
using System.Globalization;
using EZGO.Maui.Core.Models.Stages;

namespace EZGO.Maui.Core.Converter;

public class StagesToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = "signature";
        var stage = value as StageTemplateModel;

        if (stage == null)
            return result;

        if (stage.IsSigned)
        {
            result = "signature";
        }
        else if (stage.IsLocked)
        {
            result = "lock";
        }
        else
        {
            result = "lightbulb-on";
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
