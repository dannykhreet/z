using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// DataMigrationManager; Migration Manager, contains functionalities for migrating data sets in the database;
    /// These will be used when implementing new functionalities. Currently the manager contains a migration structure for creating static items that do not exist yet.
    /// Migration manager will use already created logic to migrate existing data to a new structure, a different structure or convert data to a different format. 
    /// </summary>
    public class DataMigrationManager : BaseManager<DataMigrationManager>,  IDataMigrationManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IVersionReleaseManager _versionReleaseManager;
        private readonly IAuditManager _auditManager;
        private readonly IChecklistManager _checklistManager;
        private const int BASE_LIMIT = 100;

        #endregion

        #region - constructor(s) -
        public DataMigrationManager(IConfigurationHelper confighelper, IDatabaseAccessHelper manager, IVersionReleaseManager versionReleaseManager, IAuditManager auditManager, IChecklistManager checklistManager, ILogger<DataMigrationManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = confighelper;
            _versionReleaseManager = versionReleaseManager;
            _auditManager = auditManager;
            _checklistManager = checklistManager;

        }
        #endregion

        #region - static data creation -
        /// <summary>
        /// MigrationAuditsToStaticAsync; Migrations for creating audits.
        /// </summary>
        /// <param name="companyId">companyId; references companies_company.id</param>
        /// <returns>int, number of items created or modified.</returns>
        public async Task<int> MigrationAuditsToStaticAsync(int companyId, int userId)
        {
            if(companyId > 0 && userId > 0)
            {
                return await MigrationCreateStaticAuditsForCompany(companyId: companyId, userId: userId);
            } else
            {
                return 0;
            }
        }

        /// <summary>
        /// MigrationChecklistsToStaticAsync; Migrations for creating checklists.
        /// </summary>
        /// <param name="companyId">companyId; references companies_company.id</param>
        /// <returns>int, number of items created or modified.</returns>
        public async Task<int> MigrationChecklistsToStaticAsync(int companyId, int userId)
        {
            if (companyId > 0 && userId > 0)
            {
                return await MigrationCreateStaticChecklistsForCompany(companyId: companyId, userId: userId);
            } else
            {
                return 0;
            }
        }

        public async Task<int> DataAuditingCorrectionForAuditTemplateSetActive()
        {
            var rowCount = await _manager.ExecuteScalarAsync("fix_auditing_data_audittemplates");
            
            return Convert.ToInt32(rowCount);
        }

        public async Task<int> DataAuditingCorrectionForWorkInstructionTemplateItems()
        {
            var rowCount = await _manager.ExecuteScalarAsync("fix_auditing_data_workinstruction_items");

            return Convert.ToInt32(rowCount);
        }
        #endregion

        #region - specific migrations -
        public async Task<int> ActionCommentCorrectionForActionResolvedIssue()
        {
            var rowCount = await _manager.ExecuteScalarAsync("fix_action_comment_resolved_actions");
            return Convert.ToInt32(rowCount);
        }
        #endregion

        #region - private static data creation -
        /// <summary>
        /// MigrationCreateStaticAuditsForCompany
        /// </summary>
        /// <param name="companyId">companyId; references companies_company.id</param>
        /// <returns>int, number of items created or modified.</returns>
        private async Task<int> MigrationCreateStaticAuditsForCompany(int companyId, int userId)
        {
            NpgsqlDataReader dr = null;
            var ids = new List<int>();
            var output = 0;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_limit", BASE_LIMIT));

                using (dr = await _manager.GetDataReader("migration_data_static_audit_ids", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: Data.Enumerations.ConnectionKind.Writer))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr["id"]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataMigrationManager.MigrationCreateStaticAuditsForCompany(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if(ids.Count > 0)
            {
                foreach(var id in ids)
                {
                    var resultfull = await _auditManager.GetAuditAsync(companyId: companyId, auditId: id, include: Settings.ApiSettings.FULL_INCLUDE_LIST, connectionKind: Data.Enumerations.ConnectionKind.Reader);
                    if (resultfull != null && resultfull.Id > 0)
                    {
                        var resultUserId = userId;
                        if (resultfull.Signatures != null && resultfull.Signatures.Count > 0 && resultfull.Signatures[0].SignedById.HasValue && resultfull.Signatures[0].SignedById.Value > 0)
                        {
                            resultUserId = resultfull.Signatures[0].SignedById.Value;
                        }
                        var sv = await _versionReleaseManager.SaveStaticAuditAsync(auditJson: (resultfull).ToJsonFromObject(), id: resultfull.Id, companyId: companyId, userId: resultUserId);
                        if(sv)
                        {
                            output = output + 1;
                        }
                    }
                }
            }
            return output;
        }


        /// <summary>
        /// MigrationCreateStaticChecklistsForCompany
        /// </summary>
        /// <param name="companyId">companyId; references companies_company.id</param>
        /// <returns>int, number of items created or modified.</returns>
        private async Task<int> MigrationCreateStaticChecklistsForCompany(int companyId, int userId)
        {
            NpgsqlDataReader dr = null;
            var ids = new List<int>();
            var output = 0;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_limit", BASE_LIMIT));

                using (dr = await _manager.GetDataReader("migration_data_static_checklist_ids", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: Data.Enumerations.ConnectionKind.Writer))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr["id"]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("DataMigrationManager.MigrationCreateStaticChecklistsForCompany(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (ids.Count > 0)
            {
                foreach (var id in ids)
                {
                    var resultfull = await _checklistManager.GetChecklistAsync(companyId: companyId, checklistId: id, include: Settings.ApiSettings.FULL_INCLUDE_LIST, connectionKind: Data.Enumerations.ConnectionKind.Reader);
                    if (resultfull != null && resultfull.Id > 0)
                    {
                        var resultUserId = userId;
                        if (resultfull.Signatures != null && resultfull.Signatures.Count > 0 && resultfull.Signatures[0].SignedById.HasValue && resultfull.Signatures[0].SignedById.Value > 0)
                        {
                            //note! using signed by id for userid, if not found administrator user is used.
                            resultUserId = resultfull.Signatures[0].SignedById.Value;
                        }
                        var sv = await _versionReleaseManager.SaveStaticChecklistAsync(checklistJson: (resultfull).ToJsonFromObject(), id: resultfull.Id, companyId: companyId, userId: resultUserId);
                        if (sv)
                        {
                            output = output + 1;
                        }
                    }
                }
            }
            return output;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_checklistManager.GetPossibleExceptions());
                listEx.AddRange(_auditManager.GetPossibleExceptions());
                listEx.AddRange(_versionReleaseManager.GetPossibleExceptions());
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
