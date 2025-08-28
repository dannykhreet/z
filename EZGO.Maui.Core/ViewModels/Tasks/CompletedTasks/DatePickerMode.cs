using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.ViewModels.Tasks.CompletedTasks
{
    /// <summary>
    /// Describes available modes for a date picker in all tasks page
    /// </summary>
    public enum DatePickerMode
    {
        /// <summary>
        /// Go to a specific date mode
        /// </summary>
        GoToDate,

        /// <summary>
        /// Select a date range mode
        /// </summary>
        DateRange,
    }
}
