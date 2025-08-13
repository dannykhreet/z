using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Audits;
using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter.MultiBinding
{
    public class AuditScoreColorConverter : IMultiValueConverter
    {
        private Color GreyColor => ResourceHelper.GetApplicationResource<Color>("GreyColor");
        private Color WhiteColor => Colors.White;
        private Color RedColor => ResourceHelper.GetApplicationResource<Color>("RedColor");
        private Color GreenColor => ResourceHelper.GetApplicationResource<Color>("GreenColor");
        private Color SkippedColor => ResourceHelper.GetApplicationResource<Color>("SkippedColor");

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var valueInt = (int?)values[0];
                var calculator = (IScoreColorCalculator)values[1];
                var status = values.Length > 2 ? Enum.TryParse(typeof(TaskStatusEnum), values[2]?.ToString(), out var result) ? result : default : default;

                if (valueInt.HasValue && (status == null || (TaskStatusEnum)status == TaskStatusEnum.Todo))
                {
                    return calculator.GetColor(valueInt.Value);
                }
                else
                {
                    return status switch
                    {
                        TaskStatusEnum.NotOk => RedColor,
                        TaskStatusEnum.Ok => GreenColor,
                        TaskStatusEnum.Skipped => SkippedColor,
                        TaskStatusEnum.Todo => WhiteColor,
                        _ => default,
                    };
                }
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
