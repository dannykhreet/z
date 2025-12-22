using EZGO.Api.Models.Enumerations;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Enumerators;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Task;
using RecurrencyTypeEnum = EZGO.Api.Models.Enumerations.RecurrencyTypeEnum;
using WebApp.Logic;
using EZGO.Api.Models.Settings;

namespace WebApp.ViewModels
{
    public class RecurrencyViewModel : TaskRecurrencyModel
    {
        public RecurrencyViewModel()
        {

        }

        public RecurrencyViewModel(TaskRecurrencyModel model)
        {
            if (model != null)
            {
                Id = model.Id;
                CompanyId = model.CompanyId;
                TemplateId = model.TemplateId;
                AreaId = model.AreaId;
                RecurrencyType = model.RecurrencyType;
                if (model.Schedule != null)
                {
                    Schedule = new TaskScheduleViewModel
                    {
                        Date = model.Schedule.Date,
                        Day = model.Schedule.Day ?? 1,
                        EndDate = model.Schedule.EndDate,
                        Month = model.Schedule.Month ?? 1,
                        MonthRecurrencyType = model.Schedule.MonthRecurrencyType,
                        StartDate = model.Schedule.StartDate,
                        Week = model.Schedule.Week ?? 1,
                        WeekDay = model.Schedule.WeekDay,
                        Weekday0 = model.Schedule.Weekday0 ?? false,
                        Weekday1 = model.Schedule.Weekday1 ?? false,
                        Weekday2 = model.Schedule.Weekday2 ?? false,
                        Weekday3 = model.Schedule.Weekday3 ?? false,
                        Weekday4 = model.Schedule.Weekday4 ?? false,
                        Weekday5 = model.Schedule.Weekday5 ?? false,
                        Weekday6 = model.Schedule.Weekday6 ?? false,
                        WeekDayNumber = model.Schedule.WeekDayNumber,
                        IsOncePerMonth = model.Schedule.IsOncePerMonth ?? false,
                        IsOncePerWeek = model.Schedule.IsOncePerWeek ?? false
                    };
                }
                else
                {
                    Schedule = new TaskScheduleViewModel();
                }
                Shifts = model.Shifts;
                Forever = model.Schedule?.StartDate == null && model.Schedule?.EndDate == null;
            }
        }

        public ApplicationSettings ApplicationSettings { get; set; }
        public string Locale { get; set; }
        public new TaskScheduleViewModel Schedule { get; set; }
        public Dictionary<string, string> _CmsLanguage { get; set; }

        public Dictionary<System.DayOfWeek, List<SelectListItem>> ShiftDays { get; set; } = new Dictionary<System.DayOfWeek, List<SelectListItem>>();
        public Dictionary<System.DayOfWeek, List<SelectListItem>> OnlyOnceShiftDays { get; set; } = new Dictionary<System.DayOfWeek, List<SelectListItem>>();

        public SelectList RecurrencyTypes
        {
            get
            {
                var result = RecurrencyType != null ? RecurrencyType.Replace(" ", String.Empty) : "";
                List<RecurrencyTypeEnum> List = new List<RecurrencyTypeEnum> { RecurrencyTypeEnum.NoRecurrency, RecurrencyTypeEnum.Shifts, RecurrencyTypeEnum.Week, RecurrencyTypeEnum.Month, RecurrencyTypeEnum.PeriodDay, RecurrencyTypeEnum.DynamicDay };

                var types = new SelectList(List.Select(v => new SelectListItem
                {
                    Text = getRecurrencyTranslation(v.ToString()),
                    Value = v.ToString().ToLower(),
                    Selected = (v.ToString().ToLower() == result)
                }).ToList(), "Value", "Text", "Selected");

                types ??= new SelectList(new List<SelectListItem>());
                return types;
            }
        }

        private string getRecurrencyTranslation(string input)
        {
            string output = input;
            switch (input)
            {
                case "NoRecurrency":
                    output = _CmsLanguage?.GetValue(LanguageKeys.Task.OptionNoRecurrency, input) ?? input;
                    break;
                case "Week":
                    output = _CmsLanguage?.GetValue(LanguageKeys.Task.OptionWeek, input) ?? input;
                    break;
                case "Month":
                    output = _CmsLanguage?.GetValue(LanguageKeys.Task.OptionMonth, input) ?? input;
                    break;
                case "Shifts":
                    output = _CmsLanguage?.GetValue(LanguageKeys.Task.OptionShifts, input) ?? input;
                    break;
                case "PeriodDay":
                    output = _CmsLanguage?.GetValue(LanguageKeys.Task.OptionPeriodDay, input) ?? input;
                    break;
                case "DynamicDay":
                    output = _CmsLanguage?.GetValue(LanguageKeys.Task.OptionDynamicDay, input) ?? input;
                    break;
            }
            return output;
        }

        public bool Forever { get; set; } = true;
        public int PreviousTaskCount { get; set; }

        public string FromdateName
        {
            get
            {
                if (Schedule != null && Schedule.StartDate.HasValue)
                {
                    return ((EZGO.CMS.LIB.Enumerators.DayOfWeek)Schedule.StartDate.Value.DayOfWeek).ToString().ToLower();
                }
                return ((EZGO.CMS.LIB.Enumerators.DayOfWeek)DateTime.Today.DayOfWeek).ToString().ToLower();
            }
        }

        public List<SelectListItem> WeekDays => weekDays(Schedule, _CmsLanguage);

        private List<SelectListItem> weekDays(TaskScheduleViewModel schedule, Dictionary<string, string> language)
        {
            return new List<SelectListItem> {
                new SelectListItem { Value = "2", Text = language.GetValue(LanguageKeys.Task.OptionMonday, "Monday"), Selected = schedule != null ? (schedule.WeekDay == 2) : false },
                new SelectListItem { Value = "3", Text = language.GetValue(LanguageKeys.Task.OptionTuesday, "Tuesday"), Selected = schedule != null ? (schedule.WeekDay == 3) : false },
                new SelectListItem { Value = "4", Text = language.GetValue(LanguageKeys.Task.OptionWednesday, "Wednesday"), Selected = schedule != null ? (schedule.WeekDay == 4) : false  },
                new SelectListItem { Value = "5", Text = language.GetValue(LanguageKeys.Task.OptionThursday, "Thursday"), Selected = schedule != null ? (schedule.WeekDay) == 5 : false },
                new SelectListItem { Value = "6", Text = language.GetValue(LanguageKeys.Task.OptionFriday, "Friday"), Selected = schedule != null ? (schedule.WeekDay) == 6 : false },
                new SelectListItem { Value = "7", Text = language.GetValue(LanguageKeys.Task.OptionSaturday, "Saturday"), Selected = schedule != null ? (schedule.WeekDay) == 7 : false },
                new SelectListItem { Value = "1", Text = language.GetValue(LanguageKeys.Task.OptionSunday, "Sunday"), Selected = schedule != null ? (schedule.WeekDay) == 1 : false }
            };
        }

        public List<SelectListItem> Months => months(Schedule, _CmsLanguage);

        private List<SelectListItem> months(TaskScheduleViewModel schedule, Dictionary<string, string> language)
        {
            return new List<SelectListItem> {
                new SelectListItem { Value = "1", Text = "1", Selected = schedule != null ? schedule.Month == 1 : false},
                new SelectListItem { Value = "2", Text = "2", Selected = schedule != null ?schedule.Month == 2 : false },
                new SelectListItem { Value = "3", Text = "3", Selected = schedule != null ?schedule.Month == 3 : false },
                new SelectListItem { Value = "4", Text = "4", Selected = schedule != null ?schedule.Month == 4 : false },
                new SelectListItem { Value = "5", Text = "5", Selected = schedule != null ?schedule.Month == 5 : false},
                new SelectListItem { Value = "6", Text = "6", Selected = schedule != null ?schedule.Month == 6 : false},
                new SelectListItem { Value = "7", Text = "7", Selected = schedule != null ?schedule.Month == 7 : false },
                new SelectListItem { Value = "8", Text = "8", Selected = schedule != null ?schedule.Month == 8 : false },
                new SelectListItem { Value = "9", Text = "9", Selected = schedule != null ?schedule.Month == 9 : false },
                new SelectListItem { Value = "10", Text = "10", Selected = schedule != null ?schedule.Month == 10 : false },
                new SelectListItem { Value = "11", Text = "11", Selected = schedule != null ?schedule.Month == 11 : false },
                new SelectListItem { Value = "12", Text = "12", Selected = schedule != null ?schedule.Month == 12 : false },
            };
        }

        public List<SelectListItem> Weeks => weeks(Schedule, _CmsLanguage);

        private List<SelectListItem> weeks(TaskScheduleViewModel schedule, Dictionary<string, string> language)
        {
            return new List<SelectListItem> {
                new SelectListItem { Value = "1", Text = language.GetValue(LanguageKeys.Task.OptionMonthFirst, "First"), Selected = schedule != null ? schedule.Week == 1 : false },
                new SelectListItem { Value = "2", Text = language.GetValue(LanguageKeys.Task.OptionMonthSecond, "Second"), Selected = schedule != null ? schedule.Week == 2 : false },
                new SelectListItem { Value = "3", Text = language.GetValue(LanguageKeys.Task.OptionMonthThird, "Third"), Selected = schedule != null ? schedule.Week == 3 : false },
                new SelectListItem { Value = "4", Text = language.GetValue(LanguageKeys.Task.OptionMonthFourth, "Fourth"), Selected = schedule != null ? schedule.Week == 4 : false }
            };
        }

        public List<SelectListItem> DaysOfMonth => daysOfMonth(Schedule);

        private List<SelectListItem> daysOfMonth(TaskScheduleViewModel schedule)
        {
            var result = new List<SelectListItem>();
            for (int i = 1; i < 32; i++)
            {
                result.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString(),
                    Selected = false//schedule != null ? (i == schedule.Day) : false
                });
            }
            return result;
        }

        public class TaskScheduleViewModel// : EZGO.Api.Models.Schedule
        {

            public string MonthRecurrencyType { get; set; }
            public int Week { get; set; } = 1;
            public int Day { get; set; } = 1;
            public int Month { get; set; } = 1;
            public bool Weekday0 { get; set; }
            public bool Weekday1 { get; set; }
            public bool Weekday2 { get; set; }
            public bool Weekday3 { get; set; }
            public bool Weekday4 { get; set; }
            public bool Weekday5 { get; set; }
            public bool Weekday6 { get; set; }
            public int? WeekDayNumber { get; set; }
            public int? WeekDay { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? Date { get; set; }
            public DateTime? EndDate { get; set; }

            public bool IsOncePerMonth { get; set; }
            public bool IsOncePerWeek { get; set; }
        }
    }
}