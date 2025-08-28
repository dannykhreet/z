using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Shifts;
using EZGO.Maui.Core.Models.Tasks;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.ViewModels.Tasks.Editing
{
    public class RecurrencyEditorShiftsViewModel : NotifyPropertyChanged
    {
        #region Public Properties

        /// <summary>
        /// The view model for the dates settings of this recurrency rule
        /// </summary>
        public RecurrencyDateRangeEditorViewModel Dates { get; private set; }

        /// <summary>
        /// All available shifts grouped by week day
        /// </summary>
        public List<ShiftListViewModel> Shifts { get; set; }

        #endregion

        #region Constructor

        public RecurrencyEditorShiftsViewModel(List<ShiftModel> shifts, EditTaskRecurrencyModel recurrency)
        {
            AllShifts = shifts ?? new List<ShiftModel>();
            // Save the actual model
            Model = recurrency;
            Model.PropertyChanged += Model_PropertyChanged;

            // If a different recurrency type is selected right now
            if (recurrency.RecurrencyType != RecurrencyTypeEnum.Shifts)
                // Don't load data from the object since it can contain garbage information
                recurrency = null;

            Dates = new RecurrencyDateRangeEditorViewModel(Model, recurrency != null);

            // Group by day of the week and project into a sequence of list item view models
            Shifts = ShiftListViewModel.FromShifts(GetFilteredShifts());

            // Mark the selected in the edited object, if any
            recurrency?.Shifts?.ForEach(shiftId =>
            {
                var item = Shifts.SelectMany(x => x.Items).FirstOrDefault(x => x.Id == shiftId);
                if (item != null)
                    item.IsChecked = true;
            });
        }

        #endregion

        #region Public Methods

        public void Submit()
        {
            Dates.Submit();
            Model.Shifts = Shifts
                .SelectMany(x => x.Items)
                .Where(x => x.IsChecked)
                .Select(x => x.Id)
                .ToList();
        }

        #endregion

        #region Private Members

        private readonly EditTaskRecurrencyModel Model;
        private readonly List<ShiftModel> AllShifts;

        /// <summary>
        /// Applies area filter to shifts
        /// </summary>
        /// <returns>Shifts that apply in the current context</returns>
        private IEnumerable<ShiftModel> GetFilteredShifts()
        {
            // Get all the shifts for the current area
            var filteredShifts = AllShifts.Where(x => x.AreaId == Model.AreaId);

            // If there are none
            if (filteredShifts.Any() == false)
                // Get the company shits, which are the ones without area id
                filteredShifts = AllShifts.Where(x => x.AreaId == null);

            return filteredShifts;
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditTaskRecurrencyModel.AreaId))
            {
                // Get the new shifts for the new area
                var newShifts = GetFilteredShifts();

                // Select all the ids of currently loaded shifts
                var currentShiftsIds = Shifts.SelectMany(x => x.Items).Select(x => x.Id);

                // Check if new currently loaded shifts contain exactly the same shifts as the new shift set
                // NOTE count check is important because SequenceEqual returns true also when the first sequence contains more elements than the second sequence 
                var hasSameShifts = currentShiftsIds.Count() == newShifts.Count() && currentShiftsIds.SequenceEqual(newShifts.Select(x => x.Id));

                // If has different shifts the currently displayed
                if (!hasSameShifts)
                    // Update them
                    Shifts = ShiftListViewModel.FromShifts(GetFilteredShifts());
            }
        }

        #endregion

    }
}
