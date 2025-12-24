using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class CalendarSchedule
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ScheduleType { get; set; }
        public List<CalendarDay> Days { get; set; }
    }
}
