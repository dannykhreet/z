using EZGO.Api.Models;
using EZGO.Api.Models.Stats;
using EZGO.Maui.Core.Extensions;
using NodaTime;
using Plugin.Media.Abstractions;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Media item class.
    /// </summary>
    public class MediaItem : NotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the picture URL.
        /// </summary>
        /// <value>
        /// The picture URL.
        /// </value>
        public string PictureUrl { get; set; }

        /// <summary>
        /// Gets the CreatedAt datetime.
        /// </summary>
        /// <value>
        /// CreatedAt datetime.
        /// </value>
        public LocalDateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the video URL.
        /// </summary>
        /// <value>
        /// The video URL.
        /// </value>
        public string VideoUrl { get; set; }

        /// <summary>
        /// Gets or sets the file URL.
        /// </summary>
        /// <value>
        /// The video URL.
        /// </value>
        public string FileUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a video.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a video; otherwise, <c>false</c>.
        /// </value>
        public bool IsVideo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a file; otherwise, <c>false</c>.
        /// </value>
        public bool IsFile { get; set; }

        /// <summary>
        /// Indicates if this media item is currently stored on the device
        /// </summary>
        public bool IsLocalFile { get; set; }

        /// <summary>
        /// Indicates whether this media element is empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(PictureUrl) && string.IsNullOrEmpty(VideoUrl) && string.IsNullOrEmpty(FileUrl);

        /// <summary>
        /// Media file representation of this media item
        /// </summary>
        public MediaFile MediaFile { get; set; }

        /// <summary>
        /// Currently used for PDF
        /// </summary>
        public Stream FileStream { get; set; }

        /// <summary>
        /// Used for checking if media item should be deleted from device after uploading it to the backend
        /// </summary>
        public bool ShouldRemoveAfterUpload { get; set; } = false;

        /// <summary>
        /// UserId of the user that created the file
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the user that created the file
        /// </summary>
        public string UserFullName { get; set; }

        public async Task<Stream> GetFileStream()
        {
            if (FileStream == null)
                return null;

            MemoryStream stream = new();
            FileStream.Seek(0, SeekOrigin.Begin);
            await FileStream.CopyToAsync(stream);
            return stream;
        }

        public MediaItem()
        {
            CreatedAt = DateTimeHelper.Now;
        }

        public void CopyFrom(MediaItem another)
        {
            IsLocalFile = another.IsLocalFile;
            PictureUrl = another.PictureUrl;
            VideoUrl = another.VideoUrl;
            IsVideo = another.IsVideo;
            MediaFile = another.MediaFile;
            IsFile = another.IsFile;
            FileUrl = another.FileUrl;
            FileStream = another.FileStream;
        }

        /// <summary>
        /// Creates an on-line media file item with a video
        /// </summary>
        /// <param name="video">The path to the video</param>
        /// <param name="thumbnail">The path to the thumbnail</param>
        /// <returns>A new instance of <see cref="MediaItem"/></returns>
        public static MediaItem OnlineVideo(string video, string thumbnail)
        {
            return new MediaItem()
            {
                IsLocalFile = false,
                IsVideo = true,
                VideoUrl = video,
                PictureUrl = thumbnail,
            };
        }

        /// <summary>
        /// Creates an on-line media file item with a picture
        /// </summary>
        /// <param name="picture">The path to the picture</param>
        /// <returns>A new instance of <see cref="MediaItem"/></returns>
        public static MediaItem OnlinePicture(string picture)
        {
            return new MediaItem()
            {
                IsLocalFile = false,
                IsVideo = false,
                PictureUrl = picture,
            };
        }

        public static MediaItem Video(string video, string thumbnail, bool isLocal)
        {
            return new MediaItem()
            {
                IsLocalFile = isLocal,
                IsVideo = true,
                VideoUrl = video,
                PictureUrl = thumbnail,
                MediaFile = isLocal ? new MediaFile(video, () => File.OpenRead(video)) : null,
            };
        }

        public static MediaItem Picture(string picture, bool isLocal)
        {
            return new MediaItem()
            {
                IsLocalFile = isLocal,
                IsVideo = false,
                PictureUrl = picture,
                MediaFile = isLocal ? new MediaFile(picture, () => File.OpenRead(picture)) : null,
            };
        }

        public static MediaItem Empty()
        {
            return new MediaItem();
        }

        public static MediaItem FromApiAttachment(Attachment attachment)
        {
            if (!attachment.VideoThumbnailUri.IsNullOrEmpty())
                return OnlineVideo(attachment.Uri, attachment.VideoThumbnailUri);

            return OnlinePicture(attachment.Uri);
        }

        public static Attachment ToApiAttachment(MediaItem mediaItem)
        {
            string uri = null;
            string videoThumbnail = null;

            if (mediaItem.IsVideo)
            {
                uri = mediaItem.VideoUrl;
                videoThumbnail = mediaItem.PictureUrl;
            }
            else if (mediaItem.IsFile)
            {
                uri = mediaItem.FileUrl;
            }
            else
            {
                uri = mediaItem.PictureUrl;
            }

            return new Attachment()
            {
                Uri = uri,
                VideoThumbnailUri = videoThumbnail,
            };
        }
    }
}
