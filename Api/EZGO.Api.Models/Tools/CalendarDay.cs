using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class CalendarDay
    {
        public int DayNumber { get; set; }
        public string DayName { get; set; }
        public List<CalendarItem> ScheduleItems { get; set; }
    }
}
