using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.ViewModels.Reports.Stats
{
    public class AuditStatsViewModel : BaseStatsViewModel
    {
        public int MyAuditsDone { get; set; }

        public TaskStats AuditToday { get; set; }

        public TaskStats Audit7Day { get; set; }

        public TaskStats Audit30Day { get; set; }

        private List<ReportsAverage> auditavgresults;
        private List<ReportsCount> auditresults;

        public AuditStatsViewModel(bool isRefreshing) : base(isRefreshing)
        {
        }

        private async Task GetAuditsAverageAsync() => auditavgresults = await reportService.GetAuditsAverageAsync(refresh: IsRefreshing);

        private async Task GetAuditsCountAsync() => auditresults = await reportService.GetAuditsCountAsync(refresh: IsRefreshing);

        public override async Task FillStats(List<TaskStats> statsList, List<ReportsCount> myResult)
        {
            await GetAuditsAverageAsync();
            await GetAuditsCountAsync();

            AuditToday = new TaskStats
            {
                Title = IReportNameRetriver.GetNameByConstant(auditavgresults, ReportsConstants.Today),
                Percentage = ITaskPercentageCounter.CalculatePercentage(auditavgresults, ReportsConstants.Today),
                DoneNr = IResultReportCounter.CountMyResult(auditresults, ReportsConstants.CompletedAuditsToday),
            };

            Audit7Day = new TaskStats
            {
                Title = IReportNameRetriver.GetNameByConstant(auditavgresults, ReportsConstants.SevenDays),
                Percentage = ITaskPercentageCounter.CalculatePercentage(auditavgresults, ReportsConstants.SevenDays),
                DoneNr = IResultReportCounter.CountMyResult(auditresults, ReportsConstants.CompletedAuditsLast7Days),
            };

            Audit30Day = new TaskStats
            {
                Title = IReportNameRetriver.GetNameByConstant(auditavgresults, ReportsConstants.ThirtyDays),
                Percentage = ITaskPercentageCounter.CalculatePercentage(auditavgresults, ReportsConstants.ThirtyDays),
                DoneNr = IResultReportCounter.CountMyResult(auditresults, ReportsConstants.CompletedAuditsLast30Days),
            };

            MyAuditsDone = IResultReportCounter.CountMyResult(myResult, ReportsConstants.MyAuditsToday);

        }
    }
}
