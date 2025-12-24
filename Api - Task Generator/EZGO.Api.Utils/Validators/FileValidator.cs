using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.Validators
{
    public static class FileValidator
    {
        #region - messages -
        public const string MESSAGE_FILE_IS_NOT_A_SUPPORTED_IMAGE = "The uploaded file is not a supported image format (webp,svg,jpg,jpeg,jfif,pjpeg,pjp,png,gif,avif,apng,bmp,ico,cur,tif,tiff).";
        public const string MESSAGE_FILE_IS_NOT_A_SUPPORTED_DOC = "The uploaded file is not a supported document format (pdf,docx,txt).";
        public const string MESSAGE_FILE_IS_NOT_A_SUPPORTED_VIDEO = "The uploaded file is not a supported video format (mp4, m4a, m4v, f4v, f4a, m4b, m4r, f4b, mov, avi, webm).";
        public const string MESSAGE_FILE_IS_NOT_A_SUPPORTED_CSV = "The uploaded file is not a supported csv format (csv).";
        public static string MESSAGE_FILE_SIZE_IS_TOO_LARGE = $"The file size exceeds the allowed amount of {MAX_FILE_SIZE_CSV} bytes";
        private static string[] IMAGE_COLLECTION = new[] {".webp",".svg",".jpg",".jpeg",".jfif",".pjpeg",".pjp",".png",".gif",".avif",".apng",".bmp",".ico",".cur",".tif",".tiff"};
        private static string[] DOCS_COLLECTION = new[] { ".pdf", ".docx", ".txt" };
        private static string[] VIDEO_COLLECTION = new[] { ".mp4", ".m4a", ".m4v", ".f4v", ".f4a", ".m4b", ".m4r", ".f4b", ".mov", ".avi", ".webm" };
        private static string[] CSV_COLLECTION = new[] { ".csv" };
        private static long MAX_FILE_SIZE_CSV = 250000000; //250 MB
        #endregion

        public static bool CheckImageFormat(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            return IMAGE_COLLECTION.Contains(Path.GetExtension(fileName).ToLower());

        }

        public static bool CheckDocsFormat(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            return DOCS_COLLECTION.Contains(Path.GetExtension(fileName).ToLower());

        }

        public static bool CheckVideoFormat(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            return VIDEO_COLLECTION.Contains(Path.GetExtension(fileName).ToLower());
        }

        public static bool CheckCsvFormat(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            return CSV_COLLECTION.Contains(Path.GetExtension(fileName).ToLower());
        }

        public static bool CheckCsvFilesize(long size)
        {
            return size > 0 && size < MAX_FILE_SIZE_CSV;
        }

        /// <summary>
        /// CheckMediaLocation; Check media location;
        /// This check will mostly check if internal URI's (from tablets, e.g. storage locations) are accidentally posted as a media location (which should be a URI part)
        /// </summary>
        /// <param name="mediaLocation"></param>
        /// <returns>true/false</returns>
        public static bool CheckMediaLocation (string mediaLocation, MediaLocationCheckTypeEnum mediaLocationCheckType = MediaLocationCheckTypeEnum.All)
        {
            if(string.IsNullOrEmpty(mediaLocation))
            {
                return false;
            }

            if(mediaLocationCheckType == MediaLocationCheckTypeEnum.All || mediaLocationCheckType == MediaLocationCheckTypeEnum.InternalLocations)
            {
                if (mediaLocation.Contains("/var/") || mediaLocation.Contains("/storage/")) {
                    return false;
                }
            }

            if (mediaLocationCheckType == MediaLocationCheckTypeEnum.All || mediaLocationCheckType == MediaLocationCheckTypeEnum.MediaLocations)
            {
                if (mediaLocation.StartsWith("media/")) {
                    return false;
                }
            }
            return true;
        }

        public enum MediaLocationCheckTypeEnum
        {
            All,
            InternalLocations, //e.g. /var/mobile/xxxx or /var/storage/xxxxx 
            MediaLocations, //e.g. /media/00/something/xxxx
        }
    }
}
