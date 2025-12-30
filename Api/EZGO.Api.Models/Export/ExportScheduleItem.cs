using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Export
{
    public class ExportScheduleItem
    {
        public int ScheduleRunTime { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public List<int> DayOfWeek { get; set; }
        public int TimeFrameInMinutes { get; set; }
        public int DateDirection { get; set; } // 1: calculation are made to the future; -1: calculation are made to the past
      

    }
}
