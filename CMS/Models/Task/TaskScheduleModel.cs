using System;
using System.Collections.Generic;

namespace WebApp.Models.Task
{
    public class TaskScheduleModel// : EZGO.Api.Models.Schedule
    {

        public string MonthRecurrencyType { get; set; }
        public int? Week { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public bool? Weekday0 { get; set; }
        public bool? Weekday1 { get; set; }
        public bool? Weekday2 { get; set; }
        public bool? Weekday3 { get; set; }
        public bool? Weekday4 { get; set; }
        public bool? Weekday5 { get; set; }
        public bool? Weekday6 { get; set; }


        public int? WeekDayNumber { get; set; }
        public int? WeekDay { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }        
        
        // old
        public bool? IsOncePerWeek { get; set; }
        public bool? IsOncePerMonth { get; set; }
    }
}
