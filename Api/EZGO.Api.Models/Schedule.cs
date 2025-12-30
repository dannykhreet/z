using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Schedule; Schedule object contains a schedule based on certain properties. Depending on the filled in values and type a date can be calculated.
    /// Specific rules can be found with the implementation if needed. Some will be added here later.
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// MonthRecurrencyType;
        /// Used with -> Recurrence type MONTH;
        /// Contains values -> day_of_month, weekday;
        /// A month recurrence has to subtypes:
        /// -> specific week (weekday) -> for every x Monday every x month.
        /// -> specific date (day_of_month) -> every x day of every x month.
        /// DB: [tasks_taskrecurrency.month_recurrency]
        /// </summary>
        public string MonthRecurrencyType { get; set; }
        /// <summary>
        /// Date;
        /// Used with -> Recurrence type NO RECURRENCY;
        /// Contains values -> a specific date.
        /// A no recurrence (or only once) is a task that occurs only on the date as supplied (shifts also need to be chosen).
        /// DB: [tasks_taskrecurrency.date]
        /// </summary>
        public DateTime? Date { get; set; }
        /// <summary>
        /// Week;
        /// Used with -> Recurrence type WEEK
        /// A week recurrence (period) can be set. The occurrence depending on other settings will run every X weeks where X represents this property.
        /// DB: [tasks_taskrecurrency.week]
        /// </summary>
        public int? Week { get; set; }
        /// <summary>
        /// Day;
        /// Used with -> Recurrence type MONTH with Month Recurrence type DAY_OF_MONTH (month specific date)
        /// A day part of a month recurrence (period) can be set. The occurrence depending on other settings will run every Y day of every X months where Y represents this property.
        /// DB: [tasks_taskrecurrency.day]
        /// </summary>
        public int? Day { get; set; }
        /// <summary>
        /// Month;
        /// Used with -> Recurrence type MONTH
        /// A month recurrence (period) can be set. The occurrence depending on other settings will run every X months where X represents this property.
        /// DB: [tasks_taskrecurrency.month]
        /// </summary>
        public int? Month { get; set; }
        /// <summary>
        /// Weekday0;
        /// Used with -> Recurrence type WEEK
        /// A bit value containing true or false and representing Sunday; On this day of this week the task must occur.
        /// DB: [tasks_taskrecurrency.weekday0]
        /// </summary>
        public bool? Weekday0 { get; set; }
        /// <summary>
        /// Weekday1;
        /// Used with -> Recurrence type WEEK
        /// A bit value containing true or false and representing Monday; On this day of this week the task must occur.
        /// DB: [tasks_taskrecurrency.weekday1]
        /// </summary>
        public bool? Weekday1 { get; set; }
        /// <summary>
        /// Weekday2;
        /// Used with -> Recurrence type WEEK
        /// A bit value containing true or false and representing Tuesday; On this day of this week the task must occur.
        /// DB: [tasks_taskrecurrency.weekday2]
        /// </summary>
        public bool? Weekday2 { get; set; }
        /// <summary>
        /// Weekday3;
        /// Used with -> Recurrence type WEEK
        /// A bit value containing true or false and representing Wednesday; On this day of this week the task must occur.
        /// DB: [tasks_taskrecurrency.weekday3]
        /// </summary>
        public bool? Weekday3 { get; set; }
        /// <summary>
        /// Weekday4;
        /// Used with -> Recurrence type WEEK
        /// A bit value containing true or false and representing Thursday; On this day of this week the task must occur.
        /// DB: [tasks_taskrecurrency.weekday4]
        /// </summary>
        public bool? Weekday4 { get; set; }
        /// <summary>
        /// Weekday5;
        /// Used with -> Recurrence type WEEK
        /// A bit value containing true or false and representing Friday; On this day of this week the task must occur.
        /// DB: [tasks_taskrecurrency.weekday5]
        /// </summary>
        public bool? Weekday5 { get; set; }
        /// <summary>
        /// Weekday6;
        /// Used with -> Recurrence type WEEK
        /// A bit value containing true or false and representing Saturday;
        /// DB: [tasks_taskrecurrency.weekday6]
        /// </summary>
        public bool? Weekday6 { get; set; }
        /// <summary>
        /// WeekDay;
        /// Used with -> Recurrence type MONTH with Month Recurrence type WEEKDAY (month specific week)
        /// A day part representing the day of week (starting with 1 for Monday ending on 7 for Sunday) that can occur every 1st,2nd,3rd,4th weekday every x months.
        /// e.g. occur every 2nd Monday of every 2 months.
        /// DB: [tasks_taskrecurrency.weekday]
        /// </summary>
        public int? WeekDay { get; set;  }
        /// <summary>
        /// WeekDayNumber;
        /// Used with -> Recurrence type MONTH with Month Recurrence type WEEKDAY (month specific week)
        /// A weeknumber part representing the x week that a certain day occurs. e.g occur every X Monday of Every Y Months where X is represented by this property.
        /// DB: [tasks_taskrecurrency.weekday_number]
        /// </summary>
        public int? WeekDayNumber { get; set; }
        /// <summary>
        /// IsOncePerWeek; Currently ignored for new templates.
        /// DB: [tasks_taskrecurrency.is_once_per_week]
        /// </summary>
        public bool? IsOncePerWeek { get; set; }
        /// <summary>
        /// IsOncePerMonth; Currently ignored for new templates.
        /// DB: [tasks_taskrecurrency.is_once_per_month]
        /// </summary>
        public bool? IsOncePerMonth { get; set; }
        /// <summary>
        /// StartDate;
        /// Used with -> Recurrence type WEEK, MONTH, SHIFTS
        /// Contains value -> a specific date
        /// A startdate can be set with a recurrence. From this start date all calculation will run for generating new tasks.
        /// These that will only be generate from this start date. If a start date is not supplied the creation date will be used for generation.
        /// DB: [tasks_taskrecurrency.start_date]
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// EndDate;
        /// Used with -> Recurrence type WEEK, MONTH, SHIFTS
        /// Contains value -> a specific date
        /// A enddate can be set with a recurrence. Tasks will be generation up until this date.
        /// If a end  date is not supplied no end date will be set and generation will occur infinitely.
        /// DB: [tasks_taskrecurrency.end_date]
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Minute { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Hour { get; set; }
    }

}
