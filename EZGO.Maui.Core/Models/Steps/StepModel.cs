using EZGO.Api.Models;
using EZGO.Maui.Core.Interfaces.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EZGO.Maui.Core.Models.Steps
{
    public class StepModel : Step, IDetailItem
    {
        public new int Index { get; set; }

        /// <summary>
        /// Indicates if this step contains a video.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the step is a video.
        /// <see langword="false"/> if the step is a picture.
        /// </value>
        [JsonIgnore]
        public bool IsVideo => !string.IsNullOrEmpty(Video);

        [JsonIgnore]
        public bool HasVideo => !string.IsNullOrEmpty(Video);

        /// <summary>
        /// Indicates if this step contains a picture
        /// </summary>
        [JsonIgnore]
        public bool IsPicture => !string.IsNullOrEmpty(Picture);

        /// <summary>
        /// Gets the name of the picture to display for a preview
        /// If the step contains a video then the video thumbnail is returned
        /// Otherwise if the item contains a picture the picture is returned
        /// </summary>
        [JsonIgnore]
        public string PreviewPicture => IsVideo ? !string.IsNullOrEmpty(VideoThumbnail) ? VideoThumbnail : Picture : Picture;

        /// <summary>
        /// Indicates if the media part is a local data or an online data
        /// </summary>
        [JsonIgnore]
        public bool MediaIsLocal { get; set; }

        [JsonIgnore]
        bool IDetailItem.IsTaskMarked => false;

        [JsonIgnore]
        string IDetailItem.Name { get; set; } = "";

        [JsonIgnore]
        string IDetailItem.DueDateString => "";

        [JsonIgnore]
        string IDetailItem.OverDueString => "";

        [JsonIgnore]
        string IDetailItem.TaskMarkedString => "";

        [JsonIgnore]
        public Stream PDFStream { get; set; } = null;

        [JsonIgnore]
        public bool IsLocalMedia { get; set; }
    }
}
