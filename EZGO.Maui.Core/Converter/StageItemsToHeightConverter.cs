using System;
using System.Globalization;
using EZGO.Maui.Core.Models.Stages;

namespace EZGO.Maui.Core.Converter;

public class StageItemsToHeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isGrid;
        bool.TryParse((string)parameter, out isGrid);
        int result = 200;

        StageTemplateModel stageTemplateModel = value as StageTemplateModel;
        if (stageTemplateModel == null)
            return result;

        var count = stageTemplateModel.FilteredTaskTemplates.Count;

        if (isGrid)
        {
            if (count < 0) return 200;
            int rows = (int)Math.Ceiling(count / 3.0);
            result = (rows * 210) + 55; // (rows * (item size + 2xItemSpacing)) + margin
            if (stageTemplateModel.HasTags)
                result += 50;

            return result;
        }

        if (count < 0) return 120;

        result = (count * 120) + 80; // (rows * (item size + 2xItemSpacing)) + margin
        if (stageTemplateModel.HasTags)
            result += 50;

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
