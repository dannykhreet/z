using Syncfusion.Maui.DataSource;
using System;
using System.ComponentModel;

namespace EZGO.Maui.Core.Models
{
    /// <summary>
    /// Custom data source that provides additional features
    /// </summary>
    public class CustomDataSource : DataSource
    {
        /// <summary>
        /// Event hanlder for filter changing event.
        /// </summary>
        /// <param name="sender">Send of the event.</param>
        /// <param name="args">Event arguments.</param>
        public delegate void FilterChangingHandler(CustomDataSource sender, FilterChangingEventAgrs args);

        /// <summary>
        /// Raised when the filter is about to change.
        /// </summary>
        public event FilterChangingHandler FilterChanging = (s, e) => { };

        /// <summary>
        /// Refreshes current filter with the ability to preserve current scrolling Y position.
        /// </summary>
        /// <param name="preserveScroll">Whether or not to preserve current scrolling Y position.</param>
        public void RefreshFilter(bool preserveScroll)
        {
            FilterChanging(this, new FilterChangingEventAgrs()
            {
                PreserveScroll = preserveScroll
            });

            base.RefreshFilter();
        }
    }

    /// <summary>
    /// Event args for filter changing event
    /// </summary>
    public class FilterChangingEventAgrs : EventArgs
    {
        /// <summary>
        /// Whether or not to keep the current scroll Y position
        /// </summary>
        public bool PreserveScroll { get; set; }
    }
}
