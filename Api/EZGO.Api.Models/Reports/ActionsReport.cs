using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class ActionsReport
    {
        public ActionStatusCountsStatistic Today { get; set; }
        public ActionStatusCountsStatistic Last7Days { get; set; }
        public ActionStatusCountsStatistic Last30Days { get; set; }
        public MyActionsStatusCountsStatistic MyActions { get; set; }
        public int TotalCount { get; set; }
        public int UnresolvedCount { get; set; }
        public int ResolvedCount { get; set; }
        public int OverdueCount { get; set; }
    }
}
