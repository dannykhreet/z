using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    public class TaskStatusStatistic
    {
        public string TaskName { get; set; }
        public int TemplateId { get; set; }
        public double AveragePercentage { get; set; }
        public int TotalCount { get; set; }
        public int StatusCount { get; set; }
        public int ActionsCount { get; set; }
        public int ResolvedActionsCount { get; set; }
    }
}
