using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.ObjectModel;

namespace EZGO.Maui.Core.ViewModels.Tasks.Editing
{
    public class RecurrencyEditorWeekViewModel : NotifyPropertyChanged
    {
        #region Public Properties

		///<summary>
        /// The view model for the dates settings of this recurrency rule
        /// </summary>
        public RecurrencyDateRangeEditorViewModel Dates { get; private set; }

        /// <summary>
        /// Indicates every which week the task should occur
        /// </summary>
        public int? Week { get; set; }

        /// <summary>
        /// Collection of key value pairs where keys are days of the week and values indicate whether that day is checked or not
        /// </summary>
        public ObservableCollection<KeyValuePairViewModel<DayOfWeek, bool>> Weekdays { get; set; }
        
        #endregion

        public RecurrencyEditorWeekViewModel(EditTaskRecurrencyModel recurrency)
        {
            Model = recurrency;
            if (recurrency.RecurrencyType != RecurrencyTypeEnum.Week)
                recurrency = null;

            Dates = new RecurrencyDateRangeEditorViewModel(Model, recurrency != null);
            Week = recurrency?.Schedule?.Week;

            Weekdays = new ObservableCollection<KeyValuePairViewModel<DayOfWeek, bool>>
            {
                new KeyValuePairViewModel<DayOfWeek, bool>(DayOfWeek.Monday,     recurrency?.Schedule?.Weekday1 ?? false, DayOfWeek.Monday.Translate(), null),
                new KeyValuePairViewModel<DayOfWeek, bool>(DayOfWeek.Tuesday,    recurrency?.Schedule?.Weekday2 ?? false, DayOfWeek.Tuesday.Translate(), null),
                new KeyValuePairViewModel<DayOfWeek, bool>(DayOfWeek.Wednesday,  recurrency?.Schedule?.Weekday3 ?? false, DayOfWeek.Wednesday.Translate(), null),
                new KeyValuePairViewModel<DayOfWeek, bool>(DayOfWeek.Thursday,   recurrency?.Schedule?.Weekday4 ?? false, DayOfWeek.Thursday.Translate(), null),
                new KeyValuePairViewModel<DayOfWeek, bool>(DayOfWeek.Friday,     recurrency?.Schedule?.Weekday5 ?? false, DayOfWeek.Friday.Translate(), null),
                new KeyValuePairViewModel<DayOfWeek, bool>(DayOfWeek.Saturday,   recurrency?.Schedule?.Weekday6 ?? false, DayOfWeek.Saturday.Translate(), null),
                new KeyValuePairViewModel<DayOfWeek, bool>(DayOfWeek.Sunday,     recurrency?.Schedule?.Weekday0 ?? false, DayOfWeek.Sunday.Translate(), null)
            };
        }

        /// <summary>
        /// Submits the changes to the underlying object
        /// </summary>
        public void Submit()
        {
            Dates.Submit();
            Model.Schedule.Week = Week;
            foreach(var pair in Weekdays)
            {
                // 0 - Sunday, 1 - Monday ... 6 - Saturday
                var prop = typeof(Schedule).GetProperty($"Weekday{(int)pair.Key}");
                prop.SetValue(Model.Schedule, pair.Value);
            }
        }

        private readonly EditTaskRecurrencyModel Model;

    }
}
