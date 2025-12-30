using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.General
{
    public class ShiftDayWeekTimestamps
    {
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public DateTime DayStart { get; set; }
        public DateTime DayEnd { get; set; }
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
    }
}
