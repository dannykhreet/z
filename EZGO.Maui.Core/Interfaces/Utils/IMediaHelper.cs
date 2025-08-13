using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Utils
{
    /// <summary>
    /// Media helper.
    /// </summary>
    public interface IMediaHelper
    {
        /// <summary>
        /// Lets the user pick media asynchronous.
        /// </summary>
        /// <param name="mediaOptions">The media options the user get's to choose from.</param>
        /// <param name="generateVideoThumbnail">If set to true a thumbnail will be generated if the chosen media option is a video.</param>
        /// <returns>Chosen media file or null if remove media is chosen or when the right permissions are not given.</returns>
        Task<DialogResult<MediaItem>> PickMediaAsync(IEnumerable<MediaOption> mediaOptions, bool generateVideoThumbnail = true);

        /// <summary>
        /// Lets the user pick media asynchronous.
        /// </summary>
        /// <param name="mediaOption">The media option.</param>
        /// <param name="generateVideoThumbnail">If set to true a thumbnail will be generated if the chosen media option is a video.</param>
        /// <returns>Chosen media file or null if remove media is chosen or when the right permissions are not given.</returns>
        Task<DialogResult<MediaItem>> PickMediaAsync(MediaOption mediaOption, bool generateVideoThumbnail = true);

        /// <summary>
        /// Lets the user pick a pdf file
        /// </summary>
        /// <returns>Choosen file as a dialog result</returns>
        Task<DialogResult<FileItem>> PickPdfFileAsync();
    }
}
