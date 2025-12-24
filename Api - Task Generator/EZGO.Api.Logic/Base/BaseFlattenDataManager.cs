using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Base
{
    public class BaseFlattenDataManager<T>
    {
        /// <summary>
        /// List of exceptions that occurred for further processing. 
        /// </summary>
        protected List<Exception> Exceptions = new List<Exception>();

        protected readonly ILogger<T> _logger;
        protected readonly IDatabaseAccessHelper _databaseManager;
        private readonly IConfigurationHelper _configurationHelper;

        public BaseFlattenDataManager(ILogger<T> logger, IDatabaseAccessHelper databaseManager, IConfigurationHelper configurationHelper)
        {
            this._logger = logger;
            this._databaseManager = databaseManager;
            this._configurationHelper = configurationHelper;
        }

        public string GetNewVersion()
        {
            return string.Concat("V", DateTime.UtcNow.ToString("yyyyMMddHHmmssf"));
        }

        public async Task<TT> RetrieveFlattenData<TT>(int templateId, string version, int companyId)
        {
            TT template = (TT)Activator.CreateInstance(typeof(TT));
            string storedProcedureName = null;
            switch (template)
            {
                case ChecklistTemplate:
                    storedProcedureName = "get_flattened_checklisttemplate";
                    break;
                case AuditTemplate:
                    storedProcedureName = "get_flattened_audittemplate";
                    break;
                case WorkInstructionTemplate:
                    storedProcedureName = "get_flattened_workinstructiontemplate";
                    break;
                case AssessmentTemplate:
                    storedProcedureName = "get_flattened_assessmenttemplate";
                    break;
                case TaskTemplate:
                    storedProcedureName = "get_flattened_tasktemplate";
                    break;
                default:
                    _logger.LogError($"RetrieveFlattenData; Type '{nameof(TT)}' is not an implemented type in RetrieveFlattenData().");
                    return default;
            }
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_templateid", templateId),
                new NpgsqlParameter("@_version", version)
            };

            try
            {
                using NpgsqlDataReader dr = await _databaseManager.GetDataReader(storedProcedureName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    if (dr.HasColumn("flattened_object") && dr["flattened_object"] != DBNull.Value)
                    {
                        template = JsonSerializer.Deserialize<TT>(dr["flattened_object"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"{nameof(BaseFlattenDataManager<T>)}.{nameof(RetrieveFlattenData)}: {ex.Message}");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return template;
        }

        public async Task<string> RetrieveLatestAvailableVersion(VersionedTemplateTypeEnum templateType, int templateId, int companyId)
        {
            string latestAvailableVersion = null;
            string databaseFunctionName = null;

            switch (templateType)
            {
                case VersionedTemplateTypeEnum.ChecklistTemplate:
                    databaseFunctionName = "get_latest_flattened_checklisttemplate_version";
                    break;
                case VersionedTemplateTypeEnum.AuditTemplate:
                    databaseFunctionName = "get_latest_flattened_audittemplate_version";
                    break;
                case VersionedTemplateTypeEnum.WorkInstructionTemplate:
                    databaseFunctionName = "get_latest_flattened_workinstructiontemplate_version";
                    break;
                case VersionedTemplateTypeEnum.AssessmentTemplate:
                    databaseFunctionName = "get_latest_flattened_assessmenttemplate_version";
                    break;
                case VersionedTemplateTypeEnum.TaskTemplate:
                    databaseFunctionName = "get_latest_flattened_tasktemplate_version";
                    break;
                default:
                    _logger.LogError($"RetrieveLatestAvailableVersion; Type '{nameof(templateType)}' is not an implemented VersionedTemplateTypeEnum in RetrieveLatestAvailableVersion().");
                    return default;
            }

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_templateid", templateId)
            };

            try
            {
                using NpgsqlDataReader dr = await _databaseManager.GetDataReader(databaseFunctionName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    if (dr.HasColumn("version") && dr["version"] != DBNull.Value)
                    {
                        latestAvailableVersion = dr["version"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"{nameof(BaseFlattenDataManager<T>)}.{nameof(RetrieveLatestAvailableVersion)}: {ex.Message}");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return latestAvailableVersion;
        }

        public async Task<string> RetrieveVersionForExistingObjectAsync(VersionedTemplateTypeEnum templateType, int objectId, int companyId)
        {
            string objectVersion = null;
            string databaseFunctionName = null;

            switch (templateType)
            {
                case VersionedTemplateTypeEnum.ChecklistTemplate:
                    databaseFunctionName = "get_checklist_version";
                    break;
                case VersionedTemplateTypeEnum.AuditTemplate:
                    databaseFunctionName = "get_audit_version";
                    break;
                case VersionedTemplateTypeEnum.WorkInstructionTemplate:
                    databaseFunctionName = "get_workinstructions_version";
                    break;
                case VersionedTemplateTypeEnum.AssessmentTemplate:
                    databaseFunctionName = "get_assessment_version";
                    break;
                case VersionedTemplateTypeEnum.TaskTemplate:
                    databaseFunctionName = "get_task_version";
                    break;
                default:
                    _logger.LogError($"RetrieveLatestAvailableVersion; Type '{nameof(templateType)}' is not an implemented VersionedTemplateTypeEnum in RetrieveLatestAvailableVersion().");
                    return default;
            }

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_id", objectId)
            };

            try
            {
                using NpgsqlDataReader dr = await _databaseManager.GetDataReader(databaseFunctionName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    if (dr.HasColumn("version") && dr["version"] != DBNull.Value)
                    {
                        objectVersion = dr["version"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"{nameof(BaseFlattenDataManager<T>)}.{nameof(RetrieveLatestAvailableVersion)}: {ex.Message}");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return objectVersion;
        }

        public async Task<TT> RetrieveLatestFlattenData<TT>(int templateId, int companyId)
        {
            string storedProcedureName = null;
            TT template = (TT)Activator.CreateInstance(typeof(TT));

            switch (template)
            {
                case ChecklistTemplate:
                    storedProcedureName = "get_latest_flattened_checklisttemplate";
                    break;
                case AuditTemplate:
                    storedProcedureName = "get_latest_flattened_audittemplate";
                    break;
                case WorkInstructionTemplate:
                    storedProcedureName = "get_latest_flattened_workinstructiontemplate";
                    break;
                case AssessmentTemplate:
                    storedProcedureName = "get_latest_flattened_assessmenttemplate";
                    break;
                case TaskTemplate:
                    storedProcedureName = "get_latest_flattened_tasktemplate";
                    break;
                default:
                    _logger.LogError($"RetrieveLatestFlattenData; Type '{nameof(TT)}' is not an implemented type in RetrieveLatestFlattenData().");
                    return default;
            }

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_templateid", templateId)
            };

            try
            {
                using NpgsqlDataReader dr = await _databaseManager.GetDataReader(storedProcedureName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    if (dr.HasColumn("flattened_object") && dr["flattened_object"] != DBNull.Value)
                    {
                        template = JsonSerializer.Deserialize<TT>(dr["flattened_object"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"{nameof(BaseFlattenDataManager<T>)}.{nameof(RetrieveLatestFlattenData)}: {ex.Message}");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return template;
        }

        public async Task<bool> SaveFlattenData<TT>(int companyId, int userId, TT flattenObject, int templateId)
        {
            string addFlattenedObjectStoredProcedureName = string.Empty;
            string addVersionStoredProcedureName = string.Empty;
            switch (flattenObject)
            {
                case ChecklistTemplate checklistTemplate:
                    checklistTemplate.Version = null;
                    addFlattenedObjectStoredProcedureName = "add_flattened_checklisttemplate";
                    addVersionStoredProcedureName = "change_checklisttemplate_version";
                    break;
                case AuditTemplate auditTemplate:
                    auditTemplate.Version = null;
                    addFlattenedObjectStoredProcedureName = "add_flattened_audittemplate";
                    addVersionStoredProcedureName = "change_audittemplate_version";
                    break;
                case WorkInstructionTemplate workInstructionTemplate:
                    workInstructionTemplate.Version = null;
                    addFlattenedObjectStoredProcedureName = "add_flattened_workinstructiontemplate";
                    addVersionStoredProcedureName = "change_workinstructiontemplate_version";
                    break;
                case AssessmentTemplate assessmentTemplate:
                    assessmentTemplate.Version = null;
                    if (assessmentTemplate.SkillInstructions != null)
                        foreach (AssessmentTemplateSkillInstruction assessmentTemplateSkillInstruction in assessmentTemplate.SkillInstructions)
                            assessmentTemplateSkillInstruction.Version = null;
                    addFlattenedObjectStoredProcedureName = "add_flattened_assessmenttemplate";
                    addVersionStoredProcedureName = "change_assessmenttemplate_version";
                    break;
                case TaskTemplate taskTemplate:
                    taskTemplate.Version = null;
                    addFlattenedObjectStoredProcedureName = "add_flattened_tasktemplate";
                    addVersionStoredProcedureName = "change_tasktemplate_version";
                    break;
                default:
                    _logger.LogError($"SaveFlattenData; Type '{nameof(TT)}' is not an implemented type in SaveFlattenData().");
                    return false;
            }

            string version = GetNewVersion();
            string objectJson = flattenObject.ToJsonFromObject();

            List<NpgsqlParameter> parameters = new() {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_userid", userId),
                new NpgsqlParameter("@_templateid", templateId),
                new NpgsqlParameter("@_version", version),
                new NpgsqlParameter("@_flattened_object", objectJson)
            };

            int possibleId = 0;
            int versionChangedRowCount = 0;
            try
            {
                possibleId = Convert.ToInt32(await _databaseManager.ExecuteScalarAsync(addFlattenedObjectStoredProcedureName, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                if (possibleId > 0)
                {
                    List<NpgsqlParameter> parametersForVersion = new()
                    {
                        new NpgsqlParameter("@_companyid", companyId),
                        new NpgsqlParameter("@_id", templateId),
                        new NpgsqlParameter("@_version", version)
                    };
                    versionChangedRowCount = Convert.ToInt32(await _databaseManager.ExecuteScalarAsync(addVersionStoredProcedureName, parametersForVersion, System.Data.CommandType.StoredProcedure));

                    //if  a task template was change, also set the version for the tasks that are already generated to be executed in the future
                    if (flattenObject.GetType() == typeof(TaskTemplate))
                    {
                        List<NpgsqlParameter> parametersForTasksVersion = new()
                        {
                            new NpgsqlParameter("@_companyid", companyId),
                            new NpgsqlParameter("@_templateid", templateId),
                            new NpgsqlParameter("@_version", version)
                        };
                        versionChangedRowCount = Convert.ToInt32(await _databaseManager.ExecuteScalarAsync("change_tasks_version_for_template", parametersForTasksVersion, System.Data.CommandType.StoredProcedure));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: $"{nameof(BaseFlattenDataManager<T>)}.{nameof(SaveFlattenData)}: {ex.Message}");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return possibleId > 0;
        }

        /// <summary>
        /// GetPossibleException; Get exceptions that might have occurred but are captured. 
        /// Can be used for logging/sending to error handling systems. 
        /// </summary>
        /// <returns>List of Exceptions</returns>
        protected List<Exception> GetPossibleExceptions()
        {
            return Exceptions;
        }
    }
}
