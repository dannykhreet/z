using EZGO.Api.Models;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Tasks;
using NodaTime;
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.ViewModels.Tasks
{
    /// <summary>
    /// The view model for the date range part of a recurrency rule
    /// </summary>
    public class RecurrencyDateRangeEditorViewModel : NotifyPropertyChanged
    {
        #region Public Properties

        /// <summary>
        /// Indicates whether the recurrency should repeat forever
        /// </summary>
        public bool Forever { get; set; }

        /// <summary>
        /// Gets or sets the start date of the recurrency
        /// </summary>
        public LocalDateTime? StartDate { get; set; }

        /// <summary>
        /// Maximum date for <see cref="StartDate"/>
        /// </summary>
        public DateTime? MaximumStartDate => StartDate.HasValue ? EndDate?.ToDateTimeUnspecified().AddDays(-1) : null;

        /// <summary>
        /// Gets or sets the end date of the recurrency
        /// </summary>
        public LocalDateTime? EndDate { get; set; }

        /// <summary>
        /// Minimum date for <see cref="EndDate"/>
        /// </summary>
        public DateTime? MinimumEndDate
        {
            get
            {
                if (EndDate.HasValue)
                {
                    return StartDate?.AddDays(1).ToDateTimeUnspecified();
                }
                else
                {
                    return StartDate?.AddDays(1).ToDateTimeUnspecified();
                }
            }
        }

        #endregion

        #region Commands 

        public ICommand ClearStartDateCommand => new Command(() =>
        {
            StartDate = null;
        });

        public ICommand ClearEndDateCommand => new Command(() =>
        {
            EndDate = null;
        });

        #endregion

        #region Constructor

        /// <summary>
        /// Defaults constructor
        /// </summary>
        /// <param name="model">The model of the recurrency</param>
        public RecurrencyDateRangeEditorViewModel(EditTaskRecurrencyModel model, bool loadFromModel = false)
        {
            Model = model;
            if (loadFromModel)
            {
                StartDate = Settings.ConvertDateTimeToLocal(model?.Schedule?.StartDate);
                EndDate = Settings.ConvertDateTimeToLocal(model?.Schedule?.EndDate);
                Forever = StartDate == null && EndDate == null;
            }
            else
            {
                Forever = true;
                StartDate = Settings.ConvertDateTimeToLocal(DateTime.Now.Date);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Submits changes to the underlying object
        /// </summary>
        public void Submit()
        {
            if (Forever)
            {
                Model.Schedule.StartDate = null;
                Model.Schedule.EndDate = null;
            }
            else
            {
                Model.Schedule.StartDate = StartDate?.ToDateTimeUnspecified();
                Model.Schedule.EndDate = EndDate?.ToDateTimeUnspecified();
            }
        } 

        #endregion

        private EditTaskRecurrencyModel Model;
    }
}
