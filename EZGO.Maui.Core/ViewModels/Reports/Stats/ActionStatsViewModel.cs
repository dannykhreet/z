using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.ViewModels.Reports.Stats
{
    public class ActionStatsViewModel : BaseStatsViewModel
    {
        private List<ReportsCount> actionResults;

        public TaskStats ActionsOpen { get; set; }

        public TaskStats ActionsToday { get; set; }

        public TaskStats Actions7Day { get; set; }

        public TaskStats Actions30Day { get; set; }

        public int MyActionsStarted { get; set; }

        public int MyActionsDone { get; set; }

        public int MyActionsOpen { get; set; }

        public int MyActionsPastDue { get; set; }


        public ActionStatsViewModel(bool isRefreshing) : base(isRefreshing) {}

        public override async Task FillStats(List<TaskStats> statsList, List<ReportsCount> myResult)
        {
            await GetActionsCount();

            var actionsOpen = new TaskStats
            {
                Title = ReportsConstants.ActionsOpen,
                NotOk = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.PastDueActions),
                Todo = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.UnresolvedActions),
            };
            actionsOpen.Todo = (actionsOpen.Todo - actionsOpen.NotOk);
            statsList.Add(actionsOpen);

            ActionsToday = new TaskStats
            {
                Title = ReportsConstants.ActionsToday,
                Ok = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.ResolvedActionsToday),
                Todo = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.NewActionsToday),
            };
            Actions7Day = new TaskStats
            {
                Title = ReportsConstants.Actions7Day,
                Ok = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.ResolvedActionsLast7Days),
                Todo = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.NewActionsLast7Days),
            };
            Actions30Day = new TaskStats
            {
                Title = ReportsConstants.Actions30Day,
                Ok = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.ResolvedActionsLast30Days),
                Todo = IResultReportCounter.CountMyResult(actionResults, ReportsConstants.NewActionsLast30Days),
            };

            MyActionsStarted = IResultReportCounter.CountMyResult(myResult, ReportsConstants.MyActionsCreatedByMeTotal);
            MyActionsDone = IResultReportCounter.CountMyResult(myResult, ReportsConstants.MyResolvedActionsCreatedByMe);
            MyActionsOpen = IResultReportCounter.CountMyResult(myResult, ReportsConstants.MyUnResolvedActionsCreatedByMe);
            MyActionsPastDue = IResultReportCounter.CountMyResult(myResult, ReportsConstants.MyPastDueActionsCreatedByMe);

        }

        private async Task GetActionsCount() => actionResults = await reportService.GetActionsCountPerStateAsync(refresh: IsRefreshing);

        protected override void Dispose(bool disposing)
        {
            ActionsOpen = null;
            ActionsToday = null;
            Actions7Day = null;
            Actions30Day = null;
            base.Dispose(disposing);
        }
    }
}
