using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace EZGO.Maui.Core.Models.ModelInterfaces
{
    /// <summary>
    /// Abstracts the ability to host a popup window
    /// </summary>
    public interface IHasPopup
    {
        /// <summary>
        /// Indicates if the popup window is open
        /// </summary>
        bool IsPopupOpen { get; }

        /// <summary>
        /// Gets the command to open the popup
        /// </summary>
        ICommand OpenPopupCommand { get; }

        /// <summary>
        /// Gets the command to submit the popup
        /// </summary>
        ICommand SubmitPopupCommand { get; }
        
        /// <summary>
        /// Gets the command to close the popup
        /// </summary>
        ICommand ClosePopupCommand { get; }
    }
}
