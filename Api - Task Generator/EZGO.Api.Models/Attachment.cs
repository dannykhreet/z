using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Attachment; Used within several parts when uploading/adding certain media to for example comments, feed etc. 
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// The FileName of the Attachment e.g. "a filename.pdf"
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// The FileType of the Attachment e.g. "application/pdf"
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// The FileExtension of the Attachment e.g. ".pdf"
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// The Uri part of the file in s3 e.g. "136/workinstruction/0/4516f14c-4061-4c7a-ac38-5e0d03f67ea2.pdf" (NOTE: not an actual file in the storage)
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// The AttachmentType of the Attachment e.g. "doc" or "link" (to be extended with more options)
        /// </summary>
        public string AttachmentType { get; set; }
        /// <summary>
        /// VideoThumbnailUri; Only present if the Attachment is a video
        /// </summary>
        public string VideoThumbnailUri { get; set; }
        /// <summary>
        /// The size in bytes of the Attachment
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// The DateTime when the Attachment was last modified
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
