using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Data
{
    public interface IExportingDataManager
    {
        Task<DataTable> GetTasksDataTableByCompanyAndDateAsync(int companyid, DateTime from, DateTime to);
        Task<DataTable> GetTasksDetailsDataTableByCompanyAndDateAsync(int companyid, DateTime from, DateTime to);
        Task<DataTable> GetTaskTemplateDetailsDataTableByCompanyAsync(int companyid, DateTime? from = null, DateTime? to = null);
        Task<DataTable> GetTaskTemplateOverviewDataTableByCompanyAsync(int companyId, ExportTaskTemplateOverviewTypeEnum overviewType, DateTime? from = null, DateTime? to = null);
    }
}
