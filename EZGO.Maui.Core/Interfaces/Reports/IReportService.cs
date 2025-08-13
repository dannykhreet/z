using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Models.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Reports
{
    public interface IReportService : IDisposable
    {
        Task<List<ReportsCount>> GetMyStatisticsAsync(bool refresh = false, bool isFromSyncService = false);

        Task<List<ReportsCount>> GetTasksCountAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetTasksLastMonthCountAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetTasksCountPerStatePerDateAsync(bool refresh = false, bool isFromSyncService = false);
        Task<ReportTaskDeviationsModel> GetTaskDeviationsAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false);

        Task<List<ReportsAverage>> GetAuditsAverageAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsAverage>> GetAuditsAveragePerDate(int audittemplateid = 0, bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetAuditsCountAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetAuditsCountPerDate(int audittemplateid = 0, bool refresh = false, bool isFromSyncService = false);
        Task<ReportAuditDeviationsModel> GetAuditDeviationsAsync(TimespanTypeEnum period, int audittemplateid = 0, int tasktemplateid = 0, bool refresh = false, bool isFromSyncService = false);

        Task<List<ReportsCount>> GetChecklistItemsCountAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetChecklistsCountAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetChecklistsCountPerDateAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetChecklistsCountPerDayOfYearAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetChecklistsCountPerWeekOfYearAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetChecklistsCountPerMonthOfYearAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetChecklistItemsCountPerStatePerDateAsync(bool refresh = false, bool isFromSyncService = false);
        Task<ReportChecklistDeviationsModel> GetChecklistDeviationsAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false);

        Task<List<ReportsCount>> GetActionsCountPerStateAsync(bool refresh = false, bool isFromSyncService = false);
        Task<ReportsCount> GetActionsCountAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetActionsCountStartedResolvedPerDateAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetActionsCountPerUserAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false);
        Task<List<ReportsCount>> GetActionsCountPerAssignedUserAsync(TimespanTypeEnum period, bool refresh = false, bool isFromSyncService = false);

        List<ReportsCount> GetIntervalCollection(TimespanTypeEnum interval, DateTime date);

        Task<MyEzFeedStats> GetMyEzFeedStatsAsync(bool refresh = false, bool isFromSyncService = false);
    }
}
