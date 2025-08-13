using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EZGO.Maui.Core.ViewModels.Tasks.Editing
{
    public class RecurrencyEditorMonthViewModel : NotifyPropertyChanged
    {
        #region Public Properties

        /// <summary>
        /// The view model for the dates settings of this recurrency rule
        /// </summary>
        public RecurrencyDateRangeEditorViewModel Dates { get; private set; }

        /// <summary>
        /// Indicates whether the mode of this recurrency is 'weekday'
        /// </summary>
        public bool IsWeekDay { get; set; }

        /// <summary>
        /// Indicated whether the mode of this recurrency us 'day_of_month'
        /// </summary>
        public bool IsDayOfMonth { get; set; }

        /// <summary>
        /// Reoccur on every X days of every ...
        /// </summary>
        [DependsOn(nameof(IsDayOfMonth))]
        public int? Week
        {
            get => IsDayOfMonth ? week : null;
            set => week = value;
        }

        /// <summary>
        /// Reoccur ... every X month [for month]
        /// </summary>
        [DependsOn(nameof(IsDayOfMonth))]
        public int? MonthMonth
        {
            get => IsDayOfMonth ? monthMonth : null;
            set => monthMonth = value;
        }

        /// <summary>
        /// Reoccur every ... X - Sunday = 1 ... Saturday = 7
        /// </summary>
        [DependsOn(nameof(IsWeekDay))]
        public int? WeekDay
        {
            get => IsWeekDay ? weekDay : null;
            set => weekDay = value;
        }

        /// <summary>
        /// Reoccur every 1st-4th [weekday] ... 
        /// </summary>
        [DependsOn(nameof(IsWeekDay))]
        public int? WeekDayNumber
        {
            get => IsWeekDay ? weekDayNumber : null;
            set => weekDayNumber = value;
        }

        /// <summary>
        /// Reoccur ... every X month [for day of week]
        /// </summary>
        [DependsOn(nameof(IsWeekDay))]
        public int? MonthWeek 
        { 
            get => IsWeekDay ? monthWeek : null; 
            set => monthWeek = value; 
        }

        /// <summary>
        /// All options for <see cref="WeekDayNumber"/>
        /// </summary>
        public List<KeyValuePairViewModel<string, int?>> WeekDayNumberOptions { get; set; }

        /// <summary>
        /// All options for <see cref="WeekDay"/>
        /// </summary>
        public List<KeyValuePairViewModel<string, int?>> WeekDayOptions { get; set; }

        /// <summary>
        /// Selected item for the <see cref="WeekDay"/>.
        /// </summary>
        /// <remarks>Needs to be done this way because SfComboBox doesn't allow clearing selection by setting SelectedValue to null</remarks>
        [DependsOn(nameof(WeekDay))]
        public KeyValuePairViewModel<string, int?> WeekDayValue
        {
            get => WeekDayOptions.Where(x => x.Value == WeekDay).FirstOrDefault();
            set => WeekDay = value?.Value;
        }

        [DependsOn(nameof(WeekDayNumber))]
        public KeyValuePairViewModel<string, int?> WeekDayNumberValue
        {
            get => WeekDayNumberOptions.Where(x => x.Value == WeekDayNumber).FirstOrDefault();
            set => WeekDayNumber = value?.Value;
        }

        #endregion

        private readonly EditTaskRecurrencyModel Model;
        private int? week;
        private int? monthMonth;
        private int? weekDay;
        private int? weekDayNumber;
        private int? monthWeek;

        public RecurrencyEditorMonthViewModel(EditTaskRecurrencyModel recurrency)
        { 
            Model = recurrency;
            if (recurrency.RecurrencyType != RecurrencyTypeEnum.Month)
                recurrency = null;

            Dates = new RecurrencyDateRangeEditorViewModel(Model, recurrency != null);
            WeekDayOptions = new List<KeyValuePairViewModel<string, int?>>(
                ((DayOfWeek[])Enum.GetValues(typeof(DayOfWeek)))
                .ToList()
                .Select(x => new KeyValuePairViewModel<string, int?>(x.Translate(), (int)x + 1)));

            WeekDayNumberOptions = new List<KeyValuePairViewModel<string, int?>>()
            {
                new KeyValuePairViewModel<string, int?>(TranslateExtension.GetValueFromDictionary(LanguageConstants.generalTextFirst), 1),
                new KeyValuePairViewModel<string, int?>(TranslateExtension.GetValueFromDictionary(LanguageConstants.generalTextSecond), 2),
                new KeyValuePairViewModel<string, int?>(TranslateExtension.GetValueFromDictionary(LanguageConstants.generalTextThird), 3),
                new KeyValuePairViewModel<string, int?>(TranslateExtension.GetValueFromDictionary(LanguageConstants.generalTextFourth), 4),
            };

            if (recurrency != null)
            {
                if (recurrency.Schedule.MonthRecurrencyType == "weekday")
                {
                    IsWeekDay = true;
                    IsDayOfMonth = false;
                    MonthWeek = recurrency.Schedule.Month;
                    WeekDay = recurrency.Schedule.WeekDay;
                    WeekDayNumber = recurrency.Schedule.WeekDayNumber;
                }
                else if (recurrency.Schedule.MonthRecurrencyType == "day_of_month")
                {
                    IsWeekDay = false;
                    IsDayOfMonth = true;
                    Week = recurrency.Schedule.Week;
                    MonthMonth = recurrency.Schedule.Month;
                }
            }
            else
            {
                // Defaults
                IsWeekDay = false;
                IsDayOfMonth = false;
                Week = null;
                MonthMonth = null;
                MonthWeek = null;
                WeekDay = null;
                WeekDayNumber = null;

            }
        }

        public void Submit()
        {
            Dates.Submit();

            if (IsWeekDay)
            {
                Model.Schedule.MonthRecurrencyType = "weekday";
                Model.Schedule.WeekDay = WeekDay;
                Model.Schedule.WeekDayNumber = WeekDayNumber;
                Model.Schedule.Month = MonthWeek;
            }
            else if (IsDayOfMonth)
            {
                Model.Schedule.MonthRecurrencyType = "day_of_month";
                Model.Schedule.Week = Week;
                Model.Schedule.Month = MonthMonth;
            }
        }
    }
}
