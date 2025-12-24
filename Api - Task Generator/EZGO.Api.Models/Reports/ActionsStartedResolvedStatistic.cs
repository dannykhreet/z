using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class ActionsStartedResolvedStatistic
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string LabelText { get; set; }
        public int UnresolvedCount { get; set; }
        public int ResolvedCount { get; set; }
        public int TotalCount { get; set; }
    }
}
