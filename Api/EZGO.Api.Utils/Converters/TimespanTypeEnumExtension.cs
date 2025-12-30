using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class TimespanTypeEnumExtension
    {
        /// <summary>
        /// ToDays(); Will convert the TimespanTypeEnum to days.
        /// </summary>
        /// <param name="timespan"></param>
        /// <returns></returns>
        public static int ToDays(this TimespanTypeEnum timespan)
        {
            int output = 1; //will default to one day if not correctly supplied.

            switch (timespan)
            {

                case TimespanTypeEnum.LastDay: output = (int)TimespanTypeEnum.LastDay; break;// = 1,
                case TimespanTypeEnum.LastTwelveDays: output = (int)TimespanTypeEnum.LastTwelveDays; break;// = 12,
                case TimespanTypeEnum.LastSevenDays: output = (int)TimespanTypeEnum.LastSevenDays; break;// = 7,
                case TimespanTypeEnum.LastThirtyDays: output = (int)TimespanTypeEnum.LastThirtyDays; break;// = 30,
                case TimespanTypeEnum.LastTwelveWeeks: output = (int)TimespanTypeEnum.LastTwelveWeeks; break;// = 84,
                case TimespanTypeEnum.LastTwelveMonths: output = (int)TimespanTypeEnum.LastTwelveMonths; break;// = 366,
                case TimespanTypeEnum.LastYear: output = (int)TimespanTypeEnum.LastYear; break;// 365,
                case TimespanTypeEnum.ThisYear: output = (int)DateTime.Now.DayOfYear; break;// 9999 -> will be generated dynamically based on day of year.
            }
            return output;
        }
    }
}
