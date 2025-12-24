using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class AuditsReportExtended
    {
        public List<ListExecutedStatistic> AuditsExecuted { get; set; }
        public List<AuditAverageScoreStatistic> AuditAverageScores { get; set; }
        public List<TaskStatusStatistic> DeviationStats { get; set; }
        public List<TaskStatusStatistic> SkippedStats { get; set; }
    }
}
