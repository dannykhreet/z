using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Tools
{
    public class CalendarItem
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ItemType { get; set; }
        public string Color { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
