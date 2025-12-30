using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class TasksReportExtended
    {
        public List<TasksExecutedStatistic> TasksExecuted { get; set; }
        public List<TaskTappingsStatistic> TaskTappings { get; set; }
        public List<TaskStatusStatistic> TodoStats { get; set; }
        public List<TaskStatusStatistic> NotOkStats { get; set; }
        public List<TaskStatusStatistic> SkippedStats { get; set; }
    }
}
