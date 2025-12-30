using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class MonthRecurrencyTypeEnumExtension
    {
        public static string ToDatabaseString(this MonthRecurrencyTypeEnum monthRecurrencyType)
        {
            string output = "";
            switch (monthRecurrencyType)
            {
                case MonthRecurrencyTypeEnum.DayOfMonth : output = "day_of_month"; break;
                case MonthRecurrencyTypeEnum.Weekday: output = "weekday"; break;
            }
            return output;
        }
    }
}
