using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.ViewModels.Reports.Stats
{
    public class TasksStatsViewModel : BaseStatsViewModel
    {
        private List<ReportsCount> tasksResults;

        public int MyTasksDone { get; set; }

        public int MyTasksSkipped { get; set; }

        public TaskStats TasksToday { get; set; }

        public TaskStats Tasks7Day { get; set; }

        public TaskStats Tasks30Day { get; set; }

        public TasksStatsViewModel(bool isRefreshing) : base(isRefreshing) {}

        private async Task GetTasksCountAsync() => tasksResults = await reportService.GetTasksCountAsync(refresh: IsRefreshing);

        public override async Task FillStats(List<TaskStats> statsList, List<ReportsCount> myResults)
        {
            await GetTasksCountAsync();

            var tasksToday = new TaskStats
            {
                Title = ReportsConstants.TasksToday,
                Ok = IResultReportCounter.CountMyResultTextEquals(tasksResults, ReportsConstants.OkToday),
                NotOk = IResultReportCounter.CountMyResultTextEquals(tasksResults, ReportsConstants.NotOkToday),
                Skipped = IResultReportCounter.CountMyResult(tasksResults, ReportsConstants.SkippedToday),
                Todo = IResultReportCounter.CountMyResult(tasksResults, ReportsConstants.ToDoToday),
            };
            statsList.Add(tasksToday);

            var tasks7Day = new TaskStats
            {
                Title = ReportsConstants.Tasks7Day,
                Ok = IResultReportCounter.CountMyResultTextEquals(tasksResults, ReportsConstants.OkLast7Days),
                NotOk = IResultReportCounter.CountMyResultTextEquals(tasksResults, ReportsConstants.NotOkLast7Days),
                Skipped = IResultReportCounter.CountMyResult(tasksResults, ReportsConstants.SkippedLast7Days),
                Todo = IResultReportCounter.CountMyResult(tasksResults, ReportsConstants.ToDoLast7Days),
            };
            statsList.Add(tasks7Day);

            var tasks30Day = new TaskStats
            {
                Title = ReportsConstants.Tasks30Day,
                Ok = IResultReportCounter.CountMyResultTextEquals(tasksResults, ReportsConstants.OkLast30Days),
                NotOk = IResultReportCounter.CountMyResultTextEquals(tasksResults, ReportsConstants.NotOkLast30Days),
                Skipped = IResultReportCounter.CountMyResult(tasksResults, ReportsConstants.SkippedLast30Days),
                Todo = IResultReportCounter.CountMyResult(tasksResults, ReportsConstants.ToDoLast30Days),
            };
            statsList.Add(tasks30Day);

            MyTasksDone = IResultReportCounter.CountMyResult(myResults, ReportsConstants.MyTasksToday);
            MyTasksSkipped = IResultReportCounter.CountMyResult(myResults, ReportsConstants.MySkippedTasksToday);

        }
    }
}
