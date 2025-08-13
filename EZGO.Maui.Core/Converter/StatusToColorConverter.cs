using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class StatusToColorConverter : IValueConverter
    {
        private Color GreyColor => ResourceHelper.GetApplicationResource<Color>("GreyColor");
        private Color RedColor => ResourceHelper.GetApplicationResource<Color>("RedColor");
        private Color GreenColor => ResourceHelper.GetApplicationResource<Color>("GreenColor");
        private Color SkippedColor => ResourceHelper.GetApplicationResource<Color>("SkippedColor");
        private Color OrangeColor => ResourceHelper.GetApplicationResource<Color>("OrangeColor");
        private Color PropertiesDefaultColor => ResourceHelper.GetApplicationResource<Color>("PropertiesDefaultColor");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskStatusEnum taskstatus)
            {
                switch (taskstatus)
                {
                    case TaskStatusEnum.NotOk:
                        return RedColor;
                    case TaskStatusEnum.Ok:
                        return GreenColor;
                    case TaskStatusEnum.Skipped:
                        return SkippedColor;
                    default:
                        return GreyColor;
                }
            }
            else if (value is ActionStatusEnum actionstatus)
            {
                switch (actionstatus)
                {
                    case ActionStatusEnum.Solved:
                        return GreenColor;
                    case ActionStatusEnum.PastDue:
                        return RedColor;
                    case ActionStatusEnum.Unsolved:
                        return GreyColor;
                }
            }
            else if (value is MachineStatusEnum status)
            {
                switch (status)
                {
                    case MachineStatusEnum.stopped:
                        return RedColor;
                    case MachineStatusEnum.running:
                        return GreenColor;
                    default:
                        return GreyColor;
                }
            }
            else if (value is PropertyDisplayTypeEnum displayType)
            {
                switch (displayType)
                {
                    case PropertyDisplayTypeEnum.RectangularGreen:
                    case PropertyDisplayTypeEnum.CircleGreen:
                    case PropertyDisplayTypeEnum.RoundedRectangularGreen:
                        return GreenColor;
                    case PropertyDisplayTypeEnum.RectangularRed:
                    case PropertyDisplayTypeEnum.CircleRed:
                    case PropertyDisplayTypeEnum.RoundedRectangularRed:
                        return RedColor;
                    case PropertyDisplayTypeEnum.RectangularOrange:
                    //case PropertyDisplayTypeEnum.CircleOrange:
                    case PropertyDisplayTypeEnum.RoundedRectangularOrange:
                        return OrangeColor;
                    //case PropertyDisplayTypeEnum.RectangularSkipped:
                    //case PropertyDisplayTypeEnum.CircleSkipped:
                    //case PropertyDisplayTypeEnum.RoundedRectangularSkipped:
                    //    return SkippedColor;
                    default:
                        return PropertiesDefaultColor;
                }
            }
            return GreyColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
