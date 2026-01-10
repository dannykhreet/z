using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.ExternalRelations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.Users;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Cache;
using EZGO.Api.Utils.Converters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// ActionManager; The ActionManager contains all logic for retrieving and setting Actions and ActionComments.
    /// Actions are created within the CMS and the client apps. All actions are use driven (user input). Depending on what kind of action 1 or more followup's are done through the action comments. 
    /// A action can be created stand alone (business: Action on the Spot) and have no parents or can be created with parent e.g. a Task, a Checklist item, a Audit item. 
    /// And technically it can also be created with a TaskTemplate, Audit TaskTemplate and Checklist TaskTemplate item. 
    /// A action can have a description, one ore more media items, a comment and one or more follow-up (actions comments). 
    /// </summary>
    public class ActionManager : BaseManager<ActionManager>, IActionManager
    {
        #region - privates -
        private readonly IMemoryCache _cache;
        private readonly IDatabaseAccessHelper _manager;
        private readonly IGeneralManager _generalManager;
        private readonly IAreaManager _areaManager;
        private readonly IUserManager _userManager;
        private readonly ISapPmManager _sapPmManager;
        private readonly ITagManager _tagManager;
        private readonly IToolsManager _toolsManager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IConfigurationHelper _configurationHelper;

        #endregion

        #region - properties -
        private string culture;
        public string Culture {
            get { return culture; }
            set { culture = _tagManager.Culture = value; GetSystemActionCommentTranslations(); }
        }

        Dictionary<string, string> _systemActionCommentTranslations;
        #endregion

        #region - constructor(s) -
        public ActionManager(IDatabaseAccessHelper manager, IGeneralManager generalManager, IAreaManager areaManager, ISapPmManager sapPmManager, IUserManager userManager, ITagManager tagManager, IToolsManager toolsManager, IDataAuditing dataAuditing, IConfigurationHelper configurationHelper, ILogger<ActionManager> logger, IMemoryCache memoryCache) : base(logger)
        {
            _cache = memoryCache;
            _manager = manager;
            _areaManager = areaManager;
            _userManager = userManager;
            _toolsManager = toolsManager;
            _dataAuditing = dataAuditing;
            _configurationHelper = configurationHelper;
            _tagManager = tagManager;
            _generalManager = generalManager;
            _sapPmManager = sapPmManager;
        }
        #endregion

        #region - public methods Actions -
        /// <summary>
        /// GetActionsAsync; Get actions of a company. Actions's are based on the [actions_action] table in the database.
        /// If filters are supplied there are currently 2 types of filters implemented. Filters where the source data is filtered on the database. (AssignedAreas, AssignedUsers) or filtered that are processed after the source data is loaded from the database (all other ones).
        /// Depending on future resources and throughput speeds the other filters may also be changed to source based filters. But for now the current setup works the best for a certain company.
        /// Following stored procedures will be used for database data retrieval: "get_actions_by_assigneduser_and_assignedarea" OR "get_actions_by_assigneduser" OR "get_actions_by_assignedarea"
        /// NOTE! when filtering is changed please check: <see cref="GetActionCountsAsync">GetActionCountsAsync</see>, <see cref="GetActionRelationsAsync">GetActionCountsAsync</see> if they also need to be changed. 
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <param name="include">Include parameter, comma seperated string, based on the includes enum. UnviewedCommentNr and MainParent can be used. </param>
        /// <returns>A list of actions (of a company).</returns>
        public async Task<List<ActionsAction>> GetActionsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string include = null)
        {
            var output = new List<ActionsAction>();
            List<ExternalRelation> externalRelations = new List<ExternalRelation>();

            NpgsqlDataReader dr = null;

            try
            {
                //var storedProcedure = GetActionsSourceStoredProcedureBasedOnFilter(actionFilters: filters);
                var storedProcedure = "get_actions_v3";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    //filter text
                    if(!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    if(filters.Value.UserId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", filters.Value.UserId));
                    }

                    //is resolved
                    if (filters.Value.IsResolved.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isresolved", filters.Value.IsResolved.Value));
                    }

                    //is overdue
                    if (filters.Value.IsOverdue.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isoverdue", filters.Value.IsOverdue.Value));
                    }

                    //is unresolved
                    if (filters.Value.IsUnresolved.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isunresolved", filters.Value.IsUnresolved.Value));
                    }

                    //hasunviewedcomments
                    if (filters.Value.HasUnviewedComments.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_hasunviewedcomments", filters.Value.HasUnviewedComments.Value));
                    }

                    //assigned area id
                    if (filters.Value.AssignedAreaId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedareaid", filters.Value.AssignedAreaId.Value));
                    }
                    //assigned area ids
                    else if(filters.Value.AssignedAreaIds != null && filters.Value.AssignedAreaIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedareaids", filters.Value.AssignedAreaIds));
                    }

                    //assigned user id
                    if (filters.Value.AssignedUserId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_assigneduserid", filters.Value.AssignedUserId.Value));
                    }
                    //assigned user ids
                    else if (filters.Value.AssignedUserIds != null && filters.Value.AssignedUserIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assigneduserids", filters.Value.AssignedUserIds));
                    }

                    //timestamp
                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    //created from
                    if (filters.Value.CreatedFrom.HasValue && filters.Value.CreatedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdfrom", filters.Value.CreatedFrom.Value));
                    }
                    //created to
                    if (filters.Value.CreatedTo.HasValue && filters.Value.CreatedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdto", filters.Value.CreatedTo.Value));
                    }

                    //overdue from
                    if (filters.Value.OverdueFrom.HasValue && filters.Value.OverdueFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_overduefrom", filters.Value.OverdueFrom.Value));
                    }
                    //overdue to
                    if (filters.Value.OverdueTo.HasValue && filters.Value.OverdueTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_overdueto", filters.Value.OverdueTo.Value));
                    }

                    //resolved from
                    if (filters.Value.ResolvedFrom.HasValue && filters.Value.ResolvedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_resolvedfrom", filters.Value.ResolvedFrom.Value));
                    }
                    //resolved to
                    if (filters.Value.ResolvedTo.HasValue && filters.Value.ResolvedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_resolvedto", filters.Value.ResolvedTo.Value));
                    }

                    //resolved cut-off date
                    if (filters.Value.ResolvedCutoffDate.HasValue && filters.Value.ResolvedCutoffDate.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_resolvedcutoffdate", filters.Value.ResolvedCutoffDate.Value));
                    }

                    //created by id
                    if (filters.Value.CreatedById.HasValue && filters.Value.CreatedById.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdbyid", filters.Value.CreatedById.Value));
                    }

                    //for the 'assigned to me' filter, returns all actions assigned to user or allowed area for the user
                    if (filters.Value.AssignedToMeUserId.HasValue && filters.Value.AssignedToMeUserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedtomeuserid", filters.Value.AssignedToMeUserId.Value));
                    }

                    //my actions (created by me or assigned to me)
                    if (filters.Value.CreatedByOrAssignedTo.HasValue && filters.Value.CreatedByOrAssignedTo.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdbyorassignedto", filters.Value.CreatedByOrAssignedTo.Value));
                    }

                    //task id
                    if (filters.Value.TaskId.HasValue && filters.Value.TaskId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_taskid", filters.Value.TaskId.Value));
                    }

                    //task template id
                    if (filters.Value.TaskTemplateId.HasValue && filters.Value.TaskTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tasktemplateid", filters.Value.TaskTemplateId.Value));
                    }

                    //checklist id
                    if (filters.Value.ChecklistId.HasValue && filters.Value.ChecklistId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_checklistid", filters.Value.ChecklistId.Value));
                    }

                    //checklist template id
                    if (filters.Value.ChecklistTemplateId.HasValue && filters.Value.ChecklistTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_checklisttemplateid", filters.Value.ChecklistTemplateId.Value));
                    }

                    //audit id
                    if (filters.Value.AuditId.HasValue && filters.Value.AuditId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_auditid", filters.Value.AuditId.Value));
                    }

                    //audit template id
                    if (filters.Value.AuditTemplateId.HasValue && filters.Value.AuditTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_audittemplateid", filters.Value.AuditTemplateId.Value));
                    }

                    //tag ids
                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    //parent area id
                    if (filters.Value.ParentAreaId != null && filters.Value.ParentAreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_parentareaid", filters.Value.ParentAreaId.Value));
                    }

                    //limit
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    //offset
                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                    //sort parameters
                    if (!string.IsNullOrWhiteSpace(filters.Value.SortColumn))
                    {
                        parameters.Add(new NpgsqlParameter("@_sortby", filters.Value.SortColumn.ToLower()));
                    }

                    if (!string.IsNullOrWhiteSpace(filters.Value.SortDirection))
                    {
                        parameters.Add(new NpgsqlParameter("@_sortdirection", filters.Value.SortDirection.ToLower()));
                    }
                }

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var action = CreateOrFillActionFromReader(dr);
                        output.Add(action);
                    }
                }


                List<NpgsqlParameter> externalRelationParameters = new List<NpgsqlParameter>();
                externalRelationParameters.Add(new NpgsqlParameter("@_companyid", companyId));
                using (dr = await _manager.GetDataReader("get_external_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: externalRelationParameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var externalRelation = CreateOrFillExternalRelationFromReader(dr, externalRelation: new ExternalRelation());
                        if (externalRelation.ObjectType == "actions_action")
                        {
                            externalRelations.Add(externalRelation);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }

            }

            //if (filters.HasValue && filters.Value.HasFilters())
            //{
            //    output = (await FilterActions(companyId: companyId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            //}

            try
            {
                foreach (var externalRelation in externalRelations)
                {
                    var action = output.Where(a => a.Id == externalRelation.ObjectId).FirstOrDefault();
                    if (action != null)
                    {
                        var actionIndex = output.IndexOf(action);

                        if (externalRelation.ObjectType == "actions_action" && externalRelation.ConnectorType == "ULTIMO")
                        {
                            output[actionIndex].UltimoStatus = externalRelation.Status;
                            output[actionIndex].UltimoStatusDateTime = externalRelation.ModifiedAt;
                            if (externalRelation.Status == "SENT")
                            {
                                output[actionIndex].SendToUltimo = true;
                            }
                        }
                        else
                        {
                            output[actionIndex].UltimoStatus = "NONE";
                            output[actionIndex].UltimoStatusDateTime = null;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            try
            {
                //get sap notifications for company
                var notifications = await GetSapPmNotificationsAsync(companyId: companyId, output.Select(a => a.Id).ToList());

                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP"))
                {
                    foreach (var action in output) 
                    {
                        var notification = notifications?.Where(n => n.ActionId == action.Id).FirstOrDefault();

                        if (notification != null)
                        {
                            action.SendToSapPm = true;
                            action.SapPmNotificationConfig = notification.ToSapPmNotificationConfig();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            if (userId.HasValue && !string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.UnviewedCommentNr.ToString().ToLower())) output = await GetUnviewedStatisticsWithActions(actions: output, companyId: companyId, userId: userId.Value);
            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.MainParent.ToString().ToLower())) output = await AppendParentsToActions(actions: output, companyId: companyId);
            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AssignedAreas.ToString().ToLower())) output = await AppendAssignedAreasToActions(companyId: companyId, actions: output);
            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AssignedUsers.ToString().ToLower())) output = await AppendAssignedUsersToActions(companyId: companyId, actions: output);
            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.UserInformation.ToString().ToLower())) output = await GetUserWithActionsAsync(companyId: companyId, actions: output);
            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) output = await AppendTagsToActionsAsync(companyId: companyId, actions: output);

            return output;
        }

        /// <summary>
        /// Gets the latest actions, based on create date and time
        /// when sorting is implemented in GetActionsAsync, that function can be used again and this one can become obsolete
        /// This currently only used for dashboard to get latest actions, because GetActionsAsync is unreliable while custom sorting is not implemented there.
        /// </summary>
        /// <param name="companyId">company id to get the actions for</param>
        /// <param name="userId">Unused</param>
        /// <param name="filters">Only limit and offset implemented, default limit 100 in db function</param>
        /// <param name="include">Not implemented</param>
        /// <returns>The most recently created actions for given company</returns>
        public async Task<List<ActionsAction>> GetLatestActionsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string  include = null)
        {
            List<ActionsAction> latestActions = new();

            List<NpgsqlParameter> parameters = new()
            {
                new NpgsqlParameter("@_companyid", companyId)
            };

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
            }

            try
            {
                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_actions_latest", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    var action = CreateOrFillActionFromReader(dr);
                    latestActions.Add(action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message:$"{nameof(ActionManager)}.{nameof(GetActionCountsAsync)}: {ex.Message}");

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return latestActions;
        }

        /// <summary>
        /// GetActionCountsAsync; Get a action counts objects, will return several counts related to statistics. 
        /// Filtering based on V3 version of the SP of actions. 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="filters"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public async Task<ActionCountStatistics> GetActionCountsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string include = null)
        {
            var output = new ActionCountStatistics();

            NpgsqlDataReader dr = null;

            try
            {

                var storedProcedure = "get_actions_v3_counts";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    //filter text
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    if (filters.Value.UserId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", filters.Value.UserId));
                    }

                    //is resolved
                    if (filters.Value.IsResolved.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isresolved", filters.Value.IsResolved.Value));
                    }

                    //is overdue
                    if (filters.Value.IsOverdue.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isoverdue", filters.Value.IsOverdue.Value));
                    }

                    //is unresolved
                    if (filters.Value.IsUnresolved.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isunresolved", filters.Value.IsUnresolved.Value));
                    }

                    //hasunviewedcomments
                    if (filters.Value.HasUnviewedComments.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_hasunviewedcomments", filters.Value.HasUnviewedComments.Value));
                    }

                    //assigned area id
                    if (filters.Value.AssignedAreaId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedareaid", filters.Value.AssignedAreaId.Value));
                    }
                    //assigned area ids
                    else if (filters.Value.AssignedAreaIds != null && filters.Value.AssignedAreaIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedareaids", filters.Value.AssignedAreaIds));
                    }

                    //assigned user id
                    if (filters.Value.AssignedUserId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_assigneduserid", filters.Value.AssignedUserId.Value));
                    }
                    //assigned user ids
                    else if (filters.Value.AssignedUserIds != null && filters.Value.AssignedUserIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assigneduserids", filters.Value.AssignedUserIds));
                    }

                    //timestamp
                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    //created from
                    if (filters.Value.CreatedFrom.HasValue && filters.Value.CreatedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdfrom", filters.Value.CreatedFrom.Value));
                    }
                    //created to
                    if (filters.Value.CreatedTo.HasValue && filters.Value.CreatedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdto", filters.Value.CreatedTo.Value));
                    }

                    //overdue from
                    if (filters.Value.OverdueFrom.HasValue && filters.Value.OverdueFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_overduefrom", filters.Value.OverdueFrom.Value));
                    }
                    //overdue to
                    if (filters.Value.OverdueTo.HasValue && filters.Value.OverdueTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_overdueto", filters.Value.OverdueTo.Value));
                    }

                    //resolved from
                    if (filters.Value.ResolvedFrom.HasValue && filters.Value.ResolvedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_resolvedfrom", filters.Value.ResolvedFrom.Value));
                    }
                    //resolved to
                    if (filters.Value.ResolvedTo.HasValue && filters.Value.ResolvedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_resolvedto", filters.Value.ResolvedTo.Value));
                    }

                    //created by id
                    if (filters.Value.CreatedById.HasValue && filters.Value.CreatedById.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdbyid", filters.Value.CreatedById.Value));
                    }

                    //for the 'assigned to me' filter, returns all actions assigned to user or allowed area for the user
                    if (filters.Value.AssignedToMeUserId.HasValue && filters.Value.AssignedToMeUserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedtomeuserid", filters.Value.AssignedToMeUserId.Value));
                    }

                    //my actions (created by me or assigned to me)
                    if (filters.Value.CreatedByOrAssignedTo.HasValue && filters.Value.CreatedByOrAssignedTo.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdbyorassignedto", filters.Value.CreatedByOrAssignedTo.Value));
                    }

                    //task id
                    if (filters.Value.TaskId.HasValue && filters.Value.TaskId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_taskid", filters.Value.TaskId.Value));
                    }

                    //task template id
                    if (filters.Value.TaskTemplateId.HasValue && filters.Value.TaskTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tasktemplateid", filters.Value.TaskTemplateId.Value));
                    }

                    //checklist id
                    if (filters.Value.ChecklistId.HasValue && filters.Value.ChecklistId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_checklistid", filters.Value.ChecklistId.Value));
                    }

                    //checklist template id
                    if (filters.Value.ChecklistTemplateId.HasValue && filters.Value.ChecklistTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_checklisttemplateid", filters.Value.ChecklistTemplateId.Value));
                    }

                    //audit id
                    if (filters.Value.AuditId.HasValue && filters.Value.AuditId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_auditid", filters.Value.AuditId.Value));
                    }

                    //audit template id
                    if (filters.Value.AuditTemplateId.HasValue && filters.Value.AuditTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_audittemplateid", filters.Value.AuditTemplateId.Value));
                    }

                    //tag ids
                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    //parent area id
                    if (filters.Value.ParentAreaId != null && filters.Value.ParentAreaId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_parentareaid", filters.Value.ParentAreaId.Value));
                    }

                    //limit
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    //offset
                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }
                }

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.TotalCount = dr["total_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["total_count"]);
                        output.IsResolvedCount = dr["is_resolved_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_resolved_count"]);
                        output.IsOverdueCount = dr["is_overdue_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_overdue_count"]);
                        output.IsUnresolvedCount = dr["is_unresolved_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_unresolved_count"]);
                        output.IsCreatedByMeCount = dr["is_created_by_me_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_created_by_me_count"]);
                        output.IsAssignedToMeCount = dr["is_assigned_to_me_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_assigned_to_me_count"]);
                        output.HasCommentsCount = dr["has_comments_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["has_comments_count"]);
                        output.HasUnviewedCommentsCount = dr["has_unviewed_comments_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["has_unviewed_comments_count"]);
                        output.IsDueTodayCount = dr["is_due_today_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_due_today_count"]);
                        output.IsCreatedTodayCount = dr["is_created_today_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_created_today_count"]);
                        output.IsModifiedTodayCount = dr["is_modified_today_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_modified_today_count"]);
                        output.IsActionOnTheSpotCount = dr["is_action_on_the_spot_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_action_on_the_spot_count"]);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCountsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output; 
        }

        /// <summary>
        /// GetActionRelationsAsync; Get a list of action relations. 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="filters"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public async Task<List<ActionRelation>> GetActionRelationsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string include = null)
        {
            var output = new List<ActionRelation>();

            NpgsqlDataReader dr = null;

            try
            {

                var storedProcedure = "get_actions_v3_relations";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (filters.HasValue)
                {
                    //filter text
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    if (filters.Value.UserId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", filters.Value.UserId));
                    }

                    //is resolved
                    if (filters.Value.IsResolved.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isresolved", filters.Value.IsResolved.Value));
                    }

                    //is overdue
                    if (filters.Value.IsOverdue.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isoverdue", filters.Value.IsOverdue.Value));
                    }

                    //is unresolved
                    if (filters.Value.IsUnresolved.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_isunresolved", filters.Value.IsUnresolved.Value));
                    }

                    //hasunviewedcomments
                    if (filters.Value.HasUnviewedComments.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_hasunviewedcomments", filters.Value.HasUnviewedComments.Value));
                    }

                    //assigned area id
                    if (filters.Value.AssignedAreaId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedareaid", filters.Value.AssignedAreaId.Value));
                    }
                    //assigned area ids
                    else if (filters.Value.AssignedAreaIds != null && filters.Value.AssignedAreaIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedareaids", filters.Value.AssignedAreaIds));
                    }

                    //assigned user id
                    if (filters.Value.AssignedUserId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_assigneduserid", filters.Value.AssignedUserId.Value));
                    }
                    //assigned user ids
                    else if (filters.Value.AssignedUserIds != null && filters.Value.AssignedUserIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assigneduserids", filters.Value.AssignedUserIds));
                    }

                    //timestamp
                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    //created from
                    if (filters.Value.CreatedFrom.HasValue && filters.Value.CreatedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdfrom", filters.Value.CreatedFrom.Value));
                    }
                    //created to
                    if (filters.Value.CreatedTo.HasValue && filters.Value.CreatedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdto", filters.Value.CreatedTo.Value));
                    }

                    //overdue from
                    if (filters.Value.OverdueFrom.HasValue && filters.Value.OverdueFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_overduefrom", filters.Value.OverdueFrom.Value));
                    }
                    //overdue to
                    if (filters.Value.OverdueTo.HasValue && filters.Value.OverdueTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_overdueto", filters.Value.OverdueTo.Value));
                    }

                    //resolved from
                    if (filters.Value.ResolvedFrom.HasValue && filters.Value.ResolvedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_resolvedfrom", filters.Value.ResolvedFrom.Value));
                    }
                    //resolved to
                    if (filters.Value.ResolvedTo.HasValue && filters.Value.ResolvedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_resolvedto", filters.Value.ResolvedTo.Value));
                    }

                    //created by id
                    if (filters.Value.CreatedById.HasValue && filters.Value.CreatedById.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdbyid", filters.Value.CreatedById.Value));
                    }

                    //for the 'assigned to me' filter, returns all actions assigned to user or allowed area for the user
                    if (filters.Value.AssignedToMeUserId.HasValue && filters.Value.AssignedToMeUserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedtomeuserid", filters.Value.AssignedToMeUserId.Value));
                    }

                    //my actions (created by me or assigned to me)
                    if (filters.Value.CreatedByOrAssignedTo.HasValue && filters.Value.CreatedByOrAssignedTo.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdbyorassignedto", filters.Value.CreatedByOrAssignedTo.Value));
                    }

                    //task id
                    if (filters.Value.TaskId.HasValue && filters.Value.TaskId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_taskid", filters.Value.TaskId.Value));
                    }

                    //task template id
                    if (filters.Value.TaskTemplateId.HasValue && filters.Value.TaskTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tasktemplateid", filters.Value.TaskTemplateId.Value));
                    }

                    //tag ids
                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    //limit
                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    //offset
                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }
                }

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        ActionRelation actionRelation = new ActionRelation();

                        actionRelation.ActionId = Convert.ToInt32(dr["action_id"]);
                        if (dr["checklist_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["checklist_id"].ToString()))
                        {
                            actionRelation.ChecklistId = Convert.ToInt32(dr["checklist_id"]);
                        }
                        if (dr["audit_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["audit_id"].ToString()))
                        {
                            actionRelation.AuditId = Convert.ToInt32(dr["audit_id"]);
                        }
                        if (dr["task_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["task_id"].ToString()))
                        {
                            actionRelation.TaskId = Convert.ToInt32(dr["task_id"]);
                        }
                        if (dr["tasktemplate_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["tasktemplate_id"].ToString()))
                        {
                            actionRelation.TaskTemplateId = Convert.ToInt32(dr["tasktemplate_id"]);
                        }

                        output.Add(actionRelation);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionRelationsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetActionsByTaskIdAsync; Get actions by TaskId.
        /// Following stored procedures will be used for database data retrieval: "get_actions_by_task"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskId">TaskId (DB: tasks_task.id)</param>
        /// <returns>A list of actions based on supplied TaskId.</returns>
        public async Task<List<ActionsAction>> GetActionsByTaskIdAsync(int companyId, long taskId)
        {
            var output = new List<ActionsAction>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_taskid", Convert.ToInt32(taskId)));

                using (dr = await _manager.GetDataReader("get_actions_by_task", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var action = CreateOrFillActionFromReader(dr);
                        output.Add(action);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionsByTaskIdAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetActionsByTaskTemplateIdAsync; Get actions by TaskTemplateId.
        /// Following stored procedures will be used for database data retrieval: "get_actions_by_tasktemplate"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="taskTemplateId">TaskTemplateId  (DB: tasks_tasktemplate.id)</param>
        /// <returns>A list of actions based on supplied TaskTemplateId.</returns>
        public async Task<List<ActionsAction>> GetActionsByTaskTemplateIdAsync(int companyId, int taskTemplateId)
        {
            var output = new List<ActionsAction>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_tasktemplateid", taskTemplateId));

                using (dr = await _manager.GetDataReader("get_actions_by_tasktemplate", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var action = CreateOrFillActionFromReader(dr);
                        output.Add(action);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionsByTaskTemplateIdAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetActionAsync; Get a single action object, based on the actionid parameter.
        /// Following stored procedures will be used for database data retrieval: "get_action"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="include">Include, comma separated string based on IncludesEnum, used for including extra data.</param>
        /// <returns>A single action object.</returns>
        public async Task<ActionsAction> GetActionAsync(int companyId, int actionId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, int? userId = null)
        {
            var action = new ActionsAction();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", actionId));

                using (dr = await _manager.GetDataReader("get_action", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillActionFromReader(dr, action: action);
                    }
                }

                List<NpgsqlParameter> externalRelationParameters = new List<NpgsqlParameter>();
                externalRelationParameters.Add(new NpgsqlParameter("@_companyid", companyId));
                externalRelationParameters.Add(new NpgsqlParameter("@_objectid", actionId));
                externalRelationParameters.Add(new NpgsqlParameter("@_objecttype", "actions_action"));
                ExternalRelation externalRelation = null;
                using (dr = await _manager.GetDataReader("get_external_relation", commandType: System.Data.CommandType.StoredProcedure, parameters: externalRelationParameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        externalRelation = CreateOrFillExternalRelationFromReader(dr, externalRelation: externalRelation);
                    }
                }

                if (externalRelation != null && externalRelation.ObjectId == actionId && externalRelation.ObjectType == "actions_action" && externalRelation.ConnectorType == "ULTIMO")
                {
                    action.UltimoStatus = externalRelation.Status;
                    action.UltimoStatusDateTime = externalRelation.ModifiedAt;
                    if (externalRelation.Status == "SENT")
                    {
                        action.SendToUltimo = true;
                    }
                }
                else
                {
                    action.UltimoStatus = "NONE";
                    action.UltimoStatusDateTime = null;
                }

                //get sap pm notification
                var notification = await GetSapPmNotificationAsync(companyId, actionId);

                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP") && notification.Id > 0)
                {
                    action.SendToSapPm = true;
                    action.SapPmNotificationConfig = notification.ToSapPmNotificationConfig();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (action.Id > 0)
            {
                if (userId.HasValue && !string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.UnviewedCommentNr.ToString().ToLower())) action.UnviewedCommentNr = await GetUnviewedStatisticsWithAction(actionId: action.Id, companyId: companyId, userId: userId.Value);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Comments.ToString().ToLower())) action.Comments = await GetCommentsWithAction(companyId: companyId, actionId: action.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AssignedAreas.ToString().ToLower())) action.AssignedAreas = await GetAreasWithActionAsync(companyId: companyId, actionId: action.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.AssignedUsers.ToString().ToLower())) action.AssignedUsers = await GetUsersWithActionAsync(companyId: companyId, actionId: action.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.MainParent.ToString().ToLower())) action.Parent = await GetParentBasicWithActionAsync(companyId: companyId, actionId: action.Id);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.UserInformation.ToString().ToLower())) action.CreatedByUser = await GetUserWithActionAsync(companyId: companyId, userId: action.CreatedById);
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) action.Tags = await GetTagsWithActionAsync(companyId: companyId, actionId: action.Id);
                return action;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// GetAsssignedAreasWithActions; Get assigned areas that are set for all actions of a specific company.
        /// Following stored procedures will be used for database data retrieval: "get_assignedareaids_with_actions"
        /// </summary>
        /// <param name="companyId">CompanyId of the company.</param>
        /// <returns>A list of action area relation objects.</returns>
        public async Task<List<ActionRelationArea>> GetAsssignedAreasWithActions(int companyId)
        {
            var ls = new List<ActionRelationArea>();
            var areas = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 9999, useTreeview: false);

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_assignedareaids_with_actions", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var item = new ActionRelationArea();
                        var area = areas.Where(x => x.Id == Convert.ToInt32(dr["area_id"])).FirstOrDefault();
                        if(area != null)
                        {
                            item.ActionId = Convert.ToInt32(dr["action_id"]);
                            item.Id = area.Id;
                            item.Name = area.Name;

                            ls.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetAsssignedAreasWithActions(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return ls;
        }

        /// <summary>
        /// GetAsssignedUsersWithActions, get assigned users with actions based on the companyid
        /// Following stored procedures will be used for database data retrieval: "get_assigneduserids_with_actions"
        /// </summary>
        /// <param name="companyId">CompanyId of the company.</param>
        /// <returns>List of action user relation objects.</returns>
        public async Task<List<ActionRelationUser>> GetAsssignedUsersWithActions(int companyId)
        {
            var ls = new List<ActionRelationUser>();
            var users = await _userManager.GetUserProfilesAsync(companyId: companyId);

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_assigneduserids_with_actions", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var item = new ActionRelationUser();
                        var user = users.Where(x => x.Id == Convert.ToInt32(dr["user_id"])).FirstOrDefault();
                        if (user != null)
                        {
                            item.ActionId = Convert.ToInt32(dr["action_id"]);
                            item.Id = user.Id;
                            item.Name = string.Concat(user.FirstName, " ", user.LastName);
                            item.Picture = user.Picture;

                            ls.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetAsssignedUsersWithActions(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }


            return ls;
        }


        /// <summary>
        /// AddActionAsync; Add a action to the database.
        /// If AssignedUsers and AssignedAreas are available these will be added to.
        /// Following stored procedures will be used for database data retrieval: "add_action"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="action">Action object that needs to be added to the database.</param>
        /// <returns>The created objects id (DB actions_action.id)</returns>
        public async Task<int> AddActionAsync(int companyId, int userId, ActionsAction action)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            action = PrepareAndCleanAction(action: action);

            parameters.AddRange(GetNpgsqlParametersFromAction(action: action, companyId: action.CompanyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_action", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if(possibleId > 0)
            {
                action.Id = possibleId;
                await UpdateRelationsWithAction(companyId: companyId, userId: userId, action: action);

                //if sap feature is turned on
                if (action.SendToSapPm == true && await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP"))
                {
                    //add chat message queued for sending to sap pm
                    await AddActionCommentAsync(companyId, userId, new ActionComment()
                    {
                        ActionId = action.Id,
                        UserId = userId,
                        CompanyId = companyId,
                        Comment = "Action has been queued to be sent to SAP PM."
                    });
                    //add notification in sap pm table
                    await AddSapPmNotificationAsync(companyId, userId, action);
                }

                if (possibleId > 0)
                {
                    var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), possibleId);
                    await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.actions_action.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added action.");
                }
                //fix possible images if needed.
                //await CheckValidateFixMedia(companyId: companyId, userId: userId, actionId: action.Id, action: action);

                //reset all action related cache keys because of new item.
                CacheHelpers.ResetCacheByKeyByKeyStart(_cache, CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyActionParent, companyId));
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeActionAsync; Change action in db based on action id.
        /// Following stored procedures will be used for database data retrieval: "change_action"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="action">Action object with data that needs to be changed.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeActionAsync(int companyId, int userId, int actionId, ActionsAction action)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);

            action = PrepareAndCleanAction(action: action);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromAction(action: action, companyId: companyId, actionId: actionId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_action", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure)); //await _manager.ExecuteNonQueryAsync("UPDATE actions_action SET due_date = @DueDate, is_resolved = @IsResolved, comment = @Comment, description = @Description, image_0 = @Image0, image_1 = @Image1, image_2 = @Image2, image_3 = @Image3, image_4 = @Image4, image_5 = @Image5, created_by_id = @CreatedById, task_id = @TaskId, resolved_at = @ResolvedAt, modified_at = now()::timestamp,  task_template_id = @TaskTemplateId, video_0 = @Video0, video_1 = @Video1, video_2 = @Video2, video_3 = @Video3, video_4 = @Video4, video_5 = @Video5, video_thumbnail_0 = @VideoThumbnail0, video_thumbnail_1 = @VideoThumbnail1, video_thumbnail_2 = @VideoThumbnail2, video_thumbnail_3 = @VideoThumbnail3, video_thumbnail_4 = @VideoThumbnail4, video_thumbnail_5 = @VideoThumbnail5 WHERE id = @Id AND company_id = @CompanyId", parameters: parameters, commandType: System.Data.CommandType.Text);

            await UpdateRelationsWithAction(companyId: companyId, userId: userId, action: action);

            //get sap pm notification
            var notification = await GetSapPmNotificationAsync(companyId, actionId);

            //if sap feature is turned on and no notification found (id = 0 for new notification)
            if (action.SendToSapPm == true && await _generalManager.GetHasAccessToFeatureByCompany(companyId, "MARKET_SAP"))
            {
                //add chat message queued for sending to sap pm
                await AddActionCommentAsync(companyId, userId, new ActionComment()
                {
                    ActionId = action.Id,
                    UserId = userId,
                    CompanyId = companyId,
                    Comment = "Action has been queued to be sent to SAP PM."
                });

                //add notification in sap pm table if it doesnt exist
                if (notification.Id == 0)
                {
                    await AddSapPmNotificationAsync(companyId, userId, action);
                }
                else
                {
                    await ChangeSapPmNotificationAsync(companyId: companyId, userId: userId, notificationId: notification.Id, action: action);
                }
            }

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action.ToString(), objectId: actionId, userId: userId, companyId: companyId, description: "Changed action.");
            }

            return (rowseffected > 0);

        }

        /// <summary>
        /// SetActionActiveAsync; Sets a specific action active or inactive.
        /// Following stored procedures will be used for database data retrieval: "set_action_active"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a action to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> SetActionActiveAsync(int companyId, int userId, int actionId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", actionId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_action_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action.ToString(), objectId: actionId, userId: userId, companyId: companyId, description: "Changed action active state.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// SetActionResolvedAsync; Set action to resolved.
        /// Following stored procedures will be used for database data retrieval: "set_action_resolved"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="isResolved">true / false -> set item to resolved.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> SetActionResolvedAsync(int companyId, int userId, int actionId, bool isResolved = true, bool useAutoResolvedMessage = false)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", actionId));
            parameters.Add(new NpgsqlParameter("@_isresolved", isResolved));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_action_resolved", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action.ToString(), objectId: actionId, userId: userId, companyId: companyId, description: "Changed action resolved state.");
            }

            if (useAutoResolvedMessage)
            {
                //get translation keys with get_resource_language_actions_actioncomments
                var translations = GetSystemActionCommentTranslations();

                //CMS_ACTION_WAS_EDITED
                //The following items of this action have been changed: 
                var comment = string.Empty;
                if (translations != null && translations.ContainsKey("ACTION_EDITED_TITLE"))
                {
                    comment += (translations["ACTION_EDITED_TITLE"] + " ");
                }
                else
                {
                    comment += "The following items of this action have been changed: ";
                }

                //CMS_ACTION_ISRESOLVED_EDITED
                //Completed
                if (translations != null && translations.ContainsKey("BASE_TEXT_COMPLETED"))
                {
                    comment += (translations["BASE_TEXT_COMPLETED"]);
                }
                else
                {
                    comment += "Completed";
                }

                await this.AddActionCommentAsync(companyId: companyId, userId: userId, new ActionComment() { ActionId = actionId, Comment = comment, CompanyId = companyId, UserId = userId }); //set default action resolved text.
            }
            return (rowseffected > 0);
        }

        /// <summary>
        /// SetActionTaskAsync; Set the TaskId for a certain action. This can be used when adding a checklist or audit (or task) where the action is already added for a template. That same action can then also be set to a specific task for UI purposes.
        /// Following stored procedures will be used for database data retrieval: "set_action_task"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="taskId">TaskId (DB: tasks_task.id)</param>
        /// <returns></returns>
        public async Task<bool> SetActionTaskAsync(int companyId, int userId, int actionId, int taskId)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", actionId));
            parameters.Add(new NpgsqlParameter("@_taskid", taskId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_action_task", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if(rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action.ToString(), actionId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action.ToString(), objectId: actionId, userId: userId, companyId: companyId, description: "Changed action task.");
            }

            return (rowseffected > 0);
        }


        #endregion

        #region - public methods ActionComments -
        /// <summary>
        /// GetActionCommentsAsync; Get ActionComments of a company. ActionComments are based on the [actions_actioncomment] table in the database.
        /// Following stored procedures will be used for database data retrieval: "get_actioncomments"
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id)</param>
        /// <param name="filters">Filters that can be used for filtering the data. Depending on implementation, filters can be done within the stored procedures or afterwards.</param>
        /// <param name="include">Comma seperated string based on the IncludesTypeEnum</param>
        /// <returns>A list of ActionComments (of a company).</returns>
        public async Task<List<ActionComment>> GetActionCommentsAsync(int companyId, ActionFilters? filters = null, string include = null)
        {
            var output = new List<ActionComment>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader("get_actioncomments", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var actioncomment = CreateOrFillActionCommentFromReader(dr);
                        output.Add(actioncomment);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCommentsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (filters.HasValue && filters.Value.HasFilters())
            {
                output = (await FilterActionComments(companyId: companyId, filters: filters.Value, nonFilteredCollection: output)).ToList();
            }

            output = await GetUserWithActionCommentsAsync(companyId: companyId, comments: output);

            return output;
        }

        /// <summary>
        /// GetActionCommentsByActionIdAsync; Get ActionComments of a company by it's action id. ActionComments are based on the [actions_actioncomment] table in the database.
        /// Following stored procedures will be used for database data retrieval: "get_actioncomments_by_action"
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>A list of ActionComments based on the supplied ActionId</returns>
        public async Task<List<ActionComment>> GetActionCommentsByActionIdAsync(int companyId, int actionId)
        {
            var output = new List<ActionComment>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", actionId));

                using (dr = await _manager.GetDataReader("get_actioncomments_by_action", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var actioncomment = CreateOrFillActionCommentFromReader(dr);
                        output.Add(actioncomment);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCommentsByActionIdAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            output = await GetUserWithActionCommentsAsync(companyId: companyId, comments: output);

            return output;
        }

        /// <summary>
        /// GetActionCommentAsync; Get a single ActionComment object, based on the ActionCommentId parameter.
        /// Following stored procedures will be used for database data retrieval: "get_actioncomment"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionCommentId">ActionCommentId (DB: actions_actioncomment.id)</param>
        /// <param name="include">Include items based on IncludesTypeEnum.</param>
        /// <returns>A single ActionComment object.</returns>
        public async Task<ActionComment> GetActionCommentAsync(int companyId, int actionCommentId, string include = null)
        {
            var actioncomment = new ActionComment();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", actionCommentId));

                using (dr = await _manager.GetDataReader("get_actioncomment", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillActionCommentFromReader(dr, actioncomment: actioncomment);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCommentAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            if (actioncomment.Id > 0)
            {
                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.ViewedByUsers.ToString().ToLower())) actioncomment.ViewedByUsers = await GetViewedByUsersWithComment(companyId: companyId, commentId: actioncomment.Id);

                return actioncomment;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// AddActionCommentAsync; Adds a ActionComment to the database.
        /// Following stored procedures will be used for database data retrieval: "add_actioncomment"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="actionComment">ActionComment object (DB: actions_actioncomment)</param>
        /// <returns>The identity of the table (DB: actions_actioncomment.id)</returns>
        public async Task<int> AddActionCommentAsync(int companyId, int userId, ActionComment actionComment)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            actionComment = PrepareAndCleanActionComment(actionComment: actionComment);

            parameters.AddRange(GetNpgsqlParametersFromActionComment(actionComment: actionComment, companyId: actionComment.CompanyId.HasValue ? actionComment.CompanyId.Value : companyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_actioncomment", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_actioncomment.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.actions_actioncomment.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added actioncomment.");
            }

            return possibleId;

        }

        /// <summary>
        /// ChangeActionCommentAsync; Change a ActionComment.
        /// Following stored procedures will be used for database data retrieval: "change_actioncomment"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="actionComment">ActionComment object (DB: actions_actioncomment)</param>
        /// <param name="actionCommentId">ActionComment id (DB: actions_actioncomment.id)</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> ChangeActionCommentAsync(int companyId, int userId, int actionCommentId, ActionComment actionComment)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_actioncomment.ToString(), actionCommentId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            actionComment = PrepareAndCleanActionComment(actionComment: actionComment);

            parameters.AddRange(GetNpgsqlParametersFromActionComment(actionComment: actionComment, companyId: companyId, actionCommentId: actionCommentId));

            //TODO add company check
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_actioncomment", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_actioncomment.ToString(), actionCommentId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_actioncomment.ToString(), objectId: actionCommentId, userId: userId, companyId: companyId, description: "Changed actioncomment.");

            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// SetActionCommentActiveAsync; Sets a specific ActionComment active or inactive.
        /// NOTE! because of the location of the company id in the database a join is used within the update.
        /// Following stored procedures will be used for database data retrieval: "set_actioncomment_active"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <param name="actionCommentId">ActionCommentId (DB: actions_actioncomment.id)</param>
        /// <param name="isActive">true / false -> default true is selected, for setting a ActionComment to inactive, set parameter to false.</param>
        /// <returns>true / false depending on outcome. If more then 1 row is updated, true is returned in all other cases false is returned.</returns>
        public async Task<bool> SetActionCommentActiveAsync(int companyId, int userId, int actionCommentId, bool isActive = true)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_actioncomment.ToString(), actionCommentId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", actionCommentId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_actioncomment_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if(rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_actioncomment.ToString(), actionCommentId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_actioncomment.ToString(), objectId: actionCommentId, userId: userId, companyId: companyId, description: "Changed actioncomment active state.");
            }


            return (rowseffected > 0);
        }

        /// <summary>
        /// SetActionCommentViewedAsync; Sets the viewed status for a user for a certain actioncommentid;
        /// Following stored procedures will be used for database data retrieval: "set_actioncomment_viewed"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionCommentId">ActionCommentId (DB: actioncomment_viewed.actioncomment_id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>true / false depending on outcome.</returns>
        public async Task<bool> SetActionCommentViewedAsync(int companyId, int actionCommentId, int userId)
        {

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", actionCommentId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_actioncomment_viewed", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);

        }

        /// <summary>
        /// SetActionCommentViewedAllAsync; Sets the viewed status for a user for all comments with a action;
        /// Following stored procedures will be used for database data retrieval: "set_actioncomment_viewed_all"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>true / false depending on outcome.</returns>
        public async Task<bool> SetActionCommentViewedAllAsync(int companyId, int actionId, int userId)
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", actionId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_actioncomment_viewed_all", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                return (rowseffected > 0);
#pragma warning disable CS0168 // Variable is declared but never used
            } catch (Exception ex) //swallow error for now.
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return false;
            }

        }

        /// <summary>
        /// Gets the ActionComment viewed statistics for items that the user is involved in as creator or as an assigned user.
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="userId">user id</param>
        /// <returns>List of ActionCommentViewedStatsItem objects for actions the user is involved with</returns>
        public async Task<List<ActionCommentViewedStatsItem>> GetActionCommentStatisticsRelatedToUser(int companyId, int userId)
        {
            var stats = new List<ActionCommentViewedStatsItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader("get_actioncomments_viewed_statistics_related_to_user", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var statsItem = CreateActionCommentViewedStatsItemFromReader(dr);
                        stats.Add(statsItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCommentViewedStatistics(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return stats;
        }
        #endregion

        #region - public methods Actions Other -
        /// <summary>
        /// AddActionAssignedUserAsync; Add assigned user to a action. If the user already exists it will ignore the insert and return the id.
        /// Following stored procedures will be used for database data retrieval: "add_action_assigned_user"
        /// </summary>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <returns>The Id of the inserted record.</returns>
        public async Task<int> AddActionAssignedUserAsync(int actionId, int userId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_actionid", actionId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));

            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_action_assigned_user", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return id;
        }

        /// <summary>
        /// AddActionAssignedAreaAsync; Add assigned area to a action. If the area already exists it will ignore the insert and return the id.
        /// Following stored procedures will be used for database data retrieval: "add_action_assigned_area"
        /// </summary>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <returns>The Id of the inserted record.</returns>
        public async Task<int> AddActionAssignedAreaAsync(int actionId, int areaId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_actionid", actionId));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));

            var id = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_action_assigned_area", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return id;
        }

        /// <summary>
        /// RemoveActionAssignedUserAsync; Remove a assigned user from a action.
        /// Following stored procedures will be used for database data retrieval: "remove_action_assigned_user"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>The count of the deleted record.</returns>
        public async Task<int> RemoveActionAssignedUserAsync(int companyId, int actionId, int userId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_actionid", actionId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var count = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_action_assigned_user", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return count;
        }

        /// <summary>
        /// RemoveActionAssignedAreaAsync; Remove a assigned user from a action.
        /// Following stored procedures will be used for database data retrieval: "remove_action_assigned_area"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <returns>The count of the deleted record.</returns>
        public async Task<int> RemoveActionAssignedAreaAsync(int companyId, int actionId, int areaId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_actionid", actionId));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            var count = Convert.ToInt32(await _manager.ExecuteScalarAsync("remove_action_assigned_area", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return count;
        }

        /// <summary>
        /// SetActionViewedAsync; Sets the viewed status for a user for a certain actionid;
        /// Following stored procedures will be used for database data retrieval: "set_action_viewed"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: action_viewed.action_id)</param>
        /// <param name="userId">UserId (DB: profiles_user.id)</param>
        /// <returns>true / false depending on outcome.</returns>
        public async Task<bool> SetActionViewedAsync(int companyId, int actionId, int userId)
        {

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", actionId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));
            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_action_viewed", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return (rowseffected > 0);

        }
        #endregion

        #region - public methods Actions SAP PM -
        /// <summary>
        /// AddSapPmNotificationAsync; Add a SAP PM Notification, based on an <paramref name="action"/>.
        /// </summary>
        /// <param name="companyId">The company id of the company for this notification</param>
        /// <param name="userId">The user id of the user that created this action/notification</param>
        /// <param name="action">The action that was added (must have id > 0)</param>
        /// <returns>The Id of the added SAP PM Notification (0 if not successful)</returns>
        public async Task<int> AddSapPmNotificationAsync(int companyId, int userId, ActionsAction action)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            action = await ComplementActionRelationDetails(companyId: companyId, action: action);

            parameters.AddRange(GetNpgsqlParametersFromSapPmNotificationConfig(companyId, userId, action));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_action_sap_pm_notification", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.sap_pm_notification.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.sap_pm_notification.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added SAP PM Notification.");

            }

            return possibleId;
        }

        /// <summary>
        /// AddSapPmNotificationAsync; Add a SAP PM Notification, based on an <paramref name="action"/>.
        /// </summary>
        /// <param name="companyId">The company id of the company for this notification</param>
        /// <param name="userId">The user id of the user that created this action/notification</param>
        /// <param name="action">The action that was added (must have id > 0)</param>
        /// <returns>The Id of the added SAP PM Notification (0 if not successful)</returns>
        public async Task<int> ChangeSapPmNotificationAsync(int companyId, int userId, int notificationId, ActionsAction action)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter(@"_id", notificationId));

            action = await ComplementActionRelationDetails(companyId: companyId, action: action);

            parameters.AddRange(GetNpgsqlParametersFromSapPmNotificationConfig(companyId, userId, action));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_action_sap_pm_notification", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.sap_pm_notification.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.sap_pm_notification.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Changed SAP PM Notification.");

            }

            return possibleId;
        }

        /// <summary>
        /// GetSapPmNotificationAsync; Retrieve a SAP PM Notification for a specific action
        /// </summary>
        /// <param name="companyId">The company id of the notification</param>
        /// <param name="actionId">The action id of the related action</param>
        /// <returns>A SAP PM notification related to an action</returns>
        public async Task<SapPmNotification> GetSapPmNotificationAsync(int companyId, int actionId)
        {
            var notification = new SapPmNotification();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", actionId));

                using (dr = await _manager.GetDataReader("get_action_sap_pm_notifications", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillSapPmNotificationFromReader(dr: dr, sapPmNotification: notification);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetSapPmNotificationAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return notification;
        }

        /// <summary>
        /// GetSapPmNotificationsAsync; Retrieve SAP PM Notifications for a specified list of actions
        /// </summary>
        /// <param name="companyId">The company id of the notification</param>
        /// <param name="actionIds">The action ids of the related actions</param>
        /// <returns>A list of SAP PM notifications related to a list of actions</returns>
        public async Task<List<SapPmNotification>> GetSapPmNotificationsAsync(int companyId, List<int> actionIds)
        {
            var notifications = new List<SapPmNotification>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionids", actionIds.ToArray()));

                using (dr = await _manager.GetDataReader("get_actions_sap_pm_notifications", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        SapPmNotification notification = null;
                        notification = CreateOrFillSapPmNotificationFromReader(dr: dr, sapPmNotification: notification);
                        if(notification != null)
                        {
                            notifications.Add(notification);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmManager.GetSapPmNotificationsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return notifications;
        }
        #endregion

        #region - private methods Filter ActionComments -
        /// <summary>
        /// FilterActionComments; FilterActionComments is the primary filter method for filtering ActionComments. Within this method the specific filters are determined based on the supplied ActionFilter object.
        /// Filtering is done based on cascading filters, meaning, the first filter is applied, which results in a filtered collection.
        /// On that filtered collection the second filter is applied which results in a filtered-filtered collection.
        /// This will continue until all filters are applied.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="filters">ActionFilter, depending on the values certain filters will be applies.</param>
        /// <param name="nonFilteredCollection">List of non filtered ActionComment objects.</param>
        /// <returns>A filtered list of ActionComment objects.</returns>
        private async Task<IList<ActionComment>> FilterActionComments(int companyId, ActionFilters filters, IList<ActionComment> nonFilteredCollection)
        {
            var filtered = nonFilteredCollection;
            if (filters.UserId.HasValue)
            {
                filtered = await FilterActionCommentsOnUserId(userId: filters.UserId.Value, actionComments: filtered);
            }
            if (filters.ActionId.HasValue)
            {
                filtered = await FilterActionCommentsOnActionId(actionId: filters.ActionId.Value, actionComments: filtered);
            }
            if(filters.Limit.HasValue && filtered.Count > filters.Limit)
            {
                filtered = filtered.Take(filters.Limit.Value).ToList();
            }
            return filtered;
        }

        /// <summary>
        /// FilterActionCommentsOnUserId; Filter a ActionComment collection on UserId.
        /// </summary>
        /// <param name="userId">UserId; this Id references the UserProfile.Id (DB: actions_actioncomment.user_id)</param>
        /// <param name="actionComments">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<ActionComment>> FilterActionCommentsOnUserId(int userId, IList<ActionComment> actionComments)
        {
            actionComments = actionComments.Where(x => x.UserId  == userId).ToList();
            await Task.CompletedTask; //used for making method async executable.
            return actionComments;
        }

        /// <summary>
        /// FilterActionCommentsOnActionId; Filter a ActionComment collection on ActionId.
        /// </summary>
        /// <param name="actionId">ActionId (DB: actions_actioncomment.action_id)</param>
        /// <param name="actionComments">Collection of items to be filtered.</param>
        /// <returns>A filtered set of items.</returns>
        private async Task<IList<ActionComment>> FilterActionCommentsOnActionId(int actionId, IList<ActionComment> actionComments)
        {
            actionComments = actionComments.Where(x => x.ActionId == actionId).ToList();
            await Task.CompletedTask; //used for making method async executable.
            return actionComments;
        }
        #endregion

        #region - private methods Actions -
        /// <summary>
        /// GetTagsWithAction; Get Tags with an Action based on ActionId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>List of Tags.</returns>
        private async Task<List<Tag>> GetTagsWithActionAsync(int companyId, int actionId)
        {
            var output = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Action, id: actionId);
            if (output != null && output.Count > 0)
            {
                return output;
            }
            return null;
        }

        /// <summary>
        /// AppendTagsToActions; append tags to action collections.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actions">Collection of actions</param>
        /// <returns>Collection of actions</returns>
        private async Task<List<ActionsAction>> AppendTagsToActionsAsync(int companyId, List<ActionsAction> actions)
        {
            var allTagsOnActions = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.Action);
            if (allTagsOnActions != null)
            {
                foreach (var action in actions)
                {
                    var tagsOnThisAction = allTagsOnActions.Where(t => t.ObjectId == action.Id).ToList();
                    if (tagsOnThisAction != null && tagsOnThisAction.Count > 0)
                    {
                        action.Tags ??= new List<Tag>();
                        action.Tags.AddRange(tagsOnThisAction);
                    }

                }
            }

            return actions;
        }

        /// <summary>
        /// AppendParentsToActions; append parents to action collections.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actions">Collection of actions</param>
        /// <returns>Collection of actions</returns>
        private async Task<List<ActionsAction>> AppendParentsToActions(int companyId, List<ActionsAction> actions)
        {
            var parents = await GetParentsBasicWithActionsAsync(companyId: companyId, actionIds: actions.Select(action => action.Id).ToList());
            if(parents != null)
            {
                foreach(var action in actions)
                {
                    var parent = parents.Where(x => x.ActionId == action.Id).FirstOrDefault();
                    if(parent != null)
                    {
                        action.Parent = parent;
                    }

                }
            }

            return actions;
        }

        /// <summary>
        /// AppendAssignedUsersToAction;
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private async Task<List<ActionsAction>> AppendAssignedUsersToActions(int companyId, List<ActionsAction> actions)
        {
            await Task.CompletedTask;
            //TODO add users
            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        private async Task<List<ActionsAction>> AppendAssignedAreasToActions(int companyId, List<ActionsAction> actions)
        {
            await Task.CompletedTask;
            //TODO add areas
            return actions;
        }

        /// <summary>
        /// GetUnviewedStatisticsWithActions; Get unviewed comment statistics with action.
        /// </summary>
        /// <param name="actions">Collection of actions to be checked</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserProfile Id (the current user that is making the request)</param>
        /// <returns>The list of actions with unviewed statistics appended.</returns>
        private async Task<List<ActionsAction>> GetUnviewedStatisticsWithActions(List<ActionsAction> actions, int companyId, int userId)
        {
            var commentStats = await GetActionCommentViewedStatistics(companyId: companyId, userId: userId);
            if (commentStats != null && commentStats.Count > 0)
            {
                foreach (var action in actions)
                {
                    var foundStats = commentStats.Where(x => x.ActionId == action.Id).FirstOrDefault();
                    if (foundStats != null)
                    {
                        action.UnviewedCommentNr = foundStats.CommentsNotViewedNr;
                    }
                    else
                    {
                        action.UnviewedCommentNr = 0; //no statistic available, so no comments...
                    }
                }
            }

            return actions;
        }

        /// <summary>
        /// GetUnviewedStatisticsWithActions; Get unviewed comment statistics with action.
        /// </summary>
        /// <param name="action">Collection of actions to be checked</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserProfile Id (the current user that is making the request)</param>
        /// <returns>The list of actions with unviewed statistics appended.</returns>
        private async Task<int?> GetUnviewedStatisticsWithAction(int actionId, int companyId, int userId)
        {
            var commentStats = await GetActionCommentViewedStatisticsForAction(companyId: companyId, userId: userId, actionId: actionId);

            return commentStats.CommentsNotViewedNr;
        }

        /// <summary>
        /// GetActionsSourceStoredProcedureBasedOnFilter; Gets the stored procedure to be executed based on the filters that are supplied.
        /// By default the normal actions are used, but if a AssignedUser or AssignedArea filter is supplied other stored procedures will be executed based on those filters.
        /// </summary>
        /// <param name="actionFilters">ActionFilters that need to be used.</param>
        /// <returns>A collection of Actions</returns>
        private string GetActionsSourceStoredProcedureBasedOnFilter(ActionFilters? actionFilters)
        {
            if(actionFilters.HasValue)
            {
                if (actionFilters.Value.AssignedUserId.HasValue && actionFilters.Value.CreatedById.HasValue)
                {
                    return "get_actions_by_created_by_assigned_user";
                }

                if (actionFilters.Value.AssignedUserId.HasValue && actionFilters.Value.AssignedAreaId.HasValue)
                {
                    return "get_actions_by_assigneduser_and_assignedarea";
                }

                if (actionFilters.Value.AssignedUserId.HasValue )
                {
                    return "get_actions_by_assigneduser";
                }

                if (actionFilters.Value.AssignedAreaId.HasValue)
                {
                    return "get_actions_by_assignedarea";
                }


            }
            return "get_actions"; //always return get actions unles AssignedUser or AssignedArea filters are used;
        }

        /// <summary>
        /// GetUsersWithActionAsync; Gets a list of users (basic model) for use within collections of the ProfilesUser object.
        /// The users are based on a Id list that is queried from the database and compared to the full list of users.
        /// Normally this could be queried directly but seeing the cashing system for UserProfiles is/will be pretty efficient we don't need al that data from the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>A list of user (basic) objects.</returns>
        private async Task<List<UserBasic>> GetUsersWithActionAsync(int companyId, int actionId)
        {
            var usersWithAction = new List<UserBasic>();
            var usersWithCompany = await _userManager.GetUserProfilesAsync(companyId: companyId);
            List<int> userIdsWithAction = await GetAssignedUserIdsWithActionAsync(companyId: companyId, actionId: actionId);

            if(usersWithCompany != null && usersWithCompany.Any() && userIdsWithAction != null && userIdsWithAction.Any())
            {
                foreach (var item in usersWithCompany.Where(x => userIdsWithAction.Contains(x.Id)))
                {
                    //TODO add checks
                    //TODO move to extension
                    usersWithAction.Add(new UserBasic() { Id = item.Id, Name = string.Concat(item.FirstName, " ", item.LastName), Picture = item.Picture });
                }
            }

            return usersWithAction;
        }

        /// <summary>
        /// GetUserWithActionAsync; 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<UserBasic> GetUserWithActionAsync(int companyId, int userId)
        {
            if(companyId > 0 && userId > 0)
            {
                return await _userManager.GetUserBasicAsync(companyId: companyId, userId: userId);
            }
            return null;
        }

        /// <summary>
        /// GetUserWithActionAsync; 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        private async Task<List<ActionsAction>> GetUserWithActionsAsync(int companyId, List<ActionsAction> actions)
        {
            if (companyId > 0 && actions != null && actions.Count > 0)
            {
                var users = await _userManager.GetUsersBasicAsync(companyId);
                foreach(var action in actions)
                {
                    var possibleUser = users.Where(x => x.Id == action.CreatedById).FirstOrDefault();
                    if(possibleUser != null)
                    {
                        action.CreatedByUser = possibleUser;
                    }
                }
            }
            return actions;
        }

        /// <summary>
        /// Asynchronously associates user information with each action comment for a specified company.
        /// </summary>
        /// <remarks>This method retrieves basic user information for the specified company and updates
        /// each action comment with the corresponding user details based on the user ID.</remarks>
        /// <param name="companyId">The identifier of the company whose users are to be retrieved. Must be greater than zero.</param>
        /// <param name="comments">A list of action comments to be updated with user information. Cannot be null or empty.</param>
        /// <returns>A task representing the asynchronous operation, containing the list of action comments with associated user
        /// information.</returns>
        private async Task<List<ActionComment>> GetUserWithActionCommentsAsync(int companyId, List<ActionComment> comments)
        {
            if (companyId > 0 && comments != null && comments.Count > 0)
            {
                var users = await _userManager.GetUsersBasicAsync(companyId);
                foreach (ActionComment comment in comments)
                {
                    var possibleUser = users.Where(x => x.Id == comment.UserId).FirstOrDefault();
                    if (possibleUser != null)
                    {
                        comment.CreatedByUser = possibleUser;
                    }
                }
            }
            return comments;
        }

        /// <summary>
        /// GetAreasWithActionAsync; Gets a list of areas (basic model) for use within collections of the ProfilesUser object.
        /// The areas are based on a Id list that is queried from the database and compared to the full list of areas.
        /// Normally this could be queried directly but seeing the cashing system for Areas is/will be pretty efficient we don't need all that data from the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>A list of user (basic) objects.</returns>
        private async Task<List<AreaBasic>> GetAreasWithActionAsync(int companyId, int actionId)
        {
            var areasWithAction = new List<AreaBasic>();
            var areasWithCompany = await _areaManager.GetAreasAsync(companyId: companyId, maxLevel: 100, useTreeview: false);
            List<int> areaIdsWithAction = await GetAssignedAreaIdsWithActionAsync(companyId: companyId, actionId: actionId);

            if(areasWithCompany !=null && areasWithCompany.Any() && areaIdsWithAction != null && areaIdsWithAction.Any())
            {
                foreach (var item in areasWithCompany.Where(x => areaIdsWithAction.Contains(x.Id)))
                {
                    //TODO add checks
                    //TODO move to extension
                    areasWithAction.Add(new AreaBasic() { Id = item.Id, Name = item.Name, ParentId = item.ParentId, NamePath = item.FullDisplayName });
                }
            }
  
            return areasWithAction;
        }

        /// <summary>
        /// GetParentBasicWithAction; Get Parent object containing basic data, based on company and actionid
        /// Following stored procedures will be used for database data retrieval: "get_action_parent"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>ActionParentBasic</returns>
        private async Task<ActionParentBasic> GetParentBasicWithActionAsync(int companyId, int actionId)
        {
            var parent = new ActionParentBasic();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", actionId));

                using (dr = await _manager.GetDataReader("get_action_parent", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    //NOTE SHOULD ONLY RETURN 1 ITEM, if not pick the last one.
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillActionParentBasicFromReader(dr: dr, actionparentbasic: parent);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetParentBasicWithActionAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return parent;
        }

        /// <summary>
        /// GetParentsBasicWithActionsAsync; Get parent object for actions for use within other logic.
        /// Following stored procedures will be used for database data retrieval: "get_action_parents"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of action parents.</returns>
        private async Task<List<ActionParentBasic>> GetParentsBasicWithActionsAsync(int companyId, List<int> actionIds = null)
        {
            List<ActionParentBasic> ls;

            NpgsqlDataReader dr = null;

            string actionIdsString = actionIds != null && actionIds.Any() ? string.Join(',', actionIds) : null;

            var cacheKey = CacheHelpers.GenerateCacheKey(CacheSettings.CacheKeyActionParent, companyId, actionIdsString);

            if (_cache.TryGetValue(cacheKey, out ls))
            {
                if (ls != null && ls.Any() && ls.Count > 0) return ls;
            }

            try
            {
                if(ls == null) { ls = new List<ActionParentBasic>(); }

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if (actionIds != null && actionIds.Any())
                    parameters.Add(new NpgsqlParameter("@_actionids", actionIds));

                using (dr = await _manager.GetDataReader("get_action_parents", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {

                        ls.Add (CreateOrFillActionParentBasicFromReader(dr));
                    }
                }

                _cache.Set(cacheKey, ls, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSettings.CacheTimeDefaultInSeconds) });

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetParentsBasicWithActionsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return ls;
        }

        /// <summary>
        /// GetAssignedUserIdsWithActionAsync; Gets a list of Ids from the database for further use within the manager.
        /// Following stored procedures will be used for database data retrieval: "get_assigneduserids_by_action"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>A List of Id's (int)</returns>
        private async Task<List<int>> GetAssignedUserIdsWithActionAsync(int companyId, int actionId)
        {
            var ids = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", actionId));

                using (dr = await _manager.GetDataReader("get_assigneduserids_by_action", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetAssignedUserIdsWithActionAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return ids;
        }

        /// <summary>
        /// GetAssignedAreaIdsWithActionAsync; Gets a list of Ids from the database for further use within the manager.
        /// Following stored procedures will be used for database data retrieval: "get_assignedareaids_by_action"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>A List of Id's (int)</returns>
        private async Task<List<int>> GetAssignedAreaIdsWithActionAsync(int companyId, int actionId)
        {
            var ids = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", actionId));

                using (dr = await _manager.GetDataReader("get_assignedareaids_by_action", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetAssignedAreaIdsWithActionAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return ids;
        }

        /// <summary>
        /// UpdateRelationsWithAction; Update the relations (areas, users) with in action. Based on supplied data, users and/or areas will be removed or added.
        /// </summary>
        /// <param name="action">The action where the users and or areas need to be updated.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> UpdateRelationsWithAction(int companyId, int userId, ActionsAction action)
        {
            var areaResult = await UpdateLinkedActionArea(companyId, action);

            //TODO refactor split up in add, remove methods for each relation.
            if (action.AssignedUsers != null)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action_assigned_users.ToString(), Models.Enumerations.TableFields.action_id.ToString(), action.Id);

                var currentUsers = await GetUsersWithActionAsync(companyId: action.CompanyId, actionId: action.Id);
                if (currentUsers != null && currentUsers.Count > 0)
                {
                    var usersToBeRemoved = currentUsers.Where(x => !action.AssignedUsers.Select(y => y.Id).ToList().Contains(x.Id)).ToList();
                    //Remove users, that are not available in the AssignedUsers anymore.
                    if (usersToBeRemoved != null && usersToBeRemoved.Count > 0)
                    {
                        foreach (var user in usersToBeRemoved)
                        {
                            await RemoveActionAssignedUserAsync(companyId: action.CompanyId, userId: user.Id, actionId: action.Id);
                        }
                    }
                }

                //Add users (not if relation in db already exists it will be ignored on execution within the SP)
                if (action.AssignedUsers.Count > 0)
                {
                    foreach (var user in action.AssignedUsers)
                    {
                        await AddActionAssignedUserAsync(userId: user.Id, actionId: action.Id);
                    }
                }

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action_assigned_users.ToString(), Models.Enumerations.TableFields.action_id.ToString(), action.Id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action_assigned_users.ToString(), objectId: action.Id, userId: userId, companyId: companyId, description: "Changed action user relation collection.");

            }

            if (action.AssignedAreas != null)
            {
                var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action_assigned_areas.ToString(), Models.Enumerations.TableFields.action_id.ToString(), action.Id);

                var currentAreas = await GetAreasWithActionAsync(companyId: action.CompanyId, actionId: action.Id);
                if (currentAreas != null && currentAreas.Count > 0)
                {
                    var areasToBeRemoved = currentAreas.Where(x => !action.AssignedAreas.Select(y => y.Id).ToList().Contains(x.Id)).ToList();
                    //Remove users, that are not available in the AssignedAreas anymore.
                    if (areasToBeRemoved != null && areasToBeRemoved.Count > 0)
                    {
                        foreach (var area in areasToBeRemoved)
                        {
                            await RemoveActionAssignedAreaAsync(companyId: action.CompanyId, areaId: area.Id, actionId: action.Id);
                        }
                    }
                }

                //Add areas (not if relation in db already exists it will be ignored on execution within the SP)
                if (action.AssignedAreas != null && action.AssignedAreas.Count > 0)
                {
                    foreach (var area in action.AssignedAreas)
                    {
                        await AddActionAssignedAreaAsync(areaId: area.Id, actionId: action.Id);
                    }
                }

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.actions_action_assigned_areas.ToString(), Models.Enumerations.TableFields.action_id.ToString(), action.Id);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.actions_action_assigned_areas.ToString(), objectId: action.Id, userId: userId, companyId: companyId, description: "Changed action area relation collection.");
            }

            action.Tags ??= new();
            await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.Action, action.Id, action.Tags, companyId, userId);

            return true; //TODO when adding checks change this.
        }

        private async Task<int> UpdateLinkedActionArea(int companyId, ActionsAction action)
        {
            //get linked parent area
            var possibleLinkedAreaId = await GetPossibleLinkedArea(companyId: companyId, taskId: action.TaskId.HasValue ? action.TaskId.Value : 0, taskTemplateId: action.TaskTemplateId.HasValue ? action.TaskTemplateId.Value : 0);

            //get action area relation(s)
            var actionAreaRelations = await GetActionAreaRelations(companyId: companyId, actionId: action.Id);

            //determine inactive relations that should be removed
            var actionAreasToBeRemoved = actionAreaRelations.Where(a => a.AreaId != possibleLinkedAreaId).ToList();

            //remove inactive relation(s)
            foreach (var actionAreaToBeRemoved in actionAreasToBeRemoved)
            {
                var success = await RemoveActionAreaRelationAsync(companyId: companyId, actionId: actionAreaToBeRemoved.ActionId, areaId: actionAreaToBeRemoved.AreaId);
            }

            //add active relation if area id > 0
            if (possibleLinkedAreaId > 0 && actionAreaRelations.Where(a => a.AreaId == possibleLinkedAreaId).Count() == 0)
            {
                return await AddActionAreaRelationAsync(actionId: action.Id, areaId: possibleLinkedAreaId);
            }

            return 0;
        }

        /// <summary>
        /// AddLinkedAreasToCollection; Add possible linked areas to task area collection.
        /// </summary>
        /// <param name="action">Action to be updated if needed.</param>
        /// <returns>Updated action.</returns>
        private async Task<ActionsAction> AddLinkedAreasToCollection(int companyId, ActionsAction action)
        {
            if((action.TaskId.HasValue && action.TaskId.Value > 0) || (action.TaskTemplateId.HasValue && action.TaskTemplateId.Value > 0))
            {
                var possibleLinkedAreaId = await GetPossibleLinkedArea(companyId: companyId, taskId: action.TaskId.HasValue ? action.TaskId.Value : 0, taskTemplateId: action.TaskTemplateId.HasValue ? action.TaskTemplateId.Value : 0);
                if(possibleLinkedAreaId > 0)
                {
                    if (action.AssignedAreas == null) action.AssignedAreas = new List<AreaBasic>(); //if not existing, add
                    //check if item isn't already added.
                    if(!action.AssignedAreas.Where(x => x.Id == possibleLinkedAreaId).Any())
                    {
                        action.AssignedAreas.Add(new AreaBasic() {Id = possibleLinkedAreaId});
                    }
                }
            }
            return action;
        }

        /// <summary>
        /// Retrieve possible linked area id through template and or task id
        /// </summary>
        /// <param name="taskId">Possible task id</param>
        /// <param name="taskTemplateId">Possible template id</param>
        /// <returns></returns>
        private async Task<int> GetPossibleLinkedArea(int companyId, int taskId, int taskTemplateId)
        {
            var output = 0;

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_taskid", taskId));
            parameters.Add(new NpgsqlParameter("@_tasktemplateid", taskTemplateId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            try
            {
                var result = await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_linked_area_with_task", parameters: parameters, connectionKind: ConnectionKind.Reader);
                if (result != null)
                {
                    output = (int)result;
                }

            }
            catch (Exception ex)
            {
                this.Exceptions.Add(ex);
            }
          
            return output;
        }

        /// <summary>
        /// Retrieve possible linked area id through template and or task id
        /// </summary>
        /// <param name="taskId">Possible task id</param>
        /// <param name="taskTemplateId">Possible template id</param>
        /// <returns></returns>
        private async Task<List<ActionAreaRelation>> GetActionAreaRelations(int companyId, int actionId)
        {
            var output = new List<ActionAreaRelation>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", actionId));

                using (dr = await _manager.GetDataReader("get_action_area_relations", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var actionAreaRelation = CreateOrFillActionAreaRelationFromReader(dr);
                        if (actionAreaRelation != null)
                        {
                            output.Add(actionAreaRelation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Exceptions.Add(ex);
            }

            return output;
        }

        private async Task<int> AddActionAreaRelationAsync(int actionId, int areaId)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_actionid", actionId));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));

            var relationId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_action_area", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            return relationId;
        }

        private async Task<bool> RemoveActionAreaRelationAsync(int companyId, int actionId, int areaId)
        {
            var output = 0;

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_actionid", actionId));
            parameters.Add(new NpgsqlParameter("@_areaid", areaId));

            try
            {
                var result = await _manager.ExecuteScalarAsync(procedureNameOrQuery: "remove_action_area", parameters: parameters, connectionKind: ConnectionKind.Writer);
                if (result != null)
                {
                    output = (int)result;
                }

            }
            catch (Exception ex)
            {
                this.Exceptions.Add(ex);
            }

            return output > 0;
        }

        private ActionAreaRelation CreateOrFillActionAreaRelationFromReader(NpgsqlDataReader dr, ActionAreaRelation actionAreaRelation = null)
        {
            if (actionAreaRelation == null) actionAreaRelation = new ActionAreaRelation();

            actionAreaRelation.Id = Convert.ToInt32(dr["id"]);
            actionAreaRelation.ActionId = Convert.ToInt32(dr["action_id"]);
            actionAreaRelation.AreaId = Convert.ToInt32(dr["area_id"]);

            return actionAreaRelation;
        }


        /// <summary>
        /// CreateOrFillActionFromReader; creates and fills a ActionsAction object from a DataReader.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="action">Action object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled ActionsAction object.</returns>
        private ActionsAction CreateOrFillActionFromReader(NpgsqlDataReader dr, ActionsAction action = null)
        {
            if (action == null) action = new ActionsAction();

            if (dr["comment"] != DBNull.Value && !string.IsNullOrEmpty(dr["comment"].ToString()))
            {
                action.Comment = dr["comment"].ToString();
            }
            action.CommentCount = Convert.ToInt32(dr["commentnr"]);
            action.CompanyId = Convert.ToInt32(dr["company_id"]);
            action.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            action.CreatedBy = dr["createdby"].ToString();
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                action.Description = dr["description"].ToString();
            }
            action.DueDate = Convert.ToDateTime(dr["due_date"]);
            action.Id = Convert.ToInt32(dr["id"]);
            if (dr["is_resolved"] != DBNull.Value)
            {
                action.IsResolved = Convert.ToBoolean(dr["is_resolved"]);
            }
            if (dr["resolved_at"] != DBNull.Value)
            {
                action.ResolvedAt = Convert.ToDateTime(dr["resolved_at"]);
            }
            if (dr["task_id"] != DBNull.Value)
            {
                action.TaskId = Convert.ToInt32(dr["task_id"]);
            }
            if (dr["task_template_id"] != DBNull.Value)
            {
                action.TaskTemplateId = Convert.ToInt32(dr["task_template_id"]);
            }
            if (dr.HasColumn("lastcommentdate")) {
                if(dr["lastcommentdate"] != DBNull.Value)
                {
                    action.LastCommentDate = Convert.ToDateTime(dr["lastcommentdate"]);
                }
            }
            if (dr.HasColumn("priority")) {
                if(dr["priority"] != DBNull.Value)
                {
                    action.Priority = (ActionPriorityEnum)Convert.ToInt32(dr["priority"]);
                }
            }
            if (dr.HasColumn("unviewedcommentnr"))
            {
                action.UnviewedCommentNr = Convert.ToInt32(dr["unviewedcommentnr"]);
            }
            for (var i = 0; i < 6; i++)
            {
                var imagekey = string.Concat("image_", i.ToString());
                var videokey = string.Concat("video_", i.ToString());
                var videothumbkey = string.Concat("video_thumbnail_", i.ToString());

                if (dr[videokey] != DBNull.Value && !string.IsNullOrEmpty(dr[videokey].ToString()))
                {
                    if (action.Videos == null) action.Videos = new List<string>();
                    action.Videos.Add(dr[videokey].ToString());
                }
                if (dr[videothumbkey] != DBNull.Value && !string.IsNullOrEmpty(dr[videothumbkey].ToString()))
                {
                    if (action.VideoThumbNails == null) action.VideoThumbNails = new List<string>();
                    action.VideoThumbNails.Add(dr[videothumbkey].ToString());
                }
                if (dr[imagekey] != DBNull.Value && !string.IsNullOrEmpty(dr[imagekey].ToString()))
                {
                    if (action.Images == null) action.Images = new List<string>();
                    action.Images.Add(dr[imagekey].ToString());
                }
            }
            if (dr["created_at"] != DBNull.Value)
            {
                action.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                action.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }

            return action;
        }

        /// <summary>
        /// GetNpgsqlParametersFromAction; Creates a list of NpgsqlParameter, and fills it based on the supplied ActionsAction object.
        /// NOTE! intended for use with the action stored procedures within the database.
        /// </summary>
        /// <param name="action">The supplied ActionsAction object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromAction(ActionsAction action, int companyId, int actionId = 0)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (actionId > 0) parameters.Add(new NpgsqlParameter("@_id", actionId));

            if (action.DueDate.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_duedate", new DateTime(action.DueDate.Value.Year, action.DueDate.Value.Month, action.DueDate.Value.Day)));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_duedate", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_comment", action.Comment));
            parameters.Add(new NpgsqlParameter("@_description", action.Description));

            var numberOfImages = (action.Images != null) ? action.Images.Count : 0;
            for (var i = 0; i < 6; i++)
            {
                if (i < numberOfImages)
                {
                    var currentImg = action.Images[i];
                    if(currentImg.StartsWith("media/"))
                    {
                        currentImg = currentImg.Replace("media/", "");
                    }
                    parameters.Add(new NpgsqlParameter(string.Concat("@_image", i.ToString()), currentImg));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter(string.Concat("@_image", i.ToString()), ""));
                }
            }

            parameters.Add(new NpgsqlParameter("@_createdbyid", action.CreatedById));

            if (action.TaskId.HasValue && action.TaskId.Value != 0)
            {
                parameters.Add(new NpgsqlParameter("@_taskid", action.TaskId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_taskid", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            if (action.TaskTemplateId.HasValue && action.TaskTemplateId.Value != 0)
            {
                parameters.Add(new NpgsqlParameter("@_tasktemplateid", action.TaskTemplateId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_tasktemplateid", DBNull.Value));
            }

            var numberOfVideos = (action.Videos != null) ? action.Videos.Count : 0;
            for (var i = 0; i < 6; i++)
            {
                if (i < numberOfVideos)
                {
                    parameters.Add(new NpgsqlParameter(string.Concat("@_video", i.ToString()), action.Videos[i]));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter(string.Concat("@_video", i.ToString()), ""));
                }
            }

            var numberOfVideoThumbnails = (action.VideoThumbNails != null) ? action.VideoThumbNails.Count : 0;
            for (var i = 0; i < 6; i++)
            {
                if (i < numberOfVideoThumbnails)
                {
                    parameters.Add(new NpgsqlParameter(string.Concat("@_videothumbnail", i.ToString()), action.VideoThumbNails[i]));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter(string.Concat("@_videothumbnail", i.ToString()), ""));
                }
            }

            return parameters;
        }

        #endregion

        #region - private methods ActionComments -
        /// <summary>
        /// GetCommentsWithAction; Get Comments with an Action based on ActionId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <returns>List of ActionComments.</returns>
        private async Task<List<ActionComment>> GetCommentsWithAction(int companyId, int actionId)
        {
            var output = await GetActionCommentsByActionIdAsync(companyId: companyId, actionId: actionId);
            if (output != null && output.Count > 0)
            {
                return output;
            }
            return null;
        }

        /// <summary>
        /// GetViewedByUsersWithComment; Get users based on the ViewedBy functionality.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="commentId">CommentId</param>
        /// <returns>A list of basic users.</returns>
        private async Task<List<UserBasic>> GetViewedByUsersWithComment(int companyId, int commentId)
        {
            var users = new List<UserBasic>();
            var usersWithCompany = await _userManager.GetUserProfilesAsync(companyId: companyId);
            List<int> viewedUserIds = await GetViewedUserIdsWithComment(companyId: companyId, commentId: commentId);

            foreach (var item in usersWithCompany.Where(x => viewedUserIds.Contains(x.Id)))
            {
                //TODO add checks
                //TODO move to extension
                users.Add(new UserBasic() { Id = item.Id, Name = string.Concat(item.FirstName, " ", item.LastName), Picture = item.Picture });
            }

            return users;
        }

        /// <summary>
        /// GetViewedUserIdsWithComment; Get a list of user ids that viewed a certain comment. Will be used for further processing.
        /// Following stored procedures will be used for database data retrieval: "get_viewedcommentuserids_by_comment"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="commentId">CommentId</param>
        /// <returns></returns>
        private async Task<List<int>> GetViewedUserIdsWithComment(int companyId, int commentId)
        {
            var ids = new List<int>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_commentid", commentId));

                using (dr = await _manager.GetDataReader("get_viewedcommentuserids_by_comment", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        ids.Add(Convert.ToInt32(dr[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetViewedUserIdsWithComment(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return ids;
        }

        /// <summary>
        /// GetActionCommentViewedStatistics; Get a list of statistics containing data for comments and user interaction.
        /// Following stored procedures will be used for database data retrieval: "get_actioncomments_viewed_statistics_with_user"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">User where the stats need to be generated.</param>
        /// <returns></returns>
        private async Task<List<ActionCommentViewedStatsItem>> GetActionCommentViewedStatistics(int companyId, int userId)
        {
            var stats = new List<ActionCommentViewedStatsItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                using (dr = await _manager.GetDataReader("get_actioncomments_viewed_statistics_with_user", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var statsItem = CreateActionCommentViewedStatsItemFromReader(dr);
                        stats.Add(statsItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCommentViewedStatistics(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return stats;
        }

        /// <summary>
        /// GetActionCommentViewedStatisticsForAction; Get statistics containing data for comments and user interaction for one action.
        /// Following stored procedures will be used for database data retrieval: "get_actioncomments_viewed_statistics_with_user_and_action"
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">User where the stats need to be generated.</param>
        /// <param name="actionId">Action to get the related stats for</param>
        /// <returns></returns>
        private async Task<ActionCommentViewedStatsItem> GetActionCommentViewedStatisticsForAction(int companyId, int userId, int actionId)
        {
            var stats = new ActionCommentViewedStatsItem();

            try
            {
                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_companyid", companyId),
                    new NpgsqlParameter("@_userid", userId),
                    new NpgsqlParameter("@_actionid", actionId)
                };

                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_actioncomments_viewed_statistics_with_user_and_action", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    stats = new ActionCommentViewedStatsItem()
                    {
                        ActionId = Convert.ToInt32(dr["action_id"]),
                        CommentsNotViewedNr = Convert.ToInt32(dr["comment_unviewed_count"]),
                        CommentsTotalNr = Convert.ToInt32(dr["comment_count"]),
                        CommentsViewedNr = Convert.ToInt32(dr["comment_viewed_count"])
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCommentViewedStatisticsForAction(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return stats;
        }


        private Dictionary<string, string> GetSystemActionCommentTranslations()
        {
            return _systemActionCommentTranslations ??= GetActionCommentTranslationsAsync().Result;
        }

        private async Task<Dictionary<string, string>> GetActionCommentTranslationsAsync()
        {
            Dictionary<string, string> output = new();
            try
            {
                if (string.IsNullOrEmpty(Culture) || Culture == "en_en")
                {
                    Culture = "en_us";
                }
                else
                {
                    string activeCultures = await _toolsManager.GetSupportedLanguages();
                    if (!activeCultures.Contains(Culture))
                    {
                        Culture = "en_us"; //default to english if language is not active.
                    }
                }

                List<NpgsqlParameter> parameters = new()
                {
                    new NpgsqlParameter("@_culture", Culture)
                };

                await using NpgsqlDataReader dr = await _manager.GetDataReader("get_resource_language_actions_actioncomments", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters);
                while (await dr.ReadAsync())
                {
                    output.Add(dr["resource_key"].ToString(), dr["resource_value"].ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.GetActionCommentTranslationsAsync(): ", ex.Message));
            }
            return output;
        }

        /// <summary>
        /// CreateOrFillActionCommentFromReader; creates and fills a ActionComment object from a DataReader.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="actioncomment">ActionComment object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled ActionComment object.</returns>
        private ActionComment CreateOrFillActionCommentFromReader(NpgsqlDataReader dr, ActionComment actioncomment = null) {
            if (actioncomment == null) actioncomment = new ActionComment();

            actioncomment.ActionId = Convert.ToInt32(dr["action_id"]);
            if (dr["comment"] != DBNull.Value && !string.IsNullOrEmpty(dr["comment"].ToString()))
            {
                actioncomment.Comment = dr["comment"].ToString();
            }
            actioncomment.Id = Convert.ToInt32(dr["id"]);
            if (dr["user_id"] != DBNull.Value)
            {
                actioncomment.UserId = Convert.ToInt32(dr["user_id"]);
            }
            if (dr["video"] != DBNull.Value && !string.IsNullOrEmpty(dr["video"].ToString()))
            {
                actioncomment.Video = dr["video"].ToString();
            }
            if (dr["video_thumbnail"] != DBNull.Value && !string.IsNullOrEmpty(dr["video_thumbnail"].ToString()))
            {
                actioncomment.VideoThumbnail = dr["video_thumbnail"].ToString();
            }
            for (var i = 0; i < 5; i++)
            {
                var imagekey = string.Concat("image_", i.ToString());
                if (dr.HasColumn(imagekey) && dr[imagekey] != DBNull.Value && !string.IsNullOrEmpty(dr[imagekey].ToString()))
                {
                    if (actioncomment.Images == null) actioncomment.Images = new List<string>();
                    actioncomment.Images.Add(dr[imagekey].ToString());
                }
            }
            if (dr["created_at"] != DBNull.Value)
            {
                actioncomment.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            }
            if (dr["modified_at"] != DBNull.Value)
            {
                actioncomment.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            }
            actioncomment.CreatedBy = dr["createdby"].ToString();

            return actioncomment;
        }

        /// <summary>
        /// CreateOrFillActionParentBasicFromReader; Create or fill a action parent item based on a datareader.
        /// </summary>
        /// <param name="dr">Datareader containing the information</param>
        /// <param name="actionparentbasic">ActionParentBasic item, if not supplied it will be created.</param>
        /// <returns>A filled ActionParentBasic object.</returns>
        private ActionParentBasic CreateOrFillActionParentBasicFromReader(NpgsqlDataReader dr, ActionParentBasic actionparentbasic = null)
        {
            if (actionparentbasic == null) actionparentbasic = new ActionParentBasic();

            if (dr["id"] != DBNull.Value)
            {
                actionparentbasic.ActionId = Convert.ToInt32(dr["id"]);
            }
            if (dr["audit_id"] != DBNull.Value)
            {
                actionparentbasic.AuditId = Convert.ToInt32(dr["audit_id"]);
            }
            if (dr["audittemplate_id"] != DBNull.Value)
            {
                actionparentbasic.AuditTemplateId = Convert.ToInt32(dr["audittemplate_id"]);
            }
            if (dr["audittemplate_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["audittemplate_name"].ToString()))
            {
                actionparentbasic.AuditTemplateName = dr["audittemplate_name"].ToString();
            }
            if (dr["checklist_id"] != DBNull.Value)
            {
                actionparentbasic.ChecklistId = Convert.ToInt32(dr["checklist_id"]);
            }
            if (dr["checklisttemplate_id"] != DBNull.Value)
            {
                actionparentbasic.ChecklistTemplateId = Convert.ToInt32(dr["checklisttemplate_id"]);
            }
            if (dr["checklisttemplate_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["checklisttemplate_name"].ToString()))
            {
                actionparentbasic.ChecklistTemplateName = dr["checklisttemplate_name"].ToString();
            }
            if (dr["task_id"] != DBNull.Value)
            {
                actionparentbasic.TaskId = Convert.ToInt32(dr["task_id"]);
            }

            if (dr["task_name"] != DBNull.Value && !string.IsNullOrEmpty(dr["task_name"].ToString()))
            {
                actionparentbasic.TaskName = dr["task_name"].ToString();
            }
            if (dr["task_template_id"] != DBNull.Value)
            {
                actionparentbasic.TaskTemplateId = Convert.ToInt32(dr["task_template_id"]);
            }

            if (dr["type"] != DBNull.Value && !string.IsNullOrEmpty(dr["type"].ToString()))
            {
                actionparentbasic.Type = dr["type"].ToString();
            }

            return actionparentbasic;
        }

        private ActionCommentViewedStatsItem CreateActionCommentViewedStatsItemFromReader(NpgsqlDataReader dr)
        {
            ActionCommentViewedStatsItem statsItem = new()
            {
                ActionId = Convert.ToInt32(dr["action_id"]),
                CommentsNotViewedNr = Convert.ToInt32(dr["comment_unviewed_count"]),
                CommentsTotalNr = Convert.ToInt32(dr["comment_count"]),
                CommentsViewedNr = Convert.ToInt32(dr["comment_viewed_count"])
            };
            return statsItem;
        }

        /// <summary>
        /// GetNpgsqlParametersFromActionComment; Creates a list of NpgsqlParameter, and fills it based on the supplied ActionComment object.
        /// NOTE! intended for use with the action comment stored procedures within the database.
        /// </summary>
        /// <param name="actionComment">The supplied ActionComment object, containing all data.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionCommentId">ActionCommentId (DB: actions_actioncomment.id)</param>
        /// <returns>A list of NpgsqlParameter parameters.</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromActionComment(ActionComment actionComment, int companyId, int actionCommentId = 0) {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (actionCommentId > 0) parameters.Add(new NpgsqlParameter("@_id", actionCommentId));

            parameters.Add(new NpgsqlParameter("@_comment", actionComment.Comment));

            var numberOfImages = (actionComment.Images != null) ? actionComment.Images.Count : 0;
            for (var i = 0; i < 5; i++)
            {
                if (i < numberOfImages)
                {
                    var currentImg = actionComment.Images[i];
                    if (currentImg.StartsWith("media/"))
                    {
                        currentImg = currentImg.Replace("media/", "");
                    }
                    parameters.Add(new NpgsqlParameter(string.Concat("@_image", i.ToString()), currentImg));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter(string.Concat("@_image", i.ToString()), ""));
                }
            }

            parameters.Add(new NpgsqlParameter("@_actionid", actionComment.ActionId));
            parameters.Add(new NpgsqlParameter("@_userid", actionComment.UserId));
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            if (!string.IsNullOrEmpty(actionComment.Video))
            {
                parameters.Add(new NpgsqlParameter("@_video", actionComment.Video));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_video", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(actionComment.VideoThumbnail))
            {
                parameters.Add(new NpgsqlParameter("@_videothumbnail", actionComment.VideoThumbnail));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_videothumbnail", DBNull.Value));
            }

            return parameters;
        }
        #endregion

        #region - private methods ExternalRelations -


        private ExternalRelation CreateOrFillExternalRelationFromReader(NpgsqlDataReader dr, ExternalRelation externalRelation)
        {
            //returns a table
            //"id" int4,
            //"object_type" varchar,
            //"object_id" int4,
            //"external_id" int4,
            //"status" varchar,
            //"status_message" text,
            //"connector_type" varchar,
            //"company_id" int4,
            //"created_at" timestamp,
            //"modified_at" timestamp,
            //"created_by_id" int4,
            //"modified_by_id" int4

            if (externalRelation == null) externalRelation = new ExternalRelation();


            externalRelation.Id = Convert.ToInt32(dr["id"]);

            externalRelation.ObjectType = dr["object_type"].ToString();

            externalRelation.ObjectId = Convert.ToInt32(dr["object_id"]);

            if (dr["external_id"] != DBNull.Value)
            {
                externalRelation.ExternalId = Convert.ToInt32(dr["external_id"]);
            }

            externalRelation.Status = dr["status"].ToString();

            externalRelation.StatusMessage = dr["status_message"].ToString();

            externalRelation.ConnectorType = dr["connector_type"].ToString();

            externalRelation.CompanyId = Convert.ToInt32(dr["company_id"]);

            externalRelation.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            externalRelation.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);

            externalRelation.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            externalRelation.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);

            return externalRelation;
        }

        #endregion

        #region - private methods Actions SAP PM -
        private List<NpgsqlParameter> GetNpgsqlParametersFromSapPmNotificationConfig(int companyId, int userId, ActionsAction action)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            parameters.Add(new NpgsqlParameter("@_actionid", action.Id));

            parameters.Add(new NpgsqlParameter("@_sappmlocationid", action.SapPmNotificationConfig.FunctionalLocationId));
            parameters.Add(new NpgsqlParameter("@_maintpriority", action.SapPmNotificationConfig.MaintPriority));
            parameters.Add(new NpgsqlParameter("@_notificationtype", action.SapPmNotificationConfig.Notificationtype));

            parameters.Add(new NpgsqlParameter("@_userid", userId));


            if (string.IsNullOrEmpty(action.SapPmNotificationConfig.NotificationTitle))
            {
                //Fill the notification text with max 40 chars, and build the long text with the full description.
                parameters.Add(new NpgsqlParameter("@_notificationtext", action.Description.Substring(0, Math.Min(40, action.Description.Length))));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_notificationtext", action.SapPmNotificationConfig.NotificationTitle.Substring(0, Math.Min(40, action.SapPmNotificationConfig.NotificationTitle.Length))));
            }

            StringBuilder sb = new StringBuilder(action.Description);
            sb.Append("; ");
            sb.Append(action.Comment);
            sb.Append("; ");
            action.Tags?.ForEach(x => sb.Append(string.Concat(x.Name, ", ")));
            if(action.Tags != null && action.Tags.Count > 0)
            {
                sb.Length -= 2; //remove last ", "
                sb.Append("; ");
            }
            action.AssignedAreas?.ForEach(x => sb.Append(string.Concat(x.Name, ", ")));
            if (action.AssignedAreas != null && action.AssignedAreas.Count > 0)
            {
                sb.Length -= 2; //remove last ", "
                sb.Append("; ");
            }
            action.AssignedUsers?.ForEach(x => sb.Append(string.Concat(x.Name, ", ")));
            if (action.AssignedUsers != null && action.AssignedUsers.Count > 0)
            {
                sb.Length -= 2; //remove last ", "
                sb.Append("; ");
            }
            sb.Append("ActionId: ");
            sb.Append(action.Id);

            parameters.Add(new NpgsqlParameter("@_maintnotiflongtextforedit", sb.ToString()));

            return parameters;
        }


        private SapPmNotification CreateOrFillSapPmNotificationFromReader(NpgsqlDataReader dr, SapPmNotification sapPmNotification)
        {
            if (sapPmNotification == null) sapPmNotification = new SapPmNotification();

            sapPmNotification.Id = Convert.ToInt32(dr["id"]);
            sapPmNotification.CompanyId = Convert.ToInt32(dr["company_id"]);

            sapPmNotification.ActionId = Convert.ToInt32(dr["action_id"]);

            sapPmNotification.FunctionalLocationId = Convert.ToInt32(dr["sap_pm_location_id"]);

            sapPmNotification.NotificationText = Convert.ToString(dr["notification_text"]);
            sapPmNotification.MaintNotifLongTextForEdit = Convert.ToString(dr["maint_notif_long_text_for_edit"]);

            sapPmNotification.MaintPriority = Convert.ToString(dr["maint_priority"]);
            sapPmNotification.NotificationType = Convert.ToString(dr["notification_type"]);

            if (dr["sent_to_sap_on"] != DBNull.Value)
            {
                sapPmNotification.SentToSapOn = Convert.ToDateTime(dr["sent_to_sap_on"]);
            }

            if (dr["sap_id"] != DBNull.Value)
            {
                sapPmNotification.SapId = Convert.ToInt64(dr["sap_id"]);
            }

            sapPmNotification.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            sapPmNotification.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            sapPmNotification.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);

            return sapPmNotification;
        }

        private async Task<ActionsAction> ComplementActionRelationDetails(int companyId, ActionsAction action)
        {
            //Add potentially missing details to action
            action.Tags = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Action, id: action.Id);
            foreach (AreaBasic area in action.AssignedAreas.FindAll(a => a.Name == null || a.Name == string.Empty))
            {
                Area dbArea = await _areaManager.GetAreaAsync(companyId: companyId, areaId: area.Id);
                area.Name = dbArea.Name;
            }
            foreach (UserBasic user in action.AssignedUsers.FindAll(u => u.Name == null || u.Name == string.Empty))
            {
                UserBasic dbUser = await _userManager.GetUserBasicAsync(companyId: companyId, userId: user.Id);
                user.Name = dbUser.Name;
            }

            return action;
        }

        #endregion

        #region - check items -
        /// <summary>
        /// CheckActionCommentAsync; Check action comments
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="actionId">ActionId (DB: actions_action.id)</param>
        /// <param name="timestamp">timstamp to be added</param>
        /// <returns></returns>
        public async Task<List<UpdateCheckCommentItem>> CheckActionCommentAsync(int companyId, int? actionId = null, DateTime? timestamp = null)
        {
            var output = new List<UpdateCheckCommentItem>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                if(actionId.HasValue && actionId.Value > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_actionid", actionId.Value));
                }
                if (timestamp.HasValue && timestamp.Value != DateTime.MinValue)
                {
                    parameters.Add(new NpgsqlParameter("@_timestamp", timestamp.Value));
                }

                using (dr = await _manager.GetDataReader("check_changes_actioncomments", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var item = new UpdateCheckCommentItem();

                        item.ActionId = Convert.ToInt32(dr["action_id"]);
                        item.NumberOfItems = Convert.ToInt32(dr["nr"]);

                        output.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("ActionManager.CheckActionCommentAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        #endregion

        #region - cleaners -
        /// <summary>
        /// PrepareAndCleanAction; Checks a action, and prepares it for saving.
        /// Will remove incorrect data.
        /// </summary>
        /// <param name="action">Action where images need to be cleaned</param>
        /// <returns>return cleaned action</returns>
        private ActionsAction PrepareAndCleanAction(ActionsAction action)
        {
            if(action!=null && action.Images != null)
            {
                action.Images.RemoveAll(x => x.Length > 100);
            }
            if (action != null && action.TaskTemplateId.HasValue && action.TaskTemplateId == 0)
            {
                action.TaskTemplateId = null;
            }
            if (action != null && action.TaskId.HasValue && action.TaskId == 0)
            {
                action.TaskId = null;
            }
            return action;
        }

        /// <summary>
        /// PrepareAndCleanActionComment; Checks a actioncomment, and prepares it for saving.
        /// Will remove incorrect data.
        /// </summary>
        /// <param name="actionComment">ActionComment where images need to be cleaned</param>
        /// <returns>return cleaned action</returns>
        private ActionComment PrepareAndCleanActionComment(ActionComment actionComment)
        {
            if (actionComment != null && actionComment.Images != null)
            {
                actionComment.Images.RemoveAll(x => x.Length > 100);
            }
            return actionComment;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_areaManager.GetPossibleExceptions());
                listEx.AddRange(_userManager.GetPossibleExceptions());
                listEx.AddRange(_toolsManager.GetPossibleExceptions());
                listEx.AddRange(_tagManager.GetPossibleExceptions());
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

