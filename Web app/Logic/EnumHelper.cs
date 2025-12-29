using EZGO.Api.Models.Enumerations;
using EZGO.CMS.LIB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Logic
{
    public static class EnumHelper
    {
        public static string GetTranslation(Enum enumValue, Dictionary<string, string> CmsLanguage)
        {
            string result = string.Empty;
            string resourceKey = string.Empty;

            Type enumType = enumValue.GetType();

            if (enumType == typeof(RoleTypeEnum))
            {
                switch (enumValue)
                {
                    case RoleTypeEnum.Basic:
                        resourceKey = "TASK_CONSTRUCTOR_BASIC_USER_ROLE";
                        break;
                    case RoleTypeEnum.Manager:
                        resourceKey = "TASK_CONSTRUCTOR_MANAGER_ROLE";
                        break;
                    case RoleTypeEnum.ShiftLeader:
                        resourceKey = "TASK_CONSTRUCTOR_SHIFT_USER_ROLE";
                        break;
                }
            }

            if (enumType == typeof(RecurrencyTypeEnum))
            {
                switch (enumValue)
                {
                    case RecurrencyTypeEnum.NoRecurrency:
                        resourceKey = "TASK_CONSTRUCTOR_RECURRENCE_ONCE";
                        break;
                    case RecurrencyTypeEnum.Week:
                        resourceKey = "TASK_CONSTRUCTOR_RECURRENCE_WEEKLY";
                        break;
                    case RecurrencyTypeEnum.Month:
                        resourceKey = "TASK_CONSTRUCTOR_RECURRENCE_MONTHLY";
                        break;
                    case RecurrencyTypeEnum.Shifts:
                        resourceKey = "TASK_CONSTRUCTOR_RECURRENCE_BY_SHIFT";
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(resourceKey))
            {
                result = CmsLanguage.GetValue(resourceKey, resourceKey);
            }

            return result;
        }
    }
}
