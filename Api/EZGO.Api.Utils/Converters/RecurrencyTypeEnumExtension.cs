using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class RecurrencyTypeEnumExtension
    {
        public static string ToDatabaseString(this RecurrencyTypeEnum recurrencyType)
        {
            string output = "";
            switch (recurrencyType)
            {
                case RecurrencyTypeEnum.Month: 
                case RecurrencyTypeEnum.Shifts: 
                case RecurrencyTypeEnum.Week: 
                case RecurrencyTypeEnum.PeriodDay: 
                case RecurrencyTypeEnum.PeriodHour: 
                case RecurrencyTypeEnum.PeriodMinute: 
                case RecurrencyTypeEnum.DynamicDay: 
                case RecurrencyTypeEnum.DynamicHour: 
                case RecurrencyTypeEnum.DynamicMinute: output = recurrencyType.ToString().ToLower(); break;
                case RecurrencyTypeEnum.NoRecurrency: output = "no recurrency"; break;
            }
            return output;
        }

        public static RecurrencyTypeEnum? ConvertStringToRecurrencyTypeEnum (string recurrencyTypeString)
        {
            if(!string.IsNullOrEmpty(recurrencyTypeString))
            {
                if(RecurrencyTypeEnum.Month.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.Month;

                } else if (RecurrencyTypeEnum.Shifts.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.Shifts;

                } else if (RecurrencyTypeEnum.Week.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.Week;

                } else if (RecurrencyTypeEnum.NoRecurrency.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.NoRecurrency;

                } else if (RecurrencyTypeEnum.PeriodDay.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.PeriodDay;

                } else if (RecurrencyTypeEnum.PeriodHour.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.PeriodHour;

                } else if (RecurrencyTypeEnum.PeriodMinute.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.PeriodMinute;
                }
                else if (RecurrencyTypeEnum.DynamicDay.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.DynamicDay;
                }
                else if (RecurrencyTypeEnum.DynamicHour.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.DynamicHour;
                }
                else if (RecurrencyTypeEnum.DynamicMinute.ToDatabaseString() == recurrencyTypeString)
                {
                    return RecurrencyTypeEnum.DynamicMinute;
                }

            }
            return null;
        }
    }
}
