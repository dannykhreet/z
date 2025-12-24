using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// VersionReleaseManager; Contains functionalities for saving versions (static) of certain objects.
    /// </summary>
    public class VersionReleaseManager : BaseManager<VersionReleaseManager>, IVersionReleaseManager
    {

        #region - private(s) -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private const string AUDIT_JSON_DATA_VERSION = "v1";
        private const string CHECKLIST_JSON_DATA_VERSION = "v1";
        private const string TASK_JSON_DATA_VERSION = "v1";
        #endregion

        #region - constructor -
        public VersionReleaseManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, ILogger<VersionReleaseManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
        }
        #endregion

        #region - check methods -
        /// <summary>
        /// NOTE NOT YET IMPLEMENTED 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns></returns>
        public async Task<bool> CheckStaticAuditExistsAsync(int id, int companyId)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// NOTE NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns></returns>
        public async Task<bool> CheckStaticChecklistExistsAsync(int id, int companyId)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// NOTE NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns></returns>
        public async Task<bool> CheckStaticTaskExistsAsync(int id, int companyId)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// NOT YET IMPMENETED;
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<bool> CheckStaticAssessmentExistsAsync(int id, int companyId)
        {
            await Task.CompletedTask;
            return false;
        }
        #endregion

        #region - methods -
        /// <summary>
        /// SaveStaticAudit; Saves audit as JSON to create a static version;
        /// </summary>
        /// <param name="auditJson">Static version of a audit in json format</param>
        /// <param name="id">Id of the object that is being saved. (used for references)</param>
        /// <param name="userId">UserId of the user that does the saving.</param>
        /// <param name="companyId">CompanyId of the user that does the saving</param>
        /// <returns>return true/false</returns>
        public async Task<bool> SaveStaticAuditAsync(string auditJson, int id, int userId, int companyId)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_auditid", id));
                parameters.Add(new NpgsqlParameter("@_jsonobject", auditJson));
                parameters.Add(new NpgsqlParameter("@_version", AUDIT_JSON_DATA_VERSION));

                var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_audits_static", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                if (rowseffected > 0)
                {
                    return true;
                }
            } catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred SaveStaticAudit()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }


            return false;
        }

        /// <summary>
        /// SaveStaticChecklist; Saves checklist as JSON to create a static version;
        /// </summary>
        /// <param name="checklistJson">Static version of a checklist in json format</param>
        /// <param name="id">Id of the object that is being saved. (used for references)</param>
        /// <param name="userId">UserId of the user that does the saving.</param>
        /// <param name="companyId">CompanyId of the user that does the saving</param>
        /// <returns>return true/false</returns>
        public async Task<bool> SaveStaticChecklistAsync(string checklistJson, int id, int userId, int companyId)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_checklistid", id));
                parameters.Add(new NpgsqlParameter("@_jsonobject", checklistJson));
                parameters.Add(new NpgsqlParameter("@_version", CHECKLIST_JSON_DATA_VERSION));

                // "_companyid", "_userid", "_checklistid" int4, "_jsonobject" text, "_version" varchar

                var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklists_static", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                if (rowseffected > 0)
                {
                    return true;
                }
            } catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred SaveStaticChecklist()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return false;
        }

        /// <summary>
        /// SaveStaticTask; Saves task as JSON to create a static version;
        /// </summary>
        /// <param name="taskJson">Static version of a task in json format</param>
        /// <param name="id">Id of the object that is being saved. (used for references)</param>
        /// <param name="userId">UserId of the user that does the saving.</param>
        /// <param name="companyId">CompanyId of the user that does the saving</param>
        /// <returns>return true/false</returns>
        public async Task<bool> SaveStaticTaskAsync(string taskJson, int id, int userId, int companyId)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_taskid", id));
                parameters.Add(new NpgsqlParameter("@_jsonobject", taskJson));
                parameters.Add(new NpgsqlParameter("@_version", TASK_JSON_DATA_VERSION));

                var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_tasks_static", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                if (rowseffected > 0)
                {
                    return true;
                }
            } catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred SaveStaticTask()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return false;
        }

        /// <summary>
        /// SaveStaticAssessment; Saves task as JSON to create a static version;
        /// </summary>
        /// <param name="assessmentJson">Static version of a task in json format</param>
        /// <param name="id">Id of the object that is being saved. (used for references)</param>
        /// <param name="userId">UserId of the user that does the saving.</param>
        /// <param name="companyId">CompanyId of the user that does the saving</param>
        /// <returns>return true/false</returns>
        public async Task<bool> SaveStaticAssessmentAsync(string assessmentJson, int id, int userId, int companyId)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_assessmentid", id));
                parameters.Add(new NpgsqlParameter("@_jsonobject", assessmentJson));
                parameters.Add(new NpgsqlParameter("@_version", TASK_JSON_DATA_VERSION));

                var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_assessment_static", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                if (rowseffected > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred SaveStaticAssessmentAsync()");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return false;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
