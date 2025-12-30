using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class TaskCountStatsCompletion
    {
        public int TotalCount { get; set; }
        public int TodoCount { get; set; }
        public int SkippedCount { get; set; }
        public int NotOkCount { get; set; }
        public int OkCount { get; set; }
    }
}
