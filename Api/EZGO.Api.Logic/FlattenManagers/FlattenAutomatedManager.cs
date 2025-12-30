using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Provisioner;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.FlattenManagers
{
    //TODO add logging in general (method startup and end, errors etc)

    /// <summary>
    /// FlattenHistoricalManager; Flatten historical and current data where no specific historical data is available to create version of templates for further processing. 
    /// Note, depending on implementation this might move to own project to be able to optimize the implementation in certain worker services. 
    /// </summary>
    public class FlattenAutomatedManager : BaseFlattenDataManager<FlattenAutomatedManager>, IFlattenAutomatedManager
    {
        private readonly IDatabaseAccessHelper _datamanager;
        private readonly IConfigurationHelper _configHelper;
        private readonly IChecklistManager _checklistManager;
        private readonly IAuditManager _auditManager;
        private readonly IAssessmentManager _assessmentManager;
        private readonly IWorkInstructionManager _workInstructionManager;
        private readonly ITaskManager _taskManager;
        private bool _demoModeOnly { get { return _configHelper.GetValueAsBool("AppSettings:DemoModeOnly"); } } //will be renamed probably. 
        
        private const int AUTO_DELAY_IN_MS = 2000;
        private const string INCLUDE_ASSESSMENT = "instructions,instructionitems,areapaths,tags,mutationinformation";
        private const string INCLUDE_AUDIT = "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,tags";
        private const string INCLUDE_CHECKLIST = "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,tags";
        private const string INCLUDE_TASK = "steps,propertyvalues,recurrecy,recurrencyshifts,properties,propertydetails,instructionrelations,tags";
        private const string INCLUDE_WORKINSTRUCTION = "items,tags,areapaths,parents";

        public FlattenAutomatedManager(IDatabaseAccessHelper datamanager, IConfigurationHelper configHelper, IChecklistManager checklistManager, IAuditManager auditManager, IAssessmentManager assessmentManager, IWorkInstructionManager workInstructionManager, ITaskManager taskManager, ILogger<FlattenAutomatedManager> logger) : base(logger, datamanager, configHelper)
        {
            _datamanager = datamanager;
            _configHelper = configHelper;
            _checklistManager = checklistManager;
            _auditManager = auditManager;
            _assessmentManager = assessmentManager;
            _workInstructionManager = workInstructionManager;
            _taskManager = taskManager;
        }

        #region - public flatten methods -
        /// <summary>
        /// FlattenCurrentTemplatesAll; Run all for db, if no company supplied, companies will be retrieved and per company data will be processed. 
        /// </summary>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> FlattenCurrentTemplatesAll()
        {
            var output = true;
            var companyIds = await GetResourceCompanyIds(settingKey: "TECH_FLATTEN_DATA_AUTOMATED");
            //todo add exta checks + error handling
            if(companyIds != null && companyIds.Any())
            {
                foreach(var companyId in companyIds)
                {
                    var result = await FlattenCurrentTemplatesAll(companyId: companyId);
                    if(!result)
                    {
                        output = false; //set output to failed, if it fails
                        await AddFlattenerLogEvent(message: string.Format("Flattened Failed for {0}, {1}", companyId, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")), description: "Flattened failed.");
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// FlattenCurrentTemplatesAll; Flatten all current templates, which don't have a flattended version, based on the company id. 
        /// All types will be looped-through and processed per type, per template. 
        /// </summary>
        /// <param name="companyId">CompanyId of the company where it's data needs to be flattened.</param>
        /// <returns>true/false depending on outcome. </returns>
        public async Task<bool> FlattenCurrentTemplatesAll(int companyId)
        {
            if(companyId > 0)
            {

                await AddFlattenerLogEvent(message: string.Format("Flattened Started {0} for {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), companyId), description: "Flattened Started");

                var listTypes = new List<TemplateTypeEnum>() { TemplateTypeEnum.AuditTemplate, TemplateTypeEnum.ChecklistTemplate, TemplateTypeEnum.TaskTemplate, TemplateTypeEnum.WorkInstructionTemplate, TemplateTypeEnum.AssessmentTemplate };
                foreach(var type in listTypes)
                {
                    await AddFlattenerLogEvent(message: string.Format("Flattened Started {0} for type {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), type.ToString()), description: "Flattened Started");

                    var result = await FlattenCurrentTemplatesType(companyId: companyId, templateType:type);

                    await AddFlattenerLogEvent(message: string.Format("Flattened Ended {0} for type {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), type.ToString()), description: string.Concat("Flattened Ended; Result: ", result));
                }

                await AddFlattenerLogEvent(message: string.Format("Flattened Ended {0} for {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), companyId), description: "Flattened Ended");

                return true;
            } else
            {
                //TODO add loggging
                return false;
            }


        }

        /// <summary>
        /// FlattenCurrentTemplatesType; FlattenCurrentTemplate for specific type. 
        /// </summary>
        /// <param name="companyId">CompanyId of the company where it's data needs to be flattened.</param>
        /// <param name="templateType">Type of templates to be flattened.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> FlattenCurrentTemplatesType(int companyId, TemplateTypeEnum templateType)
        {
            var succes = false; 
            switch (templateType)
            {
                case TemplateTypeEnum.AuditTemplate:
                    succes = await FlattenAuditTemplates(companyId: companyId);
                    //TODO add flatten search part
                    //TODO add historical part
                    break;
                case TemplateTypeEnum.ChecklistTemplate:
                    succes = await FlattenChecklistTemplates(companyId: companyId);
                    //TODO add flatten search part
                    //TODO add historical part
                    break;
                case TemplateTypeEnum.WorkInstructionTemplate:
                    succes = await FlattenWorkinstructionTemplates(companyId: companyId);
                    //TODO add flatten search part
                    //TODO add historical part
                    break;
                case TemplateTypeEnum.TaskTemplate:
                    succes = await FlattenTaskTemplates(companyId: companyId);
                    //TODO add flatten search part
                    //TODO add historical part
                    break;
                case TemplateTypeEnum.AssessmentTemplate:
                    succes = await FlattenAssessmentTemplates(companyId: companyId);
                    //TODO add flatten search part
                    //TODO add historical part
                    break;
                default:
                    return false;
            }

            return succes;
        }
        #endregion

        #region - support methods for flattening process -
        /// <summary>
        /// GenerateVersion(); Generate version.
        /// </summary>
        /// <returns>Generated version as string</returns>
        private async Task<string> GenerateVersion()
        {
            await Task.CompletedTask;
            return base.GetNewVersion();
        }

        /// <summary>
        /// GenerateVersion(); Generate version based on specific data
        /// </summary>
        /// <returns>Generated version as string</returns>
        private async Task<string> GenerateVersionBasedOnDate(DateTime dateToConvert)
        {
            if(dateToConvert != DateTime.MinValue)
            {
                return string.Concat("V", dateToConvert.ToString("yyyyMMddHHmmssf"));
            } else
            {
                return await GenerateVersion(); 
            }
        }

        /// <summary>
        /// GetResourceCompanyIds; Get list of company ids configured in the settings 
        /// </summary>
        /// <param name="settingKey">Key of setting to be retrieved</param>
        /// <returns>List of numbers representing company ids</returns>
        private async Task<List<int>> GetResourceCompanyIds(string settingKey)
        {
            var output = new List<int>();

            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_settingkey", settingKey));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _datamanager.GetDataReader("get_resource_settings_by_key", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["settingvalue"] != DBNull.Value)
                        {
                            var ids = dr["settingvalue"].ToString();
                            if (ids != null)
                            {
                                output = ids.Split(",").Where(x => int.TryParse(x, out int num)).Select(x => Convert.ToInt32(x)).ToList();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await AddFlattenerLogEvent(message: "Error Retrieve Companies", type: "ERROR", description: ex.Message);
                _logger.LogError(exception: ex, message: string.Concat("FlattenAutomatedManager.GetResourceCompanyIds(): ", ex.Message));

            }

            return output;
        }

        /// <summary>
        /// RetrieveSystemUserId; Retrieve system user for use while processing data
        /// </summary>
        /// <param name="companyId">CompanyId of user which needs to be retrieved (DB: company_companies.id)</param>
        /// <returns>int containing the ID</returns>
        private async Task<int> RetrieveSystemUserId(int companyId)
        {
            int output = 0; 

            var parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_holdingid", Convert.ToInt32(0)));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _datamanager.GetDataReader("get_system_users_based_on_holding_company", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["company_id"] != DBNull.Value && dr["user_id"] != DBNull.Value)
                        {
                            output = Convert.ToInt32(dr["user_id"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await AddFlattenerLogEvent(message: "Error RetrieveSystemUserId", type: "ERROR", description: ex.Message);
                // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                _logger.LogError(exception: ex, message: string.Concat("FlattenAutomatedManager.RetrieveSystemUserId(): ", ex.Message));
            }
            return output;
        }

        /// <summary>
        /// RetrieveTemplateIds; Retrieve template ids per type of a certain company. 
        /// </summary>
        /// <param name="companyId">CompanyId of templates to be retrieved.</param>
        /// <param name="templateType">Kind of templates to be retrieved.</param>
        /// <returns>true/false</returns>
        private async Task<List<int>> RetrieveTemplateIds(int companyId, TemplateTypeEnum templateType)
        {
            List<int> output = new List<int>();

            string storedProcedureName = string.Empty;

            switch (templateType)
            {
                case TemplateTypeEnum.AuditTemplate:
                    storedProcedureName = "get_audittemplate_ids_without_versions_for_company";
                    break;
                case TemplateTypeEnum.ChecklistTemplate:
                    storedProcedureName = "get_checklisttemplate_ids_without_versions_for_company";
                    break;
                case TemplateTypeEnum.WorkInstructionTemplate:
                    storedProcedureName = "get_workinstructiontemplate_ids_without_versions_for_company";
                    break;
                case TemplateTypeEnum.TaskTemplate:
                    storedProcedureName = "get_tasktemplate_ids_without_versions_for_company";
                    break;
                case TemplateTypeEnum.AssessmentTemplate:
                    storedProcedureName = "get_assessmenttemplate_ids_without_versions_for_company";
                    break;
            }

            var parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_isactive", true));

            try
            {
                NpgsqlDataReader dr = null;

                using (dr = await _datamanager.GetDataReader(storedProcedureName, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure))
                {
                    while (await dr.ReadAsync())
                    {
                        if (dr["id"] != DBNull.Value)
                        {
                            output.Add(Convert.ToInt32(dr["id"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await AddFlattenerLogEvent(message: "Error RetrieveTemplateIds", type: "ERROR", description: ex.Message);
                // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                _logger.LogError(exception: ex, message: string.Concat("FlattenAutomatedManager.RetrieveTemplateIds(): ", ex.Message));
            }
            return output;
        }

        /// <summary>
        /// SaveFlattendData; Save flattended data, data will be saved with a certain creation date (can therefor also be used for saving historical data)
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="templateId"></param>
        /// <param name="version"></param>
        /// <param name="creationDate"></param>
        /// <param name="templateType"></param>
        /// <param name="flattenedData"></param>
        /// <returns></returns>
        public async Task<bool> SaveFlattendData(int companyId, int userId, int templateId, string version, DateTime creationDate, TemplateTypeEnum templateType, string flattenedData)
        {
            string storedProcedureName = string.Empty;

            switch (templateType)
            {
                case TemplateTypeEnum.AuditTemplate:
                    storedProcedureName = "add_flattened_audittemplate_history";
                    break;
                case TemplateTypeEnum.ChecklistTemplate:
                    storedProcedureName = "add_flattened_checklisttemplate_history";
                    break;
                case TemplateTypeEnum.WorkInstructionTemplate:
                    storedProcedureName = "add_flattened_workinstructiontemplate_history";
                    break;
                case TemplateTypeEnum.TaskTemplate:
                    storedProcedureName = "add_flattened_tasktemplate_history";
                    break;
                case TemplateTypeEnum.AssessmentTemplate:
                    storedProcedureName = "add_flattened_assessmenttemplate_history";
                    break;
            }

            try
            {
                //TODO create versioned information
                if(_demoModeOnly)
                {
                    await AddFlattenerLogEvent(message: string.Format("SaveFlattendData ({0},{1},{2},{3},{4})", companyId, userId, version, creationDate, templateId), type: "INFORMATION", description: flattenedData);
                } else
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                    parameters.Add(new NpgsqlParameter("@_userid", userId));
                    parameters.Add(new NpgsqlParameter("@_templateid", templateId));

                    var versionParam = new NpgsqlParameter("@_version", NpgsqlTypes.NpgsqlDbType.Varchar); //force varchar
                    versionParam.Value = version;
                    parameters.Add(versionParam);

                    var createParam = new NpgsqlParameter("@_creationdate", NpgsqlTypes.NpgsqlDbType.Timestamp); //force timestamp without tz
                    createParam.Value = DateTime.SpecifyKind(creationDate, DateTimeKind.Unspecified);
                    parameters.Add(createParam);

                    parameters.Add(new NpgsqlParameter("@_flattened_object", flattenedData));

                    var result = await _datamanager.ExecuteScalarAsync(storedProcedureName, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
                    //TODO add logging
                }

            }
            catch (Exception ex)
            {
                await AddFlattenerLogEvent(message: "Error SaveFlattendData", type: "ERROR", description: ex.Message);
                // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                _logger.LogError(exception: ex, message: string.Concat("FlattenAutomatedManager.SaveFlattendData(): ", ex.Message));
            }

            return true;
        }

        /// <summary>
        /// RetrieveVersion; Retrieve specific version.
        /// </summary>
        /// <param name="companyId">CompanyId of item, of version, to be retrieved.</param>
        /// <param name="templateId">TemplateId of item where version needs to be retrieved.</param>
        /// <param name="version">Version to be retrieved</param>
        /// <param name="templateType">TemplateType to be retrieved. (determine the specific sp where data needs to be retrieved.</param>
        /// <returns>string containing json</returns>
        public async Task<string> RetrieveVersionJson(int companyId, int templateId, string version, TemplateTypeEnum templateType)
        {
            string output = String.Empty;
            string storedProcedureName = String.Empty;

            switch (templateType)
            {
                case TemplateTypeEnum.AuditTemplate:
                    storedProcedureName = "get_flattened_audittemplate";
                    break;
                case TemplateTypeEnum.ChecklistTemplate:
                    storedProcedureName = "get_flattened_checklisttemplate";
                    break;
                case TemplateTypeEnum.WorkInstructionTemplate:
                    storedProcedureName = "get_flattened_workinstructiontemplate";
                    break;
                case TemplateTypeEnum.TaskTemplate:
                    storedProcedureName = "get_flattened_tasktemplate";
                    break;
                case TemplateTypeEnum.AssessmentTemplate:
                    storedProcedureName = "get_flattened_assessmenttemplate";
                    break;
            }

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_templateid", templateId),
                new NpgsqlParameter("@_version", version)
            };

            if(!string.IsNullOrEmpty(storedProcedureName))
            {
                try
                {
                    using NpgsqlDataReader dr = await _databaseManager.GetDataReader(storedProcedureName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                    while (await dr.ReadAsync())
                    {
                        if (dr.HasColumn("flattened_object") && dr["flattened_object"] != DBNull.Value)
                        {
                            output = dr["flattened_object"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("FlattenAutomatedManager.RetrieveVersionJson(): ", ex.Message));
                }
            }
            return output;
        }

        /// <summary>
        /// RetrieveVersionsList; Retrieves a list of versions / date of version (sorted on the date based on sorted list)
        /// </summary>
        /// <param name="companyId">CompanyId of item, of versions, to be retrieved.</param>
        /// <param name="templateId">TemplateId of item where version needs to be retrieved.</param>
        /// <param name="templateType">TemplateType to be retrieved. (determine the specific sp where data needs to be retrieved.</param>
        /// <returns>Sorted list containing the list of versions sorted on date.</returns>
        public async Task<SortedList<DateTime, string>> RetrieveVersionsList(int companyId, int templateId, TemplateTypeEnum templateType)
        {
            SortedList<DateTime, string> output = new SortedList<DateTime, string>();
            string storedProcedureName = String.Empty;

            switch (templateType)
            {
                case TemplateTypeEnum.AuditTemplate:
                    storedProcedureName = "get_flattened_audittemplate_versions";
                    break;
                case TemplateTypeEnum.ChecklistTemplate:
                    storedProcedureName = "get_flattened_checklisttemplate_versions";
                    break;
                case TemplateTypeEnum.WorkInstructionTemplate:
                    storedProcedureName = "get_flattened_workinstructiontemplate_versions";
                    break;
                case TemplateTypeEnum.TaskTemplate:
                    storedProcedureName = "get_flattened_tasktemplate_versions";
                    break;
                case TemplateTypeEnum.AssessmentTemplate:
                    storedProcedureName = "get_flattened_assessmenttemplate_versions";
                    break;
            }

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_templateid", templateId),
            };

            if (!string.IsNullOrEmpty(storedProcedureName))
            {
                try
                {
                    using NpgsqlDataReader dr = await _databaseManager.GetDataReader(storedProcedureName, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                    while (await dr.ReadAsync())
                    {
                        //check if for some reason version is empty, than don't process (should also be filtered in query)
                        if (dr["version"] != DBNull.Value && !string.IsNullOrEmpty(dr["version"].ToString()))
                        {
                            if (dr["created_at"] != DBNull.Value)
                            {
                                output.Add(Convert.ToDateTime(dr["created_at"]), dr["version"].ToString());
                            }

                            //TODO add logging
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("FlattenAutomatedManager.RetrieveVersionsList(): ", ex.Message));
                }
            }

            return output;
        }
        #endregion

        #region - flatten methods -
        private async Task<bool> FlattenAuditTemplates(int companyId)
        {
            //TODO add logging

            //retrieve template ids
            var templateIds = await RetrieveTemplateIds(companyId: companyId, TemplateTypeEnum.AuditTemplate);

            if(templateIds != null && templateIds.Any())
            {
                //retrieve system user
                var currentSystemUser = await RetrieveSystemUserId(companyId: companyId);
                if(currentSystemUser > 0) //if no system user then don't continue
                {
                    foreach(var templateId in templateIds)
                    {
                        //generate template for processing
                        var template = await _auditManager.GetAuditTemplateAsync(companyId: companyId, auditTemplateId: templateId, include: INCLUDE_AUDIT);
                        if (template != null)
                        {
                            var version = await GenerateVersionBasedOnDate(template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1));
                            template.Version = version;
                            var json = template.ToJsonFromObject();
                            if (!string.IsNullOrEmpty(json) && template.Id == templateId)
                            {
                                //post item to flatten structure
                                var saved = await SaveFlattendData(companyId: companyId, userId: currentSystemUser, templateId: templateId, version: version, creationDate: template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1), TemplateTypeEnum.AuditTemplate, flattenedData: json);
                                //add logging
                            }
                        }
                        await Task.Delay(AUTO_DELAY_IN_MS); //add throttle delay to not overuse the database. 
                    }
                }
            }

            return true;
        }

        private async Task<bool> FlattenChecklistTemplates(int companyId)
        {
            //TODO add logging

            //retrieve template ids
            var templateIds = await RetrieveTemplateIds(companyId: companyId, TemplateTypeEnum.ChecklistTemplate);

            if (templateIds != null && templateIds.Any())
            {
                //retrieve system user
                var currentSystemUser = await RetrieveSystemUserId(companyId: companyId);
                if (currentSystemUser > 0)
                {
                    foreach (var templateId in templateIds)
                    {
                        //generate template for processing
                        var template = await _checklistManager.GetChecklistTemplateAsync(companyId: companyId, checklistTemplateId: templateId, include: INCLUDE_CHECKLIST);
                        if (template != null)
                        {
                            var version = await GenerateVersionBasedOnDate(template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1));
                            template.Version = version;
                            var json = template.ToJsonFromObject();
                            if (!string.IsNullOrEmpty(json) && template.Id == templateId)
                            {
                                //post item to flatten structure
                                var saved = await SaveFlattendData(companyId: companyId, userId: currentSystemUser, templateId: templateId, version: version, creationDate: template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1), TemplateTypeEnum.ChecklistTemplate, flattenedData: json);
                                //add logging
                            }
                        }
                        await Task.Delay(AUTO_DELAY_IN_MS); //add throttle delay to not overuse the database.
                    }
                }
            }
            return true;
        }

        private async Task<bool> FlattenTaskTemplates(int companyId)
        {
            //TODO add logging

            //retrieve template ids
            var templateIds = await RetrieveTemplateIds(companyId: companyId, TemplateTypeEnum.TaskTemplate);

            if (templateIds != null && templateIds.Any())
            {
                //retrieve system user
                var currentSystemUser = await RetrieveSystemUserId(companyId: companyId);
                if (currentSystemUser > 0)
                {
                    foreach (var templateId in templateIds)
                    {
                        //generate template for processing
                        var template = await _taskManager.GetTaskTemplateAsync(companyId: companyId, taskTemplateId: templateId, include: INCLUDE_TASK);
                        if (template != null)
                        {
                            var version = await GenerateVersionBasedOnDate(template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1));
                            template.Version = version;
                            var json = template.ToJsonFromObject();
                            if (!string.IsNullOrEmpty(json) && template.Id == templateId)
                            {
                                //post item to flatten structure
                                var saved = await SaveFlattendData(companyId: companyId, userId: currentSystemUser, templateId: templateId, version: version, creationDate: template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1), TemplateTypeEnum.TaskTemplate, flattenedData: json);
                                //add logging
                            }
                        }
                        await Task.Delay(AUTO_DELAY_IN_MS); //add throttle delay to not overuse the database.
                    }
                }
            }
            return true;
        }

        private async Task<bool> FlattenWorkinstructionTemplates(int companyId)
        {
            //TODO add logging

            //retrieve template ids
            var templateIds = await RetrieveTemplateIds(companyId: companyId, TemplateTypeEnum.WorkInstructionTemplate);

            if (templateIds != null && templateIds.Any())
            {
                //retrieve system user
                var currentSystemUser = await RetrieveSystemUserId(companyId: companyId);
                if (currentSystemUser > 0)
                {
                    foreach (var templateId in templateIds)
                    {
                        //generate template for processing
                        var template = await _workInstructionManager.GetWorkInstructionTemplateAsync(companyId: companyId, workInstructionTemplateId: templateId, include: INCLUDE_WORKINSTRUCTION);
                        if (template != null)
                        {
                            var version = await GenerateVersionBasedOnDate(template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1));
                            template.Version = version;
                            var json = template.ToJsonFromObject();
                            if (!string.IsNullOrEmpty(json) && template.Id == templateId)
                            {
                                //post item to flatten structure
                                var saved = await SaveFlattendData(companyId: companyId, userId: currentSystemUser, templateId: templateId, version: version, creationDate: template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1), TemplateTypeEnum.WorkInstructionTemplate, flattenedData: json);
                                //add logging
                            }
                        }
                        await Task.Delay(AUTO_DELAY_IN_MS); //add throttle delay to not overuse the database.
                    }
                }
            }
            return true;
        }

        private async Task<bool> FlattenAssessmentTemplates(int companyId)
        {
            //TODO add logging

            //retrieve template ids
            var templateIds = await RetrieveTemplateIds(companyId: companyId, TemplateTypeEnum.AssessmentTemplate);

            if (templateIds != null && templateIds.Any())
            {
                //retrieve system user
                var currentSystemUser = await RetrieveSystemUserId(companyId: companyId);
                if (currentSystemUser > 0)
                {
                    foreach (var templateId in templateIds)
                    {
                        //generate template for processing
                        var template = await _assessmentManager.GetAssessmentTemplateAsync(companyId: companyId, assessmentTemplateId: templateId, include: INCLUDE_ASSESSMENT);
                        if (template != null)
                        {
                            var version = await GenerateVersionBasedOnDate(template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1));
                            template.Version = version;
                            var json = template.ToJsonFromObject();
                            if (!string.IsNullOrEmpty(json) && template.Id == templateId)
                            {
                                //post item to flatten structure
                                var saved = await SaveFlattendData(companyId: companyId, userId: currentSystemUser, templateId: templateId, version: version, creationDate: template.ModifiedAt.HasValue ? template.ModifiedAt.Value : DateTime.Now.AddDays(-1), TemplateTypeEnum.AssessmentTemplate, flattenedData: json);
                                //add logging
                            }
                        }
                        await Task.Delay(AUTO_DELAY_IN_MS); //add throttle delay to not overuse the database.
                    }
                }
            }
            return true;
        }
        #endregion

        #region - flatten search methods -
        //note this needs to be done for all flatten types

        //retrieve flattened data which does not have search table items yet (using sps)

        //for each flattended data which does not have search table items yet
        //-> generate search table data
        //-> save search table data
        //-> make sure version is correctly updated

        //functionality can use same feature setting for automation, if normal flatten is not done yet it will be done also, if nog only search will be done. 
        #endregion

        #region - flattend historical data -
        //note this needs to be done for all flatten types

        //retrieve history data to created flattened data of using static tables, live tables and auditing data. (this will depend on type, for tasks can not use status tables, don't exists)

        //create flattended data

        //save flattended data
        #endregion

        #region - logging -
        /// <summary>
        /// AddFlattenerLogEvent; Adds item to AddFlattenerLogEvent log. 
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="eventId">Possible event id</param>
        /// <param name="type">Type of message</param>
        /// <param name="eventName">Possible event name</param>
        /// <param name="description">Description, containing more details information if available. </param>
        /// <returns>true/false (will mostly be ignored, but can be used if needed.)</returns>
        public async Task<bool> AddFlattenerLogEvent(string message, int eventId = 0, string type = "INFORMATION", string eventName = "", string description = "")
        {
            if (_configHelper.GetValueAsBool("AppSettings:EnableDbLogging"))
            {
                try
                {
                    var source = _configHelper.GetValueAsString("AppSettings:ApplicationName");

                    var parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_message", message.Length > 255 ? message.Substring(0, 254) : message));
                    parameters.Add(new NpgsqlParameter("@_type", type));
                    parameters.Add(new NpgsqlParameter("@_eventid", eventId.ToString()));
                    parameters.Add(new NpgsqlParameter("@_eventname", eventName));

                    if (string.IsNullOrEmpty(source))
                    {
                        parameters.Add(new NpgsqlParameter("@_source", ""));
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_source", source));
                    }
                    parameters.Add(new NpgsqlParameter("@_description", description));

                    var output = await _datamanager.ExecuteScalarAsync("add_logging_flattener", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {
                    // Error occurred within the logging. Ignore it or else the helper can end up in a painfull loop of death.
                    _logger.LogError(exception: ex, message: string.Concat("FlattenHistoricalManager.AddFlattenerLogEvent(): ", ex.Message));
                }
                finally
                {

                }
            }
            return true;
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
                listEx.AddRange(_assessmentManager.GetPossibleExceptions());
                listEx.AddRange(_workInstructionManager.GetPossibleExceptions());
                listEx.AddRange(_taskManager.GetPossibleExceptions());
            } catch(Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }
        #endregion

    }
}
