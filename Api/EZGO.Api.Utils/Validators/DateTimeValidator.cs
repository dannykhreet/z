using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Validators
{
    /// <summary>
    /// DateTimeValidator, contains various datetime validators.
    /// </summary>
    public sealed class DateTimeValidator
    {
        /// <summary>
        /// DateIsSmallerThenCurrentDate, check if date is smaller then current date based on .net datetime (server date probably). (DateTime.Now)
        /// </summary>
        /// <param name="date"></param>
        /// <returns>true/false depending on outcome.</returns>
        public static bool DateIsSmallerThenCurrentDate(DateTime date)
        {
            if (date < DateTime.Now)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  DateIsSmallerThenCurrentDate, check if date is bigger then current date based on .net datetime (server date probably). (DateTime.Now)
        /// </summary>
        /// <param name="date"></param>
        /// <returns>true/false depending on outcome.</returns>
        public static bool DateIsBiggerThenCurrentDate(DateTime date)
        {
            if (date > DateTime.Now)
            {
                return true;
            }

            return false;
        }
    }
}
