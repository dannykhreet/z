using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Logic.FlattenManagers;
using EZGO.Api.Logic.Helpers;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Stats;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Data;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Mappers;
using EZGO.Api.Utils.Tools;
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
    /// AuditManager; The AuditManager contains all logic for retrieving and setting Audits and AuditTemplates.
    /// Audits are lists of items, that can have a score ( 0 ~ 10 depending on template settings) or a thumbs-up / down. 
    /// Audits are for the most part structurally the same as Checklists but have a scoring mechanism and a other workflow (business workflow) when used. 
    /// The output data (exports) and display is also handled differently. In the future Audits and Checklists will differ more. 
    /// Technically a Audit is a parent object that contains Tasks (which are the items).
    /// Audits are filled in within the client apps, the outcome of a audit is displayed within the client apps and the CMS.
    /// AuditTemplates are managed in the CMS and based on the Template a Audit is generated in the apps to be filled in. 
    /// </summary>
    public class AuditManager : BaseManager<AuditManager>, IAuditManager
    {
        #region - properties -
        private string culture;
        public string Culture
        {
            get { return culture; }
            set { culture = _tagManager.Culture = value; }
        }
        #endregion

        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly ITaskManager _taskManager;
        private readonly IAreaManager _areaManager;
        private readonly IAreaBasicManager _areaBasicManager;
        private readonly IActionManager _actionManager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IUserAccessManager _userAccessManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IPropertyValueManager _propertyValueManager;
        private readonly IWorkInstructionManager _workInstructionManager;
        private readonly ITagManager _tagManager;
        private readonly IUserManager _userManager;
        private readonly IGeneralManager _generalManager;
        private readonly IFlattenAuditManager _flattenedAuditManager;
        private readonly ITranslationManager _translationManager;
        #endregion

        #region - constructor(s) -
        public AuditManager(IGeneralManager generalManager, IFlattenAuditManager flattenAuditManager, IDatabaseAccessHelper manager, ITagManager tagManager, IUserManager userManager, ITaskManager taskManager, IConfigurationHelper configurationHelper, IPropertyValueManager propertyValueManager, IWorkInstructionManager workInstructionManager, IAreaManager areaManager, IAreaBasicManager areaBasicManager, IActionManager actionManager, IDataAuditing dataAuditing, IUserAccessManager userAccessManager, ITranslationManager translationManager, ILogger<AuditManager> logger) : base(logger)
        {
            _manager = manager;
            _taskManager = taskManager;
            _areaManager = areaManager;
            _actionManager = actionManager;
            _dataAuditing = dataAuditing;
            _userAccessManager = userAccessManager;
            _propertyValueManager = propertyValueManager;
            _configurationHelper = configurationHelper;
            _workInstructionManager = workInstructionManager;
            _tagManager = tagManager;
            _userManager = userManager;
            _generalManager = generalManager;
            _flattenedAuditManager = flattenAuditManager;
            _areaBasicManager = areaBasicManager;
            _translationManager = translationManager;
        }
        #endregion

        #region - public methods Audits -
        /// <summary>
        /// GetAuditsAsync; Get a list of audits based on the CompanyId.
        /// Following stored procedures will be used for database data retrieval: "get_audits_static" OR "get_audits"
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id).</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <param name="userId">UserId, used for certain filters. (profiles_user.id).</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <param name="useStatic">Use static data set or the live one.</param>
        /// <returns>A list of Audit objects.</returns>
        public async Task<List<Audit>> GetAuditsAsync(int companyId, int? userId = null, AuditFilters? filters = null, string include = null, bool useStatic = false)
        {
            var output = new List<Audit>();
            string language = Culture;
            bool useStaticStorage = useStatic;

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if(filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.StartTimestamp.HasValue && filters.Value.StartTimestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_starttimestamp", filters.Value.StartTimestamp.Value));
                    }

                    if (filters.Value.EndTimestamp.HasValue && filters.Value.EndTimestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_endtimestamp", filters.Value.EndTimestamp.Value));
                    }

                    if (filters.Value.IsCompleted.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_iscomplete", filters.Value.IsCompleted.Value));
                    }

                    if(filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                    }

                    if (filters.Value.TemplateId.HasValue && filters.Value.TemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_templateid", filters.Value.TemplateId.Value));
                    }

                    if (filters.Value.TimespanType.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timespanindays", (int)filters.Value.TimespanType.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }
                }

                using (dr = await _manager.GetDataReader(useStaticStorage ? "get_audits_static" : "get_audits", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var audit = useStaticStorage ? CreateOrFillStaticAuditFromReader(dr) : CreateOrFillAuditFromReader(dr);
                        if(audit != null && audit.Id > 0)
                        {
                            output.Add(audit);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.GetAuditsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);

            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && filters.Value.HasFilters())
            {
                output = (await FilterAudits(companyId: companyId, userId: userId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            }

            if(output.Count > 0)
            {
                if(!useStaticStorage)
                {
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tasks.ToString().ToLower())) output = await AppendAuditTasksAsync(companyId: companyId, audits: output, filters: filters, userId: userId, include: include);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) output = await AppendPropertiesToAudits(audits: output, companyId: companyId, include: include);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToAuditsAsync(audits: output, companyId: companyId);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.UserInformation.ToString().ToLower()))) output = await AppendUserInformationToAuditsAsync(audits: output, companyId: companyId);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Language.ToString().ToLower())) output = await AppendTranslationsToAuditsAsync(audits: output, companyId: companyId, language);

                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
                    {
                        output = await ApplyTemplateVersionToAudits(output, companyId, include);
                    }
                }
                else
                {
                    output = await GetDynamicCountersForAudits(audits: output, parameters: Utils.Tools.Copier.DeepCopy(npgsqlParameters: parameters));


                }
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) output = await AppendAreaPathsToAuditsAsync(companyId: companyId, audits: output, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
            }

            return output;
        }

        private async Task<List<Audit>> ApplyTemplateVersionToAudits(List<Audit> audits, int companyId, string include = null)
        {
            //cache versioned templates based on template id and version
            Dictionary<KeyValuePair<int, string>, AuditTemplate> VersionedAuditsCache = new();
            foreach (Audit audit in audits)
            {
                if (!string.IsNullOrEmpty(audit.Version) && audit.Version != await _flattenedAuditManager.RetrieveLatestAvailableVersion(audit.TemplateId, companyId))
                {
                    AuditTemplate versionedTemplate = null;
                    KeyValuePair<int, string> TemplateIdVersionPair = new(audit.TemplateId, audit.Version);

                    if (VersionedAuditsCache.ContainsKey(TemplateIdVersionPair))
                    {
                        //get correct version of template from cache if it is already present
                        versionedTemplate = VersionedAuditsCache.GetValueOrDefault(TemplateIdVersionPair);
                    }
                    else
                    {
                        //retrieve the correct version of the template from the database and add it to the cache
                        versionedTemplate = await _flattenedAuditManager.RetrieveFlattenData(templateId: audit.TemplateId, companyId: companyId, version: audit.Version);
                        VersionedAuditsCache.Add(TemplateIdVersionPair, versionedTemplate);
                    }

                    if (versionedTemplate != null)
                        audit.ApplyTemplateVersion(versionedTemplate, include);
                    else
                        _logger.LogWarning($"ApplyTemplateVersionToAudits(); Template version not applied because requested version wasn't found. AuditTemplateId: {audit.TemplateId}, version: {audit.Version}");
                }
            }
            return audits;
        }

        /// <summary>
        /// GetAuditAsync; Get a single audit object based on the AuditId parameter. Based on the [audits_audit] table in the database.
        /// Following stored procedures will be used for database data retrieval: "get_audit_static" OR "get_audit"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <param name="include">Include, comma separated string based on IncludesEnum, used for including extra data.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <param name="useStatic">Use static data set or the live one.</param>
        /// <returns>Audit object.</returns>
        public async Task<Audit> GetAuditAsync(int companyId, int auditId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false)
        {
            var audit = new Audit();
            string language = Culture;
            bool useStaticStorage = useStatic;

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", auditId));

                using (dr = await _manager.GetDataReader(useStaticStorage ? "get_audit_static" : "get_audit", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        if (useStaticStorage)
                        {
                            audit = CreateOrFillStaticAuditFromReader(dr, audit: audit);
                        }
                        else
                        {
                            audit = CreateOrFillAuditFromReader(dr, audit: audit);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.GetAuditAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (audit.Id > 0)
            {
                if (!useStaticStorage)
                {
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) audit.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, id: audit.Id, objectType: ObjectTypeEnum.Audit, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tasks.ToString().ToLower())) audit.Tasks = await GetTasksWithAuditAsync(companyId: companyId, audit.Id, include: include, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) audit = await AppendPropertiesToAudit(audit: audit, companyId: companyId, include: include, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.UserInformation.ToString().ToLower()))) audit = await AppendUserInformationToAuditAsync(audit: audit, companyId: companyId);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertiesGen4.ToString().ToLower()))) audit = await ReplacePropertiesWithPropertiesGen4(audit: audit, companyId: companyId);

                    if (!string.IsNullOrEmpty(audit.Version) && audit.Version != await _flattenedAuditManager.RetrieveLatestAvailableVersion(audit.TemplateId, companyId) && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
                    {
                        audit = await ApplyTemplateVersionToAudit(audit, companyId, include);
                    }
                
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Language.ToString().ToLower()))
                    {
                        var auditList = new List<Audit> { audit };
                        auditList = await AppendTranslationsToAuditlistsAsync(auditList, companyId, language);
                        audit = auditList.First();
                    }
                    return audit;
                }
                else
                {
                    audit = await GetDynamicCountersForAudit(audit: audit, parameters: Utils.Tools.Copier.DeepCopy(npgsqlParameters: parameters), connectionKind: connectionKind);
                }
                return audit;
            }
            else
            {
                return null;
            }
        }

        private async Task<Audit> ApplyTemplateVersionToAudit(Audit audit, int companyId, string include = null)
        {
            if (!string.IsNullOrEmpty(audit.Version))
            {
                AuditTemplate versionedTemplate = await _flattenedAuditManager.RetrieveFlattenData(templateId: audit.TemplateId, companyId: companyId, version: audit.Version);

                if (versionedTemplate != null)
                    audit.ApplyTemplateVersion(versionedTemplate, include);
                else
                    _logger.LogWarning($"ApplyTemplateVersionToAudit(); Template version not applied because requested version wasn't found. AuditTemplateId: {audit.TemplateId}, version: {audit.Version}");
            }
            return audit;
        }

        /// <summary>
        /// AddAuditAsync; Adds a audit to the database.
        /// Following stored procedures will be used for database data retrieval: "add_audit"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audit">Audit object (DB: audits_audit)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>The identity of the table (DB: audits_audit.id)</returns>
        public async Task<int> AddAuditAsync(int companyId, int userId, Audit audit)
        {
            //if audit lacks a version, and fallback is enabled, use latest version of template.
            if (audit.TemplateId > 0 && string.IsNullOrEmpty(audit.Version) && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA_FALLBACK"))
            {
                audit.Version = await _flattenedAuditManager.RetrieveLatestAvailableVersion(audit.TemplateId, companyId);
            }

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAudit(audit: audit, companyId: companyId));

            if (!string.IsNullOrEmpty(audit.Version))
            {
                parameters.Add(new NpgsqlParameter("@_version", audit.Version));
            }

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_audit", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if(possibleId > 0)
            {
                if (possibleId > 0)
                {
                    audit.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, ObjectTypeEnum.AuditTemplate, id: audit.TemplateId);
                    await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.Audit, id: possibleId, tags: audit.Tags, companyId: companyId, userId: userId);
                }

                if (audit.Tasks != null && audit.Tasks.Count > 0)
                {
                    var signedUserId = (audit.Signatures != null && audit.Signatures.Count > 0 && audit.Signatures[0].SignedById.HasValue ? audit.Signatures[0].SignedById.Value : 0); //retrieve possible owner id of item based on signature
                    var rowsId = await ChangeAuditAddOrChangeTask(companyId: companyId, userId: userId, possibleOwnerId: signedUserId, auditId: possibleId, audit.Tasks);
                }


                if(audit.PropertyUserValues != null && audit.PropertyUserValues.Count > 0)
                {
                    var result = await AddChangeAuditPropertyUserValue(companyId: companyId, auditId: possibleId, userId: userId, audit.PropertyUserValues);
                }

                if(audit.OpenFieldsPropertyUserValues != null && audit.OpenFieldsPropertyUserValues.Count > 0)
                {
                    var result = await AddChangeAuditPropertyUserValue(companyId: companyId, auditId: possibleId, userId: userId, audit.OpenFieldsPropertyUserValues);
                }

                if (audit.OpenFieldsPropertiesGen4 != null && audit.OpenFieldsPropertiesGen4.Count > 0)
                {
                    foreach (PropertyDTO property in audit.OpenFieldsPropertiesGen4)
                    {
                        property.UserValue.AuditId = possibleId;
                        var result = await _propertyValueManager.AddAuditPropertyUserValueAsync(companyId: companyId, property: property, userId: userId);
                    }
                }

                if (audit.LinkedTaskId.HasValue && audit.LinkedTaskId.Value > 0)
                {
                    var result = await AddTaskAuditLinkAsync(companyId: companyId, userId: userId, taskId: audit.LinkedTaskId.Value, auditId: possibleId, isRequired: audit.IsRequiredForLinkedTask == true);
                }

                var calcresult = await SetAuditCalculatedScoreAsync(companyId: companyId, userId: userId, auditId: possibleId);

            }

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added audit.");

            }

            return possibleId;
        }

        /// <summary>
        /// ChangeAuditAsync; Change a Audit.
        /// Following stored procedures will be used for database data retrieval: "change_audit"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="audit">Audit object containing all data needed for updating the database. (DB: audits_audit)</param>
        /// <param name="auditId">AuditId, id of the object in the database that needs to be updated. (DB: audits_audit.id)</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeAuditAsync(int companyId, int userId, int auditId, Audit audit)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAudit(audit: audit, companyId: companyId, auditId: auditId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_audit", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (auditId > 0 && audit.Tasks != null)
            {
                var signedUserId = (audit.Signatures != null && audit.Signatures.Count > 0 ? audit.Signatures[0].SignedById.Value : 0); //retrieve possible owner id of item based on signature
                var rowsId = await ChangeAuditAddOrChangeTask(companyId: companyId, userId: userId, possibleOwnerId: signedUserId, auditId: auditId, audit.Tasks);
            }

            if (audit.PropertyUserValues != null && audit.PropertyUserValues.Count > 0)
            {
                var result = await AddChangeAuditPropertyUserValue(companyId: companyId, auditId: auditId, userId: userId, audit.PropertyUserValues);
            }

            if (audit.OpenFieldsPropertyUserValues != null && audit.OpenFieldsPropertyUserValues.Count > 0)
            {
                var result = await AddChangeAuditPropertyUserValue(companyId: companyId, auditId: auditId, userId: userId, audit.OpenFieldsPropertyUserValues);
            }

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: auditId, userId: userId, companyId: companyId, description: "Changed audit.");

            }
            return rowseffected > 0;
        }

        /// <summary>
        /// CreateAuditAsync; Create a new audit based on a relation object.
        /// Following stored procedures will be used for database data retrieval: "create_audit"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditRelation">AuditRelationStatus object containing all data for creating a audit based on a template and a list of tasks.</param>
        /// <returns>AuditRelationStatus object</returns>
        public async Task<AuditRelationStatus> CreateAuditAsync(int companyId, int userId, AuditRelationStatus auditRelation)
        {
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                parameters.Add(new NpgsqlParameter("@_audittemplate_id", auditRelation.AuditTemplateId));
                parameters.Add(new NpgsqlParameter("@_tasktemplate_id", auditRelation.TaskTemplateId));
                parameters.Add(new NpgsqlParameter("@_status", auditRelation.TaskStatus.ToDatabaseString())); //TODO make correct converter method.

                using (dr = await _manager.GetDataReader("create_audit", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: ConnectionKind.Writer))
                {
                    while (await dr.ReadAsync())
                    {
                        auditRelation.AuditId = Convert.ToInt32(dr["audit_id"]);
                        if (dr["task_id"] != DBNull.Value)
                        {
                            auditRelation.TaskId = Convert.ToInt32(dr["task_id"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.CreateAuditAsync()", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if(auditRelation.AuditId.HasValue && auditRelation.AuditId > 0)
            {
                //TODO Create data row json with children objects for audits and checklists
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditRelation.AuditId.Value);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: auditRelation.AuditId.Value, userId: userId, companyId: companyId, description: "Added audit.");
            }

            return auditRelation;
        }

        /// <summary>
        /// CreateAuditAsync; Create a new audit based on a relation object.
        /// Following stored procedures will be used for database data retrieval: "create_audit"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditRelation">AuditRelationStatus object containing all data for creating a audit based on a template and a list of tasks.</param>
        /// <returns>AuditRelationStatus object</returns>
        public async Task<AuditRelationStatusScore> CreateAuditAsync(int companyId, int userId, AuditRelationStatusScore auditRelation)
        {
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                parameters.Add(new NpgsqlParameter("@_audittemplate_id", auditRelation.AuditTemplateId));
                parameters.Add(new NpgsqlParameter("@_tasktemplate_id", auditRelation.TaskTemplateId));
                parameters.Add(new NpgsqlParameter("@_status", auditRelation.TaskStatus.ToDatabaseString()));
                parameters.Add(new NpgsqlParameter("@_score", auditRelation.Score));

                using (dr = await _manager.GetDataReader("create_audit", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: ConnectionKind.Writer))
                {
                    while (await dr.ReadAsync())
                    {
                        auditRelation.AuditId = Convert.ToInt32(dr["audit_id"]);
                        if (dr["task_id"] != DBNull.Value)
                        {
                            auditRelation.TaskId = Convert.ToInt32(dr["task_id"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.CreateAuditAsync()", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (auditRelation.AuditId.HasValue && auditRelation.AuditId > 0)
            {
                //Create data row json with children objects for audits and checklists
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditRelation.AuditId.Value);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: auditRelation.AuditId.Value, userId: userId, companyId: companyId, description: "Added audit.");
            }

            return auditRelation;
        }

        /// <summary>
        /// SetAuditTaskStatusAsync; Set a status of a task based on a audit.
        /// The following values must be supplied
        /// - AuditId and TaskTemplateId
        /// Or
        /// - TaskId
        /// Depending on what is supplied, the method will update the task status that corresponds to the TaskId or the Combination of AuditId and TaskTemplateId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditRelation">Relation object containing all necessary ids</param>
        /// <returns>AuditRelationStatus object.</returns>
        public async Task<AuditRelationStatus> SetAuditTaskStatusAsync(int companyId, int userId, AuditRelationStatus auditRelation)
        {
            if (auditRelation.AuditId.HasValue && !auditRelation.TaskId.HasValue)
            {
                //task id not supplied, go fish for id. //TODO REFACTOR CREATE SEPERATE METHOD
                var tasks = await GetTasksWithAuditAsync(companyId: companyId, auditId: auditRelation.AuditId.Value);
                var foundTask = tasks.Where(x => x.TemplateId == auditRelation.TaskTemplateId).FirstOrDefault();
                if (foundTask != null)
                {
                    auditRelation.TaskId = foundTask.Id;
                }
            }
            if (auditRelation.TaskId.HasValue && auditRelation.TaskId > 0)
            {
                var result = await _taskManager.SetTaskStatusAsync(companyId: companyId, taskId: Convert.ToInt32(auditRelation.TaskId.Value), userId: userId, auditRelation.TaskStatus);
            }
            //TODO fix long conversion.
            return auditRelation;
        }

        /// <summary>
        /// SetAuditTaskStatusScoreAsync; Set the task and score of a task.
        /// Following stored procedures will be used for database data retrieval: "set_task_audit_status"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditRelation">AuditRelation object.</param>
        /// <returns>relation object with partly generated data (id's etc)</returns>
        public async Task<AuditRelationStatusScore> SetAuditTaskStatusScoreAsync(int companyId, int userId, AuditRelationStatusScore auditRelation)
        {
            if (auditRelation.AuditId.HasValue && !auditRelation.TaskId.HasValue)
            {
                //task id not supplied, go fish for id. //TODO REFACTOR CREATE SEPERATE METHOD
                var tasks = await GetTasksWithAuditAsync(companyId: companyId, auditId: auditRelation.AuditId.Value);
                var foundTask = tasks.Where(x => x.TemplateId == auditRelation.TaskTemplateId).FirstOrDefault();
                if (foundTask != null)
                {
                    auditRelation.TaskId = foundTask.Id;
                }
            }
            if (auditRelation.TaskId.HasValue && auditRelation.TaskId > 0)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_task.ToString(), Convert.ToInt32(auditRelation.TaskId.Value));

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", auditRelation.TaskId.Value));
                parameters.Add(new NpgsqlParameter("@_status", auditRelation.TaskStatus.ToDatabaseString()));
                parameters.Add(new NpgsqlParameter("@_score", auditRelation.Score));

                var rowseffected = Convert.ToInt32 (await _manager.ExecuteScalarAsync("set_task_audit_status", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                if(rowseffected > 0)
                {
                    var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_task.ToString(), Convert.ToInt32(auditRelation.TaskId.Value));
                    await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.tasks_task.ToString(), objectId: Convert.ToInt32(auditRelation.TaskId.Value), userId: userId, companyId: companyId, description: "Changed audit task status and/or score.");
                }


            }
            return auditRelation;
        }

        /// <summary>
        /// SetAuditActiveAsync; Set Audit active/inactive based on AuditId.
        /// Following stored procedures will be used for database data retrieval: "set_audit_active"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a Audit to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetAuditActiveAsync(int companyId, int userId, int auditId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", auditId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_audit_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if(rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: auditId, userId: userId, companyId: companyId, description: "Changed audit active state.");

            }
            return (rowseffected > 0);
        }

        /// <summary>
        /// SetAuditActiveAsync; Set Audit active/inactive based on AuditId.
        /// Following stored procedures will be used for database data retrieval: "set_audit_score"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <param name="score">Score numeric value.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetAuditScoreAsync(int companyId, int userId, int auditId, int score)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", auditId));
            parameters.Add(new NpgsqlParameter("@_score", score));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_audit_score", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: auditId, userId: userId, companyId: companyId, description: "Changed audit score.");

            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// NOT YET IMPLEMENTED! ONLY STUB.
        /// </summary>
        public async Task<bool> SetAuditCompleteAsync(int companyId, int userId, int auditId, bool isComplete = true)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// SetAuditCalculatedScoreAsync; Set the calculated scores of a inserted audit. Based on the settings in the database with the template of the supplied audit.
        /// Following stored procedures will be used for database data retrieval: "set_audit_calculated_score"
        /// </summary>
        /// <param name="companyId">companyId; CompanyId (DB: companies_company.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <returns></returns>
        public async Task<bool> SetAuditCalculatedScoreAsync(int companyId, int userId, int auditId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", auditId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_audit_calculated_score", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: auditId, userId: userId, companyId: companyId, description: "Changed audit score.");

            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// AuditSigningAsync; Sign a audit with one or more signatures.
        /// Following stored procedures will be used for database data retrieval: "sign_audit"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <param name="signing">Signing relation object containing signatures and ids</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> AuditSigningAsync(int companyId, int userId, int auditId, AuditRelationSigning signing)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", signing.AuditId));
            for (var i = 0; i < signing.Signatures.Count; i++)
            {
                var signature = signing.Signatures[i];
                var databaseNumber = i + 1;

                //example of signature db names: signature_1, signed_by_1_id, signed_by_1
                parameters.Add(new NpgsqlParameter(string.Concat("@_signature", databaseNumber), string.IsNullOrEmpty(signature.SignatureImage) ? "" : signature.SignatureImage));
                parameters.Add(new NpgsqlParameter(string.Concat("@_signedbyid", databaseNumber), signature.SignedById));
                parameters.Add(new NpgsqlParameter(string.Concat("@_signedby", databaseNumber), string.IsNullOrEmpty(signature.SignedBy) ? "" : signature.SignedBy));
            }
            if (signing.Signatures.Any())
            {
                var signedAt = signing.Signatures[0].SignedAt;
                parameters.Add(new NpgsqlParameter("@_signedat", signedAt.HasValue && signedAt.Value != DateTime.MinValue ? signedAt.Value : DateTime.Now.ToUniversalTime()));
            }

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("sign_audit", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit.ToString(), auditId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audit.ToString(), objectId: auditId, userId: userId, companyId: companyId, description: "Signed audit.");

            }

            return (rowseffected > 0);
        }
        #endregion

        #region - public methods AuditTemplates -
        /// <summary>
        /// GetAuditTemplatesAsync; Get a list of audittemplates based on the CompanyId.
        /// Following stored procedures will be used for database data retrieval: "get_audittemplates"
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id).</param>
        /// <param name="userId">UserId (user_profiles.id).</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <param name="include">Includes, based on enum for retrieval of extra data.</param>
        /// <returns>A list of AuditTemplates objects.</returns>
        public async Task<List<AuditTemplate>> GetAuditTemplatesAsync(int companyId, int? userId = null, AuditFilters? filters = null, string include = null)
        {
            var output = new List<AuditTemplate>();

            string language = Culture;
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    //roles filter
                    if (filters.Value.Roles != null && filters.Value.Roles.Count > 0)
                    {
                        var rolesFilterValue = new List<string>();

                        foreach (var role in filters.Value.Roles)
                        {
                            if (role == RoleTypeEnum.Basic)
                            {
                                rolesFilterValue.Add("basic");
                            }
                            else if (role == RoleTypeEnum.ShiftLeader)
                            {
                                rolesFilterValue.Add("shift_leader");
                            }
                            else if (role == RoleTypeEnum.Manager)
                            {
                                rolesFilterValue.Add("manager");
                            }
                        }

                        parameters.Add(new NpgsqlParameter("@_roles", rolesFilterValue.ToArray()));
                    }

                    //filtertext
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    //instructions added
                    if (filters.Value.InstructionsAdded != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_instructionsadded", filters.Value.InstructionsAdded));
                    }

                    //photos added
                    if (filters.Value.ImagesAdded != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_imagesadded", filters.Value.ImagesAdded));
                    }
                }

                using (dr = await _manager.GetDataReader("get_audittemplates_v2", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var audittemplate = CreateOrFillAuditTemplateFromReader(dr);
                        output.Add(audittemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.GetAuditTemplatesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && filters.Value.HasFilters())
            {
                output = (await FilterAuditTemplates(companyId: companyId, userId:userId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            }

            if (output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.TaskTemplates.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower()))) output = await AppendAuditTemplateTaskTemplatesAsync(companyId: companyId, audittemplates: output, filters:filters, userId: userId, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower())) output = await AppendAuditTemplateStepsAsync(companyId: companyId, audittemplates: output, filters: filters, userId: userId);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) output = await AppendAreaPathsToAuditTemplatesAsync (companyId: companyId, audittemplates: output, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))) output = await AppendTemplatePropertiesToTaskTemplates(audittemplates: output, companyId: companyId, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) output = await AppendTemplatePropertiesToTemplates(audittemplates: output, companyId: companyId, include: include);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower()))) output = await AppendWorkInstructionRelationsAsync(auditTemplates: output, companyId: companyId);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToAuditTemplatesAsync(auditTemplates: output, companyId: companyId);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Language.ToString().ToLower()))) output = await AppendTranslationsToAuditTemplatesAsync(companyId: companyId, auditTemplates: output, language);

            }

            return output;
        }

        /// <summary>
        /// GetAuditTemplateCountsAsync; Get counts of audittemplates with selected filters.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="include">Comma separated string based on the IncludesTypeEnum</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <returns>A count of AuditTemplates.</returns>
        public async Task<AuditTemplateCountStatistics> GetAuditTemplateCountsAsync(int companyId, int? userId = null, AuditFilters? filters = null, string include = null)
        {
            var output = new AuditTemplateCountStatistics();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    if (filters.Value.AreaId.HasValue && filters.Value.AreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", userId.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    //roles filter
                    if (filters.Value.Roles != null && filters.Value.Roles.Count > 0)
                    {
                        var rolesFilterValue = new List<string>();

                        foreach (var role in filters.Value.Roles)
                        {
                            if (role == RoleTypeEnum.Basic)
                            {
                                rolesFilterValue.Add("basic");
                            }
                            else if (role == RoleTypeEnum.ShiftLeader)
                            {
                                rolesFilterValue.Add("shift_leader");
                            }
                            else if (role == RoleTypeEnum.Manager)
                            {
                                rolesFilterValue.Add("manager");
                            }
                        }

                        parameters.Add(new NpgsqlParameter("@_roles", rolesFilterValue.ToArray()));
                    }

                    //filtertext
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    //instructions added
                    if (filters.Value.InstructionsAdded != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_instructionsadded", filters.Value.InstructionsAdded));
                    }

                    //photos added
                    if (filters.Value.ImagesAdded != null)
                    {
                        parameters.Add(new NpgsqlParameter("@_imagesadded", filters.Value.ImagesAdded));
                    }
                }

                using (dr = await _manager.GetDataReader("get_audittemplates_v2_counts", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.TotalCount = Convert.ToInt32(dr["total_count"]);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.GetAuditTemplateCountsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetAuditTemplateAsync; Get a single audit template object based on the AuditTemplateId parameter.
        /// Based on the [audits_audittemplate] table in the database.
        /// Following stored procedures will be used for database data retrieval: "get_audittemplate"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <param name="include">Includes, based on enum for retrieval of extra data.</param>
        /// <returns>AuditTemplate object.</returns>
        public async Task<AuditTemplate> GetAuditTemplateAsync(int companyId, int auditTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var audittemplate = new AuditTemplate();
            string language = Culture;

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", auditTemplateId));

                using (dr = await _manager.GetDataReader("get_audittemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillAuditTemplateFromReader(dr, audittemplate: audittemplate);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.GetAuditTemplateAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (audittemplate.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.TaskTemplates.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower()))) audittemplate.TaskTemplates = await GetTaskTemplatesWithAuditTemplateAsync(companyId: companyId, auditTemplateId: audittemplate.Id, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower())) audittemplate = await AppendAuditTemplateStepsAsync(companyId: companyId, audittemplate: audittemplate);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))) audittemplate = await AppendTemplatePropertiesToTaskTemplates(audittemplate: audittemplate, companyId: companyId, include:include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) audittemplate = await AppendTemplatePropertiesToTemplate(audittemplate: audittemplate, companyId: companyId, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) audittemplate.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.AuditTemplate, id: auditTemplateId);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Language.ToString().ToLower()))
                {
                    var tempList = new List<AuditTemplate> { audittemplate };
                    tempList = await AppendTranslationsToAuditTemplatesAsync(companyId: companyId, auditTemplates: tempList, language: language);
                    audittemplate = tempList.First();
                }
                return audittemplate;
            }
            else
            {
                return null;
            }
        }

        public async Task<Dictionary<int, string>> GetAuditTemplateNamesAsync(int companyId, List<int> audittemplateIds)
        {
            Dictionary<int, string> idsNames = new();
            string language = Culture;

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_language", language),
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_audittemplateids", audittemplateIds)
                };

                using NpgsqlDataReader dr = await _manager.GetDataReader("get_audittemplate_names", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    int id = Convert.ToInt32(dr["id"]);
                    string name = dr["name"].ToString();
                    idsNames.Add(id, name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaManager.GetAreaNamesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return idsNames;
        }

        /// <summary>
        /// AddAuditTemplateAsync; Add an AuditTemplate to the database.
        /// Following stored procedures will be used for database data retrieval: "add_audittemplate"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: user_profiles.id)</param>
        /// <param name="auditTemplate">AuditTemplate object filled with data for a AuditTemplate.</param>
        /// <returns>The identity of the table (DB: audits_audittemplate.id)</returns>
        public async Task<int> AddAuditTemplateAsync(int companyId, int userId, AuditTemplate auditTemplate)
        {
            //@Name, @Description, @Picture, @DoubleSignatureRequired, @ScoreType, @MinTaskScore, @MaxTaskScore, @AreaId, @CompanyId, @Role, @SignatureRequired
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAuditTemplate(auditTemplate: auditTemplate, companyId: auditTemplate.CompanyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_audittemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                if (auditTemplate.Tags != null && auditTemplate.Tags.Count > 0)
                {
                    await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.AuditTemplate, possibleId, auditTemplate.Tags, companyId, userId);
                }

                if (auditTemplate.TaskTemplates != null && auditTemplate.TaskTemplates.Count > 0)
                {
                    auditTemplate.Id = possibleId; //set id for further processing
                    await ChangeAuditTemplateAddOrChangeTaskTemplates(companyId: companyId, userId: userId, auditTemplateId: possibleId, auditTemplate.TaskTemplates);
                }

                if(auditTemplate.Properties != null && auditTemplate.Properties.Count > 0)
                {
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: possibleId, templateProperties: auditTemplate.Properties);
                }

                if(auditTemplate.OpenFieldsProperties != null && auditTemplate.OpenFieldsProperties.Count > 0)
                {
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: possibleId, templateProperties: auditTemplate.OpenFieldsProperties);
                }
            }

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.audits_audittemplate.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added audit template.");
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeAuditTemplateAsync; Change a AuditTemplate in the Database.
        /// Following stored procedures will be used for database data retrieval: "change_audittemplate"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: user_profiles.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <param name="auditTemplate">AuditTemplate object filled with data for a AuditTemplate update.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeAuditTemplateAsync(int companyId, int userId, int auditTemplateId, AuditTemplate auditTemplate)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate.ToString(), auditTemplateId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAuditTemplate(auditTemplate: auditTemplate, companyId: companyId, auditTemplateId: auditTemplateId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_audittemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                auditTemplate.Tags ??= new();
                await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.AuditTemplate, auditTemplateId, auditTemplate.Tags, companyId, userId);

                if (auditTemplate.TaskTemplates != null)
                {
                    await ChangeAuditTemplateAddOrChangeTaskTemplates(companyId: companyId, userId: userId, auditTemplateId: auditTemplateId, auditTemplate.TaskTemplates);
                }

                if (auditTemplate.Properties != null)
                {
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: auditTemplateId, templateProperties: auditTemplate.Properties);
                }

                if (auditTemplate.OpenFieldsProperties != null)
                {
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: auditTemplateId, templateProperties: auditTemplate.OpenFieldsProperties);
                }
            }

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate.ToString(), auditTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audittemplate.ToString(), objectId: auditTemplateId, userId: userId, companyId: companyId, description: "Changed audit template.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// SetAuditTemplateActiveAsync; Set AuditTemplate active/inactive based on AuditTemplateId.
        /// Following stored procedures will be used for database data retrieval: "set_audittemplate_active"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: user_profiles.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a AuditTemplate to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetAuditTemplateActiveAsync(int companyId, int userId, int auditTemplateId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate.ToString(), auditTemplateId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", auditTemplateId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_audittemplate_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate.ToString(), auditTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audittemplate.ToString(), objectId: auditTemplateId, userId: userId, companyId: companyId, description: "Changed audit template active state.");

            }

            return (rowseffected > 0);
        }
        #endregion

        #region - public methods AuditTemplate connections - 
        /// <summary>
        /// GetConnectedTaskTemplateIds; Get connected TaskTemplates to this AuditTemplate
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId, the id of the object to get from the database. (DB: audits_audit.id)</param>
        /// <returns>List<int> with all task template ids</returns>
        public async Task<List<int>> GetConnectedTaskTemplateIds(int companyId, int auditTemplateId)
        {
            var taskTemplateIds = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_audittemplateid", auditTemplateId));

                using (dr = await _manager.GetDataReader("get_audittemplate_linked_tasktemplates", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        taskTemplateIds.Add(Convert.ToInt32(dr["tasktemplate_id"]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuditManager.GetConnectedTaskTemplateIds(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return taskTemplateIds;
        }
        #endregion

        #region - private methods Filter Audits -
        /// <summary>
        /// FilterAudits; FilterAudits is the primary filter method for filtering audits. Within this method the specific filters are determined based on the supplied AuditFilters object.
        /// Filtering is done based on cascading filters, meaning, the first filter is applied, which results in a filtered collection.
        /// On that filtered collection the second filter is applied which results in a filtered-filtered collection.
        /// This will continue until all filters are applied.
        /// NOTE! Way of filtering obsolete; All filter methods used here if still used needs to be moved when refactoring to front query filters (e,g, query parameters and let the database do the filtering)/// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="filters">AuditFilters, depending on the values certain filters will be applies.</param>
        /// <param name="nonFilteredCollection">List of non filtered Audit objects.</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>A filtered list of Audit objects.</returns>
        private async Task<IList<Audit>> FilterAudits(int companyId, AuditFilters filters, IList<Audit> nonFilteredCollection, int? userId = null)
        {
            var filtered = nonFilteredCollection;
            if (filters.SignedById.HasValue)
            {
                filtered = await FilterAuditsOnSignedOnId(signedById: filters.SignedById.Value, audits: filtered);
            }
            if (filters.TemplateId.HasValue)
            {
                filtered = await FilterAuditsOnTemplateId(templateId: filters.TemplateId.Value, audits: filtered);
            }
            if (filters.ScoreType.HasValue)
            {
                filtered = await FilterAuditsOnScoreType(scoretype: filters.ScoreType.Value, audits: filtered);
            }
            return filtered;
        }

        /// <summary>
        /// FilterAuditsOnArea; Filter a Audits collection on AreaId.
        /// </summary>
        /// <param name="areaId">AreaId ( DB: audits_audit.area_id)</param>
        /// <param name="audits">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Audit>> FilterAuditsOnArea(int areaId, IList<Audit> audits)
        {

            audits = audits.Where(x => x.AreaId == areaId).ToList();
            await Task.CompletedTask; //used for making method async executable.
            return audits;
        }


        /// <summary>
        /// FilterAuditsOnArea; Filter a Audit collection on AreaId and a FilterAreaType. Depending on type a recursive filter is being used based on the children of a Area.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="areaId">AreaId ( DB: audits_audit.area_id)</param>
        /// <param name="filterType">FilterAreaTypeEnum, type based on Single, RootToLeaf and LeafToRoot filtering.</param>
        /// <param name="audits">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Audit>> FilterAuditsOnArea(int companyId, int areaId, FilterAreaTypeEnum filterType, IList<Audit> audits)
        {
            var areas = await _areaBasicManager.GetAreasBasicByStartAreaAsync(companyId: companyId, areaId: areaId, areaFilterType: filterType);
            if (areas == null || areas.Count == 0)
            {
                areas.Add(new Models.Basic.AreaBasic() { Id = areaId, Name = "" });
            }
            //get data
            audits = audits.Where(x => areas.Select(a => a.Id).Contains(x.AreaId)).ToList();
            await Task.CompletedTask;
            return audits;
            // return nonFilteredCollection;
        }

        /// <summary>
        /// FilterAuditsOnIsCompleted; Filter a Audit collection on IsCompleted.
        /// </summary>
        /// <param name="isCompleted">IsCompleted ( DB: audits_audit.is_complete)</param>
        /// <param name="audits">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Audit>> FilterAuditsOnIsCompleted(bool isCompleted, IList<Audit> audits)
        {
            audits = audits.Where(x => x.IsCompleted == isCompleted).ToList();
            await Task.CompletedTask; //make method execute in async flow.
            return audits;
        }

        /// <summary>
        /// FilterAuditsOnSignedOnId; Filter a Audit collection on SignedById.
        /// </summary>
        /// <param name="signedById">SignedById ( DB: audits_audit.signed_by_id)</param>
        /// <param name="audits">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Audit>> FilterAuditsOnSignedOnId(int signedById, IList<Audit> audits)
        {
            audits = audits.Where(x => x.Signatures != null && x.Signatures.Where(y => y.SignedById == signedById).Any()).ToList();
            await Task.CompletedTask; //make method execute in async flow.
            return audits;
        }

        /// <summary>
        /// FilterAuditsOnTemplateId; Filter a Audit collection on TemplateId.
        /// </summary>
        /// <param name="templateId">TemplateId ( DB: audits_audit.template_id)</param>
        /// <param name="audits">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Audit>> FilterAuditsOnTemplateId(int templateId, IList<Audit> audits)
        {
            audits = audits.Where(x => x.TemplateId == templateId).ToList();
            await Task.CompletedTask; //make method execute in async flow.
            return audits;
        }

        /// <summary>
        /// FilterAuditsOnScoreType; Filter a Audit collection on role.
        /// </summary>
        /// <param name="scoretype">ScoreTypeEnum, scoretypes are stored as a string in the database. Internally we use a enumerator to represent those stings. (DB: audits_audittemplate.scoretype)</param>
        /// <param name="audits">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Audit>> FilterAuditsOnScoreType(ScoreTypeEnum scoretype, IList<Audit> audits)
        {

            audits = audits.Where(x => x.ScoreType == scoretype.ToString().ToLower()).ToList();
            await Task.CompletedTask;
            return audits;
        }

        /// <summary>
        /// FilterAreasAllowedOnly; Filter a audit collection based on the audits where a user should have access to.
        /// </summary>
        /// <param name="companyId">CompanyId of user (DB: companies.id)</param>
        /// <param name="userId">UserId of user (DB: profiles_user.id)</param>
        /// <param name="audits"></param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Audit>> FilterAuditsAllowedOnly(int companyId, int userId, IList<Audit> audits)
        {
            var allowedAuditTemplates = await _userAccessManager.GetAllowedAuditTemplateIdsWithUserAsync(companyId: companyId, userId: userId);

            audits = audits.Where(x => allowedAuditTemplates.Contains(x.TemplateId)).ToList();

            return audits;
        }
        #endregion

        #region - private methods Filter AuditTemplates -
        /// <summary>
        /// FilterAuditTemplates; FilterAuditTemplates is the primary filter method for filtering AuditTemplates, within this method the specific filters are determined based on the supplied AuditFilters object.
        /// Filtering is done based on cascading filters, meaning, the first filter is applied, which results in a filtered collection.
        /// On that filtered collection the second filter is applied which results in a filtered-filtered collection.
        /// This will continue until all filters are applied.
        /// NOTE! Way of filtering obsolete; All filter methods used here if still used needs to be moved when refactoring to front query filters (e,g, query parameters and let the database do the filtering)/// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">AuditFilters, depending on the values certain filters will be applies.</param>
        /// <param name="nonFilteredCollection">List of non filtered AuditTemplate objects.</param>
        /// <returns>A filtered list of AuditTemplate objects.</returns>
        private async Task<IList<AuditTemplate>> FilterAuditTemplates(int companyId, AuditFilters filters, IList<AuditTemplate> nonFilteredCollection, int? userId = null)
        {
            var filtered = nonFilteredCollection;

            if (filters.RoleType.HasValue)
            {
                filtered = await FilterAuditTemplatesOnRole(role: filters.RoleType.Value, auditTemplates: filtered);
            }
            if (filters.ScoreType.HasValue)
            {
                filtered = await FilterAuditTemplatesOnScoreType(scoretype: filters.ScoreType.Value, auditTemplates: filtered);
            }

            return filtered;
        }

        /// <summary>
        /// FilterAuditTemplatesOnRole; Filter a AuditTemplate collection on role.
        /// </summary>
        /// <param name="role">RoleTypeEnum, roles are stored as a string in the database. Internally we use a enumerator to represent those stings. ( DB: audits_audittemplate.role)</param>
        /// <param name="auditTemplates">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<AuditTemplate>> FilterAuditTemplatesOnRole(RoleTypeEnum role, IList<AuditTemplate> auditTemplates)
        {

            auditTemplates = auditTemplates.Where(x => x.Role == role.ToDatabaseString().ToString().ToLower()).ToList();
            await Task.CompletedTask;
            return auditTemplates;
        }

        /// <summary>
        /// FilterAuditTemplatesOnScoreType; Filter a AuditTemplate collection on role.
        /// </summary>
        /// <param name="scoretype">ScoreTypeEnum, scoretypes are stored as a string in the database. Internally we use a enumerator to represent those stings. (DB: audits_audittemplate.scoretype)</param>
        /// <param name="auditTemplates">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<AuditTemplate>> FilterAuditTemplatesOnScoreType(ScoreTypeEnum scoretype, IList<AuditTemplate> auditTemplates)
        {

            auditTemplates = auditTemplates.Where(x => x.ScoreType == scoretype.ToString().ToLower()).ToList();
            await Task.CompletedTask;
            return auditTemplates;
        }
        #endregion

        #region - private methods Audits -
        /// <summary>
        /// Adds relation between completed audit and the task it is linked to.
        /// </summary>
        /// <param name="taskId">task id</param>
        /// <param name="auditId">audit id</param>
        /// <param name="isRequired">set to true if the linked audit is mandatory before completing the task</param>
        /// <returns>id of the linking record</returns>
        private async Task<int> AddTaskAuditLinkAsync(int companyId, int userId, long taskId, int auditId, bool isRequired)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_task_id", taskId),
                new NpgsqlParameter("@_audit_id", auditId),
                new NpgsqlParameter("@_is_required", isRequired)
            };

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_task_audit_link", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_task_audit_link.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.tasks_task_audit_link.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added task audit link.");
            }

            return possibleId;
        }

        /// <summary>
        /// GetTasksWithAudit; Gets a list of Tasks with a Audit. These tasks are filled in by the user.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <returns>A List of Tasks, if the list returned 0 items, then null is returned.</returns>
        private async Task<List<TasksTask>> GetTasksWithAuditAsync(int companyId, int auditId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = await _taskManager.GetTasksByAuditIdAsync(companyId: companyId, auditId: auditId, include: include, connectionKind: connectionKind);
            if(output != null && output.Count > 0)
            {
                return output;
            }
            return null;
        }

        /// <summary>
        /// AppendAuditTasksAsync; Append tasks to audit object.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filter object containing filters; Will be used for creating task filters.</param>
        /// <param name="audits">Collection of audits.</param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. Used for including extra data. </param>
        /// <returns>The list of audits, appended with Tasks.</returns>
        private async Task<List<Audit>> AppendAuditTasksAsync(int companyId, List<Audit> audits, AuditFilters? filters = null, int? userId = null, string include = null)
        {
            var taskFilters = filters.ToTaskFilters();
            var tasks = await _taskManager.GetTasksWithAuditsAsync(companyId: companyId, auditIds: audits.Select(a => a.Id).ToList(),  userId: userId, filters: taskFilters, include: include);
            if(tasks != null && tasks.Count>0)
            {
                foreach(var audit in audits)
                {
                    audit.Tasks = tasks.Where(x => x.AuditId.HasValue && x.AuditId == audit.Id).ToList();
                }
            }

            return audits;
        }

        /// <summary>
        /// AppendAreaPathsToAuditsAsync; Add the AreaPath to the Audit. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audits">List of audits.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>Audits including area full path. </returns>
        private async Task<List<Audit>> AppendAreaPathsToAuditsAsync(int companyId, List<Audit> audits, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var audit in audits)
                {
                    var area = areas?.Where(x => x.Id == audit.AreaId)?.FirstOrDefault();
                    if(area != null)
                    {
                        if (addAreaPath) audit.AreaPath = area.FullDisplayName;
                        if (addAreaPathIds) audit.AreaPathIds = area.FullDisplayIds;
                    }
                }
            }
            return audits;
        }

        /// <summary>
        /// CreateOrFillAuditFromReader; creates and fills a Audit object from a DataReader.
        /// NOTE! intended for use with the audit stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audit">Audit object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled Audit object.</returns>
        private Audit CreateOrFillAuditFromReader(NpgsqlDataReader dr, Audit audit = null)
        {
            if(audit == null) audit = new Audit();

            audit.AreaId = Convert.ToInt32(dr["area_id"]);
            audit.Id = Convert.ToInt32(dr["id"]);
            audit.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                audit.Description = dr["description"].ToString();
            }
            audit.IsCompleted = Convert.ToBoolean(dr["is_complete"]);
            audit.IsDoubleSignatureRequired = Convert.ToBoolean(dr["double_signature_required"]);
            audit.IsSignatureRequired = Convert.ToBoolean(dr["signature_required"]);
            audit.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                audit.Picture = dr["picture"].ToString();
            }
            if (dr["score_type"] != DBNull.Value)
            {
                audit.ScoreType = dr["score_type"].ToString();

            }
            if (dr["signed_at_1"] != DBNull.Value && dr["signed_by_1_id"] != DBNull.Value)
            {
                if (audit.Signatures == null) audit.Signatures = new List<Signature>();
                audit.Signatures.Add(new Signature() { SignatureImage = dr["signature_1"].ToString(), SignedAt = Convert.ToDateTime(dr["signed_at_1"]), SignedById = Convert.ToInt32(dr["signed_by_1_id"]), SignedBy = dr["signed_by_1"].ToString() });
            }
            if (dr["signed_at_2"] != DBNull.Value && dr["signed_by_2"] != DBNull.Value)
            {
                if (audit.Signatures == null) audit.Signatures = new List<Signature>();
                audit.Signatures.Add(new Signature() { SignatureImage = dr["signature_2"].ToString(), SignedAt = Convert.ToDateTime(dr["signed_at_2"]), SignedById = Convert.ToInt32(dr["signed_by_2_id"]), SignedBy = dr["signed_by_2"].ToString() });
            }
            audit.TemplateId = Convert.ToInt32(dr["template_id"]);
            if (dr["total_score"] != DBNull.Value)
            {
                audit.TotalScore = Convert.ToInt32(dr["total_score"]);
            }
            if (dr["created_at"] != DBNull.Value)
            {
                audit.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                audit.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            if(dr.HasColumn("min_task_score"))
            {
                if (dr["min_task_score"] != DBNull.Value)
                {
                    audit.MinTaskScore = Convert.ToInt32(dr["min_task_score"]);
                }
            }

            if (dr.HasColumn("max_task_score"))
            {
                if (dr["max_task_score"] != DBNull.Value)
                {
                    audit.MaxTaskScore = Convert.ToInt32(dr["max_task_score"]);
                }
            }

            if (dr.HasColumn("version"))
            {
                if (dr["version"] != DBNull.Value)
                {
                    audit.Version = Convert.ToString(dr["version"]);
                }
            }

            return audit;

        }

        /// <summary>
        /// CreateOrFillStaticAuditFromReader; Create audit from static data store.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audit">Audit object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled Audit object.</returns>
        private Audit CreateOrFillStaticAuditFromReader(NpgsqlDataReader dr, Audit audit = null)
        {
            if (audit == null) audit = new Audit();

            if (dr["data_object"] != DBNull.Value)
            {
                audit = dr["data_object"].ToString().ToObjectFromJson<Audit>();
            }

            return audit;
        }

        /// <summary>
        /// GetNpgsqlParametersFromAudit; Creates a list of NpgsqlParameters, and fills it based on the supplied Audit object.
        /// NOTE! intended for use with the audit stored procedures within the database.
        /// </summary>
        /// <param name="audit">The supplied Audit object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromAudit(Audit audit, int companyId, int auditId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (auditId > 0) parameters.Add(new NpgsqlParameter("@_id", auditId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_templateid", audit.TemplateId));
            parameters.Add(new NpgsqlParameter("@_totalscore", Convert.ToInt16(audit.TotalScore)));
            parameters.Add(new NpgsqlParameter("@_iscompleted", audit.IsCompleted));

            //Due to the weird database structure, the first signature has an other structure than the second structure.
            //For this reason and if statement is added to handle the data differently.
            var signature = audit.Signatures != null && audit.Signatures.Count > 0 ? audit.Signatures[0] : new Signature(); //get first signature
            if (!string.IsNullOrEmpty(signature.SignatureImage))
            {
                parameters.Add(new NpgsqlParameter("@_signature1", signature.SignatureImage));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_signature1", DBNull.Value));
            }

            if (signature.SignedAt != null && signature.SignedAt != DateTime.MinValue)
            {
                parameters.Add(new NpgsqlParameter("@_signedat1", signature.SignedAt));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_signedat1", DBNull.Value));
            }

            if (signature.SignedById != null && signature.SignedById > 0)
            {
                parameters.Add(new NpgsqlParameter("@_signedbyid1", signature.SignedById));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_signedbyid1", DBNull.Value));
            }

            var signature2 = audit.Signatures != null && audit.Signatures.Count > 1 ? audit.Signatures[1] : new Signature(); //get first signature

            if (!string.IsNullOrEmpty(signature2.SignatureImage))
            {
                parameters.Add(new NpgsqlParameter("@_signature2", signature2.SignatureImage));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_signature2", DBNull.Value));
            }

            if (signature2.SignedAt != null && signature2.SignedAt != DateTime.MinValue)
            {
                parameters.Add(new NpgsqlParameter("@_signedat2", signature2.SignedAt));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_signedat2", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(signature2.SignedBy))
            {
                parameters.Add(new NpgsqlParameter("@_signedby2", signature2.SignedBy));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_signedby2", DBNull.Value));
            }

            return parameters;
        }

        #endregion

        #region - private methods AuditTemplates -
        /// <summary>
        /// GetTaskTemplatesWithAuditTemplateTemplate; Gets a list of TaskTemplates. These TaskTemplates are part of the AuditTemplates and can be used for creating a new Audit to be filled in by a User.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <returns>A List of TaskTemplates, if the list returned 0 items, then null is returned.</returns>
        private async Task<List<TaskTemplate>> GetTaskTemplatesWithAuditTemplateAsync(int companyId, int auditTemplateId, string include = "")
        {
            var output = await _taskManager.GetTaskTemplatesByAuditTemplateIdAsync(companyId: companyId, auditTemplateId: auditTemplateId, include);
            if (output != null && output.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower()))) output = await AppendWorkInstructionRelationsAsync(companyId: companyId, auditTemplateId: auditTemplateId, templateItems: output);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Instructions.ToString().ToLower()))) output = await AppendWorkInstructionsAsync(companyId: companyId, auditTemplateId: auditTemplateId, templateItems: output);

                return output;
            }
            return null;
        }

        /// <summary>
        /// AppendAuditTemplateTaskTemplatesAsync; Append TasksTemplates to AuditTemplate object.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">audit filters, will be used for creating task filters for further data retrieval</param>
        /// <param name="audittemplates">Collection of audittemplates.</param>
        /// <returns>The list of audits, appended with Tasks.</returns>
        private async Task<List<AuditTemplate>> AppendAuditTemplateTaskTemplatesAsync(int companyId, List<AuditTemplate> audittemplates, AuditFilters? filters = null, int? userId = null, string include = null)
        {
            var taskFilters = filters.ToTaskFilters();
            var tasktemplates = await _taskManager.GetTasksTemplatesWithAuditTemplatesAsync(companyId: companyId, auditIds: audittemplates.Select(c => c.Id).ToList(), filters: taskFilters, userId: userId, include: include);
            if (tasktemplates != null && tasktemplates.Count > 0)
            {
                foreach (var audittemplate in audittemplates)
                {
                    audittemplate.TaskTemplates = tasktemplates.Where(x => x.AuditTemplateId.HasValue && x.AuditTemplateId == audittemplate.Id).ToList();
                }
            }

            return audittemplates;
        }


        /// <summary>
        /// AppendAuditTemplateStepsAsync; Append Steps to AuditTemplate object (on each TaskTemplate if available).
        /// NOTE! if no TaskTemplates are available (e.g. the collection that is the source for the audits parameter doesn't have them). No steps will be available on output.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">audit filters, will be used for creating task filters for further data retrieval</param>
        /// <param name="audittemplates">Collection of audits templates.</param>
        /// <returns>The list of audits templates, appended with Steps.</returns>
        private async Task<List<AuditTemplate>> AppendAuditTemplateStepsAsync(int companyId, List<AuditTemplate> audittemplates, AuditFilters? filters = null, int? userId = null)
        {
            //TODO refactor
            var taskFilters = filters.ToTaskFilters();
            var steps = await _taskManager.GetTaskTemplateStepsWithAuditsAsync(companyId: companyId, userId: userId, filters: taskFilters);
            if (steps != null && steps.Count > 0)
            {
                foreach (var audit in audittemplates)
                {
                    if (audit.TaskTemplates != null && audit.TaskTemplates.Count > 0)
                    {
                        foreach (var tasktemplate in audit.TaskTemplates)
                        {
                            tasktemplate.Steps = steps.Where(x => x.TaskTemplateId == tasktemplate.Id).ToList();
                        }
                    }
                }
            }

            return audittemplates;
        }

        /// <summary>
        /// AppendAuditTemplateStepsAsync; Append Steps to AuditTemplate object (on each TaskTemplate if available).
        /// NOTE! if no TaskTemplates are available (e.g. the object that is the source for the audittemplate parameter doesn't have them). No steps will be available on output.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audittemplate">Audit template.</param>
        /// <returns>AuditTemplate, appended with Steps.</returns>
        private async Task<AuditTemplate> AppendAuditTemplateStepsAsync(int companyId, AuditTemplate audittemplate)
        {
            if (audittemplate.TaskTemplates == null || audittemplate.TaskTemplates.Count == 0)
                return audittemplate;

            List<int> taskTemplateIds = audittemplate.TaskTemplates.Select(t => t.Id).ToList();
            var steps = await _taskManager.GetTaskTemplateStepsAsync(companyId: companyId, taskTemplateIds: taskTemplateIds);
            if (steps != null && steps.Count > 0)
            {
                if (audittemplate.TaskTemplates != null && audittemplate.TaskTemplates.Count > 0)
                {
                    foreach (var tasktemplate in audittemplate.TaskTemplates)
                    {
                        tasktemplate.Steps = steps.Where(x => x.TaskTemplateId == tasktemplate.Id).ToList();
                    }
                }
            }

            return audittemplate;
        }

        /// <summary>
        /// AppendAreaPathsToAuditTemplatesAsync; Add the AreaPath to the AuditTemplate. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audittemplates">List of audittemplates.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>AuditTemplates including area full path. </returns>
        private async Task<List<AuditTemplate>> AppendAreaPathsToAuditTemplatesAsync(int companyId, List<AuditTemplate> audittemplates, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var auditTemplate in audittemplates)
                {
                    var area = areas?.Where(x => x.Id == auditTemplate.AreaId)?.FirstOrDefault();
                    if(area!=null)
                    {
                        if(addAreaPath) auditTemplate.AreaPath = area?.FullDisplayName;
                        if(addAreaPathIds) auditTemplate.AreaPathIds = area?.FullDisplayIds;
                    }

                }
            }
            return audittemplates;
        }

        /// <summary>
        /// Iterates through checklist templates to replace the name and descriptions with the corresponding tranlslated ones.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="audittemplates"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private async Task<List<AuditTemplate>> AppendTranslationsToAuditTemplatesAsync(int companyId, List<AuditTemplate> auditTemplates, string language)
        {
            if (string.IsNullOrEmpty(language) || auditTemplates == null || !auditTemplates.Any())
                return auditTemplates;

            foreach (var audittemplate in auditTemplates)
            {
                var translation = await _translationManager.GetTranslationAsync(
                    audittemplate.Id,
                    companyId,
                    language,
                    "public.get_audittemplate_translations",
                    audittemplate);

            }

            return auditTemplates;
        }

        /// <summary>
        /// CreateOrFillAuditTemplateFromReader; creates and fills a AuditTemplate object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audittemplate">AuditTemplate object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled AuditTemplate object.</returns>
        private AuditTemplate CreateOrFillAuditTemplateFromReader(NpgsqlDataReader dr, AuditTemplate audittemplate = null)
        {
            if (audittemplate == null) audittemplate = new AuditTemplate();

            audittemplate.AreaId = Convert.ToInt32(dr["area_id"]);
            audittemplate.Id = Convert.ToInt32(dr["id"]);
            audittemplate.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                audittemplate.Description = dr["description"].ToString();
            }
            audittemplate.IsDoubleSignatureRequired = Convert.ToBoolean(dr["double_signature_required"]);
            audittemplate.IsSignatureRequired = Convert.ToBoolean(dr["signature_required"]);
            audittemplate.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                audittemplate.Picture = dr["picture"].ToString();
            }
            if (dr["score_type"] != DBNull.Value)
            {
                audittemplate.ScoreType = dr["score_type"].ToString();

            }

            if (dr.HasColumn("score"))
            {
                if (dr["score"] != DBNull.Value)
                {
                    audittemplate.Score = Convert.ToInt32(dr["score"]);
                }
            }

            if (dr.HasColumn("last_signed_at"))
            {
                if (dr["last_signed_at"] != DBNull.Value)
                {
                    audittemplate.LastSignedAt = Convert.ToDateTime(dr["last_signed_at"]);
                }
            }

            if (dr.HasColumn("has_incomplete_audits"))
            {
                if (dr["has_incomplete_audits"] != DBNull.Value)
                {
                    audittemplate.HasIncompleteAudits = Convert.ToBoolean(dr["has_incomplete_audits"]);
                }
            }

            if (dr["role"] != DBNull.Value && !string.IsNullOrEmpty(dr["role"].ToString()))
            {
                audittemplate.Role = dr["role"].ToString();
            }

            if(dr["min_task_score"] != DBNull.Value)
            {
                audittemplate.MinScore = Convert.ToInt32(dr["min_task_score"]);
            }

            if (dr["max_task_score"] != DBNull.Value)
            {
                audittemplate.MaxScore = Convert.ToInt32(dr["max_task_score"]);
            }

            if(dr.HasColumn ("has_derived_items"))
            {
                if (dr["has_derived_items"] != DBNull.Value)
                {
                    audittemplate.HasDerivedItems = Convert.ToBoolean(dr["has_derived_items"]);
                }
            }

            if (dr.HasColumn("modified_at") && dr["modified_at"] != DBNull.Value)
            {
                audittemplate.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            if (dr.HasColumn("version"))
            {
                if (dr["version"] != DBNull.Value)
                {
                    audittemplate.Version = Convert.ToString(dr["version"]);
                }
            }

            return audittemplate;
        }

        /// <summary>
        /// GetNpgsqlParametersFromAuditTemplate; Creates a list of NpgsqlParameters, and fills it based on the supplied AuditTemplate object.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="auditTemplate">The supplied AuditTemplate object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplateId">AuditTemplateId (DB: audits_audittemplate.id)</param>
        /// <returns></returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromAuditTemplate(AuditTemplate auditTemplate, int companyId, int auditTemplateId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (auditTemplateId > 0) parameters.Add(new NpgsqlParameter("@_id", auditTemplateId));
            parameters.Add(new NpgsqlParameter("@_name", auditTemplate.Name));
            parameters.Add(new NpgsqlParameter("@_doublesignaturerequired", auditTemplate.IsDoubleSignatureRequired));
            parameters.Add(new NpgsqlParameter("@_scoretype", auditTemplate.ScoreType.ToLower()));
            parameters.Add(new NpgsqlParameter("@_areaid", auditTemplate.AreaId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_signaturerequired", auditTemplate.IsSignatureRequired));

            if(!string.IsNullOrEmpty(auditTemplate.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", auditTemplate.Description));
            }
            if(!string.IsNullOrEmpty(auditTemplate.Picture))
            {
                parameters.Add(new NpgsqlParameter("@_picture", auditTemplate.Picture));
            }
            if(auditTemplate.MinScore.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_mintaskscore", auditTemplate.MinScore.Value));
            }
            if (auditTemplate.MaxScore.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_maxtaskscore", auditTemplate.MaxScore.Value));
            }
            if (!string.IsNullOrEmpty(auditTemplate.Role))
            {
                parameters.Add(new NpgsqlParameter("@_role", auditTemplate.Role));
            }

            return parameters;
        }

        /// <summary>
        /// ChangeAuditTemplateAddOrCreateTaskTemplates; Changes a audittemplate's taskitems.
        /// Note, based on the supplied templates if updating a existing template tasktemplates that are not supplied withing the collection are set to inactive.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId of the user that's making the changes.</param>
        /// <param name="auditTemplateId">Id of the template that is being updated.</param>
        /// <param name="taskTemplates">Collection of tasktemplate items for the specific audit.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> ChangeAuditTemplateAddOrChangeTaskTemplates(int companyId, int userId, int auditTemplateId, List<TaskTemplate> taskTemplates)
        {
            if (auditTemplateId > 0)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate_tasks.ToString(), Models.Enumerations.TableFields.audittemplate_id.ToString(), auditTemplateId);

                if (auditTemplateId > 0 && taskTemplates != null)
                {
                    var currentTaskTemplates = await GetTaskTemplatesWithAuditTemplateAsync(companyId: companyId, auditTemplateId: auditTemplateId); //get current steps in db;
                    var templateids = taskTemplates.Select(x => x.Id).ToList();
                    ////clear all tasktemplates that aren't in the current collection anymore.
                    if (currentTaskTemplates != null && currentTaskTemplates.Count > 0)
                    {
                        foreach (TaskTemplate tasktemplate in currentTaskTemplates.Where(x => x.Id > 0 && !templateids.Contains(x.Id)))
                        {
                            await _taskManager.SetTaskTemplateActiveAsync(companyId: companyId, userId: userId, taskTemplateId: tasktemplate.Id, isActive: false);
                        }
                    }


                    foreach (var taskTemplate in taskTemplates)
                    {
                        await AddChangeTaskTemplate(companyId: companyId, userId: userId, auditTemplateId: auditTemplateId, taskTemplate: taskTemplate);
                    }
                }

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audittemplate_tasks.ToString(), Models.Enumerations.TableFields.audittemplate_id.ToString(), auditTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audittemplate_tasks.ToString(), objectId: auditTemplateId, userId: userId, companyId: companyId, description: "Changed audittemplate tasktemplate relation collection.");

            }

            return true;
        }

        /// <summary>
        /// AddChangeTaskTemplate; Add or change a single TaskTemplate. Based on supplied template a Add or Change functionality of the taskmanager will be called.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId of the user that's making the changes.</param>
        /// <param name="auditTemplateId">Id of the template that is being updated.</param>
        /// <param name="taskTemplate">The template to be changed or added;</param>
        /// <returns>true/false depending on outcome;</returns>
        private async Task<bool> AddChangeTaskTemplate(int companyId, int userId, int auditTemplateId, TaskTemplate taskTemplate)
        {
            if (auditTemplateId > 0 && taskTemplate != null)
            {
                if (taskTemplate.Id > 0)
                {
                    taskTemplate.AuditTemplateId = auditTemplateId;
                    var result = await _taskManager.ChangeTaskTemplateAsync(companyId: companyId, userId: userId, taskTemplateId: taskTemplate.Id, taskTemplate: taskTemplate);
                    return result;
                }
                else
                {

                    taskTemplate.AuditTemplateId = auditTemplateId;
                    var resultid = await _taskManager.AddTaskTemplateAsync(companyId: companyId, userId: userId, taskTemplate: taskTemplate);
                    return resultid > 0;
                }

            }
            return true;
        }
        #endregion

        #region - property methods -

        //TODO check if methods need to be moved to own structure.

        /// <summary>
        /// AppendPropertiesToAudits; Append properties to a list of audits. (e.g. open fields)  
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audits">List of audits where properties need to be appended</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>A list of audits with appended properties.</returns>
        private async Task<List<Audit>> AppendPropertiesToAudits(int companyId, List<Audit> audits, string include = "")
        {
            List<int> auditIds = null;
            List<int> auditTemplateIds = null;
            if (audits != null)
            {
                auditIds = audits.Select(audit => audit.Id).ToList();
                auditTemplateIds = audits.Select(audit => audit.TemplateId).Distinct().ToList();
            }

            var propertyUserValues = await _propertyValueManager.GetPropertyUserValuesWithAudits(companyId: companyId, auditIds: auditIds);
            var properties = await _propertyValueManager.GetPropertiesAuditTemplatesAsync(companyId: companyId, auditTemplateIds: auditTemplateIds);

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) {
                foreach (var audit in audits)
                {
                    audit.OpenFieldsProperties = properties.Where(x => x.AuditTemplateId == audit.TemplateId && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                    audit.OpenFieldsPropertyUserValues = propertyUserValues.Where(x => x.AuditId == audit.Id).ToList();
                }
            }

            return audits;
        }

        /// <summary>
        /// AppendPropertiesToAudit; Append properties to a specific audit. (e.g. open fields)  
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audit">Audit where properties need to be appended</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>Return audit object with included properties</returns>
        private async Task<Audit> AppendPropertiesToAudit(int companyId, Audit audit, string include = "", ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<int> auditIds = new() { audit.Id };
            var propertyUserValues = await _propertyValueManager.GetPropertyUserValuesWithAudits(companyId: companyId, auditIds: auditIds, connectionKind: connectionKind);
            var properties = await _propertyValueManager.GetPropertiesAuditTemplatesAsync(companyId: companyId);

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
            {
                audit.OpenFieldsProperties = properties.Where(x => x.AuditTemplateId == audit.TemplateId && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                audit.OpenFieldsPropertyUserValues = propertyUserValues.Where(x => x.AuditId == audit.Id).ToList();
            }

            return audit;
        }

        /// <summary>
        /// AppendTemplatePropertiesToTaskTemplates; Append properties to task templates of a audit template; 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audittemplates">List if audit templates</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>A list of audit templates where the task templates are appended with properties.</returns>
        private async Task<List<AuditTemplate>> AppendTemplatePropertiesToTaskTemplates(int companyId, List<AuditTemplate> audittemplates, string include = null)
        {
            List<int> taskTemplateIds = audittemplates.Where(auditTemplate => auditTemplate.TaskTemplates != null).SelectMany(auditTemplate => auditTemplate.TaskTemplates.Select(taskTemplate => taskTemplate.Id)).ToList();
            var properties = await _propertyValueManager.GetPropertiesTaskTemplatesAsync(companyId: companyId, taskTemplateIds: taskTemplateIds, include: include);
            foreach (var audit in audittemplates)
            {
                if(audit.TaskTemplates != null)
                {
                    foreach (var audititem in audit.TaskTemplates)
                    {
                        if (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))
                        {
                            audititem.Properties = properties.Where(x => x.TaskTemplateId == audititem.Id && x.IsActive && PropertySettings.BasicAndSpecificProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                        }
                        foreach (var item in audititem.Properties)
                        {
                            item.TaskTemplateId = audititem.Id;
                        }
                    }
                }
            }
            return audittemplates;
        }

        /// <summary>
        /// AppendTemplatePropertiesToTaskTemplates; Append property to audit template.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audittemplate">AuditTemplate where properties need to be appended to the task templates.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>Return audit template object with included properties</returns>
        private async Task<AuditTemplate> AppendTemplatePropertiesToTaskTemplates(int companyId, AuditTemplate audittemplate, string include = null)
        {

            if(audittemplate.TaskTemplates != null && audittemplate.TaskTemplates.Count > 0)
            {
                List<int> taskTemplateIds = audittemplate.TaskTemplates.Select(taskTemplate => taskTemplate.Id).ToList();
                //TODO feature
                var properties = await _propertyValueManager.GetPropertiesTaskTemplatesAsync(companyId: companyId, taskTemplateIds: taskTemplateIds, include: include);

                foreach (var audititem in audittemplate.TaskTemplates)
                {
                    if (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))
                    {
                        audititem.Properties = properties.Where(x => x.TaskTemplateId == audititem.Id && x.IsActive && PropertySettings.BasicAndSpecificProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                    }

                    audititem.Properties.ForEach(x => x.TaskTemplateId = audititem.Id);
                    
                    if (include.Split(",").Contains(IncludesEnum.PropertiesGen4.ToString().ToLower()))
                    {
                        audititem.PropertiesGen4 = audititem.Properties.ToPropertyDTOList();
                        audititem.Properties = null;
                    }
                }
            }

            return audittemplate;
        }


        /// <summary>
        /// AppendTemplatePropertiesToTemplates; Append properties to audit templates.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audittemplates">List of audit templates.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>List of updated audit templates.</returns>
        private async Task<List<AuditTemplate>> AppendTemplatePropertiesToTemplates(int companyId, List<AuditTemplate> audittemplates, string include = null)
        {
            List<int> auditTemplateIds = audittemplates.Select(auditTemplate => auditTemplate.Id).ToList();
            var properties = await _propertyValueManager.GetPropertiesAuditTemplatesAsync(companyId: companyId, auditTemplateIds: auditTemplateIds, include: include);
            if (audittemplates != null && properties != null && properties.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
                {
                    foreach (var audittemplate in audittemplates)
                    {
                        audittemplate.OpenFieldsProperties = properties.Where(x => x.AuditTemplateId == audittemplate.Id && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                        foreach (var item in audittemplate.OpenFieldsProperties)
                        {
                            item.AuditTemplateId = audittemplate.Id;
                        }

                    }
                }
            }

            return audittemplates;
        }

        /// <summary>
        /// AppendTemplatePropertiesToTemplate; Append properties to specific template;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="audittemplate">Audit template object where properties need to be added.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>Updated audit template object including properties (if available)</returns>
        private async Task<AuditTemplate> AppendTemplatePropertiesToTemplate(int companyId, AuditTemplate audittemplate, string include = null)
        {
            //TODO make more efficient
            var properties = await _propertyValueManager.GetPropertiesAuditTemplateAsync(companyId: companyId, auditTemplateId: audittemplate.Id, include: include);

            if (properties != null && properties.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
                {
                    audittemplate.OpenFieldsProperties = properties.Where(x => x.AuditTemplateId == audittemplate.Id && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();

                    audittemplate.OpenFieldsProperties.ForEach(x => x.AuditTemplateId = audittemplate.Id);

                    if (include.Split(",").Contains(IncludesEnum.PropertiesGen4.ToString().ToLower()))
                    {
                        audittemplate.OpenFieldsPropertiesGen4 = audittemplate.OpenFieldsProperties.ToPropertyDTOList();
                        audittemplate.OpenFieldsProperties = null;
                    }

                }
            }

            return audittemplate;
        }

        /// <summary>
        /// Converts the Properties and PropertyUserValues fields to the new PropertiesGen4 field
        /// The Properties and PropertyUserValues fields will be removed at the end of the conversion
        /// </summary>
        /// <param name="checklist">The checklist to convert the properties for</param>
        /// <returns>Updated Checklist</returns>
        private async Task<Audit> ReplacePropertiesWithPropertiesGen4(Audit audit, int companyId)
        {
            //List of users is only needed when there are property user values
            List<UserBasic> users = null;
            if(audit.PropertyUserValues != null || audit.OpenFieldsPropertyUserValues != null)
            {
                users = await _userManager.GetUsersBasicAsync(companyId: companyId);
            }

            if (audit.Properties != null && audit.Properties.Any())
            {
                audit.PropertiesGen4 = audit.Properties.ToPropertyDTOList(propertyUserValues: audit.PropertyUserValues, userList: users);
                audit.Properties = null;
                audit.PropertyUserValues = null;
            }

            if (audit.OpenFieldsProperties != null && audit.OpenFieldsProperties.Any())
            {
                audit.OpenFieldsPropertiesGen4 = audit.OpenFieldsProperties.ToPropertyDTOList(propertyUserValues: audit.OpenFieldsPropertyUserValues, userList: users);
                audit.OpenFieldsProperties = null;
                audit.OpenFieldsPropertyUserValues = null;
            }
            return audit;
        }

        #endregion

        #region - change/add tasks with audits -
        /// <summary>
        /// ChangeAuditAddOrChangeTask; Change audit and add a new task or update an existing task. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <param name="tasks">Collection of tasks that need to be processed.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> ChangeAuditAddOrChangeTask(int companyId, int userId, int possibleOwnerId, int auditId, List<TasksTask> tasks)
        {
            if(auditId > 0)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit_tasks.ToString(), Models.Enumerations.TableFields.audit_id.ToString(), auditId);

                if (auditId > 0 && tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        await AddChangeTask(companyId: companyId, userId: userId, possibleOwnerId: possibleOwnerId, auditId: auditId, task: task);
                    }
                }

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.audits_audit_tasks.ToString(), Models.Enumerations.TableFields.audit_id.ToString(), auditId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.audits_audit_tasks.ToString(), objectId: auditId, userId: userId, companyId: companyId, description: "Changed audit task relation collection.");

            }

            return true;
        }

        /// <summary>
        /// AddChangeTask; Add or change a task
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <param name="task">Task to be updated.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> AddChangeTask(int companyId, int userId, int auditId, int possibleOwnerId, TasksTask task)
        {
            if (auditId > 0 && task != null)
            {
                if (task.Id > 0)
                {
                    task.AuditId = auditId;
                    var result = await _taskManager.ChangeTaskAsync(companyId: companyId, userId: userId, possibleOwnerId: possibleOwnerId, taskId: Convert.ToInt32(task.Id), task: task);
                    return result;
                }
                else
                {

                    task.AuditId = auditId;
                    var resultId = await _taskManager.AddTaskAsync(companyId: companyId, userId: userId, possibleOwnerId: possibleOwnerId, task: task);

                    if(resultId > 0)
                    {
                        var result = await AddTaskAuditRelation(companyId: companyId, auditId: task.AuditId.Value, taskId: resultId);
                        var resultscore = await UpdateAuditSpecificFields(companyId: companyId, taskId: resultId, userId: userId, status: task.Status, score: task.Score.Value);
                    }

                    return resultId > 0;
                }
            }

            return true;
        }

        /// <summary>
        /// UpdateAuditSpecificFields; Update specific audit fields that are needed for data constancy.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskId">TaskId (DB: tasks_task.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="status">Status to be posted (todo, done etc).</param>
        /// <param name="score">Possible score to be posted.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> UpdateAuditSpecificFields(int companyId, int taskId, int userId, string status, int score)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_task.ToString(), taskId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", taskId));
            parameters.Add(new NpgsqlParameter("@_status", status));
            parameters.Add(new NpgsqlParameter("@_score", score));

            var rowseffected = Convert.ToInt32 (await _manager.ExecuteScalarAsync("set_task_audit_status", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_task.ToString(), taskId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.tasks_task.ToString(), objectId: taskId, userId: userId, companyId: companyId, description: "Changed audit task status and or score.");

            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// AddTaskAuditRelation; Add a relation record between a audit and task. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditId">AuditId (DB: audits_audit.id)</param>
        /// <param name="taskId">TaskId (DB: tasks_task.id)</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> AddTaskAuditRelation(int companyId, int auditId, int taskId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_auditid", auditId));
            parameters.Add(new NpgsqlParameter("@_taskid", taskId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            var rowseffected = await _manager.ExecuteScalarAsync("add_audit_task_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            return (Convert.ToInt32(rowseffected) > 0);
        }
        #endregion

        #region - template properties -
        /// <summary>
        /// AddChangeTemplatePropertiesAsync; Add or change properties with a tasktemplate
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateId">TemplateId (audit template id) of object.</param>
        /// <param name="templateProperties">Properties to be added.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<int> AddChangeTemplatePropertiesAsync(int companyId, int userId, int templateId, List<PropertyAuditTemplate> templateProperties)
        {
            //TODO add output nr (total of mutations)
            if (templateProperties != null)
            {
                //Get all current properties
                var currentProperties = await _propertyValueManager.GetPropertiesAuditTemplateAsync(companyId: companyId, auditTemplateId: templateId);
                var propIds = templateProperties.Select(x => x.Id).ToList(); //Get all new ids that are coming from templateProperties collection.

                if (currentProperties != null && currentProperties.Count > 0)
                {
                    foreach (var prop in currentProperties.Where(x => x.Id > 0 && !propIds.Contains(x.Id)))
                    {
                        //check all properties against the supplied ids, if not in the supplied ids, start removing them. (set to inactive)
                        await _propertyValueManager.RemoveAuditTemplatePropertyAsync(companyId: companyId, userId: userId, auditTemplatePropertyId: prop.Id);
                    }
                }

                //Add or Change all properties that are supplied.
                foreach (PropertyAuditTemplate auditTemplateProperty in templateProperties)
                {
                    if (auditTemplateProperty.Id > 0)
                    {
                        await ChangeAuditTemplatePropertyAsync(companyId: companyId, userId: userId, auditTemplatePropertyId: auditTemplateProperty.Id, templateproperty: auditTemplateProperty);
                    }
                    else
                    {
                        if (auditTemplateProperty.AuditTemplateId <= 0) auditTemplateProperty.AuditTemplateId = templateId; //make sure to add the templateid if not supplied with the property
                        await AddAuditTemplatePropertyAsync(companyId: companyId, userId: userId, templateproperty: auditTemplateProperty);
                    }
                }

            }

            return 0;
        }

        /// <summary>
        /// AddTaskTemplatePropertyAsync; Add audittemplate property. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty">Audit template property to be added to the template.</param>
        /// <returns>true/false depending on outcome</returns>
        private async Task<int> AddAuditTemplatePropertyAsync(int companyId, int userId, PropertyAuditTemplate templateproperty)
        {
            return await _propertyValueManager.AddAuditTemplatePropertyAsync(companyId: companyId, userId: userId, templateProperty: templateproperty);
        }

        /// <summary>
        /// ChangeTaskTemplatePropertyAsync; Change audittemplate property
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="taskTemplatePropertyId">Specific template property to be updated.</param>
        /// <param name="templateproperty">Audit template property data to be added to the template</param>
        /// <returns>true/false depending on outcome</returns>
        private async Task<int> ChangeAuditTemplatePropertyAsync(int companyId, int userId, int auditTemplatePropertyId, PropertyAuditTemplate templateproperty)
        {
            return await _propertyValueManager.ChangeAuditTemplatePropertyAsync(companyId: companyId, userId: userId, auditTemplatePropertyId: auditTemplatePropertyId, templateProperty: templateproperty); ;
        }
        #endregion

        #region - audit properties -
        /// <summary>
        /// AddChangeAuditPropertyUserValue; Add/Change user inputed value with property with a certain audit.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskId">TaskId (DB: tasks_task.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="propertyUserValues">property user values to be added/changed.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> AddChangeAuditPropertyUserValue(int companyId, int auditId, int userId, List<PropertyUserValue> propertyUserValues)
        {
            if (propertyUserValues != null && propertyUserValues.Count > 0)
            {
                foreach (PropertyUserValue propertyUserValue in propertyUserValues)
                {
                    if (propertyUserValue.Id > 0)
                    {
                        propertyUserValue.AuditId = auditId;
                        var resultChange = await _propertyValueManager.ChangeAuditPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, propertyUserValueId: propertyUserValue.Id, userId: userId);
                    }
                    else
                    {
                        propertyUserValue.AuditId = auditId;
                        var resultAdd = await _propertyValueManager.AddAuditPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, userId: userId);
                    }
                }
            }
            return true;
        }
        #endregion

        #region - object enhancements -
        /// <summary>
        /// GetDynamicCountersForAudits; Gets a list of dynamic counter (nr of actions, comments etc) for specific tasks of a specific audit and maps these values to the supplied collection of audits.
        /// </summary>
        /// <param name="audits">List of audits</param>
        /// <param name="parameters">Parameters that were used for the list of audits.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>Updated list of audits.</returns>
        private async Task<List<Audit>> GetDynamicCountersForAudits(List<Audit> audits, List<NpgsqlParameter> parameters, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;

            if(audits != null && audits.Count > 0)
            {
                List<ObjectTasksCounters> counters = new List<ObjectTasksCounters>();
                using (dr = await _manager.GetDataReader("get_counts_for_tasks_with_audits", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var counter = CreateOrFillObjectTasksCounterFromReader(dr);
                        counters.Add(counter);
                    }
                }

                if (counters != null && counters.Count > 0)
                {
                    foreach (Audit audit in audits)
                    {
                        if(audit.Tasks != null && audit.Tasks.Count > 0)
                        {
                            foreach (TasksTask task in audit.Tasks)
                            {
                                var counter = counters.Where(x => x.TaskId == task.Id && x.CompanyId == task.CompanyId).FirstOrDefault();
                                if (counter != null)
                                {
                                    task.CommentCount = counter.CommentNr;
                                    task.ActionsCount = counter.ActionNr;
                                }
                            }
                        }
                    }
                }
            }

            return audits;
        }

        /// <summary>
        /// GetDynamicCountersForAudit; Gets a list of dynamic counter (nr of actions, comments etc) for specific tasks of a specific audit and maps these values to the supplied audit.
        /// </summary>
        /// <param name="audit">Audit</param>
        /// <param name="parameters">Parameters that were used for the list of audits.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>Updated list of audits.</returns>
        private async Task<Audit> GetDynamicCountersForAudit(Audit audit, List<NpgsqlParameter> parameters, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;

            if (audit != null)
            {
                List<ObjectTasksCounters> counters = new List<ObjectTasksCounters>();
                using (dr = await _manager.GetDataReader("get_counts_for_tasks_with_audit", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var counter = CreateOrFillObjectTasksCounterFromReader(dr);
                        counters.Add(counter);
                    }
                }

                if (counters != null && counters.Count > 0)
                {
                    if (audit.Tasks != null && audit.Tasks.Count > 0)
                    {
                        foreach (TasksTask task in audit.Tasks)
                        {
                            var counter = counters.Where(x => x.TaskId == task.Id && x.CompanyId == task.CompanyId).FirstOrDefault();
                            if (counter != null)
                            {
                                task.CommentCount = counter.CommentNr;
                                task.ActionsCount = counter.ActionNr;
                            }
                        }
                    }
                }
            }

            return audit;
        }

        /// <summary>
        /// CreateOrFillObjectTasksCounterFromReader; creates and fills a ObjectTasksCounters object from a DataReader.
        /// NOTE! intended for use with the get_counts_for_tasks_with_audit(s) methods
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="objectTasksCounters">ObjectTasksCounters object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled ObjectTasksCounters object.</returns>
        private ObjectTasksCounters CreateOrFillObjectTasksCounterFromReader(NpgsqlDataReader dr, ObjectTasksCounters objectTasksCounters = null)
        {
            if (objectTasksCounters == null) objectTasksCounters = new ObjectTasksCounters();
            //"audit_id" int4, "company_id" int4, "id" int4, "actionnr" int4, "commentnr" int4

            objectTasksCounters.ActionNr = Convert.ToInt32(dr["actionnr"]);
            objectTasksCounters.CommentNr = Convert.ToInt32(dr["commentnr"]);
            objectTasksCounters.ParentObjectId = Convert.ToInt32(dr["audit_id"]);
            objectTasksCounters.TaskId = Convert.ToInt32(dr["id"]);
            objectTasksCounters.CompanyId = Convert.ToInt32(dr["company_id"]);

            return objectTasksCounters;
        }
        #endregion

        #region - Work Instructions -
        private async Task<List<TaskTemplate>> AppendWorkInstructionsAsync(int companyId, int auditTemplateId, List<TaskTemplate> templateItems)
        {
            //TODO fill
            if (companyId > 0 && auditTemplateId > 0)
            {

                var workInstructionRelations = await GetWorkInstructionRelationsAsync(companyId: companyId, auditTemplateId: auditTemplateId);

                if (workInstructionRelations != null && workInstructionRelations.Any())
                {

                    var workInstructions = await _workInstructionManager.GetWorkInstructionTemplatesAsync(companyId: companyId, include: "items");

                    if (workInstructions != null && workInstructions.Any())
                    {

                        foreach (var template in templateItems)
                        {
                            var possibleRelations = workInstructionRelations.Where(x => x.TaskTemplateId == template.Id);
                            if (possibleRelations.Any())
                            {
                                var possibleWorkInstructions = workInstructions.Where(x => possibleRelations.OrderBy(o => o.Index).Select(r => r.WorkInstructionTemplateId).ToArray().Contains(x.Id));
                                if (possibleWorkInstructions != null && possibleWorkInstructions.Any())
                                {
                                    template.WorkInstructions = possibleWorkInstructions.ToList();
                                }

                            }
                        }
                    }
                }
            }
            return templateItems;
        }

        private async Task<List<TaskTemplate>> AppendWorkInstructionRelationsAsync(int companyId, int auditTemplateId, List<TaskTemplate> templateItems)
        {
            if (companyId > 0 && auditTemplateId > 0)
            {
                var workInstructionRelations = await GetWorkInstructionRelationsAsync(companyId: companyId, auditTemplateId: auditTemplateId);
                if (workInstructionRelations != null && workInstructionRelations.Any())
                {
                    foreach (var taskTemplate in templateItems)
                    {
                        var possibleWorkInstructionRelation = workInstructionRelations.Where(x => x.TaskTemplateId == taskTemplate.Id && x.AuditTemplateId == auditTemplateId);
                        if (possibleWorkInstructionRelation != null && possibleWorkInstructionRelation.Any()) taskTemplate.WorkInstructionRelations = possibleWorkInstructionRelation.ToList();
                    }
                }
            }
            return templateItems;
        }

        private async Task<List<AuditTemplate>> AppendWorkInstructionRelationsAsync(int companyId, List<AuditTemplate> auditTemplates)
        {
            if (companyId > 0)
            {
                var workInstructionRelations = await GetWorkInstructionRelationsAsync(companyId: companyId);
                if (workInstructionRelations != null && workInstructionRelations.Any())
                {
                    foreach (var auditTemplate in auditTemplates)
                    {
                        if(auditTemplate.TaskTemplates != null)
                        {
                            foreach (var taskTemplate in auditTemplate.TaskTemplates)
                            {
                                var possibleWorkInstructionRelation = workInstructionRelations.Where(x => x.TaskTemplateId == taskTemplate.Id && x.AuditTemplateId == auditTemplate.Id);
                                if (possibleWorkInstructionRelation != null && possibleWorkInstructionRelation.Any()) taskTemplate.WorkInstructionRelations = possibleWorkInstructionRelation.ToList();
                            }
                        }
                    }
                }
            }
            return auditTemplates;
        }

        private async Task<List<TaskTemplateRelationWorkInstructionTemplate>> GetWorkInstructionRelationsAsync(int companyId, int? auditTemplateId = null)
        {
            if (companyId > 0)
            {
                //TODO replace
                var output = new List<TaskTemplateRelationWorkInstructionTemplate>();

                NpgsqlDataReader dr = null;

                try
                {
                    //"get_workinstruction_tasktemplate_relations"("_companyid" int4, "_tasktemplateid" int4=0, "_workinstructiontemplateid" int4=0)
                    //"id" int4, "company_id" int4, "workinstruction_template_id" int4, "tasktemplate_id" int4, "index" int4, "name" varchar, "media" text
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                    if (auditTemplateId.HasValue) parameters.Add(new NpgsqlParameter("@_audittemplateid", auditTemplateId));

                    string sp = "get_workinstruction_audit_item_relations";

                    using (dr = await _manager.GetDataReader(sp, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                    {
                        while (await dr.ReadAsync())
                        {
                            var item = new TaskTemplateRelationWorkInstructionTemplate();

                            item.Id = Convert.ToInt32(dr["id"]);
                            item.Name = dr["name"].ToString();
                            item.Index = Convert.ToInt32(dr["index"]);
                            item.AuditTemplateId = Convert.ToInt32(dr["audittemplate_id"]);
                            item.TaskTemplateId = Convert.ToInt32(dr["tasktemplate_id"]);
                            item.WorkInstructionTemplateId = Convert.ToInt32(dr["workinstruction_template_id"]);

                            if (dr.HasColumn("media") && dr["media"] != DBNull.Value && !string.IsNullOrEmpty(dr["media"].ToString()))
                            {
                                if (dr["media"].ToString().Contains("[") || dr["media"].ToString().Contains("{")) //make sure it contains json
                                {
                                    var list = dr["media"].ToString().ToObjectFromJson<List<string>>();
                                    if (list != null)
                                    {
                                        item.Media = list;
                                    }
                                }
                                else
                                { //if not; handle as single string and add to collection
                                    item.Media = new List<string>();
                                    item.Media.Add(dr["media"].ToString());
                                }
                            }

                            output.Add(item);

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("TaskManager.GetWorkInstructionRelationsAsync(): ", ex.Message));

                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                }
                finally
                {
                    if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
                }

                return output;
            }
            return null;
        }
        #endregion

        #region - Tags -
        /// <summary>
        /// AppendTagsToAuditTemplatesAsync; append tags to AuditTemplate collection.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="auditTemplates">Collection of AuditTemplate</param>
        /// <returns>Collection of AuditTemplate</returns>
        private async Task<List<AuditTemplate>> AppendTagsToAuditTemplatesAsync(int companyId, List<AuditTemplate> auditTemplates)
        {
            var allTagsOnAuditTemplates = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.AuditTemplate);
            if (allTagsOnAuditTemplates != null)
            {
                foreach (var auditTemplate in auditTemplates)
                {
                    var tagsOnThisAuditTemplate = allTagsOnAuditTemplates.Where(t => t.ObjectId == auditTemplate.Id).ToList();
                    if (tagsOnThisAuditTemplate != null && tagsOnThisAuditTemplate.Count > 0)
                    {
                        auditTemplate.Tags ??= new List<Models.Tags.Tag>();
                        auditTemplate.Tags.AddRange(tagsOnThisAuditTemplate);
                    }

                }
            }

            return auditTemplates;
        }

        private async Task<List<Audit>> AppendTagsToAuditsAsync(int companyId, List<Audit> audits)
        {
            var allTagsOnAdits = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.Audit);
            if (allTagsOnAdits != null)
            {
                foreach (var audit in audits)
                {
                    var tagsOnThisAudit = allTagsOnAdits.Where(t => t.ObjectId == audit.Id).ToList();
                    if (tagsOnThisAudit != null && tagsOnThisAudit.Count > 0)
                    {
                        audit.Tags ??= new List<Models.Tags.Tag>();
                        audit.Tags.AddRange(tagsOnThisAudit);
                    }

                }
            }

            return audits;
        }

        /// <summary>
        /// AppendUserInformationToAudits; Append firstname, lastname combinations to objects with audits which are separately stored. (e.g. modified_by_id etc).
        /// </summary>
        /// <param name="companyId">CompanyId of all users that need to be retrieved.</param>
        /// <param name="audit">Audits that need to be amended with data</param>
        /// <returns>return updated audits</returns>
        private async Task<List<Audit>> AppendUserInformationToAuditsAsync(int companyId, List<Audit> audits)
        {
            var possibleUsers = await _userManager.GetUserProfilesAsync(companyId: companyId);
            if (possibleUsers != null && audits != null)
            {
                foreach (var audit in audits)
                {
                    if (audit.PropertyUserValues != null)
                    {
                        foreach (var prop in audit.PropertyUserValues)
                        {
                            if (prop.UserId > 0)
                            {
                                var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == prop.UserId);
                                if (possibleUser != null)
                                {
                                    prop.ModifiedBy = string.Concat(possibleUser.FirstName," ", possibleUser.LastName);
                                }
                            }
                        }
                    }
                    if (audit.OpenFieldsPropertyUserValues != null)
                    {
                        foreach (var openprop in audit.OpenFieldsPropertyUserValues)
                        {
                            if (openprop.UserId > 0)
                            {
                                var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == openprop.UserId);
                                if (possibleUser != null)
                                {
                                    openprop.ModifiedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);
                                }
                            }
                        }
                    }
                    if (audit.Tasks != null)
                    {
                        foreach (var item in audit.Tasks)
                        {
                            if (item.PropertyUserValues != null)
                            {
                                foreach (var prop in item.PropertyUserValues)
                                {
                                    if (prop.UserId > 0)
                                    {
                                        var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == prop.UserId);
                                        if (possibleUser != null)
                                        {
                                            prop.ModifiedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return audits;
        }

        /// <summary>
        /// Calls the translation manager to fetch translations for all audits  based on their templateID (Names are the same and therefore we use templateID for fetching tranlsations)
        /// </summary>
        /// <param name="audits"></param>
        /// <param name="companyId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private async Task<List<Audit>> AppendTranslationsToAuditsAsync(List<Audit> audits, int companyId, string language)
        {
            if (string.IsNullOrEmpty(language) || audits == null || !audits.Any())
                return audits;

            foreach (var audit in audits)
            {
                var translation = await _translationManager.GetTranslationAsync(
                    audit.TemplateId,
                    companyId,
                    language,
                    "public.get_audittemplate_translations",
                    audit);

            }

            return audits;
        }

        /// <summary>
        /// AppendUserInformationToAuditAsync; Append firstname, lastname combinations to objects with audit which are separately stored. (e.g. modified_by_id etc).
        /// </summary>
        /// <param name="companyId">CompanyId of all users that need to be retrieved.</param>
        /// <param name="audit">Audit that need to be amended with data</param>
        /// <returns>return updated audit</returns>
        private async Task<Audit> AppendUserInformationToAuditAsync(int companyId, Audit audit)
        {
            var possibleUsers = await _userManager.GetUserProfilesAsync(companyId: companyId);
            if (possibleUsers != null && audit != null)
            {
                if (audit.PropertyUserValues != null)
                {
                    foreach (var prop in audit.PropertyUserValues)
                    {
                        if (prop.UserId > 0)
                        {
                            var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == prop.UserId);
                            if (possibleUser != null)
                            {
                                prop.ModifiedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);
                            }
                        }
                    }
                }
                if (audit.OpenFieldsPropertyUserValues != null)
                {
                    foreach (var openprop in audit.OpenFieldsPropertyUserValues)
                    {
                        if (openprop.UserId > 0)
                        {
                            var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == openprop.UserId);
                            if (possibleUser != null)
                            {
                                openprop.ModifiedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);
                            }
                        }
                    }
                }
                if (audit.Tasks != null)
                {
                    foreach (var item in audit.Tasks)
                    {
                        if (item.PropertyUserValues != null)
                        {
                            foreach (var prop in item.PropertyUserValues)
                            {
                                if (prop.UserId > 0)
                                {
                                    var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == prop.UserId);
                                    if (possibleUser != null)
                                    {
                                        prop.ModifiedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);
                                    }
                                }
                            }
                        }
                    }
                }

            }

            return audit;
        }

        private async Task<List<Audit>> AppendTranslationsToAuditlistsAsync(List<Audit> auditlists, int companyId, string language)
        {
            if (string.IsNullOrEmpty(language) || auditlists == null || !auditlists.Any())
                return auditlists;

            foreach (var auditlist in auditlists)
            {
                var translation = await _translationManager.GetTranslationAsync(
                    auditlist.TemplateId,
                    companyId,
                    language,
                    "public.get_audittemplate_translations",
                    auditlist);

            }

            return auditlists;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_taskManager.GetPossibleExceptions());
                listEx.AddRange(_areaManager.GetPossibleExceptions());
                listEx.AddRange(_actionManager.GetPossibleExceptions());
                listEx.AddRange(_userAccessManager.GetPossibleExceptions());
                listEx.AddRange(_propertyValueManager.GetPossibleExceptions());
                listEx.AddRange(_workInstructionManager.GetPossibleExceptions());
                listEx.AddRange(_tagManager.GetPossibleExceptions());
                listEx.AddRange(_userManager.GetPossibleExceptions());
                listEx.AddRange(_generalManager.GetPossibleExceptions());
                listEx.AddRange(_flattenedAuditManager.GetPossibleExceptions());
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
