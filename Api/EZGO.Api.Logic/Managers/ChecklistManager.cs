using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Presentation;
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
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Data;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Mappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

//TODO sort methods, rename all append methods in same structure.

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// ChecklistManager; The ChecklistManager contains all logic for retrieving and setting Checklists and ChecklistTemplates.
    /// Checklists are lists of items that can be checked off (thumbs up/down/skipped). 
    /// Checklists are for the most part structurally the same as Audits but lack a scoring mechanism and a other workflow (business workflow) when used. 
    /// The output data (exports) and display is also handled differently. In the future Checklists and Audits will differ more. 
    /// Technically a Checklist is a parent object that contains Tasks (which are the items).
    /// Checklists are filled in within the client apps, the outcome of a checklist is displayed within the client apps and the CMS.
    /// Checklists are managed in the CMS and based on the Template a Checklist is generated in the apps to be filled in. 
    /// </summary>
    public class ChecklistManager : BaseManager<ChecklistManager>, IChecklistManager
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
        private readonly IActionManager _actionManager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IPropertyValueManager _propertyValueManager;
        private readonly ITagManager _tagManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IWorkInstructionManager _workInstructionManager;
        private readonly IUserManager _userManager;
        private readonly IFlattenChecklistManager _flattenedChecklistManager;
        private readonly IGeneralManager _generalManager;
        #endregion

        #region - constructor(s) -
        public ChecklistManager(IGeneralManager generalManager, IFlattenChecklistManager flattenChecklistManager, IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, ITagManager tagManager, IUserManager userManager, IPropertyValueManager propertyValueManager, IWorkInstructionManager workInstructionManager, ITaskManager taskManager, IAreaManager areaManager, IActionManager actionManager, IDataAuditing dataAuditing, ILogger<ChecklistManager> logger) : base(logger)
        {
            _manager = manager;
            _taskManager = taskManager;
            _areaManager = areaManager;
            _actionManager = actionManager;
            _dataAuditing = dataAuditing;
            _propertyValueManager = propertyValueManager;
            _tagManager = tagManager;
            _configurationHelper = configurationHelper;
            _workInstructionManager = workInstructionManager;
            _userManager = userManager;
            _flattenedChecklistManager = flattenChecklistManager;
            _generalManager = generalManager;
        }
        #endregion

        #region - public methods Checklists -
        /// <summary>
        /// GetChecklistsAsync; Get checklists.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="include">Comma seperated string based on the IncludesTypeEnum</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <param name="useStatic">Use static data set or the live one.</param>
        /// <returns>A List of Checklists.</returns>
        public async Task<List<Checklist>> GetChecklistsAsync(int companyId, int? userId = null, ChecklistFilters? filters = null, string include = null, bool useStatic = false)
        {
            var output = new List<Checklist>();
            bool useStaticStorage = useStatic;

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
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

                    if (filters.Value.AllowedOnly.HasValue && filters.Value.AllowedOnly.Value && userId.HasValue && userId > 0)
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

                    if (filters.Value.TaskId != null && filters.Value.TaskId != 0)
                    {
                        parameters.Add(new NpgsqlParameter(@"_taskid", filters.Value.TaskId));
                    }

                    if(filters.Value.SortByModifiedAt != null && filters.Value.SortByModifiedAt.Value)
                    {
                        parameters.Add(new NpgsqlParameter(@"_sortbymodifiedat", filters.Value.SortByModifiedAt.Value));
                    }
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText.ToString()));
                    }
                }

                using (dr = await _manager.GetDataReader(useStaticStorage ? "get_checklists_static" : "get_checklists", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var checklist = useStaticStorage ? CreateOrFillStaticChecklistFromReader(dr) : CreateOrFillChecklistFromReader(dr);
                        if(checklist != null && checklist.Id > 0)
                        {
                            if (filters.Value.IsCompleted.HasValue && filters.Value.IsCompleted.Value == false)
                            {
                                if (checklist.CreatedById == null && checklist.CreatedAt < new DateTime(2023, 1, 1)) //filter old, incomplete checklists without CreatedById, check commit message for more info
                                    continue;
                            }
                            output.Add(checklist);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetChecklistsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && filters.Value.HasFilters())
            {
                //filter out technical filters; note! when used with offsets the number of returned items may not be the same
                output = (await FilterChecklists(companyId: companyId, userId: userId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            }

            if (output.Count > 0)
            {
                if(!useStaticStorage)
                {
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tasks.ToString().ToLower())) output = await AppendChecklistsTasksAndStagesAsync(companyId: companyId, checklists: output, filters: filters, userId: userId, include: include);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) output = await AppendPropertiesToChecklists(checklists: output, companyId: companyId, include: include);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToChecklistsAsync(checklists: output, companyId: companyId);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.UserInformation.ToString().ToLower()))) output = await AppendUserInformationToChecklistsAsync(checklists: output, companyId: companyId);

                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
                    {
                        output = await ApplyTemplateVersionToChecklists(output, companyId, include);
                    }
                }
                else
                {
                    output = await GetDynamicCountersForChecklists(checklists: output, parameters: Utils.Tools.Copier.DeepCopy(npgsqlParameters: parameters));
                }
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) output = await AppendAreaPathsToChecklistsAsync(companyId: companyId, checklists: output, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
            }

            return output;
        }

        /// <summary>
        /// GetChecklistAsync; Get a single checklist object based on the ChecklistId parameter. Based on the [checklists_checklist] table in the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <param name="include">Include, comma separated string based on IncludesEnum, used for including extra data.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <param name="useStatic">Use static data set or the live one.</param>
        /// <returns>Checklist object.</returns>
        public async Task<Checklist> GetChecklistAsync(int companyId, int checklistId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false)
        {
            var checklist = new Checklist();
            bool useStaticStorage = useStatic;

            NpgsqlDataReader dr = null;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            try
            {
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", checklistId));

                using (dr = await _manager.GetDataReader(useStaticStorage ? "get_checklist_static" : "get_checklist", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        if (useStaticStorage)
                        {
                            checklist = CreateOrFillStaticChecklistFromReader(dr, checklist: checklist);
                        } else
                        {
                            checklist = CreateOrFillChecklistFromReader(dr, checklist: checklist);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetChecklistAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (checklist.Id > 0)
            {
                if(!useStaticStorage)
                {
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) checklist.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, id: checklist.Id, objectType: ObjectTypeEnum.Checklist, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tasks.ToString().ToLower())) checklist = await AppendChecklistTasksAndStagesAsync(companyId: companyId, checklist: checklist, include: include, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) checklist = await AppendPropertiesToChecklist(checklist: checklist, companyId: companyId, include: include, connectionKind: connectionKind);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.UserInformation.ToString().ToLower()))) checklist = await AppendUserInformationToChecklistAsync(checklist: checklist, companyId: companyId);
                    if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertiesGen4.ToString().ToLower()))) checklist = await ReplacePropertiesWithPropertiesGen4(checklist: checklist, companyId: companyId);

                    if (!string.IsNullOrEmpty(checklist.Version) && checklist.Version != await _flattenedChecklistManager.RetrieveLatestAvailableVersion(checklist.TemplateId, companyId) && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
                    {
                        checklist = await ApplyTemplateVersionToChecklist(checklist, companyId, include);
                    }
                }
                else
                {
                    checklist = await GetDynamicCountersForChecklist(checklist: checklist, parameters: Utils.Tools.Copier.DeepCopy(npgsqlParameters: parameters), connectionKind: connectionKind);
                }

                return checklist;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// AddChecklistAsync; Add a checklist to the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="checklist">Checklist object, containing all relevant checklist data to add.(DB: checklists_checklist)</param>
        /// <returns>The identity of the table (DB: checklists_checklist.id)</returns>
        public async Task<int> AddChecklistAsync(int companyId, int userId, Checklist checklist)
        {
            //if checklist lacks a version, and fallback is enabled, use latest version of template.
            if (checklist.TemplateId > 0 && string.IsNullOrEmpty(checklist.Version) && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA_FALLBACK"))
            {
                checklist.Version = await _flattenedChecklistManager.RetrieveLatestAvailableVersion(checklist.TemplateId, companyId);
            }

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromChecklist(checklist: checklist, companyId: checklist.CompanyId, userId: userId));

            if (!string.IsNullOrEmpty(checklist.Version))
            {
                parameters.Add(new NpgsqlParameter("@_version", checklist.Version));
            }

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklist", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                checklist.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, ObjectTypeEnum.ChecklistTemplate, id: checklist.TemplateId);
                await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.Checklist, id: possibleId, tags: checklist.Tags, companyId: companyId, userId: userId);
            }

            //add stages
            if (checklist.Stages != null && checklist.Stages.Count > 0)
            {
                checklist.Stages = await AddChecklistStages(companyId: companyId, checklistId: possibleId, userId: userId, stages: checklist.Stages);
            }

            if (possibleId > 0 && checklist.Tasks != null && checklist.Tasks.Count > 0)
            {
                var signedUserId = (checklist.Signatures != null && checklist.Signatures.Count > 0 && checklist.Signatures[0].SignedById.HasValue ? checklist.Signatures[0].SignedById.Value : 0); //retrieve possible owner id of item based on signature
                checklist.Tasks = await ChangeChecklistAddOrChangeTask(companyId: companyId, userId: userId, possibleOwnerId: signedUserId, checklistId: possibleId, checklist.Tasks);
            }

            if (checklist.PropertyUserValues != null && checklist.PropertyUserValues.Count > 0)
            {
                var result = await AddChangeChecklistPropertyUserValue(companyId: companyId, checklistId: possibleId, userId: userId, checklist.PropertyUserValues);
            }

            if (checklist.OpenFieldsPropertyUserValues != null && checklist.OpenFieldsPropertyUserValues.Count > 0)
            {
                var result = await AddChangeChecklistPropertyUserValue(companyId: companyId, checklistId: possibleId, userId: userId, checklist.OpenFieldsPropertyUserValues);
            }

            if (checklist.OpenFieldsPropertiesGen4 != null && checklist.OpenFieldsPropertiesGen4.Count > 0)
            {
                foreach (PropertyDTO property in checklist.OpenFieldsPropertiesGen4)
                {
                    property.UserValue.ChecklistId = possibleId;
                    var result = await _propertyValueManager.AddChecklistPropertyUserValueAsync(companyId: companyId, property: property, userId: userId);
                }
            }

            if (checklist.LinkedTaskId.HasValue && checklist.LinkedTaskId.Value > 0)
            {
                var result = await AddTaskChecklistLinkAsync(companyId: companyId, userId: userId, taskId: checklist.LinkedTaskId.Value, checklistId: possibleId, isRequired: checklist.IsRequiredForLinkedTask == true);
            }
            
            //add relations between added stages and tasks
            if (checklist.Stages != null && checklist.Stages.Count > 0 && checklist.Tasks != null && checklist.Tasks.Count > 0)
            {
                var result = await AddTasksStagesRelations(userId: userId, companyId: companyId, stages: checklist.Stages, tasks: checklist.Tasks);
            }

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added checklist.");
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeChecklistAsync; Change a checklist in the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="checklistId">ChecklistId, id of the object in the database that needs to be updated. (DB: checklists_checklist.id) </param>
        /// <param name="checklist">Checklist object containing all data needed for updating the database. (DB: checklists_checklist)</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeChecklistAsync(int companyId, int userId, int checklistId, Checklist checklist)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), checklistId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromChecklist(checklist: checklist, companyId: companyId, checklistId: checklistId, userId: userId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_checklist", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (checklistId > 0 && checklist.Tasks != null)
            {
                var signedUserId = (checklist.Signatures != null && checklist.Signatures.Count > 0 && checklist.Signatures[0].SignedById.HasValue ? checklist.Signatures[0].SignedById.Value : 0); //retrieve possible owner id of item based on signature
                await ChangeChecklistAddOrChangeTask(companyId: companyId, userId: userId, possibleOwnerId: signedUserId, checklistId: checklistId, checklist.Tasks);
            }

            //change stages
            if (checklist.Stages != null && checklist.Stages.Count > 0)
            {
                await ChangeChecklistStages(companyId: companyId, checklistId: checklistId, userId: userId, stages: checklist.Stages);
            }

            if (checklist.PropertyUserValues != null && checklist.PropertyUserValues.Count > 0)
            {
                var result = await AddChangeChecklistPropertyUserValue(companyId: companyId, checklistId: checklistId, userId: userId, checklist.PropertyUserValues);
            }

            if (checklist.OpenFieldsPropertyUserValues != null && checklist.OpenFieldsPropertyUserValues.Count > 0)
            {
                var result = await AddChangeChecklistPropertyUserValue(companyId: companyId, checklistId: checklistId, userId: userId, checklist.OpenFieldsPropertyUserValues);
            }

            if (checklist.LinkedTaskId.HasValue && checklist.LinkedTaskId.Value > 0)
            {
                var result = await AddTaskChecklistLinkAsync(companyId: companyId, userId: userId, taskId: checklist.LinkedTaskId.Value, checklistId: checklistId, isRequired: checklist.IsRequiredForLinkedTask == true);
            }

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), checklistId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist.ToString(), objectId: checklistId, userId: userId, companyId: companyId, description: "Changed checklist.");

            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// CreateChecklistAsync; Create a new checklist based on a relation object.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="checklistRelation">ChecklistRelationStatus object containing all data for creating a checklist based on a template and a list of tasks.</param>
        /// <returns></returns>
        public async Task<ChecklistRelationStatus> CreateChecklistAsync(int companyId, int userId, ChecklistRelationStatus checklistRelation)
        {
            //TODO refactor
            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_company_id", companyId));
                parameters.Add(new NpgsqlParameter("@_checklisttemplate_id", checklistRelation.ChecklistTemplateId));
                parameters.Add(new NpgsqlParameter("@_tasktemplate_id", checklistRelation.TaskTemplateId));
                parameters.Add(new NpgsqlParameter("@_status", checklistRelation.TaskStatus.ToDatabaseString())); //TODO make correct converter method.

                using (dr = await _manager.GetDataReader("create_checklist", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: ConnectionKind.Writer))
                {
                    while (await dr.ReadAsync())
                    {
                        checklistRelation.ChecklistId = Convert.ToInt32(dr["checklist_id"]);
                        if (dr["task_id"] != DBNull.Value)
                        {
                            checklistRelation.TaskId = Convert.ToInt32(dr["task_id"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.CreateChecklistAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (checklistRelation.ChecklistId.HasValue && checklistRelation.ChecklistId > 0)
            {
                //Create data row json with children objects for audits and checklists
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), checklistRelation.ChecklistId.Value);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist.ToString(), objectId: checklistRelation.ChecklistId.Value, userId: userId, companyId: companyId, description: "Added checklist.");
            }

            return checklistRelation;
        }

        /// <summary>
        /// SetChecklistTaskStatusAsync; Set a status of a task based on a checklist.
        /// The following values must be supplied
        /// - ChecklistId and TaskTemplateId
        /// Or
        /// - TaskId
        /// Depending on what is supplied, the method will update the task status that corresponds to the TaskId or the Combination of ChecklistId and TaskTemplateId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistRelation">Relation object containing all necessary ids</param>
        /// <returns>ChecklistRelationStatus object.</returns>
        public async Task<ChecklistRelationStatus> SetChecklistTaskStatusAsync(int companyId, int userId, ChecklistRelationStatus checklistRelation)
        {
            if (checklistRelation.ChecklistId.HasValue && !checklistRelation.TaskId.HasValue)
            {
                //task id not supplied, go fish for id.
                var tasks = await GetTasksWithChecklist(companyId: companyId, checklistId: checklistRelation.ChecklistId.Value);
                var foundTask = tasks.Where(x => x.TemplateId == checklistRelation.TaskTemplateId).FirstOrDefault();
                if (foundTask != null)
                {
                    checklistRelation.TaskId = foundTask.Id;
                }
            }
            if (checklistRelation.TaskId.HasValue && checklistRelation.TaskId > 0)
            {
                var result = await _taskManager.SetTaskStatusAsync(companyId: companyId, taskId: Convert.ToInt32(checklistRelation.TaskId.Value), userId: userId, checklistRelation.TaskStatus);
            }

            return checklistRelation;
        }

        /// <summary>
        /// SetChecklistActiveAsync; Set Checklist active/inactive based on ChecklistId.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a Checklist to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetChecklistActiveAsync(int companyId, int userId, int checklistId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), checklistId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", checklistId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_checklist_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), checklistId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist.ToString(), objectId: checklistId, userId: userId, companyId: companyId, description: "Changed checklist active state.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="checklistId"></param>
        /// <param name="isCompleted"></param>
        /// <returns></returns>
        public async Task<bool> SetChecklistCompletedAsync(int companyId, int userId, int checklistId, bool isCompleted = true)
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// ChecklistSigningAsync; Sign a checklist with one or more signatures.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <param name="signing">Signing relation object containing signatures and ids</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ChecklistSigningAsync(int companyId, int userId, int checklistId, ChecklistRelationSigning signing)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), checklistId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", signing.ChecklistId));
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

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("sign_checklist", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist.ToString(), checklistId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist.ToString(), objectId: checklistId, userId: userId, companyId: companyId, description: "Signed checklist.");
            }

            return (rowseffected > 0);
        }
        #endregion

        #region - public methods ChecklistTemplates -
        /// <summary>
        /// GetChecklistTemplatesAsync; Get collection of ChecklistsTemplates.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="include">Comma separated string based on the IncludesTypeEnum</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <returns>A List of ChecklistsTemplates.</returns>
        public async Task<List<ChecklistTemplate>> GetChecklistTemplatesAsync(int companyId, int? userId = null, ChecklistFilters? filters = null, string include = null)
        {
            var output = new List<ChecklistTemplate>();

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
                            if(role == RoleTypeEnum.Basic)
                            {
                                rolesFilterValue.Add("basic");
                            }
                            else if(role == RoleTypeEnum.ShiftLeader)
                            {
                                rolesFilterValue.Add("shift_leader");
                            }
                            else if(role == RoleTypeEnum.Manager)
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

                using (dr = await _manager.GetDataReader("get_checklisttemplates_v2", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var checklisttemplate = CreateOrFillChecklistTemplateFromReader(dr);
                        output.Add(checklisttemplate);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetChecklistTemplatesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && filters.Value.HasFilters())
            {
                output = (await FilterChecklistTemplates(companyId: companyId, userId: userId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            }

            if (output.Count > 0)
            {
                //NOTE! if steps need to be included, also task templates are included.
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.TaskTemplates.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower()))) output = await AppendChecklistTemplateTaskTemplatesAndStageTemplates(companyId: companyId, checklisttemplates: output, filters: filters, userId: userId, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower())) output = await AppendChecklistTemplateStepsAsync(companyId: companyId, checklisttemplates: output, filters: filters, userId: userId);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower())) output = await AppendAreaPathsToChecklistTemplatesAsync(companyId: companyId, checklisttemplates: output, addAreaPath: include.Split(",").Contains(IncludesEnum.AreaPaths.ToString().ToLower()), addAreaPathIds: include.Split(",").Contains(IncludesEnum.AreaPathIds.ToString().ToLower()));
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))) output = await AppendTemplatePropertiesToTaskTemplates(checklisttemplates: output, companyId: companyId, include: include);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) output = await AppendTemplatePropertiesToTemplates(checklisttemplates: output, companyId: companyId, include: include);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower()))) output = await AppendWorkInstructionRelationsAsync(checklistTemplates: output, companyId: companyId);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))) output = await AppendTagsToChecklistTemplatesAsync(checklistTemplates: output, companyId: companyId);
            }

            return output;
        }

        /// <summary>
        /// GetChecklistTemplateCountsAsync; Get counts of checklisttemplates with selected filters.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="include">Comma separated string based on the IncludesTypeEnum</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <returns>A List of ChecklistsTemplates.</returns>
        public async Task<ChecklistTemplateCountStatistics> GetChecklistTemplateCountsAsync(int companyId, int? userId = null, ChecklistFilters? filters = null, string include = null)
        {
            var output = new ChecklistTemplateCountStatistics();

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

                using (dr = await _manager.GetDataReader("get_checklisttemplates_v2_counts", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.TotalCount = Convert.ToInt32(dr["total_count"]);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetChecklistTemplateCountsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetChecklistTemplateAsync; Get a single ChecklistTemplate object based on the ChecklistTemplateId parameter. Based on the [checklists_checklisttemplate] table in the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId, the id of the object to get from the database. (DB: checklists_checklist.id)</param>
        /// <param name="include">Include, comma separated string based on IncludesEnum, used for including extra data.</param>
        /// <returns>ChecklistTemplate object.</returns>
        public async Task<ChecklistTemplate> GetChecklistTemplateAsync(int companyId, int checklistTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var checklisttemplate = new ChecklistTemplate();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", checklistTemplateId));

                using (dr = await _manager.GetDataReader("get_checklisttemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillChecklistTemplateFromReader(dr, checklisttemplate: checklisttemplate);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetChecklistTemplateAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (checklisttemplate.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.TaskTemplates.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower()))) checklisttemplate = await AppendTasktemplatesAndStagesToChecklistTemplate(companyId: companyId, checklistTemplate: checklisttemplate, include: include, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower())) checklisttemplate = await AppendChecklistTemplateStepsAsync(companyId: companyId, checklisttemplate: checklisttemplate, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))) checklisttemplate = await AppendTemplatePropertiesToTaskTemplates(checklisttemplate: checklisttemplate, companyId: companyId, include: include, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) checklisttemplate = await AppendTemplatePropertiesToTemplate(checklisttemplate: checklisttemplate, companyId: companyId, include: include, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) checklisttemplate.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.ChecklistTemplate, id: checklisttemplate.Id, connectionKind: connectionKind);
                return checklisttemplate;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get checklist template names based on checklist template ids
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="checklistTemplateIds">checklist template ids to get the names for</param>
        /// <returns>dictionary of checklist template ids and checklist template names</returns>
        public async Task<Dictionary<int, string>> GetChecklistTemplateNamesAsync (int companyId, List<int> checklistTemplateIds)
        {
            Dictionary<int, string> idsNames = new();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_checklisttemplateids", checklistTemplateIds)
                };

                using NpgsqlDataReader dr = await _manager.GetDataReader("get_checklisttemplate_names", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    int id = Convert.ToInt32(dr["id"]);
                    string name = dr["name"].ToString();
                    idsNames.Add(id, name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetChecklistTemplateNamesAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return idsNames;
        }

        /// <summary>
        /// AddChecklistTemplateAsync; Add a ChecklistTemplate to the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="checklistTemplate">ChecklistTemplate object, containing all relevant ChecklistTemplate data to add.(DB: checklists_checklisttemplate)</param>
        /// <returns>The identity of the table (DB: checklists_checklisttemplate.id)</returns>
        public async Task<int> AddChecklistTemplateAsync(int companyId, int userId, ChecklistTemplate checklistTemplate)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromChecklistTemplate(checklistTemplate: checklistTemplate, companyId: checklistTemplate.CompanyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklisttemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                checklistTemplate.Id = possibleId; //set id for further processing

                if (checklistTemplate.Tags != null && checklistTemplate.Tags.Count > 0)
                {
                    await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.ChecklistTemplate, id: possibleId, tags: checklistTemplate.Tags, companyId: companyId, userId: userId);
                }
                if (checklistTemplate.TaskTemplates != null && checklistTemplate.TaskTemplates.Count > 0)
                {
                    await ChangeChecklistTemplateAddOrChangeTaskTemplates(companyId: companyId, userId: userId, checklistTemplateId: possibleId, checklistTemplate.TaskTemplates);
                }

                if (checklistTemplate.HasStages)
                {
                    foreach (StageTemplate stageTemplate in checklistTemplate.StageTemplates)
                    {
                        SetStageTemplateTaskIds(stageTemplate, checklistTemplate);
                        stageTemplate.Id = await AddStageTemplateAsync(userId, companyId, stageTemplate, checklistTemplate.Id);
                        if (stageTemplate.TaskTemplateIds != null && stageTemplate.TaskTemplateIds.Count > 0)
                        {
                            var relationRowCount = await UpsertStageTemplateTaskTemplateRelations(userId: userId, companyId: companyId, stageTemplateId: stageTemplate.Id, taskTemplateIds: stageTemplate.TaskTemplateIds);
                        }
                    }
                }

                if (checklistTemplate.Properties != null && checklistTemplate.Properties.Count > 0)
                {
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: possibleId, templateProperties: checklistTemplate.Properties);
                }

                if (checklistTemplate.OpenFieldsProperties != null && checklistTemplate.OpenFieldsProperties.Count > 0)
                {
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: possibleId, templateProperties: checklistTemplate.OpenFieldsProperties);
                }
            }

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added checklist template.");
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeChecklistTemplateAsync; Change a ChecklistTemplate in the database.
        /// NOTE! when not supplying ANY TaskTemplates when, tasktemplates that are already coupled will not be automatically deleted.
        /// Technically a checklist needs items for display so checklists without don't exists.
        /// When supplying checklist items, any item that is not supplied is automatically removed. (set active = false).
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId, id of the object in the database that needs to be updated. (DB: checklists_checklisttemplate.id) </param>
        /// <param name="checklistTemplate">ChecklistTemplate object containing all data needed for updating the database. (DB: checklists_checklisttemplate)</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeChecklistTemplateAsync(int companyId, int userId, int checklistTemplateId, ChecklistTemplate checklistTemplate)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), checklistTemplateId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromChecklistTemplate(checklistTemplate: checklistTemplate, companyId: companyId, checklistTemplateId: checklistTemplateId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_checklisttemplate", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                checklistTemplate.Tags ??= new();
                await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.ChecklistTemplate, id: checklistTemplateId, tags: checklistTemplate.Tags, companyId: companyId, userId: userId);

                //if there are any task templates, change/add those if needed.
                //if there are no task templates, potentially set the currently active ones to inactive
                await ChangeChecklistTemplateAddOrChangeTaskTemplates(companyId: companyId, userId: userId, checklistTemplateId: checklistTemplateId, checklistTemplate.TaskTemplates);
                List<StageTemplate> originalStageTemplates = await GetStageTemplatesByChecklistTemplateIdAsync(companyId, checklistTemplateId);
                var originalStageTemplateIds = originalStageTemplates?.Select(stage => stage.Id) ?? new List<int>();
                var newStageTemplateIds = checklistTemplate.StageTemplates?.Select(stage => stage.Id) ?? new List<int>();
                var removedStageTemplateIds = originalStageTemplateIds.Except(newStageTemplateIds);

                foreach (StageTemplate stageTemplate in originalStageTemplates.Where(st => removedStageTemplateIds.Contains(st.Id)))
                {
                    //set removed stages inactive
                    bool success = await SetStageTemplateActiveAsync(userId: userId, companyId: companyId, checklistStageTemplateId: stageTemplate.Id, isActive: false);
                }

                if (checklistTemplate.HasStages)
                {
                    foreach (StageTemplate stageTemplate in checklistTemplate.StageTemplates)
                    {
                        SetStageTemplateTaskIds(stageTemplate, checklistTemplate);

                        if (stageTemplate.Id > 0)
                        {
                            //update existing stage stage
                            var rowCount = await ChangeStageTemplateAsync(userId: userId, companyId: companyId, stageTemplate: stageTemplate);
                        }
                        else
                        {
                            //add new stage
                            var possibleId = await AddStageTemplateAsync(userId: userId, companyId: companyId, stageTemplate: stageTemplate, checklistTemplateId: checklistTemplateId);
                        }

                        //get current stage tasktemplate relations
                        var stageTaskTemplateRelations = await GetStageTaskTemplateRelationsAsync(companyId: companyId, stageTemplateId: stageTemplate.Id);

                        //determine the no longer active relations
                        var inactiveRelations = stageTaskTemplateRelations.Where(r => !stageTemplate.TaskTemplateIds.Contains(r.TaskTemplateId)).ToList();

                        //remove them
                        if (inactiveRelations != null && inactiveRelations.Count > 0)
                        {
                            foreach(var inactiveRelation in inactiveRelations)
                            {
                                await RemoveStageTemplateTaskTemplateRelation(id: inactiveRelation.Id, taskTemplateId: inactiveRelation.TaskTemplateId, checklistTemplateStageId: inactiveRelation.StageTemplateId, userId: userId, companyId: companyId);
                            }
                        }

                        if (stageTemplate.TaskTemplateIds != null && stageTemplate.TaskTemplateIds.Count > 0)
                        {
                            //upsert all stage tasktemplate relations here
                            int rowcount = await UpsertStageTemplateTaskTemplateRelations(userId: userId, companyId: companyId, stageTemplateId: stageTemplate.Id, taskTemplateIds: stageTemplate.TaskTemplateIds);
                        }
                    }
                }

                if (checklistTemplate.Properties != null)
                {
                    //general properties
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: checklistTemplateId, templateProperties: checklistTemplate.Properties);
                }

                if (checklistTemplate.OpenFieldsProperties != null)
                {
                    //open field specific properties
                    var propNr = await AddChangeTemplatePropertiesAsync(companyId: companyId, userId: userId, templateId: checklistTemplateId, templateProperties: checklistTemplate.OpenFieldsProperties);
                }
            }

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), checklistTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), objectId: checklistTemplateId, userId: userId, companyId: companyId, description: "Changed checklist template.");
            }

            return rowseffected > 0;
        }

        /// <summary>
        /// SetChecklistTemplateActiveAsync; Set ChecklistTemplate active/inactive based on ChecklistTemplateId.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId (DB: checklists_checklisttemplate.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a ChecklistTemplate to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false.</returns>
        public async Task<bool> SetChecklistTemplateActiveAsync(int companyId, int userId, int checklistTemplateId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), checklistTemplateId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", checklistTemplateId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_checklisttemplate_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), checklistTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), objectId: checklistTemplateId, userId: userId, companyId: companyId, description: "Changed checklist template active state.");

            }

            return (rowseffected > 0);
        }
        #endregion

        #region - public methods ChecklistTemplate connections - 
        /// <summary>
        /// GetConnectedTaskTemplateIds; Get connected TaskTemplates to this ChecklistTemplate
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId, the id of the object to get from the database. (DB: checklists_checklist.id)</param>
        /// <returns>List<int> with all task template ids</returns>
        public async Task<List<int>> GetConnectedTaskTemplateIds(int companyId, int checklistTemplateId)
        {
            var taskTemplateIds = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_checklisttemplateid", checklistTemplateId));

                using (dr = await _manager.GetDataReader("get_checklisttemplate_linked_tasktemplates", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        taskTemplateIds.Add(Convert.ToInt32(dr["tasktemplate_id"]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetConnectedTaskTemplateIds(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return taskTemplateIds;
        }
        #endregion

        #region - private methods Filters Checklist -
        /// <summary>
        /// FilterChecklists; FilterChecklist is the primary filter method for filtering checklists, within this method the specific filters are determined based on the supplied ChecklistFilters object.
        /// Filtering is done based on cascading filters, meaning, the first filter is applied, which results in a filtered collection.
        /// On that filtered collection the second filter is applied which results in a filtered-filtered collection.
        /// This will continue until all filters are applied.
        /// NOTE! Way of filtering obsolete; All filter methods used here if still used needs to be moved when refactoring to front query filters (e,g, query parameters and let the database do the filtering) /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="filters">ChecklistFilters, depending on the values certain filters will be applies.</param>
        /// <param name="nonFilteredCollection">List of non filtered Checklist objects.</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>A filtered list of Checklist objects.</returns>
        private async Task<IList<Checklist>> FilterChecklists(int companyId, ChecklistFilters filters, IList<Checklist> nonFilteredCollection, int? userId = null)
        {
            var filtered = nonFilteredCollection;
            if (filters.SignedById.HasValue)
            {
                filtered = await FilterChecklistsOnSignedOnId(signedById: filters.SignedById.Value, checklists: filtered);
            }
            if (filters.TemplateId.HasValue)
            {
                filtered = await FilterChecklistsOnTemplateId(templateId: filters.TemplateId.Value, checklists: filtered);
            }
            return filtered;
        }

        /// <summary>
        /// FilterChecklistsOnSignedOnId; Filter a Checklist collection on SignedById.
        /// </summary>
        /// <param name="signedById">SignedById ( DB: checklists_checklist.signed_by_id)</param>
        /// <param name="checklists">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Checklist>> FilterChecklistsOnSignedOnId(int signedById, IList<Checklist> checklists)
        {
            checklists = checklists.Where(x => x.Signatures != null && x.Signatures.Where(y => y.SignedById == signedById).Any()).ToList();
            await Task.CompletedTask; //make method execute in async flow.
            return checklists;
        }

        /// <summary>
        /// FilterChecklistsOnTemplateId; Filter a Checklist collection on TemplateId.
        /// </summary>
        /// <param name="templateId">TemplateId ( DB: checklists_checklist.template_id)</param>
        /// <param name="checklists">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<Checklist>> FilterChecklistsOnTemplateId(int templateId, IList<Checklist> checklists)
        {
            checklists = checklists.Where(x => x.TemplateId == templateId).ToList();
            await Task.CompletedTask; //make method execute in async flow.
            return checklists;
        }

        #endregion

        #region - private methods Filters ChecklistTemplates -
        /// <summary>
        /// FilterChecklistTemplates; FilterChecklistTemplates is the primary filter method for filtering ChecklistTemplates, within this method the specific filters are determined based on the supplied ChecklistFilters object.
        /// Filtering is done based on cascading filters, meaning, the first filter is applied, which results in a filtered collection.
        /// On that filtered collection the second filter is applied which results in a filtered-filtered collection.
        /// This will continue until all filters are applied.
        /// NOTE! Way of filtering obsolete; All filter methods used here if still used needs to be moved when refactoring to front query filters (e,g, query parameters and let the database do the filtering)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">ChecklistFilters, depending on the values certain filters will be applies.</param>
        /// <param name="nonFilteredCollection">List of non filtered ChecklistTemplate objects.</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>A filtered list of ChecklistTemplate objects.</returns>
        private async Task<IList<ChecklistTemplate>> FilterChecklistTemplates(int companyId, ChecklistFilters filters, IList<ChecklistTemplate> nonFilteredCollection, int? userId = null)
        {
            var filtered = nonFilteredCollection;
            if (filters.RoleType.HasValue)
            {
                filtered = await FilterChecklistTemplatesOnRole(role: filters.RoleType.Value, checklistTemplates: filtered);
            }
            return filtered;
        }

        /// <summary>
        /// FilterChecklistTemplatesOnRole; Filter a ChecklistTemplate collection on role.
        /// </summary>
        /// <param name="role">RoleTypeEnum, roles are stored as a string in the database. Internally we use a enumerator to represent those stings. ( DB: checklists_checklisttemplate.role)</param>
        /// <param name="checklistTemplates">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<ChecklistTemplate>> FilterChecklistTemplatesOnRole(RoleTypeEnum role, IList<ChecklistTemplate> checklistTemplates)
        {

            checklistTemplates = checklistTemplates.Where(x => x.Role == role.ToDatabaseString().ToString().ToLower()).ToList();
            await Task.CompletedTask;
            return checklistTemplates;
        }

        #endregion

        #region - private methods ChecklistTemplates -
        /// <summary>
        /// AppendTagsToChecklistTemplatesAsync; append tags to ChecklistTemplate collection.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistTemplates">Collection of ChecklistTemplate</param>
        /// <returns>Collection of ChecklistTemplate</returns>
        private async Task<List<ChecklistTemplate>> AppendTagsToChecklistTemplatesAsync(int companyId, List<ChecklistTemplate> checklistTemplates)
        {
            var allTagsOnChecklistTemplates = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.ChecklistTemplate);
            if (allTagsOnChecklistTemplates != null)
            {
                foreach (var checklistTemplate in checklistTemplates)
                {
                    var tagsOnThisChecklistTemplate = allTagsOnChecklistTemplates.Where(t => t.ObjectId == checklistTemplate.Id).ToList();
                    if (tagsOnThisChecklistTemplate != null && tagsOnThisChecklistTemplate.Count > 0)
                    {
                        checklistTemplate.Tags ??= new List<Models.Tags.Tag>();
                        checklistTemplate.Tags.AddRange(tagsOnThisChecklistTemplate);
                    }

                }
            }

            return checklistTemplates;
        }

        private async Task<List<Checklist>> AppendTagsToChecklistsAsync(int companyId, List<Checklist> checklists)
        {
            var allTagsOnChecklists = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.Checklist);
            if (allTagsOnChecklists != null)
            {
                foreach (var checklist in checklists)
                {
                    var tagsOnThisChecklist = allTagsOnChecklists.Where(t => t.ObjectId == checklist.Id).ToList();
                    if (tagsOnThisChecklist != null && tagsOnThisChecklist.Count > 0)
                    {
                        checklist.Tags ??= new List<Models.Tags.Tag>();
                        checklist.Tags.AddRange(tagsOnThisChecklist);
                    }

                }
            }

            return checklists;
        }

        /// <summary>
        /// AppendTagsToChecklistStageTemplatesAsync; append tags to StageTemplate collection.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="stageTemplates">Collection of StageTemplate</param>
        /// <returns>Collection of ChecklistTemplate</returns>
        private async Task<List<StageTemplate>> AppendTagsToChecklistStageTemplatesAsync(int companyId, List<StageTemplate> stageTemplates, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var allTagsOnStageTemplates = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.ChecklistTemplateStage, connectionKind: connectionKind);
            if (allTagsOnStageTemplates != null)
            {
                foreach (var stageTemplate in stageTemplates)
                {
                    var tagsOnThisStageTemplate = allTagsOnStageTemplates.Where(t => t.ObjectId == stageTemplate.Id).ToList();
                    if (tagsOnThisStageTemplate != null && tagsOnThisStageTemplate.Count > 0)
                    {
                        stageTemplate.Tags ??= new List<Models.Tags.Tag>();
                        stageTemplate.Tags.AddRange(tagsOnThisStageTemplate);
                    }

                }
            }

            return stageTemplates;
        }

        /// <summary>
        /// AppendTagsToChecklistStagesAsync; append tags to Stages collection.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="stages">Collection of Stage</param>
        /// <returns>Collection of StageTemplate</returns>
        private async Task<List<Stage>> AppendTagsToChecklistStagesAsync(int companyId, List<Stage> stages)
        {
            var allTagsOnStages = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.ChecklistStage);
            if (allTagsOnStages != null)
            {
                foreach (var stage in stages)
                {
                    var tagsOnThisStage = allTagsOnStages.Where(t => t.ObjectId == stage.Id).ToList();
                    if (tagsOnThisStage != null && tagsOnThisStage.Count > 0)
                    {
                        stage.Tags ??= new List<Models.Tags.Tag>();
                        stage.Tags.AddRange(tagsOnThisStage);
                    }

                }
            }

            return stages;
        }

        /// <summary>
        /// AppendUserInformationToChecklists; Append firstname, lastname combinations to objects with checklists which are separately stored. (e.g. modified_by_id etc).
        /// </summary>
        /// <param name="companyId">CompanyId of all users that need to be retrieved.</param>
        /// <param name="checklists">Checklists that need to be amended with data</param>
        /// <returns>return updated checklists</returns>
        private async Task<List<Checklist>> AppendUserInformationToChecklistsAsync(int companyId, List<Checklist> checklists)
        {
            var possibleUsers = await _userManager.GetUserProfilesAsync(companyId: companyId);
            if (possibleUsers != null && checklists != null)
            {
                foreach (var checklist in checklists)
                {
                    AppendUserInformationToChecklist(checklist: checklist, possibleUsers: possibleUsers);
                }
            }

            return checklists;
        }

        /// <summary>
        /// AppendUserInformationToChecklistAsync; Append firstname, lastname combinations to objects with checklists which are separately stored. (e.g. modified_by_id etc).
        /// </summary>
        /// <param name="companyId">CompanyId of all users that need to be retrieved.</param>
        /// <param name="checklist">Checklist that need to be amended with data</param>
        /// <returns>return updated checklist</returns>
        private async Task<Checklist> AppendUserInformationToChecklistAsync(int companyId, Checklist checklist)
        {
            var possibleUsers = await _userManager.GetUserProfilesAsync(companyId: companyId);
            if (possibleUsers != null && checklist != null)
            {
                checklist = AppendUserInformationToChecklist(checklist: checklist, possibleUsers: possibleUsers);
            }

            return checklist;
        }

        private Checklist AppendUserInformationToChecklist(Checklist checklist, List<UserProfile> possibleUsers)
        {
            var editedByUserIds = new List<int>();

            if (checklist.CreatedById > 0)
            {
                var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == checklist.CreatedById);
                if (possibleUser != null)
                {
                    checklist.CreatedByUser = new Models.Basic.UserBasic()
                    {
                        Id = possibleUser.Id,
                        Name = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName),
                        Picture = possibleUser.Picture
                    };
                }
            }

            if (checklist.ModifiedById > 0)
            {
                var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == checklist.ModifiedById);
                if (possibleUser != null)
                {
                    checklist.ModifiedByUser = new Models.Basic.UserBasic()
                    {
                        Id = possibleUser.Id,
                        Name = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName),
                        Picture = possibleUser.Picture
                    };
                }
            }

            if(checklist.Signatures != null && checklist.Signatures.Count > 0)
            {
                foreach (var signature in checklist.Signatures)
                {
                    if (signature.SignedById > 0)
                    {
                        var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == signature.SignedById);
                        if (possibleUser != null)
                        {
                            editedByUserIds.Add(possibleUser.Id);
                        }
                    }
                }
            }

            if (checklist.Stages != null && checklist.Stages.Count > 0)
            {
                //add SignedByUser to stage signatures
                foreach (var stage in checklist.Stages)
                {
                    if (stage.Signatures != null && stage.Signatures.Count > 0)
                    {
                        foreach (var signature in stage.Signatures)
                        {
                            if (signature != null && signature.SignedById > 0)
                            {
                                var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == signature.SignedById);
                                if (possibleUser != null)
                                {
                                    signature.SignedByPicture = possibleUser.Picture;
                                    signature.SignedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);

                                    editedByUserIds.Add(possibleUser.Id);
                                }
                            }
                        }
                    }

                    if (stage.CreatedById > 0)
                    {
                        var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == stage.CreatedById);
                        if (possibleUser != null)
                        {
                            editedByUserIds.Add(possibleUser.Id);
                        }
                    }

                    if (stage.ModifiedById > 0)
                    {
                        var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == stage.ModifiedById);
                        if (possibleUser != null)
                        {
                            editedByUserIds.Add(possibleUser.Id);
                        }
                    }
                }
            }

            if (checklist.PropertyUserValues != null)
            {
                foreach (var prop in checklist.PropertyUserValues)
                {
                    if (prop.UserId > 0)
                    {
                        var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == prop.UserId);
                        if (possibleUser != null)
                        {
                            prop.ModifiedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);

                            editedByUserIds.Add(possibleUser.Id);
                        }
                    }
                }
            }

            if (checklist.OpenFieldsPropertyUserValues != null)
            {
                foreach (var openprop in checklist.OpenFieldsPropertyUserValues)
                {
                    if (openprop.UserId > 0)
                    {
                        var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == openprop.UserId);
                        if (possibleUser != null)
                        {
                            openprop.ModifiedBy = string.Concat(possibleUser.FirstName, " ", possibleUser.LastName);

                            editedByUserIds.Add(possibleUser.Id);
                        }
                    }
                }
            }
            
            if (checklist.Tasks != null)
            {
                foreach (var item in checklist.Tasks)
                {
                    var taskEditedByUserIds = new List<int>();
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

                                    editedByUserIds.Add(possibleUser.Id);
                                    taskEditedByUserIds.Add(possibleUser.Id);
                                }
                            }
                        }
                    }

                    if (item.Signature != null && item.Signature.SignedById > 0)
                    {
                        var possibleUser = possibleUsers.FirstOrDefault(x => x.Id == item.Signature.SignedById);
                        if (possibleUser != null)
                        {
                            editedByUserIds.Add(possibleUser.Id);
                            taskEditedByUserIds.Add(possibleUser.Id);
                        }
                    }

                    if (taskEditedByUserIds != null && taskEditedByUserIds.Count > 0)
                    {
                        item.EditedByUsers = possibleUsers.Distinct()
                                                               .Where(u => taskEditedByUserIds.Distinct().Contains(u.Id))
                                                               .Select(u => new Models.Basic.UserBasic()
                                                               {
                                                                   Id = u.Id,
                                                                   Name = string.Concat(u.FirstName, " ", u.LastName),
                                                                   Picture = u.Picture
                                                               }).ToList();
                    }
                }
            }

            //only if no edited by user ids are found, add the created and modified by user ids if they are set.
            if (editedByUserIds.Count == 0)
            {
                if(checklist.CreatedById > 0)
                {
                    editedByUserIds.Add(checklist.CreatedById.Value);
                }

                if (checklist.ModifiedById > 0)
                {
                    editedByUserIds.Add(checklist.ModifiedById.Value);
                }
            }

            if (editedByUserIds != null && editedByUserIds.Count > 0)
            {
                checklist.EditedByUsers = possibleUsers.Distinct()
                                                       .Where(u => editedByUserIds.Distinct().Contains(u.Id))
                                                       .Select(u => new Models.Basic.UserBasic()
                                                       {
                                                           Id = u.Id,
                                                           Name = string.Concat(u.FirstName, " ", u.LastName),
                                                           Picture = u.Picture
                                                       }).ToList();
            }

            return checklist;
        }

        private async Task<ChecklistTemplate> AppendTasktemplatesAndStagesToChecklistTemplate(int companyId, ChecklistTemplate checklistTemplate, string include = "", ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            checklistTemplate.TaskTemplates = await GetTaskTemplatesWithChecklistTemplate(companyId: companyId, checklistTemplateId: checklistTemplate.Id, include: include, connectionKind: connectionKind);
            
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_CHECKLIST_STAGES"))
                checklistTemplate.StageTemplates = await GetStageTemplatesByChecklistTemplateIdAsync(companyId: companyId, checklistTemplateId: checklistTemplate.Id, include: include, connectionKind: connectionKind);

            if (checklistTemplate.TaskTemplates != null && checklistTemplate.StageTemplates != null)
            {
                foreach(var stageTemplate in checklistTemplate.StageTemplates)
                {
                    if (stageTemplate.TaskTemplateIds != null && stageTemplate.TaskTemplateIds.Count > 0)
                    {
                        var taskTemplateIdsInStage = checklistTemplate.TaskTemplates.Where(t => stageTemplate.TaskTemplateIds.Contains(t.Id)).OrderBy(t => t.Index).Select(t => t.Id).ToList();

                        stageTemplate.TaskTemplateIds = taskTemplateIdsInStage;
                    }
                }
            }

            return checklistTemplate;
        }

        /// <summary>
        /// GetTaskTemplatesWithChecklistTemplate; Gets a list of TaskTemplates based on the ChecklistTemplateId.
        /// These TaskTemplates are part of the ChecklistTemplates and can be used for creating a new Audit to be filled in by a User.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId (DB: checklists_checklisttemplate.id)</param>
        /// <returns>A List of TaskTemplates</returns>
        public async Task<List<TaskTemplate>> GetTaskTemplatesWithChecklistTemplate(int companyId, int checklistTemplateId, string include = "", ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = await _taskManager.GetTaskTemplatesByChecklistTemplateIdAsync(companyId: companyId, checklistTemplateId: checklistTemplateId, include: include, connectionKind: connectionKind);
            if (output != null && output.Count > 0)
            {
                //_logger.Log(logLevel: LogLevel.Information, message: "GetTaskTemplatesWithChecklistTemplate {0} {1}", companyId, checklistTemplateId, include);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower()))) output = await AppendWorkInstructionRelationsAsync(companyId: companyId, checklistTemplateId: checklistTemplateId, templateItems: output, connectionKind: connectionKind);
                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Instructions.ToString().ToLower()))) output = await AppendWorkInstructionsAsync(companyId: companyId, checklistTemplateId: checklistTemplateId, templateItems: output, connectionKind: connectionKind);

                return output;
            }
            return null;
        }


        /// <summary>
        /// AppendChecklistTemplateTaskTemplatesAsync; Append TasksTemplates to ChecklistTemplate object.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplates">Collection of checklist templates.</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="fitlers">Filters used for further retrieval of data, will be converted to TaskFilters</param>
        /// <returns>The list of checklist templates, appended with Tasks.</returns>
        private async Task<List<ChecklistTemplate>> AppendChecklistTemplateTaskTemplatesAsync(int companyId, List<ChecklistTemplate> checklisttemplates, ChecklistFilters? filters = null, int? userId = null, string include = null)
        {
            var taskFilters = filters.ToTaskFilters();
            var tasktemplates = await _taskManager.GetTasksTemplatesWithChecklistTemplatesAsync(companyId: companyId, checklistIds: checklisttemplates.Select(c => c.Id).ToList(), filters: taskFilters, userId: userId, include: include);
            if (tasktemplates != null && tasktemplates.Count > 0)
            {
                foreach (var checklisttemplate in checklisttemplates)
                {
                    checklisttemplate.TaskTemplates = tasktemplates.Where(x => x.ChecklistTemplateId.HasValue && x.ChecklistTemplateId == checklisttemplate.Id).ToList();
                }
            }

            return checklisttemplates;
        }

        private async Task<List<ChecklistTemplate>> AppendChecklistTemplateStageTemplatesAsync(int companyId, List<ChecklistTemplate> checklisttemplates, string include = null)
        {
            List<StageTemplate> stageTemplates = await GetStageTemplatesForChecklistTemplateIds(companyId: companyId, checklistTemplateIds: checklisttemplates.Select(c => c.Id).ToList(), include: include);
            if (stageTemplates != null && stageTemplates.Count > 0)
            {
                foreach (var checklisttemplate in checklisttemplates)
                {
                    checklisttemplate.StageTemplates = stageTemplates.Where(stageTemplate => stageTemplate.ChecklistTemplateId == checklisttemplate.Id).ToList();
                }
            }

            return checklisttemplates;
        }

        private async Task<List<ChecklistTemplate>> AppendChecklistTemplateTaskTemplatesAndStageTemplates(int companyId, List<ChecklistTemplate> checklisttemplates, ChecklistFilters? filters = null, int? userId = null, string include = null)
        {
            checklisttemplates = await AppendChecklistTemplateTaskTemplatesAsync(companyId, checklisttemplates, filters, userId, include);

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_CHECKLIST_STAGES"))
                checklisttemplates = await AppendChecklistTemplateStageTemplatesAsync(companyId, checklisttemplates, include: include);

            return checklisttemplates;
        }

        /// <summary>
        /// AppendChecklistTemplateStepsAsync; Append Steps to ChecklistTemplate object (on each TaskTemplate if available).
        /// NOTE! if no TaskTemplates are available (e.g. the collection that is the source for the checklisttemplates parameter doesn't have them). No steps will be available on output.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplates">Collection of checklist templates.</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="fitlers">Filters used for further retrieval of data, will be converted to TaskFilters</param>
        /// <returns>The list of checklist templates, appended with Steps.</returns>
        private async Task<List<ChecklistTemplate>> AppendChecklistTemplateStepsAsync(int companyId, List<ChecklistTemplate> checklisttemplates, ChecklistFilters? filters = null, int? userId = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var taskFilters = filters.ToTaskFilters();
            var steps = await _taskManager.GetTaskTemplateStepsWithChecklistsAsync(companyId: companyId, userId: userId, filters: taskFilters, connectionKind: connectionKind);
            if (steps != null && steps.Count > 0)
            {
                foreach (var checklisttemplate in checklisttemplates)
                {
                    if (checklisttemplate.TaskTemplates != null && checklisttemplate.TaskTemplates.Count > 0)
                    {
                        foreach (var tasktemplate in checklisttemplate.TaskTemplates)
                        {
                            tasktemplate.Steps = steps.Where(x => x.TaskTemplateId == tasktemplate.Id).ToList();
                        }
                    }
                }
            }

            return checklisttemplates;
        }

        /// <summary>
        /// AppendChecklistTemplateStepsAsync; Append Steps to ChecklistTemplate object (on each TaskTemplate if available).
        /// NOTE! if no TaskTemplates are available (e.g. the object that is the source for the checklisttemplate parameter doesn't have them). No steps will be available on output.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplate">Checklist template.</param>
        /// <returns>ChecklistTemplate, appended with Steps.</returns>
        private async Task<ChecklistTemplate> AppendChecklistTemplateStepsAsync(int companyId, ChecklistTemplate checklisttemplate, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            if (checklisttemplate.TaskTemplates == null || checklisttemplate.TaskTemplates.Count == 0)
                return checklisttemplate;

            List<int> taskTemplateIds = checklisttemplate.TaskTemplates.Select(t => t.Id).ToList();
            var steps = await _taskManager.GetTaskTemplateStepsAsync(companyId: companyId, taskTemplateIds: taskTemplateIds, connectionKind: connectionKind);
            if (steps != null && steps.Count > 0)
            {
                if (checklisttemplate.TaskTemplates != null && checklisttemplate.TaskTemplates.Count > 0)
                {
                    foreach (var tasktemplate in checklisttemplate.TaskTemplates)
                    {
                        tasktemplate.Steps = steps.Where(x => x.TaskTemplateId == tasktemplate.Id).ToList();
                    }
                }
            }

            return checklisttemplate;
        }

        /// <summary>
        /// AppendAreaPathsToChecklistTemplatesAsync; Add the AreaPath to the ChecklistTemplates. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplates">List of checklisttemplates.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>ChecklistTemplates including area full path. </returns>
        private async Task<List<ChecklistTemplate>> AppendAreaPathsToChecklistTemplatesAsync(int companyId, List<ChecklistTemplate> checklisttemplates, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var checklisttemplate in checklisttemplates)
                {
                    var area = areas?.Where(x => x.Id == checklisttemplate.AreaId)?.FirstOrDefault();
                    if (area != null)
                    {
                        if (addAreaPath) checklisttemplate.AreaPath = area?.FullDisplayName;
                        if (addAreaPathIds) checklisttemplate.AreaPathIds = area?.FullDisplayIds;
                    }
                }
            }
            return checklisttemplates;
        }


        /// <summary>
        /// CreateOrFillChecklistTemplateFromReader; creates and fills a ChecklistTemplate object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="audit">ChecklistTemplate object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled ChecklistTemplate object.</returns>
        private ChecklistTemplate CreateOrFillChecklistTemplateFromReader(NpgsqlDataReader dr, ChecklistTemplate checklisttemplate = null)
        {
            if (checklisttemplate == null) checklisttemplate = new ChecklistTemplate();

            checklisttemplate.Id = Convert.ToInt32(dr["id"]);
            checklisttemplate.IsDoubleSignatureRequired = Convert.ToBoolean(dr["double_signature_required"]);
            checklisttemplate.Name = dr["name"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                checklisttemplate.Description = dr["description"].ToString();
            }
            checklisttemplate.AreaId = Convert.ToInt32(dr["area_id"]);
            checklisttemplate.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                checklisttemplate.Picture = dr["picture"].ToString();
            }
            if (dr.HasColumn("has_incomplete_checklists"))
            {
                if (dr["has_incomplete_checklists"] != DBNull.Value)
                {
                    checklisttemplate.HasIncompleteChecklists = Convert.ToBoolean(dr["has_incomplete_checklists"]);
                }
            }
            if (dr["role"] != DBNull.Value && !string.IsNullOrEmpty(dr["role"].ToString()))
            {
                checklisttemplate.Role = dr["role"].ToString();
            }
            if (dr.HasColumn("signature_required"))
            {
                if (dr["signature_required"] != DBNull.Value)
                {
                    checklisttemplate.IsSignatureRequired = Convert.ToBoolean(dr["signature_required"]);
                }
            }
            else
            {
                checklisttemplate.IsSignatureRequired = true; //TODO: Default to true, change when db updates are done in a later release.
            }

            if (dr.HasColumn("has_derived_items"))
            {
                if (dr["has_derived_items"] != DBNull.Value)
                {
                    checklisttemplate.HasDerivedItems = Convert.ToBoolean(dr["has_derived_items"]);
                }
            }

            if (dr.HasColumn("has_stages"))
            {
                if (dr["has_stages"] != DBNull.Value)
                {
                    checklisttemplate.ContainsStages = Convert.ToBoolean(dr["has_stages"]);
                }
            }

            if (dr.HasColumn("modified_at"))
            {
                if (dr["modified_at"] != DBNull.Value)
                {
                    checklisttemplate.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
                }
            }

            if (dr.HasColumn("version"))
            {
                if (dr["version"] != DBNull.Value)
                {
                    checklisttemplate.Version = Convert.ToString(dr["version"]);
                }
            }

            return checklisttemplate;
        }

        private StageTaskTemplateRelation CreateOrFillStageTaskTemplateRelationFromReader(NpgsqlDataReader dr, StageTaskTemplateRelation stageTaskTemplateRelation = null)
        {
            if (stageTaskTemplateRelation == null) stageTaskTemplateRelation = new StageTaskTemplateRelation();

            //id integer, company_id integer, stage_template_id integer, task_template_id integer
            stageTaskTemplateRelation.Id = Convert.ToInt32(dr["id"]);
            stageTaskTemplateRelation.CompanyId = Convert.ToInt32(dr["company_id"]);
            stageTaskTemplateRelation.StageTemplateId = Convert.ToInt32(dr["stage_template_id"]);
            stageTaskTemplateRelation.TaskTemplateId = Convert.ToInt32(dr["task_template_id"]);
            
            return stageTaskTemplateRelation;
        }

        /// <summary>
        /// Retrieve stage task template relations for the given companyid and stageTemplateId
        /// </summary>
        /// <param name="companyId">The id of the company that is coupled to the stage template</param>
        /// <param name="stageTemplateId">The id of the stage template that you want to retrieve task template relations for</param>
        /// <returns>A list of stage task template relations</returns>
        private async Task<List<StageTaskTemplateRelation>> GetStageTaskTemplateRelationsAsync(int companyId, int stageTemplateId)
        {
            NpgsqlDataReader dr = null;
            var stageTaskTemplateRelations = new List<StageTaskTemplateRelation>();
            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_stagetemplateid", stageTemplateId)
                };

                using (dr = await _manager.GetDataReader("get_checklisttemplate_stage_task_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: ConnectionKind.Writer))
                {
                    while (await dr.ReadAsync())
                    {
                        StageTaskTemplateRelation stageTaskTemplateRelation = null;
                        
                        stageTaskTemplateRelation = CreateOrFillStageTaskTemplateRelationFromReader(dr, stageTaskTemplateRelation);

                        if (stageTaskTemplateRelation != null && stageTaskTemplateRelation.Id > 0)
                        {
                            stageTaskTemplateRelations.Add(stageTaskTemplateRelation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetStageTaskTemplateRelationsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return stageTaskTemplateRelations;
        }

        /// <summary>
        /// GetNpgsqlParametersFromChecklistTemplate; Creates a list of NpgsqlParameters, and fills it based on the supplied ChecklistTemplate object.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="checklistTemplate">The supplied ChecklistTemplate object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistTemplateId">ChecklistTemplateId (DB: checklists_checklisttemplate.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromChecklistTemplate(ChecklistTemplate checklistTemplate, int companyId, int checklistTemplateId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (checklistTemplateId > 0) parameters.Add(new NpgsqlParameter("@_id", checklistTemplateId));

            // @Name, @Description, @Picture, @AreaId, @CompanyId, @DoubleSignatureRequired, @Role
            parameters.Add(new NpgsqlParameter("@_name", checklistTemplate.Name));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            parameters.Add(new NpgsqlParameter("@_areaid", checklistTemplate.AreaId));
            parameters.Add(new NpgsqlParameter("@_doublesignaturerequired", checklistTemplate.IsDoubleSignatureRequired));
            parameters.Add(new NpgsqlParameter("@_signaturerequired", checklistTemplate.IsSignatureRequired));

            if (!string.IsNullOrEmpty(checklistTemplate.Picture))
            {
                parameters.Add(new NpgsqlParameter("@_picture", checklistTemplate.Picture));
            }
            if (!string.IsNullOrEmpty(checklistTemplate.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", checklistTemplate.Description));
            }
            if (!string.IsNullOrEmpty(checklistTemplate.Role))
            {
                parameters.Add(new NpgsqlParameter("@_role", checklistTemplate.Role));
            }

            return parameters;
        }

        /// <summary>
        /// ChangeChecklistAddOrCreateTaskTemplates; Changes a checklisttemplate's taskitems.
        /// Note, based on the supplied templates if updating a existing template tasktemplates that are not supplied withing the collection are set to inactive.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId of the user that's making the changes.</param>
        /// <param name="checklistTemplateId">Id of the template that is being updated.</param>
        /// <param name="taskTemplates">Collection of tasktemplate items for the specific checklist.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> ChangeChecklistTemplateAddOrChangeTaskTemplates(int companyId, int userId, int checklistTemplateId, List<TaskTemplate> taskTemplates)
        {
            if(checklistTemplateId > 0)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_tasks.ToString(), Models.Enumerations.TableFields.checklisttemplate_id.ToString(), checklistTemplateId);

                if (checklistTemplateId > 0 && taskTemplates != null)
                {
                    var currentTaskTemplates = await GetTaskTemplatesWithChecklistTemplate(companyId: companyId, checklistTemplateId: checklistTemplateId); //get current steps in db;
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
                        await AddChangeTaskTemplate(companyId: companyId, userId: userId, checklistTemplateId: checklistTemplateId, taskTemplate: taskTemplate);
                    }
                }

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_tasks.ToString(), Models.Enumerations.TableFields.checklisttemplate_id.ToString(), checklistTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_tasks.ToString(), objectId: checklistTemplateId, userId: userId, companyId: companyId, description: "Changed checklisttemplate tasktemplate relation collection.");

            }

            return true;
        }

        /// <summary>
        /// AddChangeTaskTemplate; Add or change a single TaskTemplate. Based on supplied template a Add or Change functionality of the taskmanager will be called.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId of the user that's making the changes.</param>
        /// <param name="checklistTemplateId">Id of the template that is being updated.</param>
        /// <param name="taskTemplate">The template to be changed or added;</param>
        /// <returns>true/false depending on outcome;</returns>
        private async Task<bool> AddChangeTaskTemplate(int companyId, int userId, int checklistTemplateId, TaskTemplate taskTemplate)
        {
            if (checklistTemplateId > 0 && taskTemplate != null)
            {
                
                if (taskTemplate.Id > 0)
                {
                    taskTemplate.ChecklistTemplateId = checklistTemplateId;
                    var result = await _taskManager.ChangeTaskTemplateAsync(companyId: companyId, userId: userId, taskTemplateId: taskTemplate.Id, taskTemplate: taskTemplate);
                    return result;

                }
                else
                {
                    taskTemplate.ChecklistTemplateId = checklistTemplateId;
                    var resultid = await _taskManager.AddTaskTemplateAsync(companyId: companyId, userId: userId, taskTemplate: taskTemplate);
                    return resultid > 0;
                }

            }

            return true;
        }
        #endregion

        #region - private methods stages -
        /// <summary>
        /// Creates a new stage template if none is provided. Fills the stage template with data from the provided datareader.
        /// </summary>
        /// <param name="dr">Data reader</param>
        /// <param name="stageTemplate">Optional. Stage template to fill with data. If none is provided, a new stage template object will be created and returned.</param>
        /// <returns>Stage template filled with the data provided by the data reader</returns>
        private StageTemplate CreateOrFillStageTemplateFromReader(NpgsqlDataReader dr, StageTemplate stageTemplate = null)
        {
            stageTemplate ??= new();

            stageTemplate.Id = Convert.ToInt32(dr["Id"]);
            stageTemplate.CompanyId = Convert.ToInt32(dr["company_id"]);
            stageTemplate.ChecklistTemplateId = Convert.ToInt32(dr["checklisttemplate_id"]);
            stageTemplate.Name = Convert.ToString(dr["name"]);
            stageTemplate.BlockNextStagesUntilCompletion = Convert.ToBoolean(dr["block_next_stages_until_completion"]);
            stageTemplate.LockStageAfterCompletion = Convert.ToBoolean(dr["lock_stage_after_completion"]);
            stageTemplate.UseShiftNotes = Convert.ToBoolean(dr["use_shift_notes"]);
            stageTemplate.NumberOfSignaturesRequired = Convert.ToInt32(dr["number_of_signatures"]);
            stageTemplate.Index = Convert.ToInt32(dr["index"]);

            if (dr["description"] != DBNull.Value)
                stageTemplate.Description = Convert.ToString(dr["description"]);
            if (dr["tasktemplateids"] != DBNull.Value)
            {
                stageTemplate.TaskTemplateIds = ((int[])dr["tasktemplateids"]).ToList();
            }

            return stageTemplate;
        }

        private Stage CreateOrFillStageFromReader(NpgsqlDataReader dr, Stage stage = null)
        {
            stage ??= new();
            stage.Id = Convert.ToInt32(dr["id"]);
            stage.CompanyId = Convert.ToInt32(dr["company_id"]);
            
            if (dr["signatures"] != DBNull.Value && !string.IsNullOrEmpty(dr["signatures"].ToString()))
            {
                stage.Signatures = dr["signatures"].ToString().ToObjectFromJson<List<Signature>>();
                foreach (Signature signature in stage.Signatures)
                {
                    if (signature.SignedAt.HasValue && signature.SignedAt.Value.Kind == DateTimeKind.Unspecified)
                        signature.SignedAt = DateTime.SpecifyKind(signature.SignedAt.Value, DateTimeKind.Utc);
                }
            }

            stage.Status = Convert.ToString(dr["status"]);

            if (dr["shift_notes"] != DBNull.Value && !string.IsNullOrEmpty(dr["shift_notes"].ToString()))
            {
                stage.ShiftNotes = Convert.ToString(dr["shift_notes"]);
            }

            stage.StageTemplateId = Convert.ToInt32(dr["stage_template_id"]);
            stage.ChecklistId = Convert.ToInt32(dr["checklist_id"]);
            stage.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            stage.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            stage.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            stage.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
            stage.IsActive = Convert.ToBoolean(dr["is_active"]);

            if (dr.HasColumn("name") && dr["name"] != DBNull.Value)
            {
                stage.Name = Convert.ToString(dr["name"]);
            }

            if (dr.HasColumn("description") && dr["description"] != DBNull.Value)
            {
                stage.Description = Convert.ToString(dr["description"]);
            }

            if (dr.HasColumn("block_next_stages_until_completion") && dr["block_next_stages_until_completion"] != DBNull.Value)
            {
                stage.BlockNextStagesUntilCompletion = Convert.ToBoolean(dr["block_next_stages_until_completion"]);
            }

            if (dr.HasColumn("lock_stage_after_completion") && dr["lock_stage_after_completion"] != DBNull.Value)
            {
                stage.LockStageAfterCompletion = Convert.ToBoolean(dr["lock_stage_after_completion"]);
            }

            if (dr.HasColumn("use_shift_notes") && dr["use_shift_notes"] != DBNull.Value)
            {
                stage.UseShiftNotes = Convert.ToBoolean(dr["use_shift_notes"]);
            }

            if (dr.HasColumn("number_of_signatures") && dr["number_of_signatures"] != DBNull.Value)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                stage.NumberOfSignatures = Convert.ToInt32(dr["number_of_signatures"]); //for backwards compatibility
#pragma warning restore CS0618 // Type or member is obsolete
                stage.NumberOfSignaturesRequired = Convert.ToInt32(dr["number_of_signatures"]);
            }

            if (dr["taskids"] != DBNull.Value)
            {
                stage.TaskIds = ((int[])dr["taskids"]).ToList();
            }

            if (dr.HasColumn("index"))
            {
                stage.Index = Convert.ToInt32(dr["index"]);
            }

            return stage;
        }

        /// <summary>
        /// Get all StageTemplates for the checklist with given id
        /// </summary>
        /// <param name="companyId">CompanyId</param>
        /// <param name="checklistTemplateId">Target checklist template id</param>
        /// <returns></returns>
        public async Task<List<StageTemplate>> GetStageTemplatesByChecklistTemplateIdAsync(int companyId, int checklistTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<StageTemplate> stages = new();

            NpgsqlDataReader dr = null;

            try
            {

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_checklisttemplateid", checklistTemplateId)
                };

                using (dr = await _manager.GetDataReader("get_checklisttemplate_stages", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {

                    while (await dr.ReadAsync())
                    {
                        StageTemplate stage = CreateOrFillStageTemplateFromReader(dr);
                        stages.Add(stage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ChecklistManager.GetStageTemplatesByChecklistTemplateIdAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (stages.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) stages = await AppendTagsToChecklistStageTemplatesAsync(stageTemplates: stages, companyId: companyId, connectionKind: connectionKind);
            }

            return stages;
        }

        public async Task<List<Stage>> GetStagesByChecklistIdAsync(int companyId, int checklistId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<Stage> stages = new();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_checklistid", checklistId)
                };

                using (dr = await _manager.GetDataReader("get_checklist_stages", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        Stage stage = CreateOrFillStageFromReader(dr);
                        stages.Add(stage);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskManager.GetStagesByChecklistIdAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (stages.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) stages = await AppendTagsToChecklistStagesAsync(stages: stages, companyId: companyId);
            }
            return stages;
        }

        public async Task<List<Stage>> GetStagesByChecklistIdsAsync(int companyId, List<int> checklistIds, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<Stage> stages = new();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_checklistids", checklistIds.ToArray())
                };

                using (dr = await _manager.GetDataReader("get_checklists_stages", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        Stage stage = CreateOrFillStageFromReader(dr);
                        stages.Add(stage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskManager.GetStagesByChecklistIdsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


          
            
            if (stages.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) stages = await AppendTagsToChecklistStagesAsync(stages: stages, companyId: companyId);
            }

            return stages;
        }

        /// <summary>
        /// Get all stage templates for the checklist templates with the given ids
        /// </summary>
        /// <param name="companyId">Company id</param>
        /// <param name="checklistTemplateIds">List of target checklist templates</param>
        /// <returns></returns>
        private async Task<List<StageTemplate>> GetStageTemplatesForChecklistTemplateIds(int companyId, List<int> checklistTemplateIds, string include = null)
        {
            List<StageTemplate> stageTemplates = new();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_checklisttemplateids", checklistTemplateIds)
            };

                using (dr = await _manager.GetDataReader("get_checklisttemplates_stages", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        StageTemplate stageTemplate = CreateOrFillStageTemplateFromReader(dr);
                        stageTemplates.Add(stageTemplate);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("TaskManager.GetStageTemplatesForChecklistTemplateIds(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


           

            if (stageTemplates.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) stageTemplates = await AppendTagsToChecklistStageTemplatesAsync(stageTemplates: stageTemplates, companyId: companyId);
            }

            return stageTemplates;
        }

        /// <summary>
        /// Add a new stage template to a checklist template.
        /// Calls database function 'add_checklisttemplate_stage'.
        /// </summary>
        /// <param name="userId">User id of current user</param>
        /// <param name="companyId">Company id</param>
        /// <param name="stageTemplate">Stage template to add</param>
        /// <param name="checklistTemplateId">Id for the checklist template to add the stage template to</param>
        /// <returns>Id of the added stage template</returns>
        private async Task<int> AddStageTemplateAsync(int userId, int companyId, StageTemplate stageTemplate, int checklistTemplateId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_checklisttemplateid", checklistTemplateId),
                new NpgsqlParameter("@_name", stageTemplate.Name),
                new NpgsqlParameter("@_description", string.IsNullOrEmpty(stageTemplate.Description) ? DBNull.Value : stageTemplate.Description),
                new NpgsqlParameter("@_block_next_stages_until_completion", stageTemplate.BlockNextStagesUntilCompletion),
                new NpgsqlParameter("@_lock_stage_after_completion", stageTemplate.LockStageAfterCompletion),
                new NpgsqlParameter("@_use_shift_notes", stageTemplate.UseShiftNotes),
                new NpgsqlParameter("@_number_of_signatures", stageTemplate.NumberOfSignaturesRequired),
                new NpgsqlParameter("@_index", stageTemplate.Index),
                new NpgsqlParameter("@_userid", userId)
            };

            int possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklisttemplate_stage", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                if (stageTemplate.Tags != null && stageTemplate.Tags.Count > 0)
                {
                    await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.ChecklistTemplateStage, id: possibleId, tags: stageTemplate.Tags, companyId: companyId, userId: userId);
                }

                stageTemplate.Id = possibleId;

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage.ToString(), stageTemplate.Id);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_stage.ToString(), objectId: stageTemplate.Id, userId: userId, companyId: companyId, description: "Added checklist template stage template.");
            }

            return possibleId;
        }

        /// <summary>
        /// Change a stage template.
        /// Calls database function 'change_checklisttemplate_stage'.
        /// </summary>
        /// <param name="userId">Id of current user</param>
        /// <param name="companyId">Company id</param>
        /// <param name="stageTemplate">Stage template will be updated to given StageTemplate object</param>
        /// <returns>Rowcount of affected rows</returns>
        private async Task<int> ChangeStageTemplateAsync(int userId, int companyId, StageTemplate stageTemplate)
        {
            if (stageTemplate.Id <= 0) return stageTemplate.Id;

            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage.ToString(), stageTemplate.Id);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_id", stageTemplate.Id),
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_name", stageTemplate.Name),
                new NpgsqlParameter("@_description", string.IsNullOrEmpty(stageTemplate.Description) ? DBNull.Value : stageTemplate.Description),
                new NpgsqlParameter("@_block_next_stages_until_completion", stageTemplate.BlockNextStagesUntilCompletion),
                new NpgsqlParameter("@_lock_stage_after_completion", stageTemplate.LockStageAfterCompletion),
                new NpgsqlParameter("@_use_shift_notes", stageTemplate.UseShiftNotes),
                new NpgsqlParameter("@_number_of_signatures", stageTemplate.NumberOfSignaturesRequired),
                new NpgsqlParameter("@_index", stageTemplate.Index),
                new NpgsqlParameter("@_userid", userId)
            };

            int rowcount = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_checklisttemplate_stage", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            stageTemplate.Tags ??= new();
            await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.ChecklistTemplateStage, id: stageTemplate.Id, tags: stageTemplate.Tags, companyId: companyId, userId: userId);

            if (rowcount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage.ToString(), stageTemplate.Id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_stage.ToString(), objectId: stageTemplate.Id, userId: userId, companyId: companyId, description: "Changed checklist template stage template.");
            }

            return rowcount;
        }

        /// <summary>
        /// Set the active state of target stage template.
        /// Calls database function 'set_checklisttemplate_stage_active'.
        /// </summary>
        /// <param name="userId">Id of current user</param>
        /// <param name="companyId">Company id</param>
        /// <param name="checklistStageTemplateId">Id of the stage template to set the active state for</param>
        /// <param name="isActive">False to deactivate, true to reactivate. Defaults to true.</param>
        /// <returns>True if successful</returns>
        private async Task<bool> SetStageTemplateActiveAsync(int userId, int companyId, int checklistStageTemplateId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage.ToString(), checklistStageTemplateId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId),
                new NpgsqlParameter("@_id", checklistStageTemplateId),
                new NpgsqlParameter("@_active", isActive)
            };

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_checklisttemplate_stage_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), checklistStageTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_stage.ToString(), objectId: checklistStageTemplateId, userId: userId, companyId: companyId, description: "Changed checklist template stage active state.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// Upserts relations between stage template and task templates.
        /// Calls database function 'upsert_checklisttemplate_stage_task_relations'.
        /// </summary>
        /// <param name="userId">Id of current user</param>
        /// <param name="companyId">Company id</param>
        /// <param name="stageTemplateId">Id of stage template</param>
        /// <param name="taskTemplateIds">Ids of task templates to link to the given stage template</param>
        /// <returns>Row count of affected rows</returns>
        private async Task<int> UpsertStageTemplateTaskTemplateRelations(int userId, int companyId, int stageTemplateId, List<int> taskTemplateIds)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage_tasks.ToString(), Models.Enumerations.TableFields.checklisttemplate_stage_id.ToString(), stageTemplateId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_checklisttemplatestageid", stageTemplateId),
                new NpgsqlParameter("@_tasktemplateids", taskTemplateIds)
            };

            var rowCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("upsert_checklisttemplate_stage_task_relations", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage_tasks.ToString(), Models.Enumerations.TableFields.checklisttemplate_stage_id.ToString(), stageTemplateId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_stage_tasks.ToString(), objectId: stageTemplateId, userId: userId, companyId: companyId, description: "Changed checklist template stage task template relation collection.");
            }

            return rowCount;
        }

        private async Task<int> RemoveStageTemplateTaskTemplateRelation(int id, int taskTemplateId, int checklistTemplateStageId, int userId, int companyId)
        {
            //remove_checklisttemplate_stage_task_relation(_id integer, _tasktemplateid integer, _checklisttemplatestageid integer)

            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage_tasks.ToString(), Models.Enumerations.TableFields.checklisttemplate_stage_id.ToString(), checklistTemplateStageId);

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_id", id),
                new NpgsqlParameter("@_tasktemplateid", taskTemplateId),
                new NpgsqlParameter("@_checklisttemplatestageid", checklistTemplateStageId)
            };

            var rowCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_checklisttemplate_stage_task_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowCount > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklisttemplate_stage_tasks.ToString(), Models.Enumerations.TableFields.checklisttemplate_stage_id.ToString(), checklistTemplateStageId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklisttemplate_stage_tasks.ToString(), objectId: checklistTemplateStageId, userId: userId, companyId: companyId, description: "Removed checklist template stage task template relation.");
            }

            return rowCount;
        }


        private async Task<int> AddStageTaskTemplateRelationAsync(int stageTemplateId, int taskTemplateId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_checklisttemplatestageid", stageTemplateId),
                new NpgsqlParameter("@_tasktemplateid", taskTemplateId)
            };

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklisttemplate_stage_task_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return possibleId;
        }

        
        private async Task<bool> ChangeStageTaskTemplateRelationAsync(int stageTemplateId, int taskTemplateId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_checklisttemplatestageid", stageTemplateId),
                new NpgsqlParameter("@_tasktemplateid", taskTemplateId)
            };

            var rowCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_checklisttemplate_stage_task_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowCount > 0;
        }

        
        private async Task<bool> RemoveStageTaskTemplateRelationAsync(int stageTemplateId, int taskTemplateId)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_checklisttemplatestageid", stageTemplateId),
                new NpgsqlParameter("@_tasktemplateid", taskTemplateId)
            };

            var rowCount = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_checklisttemplate_stage_task_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return rowCount > 0;
        }

        /// <summary>
        /// Set TaskTemplateIds for the stage template to all task template ids between current stage index and previous stage index of the checklist
        /// </summary>
        /// <param name="stageTemplate"></param>
        /// <param name="checklistTemplate"></param>
        private void SetStageTemplateTaskIds(StageTemplate stageTemplate, ChecklistTemplate checklistTemplate)
        {
            var previousStages = checklistTemplate.StageTemplates.FindAll(s => s.Index < stageTemplate.Index);
            var previousStageIndex = previousStages.Any() ? previousStages.Max(s => s.Index) : 0;
            if (checklistTemplate.TaskTemplates != null && checklistTemplate.TaskTemplates.Count > 0)
            {
                stageTemplate.TaskTemplateIds = checklistTemplate.TaskTemplates.Where(t => t.Index > previousStageIndex && t.Index < stageTemplate.Index).Select(t => t.Id).ToList();
            }
        }
        #endregion

        #region - private methods Checklists -
        /// <summary>
        /// Adds relation between completed checklist and the task it is linked to.
        /// </summary>
        /// <param name="taskId">task id</param>
        /// <param name="checklistId">audit id</param>
        /// <param name="isRequired">set to true if the linked checklist is mandatory before completing the task</param>
        /// <returns>id of the linking record</returns>
        private async Task<int> AddTaskChecklistLinkAsync(int companyId, int userId, long taskId, int checklistId, bool isRequired)
        {
            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_task_id", taskId),
                new NpgsqlParameter("@_checklist_id", checklistId),
                new NpgsqlParameter("@_is_required", isRequired)
            };

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_task_checklist_link", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.tasks_task_checklist_link.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.tasks_task_checklist_link.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added task checklist link.");
            }

            return possibleId;
        }

        /// <summary>
        /// GetTasksWithChecklist; Gets a list of Tasks based on the ChecklistId.
        /// These Tasks are part of the filled in Checklists by a user.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. Used for including extra data. </param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>A List of Tasks (TasksTask objects)</returns>
        private async Task<List<TasksTask>> GetTasksWithChecklist(int companyId, int checklistId, int? checklistTemplateId = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = await _taskManager.GetTasksByChecklistIdAsync(companyId: companyId, checklistId: checklistId, checklistTemplateId: checklistTemplateId, include: include, connectionKind: connectionKind);
            if (output != null && output.Count > 0)
            {
                return output;
            }
            return null;
        }

        /// <summary>
        /// AppendChecklistTasksAndStagesAsync; Append tasks and stages to checklist.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklist">Already retrieved.</param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. Used for including extra data. </param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <param name="filters">Filters containing filters for execution within child functionality.</param>
        /// <param name="userId">Id of current user</param>
        /// <param name="connectionKind">ConnectionKind (writer or reader)</param>
        /// <returns></returns>
        private async Task<Checklist> AppendChecklistTasksAndStagesAsync(int companyId, Checklist checklist, ChecklistFilters? filters = null, int? userId = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            checklist.Tasks = await _taskManager.GetTasksByChecklistIdAsync(companyId: companyId, checklistId: checklist.Id, checklistTemplateId: checklist.TemplateId, include: include, connectionKind: connectionKind);

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_CHECKLIST_STAGES"))
                checklist.Stages = await GetStagesByChecklistIdAsync(companyId: companyId, checklistId: checklist.Id, include: include, connectionKind: connectionKind);

            return checklist;
        }

        private async Task<List<Checklist>> AppendChecklistsTasksAndStagesAsync(int companyId, List<Checklist> checklists, ChecklistFilters? filters = null, int? userId = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            checklists = await AppendChecklistsTasksAsync(companyId: companyId, checklists: checklists, include: include, connectionKind: connectionKind);

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_CHECKLIST_STAGES"))
                checklists = await AppendChecklistsStagesAsync(companyId: companyId, checklists: checklists, include: include, connectionKind: connectionKind);

            return checklists;
        }

        /// <summary>
        /// AppendChecklistTasksAsync; Append tasks to checklist objects.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklists">Collection of checklists.</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters used for further retrieval of data, will be converted to TaskFilters</param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. Used for including extra data. </param>
        /// <returns>The list of checklists, appended with Tasks.</returns>
        private async Task<List<Checklist>> AppendChecklistsTasksAsync(int companyId, List<Checklist> checklists, ChecklistFilters? filters = null, int? userId = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var taskFilters = filters.ToTaskFilters();
            var tasks = await _taskManager.GetTasksWithChecklistsAsync(companyId: companyId, checklistIds: checklists.Select(c => c.Id).ToList(), checklistTemplateIds: checklists.Select(c => c.TemplateId).Distinct().ToList(), userId: userId, filters: taskFilters, include: include, connectionKind: connectionKind);
            if (tasks != null && tasks.Count > 0)
            {
                foreach (var checklist in checklists)
                {
                    checklist.Tasks = tasks.Where(x => x.ChecklistId.HasValue && x.ChecklistId == checklist.Id).ToList();
                }
            }

            return checklists;
        }

        /// <summary>
        /// AppendChecklistStagesAsync; Append tasks to checklist objects.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklists">Collection of checklists.</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters used for further retrieval of data, will be converted to TaskFilters</param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. Used for including extra data. </param>
        /// <returns>The list of checklists, appended with Tasks.</returns>
        private async Task<List<Checklist>> AppendChecklistsStagesAsync(int companyId, List<Checklist> checklists, ChecklistFilters? filters = null, int? userId = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var stages = await GetStagesByChecklistIdsAsync(companyId: companyId, checklistIds: checklists.Select(c => c.Id).ToList(), include: include, connectionKind: connectionKind);
            if (stages != null && stages.Count > 0)
            {
                foreach (var checklist in checklists)
                {
                    checklist.Stages = stages.Where(x => x.ChecklistId == checklist.Id).ToList();
                }
            }

            return checklists;
        }

        /// <summary>
        /// AppendAreaPathsToChecklistsAsync; Add the AreaPath to the Checklist. (used for CMS purposes);
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklists">List of checklists.</param>
        /// <param name="addAreaPath">Add area paths to the output objects.</param>
        /// <param name="addAreaPathIds">Add area paths ids to the output objects.</param>
        /// <returns>Checklists including area full path. </returns>
        private async Task<List<Checklist>> AppendAreaPathsToChecklistsAsync(int companyId, List<Checklist> checklists, bool addAreaPath = true, bool addAreaPathIds = false)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var checklist in checklists)
                {
                    var area = areas?.Where(x => x.Id == checklist.AreaId)?.FirstOrDefault();
                    if (area != null)
                    {
                        if (addAreaPath) checklist.AreaPath = area.FullDisplayName;
                        if (addAreaPathIds) checklist.AreaPathIds = area.FullDisplayIds;
                    }

                }
            }
            return checklists;
        }

        /// <summary>
        /// CreateOrFillChecklistFromReader; creates and fills a Checklist object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="checklist">Checklist object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled Checklist object.</returns>
        private Checklist CreateOrFillChecklistFromReader(NpgsqlDataReader dr, Checklist checklist = null)
        {
            if (checklist == null) checklist = new Checklist();

            checklist.Id = Convert.ToInt32(dr["id"]);
            checklist.AreaId = Convert.ToInt32(dr["area_id"]);
            checklist.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                checklist.Description = dr["description"].ToString();
            }
            checklist.IsCompleted = Convert.ToBoolean(dr["is_complete"]);
            checklist.IsDoubleSignatureRequired = Convert.ToBoolean(dr["double_signature_required"]);
            if (dr["signed_at_1"] != DBNull.Value && dr["signed_by_1_id"] != DBNull.Value)
            {
                if (checklist.Signatures == null) checklist.Signatures = new List<Signature>();
                checklist.Signatures.Add(new Signature() { SignatureImage = dr["signature_1"].ToString(), SignedAt = Convert.ToDateTime(dr["signed_at_1"]), SignedById = Convert.ToInt32(dr["signed_by_1_id"]), SignedBy = dr["signed_by_1"].ToString() });

                if(!dr.HasColumn("created_by_id") || (dr.HasColumn("created_by_id") && dr["created_by_id"] == DBNull.Value))
                {
                    checklist.CreatedById = Convert.ToInt32(dr["signed_by_1_id"]);
                    checklist.CreatedBy = dr["signed_by_1"].ToString();
                }
            }
            if (dr["signed_at_2"] != DBNull.Value && dr["signed_by_2"] != DBNull.Value)
            {
                if (checklist.Signatures == null) checklist.Signatures = new List<Signature>();
                checklist.Signatures.Add(new Signature() { SignatureImage = dr["signature_2"].ToString(), SignedAt = Convert.ToDateTime(dr["signed_at_2"]), SignedById = Convert.ToInt32(dr["signed_by_2_id"]), SignedBy = dr["signed_by_2"].ToString() });
            }
            checklist.Name = dr["name"].ToString();
            if (dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString()))
            {
                checklist.Picture = dr["picture"].ToString();
            }
            checklist.TemplateId = Convert.ToInt32(dr["template_id"]);
            if (dr["created_at"] != DBNull.Value)
            {
                checklist.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                checklist.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            if (dr.HasColumn("created_by_id"))
            {
                if (dr["created_by_id"] != DBNull.Value)
                {
                    checklist.CreatedById = Convert.ToInt32(dr["created_by_id"]);
                }
            }

            if (dr.HasColumn("created_by") && !string.IsNullOrEmpty(dr["created_by"].ToString().Trim()))
            {
                if (dr["created_by"] != DBNull.Value)
                {
                    checklist.CreatedBy = dr["created_by"].ToString();
                }
            }

            if (dr.HasColumn("modified_by_id"))
            {
                if (dr["modified_by_id"] != DBNull.Value)
                {
                    checklist.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);
                }
            }

            if (dr.HasColumn("linked_task_id"))
            {
                if (dr["linked_task_id"] != DBNull.Value)
                {
                    checklist.LinkedTaskId = Convert.ToInt32(dr["linked_task_id"]);
                }
            }

            if (dr.HasColumn("modified_by") && !string.IsNullOrEmpty(dr["modified_by"].ToString().Trim()))
            {
                if (dr["modified_by"] != DBNull.Value)
                {
                    checklist.ModifiedBy = dr["modified_by"].ToString();
                }
            }

            if (dr.HasColumn("signature_required"))
            {
                if (dr["signature_required"] != DBNull.Value)
                {
                    checklist.IsSignatureRequired = Convert.ToBoolean(dr["signature_required"]);
                }
            }
            else
            {
                checklist.IsSignatureRequired = true; //TODO: Default to true, change when db updates are done in a later release.
            }

            if (dr.HasColumn("version") && dr["version"] != DBNull.Value)
            {
                checklist.Version = dr["version"].ToString();
            }

            return checklist;
        }

        /// <summary>
        /// CreateOrFillChecklistFromReader; creates and fills a Checklist object from a DataReader based on static data store.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="checklist">Checklist object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled Checklist object.</returns>
        private Checklist CreateOrFillStaticChecklistFromReader(NpgsqlDataReader dr, Checklist checklist = null)
        {
            if (checklist == null) checklist = new Checklist();

            if (dr["data_object"] != DBNull.Value)
            {
                checklist = dr["data_object"].ToString().ToObjectFromJson<Checklist>();
            }

            return checklist;
        }

        /// <summary>
        /// GetNpgsqlParametersFromChecklist; Creates a list of NpgsqlParameters, and fills it based on the supplied Checklist object.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="checklist">The supplied Checklist object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromChecklist(Checklist checklist, int companyId, int userId, int checklistId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (checklistId > 0) parameters.Add(new NpgsqlParameter("@_id", checklistId));

            parameters.Add(new NpgsqlParameter("@_templateid", checklist.TemplateId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_iscompleted", checklist.IsCompleted));

            var signature = checklist.Signatures != null && checklist.Signatures.Count > 0 ? checklist.Signatures[0] : new Signature(); //get first signature
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

            var signature2 = checklist.Signatures != null && checklist.Signatures.Count > 1 ? checklist.Signatures[1] : new Signature(); //get first signature

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

            if (userId > 0)
            {
                parameters.Add(new NpgsqlParameter("@_userid", userId));
            }

            return parameters;
        }


        #endregion

        #region - private methods -
        /// <summary>
        /// AddTaskTemplatesActionsWithTaskTemplates; Adds actions (if available) to a Task Within a taskTemplate Collection.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskTemplates">List of TaskTemplates where possible actions need to be added.</param>
        /// <returns>A list of TaskTemplates (based on the input) with possible actions added.</returns>
        private async Task<List<TaskTemplate>> AddTaskTemplatesActionsWithTaskTemplates(int companyId, List<TaskTemplate> taskTemplates)
        {
            if (taskTemplates != null && taskTemplates.Count > 0)
            {
                var actions = await _actionManager.GetActionsAsync(companyId: companyId, filters: new ActionFilters() { IsResolved = false });

                if (actions.Count > 0)
                {
                    foreach (var taskTemplate in taskTemplates)
                    {
                        if (taskTemplate.ActionsCount.HasValue && taskTemplate.ActionsCount > 0)
                        {
                            //TODO add logic
                        }
                    }
                }
            }
            await Task.CompletedTask;
            return taskTemplates;
        }
        #endregion

        #region - template versioning -
        private async Task<List<Checklist>> ApplyTemplateVersionToChecklists(List<Checklist> checklists, int companyId, string include = null)
        {
            //cache versioned templates based on template id and version
            Dictionary<KeyValuePair<int, string>, ChecklistTemplate> VersionedChecklistsCache = new();
            foreach (Checklist checklist in checklists)
            {
                if (!string.IsNullOrEmpty(checklist.Version) && checklist.Version != await _flattenedChecklistManager.RetrieveLatestAvailableVersion(checklist.TemplateId, companyId))
                {
                    ChecklistTemplate versionedTemplate = null;
                    KeyValuePair<int, string> TemplateIdVersionPair = new(checklist.TemplateId, checklist.Version);

                    if (VersionedChecklistsCache.ContainsKey(TemplateIdVersionPair))
                    {
                        //get correct version of template from cache if it is already present
                        versionedTemplate = VersionedChecklistsCache.GetValueOrDefault(TemplateIdVersionPair);
                    }
                    else
                    {
                        //retrieve the correct version of the template from the database and add it to the cache
                        versionedTemplate = await _flattenedChecklistManager.RetrieveFlattenData(templateId: checklist.TemplateId, companyId: companyId, version: checklist.Version);
                        VersionedChecklistsCache.Add(TemplateIdVersionPair, versionedTemplate);
                    }

                    if (versionedTemplate != null)
                        checklist.ApplyTemplateVersion(versionedTemplate, include);
                    else
                        _logger.LogWarning($"ApplyTemplateVersionToChecklists(); Template version not applied because requested version wasn't found. ChecklistTemplateId: {checklist.TemplateId}, version: {checklist.Version}");
                }
            }
            return checklists;
        }

        private async Task<Checklist> ApplyTemplateVersionToChecklist(Checklist checklist, int companyId, string include = null)
        {
            //fill all template related data from the retrieved versioned template
            ChecklistTemplate versionedTemplate = await _flattenedChecklistManager.RetrieveFlattenData(templateId: checklist.TemplateId, companyId: companyId, version: checklist.Version);
            if (versionedTemplate != null)
                checklist.ApplyTemplateVersion(versionedTemplate, include);
            else
                _logger.LogWarning($"ApplyTemplateVersionToChecklist; Template version not applied because requested version wasn't found. ChecklistTemplateId: {checklist.TemplateId}, version: {checklist.Version}");
            return checklist;
        }
        #endregion

        #region - properties -

        //NOTE METHODS WILL BE MOVED WHEN FULLY IMPLEMENTED

        /// <summary>
        /// AppendPropertiesToChecklists; Append properties to checklists (e.g. open fields)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklists">List of checklist items where the open fields need to be added.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. Used for including extra data. </param>
        /// <returns>List of updated checklists which includes. </returns>
        private async Task<List<Checklist>> AppendPropertiesToChecklists(int companyId, List<Checklist> checklists, string include = "")
        {
            List<int> checklistIds = null;
            List<int> checklistTemplateIds = null;
            if (checklists != null)
            {
                checklistIds = checklists.Select(checklist => checklist.Id).ToList();
                checklistTemplateIds = checklists.Select(checklist => checklist.TemplateId).Distinct().ToList();
            }


            var propertyUserValues = await _propertyValueManager.GetPropertyUserValuesWithChecklists(companyId: companyId, checklistIds: checklistIds);
            var properties = await _propertyValueManager.GetPropertiesChecklistTemplatesAsync(companyId: companyId, checklistTemplateIds: checklistTemplateIds);

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
            {
                foreach (var checklist in checklists)
                {
                    checklist.OpenFieldsProperties = properties.Where(x => x.ChecklistTemplateId == checklist.TemplateId && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                    checklist.OpenFieldsPropertyUserValues = propertyUserValues.Where(x => x.ChecklistId == checklist.Id).ToList();
                }
            }

            return checklists;
        }

        /// <summary>
        /// AppendPropertiesToChecklist; Append properties to specific checklist (e.g. open fields)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklist">Checklist where properties need to be added.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>Updated checklist with added properties.</returns>
        private async Task<Checklist> AppendPropertiesToChecklist(int companyId, Checklist checklist, string include = "", ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<int> checklistIds = new()
            {
                checklist.Id
            };
            List<int> checklistTemplateIds = new()
            {
                checklist.TemplateId
            };

            var propertyUserValues = await _propertyValueManager.GetPropertyUserValuesWithChecklists(companyId: companyId, checklistIds: checklistIds, connectionKind: connectionKind);
            var properties = await _propertyValueManager.GetPropertiesChecklistTemplatesAsync(companyId: companyId, checklistTemplateIds: checklistTemplateIds);

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
            {
                checklist.OpenFieldsProperties = properties.Where(x => x.ChecklistTemplateId == checklist.TemplateId && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                checklist.OpenFieldsPropertyUserValues = propertyUserValues.Where(x => x.ChecklistId == checklist.Id).ToList();
            }

            return checklist;
        }

        /// <summary>
        /// AppendTemplatePropertiesToTaskTemplates; Append template properties to the task templates of a checklist template.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplates">List of checklist templates, where template properties need to be added.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>Updated list of Checklist templates.</returns>
        private async Task<List<ChecklistTemplate>> AppendTemplatePropertiesToTaskTemplates(int companyId, List<ChecklistTemplate> checklisttemplates, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            List<int> taskTemplateIds = checklisttemplates.Where(checklistTemplate => checklistTemplate.TaskTemplates != null).SelectMany(checklistTemplate => checklistTemplate.TaskTemplates.Select(taskTemplate => taskTemplate.Id)).ToList();
            var properties = await _propertyValueManager.GetPropertiesTaskTemplatesAsync(companyId: companyId, taskTemplateIds: taskTemplateIds, include: include, connectionKind: connectionKind);
            foreach (var checklist in checklisttemplates)
            {
                if (checklist.TaskTemplates != null)
                {
                    foreach (var checklistitem in checklist.TaskTemplates)
                    {
                        if (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))
                        {
                            checklistitem.Properties = properties.Where(x => x.TaskTemplateId == checklistitem.Id && x.IsActive && PropertySettings.BasicAndSpecificProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                        }

                        foreach (var item in checklistitem.Properties)
                        {
                            item.TaskTemplateId = checklistitem.Id;
                        }
                    }
                }
            }

            return checklisttemplates;
        }

        /// <summary>
        /// AppendTemplatePropertiesToTaskTemplates; Append template properties to task templates of a checklisttemplates. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplate">Checklisttemplate where properties need to be added.</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum.</param>
        /// <returns>Updated checklist template with task templates with properties.</returns>
        private async Task<ChecklistTemplate> AppendTemplatePropertiesToTaskTemplates(int companyId, ChecklistTemplate checklisttemplate, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {

            if (checklisttemplate.TaskTemplates != null && checklisttemplate.TaskTemplates.Count > 0)
            {
                List<int> taskTemplateIds = checklisttemplate.TaskTemplates.Select(taskTemplate => taskTemplate.Id).ToList();
                var properties = await _propertyValueManager.GetPropertiesTaskTemplatesAsync(companyId: companyId, taskTemplateIds: taskTemplateIds, include: include, connectionKind: connectionKind);

                foreach (var checklistitem in checklisttemplate.TaskTemplates)
                {
                    if (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower()))
                    {
                        checklistitem.Properties = properties.Where(x => x.TaskTemplateId == checklistitem.Id && x.IsActive && PropertySettings.BasicAndSpecificProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                    }

                    checklistitem.Properties.ForEach(x => x.TaskTemplateId = checklistitem.Id);

                    if (include.Split(",").Contains(IncludesEnum.PropertiesGen4.ToString().ToLower()))
                    {
                        checklistitem.PropertiesGen4 = checklistitem.Properties.ToPropertyDTOList();
                        checklistitem.Properties = null;
                    }
                }
            }

            return checklisttemplate;
        }

        /// <summary>
        /// AppendTemplatePropertiesToTemplates; Append properties to list of checklisttemplates;
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplates">Checklisttemplates collection.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>Updated list of checklist templates.</returns>
        private async Task<List<ChecklistTemplate>> AppendTemplatePropertiesToTemplates(int companyId, List<ChecklistTemplate> checklisttemplates, string include = null)
        {
            List<int> checklistTemplateIds = checklisttemplates.Select(checklistTemplate => checklistTemplate.Id).ToList();
            var properties = await _propertyValueManager.GetPropertiesChecklistTemplatesAsync(companyId: companyId, checklistTemplateIds: checklistTemplateIds, include: include);
            if (checklisttemplates != null && properties != null && properties.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower())) {
                    foreach (var checklisttemplate in checklisttemplates)
                    {
                        checklisttemplate.OpenFieldsProperties = properties.Where(x => x.ChecklistTemplateId == checklisttemplate.Id && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();
                        foreach (var item in checklisttemplate.OpenFieldsProperties)
                        {
                            item.ChecklistTemplateId = checklisttemplate.Id;
                        }

                    }
                }
            }

            return checklisttemplates;
        }

        /// <summary>
        /// AppendTemplatePropertiesToTemplate; Add template properties to checklist template
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklisttemplate">Checklist template where properties need to be added.</param>
        /// <param name="include">Include parameter can be used for including extra data, based on includes enum.</param>
        /// <returns>Checklist template with properties.</returns>
        private async Task<ChecklistTemplate> AppendTemplatePropertiesToTemplate(int companyId, ChecklistTemplate checklisttemplate, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            //TODO make more efficient
            var properties = await _propertyValueManager.GetPropertiesChecklistTemplateAsync(companyId: companyId, checklistTemplateId: checklisttemplate.Id, include: include, connectionKind: connectionKind);

            if (properties != null && properties.Count > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
                {
                    checklisttemplate.OpenFieldsProperties = properties.Where(x => x.ChecklistTemplateId == checklisttemplate.Id && PropertySettings.OpenFieldProperties.ToList().Contains(x.PropertyGroupId.Value)).OrderBy(y => y.Index)?.ToList();

                    checklisttemplate.OpenFieldsProperties.ForEach(x => x.ChecklistTemplateId = checklisttemplate.Id);

                    if (include.Split(",").Contains(IncludesEnum.PropertiesGen4.ToString().ToLower()))
                    {
                        checklisttemplate.OpenFieldsPropertiesGen4 = checklisttemplate.OpenFieldsProperties.ToPropertyDTOList();
                        checklisttemplate.OpenFieldsProperties = null;
                    }

                }
            }

            return checklisttemplate;
        }

        /// <summary>
        /// Converts the Properties and PropertyUserValues fields to the new PropertiesGen4 field
        /// The Properties and PropertyUserValues fields will be removed at the end of the conversion
        /// </summary>
        /// <param name="checklist">The checklist to convert the properties for</param>
        /// <returns>Updated Checklist</returns>
        private async Task<Checklist> ReplacePropertiesWithPropertiesGen4(Checklist checklist, int companyId)
        {
            //List of users is only needed when there are property user values
            List<UserBasic> users = null;
            if (checklist.PropertyUserValues != null || checklist.OpenFieldsPropertyUserValues != null)
            {
                users = await _userManager.GetUsersBasicAsync(companyId: companyId);
            }

            if (checklist.Properties != null && checklist.Properties.Any())
            {
                checklist.PropertiesGen4 = checklist.Properties.ToPropertyDTOList(propertyUserValues: checklist.PropertyUserValues, userList: users);
                checklist.Properties = null;
                checklist.PropertyUserValues = null;
            }
            if (checklist.OpenFieldsProperties != null && checklist.OpenFieldsProperties.Any())
            {
                checklist.OpenFieldsPropertiesGen4 = checklist.OpenFieldsProperties.ToPropertyDTOList(propertyUserValues: checklist.OpenFieldsPropertyUserValues, userList: users);
                checklist.OpenFieldsProperties = null;
                checklist.OpenFieldsPropertyUserValues = null;
            }
            return checklist;
        }

        #endregion

        #region - private methods updates / changes to checklists -
        /// <summary>
        /// ChangeChecklistAddOrChangeTask; Add or change tasks with a checklist
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <param name="tasks">Collection of tasks that needs to be added or updated.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<List<TasksTask>> ChangeChecklistAddOrChangeTask(int companyId, int userId, int possibleOwnerId, int checklistId,  List<TasksTask> tasks)
        {
            var output = new List<TasksTask>();
            if(checklistId > 0)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist_tasks.ToString(), Models.Enumerations.TableFields.checklist_id.ToString(), checklistId);

                if (checklistId > 0 && tasks != null)
                {
                    foreach (var task in tasks)
                    {
                        var updatedTask = await AddChangeTask(companyId: companyId, userId: userId, possibleOwnerId: possibleOwnerId, checklistId: checklistId, task: task);

                        output.Add(updatedTask);
                    }
                }

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist_tasks.ToString(), Models.Enumerations.TableFields.checklist_id.ToString(), checklistId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist_tasks.ToString(), objectId: checklistId, userId: userId, companyId: companyId, description: "Changed checklist task relation collection.");
            }

            return output;
        }

        /// <summary>
        /// AddChangeTask; Add/Change specific task with a checklist 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <param name="task">Task object that needs to be updated or added.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<TasksTask> AddChangeTask(int companyId, int userId, int checklistId, int possibleOwnerId, TasksTask task)
        {
            TasksTask output = null;
            if (checklistId > 0 && task != null)
            {
                if (task.Id > 0)
                {
                    task.ChecklistId = checklistId;
                    var result = await _taskManager.ChangeTaskAsync(companyId: companyId, userId: userId, possibleOwnerId: possibleOwnerId, taskId: Convert.ToInt32(task.Id), task: task);
                    output = task;
                }
                else
                {

                    task.ChecklistId = checklistId;
                    var resultId = await _taskManager.AddTaskAsync(companyId: companyId, userId: userId, possibleOwnerId: possibleOwnerId, task: task);

                    task.Id = resultId;

                    if (resultId > 0)
                    {
                        var result = await AddTaskChecklistRelation(companyId: companyId, checklistId: task.ChecklistId.Value, taskId: resultId);
                        output = task;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// AddTaskChecklistRelation; Add Task Checklist relation. (connection table between tasks and checklists)
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="checklistId">ChecklistId (DB: checklists_checklist.id)</param>
        /// <param name="taskId">TaskId (DB: tasks_task.id)</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> AddTaskChecklistRelation(int companyId, int checklistId, int taskId)
        {
           
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_checklistid", checklistId));
            parameters.Add(new NpgsqlParameter("@_taskid", taskId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklist_task_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);
        }

        private async Task<List<Stage>> AddChecklistStages(int companyId, int checklistId, int userId, List<Stage> stages)
        {
            var output = new List<Stage>();

            foreach (var stage in stages)
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_checklistid", checklistId));
                
                if (stage.Signatures != null && stage.Signatures.Count > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_signatures", stage.Signatures.ToJsonFromObject().ToString()));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_signatures", DBNull.Value));
                }

                parameters.Add(new NpgsqlParameter("@_status", stage.Status));

                if (!string.IsNullOrEmpty(stage.ShiftNotes))
                {
                    parameters.Add(new NpgsqlParameter("@_shiftnotes", stage.ShiftNotes));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_shiftnotes", DBNull.Value));
                }

                parameters.Add(new NpgsqlParameter("@_stagetemplateid", stage.StageTemplateId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklist_stage", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                if (id > 0)
                {
                    //copy stage template tags to stage tags
                    stage.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, ObjectTypeEnum.ChecklistTemplateStage, id: stage.StageTemplateId);
                    await _tagManager.UpdateTagsOnObjectAsync(objectType: ObjectTypeEnum.ChecklistStage, id: id, tags: stage.Tags, companyId: companyId, userId: userId);

                    stage.Id = id;
                    output.Add(stage);

                    var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist_stage.ToString(), stage.Id);

                    await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist_stage.ToString(), objectId: stage.Id, userId: userId, companyId: companyId, description: "Added checklist stage.");
                }
            }

            return output;
        }

        private async Task<bool> ChangeChecklistStages(int companyId, int checklistId, int userId, List<Stage> stages)
        {
            var rowsaffected = 0;

            foreach (var stage in stages)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist_stage.ToString(), stage.Id);

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_id", stage.Id));

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (stage.Signatures != null && stage.Signatures.Count > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_signatures", stage.Signatures.ToJsonFromObject().ToString()));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_signatures", DBNull.Value));
                }

                parameters.Add(new NpgsqlParameter("@_status", stage.Status));

                if (!string.IsNullOrEmpty(stage.ShiftNotes))
                {
                    parameters.Add(new NpgsqlParameter("@_shift_notes", stage.ShiftNotes));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_shift_notes", DBNull.Value));
                }

                parameters.Add(new NpgsqlParameter("@_userid", userId));

                var currentRowsAffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_checklist_stage", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                rowsaffected += currentRowsAffected;

                //NOTE: Intentionally not updating tags. They should not be changable if the stage changes. 

                if (currentRowsAffected > 0)
                {
                    var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist_stage.ToString(), stage.Id);

                    await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist_stage.ToString(), objectId: stage.Id, userId: userId, companyId: companyId, description: "Changed checklist stage.");
                }
            }


            return (rowsaffected > 0);
        }

        private async Task<bool> AddTasksStagesRelations(int userId, int companyId, List<Stage> stages, List<TasksTask> tasks)
        {
            var rowsaffected = 0;

            foreach (var stage in stages)
            {
                //Stage TasksIds preprocessing based on TaskTemplateIds
                if (stage.TaskTemplateIds != null && stage.TaskTemplateIds.Count > 0 && tasks != null && tasks.Count > 0)
                {
                    if (stage.TaskIds == null)
                    {
                        stage.TaskIds = new List<int>();
                    }

                    foreach (var taskTemplateId in stage.TaskTemplateIds)
                    {
                        var task = tasks.Where(t => t.TemplateId == taskTemplateId).FirstOrDefault();
                        if (task != null && task.Id > 0)
                        {
                            stage.TaskIds.Add((int)task.Id);
                        }
                    }
                }

                if(stage.TaskIds != null && stage.TaskIds.Count > 0 && stage.Id > 0)
                {
                    foreach (var taskId in stage.TaskIds)
                    {
                        List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                        parameters.Add(new NpgsqlParameter("@_checkliststageid", stage.Id));
                        parameters.Add(new NpgsqlParameter("@_taskid", taskId));

                        var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_checklist_stage_task_relation", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                        if (id > 0)
                        {
                            rowsaffected += 1;

                            var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.checklists_checklist_stage_tasks.ToString(), id);

                            await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.checklists_checklist_stage_tasks.ToString(), objectId: stage.Id, userId: userId, companyId: companyId, description: "Added checklist task stage relation.");
                        }
                    }
                }
            }


            return (rowsaffected > 0);
        }

        #endregion

        #region - template properties -
        /// <summary>
        /// AddChangeTemplatePropertiesAsync; Add or change properties with a tasktemplate
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateId">TemplateId of template where proeprties need to be added.</param>
        /// <param name="templateProperties">List of template properties to be added/changed.</param>
        /// <returns>Number of updates. (not implemented)</returns>
        public async Task<int> AddChangeTemplatePropertiesAsync(int companyId, int userId, int templateId, List<PropertyChecklistTemplate> templateProperties)
        {
            //TODO add output nr (total of mutations)
            if (templateProperties != null)
            {
                //Get all current properties
                var currentProperties = await _propertyValueManager.GetPropertiesChecklistTemplateAsync(companyId: companyId, checklistTemplateId: templateId);
                var propIds = templateProperties.Select(x => x.Id).ToList(); //Get all new ids that are coming from templateProperties collection.

                if (currentProperties != null && currentProperties.Count > 0)
                {
                    foreach (var prop in currentProperties.Where(x => x.Id > 0 && !propIds.Contains(x.Id)))
                    {
                        //check all properties against the supplied ids, if not in the supplied ids, start removing them. (set to inactive)
                        await _propertyValueManager.RemoveChecklistTemplatePropertyAsync(companyId: companyId, userId: userId, checklistTemplatePropertyId: prop.Id);
                    }
                }

                //Add or Change all properties that are supplied.
                foreach (PropertyChecklistTemplate checklistTemplateProperty in templateProperties)
                {
                    if (checklistTemplateProperty.Id > 0)
                    {
                        await ChangeChecklistTemplatePropertyAsync(companyId: companyId, userId: userId, checklistTemplatePropertyId: checklistTemplateProperty.Id, templateproperty: checklistTemplateProperty);
                    }
                    else
                    {
                        if (checklistTemplateProperty.ChecklistTemplateId <= 0) checklistTemplateProperty.ChecklistTemplateId = templateId; //make sure to add the templateid if not supplied with the property
                        await AddChecklistTemplatePropertyAsync(companyId: companyId, userId: userId, templateproperty: checklistTemplateProperty);
                    }
                }

            }

            return 0;
        }

        /// <summary>
        /// AddTaskTemplatePropertyAsync; Add checklist template property.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="templateproperty">Template property to be added/changed.</param>
        /// <returns>Number of updated rows</returns>
        private async Task<int> AddChecklistTemplatePropertyAsync(int companyId, int userId, PropertyChecklistTemplate templateproperty)
        {
            return await _propertyValueManager.AddChecklistTemplatePropertyAsync(companyId: companyId, userId: userId, templateProperty: templateproperty);
        }

        /// <summary>
        /// ChangeTaskTemplatePropertyAsync; Change checklist template property.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="checklistTemplatePropertyId">Id of the cehcklist property id that needs to be updated (should be the same as templateproperty.id)</param>
        /// <param name="templateproperty">Template property to be added/changed.</param>
        /// <returns>Number of updated rows</returns>
        private async Task<int> ChangeChecklistTemplatePropertyAsync(int companyId, int userId, int checklistTemplatePropertyId, PropertyChecklistTemplate templateproperty)
        {
            return await _propertyValueManager.ChangeChecklistTemplatePropertyAsync(companyId: companyId, userId: userId, checklistTemplatePropertyId: checklistTemplatePropertyId, templateProperty: templateproperty); ;
        }
        #endregion

        #region - audit properties -
        /// <summary>
        /// AddChangeChecklistPropertyUserValue; Add or change checklist property user value. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskId">TaskId (DB: tasks_task.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="propertyUserValues">User values to be added or changed.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> AddChangeChecklistPropertyUserValue(int companyId, int checklistId, int userId, List<PropertyUserValue> propertyUserValues)
        {
            var existingPropertyUserValues = await _propertyValueManager.GetPropertyUserValuesByChecklistId(companyId: companyId, checklistId: checklistId);

            if (propertyUserValues != null && propertyUserValues.Count > 0)
            {
                foreach (PropertyUserValue propertyUserValue in propertyUserValues)
                {
                    var foundProperty = existingPropertyUserValues.Where(p => p.TemplatePropertyId == propertyUserValue.TemplatePropertyId).FirstOrDefault();
                    if (foundProperty != null)
                    {
                        propertyUserValue.Id = foundProperty.Id;
                    }

                    if (propertyUserValue.Id > 0)
                    {
                        propertyUserValue.ChecklistId = checklistId;
                        var resultChange = await _propertyValueManager.ChangeChecklistPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, propertyUserValueId: propertyUserValue.Id, userId: userId);
                    }
                    else 
                    {
                        propertyUserValue.ChecklistId = checklistId;
                        var resultAdd = await _propertyValueManager.AddChecklistPropertyUserValueAsync(companyId: companyId, propertyValue: propertyUserValue, userId: userId);
                    }
                }
            }
            return true;
        }
        #endregion

        #region - object enhancements -
        /// <summary>
        /// GetDynamicCountersForChecklists; Gets a list of dynamic counter (nr of actions, comments etc) for specific tasks of a specific checklist and maps these values to the supplied collection of checklists.
        /// </summary>
        /// <param name="checklists">List of checklists</param>
        /// <param name="parameters">Parameters that were used for the list of checklists.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>Updated list of checklists.</returns>
        private async Task<List<Checklist>> GetDynamicCountersForChecklists(List<Checklist> checklists, List<NpgsqlParameter> parameters, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;

            if (checklists != null && checklists.Count > 0)
            {
                List<ObjectTasksCounters> counters = new List<ObjectTasksCounters>();
                using (dr = await _manager.GetDataReader("get_counts_for_tasks_with_checklists", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var counter = CreateOrFillObjectTasksCounterFromReader(dr);
                        counters.Add(counter);
                    }
                }

                if (counters != null && counters.Count > 0)
                {
                    foreach (Checklist checklist in checklists)
                    {
                        if (checklist.Tasks != null && checklist.Tasks.Count > 0)
                        {
                            foreach (TasksTask task in checklist.Tasks)
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

            return checklists;
        }

        /// <summary>
        /// GetDynamicCountersForChecklist; Gets a list of dynamic counter (nr of actions, comments etc) for specific tasks of a specific checklist and maps these values to the supplied collection of checklist.
        /// </summary>
        /// <param name="checkliss">Checklist</param>
        /// <param name="parameters">Parameters that were used for the list of checklists.</param>
        /// <param name="connectionKind">Connection used based on connection kind. If needed connection type can be overruled (e.g. after update or insert, use writer seeing the API is faster than the DB sync).</param>
        /// <returns>Updated list of checklists.</returns>
        private async Task<Checklist> GetDynamicCountersForChecklist(Checklist checklist, List<NpgsqlParameter> parameters, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            NpgsqlDataReader dr = null;

            if (checklist != null)
            {
                List<ObjectTasksCounters> counters = new List<ObjectTasksCounters>();
                using (dr = await _manager.GetDataReader("get_counts_for_tasks_with_checklist", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var counter = CreateOrFillObjectTasksCounterFromReader(dr);
                        counters.Add(counter);
                    }
                }

                if (counters != null && counters.Count > 0)
                {

                    if (checklist.Tasks != null && checklist.Tasks.Count > 0)
                    {
                        foreach (TasksTask task in checklist.Tasks)
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

            return checklist;
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
            objectTasksCounters.ParentObjectId = Convert.ToInt32(dr["checklist_id"]);
            objectTasksCounters.TaskId = Convert.ToInt32(dr["id"]);
            objectTasksCounters.CompanyId = Convert.ToInt32(dr["company_id"]);

            return objectTasksCounters;
        }
        #endregion

        #region - Work Instructions -
        private async Task<List<TaskTemplate>> AppendWorkInstructionsAsync(int companyId, int checklistTemplateId, List<TaskTemplate> templateItems, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            //TODO fill
            if(companyId > 0 && checklistTemplateId > 0)
            {

                var workInstructionRelations = await GetWorkInstructionRelationsAsync(companyId: companyId, checklistTemplateId: checklistTemplateId, connectionKind: connectionKind);

                if (workInstructionRelations != null && workInstructionRelations.Any())
                {

                    var workInstructions = await _workInstructionManager.GetWorkInstructionTemplatesAsync(companyId: companyId, include: "items", connectionKind: connectionKind);
                  
                    if(workInstructions != null && workInstructions.Any())
                    {

                        foreach (var template in templateItems)
                        {
                            var possibleRelations = workInstructionRelations.Where(x => x.TaskTemplateId == template.Id);
                            if (possibleRelations.Any())
                            {
                                var possibleWorkInstructions = workInstructions.Where(x => possibleRelations.OrderBy(o => o.Index).Select(r => r.WorkInstructionTemplateId).ToArray().Contains(x.Id));
                                if(possibleWorkInstructions != null && possibleWorkInstructions.Any())
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

        private async Task<List<TaskTemplate>> AppendWorkInstructionRelationsAsync(int companyId, int checklistTemplateId, List<TaskTemplate> templateItems, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            if (companyId > 0 && checklistTemplateId > 0)
            {
                var workInstructionRelations = await GetWorkInstructionRelationsAsync(companyId: companyId, checklistTemplateId: checklistTemplateId, connectionKind: connectionKind);
                if (workInstructionRelations != null && workInstructionRelations.Any())
                {
                    foreach (var taskTemplate in templateItems)
                    {
                        var possibleWorkInstructionRelation = workInstructionRelations.Where(x => x.TaskTemplateId == taskTemplate.Id && x.ChecklistTemplateId == checklistTemplateId);
                        if (possibleWorkInstructionRelation != null && possibleWorkInstructionRelation.Any()) taskTemplate.WorkInstructionRelations = possibleWorkInstructionRelation.ToList();
                    }
                }
            }
            return templateItems;
        }

        private async Task<List<ChecklistTemplate>> AppendWorkInstructionRelationsAsync(int companyId, List<ChecklistTemplate> checklistTemplates)
        {
            if (companyId > 0)
            {
                var workInstructionRelations = await GetWorkInstructionRelationsAsync(companyId: companyId);
                if (workInstructionRelations != null && workInstructionRelations.Any())
                {
                    foreach (var checklistTemplate in checklistTemplates)
                    {
                        if(checklistTemplate.TaskTemplates != null)
                        {
                            foreach (var taskTemplate in checklistTemplate.TaskTemplates)
                            {
                                var possibleWorkInstructionRelation = workInstructionRelations.Where(x => x.TaskTemplateId == taskTemplate.Id && x.ChecklistTemplateId == checklistTemplate.Id);
                                if (possibleWorkInstructionRelation != null && possibleWorkInstructionRelation.Any()) taskTemplate.WorkInstructionRelations = possibleWorkInstructionRelation.ToList();
                            }
                        } 
                    }
                }
            }
            return checklistTemplates;
        }

        private async Task<List<TaskTemplateRelationWorkInstructionTemplate>> GetWorkInstructionRelationsAsync(int companyId, int? checklistTemplateId = null, ConnectionKind connectionKind = ConnectionKind.Reader)
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
                    if (checklistTemplateId.HasValue) parameters.Add(new NpgsqlParameter("@_checklisttemplateid", checklistTemplateId));

                    string sp = "get_workinstruction_checklist_item_relations";

                    using (dr = await _manager.GetDataReader(sp, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                    {
                        while (await dr.ReadAsync())
                        {
                            var item = new TaskTemplateRelationWorkInstructionTemplate();

                            item.Id = Convert.ToInt32(dr["id"]);
                            item.Name = dr["name"].ToString();
                            item.Index = Convert.ToInt32(dr["index"]);
                            item.ChecklistTemplateId = Convert.ToInt32(dr["checklisttemplate_id"]);
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
                listEx.AddRange(_propertyValueManager.GetPossibleExceptions());
                listEx.AddRange(_tagManager.GetPossibleExceptions());
                listEx.AddRange(_workInstructionManager.GetPossibleExceptions());
                listEx.AddRange(_userManager.GetPossibleExceptions());
                listEx.AddRange(_flattenedChecklistManager.GetPossibleExceptions());
                listEx.AddRange(_generalManager.GetPossibleExceptions());
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
