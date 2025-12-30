using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class TaskStatisticsOverview
    {
        public TaskCountStatsCompletion ThisShift { get; set; }
        public TaskCountStatsCompletion Today {  get; set; }
        public TaskCountStatsCompletion ThisWeek { get; set; }
        public TaskCountStatsCompletion Overdue { get; set; }
        public TaskCountStatsCompletion PreviousShift {  get; set; }
        public TaskCountStatsCompletion Yesterday { get; set; }
        public TaskCountStatsCompletion PreviousWeek { get; set; }
    }
}
