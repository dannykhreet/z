using EZGO.Maui.Core.Models.Reports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ReportsCountConverter : IValueConverter
    {
        private double percentage = 0;
        public double Percentage
        {
            get => percentage;
            set
            {
                if (value > 100 || value < 0) percentage = 0;
                else percentage = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {                
            Percentage = 0;
            if (value is ReportsCount count)
            {
                switch ((string)parameter)
                {
                    case "All":
                    case "Count":
                        if (count.MaxCountNr != 0)
                        {
                            Percentage = Math.Round(((double)count.CountNr / count.MaxCountNr) * 100, 2);
                        }
                        if ((string)parameter == "All")
                        {
                            Percentage = 100 - Percentage;
                        }
                        break;
                    case "Done":
                    case "NotDone":
                        if (count.CountNr != 0)
                        {
                            Percentage = Math.Round(((double)count.NrDone / count.CountNr) * 100, 2);
                        }
                        if ((string)parameter == "NotDone")
                        {
                            Percentage = 100 - Percentage;
                        }
                        break;
                    case "Ok":
                        if (count.CountNr != 0)
                        {
                            Percentage = Math.Round(((double)count.NrOk / count.CountNr) * 100, 2);
                        }
                        break;
                    case "NotOk":
                        if (count.CountNr != 0)
                        {
                            Percentage = Math.Round(((double)count.NrNotOk / count.CountNr) * 100, 2);
                        }
                        break;
                    case "Skipped":
                        if (count.CountNr != 0)
                        {
                            Percentage = Math.Round(((double)count.NrSkipped / count.CountNr) * 100, 2);
                        }
                        break;
                    case "Todo":
                        if (count.CountNr != 0 && (count.NrNotOk+count.NrOk+count.NrSkipped!=0))
                        {
                            Percentage = Math.Round(((double)count.NrTodo / count.CountNr) * 100, 2);
                        }
                        else
                        {
                            Percentage = 100;
                        }
                        break;
                    case "Started":
                    case "Resolved":

                        if (count.CountNr != 0)
                        {
                            Percentage = Math.Round(((double)count.CountNrResolved / count.CountNr) * 100, 2);
                        }
                        if ((string)parameter == "Started")
                        {
                            Percentage = 100 - Percentage;
                        }
                        break;
                    case "Overdue":
                    case "Unresolved":
                        if (count.CountNrUnresolved != 0)
                        {
                            Percentage = Math.Round(((double)count.CountNrOverdue / count.CountNrUnresolved) * 100, 2);
                        }
                        if ((string)parameter == "Unresolved")
                        {
                            Percentage = 100 - Percentage;
                        }
                        break;
                    default:
                        Percentage = 0;
                        break;
                }
            }
            return new GridLength(Percentage, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new GridLength(0, GridUnitType.Star);
        }
    }
}
