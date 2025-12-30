using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Versions;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class VersionManager : BaseManager<VersionManager>, IVersionManager
    {

        #region - private(s) -
        private readonly IDatabaseAccessHelper _databaseAccessHelper;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDataAuditing _dataAuditing;
        #endregion

        #region - constructor -
        public VersionManager(IDatabaseAccessHelper databaseAccessHelper, IConfigurationHelper configurationHelper, ILogger<VersionManager> logger, IDataAuditing dataAuditing) : base(logger)
        {
            _databaseAccessHelper = databaseAccessHelper;
            _configurationHelper = configurationHelper;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public methods -
        public async Task<List<VersionApp>> GetVersionsAppAsync()
        {
            List<VersionApp> versionsApp = new();

            try
            {
                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_versions_app", commandType: System.Data.CommandType.StoredProcedure);
                while (await dr.ReadAsync())
                {
                    VersionApp versionApp = CreateOrFillVersionAppFromReader(dr);
                    if (versionApp != null && versionApp.Id > 0)
                    {
                        versionsApp.Add(versionApp);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("VersionManager.GetVersionsApp(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }

            return versionsApp;
        }

        public async Task<VersionApp> GetVersionAppAsync(int versionAppId, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var versionApp = new VersionApp();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_id", versionAppId)
                };

                await using NpgsqlDataReader dr = await _databaseAccessHelper.GetDataReader("get_version_app", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind);
                while (await dr.ReadAsync())
                {
                    CreateOrFillVersionAppFromReader(dr, versionApp: versionApp);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("VersionManager.GetVersionAppAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            if (versionApp.Id > 0)
            {
                return versionApp;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> AddVersionAppAsync(VersionApp versionApp, int userId, int companyId)
        {
            List<NpgsqlParameter> parameters = new();
            parameters.AddRange(GetNpgsqlParametersFromVersionApp(versionApp: versionApp));

            var possibleId = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_version_app", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                versionApp.Id = possibleId; //set id for further processing
            }

            if (possibleId > 0)
            {
                var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.version_app.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.version_app.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added app version.");
            }

            return possibleId;
        }

        public async Task<bool> ChangeVersionAppAsync(VersionApp versionApp, int userId, int companyId)
        {
            var original = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.version_app.ToString(), versionApp.Id);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.AddRange(GetNpgsqlParametersFromVersionApp(versionApp));

            var rowseffected = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("change_version_app", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.version_app.ToString(), versionApp.Id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.version_app.ToString(), objectId: versionApp.Id, userId: userId, companyId: companyId, description: "Changed app version.");

            }

            return rowseffected > 0;
        }

        public async Task<bool> SetVersionActiveAsync(int userId, int companyId, int versionId, bool isActive = true)
        {
            var original = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.version_app.ToString(), versionId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_id", versionId),
                new NpgsqlParameter("@_active", isActive)
            };
            var rowseffected = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("set_version_app_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _databaseAccessHelper.GetDataRowAsJson(Models.Enumerations.TableNames.version_app.ToString(), versionId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.version_app.ToString(), objectId: versionId, userId: userId, companyId: companyId, description: "Changed version active state.");
            }

            return (rowseffected > 0);
        }
        #endregion

        #region - private methods -
        private VersionApp CreateOrFillVersionAppFromReader(NpgsqlDataReader dr, VersionApp versionApp = null)
        {
            versionApp ??= new();

            versionApp.Id                       = Convert.ToInt32(dr["id"]);
            versionApp.IsValidated              = Convert.ToBoolean(dr["is_validated"]);
            versionApp.AppName                  = dr["app_name"] != DBNull.Value                  ? dr["app_name"].ToString()                                                       : versionApp.AppName;
            versionApp.AppVersionInternal       = dr["app_version_internal"] != DBNull.Value      ? dr["app_version_internal"].ToString()                                           : versionApp.AppVersionInternal;
            versionApp.AppVersion               = dr["app_version"] != DBNull.Value               ? dr["app_version"].ToString()                                                    : versionApp.AppVersion;
            versionApp.OctopusVersion           = dr["octopus_version"] != DBNull.Value           ? dr["octopus_version"].ToString()                                                : versionApp.OctopusVersion;
            versionApp.Platform                 = dr["platform"] != DBNull.Value                  ? dr["platform"].ToString()                                                       : versionApp.Platform;
            versionApp.IsLive                   = dr["is_live"] != DBNull.Value                   ? Convert.ToBoolean(dr["is_live"])                                                : versionApp.IsLive;
            versionApp.IsCurrentActiveVersion   = dr["is_current_active_version"] != DBNull.Value ? Convert.ToBoolean(dr["is_current_active_version"])                              : versionApp.IsCurrentActiveVersion;
            versionApp.ReleaseDate              = dr["release_date"] != DBNull.Value              ? DateTime.SpecifyKind(Convert.ToDateTime(dr["release_date"]), DateTimeKind.Utc)  : versionApp.ReleaseDate;
            versionApp.ReleaseNotes             = dr["release_notes"] != DBNull.Value             ? dr["release_notes"].ToString()                                                  : versionApp.ReleaseNotes;
            versionApp.ModifiedAt               = dr["modified_at"] != DBNull.Value               ? Convert.ToDateTime(dr["modified_at"])                                           : versionApp.ModifiedAt;

            return versionApp;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersFromVersionApp(VersionApp versionApp)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("_isvalidated", versionApp.IsValidated),
                new NpgsqlParameter("_appversion", versionApp.AppVersion),
                string.IsNullOrEmpty(versionApp.AppName)                ? new NpgsqlParameter("_appname", DBNull.Value)                 : new NpgsqlParameter("_appname", versionApp.AppName),
                string.IsNullOrEmpty(versionApp.AppVersionInternal)     ? new NpgsqlParameter("_appversioninternal", DBNull.Value)      : new NpgsqlParameter("_appversioninternal", versionApp.AppVersionInternal),
                string.IsNullOrEmpty(versionApp.OctopusVersion)         ? new NpgsqlParameter("_octopusversion", DBNull.Value)          : new NpgsqlParameter("_octopusversion", versionApp.OctopusVersion),
                string.IsNullOrEmpty(versionApp.Platform)               ? new NpgsqlParameter("_platform", DBNull.Value)                : new NpgsqlParameter("_platform", versionApp.Platform),
                versionApp.IsLive == null                               ? new NpgsqlParameter("_islive", DBNull.Value)                  : new NpgsqlParameter("_islive", versionApp.IsLive),
                versionApp.IsCurrentActiveVersion == null               ? new NpgsqlParameter("_iscurrentactiveversion", DBNull.Value)  : new NpgsqlParameter("_iscurrentactiveversion", versionApp.IsCurrentActiveVersion),
                versionApp.ReleaseDate == null                          ? new NpgsqlParameter("_releasedate", DBNull.Value)             : new NpgsqlParameter("_releasedate", versionApp.ReleaseDate),
                string.IsNullOrEmpty(versionApp.ReleaseNotes)           ? new NpgsqlParameter("_releasenotes", DBNull.Value)            : new NpgsqlParameter("_releasenotes", versionApp.ReleaseNotes)
            };

            if (versionApp.Id > 0) parameters.Add(new NpgsqlParameter("_id", versionApp.Id));

            return parameters;
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
