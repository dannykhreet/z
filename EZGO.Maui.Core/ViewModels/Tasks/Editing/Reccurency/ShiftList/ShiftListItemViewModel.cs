using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Shifts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace EZGO.Maui.Core.ViewModels.Tasks
{
    public class ShiftListItemViewModel : NotifyPropertyChanged
    {
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsChecked { get; set; }

        public int Id { get; set; }

        public ShiftListItemViewModel(ShiftModel shift)
        {
            Id = shift.Id;
            StartTime = shift.StartTime;
            EndTime = shift.EndTime;
        }
    }
}
