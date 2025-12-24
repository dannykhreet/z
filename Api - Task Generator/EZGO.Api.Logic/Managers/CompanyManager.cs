using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Logic.Exporting;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Companies;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Setup;
using EZGO.Api.Models.Stats;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Crypto;
using EZGO.Api.Utils.Data;
using EZGO.Api.Utils.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

//TODO sort methods, rename all append methods in same structure.

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// CompanyManager; The CompanyManager contains all logic for retrieving and setting companies.
    /// A company is usually a physical location of a customer. (e.g. the 'factory' where things are produced). 
    /// A company is linked to every object in the database. 
    /// A company consists of a name, description, media items. Under a company there is a area tree, a set of users, and one or more data objects (Tasks, Checklists etc.).
    /// In the future a company will be extended with a parent structure (holding) to link all separate companies of a customer together.
    /// </summary>
    public class CompanyManager : BaseManager<CompanyManager>, ICompanyManager
    {
        #region - privates -
        private readonly IMemoryCache _cache;
        private readonly IDatabaseAccessHelper _manager;
        private readonly IShiftManager _shiftManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IGeneralManager _generalManager;
        private readonly IUserManager _userManager;
        private readonly IStatisticsManager _statisticsManager;
        private readonly IFeedManager _feedManager;
        private readonly ICryptography _cryptography;
        private readonly IDataAuditing _dataAuditing;
        #endregion

        #region - constructor(s) -
        public CompanyManager(IDatabaseAccessHelper manager, IDataAuditing dataAuditing, IShiftManager shiftManager, IGeneralManager generalManager, IUserManager userManager, IStatisticsManager statisticsManager, IConfigurationHelper configurationHelper, IFeedManager feedManager, ICryptography cryptography, ILogger<CompanyManager> logger, IMemoryCache memoryCache) : base(logger)
        {
            _cache = memoryCache;
            _manager = manager;
            _shiftManager = shiftManager;
            _configurationHelper = configurationHelper;
            _generalManager = generalManager;
            _userManager = userManager;
            _statisticsManager = statisticsManager;
            _feedManager = feedManager;
            _cryptography = cryptography;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public methods -
        /// <summary>
        /// GetCompaniesAsync; Get a list of companies. 
        /// NOTE! this functionality should only be used within a Management CMS.
        /// </summary>
        /// <returns>A List of companies.</returns>
        public async Task<List<Company>> GetCompaniesAsync(string include = null)
        {
            var output = new List<Company>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                using (dr = await _manager.GetDataReader("get_companies", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var company = CreateOrFillCompanyFromReader(dr);
                        output.Add(company);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompaniesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Any() && !string.IsNullOrEmpty(include))
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.CompanySettings.ToString().ToLower())) output = await AppendSettingsToCompanyAsync(companies: output);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Holding.ToString().ToLower())) output = await AppendHoldingToCompanies(companies: output, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.HoldingUnits.ToString().ToLower())) output = await AppendHoldingUnitsToCompanies(companies: output);
            }

            return output;
        }

        /// <summary>
        /// GetCompaniesFeaturesAsync();
        /// </summary>
        /// <returns>List of CompanyFeatures</returns>
        public async Task<List<CompanyFeatures>> GetCompaniesFeaturesAsync()
        {
            var output = new List<CompanyFeatures>();
            var companies = await GetCompaniesAsync();
            var companiesFeatureResources = await _generalManager.GetFeatureSettingResources();
            var taskGenerationCompaniesResourceSetting = companiesFeatureResources.Where(x => x.SettingsKey == "TECH_TASKGENERATION").FirstOrDefault();
            foreach (var company in companies)
            {
                output.Add(new CompanyFeatures()
                {
                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    Features = await _generalManager.GetFeatures(resourceSettings: companiesFeatureResources, companyId: company.Id),
                    TaskGenerationEnabled = await CheckTaskGeneratorEnabled(companyId: company.Id, settingResource: taskGenerationCompaniesResourceSetting)
                });
            }
            return output;
        }

        /// <summary>
        /// CheckTaskGeneratorEnabled; Check based on settings resource if company has task generation enabled.
        /// </summary>
        /// <param name="companyId">companyId, based on companies_company.id</param>
        /// <param name="settingResource">Resource with values (company id collection)</param>
        /// <returns>true/false if enabled.</returns>
        private async Task<bool> CheckTaskGeneratorEnabled(int companyId, SettingResource settingResource)
        {
            bool found = false;
            if (settingResource != null)
            {
                var possibleCompanyCollection = settingResource.Value.Split(",");
                if (possibleCompanyCollection != null)
                {
                    found = possibleCompanyCollection.Contains(companyId.ToString()) || possibleCompanyCollection.Contains("ALL");

                }
            }
            await Task.CompletedTask;
            return found;
        }

        /// <summary>
        /// GetCompanyAsync; Get company based on the CompanyId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="getCompanyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="include">Include, comma separated string based on IncludesEnum, used for including extra data.</param>
        /// <returns>A Company, depending on include parameter this will also contains a Shift collection.</returns>
        public async Task<Company> GetCompanyAsync(int companyId, int getCompanyId, string include = null)
        {
            var company = new Company();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", getCompanyId));

                using (dr = await _manager.GetDataReader("get_company", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        company = CreateOrFillCompanyFromReader(dr, company: company);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanyAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (company != null && company.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Shifts.ToString().ToLower())) company.Shifts = await GetShiftsWithCompany(company.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Users.ToString().ToLower())) company.Users = await GetUsersWithCompany(company.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Holding.ToString().ToLower())) company = await AppendHoldingToCompany(company: company, companyId: company.Id, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.HoldingUnits.ToString().ToLower())) company = await AppendHoldingUnitsToCompany(company: company, companyId: company.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.CompanySettings.ToString().ToLower())) company = await AppendSettingsToCompanyAsync(company: company);

                return company;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// AddCompanyAsync; Add a Company to the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="company">Company object, containing all relevant Company data to add.(DB: companies_company)</param>
        /// <returns>The identity of the table (DB: companies_company.id)</returns>
        public async Task<int> AddCompanyAsync(int companyId, int userId, Company company)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromCompany(company: company));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_company.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.companies_company.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added company.");
            }

            if (possibleId > 0 && company.HoldingId > 0)
            {
                await SetCompanyHolding(companyId: possibleId, holdingId: company.HoldingId.Value);
            }

            if (company.HoldingUnitIds != null && company.HoldingUnitIds.Any())
            {
                await SetCompanyHoldingUnits(companyId: possibleId, holdingUnitIds: company.HoldingUnitIds);
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeCompanyAsync; Change a Company in the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id) company of the user that mutates the company</param>
        /// <param name="userId">UserId user id which is doing the changing.</param>
        /// <param name="changedCompanyId">CompanyId, id of the object in the database that needs to be updated. (DB: companies_company.id) </param>
        /// <param name="company">Company object containing all data needed for updating the database. (DB: companies_company)</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeCompanyAsync(int companyId, int changedCompanyId, int userId, Company company)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_company.ToString(), changedCompanyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromCompany(company: company, companyId: changedCompanyId));

            var rowseffected = await _manager.ExecuteScalarAsync("change_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

            if (Convert.ToInt32(rowseffected) > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_company.ToString(), changedCompanyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.companies_company.ToString(), objectId: changedCompanyId, userId: userId, companyId: companyId, description: "Changed company.");
            }

            if (changedCompanyId > 0)
            {
                if (company.HoldingId > 0)
                {
                    await SetCompanyHolding(companyId: changedCompanyId, holdingId: company.HoldingId.Value);

                    if (!string.IsNullOrEmpty(company.HoldingCompanySecurityGUID)) { await _generalManager.ChangeSettingResourceCompany(companyid: company.Id, new SettingResourceItem() { CompanyId = company.Id, ResourceId = 71, Value = company.HoldingCompanySecurityGUID }); }
                }
                else if (string.IsNullOrEmpty(company.HoldingCompanySecurityGUID))
                {
                    await RemoveCompanyHoldingAsync(companyId: changedCompanyId, holdingId: await GetCompanyHoldingIdAsync(changedCompanyId));

                    await _generalManager.ChangeSettingResourceCompany(companyid: company.Id, new SettingResourceItem() { CompanyId = company.Id, ResourceId = 71, Value = string.Empty });
                }

            }

            if (company.HoldingUnitIds != null)
            {
                await SetCompanyHoldingUnits(companyId: changedCompanyId, holdingUnitIds: company.HoldingUnitIds);
            }

            return (Convert.ToInt32(rowseffected) > 0);
        }

        /// <summary>
        /// SetCompanyActiveAsync; Set Company active/inactive based on CompanyId.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="companyId">UserId (DB: profiles_user.id)</param>
        /// <param name="inactiveCompanyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a Company to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetCompanyActiveAsync(int companyId, int userId, int inactiveCompanyId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_company.ToString(), inactiveCompanyId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", inactiveCompanyId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_company_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_company.ToString(), inactiveCompanyId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.companies_company.ToString(), objectId: inactiveCompanyId, userId: userId, companyId: companyId, description: "Set company activity state.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// CheckCompany; Check if company exists
        /// </summary>
        /// <param name="name">Company name to check</param>
        /// <param name="companyId">Id current company, if not avaibke reset to 0</param>
        /// <returns>true/false</returns>
        public async Task<bool> CheckCompany(string name, int companyId)
        {
            if (string.IsNullOrEmpty(name)) return false; //no upn supplied, empty email will not be checked.

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_companyname", name));

                return (int)(await _manager.ExecuteScalarAsync("check_companyname", parameters)) > 0;

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred CheckCompany()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {

            }
            return true; //something went wrong, return true.
        }

        /// <summary>
        /// GetShiftsWithCompany; Get the Shifts with a Company. This functionality makes use of the ShiftManager functionality.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A list of Shift objects based on the CompanyId.</returns>
        public async Task<List<Shift>> GetShiftsWithCompany(int companyId)
        {
            var output = await _shiftManager.GetShiftsAsync(companyId: companyId);
            return output;

        }

        /// <summary>
        /// GetUsersWithCompany; Get the Users with a Company. This functionality makes use of the UserManager functionality.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A list of User objects based on the CompanyId.</returns>
        private async Task<List<UserProfile>> GetUsersWithCompany(int companyId)
        {
            var output = await _userManager.GetUserProfilesAsync(companyId: companyId);
            return output;

        }

        /// <summary>
        /// GetCompanyStatistics; Get company statistics for company.
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<CompanyStatistics> GetCompanyStatistics(int companyId, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            var output = new CompanyStatistics();
            output.CompanyBasicStatistics = await _statisticsManager.GetTotalsOverviewByCompanyAsync(companyId: companyId);
            output.ActionCreatedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "actionscount_created_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ActionDueAtStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "actionscount_duedate_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.CommentCreatedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "commentscount_created_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AssessmentsStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "assessmentscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AuditsStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "auditscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ChecklistStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "checklistscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "taskscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksNotOkStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "taskscount_notok_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksOkStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "taskscount_ok_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksSkippedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "taskscount_skipped_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TaskTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "tasktemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ChecklistTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "checklisttemplatecount_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AuditTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "audittemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AssessmentTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "assessmenttemplatecount_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.WorkInstructionTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: companyId, holdingId: 0, storedProcedureReference: "witemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);

            return output;
        }

        public async Task<CompanyStatistics> GetHoldingStatistics(int holdingId, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            var output = new CompanyStatistics();
            output.CompanyBasicStatistics = await _statisticsManager.GetTotalsOverviewByHoldingAsync(holdingId: holdingId);
            output.ActionCreatedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "actionscount_created_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ActionDueAtStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "actionscount_duedate_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.CommentCreatedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "commentscount_created_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AssessmentsStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "assessmentscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AuditsStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "auditscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ChecklistStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "checklistscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "taskscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksNotOkStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "taskscount_notok_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksOkStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "taskscount_ok_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksSkippedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "taskscount_skipped_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TaskTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "tasktemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ChecklistTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "checklisttemplatecount_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AuditTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "audittemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AssessmentTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "assessmenttemplatecount_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.WorkInstructionTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: holdingId, storedProcedureReference: "witemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);

            return output;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task<CompanyStatistics> GetCompanyStatisticsAll(DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            var output = new CompanyStatistics();
            output.CompanyBasicStatistics = await _statisticsManager.GetTotalsOverviewByCompanyAsync();
            output.ActionCreatedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "actionscount_created_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ActionDueAtStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "actionscount_duedate_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.CommentCreatedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "commentscount_created_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AssessmentsStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "assessmentscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AuditsStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "auditscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ChecklistStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "checklistscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "taskscount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksNotOkStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "taskscount_notok_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksOkStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "taskscount_ok_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TasksSkippedStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "taskscount_skipped_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.TaskTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "tasktemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.ChecklistTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "checklisttemplatecount_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AuditTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "audittemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.AssessmentTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "assessmenttemplatecount_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);
            output.WorkInstructionTemplateStatistics = await _statisticsManager.GetDateStatisticsCollectionAsync(companyId: 0, holdingId: 0, storedProcedureReference: "witemplatecount_per_month_year_by_range", startDateTime: startDateTime, endDateTime: endDateTime);

            return output;
        }

        /// <summary>
        /// GetCompanyHoldingIdAsync; Get holding id with company. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <returns>holdingid (with company)</returns>
        public async Task<int> GetCompanyHoldingIdAsync(int companyId)
        {
            if (companyId > 0)
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                var possibleHoldingId = await _manager.ExecuteScalarAsync("get_company_holding_id", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return (possibleHoldingId != null ? Convert.ToInt32(possibleHoldingId) : 0);
            }
            return 0;
        }

        public async Task<DateTime> GetCompanyLocalTime(int companyId, DateTime? timestamp = null)
        {
            timestamp ??= DateTime.UtcNow;
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(await _generalManager.GetSettingValueForCompanyOrHoldingByResourceId(companyid: companyId, resourcesettingid: 1)); // company timezone = 1

            return TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, tzInfo);
        }
        #endregion

        #region - company creation -
        /// <summary>
        /// CreateCompany; Created a new company; Will also create a first user and set basic settings.
        /// </summary>
        /// <param name="company">Company object containing all data</param>
        /// <returns>Incoming object with updated Ids</returns>
        public async Task<SetupCompany> CreateCompany(SetupCompany company, int companyId, int userId)
        {
            NpgsqlDataReader dr = null;

            try
            {
                Authenticator authenticator = new Authenticator();

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_name", company.Name));
                parameters.Add(new NpgsqlParameter("@_description", company.Description));
                parameters.Add(new NpgsqlParameter("@_picture", company.Picture));
                parameters.Add(new NpgsqlParameter("@_email", string.Empty));
                parameters.Add(new NpgsqlParameter("@_firstname", company.PrimaryFirstName));
                parameters.Add(new NpgsqlParameter("@_lastname", company.PrimaryLastName));
                parameters.Add(new NpgsqlParameter("@_username", company.PrimaryUserName));
                parameters.Add(new NpgsqlParameter("@_password", authenticator.GenerateEncryptedPassword(company.PrimaryUserPassword)));

                using (dr = await _manager.GetDataReader("create_company", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: ConnectionKind.Writer))
                {
                    while (await dr.ReadAsync())
                    {
                        company.CompanyId = Convert.ToInt32(dr["company_id"]);
                        company.ManagerId = Convert.ToInt32(dr["manager_id"]);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("Company.CreateCompany(SetupCompany)", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            if (company.CompanyId.HasValue && company.CompanyId.Value > 0)
            {

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.companies_company.ToString(), company.CompanyId.Value);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.companies_company.ToString(), objectId: company.CompanyId.Value, userId: userId, companyId: companyId, description: "Added company.");

                await SetResourceSettings(company: company, companyId: companyId, userId: userId);
                //Update resource settings

            }

            if (company.CompanyId.HasValue && company.CompanyId.Value > 0 && company.ManagerId.HasValue && company.ManagerId.Value > 0)
            {
                //get feeds and add feeds for company if they dont exist
                await TryInitializeEzFeed(company.CompanyId.Value, company.ManagerId.Value);
            }
            if (company.HoldingId.HasValue && company.HoldingId.Value > 0)
            {
                await SetCompanyHolding(companyId: company.CompanyId.Value, holdingId: company.HoldingId.Value);
            }

            if (company.HoldingUnitIds != null && company.HoldingUnitIds.Count > 0)
            {
                await SetCompanyHoldingUnits(companyId: company.CompanyId.Value, holdingUnitIds: company.HoldingUnitIds);
            }


            company.PrimaryUserPassword = string.Empty; //clear password so it is not send back. 

            return company;
        }

        public async Task<bool> SetupCompanySettings(SetupCompanySettings companySettings, int companyId, int userId)
        {
            if (companySettings.CompanyId.HasValue && companySettings.CompanyId.Value > 0)
            {
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    await SetupCompanyResourceSettings(companySettings: companySettings, companyId: companyId, userId: userId);
                }
                catch (Exception ex)
                {
                    return false;
                }
#pragma warning restore CS0168 // Variable is declared but never used
                return true;
            }

            return false;
        }

        /// <summary>
        /// SetResourceSettings; Set specific tier setting and or other company settings from SetupCompany.
        /// </summary>
        /// <param name="company">Company containing all information.</param>
        /// <returns></returns>
        private async Task SetResourceSettings(SetupCompany company, int companyId, int userId)
        {
            //Add resource settings
            if (!string.IsNullOrEmpty(company.TimeZone)) { await _generalManager.ChangeSettingResourceCompany(companyid: company.CompanyId.Value, new SettingResourceItem() { CompanyId = company.CompanyId, ResourceId = 1, Value = company.TimeZone }); }
            if (!string.IsNullOrEmpty(company.Locale)) { await _generalManager.ChangeSettingResourceCompany(companyid: company.CompanyId.Value, new SettingResourceItem() { CompanyId = company.CompanyId, ResourceId = 43, Value = company.Locale }); }
            if (!string.IsNullOrEmpty(company.HoldingCompanySecurityGUID)) { await _generalManager.ChangeSettingResourceCompany(companyid: company.CompanyId.Value, new SettingResourceItem() { CompanyId = company.CompanyId, ResourceId = 71, Value = company.HoldingCompanySecurityGUID }); }

            if (!string.IsNullOrEmpty(company.Country)) { await _generalManager.ChangeSettingResourceCompany(companyid: company.CompanyId.Value, new SettingResourceItem() { CompanyId = company.CompanyId, ResourceId = 124, Value = company.Country }); }
            if (!string.IsNullOrEmpty(company.Coords)) { await _generalManager.ChangeSettingResourceCompany(companyid: company.CompanyId.Value, new SettingResourceItem() { CompanyId = company.CompanyId, ResourceId = 123, Value = company.Coords }); }


            //TODO refactor
            if (!string.IsNullOrEmpty(company.TierLevel))
            {
                //16  Feature Tier Essentials
                if (company.TierLevel == "essential")
                {
                    //FEATURE_TIER_ESSENTIALS : 16
                    await AddCompanyToResourceSetting(company.CompanyId.Value, userId, "FEATURE_TIER_ESSENTIALS", 16);
                }

                //17  Feature Tier Advanced
                if (company.TierLevel == "advanced")
                {
                    //FEATURE_TIER_ADVANCED : 17
                    await AddCompanyToResourceSetting(company.CompanyId.Value, userId, "FEATURE_TIER_ADVANCED", 17);
                }

                //18	Feature Tier Premium
                if (company.TierLevel == "premium")
                {
                    //FEATURE_TIER_PREMIUM : 18
                    await AddCompanyToResourceSetting(company.CompanyId.Value, userId, "FEATURE_TIER_PREMIUM", 18);
                }
            }

            if (company.EnableTaskGeneration)
            {
                //TECH_TASKGENERATION : 27
                await AddCompanyToResourceSetting(company.CompanyId.Value, userId, "TECH_TASKGENERATION", 27);
            }

            if (company.EnableDataWarehouse)
            {
                //TECH_DATAWAREHOUSE : 84
                await AddCompanyToResourceSetting(company.CompanyId.Value, userId, "TECH_DATAWAREHOUSE", 84);
            }

            if (company.EnableWorkInstructionChangesNotifications)
            {
                //FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS : 93
                await AddCompanyToResourceSetting(company.CompanyId.Value, userId, "FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS", 93);
            }
            if (company.EnableMatrixStandardScore)
            {
                //FEATURE_MATRIX_CHANGED_SCORE_STANDARD : 109
                await AddCompanyToResourceSetting(company.CompanyId.Value, userId, "FEATURE_MATRIX_CHANGED_SCORE_STANDARD", 109);
            }
        }

        /// <summary>
        /// SetupCompanyResourceSettings; Set specific tier setting and or other company settings from SetupCompanySettings.
        /// </summary>
        /// <param name="companySettings">CompanySettings containing all information.</param>
        /// <returns></returns>
        private async Task SetupCompanyResourceSettings(SetupCompanySettings companySettings, int companyId, int userId)
        {
            //Add resource settings
            if (!string.IsNullOrEmpty(companySettings.TimeZone)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 1, Value = companySettings.TimeZone }); }
            if (!string.IsNullOrEmpty(companySettings.Locale)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 43, Value = companySettings.Locale }); }

            if (!string.IsNullOrEmpty(companySettings.Country)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 124, Value = companySettings.Country }); }
            if (!string.IsNullOrEmpty(companySettings.MapsJson)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 125, Value = companySettings.MapsJson }); }
            if (!string.IsNullOrEmpty(companySettings.Coords)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 123, Value = companySettings.Coords }); }

            if (!string.IsNullOrEmpty(companySettings.IpRestrictionList)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 131, Value = companySettings.IpRestrictionList }); }
            if (!string.IsNullOrEmpty(companySettings.VirtualTeamLeadModules)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 127, Value = companySettings.VirtualTeamLeadModules }); }
            if (!string.IsNullOrEmpty(companySettings.TranslationModules)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 132, Value = companySettings.TranslationModules }); }
            if (!string.IsNullOrEmpty(companySettings.TranslationLanguages)) { await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 129, Value = companySettings.TranslationLanguages }); }

            //TODO refactor, add clear methods
            if (!string.IsNullOrEmpty(companySettings.TierLevel))
            {
                //16  Feature Tier Essentials
                if (companySettings.TierLevel == "essential")
                {
                    //FEATURE_TIER_ESSENTIALS : 16
                    await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TIER_ESSENTIALS", 16);
                }
                else //remove from essential if it's in there
                {
                    //FEATURE_TIER_ESSENTIALS : 16
                    await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TIER_ESSENTIALS", 16);
                }

                //17  Feature Tier Advanced
                if (companySettings.TierLevel == "advanced")
                {
                    //FEATURE_TIER_ADVANCED : 17
                    await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TIER_ADVANCED", 17);
                }
                else
                {
                    //FEATURE_TIER_ADVANCED : 17
                    await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TIER_ADVANCED", 17);
                }

                //18	Feature Tier Premium
                if (companySettings.TierLevel == "premium")
                {
                    //FEATURE_TIER_PREMIUM : 18
                    await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TIER_PREMIUM", 18);
                }
                else
                {
                    //FEATURE_TIER_PREMIUM : 18
                    await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TIER_PREMIUM", 18);
                }
            }

            if (companySettings.EnableTaskGeneration)
            {
                //TECH_TASKGENERATION : 27
                await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "TECH_TASKGENERATION", 27);
            }
            else
            {
                //TECH_TASKGENERATION : 27
                await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "TECH_TASKGENERATION", 27);
            }

            if (companySettings.EnableDataWarehouse)
            {
                //TECH_DATAWAREHOUSE : 84
                await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "TECH_DATAWAREHOUSE", 84);
            }
            else
            {
                //TECH_DATAWAREHOUSE : 84
                await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "TECH_DATAWAREHOUSE", 84);
            }

            if (companySettings.EnableWorkInstructionChangesNotifications)
            {
                //FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS : 93
                await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS", 93);
            }
            else
            {
                //FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS : 93
                await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_WORK_INSTRUCTIONS_CHANGED_NOTIFICATIONS", 93);
            }

            if (companySettings.EnableSkillsMatrixStandardRound)
            {
                //FEATURE_MATRIX_CHANGED_SCORE_STANDARD : 109
                await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_MATRIX_CHANGED_SCORE_STANDARD", 109);
            }
            else
            {
                //FEATURE_MATRIX_CHANGED_SCORE_STANDARD: 109
                await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_MATRIX_CHANGED_SCORE_STANDARD", 109);
            }

            if(companySettings.EnableIpRestrictions)
            {
                await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_IP_RESTRICTION", 130);
            } else
            {
                await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_IP_RESTRICTION", 130);
            }

            if (companySettings.EnableVirtualTeamLead)
            {
                await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_VIRTUAL_TEAM_LEAD", 126);
            }
            else
            {
                await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_VIRTUAL_TEAM_LEAD", 126);
            }

            if (companySettings.EnableTranslations)
            {
                await AddCompanyToResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TRANSLATE_AUTO", 128);
            }
            else
            {
                await RemoveCompanyFromResourceSetting(companySettings.CompanyId.Value, userId, "FEATURE_TRANSLATE_AUTO", 128);
            }

            if (!string.IsNullOrEmpty(companySettings.SapPmCompanyId))
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 112, Value = companySettings.SapPmCompanyId });
            }
            else
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 112, Value = "" });
            }
            if (!string.IsNullOrEmpty(companySettings.SapPmNotificationOptions))
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 113, Value = companySettings.SapPmNotificationOptions });
            }
            else
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 113, Value = "" });
            }
            if (!string.IsNullOrEmpty(companySettings.SapPmAuthorizationUrl))
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 119, Value = companySettings.SapPmAuthorizationUrl });
            }
            else
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 119, Value = "" });
            }
            if (!string.IsNullOrEmpty(companySettings.SapPmFunctionalLocationUrl))
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 120, Value = companySettings.SapPmFunctionalLocationUrl });
            }
            else
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 120, Value = "" });
            }
            if (!string.IsNullOrEmpty(companySettings.SapPmNotificationUrl))
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 121, Value = companySettings.SapPmNotificationUrl });
            }
            else
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 121, Value = "" });
            }

            if (!string.IsNullOrEmpty(companySettings.SapPmTimezone)) 
            { 
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 133, Value = companySettings.SapPmTimezone }); 
            }
            else
            {
                await _generalManager.ChangeSettingResourceCompany(companyid: companySettings.CompanyId.Value, new SettingResourceItem() { CompanyId = companySettings.CompanyId, ResourceId = 133, Value = "" });
            }

        }

        private async Task AddCompanyToResourceSetting(int companyId, int userId, string settingKey, int settingId)
        {
            var currentTierSetting = await _generalManager.GetSettingResourceByKey(settingKey);
            var updatedValue = string.Empty;
            if (currentTierSetting.Id > 0 && currentTierSetting.Value != "ALL")
            {
                if (!string.IsNullOrEmpty(currentTierSetting.Value))
                {
                    //if currentTierSetting.Value doesn't contain "," then it will return the original string
                    var settingValues = currentTierSetting.Value.Split(",");
                    var companyIds = new List<int>() { companyId };

                    foreach (var settingValue in settingValues)
                    {
                        if (int.TryParse(settingValue, out var settingCompanyId))
                        {
                            companyIds.Add(settingCompanyId);
                        }
                        else
                        {
                            _logger.LogError($"Invalid CompanyId ({settingCompanyId}) value found when adding company to resource setting. Cancelled adding of company to resource setting.");
                            return;
                        }
                    }

                    companyIds = companyIds.Distinct().OrderBy(c => c).ToList();
                    updatedValue = string.Join(",", companyIds);
                }
                else
                {
                    updatedValue = companyId.ToString();
                }

                if (!string.IsNullOrEmpty(updatedValue) && updatedValue != currentTierSetting?.Value && currentTierSetting.Id == settingId) { await _generalManager.ChangeSettingResource(companyId: companyId, userId: userId, id: settingId, value: updatedValue); }
            }
        }
        private async Task RemoveCompanyFromResourceSetting(int companyId, int userId, string settingKey, int settingId)
        {
            var currentTierSetting = await _generalManager.GetSettingResourceByKey(settingKey);
            if (currentTierSetting.Id > 0 && currentTierSetting.Value != "ALL" && currentTierSetting.Value.Contains(companyId.ToString()))
            {
                var originalValue = currentTierSetting.Value.Split(',').ToList();
                var updatedValue = string.Empty;
                if (originalValue.Contains(companyId.ToString()))
                {
                    updatedValue = string.Join(',', originalValue.Where(v => v != companyId.ToString()).ToList());

                    if (!string.IsNullOrEmpty(updatedValue) && currentTierSetting.Id == settingId) { await _generalManager.ChangeSettingResource(companyId: companyId, userId: userId, id: settingId, value: updatedValue); }
                }
            }
        }

        #endregion

        #region - company removal -
        public async Task<bool> RemoveCompany(SetupCompany company, int companyId, int userId)
        {
            var output = false;
            //check if company is filled, and if company not same as companyid so you can't delete the admin company.
            if (!company.CompanyId.HasValue || !(company.CompanyId.Value > 0))
            {
                return false;
            }

            try
            {
                Authenticator authenticator = new Authenticator();

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", company.CompanyId)); //company that needs to be deleted
                parameters.Add(new NpgsqlParameter("@_userid", userId)); //user doing the deletion for validation purposes.
                parameters.Add(new NpgsqlParameter("@_companyname", company.Name)); //name of the company (for validation purposes)

                var result = await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_company_inactive", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                if (result != null)
                {
                    output = Convert.ToInt32(result) > 0;
                }

                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: "{}", Models.Enumerations.TableNames.companies_company.ToString(), objectId: company.CompanyId.Value, userId: userId, companyId: companyId, description: "Deleted company.");

                //TECH_TASKGENERATION : 27, kill task generation if available.
                var currentTaskGenerationSetting = await _generalManager.GetSettingResourceByKey("TECH_TASKGENERATION");
                if (currentTaskGenerationSetting.Id > 0 && currentTaskGenerationSetting.Value != "ALL") //all so system should handle deletion.
                {
                    var updatedValue = string.Empty;
                    if (!string.IsNullOrEmpty(currentTaskGenerationSetting.Value))
                    {
                        var values = currentTaskGenerationSetting.Value.Split(',').ToList();
                        if (values.Count > 0)
                        {
                            if (values.Remove(company.CompanyId.Value.ToString()))
                            {

                                updatedValue = string.Join<string>(",", values);

                                if (!string.IsNullOrEmpty(updatedValue) && currentTaskGenerationSetting.Id == 27) { await _generalManager.ChangeSettingResource(companyId: companyId, userId: userId, id: 27, value: updatedValue); }
                            }
                        }
                    }
                }

                //TECH_DATAWAREHOUSE : 84, kill datawarehouse sync if available
                var currentDatawarehouseSetting = await _generalManager.GetSettingResourceByKey("TECH_DATAWAREHOUSE");
                if (currentDatawarehouseSetting.Id > 0 && currentDatawarehouseSetting.Value != "ALL") //all so system should handle deletion.
                {
                    var updatedValue = string.Empty;
                    if (!string.IsNullOrEmpty(currentDatawarehouseSetting.Value))
                    {
                        var values = currentDatawarehouseSetting.Value.Split(',').ToList();
                        if (values.Count > 0)
                        {
                            if (values.Remove(company.CompanyId.Value.ToString()))
                            {

                                updatedValue = string.Join<string>(",", values);

                                if (!string.IsNullOrEmpty(updatedValue) && currentDatawarehouseSetting.Id == 84) { await _generalManager.ChangeSettingResource(companyId: companyId, userId: userId, id: 84, value: updatedValue); }
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("Company.RemoveCompany(SetupCompany)", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {

            }


            return output;
        }
        #endregion

        #region - private company methods -
        /// <summary>
        /// AppendSettingsToCompanyAsync; Add settings to a specific company object.
        /// </summary>
        /// <param name="companies">List of companies where settings need to be added.</param>
        /// <returns>A list of companies with it's settings.</returns>
        private async Task<List<Company>> AppendSettingsToCompanyAsync(List<Company> companies)
        {
            var companySettings = await GetCompanySettings();

            foreach (var company in companies)
            {
                company.Settings = companySettings.Where(x => x.CompanyId.HasValue && x.CompanyId.Value == company.Id).ToList();
            }

            return companies;
        }

        /// <summary>
        /// AppendSettingsToCompanyAsync; Add settings to a specific company object.
        /// </summary>
        /// <param name="companies">List of companies where settings need to be added.</param>
        /// <returns>A list of companies with it's settings.</returns>
        private async Task<Company> AppendSettingsToCompanyAsync(Company company)
        {
            var companySettings = await GetCompanySettingsForCompany(company.Id);

            company.Settings = companySettings.Where(x => x.CompanyId.HasValue && x.CompanyId.Value == company.Id).ToList();

            return company;
        }

        /// <summary>
        /// GetCompanySettings; Retrieve company settings for all companies.
        /// </summary>
        /// <returns>List of SettingResourceItems.</returns>
        private async Task<List<SettingResourceItem>> GetCompanySettings()
        {
            var output = new List<SettingResourceItem>();

            NpgsqlDataReader dr = null;

            try
            {

                using (dr = await _manager.GetDataReader("get_resource_settings_companies", commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var setting = CreateOrFillCompanySettingFromReader(dr);
                        output.Add(setting);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanySettings(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;

        }

        /// <summary>
        /// GetCompanySettings; Retrieve company settings for one company.
        /// </summary>
        /// <returns>List of SettingResourceItems.</returns>
        private async Task<List<SettingResourceItem>> GetCompanySettingsForCompany(int companyId)
        {
            var output = new List<SettingResourceItem>();

            NpgsqlDataReader dr = null;

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_resource_settings_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var setting = CreateOrFillCompanySettingFromReader(dr);
                        output.Add(setting);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanySettingsForCompany(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;

        }

        /// <summary>
        /// CreateOrFillCompanySettingFromReader; Get company setting from reader.
        /// </summary>
        /// <param name="dr"> DataReader containing the relevant data.</param>
        /// <param name="setting">Setting object, if not supplied it will be created.</param>
        /// <returns></returns>
        private SettingResourceItem CreateOrFillCompanySettingFromReader(NpgsqlDataReader dr, SettingResourceItem setting = null)
        {

            if (setting == null) setting = new SettingResourceItem();

            setting.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value)
            {
                setting.Description = dr["description"].ToString();
            }
            setting.ResourceId = Convert.ToInt32(dr["resource_setting_id"]);
            setting.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableUltimoConnector") && setting.ResourceId == 83) //83 is COMPANY_ULTIMO_API_TOKEN, Ultimo REST API Token
            {
                try
                {
                    setting.Value = _cryptography.Decrypt(dr["value"].ToString());
                }
                catch(Exception ex)
                {

                }
            }
            else
            {
                setting.Value = dr["value"].ToString();
            }

            return setting;
        }


        /// <summary>
        /// CreateOrFillCompanyFromReader; creates and fills a Company object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="company">Company object containing all data needed for updating the database. (DB: companies_company)</param>
        /// <returns>A filled Company object.</returns>
        private Company CreateOrFillCompanyFromReader(NpgsqlDataReader dr, Company company = null)
        {

            if (company == null) company = new Company();

            company.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                company.Description = dr["description"].ToString();
            }
            if (dr["manager_id"] != DBNull.Value)
            {
                company.ManagerId = Convert.ToInt32(dr["manager_id"]);
            }
            company.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                company.Picture = dr["picture"].ToString();
            }

            return company;
        }

        /// <summary>
        /// GetNpgsqlParametersFromCompany; Creates a list of NpgsqlParameters, and fills it based on the supplied Company object.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="company">Company object containing all data needed for updating the database.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromCompany(Company company, int companyId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (companyId > 0) parameters.Add(new NpgsqlParameter("@_id", companyId));

            parameters.Add(new NpgsqlParameter("@_name", company.Name));
            parameters.Add(new NpgsqlParameter("@_description", company.Description));
            parameters.Add(new NpgsqlParameter("@_picture", company.Picture));
            parameters.Add(new NpgsqlParameter("@_managerid", company.ManagerId));

            return parameters;
        }

        /// <summary>
        /// SetCompanyHolding; Set CompanyHolding based on company and holdingid
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <param name="holdingId">HoldingId (DB: holding.id)</param>
        /// <returns></returns>
        private async Task<bool> SetCompanyHolding(int companyId, int holdingId)
        {
            if (companyId > 0 && holdingId > 0)
            {
                int oldHoldingId = await GetCompanyHoldingIdAsync(companyId);

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
                var rowseffected = await _manager.ExecuteScalarAsync("set_company_holding", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                if (oldHoldingId > 0 && oldHoldingId != holdingId)
                {
                    var rowsDeleted = await RemoveAllHoldingTagComapnyRelations(companyId, oldHoldingId);
                }

                if (Convert.ToInt32(rowseffected) > 0)
                {
                    var rowsAdded = await AddAllHoldingTagCompanyRelations(companyId, holdingId);
                }

                return (Convert.ToInt32(rowseffected) > 0);
            }
            return false;
        }

        /// <summary>
        /// SetCompanyHoldingUnits; Save holding unit with a company
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <param name="holdingUnitIds">HoldingUnitId collection (DB: holdingunit.id)</param>
        /// <returns>Nothing</returns>
        private async Task SetCompanyHoldingUnits(int companyId, List<int> holdingUnitIds)
        {
            if (companyId > 0 && holdingUnitIds != null)
            {
                var currentHoldingUnitIds = await GetCompanyHoldingUnitIdsAsync(companyId: companyId);
                //remove all units that are not part of the new holding collection.
                foreach (var holdingUnitId in currentHoldingUnitIds)
                {
                    if (!holdingUnitIds.Contains(holdingUnitId))
                    {
                        await RemoveCompanyHoldingUnitAsync(companyId: companyId, holdingUnitId: holdingUnitId);
                    }

                }

                foreach (var holdingUnitId in holdingUnitIds)
                {
                    if (!currentHoldingUnitIds.Contains(holdingUnitId))
                    {
                        await AddCompanyHoldingUnitAsync(companyId: companyId, holdingUnitId: holdingUnitId);
                    }
                }



            }
        }

        /// <summary>
        /// RemoveCompanyHoldingAsync; Remove company holding relation in DB; Used when decoupling a company from a holding. Normally this will only be used when a company is orphaned. 
        /// If not the normal add functionality can be used. This will update and or insert depending on current data in db.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <param name="holdingId">HoldingId (DB: holding.id)</param>
        /// <returns>true/false depending on outcome</returns>
        private async Task<bool> RemoveCompanyHoldingAsync(int companyId, int holdingId)
        {
            if (companyId > 0 && holdingId > 0)
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
                var rowseffected = await _manager.ExecuteScalarAsync("remove_company_holding", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                if (Convert.ToInt32(rowseffected) > 0)
                {
                    var rowsDeleted = await RemoveAllHoldingTagComapnyRelations(companyId, holdingId);
                }

                return (Convert.ToInt32(rowseffected) > 0);
            }
            return false;
        }

        /// <summary>
        /// RemoveCompanyHoldingUnitAsync; 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <param name="holdingUnitId">HoldingUnitId (DB: holdingunit.id)</param>
        /// <returns>true/false depending on outcome</returns>
        private async Task<bool> RemoveCompanyHoldingUnitAsync(int companyId, int holdingUnitId)
        {
            if (companyId > 0 && holdingUnitId > 0)
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_holdingunitid", holdingUnitId));
                var rowseffected = await _manager.ExecuteScalarAsync("remove_company_holdingunit", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return (Convert.ToInt32(rowseffected) > 0);
            }
            return false;
        }

        /// <summary>
        /// AddCompanyHoldingUnitAsync; Add company holding unit relation. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <param name="holdingUnitId">HoldingUnitId (DB: holdingunit.id)</param>
        /// <returns>true/false depending on outcome</returns>
        private async Task<bool> AddCompanyHoldingUnitAsync(int companyId, int holdingUnitId)
        {
            if (companyId > 0 && holdingUnitId > 0)
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_holdingunitid", holdingUnitId));
                var rowseffected = await _manager.ExecuteScalarAsync("add_company_holdingunit", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                return (Convert.ToInt32(rowseffected) > 0);
            }
            return false;
        }

        /// <summary>
        /// GetCompanyHoldingUnitIdsAsync; Get list of Ids containing all holdingunit ids connected to company. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <returns>List of Ids</returns>
        private async Task<List<int>> GetCompanyHoldingUnitIdsAsync(int companyId)
        {
            var output = new List<int>();

            if (companyId > 0)
            {

                NpgsqlDataReader dr = null;

                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                    using (dr = await _manager.GetDataReader("get_company_holdingunit_ids", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                    {
                        while (await dr.ReadAsync())
                        {
                            output.Add(Convert.ToInt32(dr["holdingunit_id"]));
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanyHoldingUnitIdsAsync(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

                }
                finally
                {
                    if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
                }
            }

            return output;
        }

        /// <summary>
        /// GetCompanyHoldingReleationsAsync; Get a list of relation objects between company and holding.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <returns>List of companyrelationholding objects containing a company and holding id. </returns>
        private async Task<List<CompanyRelationHolding>> GetCompanyHoldingRelationsAsync()
        {
            var output = new List<CompanyRelationHolding>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                using (dr = await _manager.GetDataReader("get_companies_holdings", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var releation = new CompanyRelationHolding() { CompanyId = Convert.ToInt32(dr["company_id"]), HoldingId = Convert.ToInt32(dr["holding_id"]) };
                        output.Add(releation);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanyHoldingUnitIdsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        /// <summary>
        /// GetCompanyHoldingUnitReleationsAsync; Get a list of relation objects between units and companies.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: company_companies.id)</param>
        /// <returns>List of companyrelationholdingunit objects containing a company and holding id. </returns>
        private async Task<List<CompanyRelationHoldingUnit>> GetCompanyHoldingUnitReleationsAsync()
        {
            var output = new List<CompanyRelationHoldingUnit>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                using (dr = await _manager.GetDataReader("get_companies_holdingunits", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var releation = new CompanyRelationHoldingUnit() { CompanyId = Convert.ToInt32(dr["company_id"]), HoldingUnitId = Convert.ToInt32(dr["holdingunit_id"]) };
                        output.Add(releation);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanyHoldingUnitIdsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return output;
        }

        private async Task<int> AddAllHoldingTagCompanyRelations(int companyId, int holdingId)
        {
            List<NpgsqlParameter> addHoldingTagRelationsParameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_holdingid", holdingId)
            };
            var rowsAdded = await _manager.ExecuteScalarAsync("add_all_holding_tag_relations_for_company", parameters: addHoldingTagRelationsParameters, commandType: System.Data.CommandType.StoredProcedure);
            return Convert.ToInt32(rowsAdded);
        }

        private async Task<int> RemoveAllHoldingTagComapnyRelations(int companyId, int holdingId)
        {
            List<NpgsqlParameter> deleteHoldingTagRelationsParameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_holdingid", holdingId)
            };
            var rowsDeleted = await _manager.ExecuteScalarAsync("delete_all_holding_tag_relations_for_company", parameters: deleteHoldingTagRelationsParameters, commandType: System.Data.CommandType.StoredProcedure);
            return Convert.ToInt32(rowsDeleted);
        }

        private async Task<bool> TryInitializeEzFeed(int companyId, int userId)
        {
            var feeds = await _feedManager.GetFeedAsync(companyId: companyId);
            if (feeds.Where(f => f.FeedType == FeedTypeEnum.MainFeed).FirstOrDefault() == null)
            {
                await _feedManager.AddFeedAsync(companyId: companyId,
                    userId: userId,
                    new Models.Feed.FactoryFeed()
                    {
                        Name = "Main Feed",
                        Description = "Main Feed",
                        Attachments = new List<string>(),
                        CompanyId = companyId,
                        DataJson = "",
                        FeedType = EZGO.Api.Models.Enumerations.FeedTypeEnum.MainFeed,
                        Items = new List<Models.Feed.FeedMessageItem>(),
                    });
            }

            if (feeds.Where(f => f.FeedType == FeedTypeEnum.FactoryUpdates).FirstOrDefault() == null)
            {
                await _feedManager.AddFeedAsync(companyId: companyId,
                    userId: userId,
                    new Models.Feed.FactoryFeed()
                    {
                        Name = "Factory Updates",
                        Description = "Factory Updates",
                        Attachments = new List<string>(),
                        CompanyId = companyId,
                        DataJson = "",
                        FeedType = EZGO.Api.Models.Enumerations.FeedTypeEnum.FactoryUpdates,
                        Items = new List<Models.Feed.FeedMessageItem>(),
                    });
            }

            return true;
        }
        #endregion

        #region - roles -
        /// <summary>
        /// GetCompanyRoles; Get roles for a specific company.
        /// </summary>
        /// <param name="companyId">CompanyId where roles should be retrieved</param>
        /// <returns>Roles object containing the roles.</returns>
        public async Task<CompanyRoles> GetCompanyRolesAsync(int companyId)
        {
            var roles = new CompanyRoles();

            NpgsqlDataReader dr = null;


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_company_roles", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        roles.BasicDisplayName = dr["basic_role_display_name"].ToString();
                        roles.ManagerDisplayName = dr["manager_role_display_name"].ToString();
                        roles.ShiftLeaderDisplayName = dr["shift_leader_role_display_name"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanyRoles(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return roles;
        }

        /// <summary>
        /// SetCompanyRoles; Sets the company roles; Currently 3 roles are hardcoded in the database. Only their display name can be changed.
        /// </summary>
        /// <param name="companyId">CompanyId where roles should be set</param>
        /// <param name="roles">Role object containing the roles</param>
        /// <returns>bool false/true depending on outcome.</returns>
        public async Task<bool> SetCompanyRolesAsync(int companyId, CompanyRoles roles)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_basic_role_display_name", roles.BasicDisplayName));
            parameters.Add(new NpgsqlParameter("@_manager_role_display_name", roles.ManagerDisplayName));
            parameters.Add(new NpgsqlParameter("@_shift_leader_role_display_name", roles.ShiftLeaderDisplayName));
            var rowseffected = await _manager.ExecuteScalarAsync("set_company_roles", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            return (Convert.ToInt32(rowseffected) > 0);
        }

        #endregion

        #region - holdings -
        /// <summary>
        /// GetHoldings; Get holdings list. Used in management portal.
        /// </summary>
        /// <param name="companyId">DB: companies_company.id used for validation in query</param>
        /// <param name="userId">DB: companies_company.id used for validation in query</param>
        /// <param name="filters">Possible filters that can be used.</param>
        /// <param name="include">Include parameters: companies is supported.</param>
        /// <returns>returns a list of holdings.</returns>
        public async Task<List<Holding>> GetHoldings(int? companyId = null, int? userId = null, HoldingFilters? filters = null, string include = null)
        {
            var output = new List<Holding>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();


                using (dr = await _manager.GetDataReader("get_holdings", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var holding = CreateOrFillHoldingFromReader(dr);
                        output.Add(holding);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetHoldings(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Any())
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.HoldingUnits.ToString().ToLower())) output = await AppendHoldingUnitsToHoldings(holdings: output);

            }

            return output;

        }

        /// <summary>
        /// GetHolding; Get holding item
        /// </summary>
        /// <param name="holdingId">HoldingId of holding to be retrieved; DB: holding.id</param>
        /// <param name="companyId">DB: companies_company.id used for validation in query</param>
        /// <param name="userId">DB: companies_company.id used for validation in query</param>
        /// <param name="include">Include parameters: companies is supported.</param>
        /// <returns>returns a holding.</returns>
        public async Task<Holding> GetHolding(int holdingId, int? companyId = null, int? userId = null, string include = null)
        {
            var output = new Holding();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("_id", holdingId));

                using (dr = await _manager.GetDataReader("get_holding", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output = CreateOrFillHoldingFromReader(dr);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetHolding(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output != null)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.HoldingUnits.ToString().ToLower())) output = await AppendHoldingUnitsToHolding(holding: output);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.HoldingSettings.ToString().ToLower())) output = await AppendHoldingSettingsToHolding(holding: output);
            }

            return output;


        }

        /// <summary>
        /// AddHoldingAsync; Add a new holding. Companies can be included.
        /// </summary>
        /// <param name="holding"></param>
        /// <param name="companyId">DB: companies_company.id used for validation in query</param>
        /// <param name="userId">DB: companies_company.id used for validation in query</param>
        /// <returns></returns>
        public async Task<int> AddHoldingAsync(Holding holding, int? companyId = null, int? userId = null)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters = GetNpgsqlParametersFromHolding(holding);
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_holding", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                if (!string.IsNullOrEmpty(holding.SecurityGUID))
                {
                    await _generalManager.ChangeSettingResourceHolding(holdingId: possibleId, new SettingResourceItem() { HoldingId = possibleId, ResourceId = 72, Value = holding.SecurityGUID });
                }

                if (!string.IsNullOrEmpty(holding.SapPmAuthorizationUrl))
                {
                    await _generalManager.ChangeSettingResourceHolding(holdingId: possibleId, new SettingResourceItem() { HoldingId = possibleId, ResourceId = 119, Value = holding.SapPmAuthorizationUrl });
                }

                if (!string.IsNullOrEmpty(holding.SapPmFunctionalLocationUrl))
                {
                    await _generalManager.ChangeSettingResourceHolding(holdingId: possibleId, new SettingResourceItem() { HoldingId = possibleId, ResourceId = 120, Value = holding.SapPmFunctionalLocationUrl });
                }

                if (!string.IsNullOrEmpty(holding.SapPmNotificationOptions))
                {
                    await _generalManager.ChangeSettingResourceHolding(holdingId: possibleId, new SettingResourceItem() { HoldingId = possibleId, ResourceId = 113, Value = holding.SapPmNotificationOptions });
                }

                if (!string.IsNullOrEmpty(holding.SapPmNotificationUrl))
                {
                    await _generalManager.ChangeSettingResourceHolding(holdingId: possibleId, new SettingResourceItem() { HoldingId = possibleId, ResourceId = 121, Value = holding.SapPmNotificationUrl });
                }

                if (!string.IsNullOrEmpty(holding.SapPmTimezone))
                {
                    await _generalManager.ChangeSettingResourceHolding(holdingId: possibleId, new SettingResourceItem() { HoldingId = possibleId, ResourceId = 133, Value = holding.SapPmTimezone });
                }
                else
                {
                    await _generalManager.ChangeSettingResourceHolding(holdingId: possibleId, new SettingResourceItem() { HoldingId = possibleId, ResourceId = 133, Value = "" });
                }

                if (holding.CompanyRelations != null && holding.CompanyRelations.Any())
                {
                    //Add relations
                    foreach (var relation in holding.CompanyRelations)
                    {
                        if (relation.CompanyId > 0) await SetCompanyHolding(companyId: relation.CompanyId, possibleId);
                    }
                }
            }

            return possibleId;
        }

        /// <summary>
        /// Change company holding; Companies can be included.
        /// </summary>
        /// <param name="holdingId">HoldingId of holding to be changed</param>
        /// <param name="holding">Holding object to be changed</param>
        /// <param name="companyId">DB: companies_company.id used for validation in query</param>
        /// <param name="userId">DB: companies_company.id used for validation in query</param>
        /// <returns></returns>
        public async Task<bool> ChangeHoldingAsync(int holdingId, Holding holding, int? companyId = null, int? userId = null)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters = GetNpgsqlParametersFromHolding(holding: holding, holdingId: holding.Id);
            var rowseffected = await _manager.ExecuteScalarAsync("change_holding", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

            if (!string.IsNullOrEmpty(holding.SecurityGUID))
            {
                await _generalManager.ChangeSettingResourceHolding(holdingId: holding.Id, new SettingResourceItem() { HoldingId = holding.Id, ResourceId = 72, Value = holding.SecurityGUID });
            }

            if (!string.IsNullOrEmpty(holding.SapPmAuthorizationUrl))
            {
                await _generalManager.ChangeSettingResourceHolding(holdingId: holding.Id, new SettingResourceItem() { HoldingId = holding.Id, ResourceId = 119, Value = holding.SapPmAuthorizationUrl });
            }

            if (!string.IsNullOrEmpty(holding.SapPmFunctionalLocationUrl))
            {
                await _generalManager.ChangeSettingResourceHolding(holdingId: holding.Id, new SettingResourceItem() { HoldingId = holding.Id, ResourceId = 120, Value = holding.SapPmFunctionalLocationUrl });
            }

            if (!string.IsNullOrEmpty(holding.SapPmNotificationOptions))
            {
                await _generalManager.ChangeSettingResourceHolding(holdingId: holding.Id, new SettingResourceItem() { HoldingId = holding.Id, ResourceId = 113, Value = holding.SapPmNotificationOptions });
            }

            if (!string.IsNullOrEmpty(holding.SapPmNotificationUrl))
            {
                await _generalManager.ChangeSettingResourceHolding(holdingId: holding.Id, new SettingResourceItem() { HoldingId = holding.Id, ResourceId = 121, Value = holding.SapPmNotificationUrl });
            }

            if (!string.IsNullOrEmpty(holding.SapPmTimezone))
            {
                await _generalManager.ChangeSettingResourceHolding(holdingId: holding.Id, new SettingResourceItem() { HoldingId = holding.Id, ResourceId = 133, Value = holding.SapPmTimezone });
            }
            else
            {
                await _generalManager.ChangeSettingResourceHolding(holdingId: holding.Id, new SettingResourceItem() { HoldingId = holding.Id, ResourceId = 133, Value = "" });
            }

            if (holding.CompanyRelations != null && holding.CompanyRelations.Any())
            {
                try
                {
                    //Remove removed relations, retrieve all possible existing items and create a list of company ids that are not available in the new relation collection.
                    var companies = (await this.GetCompaniesAsync(include: "holding")).Where(y => y.HoldingId.HasValue && y.HoldingId.Value == holding.Id && !holding.CompanyRelations.Select(x => x.CompanyId).Contains(y.Id));
                    foreach (var company in companies)
                    {
                        await RemoveCompanyHoldingAsync(companyId: company.Id, holdingId: holding.Id);
                    }

                    //Add new relations/update relations
                    foreach (var relation in holding.CompanyRelations)
                    {
                        if (relation.CompanyId > 0) await SetCompanyHolding(companyId: relation.CompanyId, holding.Id);
                    }
                } catch(Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("CompanyManager.ChangeHoldingAsync(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
               
            }

            return (Convert.ToInt32(rowseffected) > 0);
        }

        /// <summary>
        /// SetHoldingActiveAsync; Set a holding active or not
        /// </summary>
        /// <param name="holdingId">HoldingId (DB: holding.id)</param>
        /// <param name="isActive">true/false</param>
        /// <returns>true/false depending on outcome</returns>
        public async Task<bool> SetHoldingActiveAsync(int holdingId, bool isActive = true)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = await _manager.ExecuteScalarAsync("set_holding_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            return (Convert.ToInt32(rowseffected) > 0);
        }

        /// <summary>
        /// GetHoldingUnits; Get a list of HoldingUnits. Depending on useTreeView and or includes these can be extended with other data.
        /// </summary>
        /// <param name="holdingId">HoldingId (DB: holding.id), used when filtering on holding.</param>
        /// <param name="useTreeview">Use a treeview structure or not.</param>
        /// <param name="include">Include extra data, based on the includes enum.</param>
        /// <returns>List of HoldingUnit objects.</returns>
        public async Task<List<HoldingUnit>> GetHoldingUnits(int? holdingId = null, bool useTreeview = true, string include = null)
        {
            var output = new List<HoldingUnit>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                if (holdingId.HasValue && holdingId.Value > 0) parameters.Add(new NpgsqlParameter("@_holdingid", holdingId.Value));

                using (dr = await _manager.GetDataReader("get_holdingunits", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var holdingUnit = CreateOrFillHoldingUnitFromReader(dr);
                        output.Add(holdingUnit);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetHoldingUnits(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (output.Any() && useTreeview)
            {
                output = CreateTree(output);
            }

            return output;
        }

        /// <summary>
        /// AddHoldingUnitAsync; Add a holding unit to the database.
        /// </summary>
        /// <param name="holdingUnit">HoldingUnit containing all information for adding a holding unit to the database.</param>
        /// <param name="companyId">Db: companies_company.id</param>
        /// <param name="userId">UserId (not yet implemented)</param>
        /// <returns>The id of the just inserted object.</returns>
        public async Task<int> AddHoldingUnitAsync(HoldingUnit holdingUnit, int? companyId = null, int? userId = null)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters = GetNpgsqlParametersFromHoldingUnit(holdingUnit);
            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_holdingunit", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return possibleId;
        }

        /// <summary>
        /// ChangeHoldingUnitAsync; Change a holding unit
        /// </summary>
        /// <param name="holdingId">HoldingId</param>
        /// <param name="holdingUnit">HoldingUnit containing all information for changing a holding unit to the database</param>
        /// <param name="companyId">CompanyId of user that is inserting the data (DB: companies_company.id)</param>
        /// <param name="userId">UserId of user that is inserting the data</param>
        /// <returns>true/false depending on outcome</returns>
        public async Task<bool> ChangeHoldingUnitAsync(int holdingId, HoldingUnit holdingUnit, int? companyId = null, int? userId = null)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters = GetNpgsqlParametersFromHoldingUnit(holdingunit: holdingUnit, holdingUnitId: holdingUnit.Id);
            var rowseffected = await _manager.ExecuteScalarAsync("change_holdingunit", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            return (Convert.ToInt32(rowseffected) > 0);
        }

        /// <summary>
        /// SetHoldingUnitActiveAsync; Set a holdingunit active/inactive.
        /// </summary>
        /// <param name="holdingUnitId">HoldingUnitId of the unit where the activity state needs to be set.</param>
        /// <param name="isActive">true/false</param>
        /// <returns>true/false depending on outcome</returns>
        public async Task<bool> SetHoldingUnitActiveAsync(int holdingUnitId, bool isActive = true)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_holdingunitid", holdingUnitId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = await _manager.ExecuteScalarAsync("set_holdingunit_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            return (Convert.ToInt32(rowseffected) > 0);
        }

        /// <summary>
        /// GetCompanyIdsInSameHolding; Get all company ids of the companies in the same holding
        /// </summary>
        /// <param name="holdingId">Id of the company to check the holding for</param>
        /// <returns>list of company ids of companies that are in the same holding as company with companyId</returns>
        public async Task<List<int>> GetCompanyIdsInHolding(int holdingId)
        {
            var output = new List<int>();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("_holdingid", holdingId)
                };

                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_companyids_with_holding", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    output.Add(Convert.ToInt32(dr["id"]));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompanyIdsInSameHolding(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return output;

        }

        /// <summary>
        /// Get list of basic company obbjects based on holding id
        /// </summary>
        /// <param name="holdingId">holding id</param>
        /// <returns>list of basic company basic objects</returns>
        public async Task<List<CompanyBasic>> GetCompaniesInHoldingAsync(int holdingId)
        {
            var output = new List<CompanyBasic>();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("_holdingid", holdingId)
                };

                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_companybasics_in_holding", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    output.Add(new CompanyBasic()
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Name = Convert.ToString(dr["name"]),
                        Picture = Convert.ToString(dr["picture"])
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompaniesInHoldingAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return output;

        }

        /// <summary>
        /// Get list of basic company obbjects based on holding id
        /// </summary>
        /// <param name="holdingId">holding id</param>
        /// <returns>list of basic company basic objects</returns>
        public async Task<List<CompanyBasic>> GetCompaniesInHoldingWithTemplateSharingEnabledAsync(int holdingId)
        {
            var output = new List<CompanyBasic>();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("_holdingid", holdingId)
                };

                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_companybasics_in_holding_for_template_sharing", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    output.Add(new CompanyBasic()
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Name = Convert.ToString(dr["name"]),
                        Picture = Convert.ToString(dr["picture"])
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CompanyManager.GetCompaniesInHoldingAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return output;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holdings"></param>
        /// <returns></returns>
        private async Task<List<Holding>> AppendHoldingUnitsToHoldings(List<Holding> holdings)
        {
            List<HoldingUnit> holdingunits = await GetHoldingUnits(useTreeview: false);

            foreach (var holding in holdings)
            {
                holding.HoldingUnits = CreateTree(holdingunits.Where(x => x.HoldingId == holding.Id).ToList());
            }

            return holdings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holding"></param>
        /// <returns></returns>
        private async Task<Holding> AppendHoldingUnitsToHolding(Holding holding)
        {
            holding.HoldingUnits = await GetHoldingUnits(holdingId: holding.Id);
            return holding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holding"></param>
        /// <returns></returns>
        private async Task<Holding> AppendHoldingSettingsToHolding(Holding holding)
        {
            var settings = await GetHoldingSettingsForHolding(holding.Id);
            if(settings != null && settings.Any())
            {
                if(settings.Where(x => x.ResourceId == 119).Any())
                {
                    holding.SapPmAuthorizationUrl = settings.Where(x => x.ResourceId == 119).FirstOrDefault().Value;
                }
                if (settings.Where(x => x.ResourceId == 120).Any())
                {
                    holding.SapPmFunctionalLocationUrl = settings.Where(x => x.ResourceId == 120).FirstOrDefault().Value;
                }
                if (settings.Where(x => x.ResourceId == 113).Any())
                {
                    holding.SapPmNotificationOptions = settings.Where(x => x.ResourceId == 113).FirstOrDefault().Value;
                }
                if (settings.Where(x => x.ResourceId == 121).Any())
                {
                    holding.SapPmNotificationUrl = settings.Where(x => x.ResourceId == 121).FirstOrDefault().Value;
                }
                if (settings.Where(x => x.ResourceId == 133).Any())
                {
                    holding.SapPmTimezone = settings.Where(x => x.ResourceId == 133).FirstOrDefault().Value;
                }
            }

            return holding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="company"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        private async Task<Company> AppendHoldingToCompany(Company company, int companyId, string include = null)
        {
            if (!company.HoldingId.HasValue) company.HoldingId = await GetCompanyHoldingIdAsync(companyId: companyId);
            if (company.HoldingId.HasValue)
            {
                company.Holding = await GetHolding(holdingId: company.HoldingId.Value, include: include);
                company.HoldingCompanySecurityGUID = await _generalManager.GetSettingValueForCompanyByResourceId(companyid: companyId, resourcesettingid: 71);
            }
            return company;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companies"></param>
        /// <returns></returns>
        private async Task<List<Company>> AppendHoldingToCompanies(List<Company> companies, string include = null)
        {
            if (companies != null && companies.Count > 0)
            {
                List<Holding> holdings = await GetHoldings(include: include);
                List<CompanyRelationHolding> holdingReleations = await GetCompanyHoldingRelationsAsync();
                if (holdings != null && holdings.Count > 0)
                {
                    foreach (var company in companies)
                    {
                        if (!company.HoldingId.HasValue) { company.HoldingId = holdingReleations.Where(x => x.CompanyId == company.Id)?.FirstOrDefault()?.HoldingId; }
                        if (company.HoldingId.HasValue)
                        {
                            company.Holding = holdings.Where(x => x.Id == company.HoldingId.Value).FirstOrDefault();
                        }

                    }
                }
            }
            return companies;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="company"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        private async Task<Company> AppendHoldingUnitsToCompany(Company company, int companyId)
        {
            if (company.Holding != null)
            {
                var companyHoldingIds = await GetCompanyHoldingUnitIdsAsync(companyId: companyId);
                if (companyHoldingIds != null && companyHoldingIds.Any())
                {
                    var holdUnitsFlat = await GetHoldingUnits(holdingId: company.Holding.Id, useTreeview: false);
                    company.HoldingUnits = holdUnitsFlat.Where(x => companyHoldingIds.Contains(x.Id)).ToList();
                }

            }
            return company;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companies"></param>
        /// <returns></returns>
        private async Task<List<Company>> AppendHoldingUnitsToCompanies(List<Company> companies)
        {
            if (companies != null && companies.Count > 0)
            {
                var companiesHoldingUnitRelations = await GetCompanyHoldingUnitReleationsAsync();
                var companiesHoldingUnits = await GetHoldingUnits(useTreeview: false);
                foreach (var company in companies)
                {
                    var localcollection = companiesHoldingUnitRelations.Where(x => x.CompanyId == company.Id);
                    if (localcollection.Any())
                    {
                        company.HoldingUnits = companiesHoldingUnits.Where(x => localcollection.Select(y => y.HoldingUnitId).Contains(x.Id)).ToList();
                    }
                }
            }
            return companies;
        }

        /// <summary>
        /// Retrieves the list of resource settings associated with the specified holding.
        /// </summary>
        /// <remarks>This method executes asynchronously and queries the underlying data store for
        /// resource settings linked to the given holding. The caller should await the returned task to obtain the
        /// results.</remarks>
        /// <param name="holdingId">The unique identifier of the holding for which to retrieve resource settings.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="SettingResourceItem"/> objects representing the resource settings for the specified holding. The list
        /// is empty if no settings are found.</returns>
        private async Task<List<SettingResourceItem>> GetHoldingSettingsForHolding(int holdingId)
        {
            var output = new List<SettingResourceItem>();

            NpgsqlDataReader dr = null;

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_holdingid", holdingId));

                using (dr = await _manager.GetDataReader("get_resource_settings_holding", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        var setting = CreateOrFillHoldingSettingFromReader(dr);
                        output.Add(setting);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("HoldingManager.GetHoldingSettingsForHolding(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;

        }

        /// <summary>
        /// Creates a new <see cref="SettingResourceItem"/> instance or populates an existing one with values from the
        /// specified <see cref="NpgsqlDataReader"/>.
        /// </summary>
        /// <remarks>The method reads values for <c>Id</c>, <c>Description</c>, <c>ResourceId</c>,
        /// <c>HoldingId</c>, and <c>Value</c> from the data reader.  If the application setting
        /// <c>AppSettings:EnableUltimoConnector</c> is enabled and the <c>ResourceId</c> is 83, the <c>Value</c>
        /// property is decrypted before assignment.</remarks>
        /// <param name="dr">The <see cref="NpgsqlDataReader"/> containing the data to populate the <see cref="SettingResourceItem"/>.
        /// Must not be <c>null</c>.</param>
        /// <param name="setting">An optional existing <see cref="SettingResourceItem"/> to populate. If <c>null</c>, a new instance is
        /// created.</param>
        /// <returns>A <see cref="SettingResourceItem"/> populated with values from the data reader.</returns>
        private SettingResourceItem CreateOrFillHoldingSettingFromReader(NpgsqlDataReader dr, SettingResourceItem setting = null)
        {

            if (setting == null) setting = new SettingResourceItem();

            setting.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value)
            {
                setting.Description = dr["description"].ToString();
            }
            setting.ResourceId = Convert.ToInt32(dr["resource_setting_id"]);
            setting.HoldingId = Convert.ToInt32(dr["holding_id"]);
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableUltimoConnector") && setting.ResourceId == 83) //83 is COMPANY_ULTIMO_API_TOKEN, Ultimo REST API Token
            {
                try
                {
                    setting.Value = _cryptography.Decrypt(dr["value"].ToString());
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                setting.Value = dr["value"].ToString();
            }

            return setting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="holding"></param>
        /// <returns></returns>
        private Holding CreateOrFillHoldingFromReader(NpgsqlDataReader dr, Holding holding = null)
        {
            if (holding == null) holding = new Holding();

            holding.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                holding.Description = dr["description"].ToString();
            }
            holding.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                holding.Picture = dr["picture"].ToString();
            }
            if (dr["securityguid"] != DBNull.Value && !string.IsNullOrEmpty(dr["securityguid"].ToString()))
            {
                holding.SecurityGUID = dr["securityguid"].ToString();
            }

            return holding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="holdingunit"></param>
        /// <returns></returns>
        private HoldingUnit CreateOrFillHoldingUnitFromReader(NpgsqlDataReader dr, HoldingUnit holdingunit = null)
        {
            if (holdingunit == null) holdingunit = new HoldingUnit();

            holdingunit.Id = Convert.ToInt32(dr["id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                holdingunit.Description = dr["description"].ToString();
            }
            holdingunit.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                holdingunit.Picture = dr["picture"].ToString();
            }
            if (dr["holding_id"] != DBNull.Value)
            {
                holdingunit.HoldingId = Convert.ToInt32(dr["holding_id"]);
            }
            if (dr["parent_id"] != DBNull.Value)
            {
                holdingunit.ParentId = Convert.ToInt32(dr["parent_id"]);
            }
            return holdingunit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holding"></param>
        /// <param name="holdingId"></param>
        /// <returns></returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromHolding(Holding holding, int holdingId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (holdingId > 0) parameters.Add(new NpgsqlParameter("@_id", holdingId));
            if (string.IsNullOrEmpty(holding.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_description", holding.Description));
            }

            parameters.Add(new NpgsqlParameter("@_name", holding.Name));
            if (string.IsNullOrEmpty(holding.Picture))
            {
                parameters.Add(new NpgsqlParameter("@_picture", DBNull.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_picture", holding.Picture));
            }
            return parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holdingunit"></param>
        /// <param name="holdingUnitId"></param>
        /// <returns></returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromHoldingUnit(HoldingUnit holdingunit, int holdingUnitId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (holdingUnitId > 0) parameters.Add(new NpgsqlParameter("@_id", holdingUnitId));
            if (string.IsNullOrEmpty(holdingunit.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_description", holdingunit.Description));
            }

            parameters.Add(new NpgsqlParameter("@_name", holdingunit.Name));
            if (string.IsNullOrEmpty(holdingunit.Picture))
            {
                parameters.Add(new NpgsqlParameter("@_picture", DBNull.Value));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_picture", holdingunit.Picture));
            }
            if (holdingunit.ParentId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_parentid", holdingunit.ParentId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_parentid", DBNull.Value));
            }
            parameters.Add(new NpgsqlParameter("@_holdingid", holdingunit.HoldingId));

            return parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holdingUnits"></param>
        /// <returns></returns>
        private List<HoldingUnit> CreateTree(List<HoldingUnit> holdingUnits)
        {
            var output = new List<HoldingUnit>();

            foreach (var item in holdingUnits)
            {
                var currentHoldingUnit = item;
                if (item.ParentId > 0)
                {
                    var foundItem = FindRecursivelyHoldingUnitInListByParentId(output, item.ParentId);
                    if (foundItem != null)
                    {
                        if (foundItem.HoldingUnits == null) foundItem.HoldingUnits = new List<HoldingUnit>();
                        foundItem.HoldingUnits.Add(currentHoldingUnit);
                    }
                    else
                    {
                        output.Add(currentHoldingUnit);
                    }
                }
                else
                {
                    output.Add(currentHoldingUnit); //root item, does not have a parent.
                }
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="holdingUnits"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private HoldingUnit FindRecursivelyHoldingUnitInListByParentId(List<HoldingUnit> holdingUnits, int parentId)
        {
            if (holdingUnits.Where(x => x.Id == parentId).Any())
            {
                return holdingUnits.Where(x => x.Id == parentId).FirstOrDefault();
            }
            else
            {
                foreach (var item in holdingUnits)
                {
                    if (item.HoldingUnits != null)
                    {
                        var foundItem = FindRecursivelyHoldingUnitInListByParentId(item.HoldingUnits, parentId);
                        if (foundItem != null)
                        {
                            return foundItem;
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_shiftManager.GetPossibleExceptions());
                listEx.AddRange(_generalManager.GetPossibleExceptions());
                listEx.AddRange(_userManager.GetPossibleExceptions());
                listEx.AddRange(_statisticsManager.GetPossibleExceptions());
                listEx.AddRange(_feedManager.GetPossibleExceptions());
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }
        #endregion
    }
}
