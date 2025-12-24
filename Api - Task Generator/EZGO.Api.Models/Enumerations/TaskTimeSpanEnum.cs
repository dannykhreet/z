using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Enumerations
{
    /// <summary>
    /// Represents the time span categories for tasks.
    /// </summary>
    /// <remarks>This enumeration is used to classify tasks based on their time span, such as within a shift,
    /// a day, a week, or overdue.</remarks>
    public enum TaskTimeSpanEnum
    {
        /// <summary>
        /// Represents a time period of one shift.
        /// </summary>
        Shift = 0,
        /// <summary>
        /// Represents a time period of one day.
        /// </summary>
        Day = 1,
        /// <summary>
        /// Represents a time period of one week.
        /// </summary>
        Week = 2,
        /// <summary>
        /// Represents a status indicating that the item is overdue.
        /// </summary>
        /// <remarks>This status is typically used to indicate that a deadline or due date has passed
        /// without the required task being completed.</remarks>
        Overdue = 3
    }

}
