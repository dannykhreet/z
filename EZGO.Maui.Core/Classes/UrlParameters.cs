using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    public class UrlParameters
    {
        public long TaskId { get; set; }
        public int UserId { get; set; }
        public int AssignedUserId { get; set; }
        public int TaskTemplateId { get; set; }
        public TimeSpan TimeStamp { get; set; }
        public int AssignedAreaId { get; set; }
        public int RecurrencyId { get; set; }
    }
}
