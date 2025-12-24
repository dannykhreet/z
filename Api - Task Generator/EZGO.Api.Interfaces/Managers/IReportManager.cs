using EZGO.Api.Models.Reports;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{

    /// <summary>
    /// IReportManager, Interface for use with the ReportManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IReportManager
    {
        Task<TaskOverviewReport> GetTaskOverviewReportAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);
        Task<TaskOverviewReport> GetPastTaskOverviewReportAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);
        Task<List<TaskOverviewReportItem>> GetPastTaskOverviewReportPreviousShiftAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);
        Task<List<TaskOverviewReportItem>> GetPastTaskOverviewReportLastWeekAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);
        Task<List<TaskOverviewReportItem>> GetPastTaskOverviewReportYesterdayAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);
        Task<TaskOverviewReport> GetCurrentTaskOverviewReportAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);
        Task<int> GetOverdueTaskReportsAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);
        Task<List<TaskOverviewReportItem>> GetTaskOverdueReportsAsync(int companyId, int? userId = null, int? areaid = null, DateTime? timestamp = null, bool? allowedOnly = null);

        Task<ReportAuditDeviations> GetAuditsDeviationReportAsync(int companyId, int? auditTemplateId = null, int? taskTemplateId = null, int? areaId = null, int? timespanInDays = null);

        Task<ReportChecklistDeviations> GetChecklistDeviationReportAsync(int companyId, int? checklistTemplateId = null, int? taskTemplateId = null, int? areaId = null, int? timespanInDays = null);

        Task<ReportTaskDeviations> GetTasksDeviationReportAsync(int companyId, int? areaId = null, int? timespanInDays = null);
        List<Exception> GetPossibleExceptions();
    }
}
