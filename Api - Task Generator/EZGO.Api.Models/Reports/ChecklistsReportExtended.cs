using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class ChecklistsReportExtended
    {
        public List<ListExecutedStatistic> ExecutionStats { get; set; }
        public List<TaskTappingsStatistic> ItemTappings { get; set; }
        public List<TaskStatusStatistic> NotOkStats { get; set; }
        public List<TaskStatusStatistic> SkippedStats { get; set; }
        public List<TaskStatusStatistic> TodoStats { get; set; }
    }
}
