using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Services.Navigation;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.ViewModels.Reports.Stats
{
    public class ChecklistStatsViewModel : BaseStatsViewModel
    {
        public int MyChecklistsDone { get; set; }

        public TaskStats ChecklistsToday { get; set; }

        public TaskStats Checklists7Day { get; set; }

        public TaskStats Checklists30Day { get; set; }

        private List<ReportsCount> checklistitemsresults, checklistresults;

        public ChecklistStatsViewModel(bool isRefreshing) : base(isRefreshing)
        {
        }

        private async Task GetChecklistItemsCountAsync() => checklistitemsresults = await reportService.GetChecklistItemsCountAsync(refresh: IsRefreshing);

        private async Task GetChecklistsCountAsync() => checklistresults = await reportService.GetChecklistsCountAsync(refresh: IsRefreshing);

        public override async Task FillStats(List<TaskStats> statsList, List<ReportsCount> myresults)
        {
            await GetChecklistsCountAsync();
            await GetChecklistItemsCountAsync();

            var checklistsToday = new TaskStats
            {
                Title = ReportsConstants.ChecklistsToday,
                Ok = IResultReportCounter.CountMyResultTextEquals(checklistitemsresults, ReportsConstants.OkToday),
                NotOk = IResultReportCounter.CountMyResultTextEquals(checklistitemsresults, ReportsConstants.NotOkToday),
                Skipped = IResultReportCounter.CountMyResult(checklistitemsresults, ReportsConstants.SkippedToday),
                Todo = IResultReportCounter.CountMyResult(checklistitemsresults, ReportsConstants.ToDoToday),
                DoneNr = IResultReportCounter.CountMyResult(checklistresults, ReportsConstants.CompletedChecklistsToday),
            };
            statsList.Add(checklistsToday);

            var checklists7Day = new TaskStats
            {
                Title = ReportsConstants.Checklists7Day,
                Ok = IResultReportCounter.CountMyResultTextEquals(checklistitemsresults, ReportsConstants.OkLast7Days),
                NotOk = IResultReportCounter.CountMyResultTextEquals(checklistitemsresults, ReportsConstants.NotOkLast7Days),
                Skipped = IResultReportCounter.CountMyResult(checklistitemsresults, ReportsConstants.SkippedLast7Days),
                Todo = IResultReportCounter.CountMyResult(checklistitemsresults, ReportsConstants.ToDoLast7Days),
                DoneNr = IResultReportCounter.CountMyResult(checklistresults, ReportsConstants.CompletedChecklistsLast7Days),
            };
            statsList.Add(checklists7Day);

            var checklists30Day = new TaskStats
            {
                Title = ReportsConstants.Checklists30Day,
                Ok = IResultReportCounter.CountMyResultTextEquals(checklistitemsresults, ReportsConstants.OkLast30Days),
                NotOk = IResultReportCounter.CountMyResultTextEquals(checklistitemsresults, ReportsConstants.NotOkLast30Days),
                Skipped = IResultReportCounter.CountMyResult(checklistitemsresults, ReportsConstants.SkippedLast30Days),
                Todo = IResultReportCounter.CountMyResult(checklistitemsresults, ReportsConstants.ToDoLast30Days),
                DoneNr = IResultReportCounter.CountMyResult(checklistresults, ReportsConstants.CompledtedChecklistLast30Days),
            };
            statsList.Add(checklists30Day);

            MyChecklistsDone = IResultReportCounter.CountMyResult(myresults, ReportsConstants.MyChecklistsToday);

        }

        protected override void Dispose(bool disposing)
        {
            ChecklistsToday = null;
            Checklists7Day = null;
            Checklists30Day = null;
            checklistitemsresults = null;
            checklistresults = null;
            base.Dispose(disposing);
        }
    }
}
