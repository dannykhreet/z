using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class TaskTappingsStatistic
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string LabelText { get; set; }
		public int? OkCount { get; set; }
        public int? NotOkCount { get; set; }
        public int? SkippedCount { get; set; }
        public int? TodoCount { get; set; }
        public int TotalCount { get; set; }
    }
}
