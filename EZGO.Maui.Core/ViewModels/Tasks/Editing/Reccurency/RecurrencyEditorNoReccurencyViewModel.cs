using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Shifts;
using EZGO.Maui.Core.Models.Tasks;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.ViewModels.Tasks.Editing
{
    public class RecurrencyEditorNoReccurencyViewModel : NotifyPropertyChanged
    {
        #region Public Properties

        /// <summary>
        /// A single time the recurrency should happen
        /// </summary>
        public LocalDateTime Date { get; set; }

        /// <summary>
        /// Shifts during which the recurrency should happen
        /// </summary>
        public ShiftListViewModel CurrenctDisplayedShift { get; set; }

        #endregion

        public RecurrencyEditorNoReccurencyViewModel(List<ShiftModel> allShifts, EditTaskRecurrencyModel model)
        {
            Model = model;
            AllShifts = ShiftListViewModel.FromShifts(allShifts);
            // We only care about the model if it contains the data regarding no recurrency
            if (model.RecurrencyType != RecurrencyTypeEnum.NoRecurrency)
                model = null;

            if (model != null)
            {
                Date = Settings.ConvertDateTimeToLocal(model.Schedule.Date) ?? DateTimeHelper.Now;
                CurrenctDisplayedShift = AllShifts.Where(x => x.WeekDay == (DayOfWeek)Date.DayOfWeek).FirstOrDefault();
                CurrenctDisplayedShift.Items.Where(x => x != null).ToList().ForEach(x =>
                {
                    if (model.Shifts.Contains(x.Id))
                        x.IsChecked = true;
                });
            }
            else
            {
                Date = DateTimeHelper.Now.Date.AddDays(1);
                CurrenctDisplayedShift = AllShifts.Where(x => x.WeekDay == (DayOfWeek)Date.DayOfWeek).FirstOrDefault();
            }

            PropertyChanged += ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Date))
            {
                CurrenctDisplayedShift = AllShifts.Where(x => x.WeekDay == (DayOfWeek)Date.DayOfWeek).FirstOrDefault();
            }
        }

        private EditTaskRecurrencyModel Model;
        private List<ShiftListViewModel> AllShifts;

        public void Submit()
        {
            if (Model.Schedule != null)
                Model.Schedule.Date = Date.ToDateTimeUnspecified();

            Model.Shifts = CurrenctDisplayedShift?.Items
                .Where(x => x.IsChecked)
                .Select(x => x.Id)
                .ToList();
        }
    }
}
