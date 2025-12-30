using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.ViewModels
{
    public class TaskGenerationShiftViewModel
    {
        public int Id { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        //public int Day { get; set; }
        public int Weekday { get; set; } //  // 0 to 6 -> monday to sunday
        //public int? CompanyId { get; set; }
        public int? AreaId { get; set; }
        //public int? ShiftNr { get; set; }
        public DayOfWeek DayOfWeek => (DayOfWeek)(Weekday + 1 < 7 ? Weekday + 1 : 0);
        override public string ToString()
        {
            var startTime = string.Join(":", Start.Split(":").Take(2));
            var endTime = string.Join(":", End.Split(":").Take(2));
            return startTime + "-" + endTime;
        }
    }
}
