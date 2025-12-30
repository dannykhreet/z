using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Reporting
{
    /// <summary>
    /// IExportingManager, interface for all Business Intelligence related calls to the data objects.
    ///
    /// Depending on use all methods return or a Collection< object > or a DataTable for further processing.
    ///
    /// This interface can be used for implementing basic external reporting and raw exports.
    /// </summary>
    public interface IExportingManager
    {
        Task<DataTable> GetTaskOverviewByCompanyAndDateAsync(int companyId, DateTime? from, DateTime? to);
        Task<DataTable> GetTaskDetailsOverviewByCompanyAndDateAsync(int companyId, DateTime? from, DateTime? to);
        Task<DataTable> GetTaskTemplateDetailsOverviewByCompanyAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetTaskTemplateOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetTaskOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetTaskPropertyOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetTaskChecklistPropertyOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetTaskAuditPropertyOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetTaskOverviewCustomerSpecificAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetChecklistAuditOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetAuditOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetChecklistOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetChecklistAuditTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetChecklistTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetAuditTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetWorkInstructionTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetWorkInstructionChangeNotificationsOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetAssessmentTemplatesOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetAssessmentOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetMatrixSkillsUserOverviewByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetActionCommentOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetActionOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetCommentOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataSet> GetAuditingLogOverviewAsync(int companyId, DateTime? from = null, DateTime? to = null, int? timespanInDays = null);
        Task<DataTable> GetLanguageResourcesAsync();
        Task<DataTable> GetManagementCompanyOverview(DateTime? from = null, DateTime? to = null);
        Task<DataTable> GetLanguageImportQueriesAsync();
        Task<DataSet> GetAutomatedTaskExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<DataSet> GetAutomatedChecklistExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<DataSet> GetAutomatedAuditExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<DataSet> GetAutomatedDatawarehouseExport(int holdingId, int companyId, string module, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<bool> ExportToDatawarehouse(int holdingId, int companyId, string storedProcedureName, DateTime? fromTime = null, DateTime? toTime = null);
        Task<DataSet> GetAutomatedDataCollectionExport(int companyId, string module, DateTime? from = null);
        Task<DataTable> GetAutomatedExportAtoss(int holdingId, int companyId, string module, List<int> validCompanyIds);
        Task<DataSet> GetCompanyActiveAreas(int companyid);

    }
}
