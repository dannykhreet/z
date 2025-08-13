using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Stages;

namespace EZGO.Maui.Core.Converter;

public class StagesToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isHeaderColor = false;
        Color result = ResourceHelper.GetApplicationResource<Color>("BackgroundColor");
        var stage = value as StageTemplateModel;

        bool.TryParse((string)parameter, out isHeaderColor);

        if (stage == null || stage.Id == -1)
            return result;

        if (stage.IsSigned)
        {
            if (isHeaderColor)
                result = ResourceHelper.GetApplicationResource<Color>("GreenColor");
            else
                result = ResourceHelper.GetApplicationResource<Color>("LightGreenColor");
        }
        else if (stage.IsLocked)
        {
            if (isHeaderColor)
                result = ResourceHelper.GetApplicationResource<Color>("DarkerGreyColor");
            else
                result = ResourceHelper.GetApplicationResource<Color>("LightGreyColor");
        }
        else
        {
            if (isHeaderColor)
                result = ResourceHelper.GetApplicationResource<Color>("DarkBlueColor");
            else
                result = ResourceHelper.GetApplicationResource<Color>("LightBlueColor");
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
