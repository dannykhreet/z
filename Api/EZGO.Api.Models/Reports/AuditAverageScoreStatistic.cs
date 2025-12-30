using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class AuditAverageScoreStatistic
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string LabelText { get; set; }
		public double AverageScore { get; set; }
    }
}
