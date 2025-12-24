using System;
using System.Collections.Generic;
using System.Text;
using EZGO.Api.Models.Reports;
using EZGO.Api.Models.Stats;

namespace EZGO.Api.Models
{
    public class TasksMetaData
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public TaskCountStatsCompletion Counts { get; set; }
}
}
