using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Shifts;
using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.ViewModels.Tasks
{
    public class ShiftListViewModel : NotifyPropertyChanged
    {
        public DayOfWeek WeekDay { get; set; }

        public string DisplayWeekDay => WeekDay.Translate();

        public List<ShiftListItemViewModel> Items { get; set; }

        private bool? _SelectAll;
        public bool? SelectAll
        {
            get => _SelectAll;
            set 
            {
                _SelectAll = value;

                if (value.HasValue)
                    Items.ForEach(x => x.IsChecked = value.Value);
            }
        }

        public ShiftListViewModel(DayOfWeek day, List<ShiftListItemViewModel> items)
        {
            WeekDay = day;
            Items = items;
            Items.ForEach(x => x.PropertyChanged += Item_PropertyChanged);
            // Call the property changed just to update the SelectAll flag
            Item_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(ShiftListItemViewModel.IsChecked)));
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShiftListItemViewModel.IsChecked))
            {
                if (Items.All(x => x.IsChecked))
                    _SelectAll = true;
                else if (Items.All(x => !x.IsChecked))
                    _SelectAll = false;
                else 
                    _SelectAll = null;
                OnPropertyChanged(nameof(SelectAll));
            }
        }

        public static List<ShiftListViewModel> FromShifts(IEnumerable<ShiftModel> shifts)
        {
            if (shifts == null)
                return new List<ShiftListViewModel>();

            return shifts.GroupBy(x => x.DayOfWeek).Select(group => new ShiftListViewModel(
                group.Key,
                group.Select(shift => new ShiftListItemViewModel(shift)).ToList())
            ).ToList();
        }
    }
}
