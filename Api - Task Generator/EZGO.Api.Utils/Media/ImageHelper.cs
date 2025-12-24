using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils
{
    /// <summary>
    /// ImageHelper; Helpers for images;
    /// - Getting a basic image based on enum value (DefaultImageTypeEnum)
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Getting a basic image based on enum value (DefaultImageTypeEnum)
        /// TODO not yet implemanted.
        /// </summary>
        /// <param name="image">images enum value.</param>
        /// <returns>string containing a image.</returns>
        public static string GetImage(int image)
        {
            string baseURL = "/media/tasks/";
            switch (image)
            {
                case (int)DefaultImageTypeEnum.Checklist:
                    return $"{baseURL}31259/de9b9132-a5e5-42a1-8ad7-5dd008786253.png";
                //case (int)DefaultImageType.Audit:
                //    return $"{baseURL}31259/de9b9132-a5e5-42a1-8ad7-5dd008786253.png";
                //case (int)DefaultImageType.Area:
                //    return $"{baseURL}31259/de9b9132-a5e5-42a1-8ad7-5dd008786253.png";
                case (int)DefaultImageTypeEnum.Action:
                    return $"{baseURL}31257/db15782f-c763-4a8e-aaa7-e3d4615fa469.png";
                //case (int)DefaultImageType.Shift:
                //    return $"{baseURL}31259/de9b9132-a5e5-42a1-8ad7-5dd008786253.png";
                case (int)DefaultImageTypeEnum.Reports:
                    return $"{baseURL}31260/9c7271d6-abcc-4ed6-9809-07ad2db95b63.png";
                case (int)DefaultImageTypeEnum.Task:
                    return $"{baseURL}31261/dd531e0e-bcef-4771-af4e-cb9beff180d5.png";
                case (int)DefaultImageTypeEnum.General:
                default:
                    return $"{baseURL}31262/1a073a8a-f1d7-4ca6-94c4-9f49767b6781.png";
            }
        }
    }
}
