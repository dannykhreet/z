using EZGO.Api.Models.Settings;
using Microsoft.Extensions.Configuration;
using System;
using WebApp.Logic;

namespace WebApp.Helpers
{
    public static class MediaHelpers
    {
        public static string GetMediaImageUrl(ApplicationSettings applicationSettings, string mediaUrl)
        {
            if (string.IsNullOrEmpty(applicationSettings?.MediaLocations?.ImageMediaBaseUri))
            {
                return Constants.General.GetMediaUrl(mediaUrl);
            }
            else
            {
                return string.Format("{0}{1}", applicationSettings?.MediaLocations?.ImageMediaBaseUri, mediaUrl);
            }
        }

        public static string GetMediaFileUrl(ApplicationSettings applicationSettings, string mediaUrl)
        {
            if (string.IsNullOrEmpty(applicationSettings?.MediaLocations?.FileMediaBaseUri))
            {
                return Constants.General.GetMediaUrl(mediaUrl);
            }
            else
            {
                return string.Format("{0}{1}", applicationSettings?.MediaLocations?.FileMediaBaseUri, mediaUrl);
            }

        }

        public static string GetMediaVideoUrl(ApplicationSettings applicationSettings, string mediaUrl)
        {
            if (string.IsNullOrEmpty(applicationSettings?.MediaLocations?.VideoMediaBaseUri))
            {
                return mediaUrl;
            }
            else
            {
                return string.Format("{0}{1}", applicationSettings?.MediaLocations?.VideoMediaBaseUri, mediaUrl);
            }

        }
    }
}
