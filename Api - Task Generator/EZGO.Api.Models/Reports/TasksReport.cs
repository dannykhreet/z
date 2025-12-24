using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class TasksReport
    {
        public TaskStatusCountStatistic Today { get; set; }
        public TaskStatusCountStatistic Last7Days { get; set; }
        public TaskStatusCountStatistic Last30Days { get; set; }
        public int TasksExecutedToday { get; set; }
        public int TasksSkippedToday { get; set; }
    }
}
