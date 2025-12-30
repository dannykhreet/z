using System.Collections.Generic;
using WebApp.Models.Task;

namespace WebApp.ViewModels
{
    public class TasksPerPeriodViewModel
    {
        public Dictionary<TaskStatisticsPeriod, List<EZGO.Api.Models.Reports.TaskStatistics>> TaskStatisticsPerPeriod { get; set; }
        public string Locale { get; set; }
    }
}
