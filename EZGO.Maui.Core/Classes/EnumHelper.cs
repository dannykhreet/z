using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;
using System;

namespace EZGO.Maui.Core.Classes
{
    public static class EnumHelper
    {


        public static string GetTranslation(Enum enumValue)
        {
            string result = string.Empty;
            string resourceKey = string.Empty;

            Type enumType = enumValue.GetType();

            if (enumType == typeof(RoleTypeEnum))
            {
                switch (enumValue)
                {
                    case RoleTypeEnum.Basic:
                        resourceKey = LanguageConstants.taskConstructorBasicUserRole;
                        break;
                    case RoleTypeEnum.Manager:
                        resourceKey = LanguageConstants.taskConstructorManagerRole;
                        break;
                    case RoleTypeEnum.ShiftLeader:
                        resourceKey = LanguageConstants.taskConstructorShiftUserRole;
                        break;
                }
            }

            if (enumType == typeof(RecurrencyTypeEnum))
            {
                switch (enumValue)
                {
                    case RecurrencyTypeEnum.NoRecurrency:
                        resourceKey = LanguageConstants.taskConstructorRecurrenceOnce;
                        break;
                    case RecurrencyTypeEnum.Week:
                        resourceKey = LanguageConstants.taskConstructorRecurrenceWeekly;
                        break;
                    case RecurrencyTypeEnum.Month:
                        resourceKey = LanguageConstants.taskConstructorRecurrenceMonthly;
                        break;
                    case RecurrencyTypeEnum.Shifts:
                        resourceKey = LanguageConstants.taskConstructorRecurrenceByShift;
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(resourceKey))
                result = TranslateExtension.GetValueFromDictionary(resourceKey);

            return result;
        }
    }
}
