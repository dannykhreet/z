using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Cleaners
{
    /// <summary>
    /// MediaCleaner; Media cleaner, cleans media strings and removes incorrect data.
    /// </summary>
    public static class MediaCleaner
    {
        public static string CleanPicture(string picture)
        {
            if (!string.IsNullOrEmpty(picture))
            {
                if (picture.Contains("blob:"))
                {
                    picture = string.Empty;
                }
                if (picture.Length > 100)
                {
                    picture = string.Empty;
                }
            }

            return picture;
        }

        public static string CleanVideo(string video)
        {
            if (!string.IsNullOrEmpty(video))
            {
                if (video.Contains("blob:"))
                {
                    video = string.Empty;
                }
                if (video.Length > 200)
                {
                    video = string.Empty;
                }
            }

            return video;
        }
    }
}
