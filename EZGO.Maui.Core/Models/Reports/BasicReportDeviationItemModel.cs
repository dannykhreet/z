using EZGO.Maui.Core.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Reports
{
    public class BasicReportDeviationItemModel : NotifyPropertyChanged
    {
        public double Percentage { get; set; }
        public int Id { get; set; }
        public int CountNr { get; set; }
        public int ParentTemplateId { get; set; }
        public string Name { get; set; }

        public int ActionCount { get; set; }
        public int ActionDoneCount { get; set; }

        public double MaxPercentage { get; set; }
        public double CalculatedPercentage { get; set; }
        public int PercentageActionDone { get; set; }

        private string displayAmount;
        public string DisplayAmount
        {
            get => displayAmount;
            set
            {
                displayAmount = value;

                OnPropertyChanged();
            }
        }
    }
}
