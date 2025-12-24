using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.General
{
    /// <summary>
    /// Dashboard; Dashboard object used for setting up a dashboard page. 
    /// Contains a set of collections that can be displayed. Normally these are statistics and top 5 / top 3 items.
    /// </summary>
    public class Dashboard
    {
        public List<Announcement> Announcements { get; set; }
        public StatsTotals StatisticTotals { get; set; }
        public List<Checklist> CompletedChecklists { get; set; }
        public List<Audit> CompletedAudits { get; set; }
        public List<TasksTask> CompletedTasks { get; set; }
        public List<ActionsAction> Actions { get; set; }
        public List<StatisticTypeItem> CompanyOverview { get; set; }
    }
}
