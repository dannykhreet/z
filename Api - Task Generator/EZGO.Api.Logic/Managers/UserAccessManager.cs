using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// UserAccessManager; User access manager contains all logic for checking if a user has certain rights (db level) on certain objects. 
    /// These methods will be extended with collection checks. All routes need to have some kind of check based on this information.
    /// </summary>
    public class UserAccessManager : BaseManager<UserAccessManager>, IUserAccessManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        #endregion

        #region - constructor(s) -
        public UserAccessManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationhelper, ILogger<UserAccessManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationhelper;
        }
        #endregion

        /// <summary>
        /// GetAllowedTaskTemplateIdsAsync; Get a list of allowed templates that the user may use.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>A list of Ids</returns>
        public async Task<List<int>> GetAllowedTaskTemplateIdsWithUserAsync(int companyId, int userId)
        {
            var output = await GetAllowedIds(storedProcedureName: "get_user_allowed_tasktemplateids", companyId: companyId, userId: userId);
            await Task.CompletedTask;
            return output;
        }

        /// <summary>
        /// GetAllowedChecklistTemplateIdsAsync; Get a list of allowed templates that the user may use.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>A list of Ids</returns>
        public async Task<List<int>> GetAllowedChecklistTemplateIdsWithUserAsync(int companyId, int userId)
        {
            var output = await GetAllowedIds(storedProcedureName: "get_user_allowed_checklisttemplateids", companyId: companyId, userId: userId);
            return output;
        }

        /// <summary>
        /// GetAllowedAuditTemplateIdsAsync; Get a list of allowed templates that the user may use.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>A list of Ids</returns>
        public async Task<List<int>> GetAllowedAuditTemplateIdsWithUserAsync(int companyId, int userId)
        {
            var output = await GetAllowedIds(storedProcedureName: "get_user_allowed_audittemplateids", companyId: companyId, userId: userId);
            return output;
        }

        /// <summary>
        /// GetAllowedAuditTemplateIdsAsync; Get a list of allowed templates that the user may use.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>A list of Ids</returns>
        public async Task<List<int>> GetAllowedWorkInstructionTemplateIdsWithUserAsync(int companyId, int userId)
        {
            var output = await GetAllowedIds(storedProcedureName: "get_user_allowed_workinstructiontemplateids", companyId: companyId, userId: userId);
            return output;
        }

        /// <summary>
        ///GetAllowedAreaIdsWithUserAsync; Get a list of allowed are ids.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>A list of Ids</returns>
        public async Task<List<int>> GetAllowedAreaIdsWithUserAsync(int companyId, int userId)
        {
            var output = await GetAllowedIds(storedProcedureName: "get_allowedareaids_by_user", companyId: companyId, userId: userId);
            return output;
        }


        /// <summary>
        /// GetAllowedIds; Get allowed ids for a certain type of object in the database. These will be matched againsts the allowed areas for a user.
        /// Sps that can be used.
        /// - get_user_allowed_tasktemplateids
        /// - get_user_allowed_audittemplateids
        /// - get_user_allowed_checklisttemplateids
        /// - get_allowedareaids_by_user
        /// </summary>
        /// <param name="storedProcedureName">Name of stored procedure</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId, (DB: profiles_user.id)</param>
        /// <returns>A list of ids.</returns>
        private async Task<List<int>> GetAllowedIds(string storedProcedureName, int companyId, int userId)
        {
            //Sps that can be used.
            //get_user_allowed_tasktemplateids
            //get_user_allowed_audittemplateids
            //get_user_allowed_checklisttemplateids
            //get_allowedareaids_by_user

            var ids = new List<int>();

            NpgsqlDataReader dr = null;
            

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader(storedProcedureName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("UserManager.GetAllowedIds(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return ids;

        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
