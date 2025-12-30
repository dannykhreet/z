using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class ListExecutedStatistic
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string LabelText { get; set; }
        public int? ExecutedCount { get; set; }
        public int? TaskExecutedCount { get; set; }
        public int? PlannedCount { get; set; }
    }
}
