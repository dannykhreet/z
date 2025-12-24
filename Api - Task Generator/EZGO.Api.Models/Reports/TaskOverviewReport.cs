using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// TaskOverviewReport; Contains a set of data for use with an task-overview page. This object will contain multiple sets of data.
    /// </summary>
    public class TaskOverviewReport
    {
        public int OverDueTasks { get; set; }
        public List<TaskOverviewReportItem> ThisShift { get; set; }
        public List<TaskOverviewReportItem> LastShift { get; set; }
        public List<TaskOverviewReportItem> Shifts { get; set; }
        public List<TaskOverviewReportItem> Today { get; set; }
        public List<TaskOverviewReportItem> TodayTotal { get; set; }
        public List<TaskOverviewReportItem> WeekMonth { get; set; }
        public List<TaskOverviewReportItem> Week { get; set; }
        public List<TaskOverviewReportItem> Month { get; set; }
        public List<TaskOverviewReportItem> LastWeek { get; set; }
        public List<TaskOverviewReportItem> LastMonth { get; set; }
        public List<TaskOverviewReportItem> Yesterday { get; set; }

        public List<TaskOverviewReportItem> Overdue { get; set; }

        public TaskOverviewReport()
        {
            ThisShift = new List<TaskOverviewReportItem>();
            Shifts = new List<TaskOverviewReportItem>();
            Today = new List<TaskOverviewReportItem>();
            TodayTotal = new List<TaskOverviewReportItem>();
            Week = new List<TaskOverviewReportItem>();
            WeekMonth = new List<TaskOverviewReportItem>();
            Month = new List<TaskOverviewReportItem>();
            LastWeek = new List<TaskOverviewReportItem>();
            LastMonth = new List<TaskOverviewReportItem>();
            Yesterday = new List<TaskOverviewReportItem>();
            Overdue = new List<TaskOverviewReportItem>();
        }
    }
}
