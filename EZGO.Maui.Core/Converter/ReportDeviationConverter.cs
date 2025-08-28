using EZGO.Maui.Core.Models.Reports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ReportDeviationConverter : IValueConverter
    {
        private double percentage = 0;
        public double Percentage
        {
            get => percentage;
            set
            {
                if (value > 100)
                    percentage = 100;
                else if (value < 0)
                    percentage = 0;
                else
                    percentage = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReportDeviationItemModel item)
            {
                switch ((string)parameter)
                {
                    case "Max":
                    case "Score":
                        if (item.MaxPercentage != 0)
                        {
                            Percentage = Math.Round(((double)item.Percentage / item.MaxPercentage) * 100, 2);
                        }
                        if ((string)parameter == "Max")
                        {
                            Percentage = 100 - Percentage;
                        }
                        break;
                    case "MaxCount":
                    case "ScoreCount":
                        if (item.MaxPercentage != 0)
                        {
                            Percentage = Math.Round(((double)item.CountNr / item.MaxPercentage) * 100, 2);
                        }
                        if ((string)parameter == "Max")
                        {
                            Percentage = 100 - Percentage;
                        }
                        break;
                }
                return new GridLength(Percentage, GridUnitType.Star);
            }
            else if (value is ReportAuditDeviationItemModel item2)
            {
                switch ((string)parameter)
                {
                    case "Max":
                    case "Score":
                        if (item2.MaxDeviationScore != 0)
                        {
                            Percentage = Math.Round(((double)item2.DeviationScore / item2.MaxDeviationScore) * 100, 2);
                        }
                        if ((string)parameter == "Max")
                        {
                            Percentage = 100 - Percentage;
                        }
                        break;
                }
                return new GridLength(Percentage, GridUnitType.Star);
            }
            return new GridLength(Percentage, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
