using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class EditTaskRecurrencyModel : NotifyPropertyChanged
    {
        public int AreaId { get; set; }

        public RecurrencyTypeEnum RecurrencyType { get; set; }

        public List<int> Shifts { get; set; }

        public Schedule Schedule { get; set; }


        private TaskRecurrency _recurrency;

        public EditTaskRecurrencyModel(TaskRecurrency recurrency = null)
        {
            _recurrency = recurrency;
            if (_recurrency == null)
            {
                // Create defaults
                AreaId = Settings.WorkAreaId;
                _recurrency = new TaskRecurrency();
                Shifts = new List<int>();
                Schedule = new Schedule();
            }
            else
            {
                AreaId = _recurrency.AreaId;
                RecurrencyType = _recurrency.GetRecurrencyType();
                // NOTE .ToList() ensures we have a copied list because we don't want to modify the existing collection
                Shifts = _recurrency.Shifts?.ToList() ?? new List<int>();
                Schedule = _recurrency.Schedule.Clone() ?? new Schedule();
            }

        }

        public TaskRecurrency GetUpdatedObject()
        {
            TaskRecurrency rec;
            if (_recurrency == null)
                rec = new TaskRecurrency();
            else
                rec = _recurrency;

            rec.AreaId = AreaId;
            rec.RecurrencyType = RecurrencyType.ToApiString();
            rec.Shifts = Shifts;
            rec.Schedule = Schedule;
            // Must be set or the API returns 500
            rec.Schedule.IsOncePerMonth ??= default;
            rec.Schedule.IsOncePerWeek ??= default;
            rec.Schedule.Weekday0 ??= default;
            rec.Schedule.Weekday1 ??= default;
            rec.Schedule.Weekday2 ??= default;
            rec.Schedule.Weekday3 ??= default;
            rec.Schedule.Weekday4 ??= default;
            rec.Schedule.Weekday5 ??= default;
            rec.Schedule.Weekday6 ??= default;

            return _recurrency;
        }
    }
}
