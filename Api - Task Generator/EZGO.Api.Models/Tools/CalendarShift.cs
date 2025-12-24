using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class CalendarShift
    {
        public int ConvertedShiftDay { get; set; }

        public string ConvertedShiftStart { get; set; }

        public string ConvertedShiftEnd { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public int Day { get; set; }

        public int Weekday { get; set; }

        public int? CompanyId { get; set; }

        public int? AreaId { get; set; }

        public int? ShiftNr { get; set; }


    }
}
