using System;
using System.Collections.Generic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.Services.Tasks
{
    public static class TaskFilterOptions
    {
        private static List<string> TodayRecurrecyTypes { get; set; } = new List<string>() {
            RecurrencyTypeEnum.Month.ToString().ToLowerInvariant(),
            RecurrencyTypeEnum.Week.ToString().ToLowerInvariant(),
            RecurrencyTypeEnum.NoRecurrency.ToString().ToLowerInvariant(),
            RecurrencyTypeEnum.PeriodDay.ToString().ToLowerInvariant(),
            RecurrencyTypeEnum.DynamicDay.ToString().ToLowerInvariant(),
        };

        private static List<string> WeekRecurrecyTypes { get; set; } = new List<string>() {
            RecurrencyTypeEnum.Month.ToString().ToLowerInvariant(),
            RecurrencyTypeEnum.Week.ToString().ToLowerInvariant(),
            RecurrencyTypeEnum.PeriodDay.ToString().ToLowerInvariant(),
            RecurrencyTypeEnum.DynamicDay.ToString().ToLowerInvariant(),
        };

        public static bool HasTodayRecurrency(BasicTaskModel task)
        {
            return TodayRecurrecyTypes.Contains(task.RecurrencyType);
        }

        public static bool HasWeekRecurrency(BasicTaskModel task)
        {
            return WeekRecurrecyTypes.Contains(task.RecurrencyType);
        }
    }
}
