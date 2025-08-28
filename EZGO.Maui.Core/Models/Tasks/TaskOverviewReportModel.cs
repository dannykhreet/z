using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class TaskOverviewReportModel
    {
        public int OverDueTasks { get; set; }

        public List<TaskOverviewReportItemModel> ThisShift { get; set; }

        public List<TaskOverviewReportItemModel> Shifts { get; set; }

        public List<TaskOverviewReportItemModel> Today { get; set; }

        public List<TaskOverviewReportItemModel> Week { get; set; }

        public List<TaskOverviewReportItemModel> WeekMonth { get; set; }

        public List<TaskOverviewReportItemModel> Month { get; set; }

        public List<TaskOverviewReportItemModel> LastWeek { get; set; }

        public List<TaskOverviewReportItemModel> LastMonth { get; set; }

        public List<TaskOverviewReportItemModel> Yesterday { get; set; }

        public List<TaskOverviewReportItemModel> LastShift { get; set; }

        public TaskOverviewReportModel()
        {
            ThisShift = new List<TaskOverviewReportItemModel>();
            Shifts = new List<TaskOverviewReportItemModel>();
            Today = new List<TaskOverviewReportItemModel>();
            Week = new List<TaskOverviewReportItemModel>();
            Month = new List<TaskOverviewReportItemModel>();
            LastWeek = new List<TaskOverviewReportItemModel>();
            LastMonth = new List<TaskOverviewReportItemModel>();
            Yesterday = new List<TaskOverviewReportItemModel>();
            LastShift = new List<TaskOverviewReportItemModel>();
        }
    }
}
