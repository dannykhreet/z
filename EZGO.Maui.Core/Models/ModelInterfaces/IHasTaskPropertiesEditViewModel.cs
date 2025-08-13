using EZGO.Maui.Core.Models.Tasks.Properties;

namespace EZGO.Maui.Core.Models.ModelInterfaces
{
    /// <summary>
    /// Abstracts the ability to be able to edit a task property.
    /// </summary>
    public interface IHasTaskPropertiesEditViewModel
    {
        /// <summary>
        /// Gets the view model used to edit the task property.
        /// </summary>
        BaseTaskPropertyEditViewModel PropertyEditViewModel { get; }
    }
}
