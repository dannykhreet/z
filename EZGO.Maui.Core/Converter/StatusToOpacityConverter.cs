using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class StatusToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskStatusEnum taskstatus)
            {
                switch (taskstatus)
                {
                    case TaskStatusEnum.NotOk:
                        return 0.1;
                    case TaskStatusEnum.Ok:
                        return 0.1;
                    case TaskStatusEnum.Skipped:
                        return 0.1;
                    default:
                        return 0.15;
                }
            }
            return 0.15;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1.0;
        }
    }
}
