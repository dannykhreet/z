using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class ChecklistsReport
    {
        public ChecklistItemStatusCountStatistic Today { get; set; }
        public ChecklistItemStatusCountStatistic Last7Days { get; set; }
        public ChecklistItemStatusCountStatistic Last30Days { get; set; }
        public int ChecklistsExecutedToday { get; set; }
    }
}
