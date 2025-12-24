using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Companies;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Setup;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// ICompanyManager, Interface for use with the CompanyManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface ICompanyManager
    {
        Task<List<Company>> GetCompaniesAsync(string include = null);
        Task<List<CompanyFeatures>> GetCompaniesFeaturesAsync();
        Task<Company> GetCompanyAsync(int companyId, int getCompanyId, string include = null); //todo rename param
        Task<CompanyRoles> GetCompanyRolesAsync(int companyId);
        Task<int> AddCompanyAsync(int companyId, int userId, Company company);
        Task<bool> ChangeCompanyAsync(int companyId, int changedCompanyId, int userId, Company company);
        Task<bool> SetCompanyActiveAsync(int companyId, int userId, int inactiveCompanyId, bool isActive = true);
        Task<bool> RemoveCompany(SetupCompany company, int companyId, int userId);
        Task<bool> SetCompanyRolesAsync(int companyId, CompanyRoles roles);
        Task<SetupCompany> CreateCompany(SetupCompany company, int companyId, int userId);
        Task<bool> SetupCompanySettings(SetupCompanySettings companySettings, int companyId, int userId);
        Task<bool> CheckCompany(string name, int companyId);
        Task<List<Holding>> GetHoldings(int? companyId = null, int? userId = null, HoldingFilters? filters = null, string include = null);
        Task<Holding> GetHolding(int holdingId, int? companyId = null, int? userId = null, string include = null);
        Task<int> AddHoldingAsync(Holding holding, int? companyId = null, int? userId = null);
        Task<bool> ChangeHoldingAsync(int holdingId, Holding holding, int? companyId = null, int? userId = null);
        Task<bool> SetHoldingActiveAsync(int holdingId, bool isActive = true);
        Task<List<HoldingUnit>> GetHoldingUnits(int? holdingId = null, bool useTreeview = true, string include = null);
        Task<int> AddHoldingUnitAsync(HoldingUnit holdingUnit, int? companyId = null, int? userId = null);
        Task<bool> ChangeHoldingUnitAsync(int holdingId, HoldingUnit holdingUnit, int? companyId = null, int? userId = null);
        Task<bool> SetHoldingUnitActiveAsync(int holdingUnitId, bool isActive = true);
        Task<List<int>> GetCompanyIdsInHolding(int companyId);
        Task<List<CompanyBasic>> GetCompaniesInHoldingAsync(int holdingId);
        Task<List<CompanyBasic>> GetCompaniesInHoldingWithTemplateSharingEnabledAsync(int holdingId);
        Task<CompanyStatistics> GetCompanyStatistics(int companyId, DateTime? startDateTime = null, DateTime? endDateTime = null);
        Task<CompanyStatistics> GetHoldingStatistics(int holdingId, DateTime? startDateTime = null, DateTime? endDateTime = null);
        Task<CompanyStatistics> GetCompanyStatisticsAll(DateTime? startDateTime = null, DateTime? endDateTime = null);
        Task<int> GetCompanyHoldingIdAsync(int companyId);
        List<Exception> GetPossibleExceptions();
        Task<DateTime> GetCompanyLocalTime(int companyId, DateTime? timestamp = null);
    }
}
