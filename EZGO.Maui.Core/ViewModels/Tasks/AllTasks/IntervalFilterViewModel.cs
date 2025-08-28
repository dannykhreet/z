using EZGO.Maui.Core.Attributes;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;
using Syncfusion.Maui.DataSource.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EZGO.Maui.Core.ViewModels.AllTasks
{
    /// <summary>
    /// View model for the interval filter dropdown
    /// </summary>
    public class IntervalFilterViewModel : NotifyPropertyChanged
    {
        private bool _Once;

        [NamedProperty("no recurrency")]
        public bool Once
        {
            get => _Once;
            set
            {
                _Once = value;
                OnPropertyChanged();
            }
        }

        private string _StrOnce;
        public string StrOnce
        {
            get => _StrOnce;
            set
            {
                _StrOnce = value;
                OnPropertyChanged();
            }
        }


        private bool _Shift;

        [NamedProperty("shifts")]
        public bool Shift
        {
            get => _Shift;
            set
            {
                _Shift = value;
                OnPropertyChanged();
            }
        }

        private string _StrShift;
        public string StrShift
        {
            get => _StrShift;
            set
            {
                _StrShift = value;
                OnPropertyChanged();
            }
        }

        private bool _Week;

        [NamedProperty("week")]
        public bool Week
        {
            get => _Week;
            set
            {
                _Week = value;
                OnPropertyChanged();
            }
        }

        private string _StrWeek;
        public string StrWeek
        {
            get => _StrWeek;
            set
            {
                _StrWeek = value;
                OnPropertyChanged();
            }
        }

        private bool _Month;

        [NamedProperty("month")]
        public bool Month
        {
            get => _Month;
            set
            {
                _Month = value;
                OnPropertyChanged();
            }
        }

        private string _StrMonth;
        public string StrMonth
        {
            get => _StrMonth;
            set
            {
                _StrMonth = value;
                OnPropertyChanged();
            }
        }

        [NamedProperty("periodday")]
        public bool DailyInterval { get; set; }
        public string StrDailyInterval { get; set; }

        [NamedProperty("dynamicday")]
        public bool DynamicDailyInterval { get; set; }
        public string StrDynamicDailyInterval { get; set; }

        public IntervalFilterViewModel(bool initialValue = true)
        {
            Once = initialValue;
            Shift = initialValue;
            Week = initialValue;
            Month = initialValue;
            DailyInterval = initialValue;
            DynamicDailyInterval = initialValue;

            StrOnce = SetIntervalFor(LanguageConstants.shiftTypeOnce, nameof(Once));
            StrShift = SetIntervalFor(LanguageConstants.shiftTypeShift, nameof(Shift));
            StrWeek = SetIntervalFor(LanguageConstants.shiftTypeWeek, nameof(Week));
            StrMonth = SetIntervalFor(LanguageConstants.shiftTypeMonth, nameof(Month));
            StrDailyInterval = SetIntervalFor(LanguageConstants.shiftTypeDailyInterval, nameof(DailyInterval));
            StrDynamicDailyInterval = SetIntervalFor(LanguageConstants.shiftTypeDynamicDailyInterval, nameof(DynamicDailyInterval));
        }

        private string SetIntervalFor(string shiftType, string type)
        {
            string value = TranslateExtension.GetValueFromDictionary(shiftType);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            else
            {
                return type;
            }
        }

        public List<string> GetChecked()
        {
            var selected =
                GetType()
                .GetProperties()
                .Select(x => new { prop = x, attr = x.GetCustomAttributes(typeof(NamedPropertyAttribute)).FirstOrDefault() })
                .Where(x => x.attr != null)
                .Select(x => new { prop = x.prop.GetValue(this), name = ((NamedPropertyAttribute)x.attr).Name })
                .Where(x => x.prop is bool value && value)
                .Select(x => x.name)
                .ToList();

            return selected;
        }
    }

}
