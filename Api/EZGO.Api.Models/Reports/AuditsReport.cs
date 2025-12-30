using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class AuditsReport
    {
        public AuditScoreStatistic Today { get; set; }
        public AuditScoreStatistic Last7Days { get; set; }
        public AuditScoreStatistic Last30Days { get; set; }
        public int AuditsExecutedToday { get; set; }
    }
}
