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
    /// IAutomatedExportingManager, interface for all Business Intelligence related calls to the data objects.
    ///
    /// Depending on use all methods return or a Collection< object > or a DataTable for further processing.
    ///
    /// This interface can be used for implementing automated exports
    /// </summary>
    public interface IAutomatedExportingManager
    {
        Task<DataSet> GetAutomatedTaskExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<DataSet> GetAutomatedChecklistExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<DataSet> GetAutomatedAuditExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<DataSet> GetAutomatedCustomExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null);
        Task<DataSet> GetAutomatedCustomBackwardsCompatibleExportsOverview(int holdingId, int companyId, DateTime? from = null, DateTime? to = null);
        Task<bool> AddExportLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "");
        Task<DataSet> GetAutomatedDatawarehouseExport(int holdingId, int companyId, string module, DateTime? from = null, DateTime? to = null, string dataType = null);
        Task<DataSet> GetAutomatedDatCollectionExport(int companyId, string module, DateTime? from = null);
        Task<bool> ExportToDatawarehouse(int holdingId, int companyId, string storedProcedureName, DateTime fromTime, DateTime toTime);
        Task<DataTable> RetrieveDataTableSource(int holdingId, int companyId, string storedProcedureName, DateTime fromTime, DateTime toTime);
        Task<bool> SaveDataTable(int holdingId, int companyId, string storedProcedureName, DataTable sourceTable);
        Task<DataTable> GetAutomatedUserExportForAtoss(int holdingId, int companyId, List<int> validHoldingIds, List<int> validCompanyIds, bool checkValidIds = true);
        Task<DataTable> GetAutomatedScoreDataExportForAtoss(int holdingId, int companyId, List<int> validHoldingIds, List<int> validCompanyIds, bool checkValidIds = true);
        Task<DataTable> GetAutomatedMasterDataExportForAtoss(int holdingId, int companyId, List<int> validHoldingIds, List<int> validCompanyIds, bool checkValidIds = true);

    }
}
