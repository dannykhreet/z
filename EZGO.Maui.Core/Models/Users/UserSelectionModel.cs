namespace EZGO.Maui.Core.Models.Users
{
    /// <summary>
    /// User selection model.
    /// </summary>
    public class UserSelectionModel
    {
        /// <summary>
        /// Gets or sets the fullname.
        /// </summary>
        /// <value>
        /// The fullname.
        /// </value>
        public string Fullname { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an icon should be shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if an icon should be shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowIcon { get; set; }
    }
}
