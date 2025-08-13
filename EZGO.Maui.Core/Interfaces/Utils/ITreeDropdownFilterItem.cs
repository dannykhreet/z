using System.Collections.Generic;
using System.Windows.Input;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    /// <summary>
    /// Interface used in tree dropdown filter control
    /// </summary>
    public interface ITreeDropdownFilterItem
    {
        /// <summary>
        /// Id of the item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name to be displayed on control
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of children 
        /// </summary>
        List<ITreeDropdownFilterItem> Children { get; set; }

        /// <summary>
        /// Command to be used when item gets tapped 
        /// </summary>        
        //ICommand DropdownTapCommand { get; set; }
    }    
}
