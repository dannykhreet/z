using EZGO.Api.Models.Reports;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Reporting
{
    public interface IStatisticsManager
    {
        Task<List<StatisticTypeItem>> GetTotalsOverviewByCompanyAsync(int companyId = 0);
        Task<List<StatisticTypeItem>> GetTotalsOverviewByHoldingAsync(int holdingId = 0);

        Task<List<StatisticUserItem>> GetUserActivityTotalsByCompanyAsync(int companyId);

        Task<List<StatisticGenericItem>> GetGenericStatisticsCollectionAsync(int companyId, string storedProcedureReference, DateTime? timestamp = null, int? areaId = null, int? auditTemplateId = null, int? checklistTemplateId = null, int? taskTemplateId = null, int ? timespanInDays = null);

        Task<List<StatisticGenericItem>> GetAverageStatisticsCollectionAsync(int companyId, string storedProcedureReference, int? areaId = null, int? auditTemplateId = null, int? checklistTemplateId = null, int? taskTemplateId = null, int ? timespanInDays = null);

        Task<TasksReport> GetTasksStatisticsAsync(int companyId, DateTime? timestamp = null, int? areaId = null);
        Task<AuditsReport> GetAuditsStatisticsAsync(int companyId, DateTime? timestamp = null, int? areaId = null, int? templateId = null);
        Task<ChecklistsReport> GetChecklistsStatisticsAsync(int companyId, DateTime? timestamp = null, int? areaId = null);
        Task<ActionsReport> GetActionsStatisticsAsync(int companyId, int userId, DateTime? timestamp = null, int? areaId = null);

        Task<ChecklistsReportExtended> GetTaskChecklistsStatisticsExtendedAsync(int companyId, DateTime? timestamp = null, int? areaId = null, string periodType = "last12days", string reportType = "task");
        Task<AuditsReportExtended> GetAuditsStatisticsExtendedAsync(int companyId, DateTime? timestamp = null, int? areaId = null, string periodType = "last12days", int? templateId = null);
        Task<ActionsReportExtended> GetActionsStatisticsExtendedAsync(int companyId, DateTime? timestamp, int? areaId, string periodType = "last12days");

        Task<List<StatisticGenericItem>> GetMyStatisticsCollectionAsync(int companyId, int userId, int? areaId = null, int? timespanInDays = null); //general overview
        
        Task<List<StatisticGenericItem>> GetMyEZFeedStatisticsCollectionAsync(int companyId, int userId); //EZ Feed overview

        Task<List<StatisticGenericItem>> GetMyStatisticsCollectionAsync(int companyId, int userId, string storedProcedureReference, int? areaId = null, int? timespanInDays = null); //dynamic

        Task<List<StatisticGenericItem>> GetLoggingRequestStatisticsCollectionAsync();

        Task<List<StatisticMonthYearItem>> GetDateStatisticsCollectionAsync(int companyId, int holdingId, string storedProcedureReference, DateTime? startDateTime = null, DateTime? endDateTime = null);
        Task<List<StatisticMonthYearItem>> GetDateStatisticsCollectionAsync(string storedProcedureReference, DateTime? startDateTime = null, DateTime? endDateTime = null);

        Task<ActionsCountStatistic> GetActionCountStatistics(int companyId, int? areaId = null, int? timespanInDays = null);
        Task<StatsTotals> GetTotalStatisticsAsync();
        Task<List<CompanyReport>> GetCompanyReports(DateTime startTime, DateTime endTime);
        List<Exception> GetPossibleExceptions();
        Task<StatisticsData> GetStatisticsDataWarehouse(int companyId, int holdingId, string statsReference, DateTime startDateTime, DateTime endDateTime);
    }
}
