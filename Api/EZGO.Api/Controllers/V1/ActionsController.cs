using Elastic.Apm;
using Elastic.Apm.Api;
using EZ.Connector.Init.Interfaces;
using EZ.Connector.SAP.Interfaces;
using EZ.Connector.Ultimo.Interfaces;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// ActionsController; contains all routes based on action this includes Actions, single Action, Comments and single Comments.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Actions)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ActionsController : BaseController<ActionsController>
    {
        #region - variables -
        private readonly IMemoryCache _cache;
        private readonly IActionManager _manager;
        private readonly IConnectorManager _connectorManager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly IGeneralManager _generalManager;
        #endregion

        #region - constructor(s) -
        public ActionsController(IUserManager userManager, IActionManager manager, IGeneralManager generalManager, IMemoryCache memoryCache, IConfigurationHelper configurationHelper, IConnectorManager connectorManager, IToolsManager toolsManager, ISAPConnector sapconnector, IUltimoConnector ultimoConnector, ILogger<ActionsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _cache = memoryCache;
            _manager = manager;
            _connectorManager = connectorManager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _generalManager = generalManager;
        }
        #endregion

        #region - GET routes actions -
        /// <summary>
        /// GetActions -> retrieve actions based on several parameters.
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedtome">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="checklistid">checklistid: id of the checklist connected to the actions</param>
        /// <param name="checklisttemplateid">checklisttemplateid: id of the checklist template connected to the actions</param>
        /// <param name="auditid">auditid: id of the audit connected to the actions</param>
        /// <param name="audittemplateid">audittemplateid: id of the audit template connected to the actions</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <param name="rangeperiod">rangeperiod: optional predefined creation date range filter.Supported values: last12days, last12weeks, last12months, thisyear.</param>
        /// <returns>Returns a IActionResult containing a list of actions.</returns>
        /// <response code="200">Collection of actions</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        [Route("actions")]
        [HttpGet]
        public async Task<IActionResult> GetActions([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved,
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? assignedtome, [FromQuery] bool? createdByOrAssignedToMe, [FromQuery] int? createdbyid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid,
            [FromQuery] string tagids, [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid,
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom, [FromQuery] string resolvedto,
            [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid,
            [FromQuery] string rangeperiod, [FromQuery] string sort, [FromQuery] string direction,
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;
            DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTimeStamp);

             var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedtome, createdByOrAssignedToMe,
                    createdbyid, taskid, tasktemplateid, tagids, assigneduserid, assigneduserids, filterassignedareatype, assignedareaid,
                    assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, sort, direction, limit, offset);
                                  
            var _companyId = await CurrentApplicationUser.GetAndSetCompanyIdAsync();

            if (!string.IsNullOrWhiteSpace(rangeperiod) && !(filters.CreatedFrom.HasValue || filters.CreatedTo.HasValue))
            {
                    var period = await _generalManager.GetCreatedPeriodByRangeAsync(_companyId, rangeperiod);
                    if (period.start != null || period.end != null)
                    {
                        filters.CreatedFrom = period.start;
                        filters.CreatedTo  = period.end;
                    }
            }
          

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            var uniqueKey = string.Format("GET_ACTIONS_T{0}_C{1}_U{2}_L{3}_O{4}", parsedTimeStamp.ToString("dd-MM-yyyy_HH:mm"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("actions", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: _companyId, "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId:_companyId, "TECH_TRAFFICSHAPING_LIMITS"))
                    {
                        if (filters.Limit.HasValue && filters.Limit.Value == 0) { filters.Limit = ApiSettings.DEFAULT_MAX_NUMBER_OF_ACTION_RETURN_ITEMS; }
                    }

                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: _companyId, "TECH_TRAFFICSHAPING_ACTIONS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                } else 
                { 
                    enableTrafficShaping = false; 
                }  
            }


            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return GetObjectResultJsonWithStatus(result);

        }
       
        /// <summary>
        /// GetActionCounts -> retrieves several action counts based on the same filters and structures as the normal action calls. 
        /// When retrieving with no parameters it will retrieve the counts based on no filters. 
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedtome">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="checklistid">checklistid: id of the checklist connected to the actions</param>
        /// <param name="checklisttemplateid">checklisttemplateid: id of the checklist template connected to the actions</param>
        /// <param name="auditid">auditid: id of the audit connected to the actions</param>
        /// <param name="audittemplateid">audittemplateid: id of the audit template connected to the actions</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a ActionCountStatistics.</returns>
        /// <response code="200">ActionCountStatistic item</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        /// <summary>
        /// GetActionCounts -> retrieves several action counts based on the same filters and structures as the normal action calls. 
        /// When retrieving with no parameters it will retrieve the counts based on no filters. 
        /// </summary>
        [Route("actions_counts")]
        [Route("actions/counts")]
        [HttpGet]
        public async Task<IActionResult> GetActionCounts([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved, 
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? assignedtome, [FromQuery] bool? createdByOrAssignedToMe, [FromQuery] int? createdbyid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, 
            [FromQuery] string tagids, [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid, 
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom, 
            [FromQuery] string resolvedto, [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid, 
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedtome, createdByOrAssignedToMe,
                createdbyid, taskid, tasktemplateid, tagids, assigneduserid, assigneduserids, filterassignedareatype, assignedareaid,
                assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, limit, offset);

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        /// <summary>
        /// GetActionRelations -> retrieves several lsit of action relation items based on the same filters and structures as the normal action calls. 
        /// The following relations can be found within the collection: task_id, task_template_id, checklist_id , audit_id 
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedtome">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a List of ActionRelation objects.</returns>
        /// <response code="200">List of ActionRelation items collection</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        [Route("actions_relations")]
        [Route("actions/relations")]
        [HttpGet]
        public async Task<IActionResult> GetActionRelations([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved, 
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? assignedtome, [FromQuery] bool? createdByOrAssignedToMe, [FromQuery] int? createdbyid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, 
            [FromQuery] string tagids, [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid, 
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom, [FromQuery] string resolvedto, 
            [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedtome, createdByOrAssignedToMe,
                createdbyid, taskid, tasktemplateid, tagids, assigneduserid, assigneduserids, filterassignedareatype, assignedareaid,
                assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, null, null, null, null, null, limit, offset);//todo support checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionRelationsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }



        /// <summary>
        /// GetActionsMy -> retrieve my actions based on several parameters.
        ///                 createdByOrAssignedToMe will be set to current user id
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedtome">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a list of actions.</returns>
        /// <response code="200">Collection of actions</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        //no createdByOrAssignedToMe filter
        [Route("actions/my")]
        [HttpGet]
        public async Task<IActionResult> GetActionsMy([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved,
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? assignedtome, [FromQuery] int? createdbyid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, [FromQuery] string tagids,
            [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid,
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom,
            [FromQuery] string resolvedto, [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid, 
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedtome, createdByOrAssignedToMe: true,
                createdbyid, taskid, tasktemplateid, tagids, assigneduserid, assigneduserids, filterassignedareatype, assignedareaid,
                assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, limit, offset);//todo support checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        /// <summary>
        /// Retrieve counts based on 'my actions' call. Uses same filters.
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedtome">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a ActionCountStatistics.</returns>
        /// <response code="200">Collection of actions</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        [Route("actions/my/counts")]
        [HttpGet]
        public async Task<IActionResult> GetActionsMyCounts([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved,
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? assignedtome, [FromQuery] int? createdbyid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, [FromQuery] string tagids,
            [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid,
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom,
            [FromQuery] string resolvedto, [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid, 
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedtome, createdByOrAssignedToMe: true,
                createdbyid, taskid, tasktemplateid, tagids, assigneduserid, assigneduserids, filterassignedareatype, assignedareaid,
                assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, limit, offset);//todo support checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        /// <summary>
        /// GetActionsStartedByMe -> retrieve actions based on several parameters.
        ///                          createdbyid filter will be set to current user id
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedtome">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)/param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a list of actions.</returns>
        /// <response code="200">Collection of actions</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        //no createdbyid filter
        [Route("actions/started_by_me")]
        [HttpGet]
        public async Task<IActionResult> GetActionsStartedByMe([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved, 
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? assignedtome, [FromQuery] bool? createdByOrAssignedToMe, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, [FromQuery] string tagids, 
            [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid, 
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom, 
            [FromQuery] string resolvedto, [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid, 
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedtome, createdByOrAssignedToMe, createdbyid: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskid, tasktemplateid, tagids,
            assigneduserid, assigneduserids, filterassignedareatype, assignedareaid, assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, limit, offset);//todo support checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        /// <summary>
        /// Retrieve action counts based on the 'started by me' filter.
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedtome">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a ActionCountStatistics.</returns>
        /// <response code="200">Collection of actions</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        [Route("actions/started_by_me/counts")]
        [HttpGet]
        public async Task<IActionResult> GetActionsStartedByMeCounts([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved,
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? assignedtome, [FromQuery] bool? createdByOrAssignedToMe, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, [FromQuery] string tagids,
            [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid,
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom,
            [FromQuery] string resolvedto, [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid, 
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedtome, createdByOrAssignedToMe, createdbyid: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskid, tasktemplateid, tagids,
            assigneduserid, assigneduserids, filterassignedareatype, assignedareaid, assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, limit, offset);//todo support checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        /// <summary>
        /// GetActionsAssignedToMe -> retrieve actions based on several parameters.
        ///                           assigneduserid will be set to current user id
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a list of actions.</returns>
        /// <response code="200">Collection of actions</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        //no assigned user id filter
        [Route("actions/assigned_to_me")]
        [HttpGet]
        public async Task<IActionResult> GetActionsAssignedToMe([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved, 
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? createdByOrAssignedToMe, [FromQuery] int? createdbyid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, 
            [FromQuery] string tagids, [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid, 
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom, 
            [FromQuery] string resolvedto, [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid, 
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedToMe: true, createdByOrAssignedToMe,
                createdbyid, taskid, tasktemplateid, tagids, assigneduserid, assigneduserids,
                filterassignedareatype, assignedareaid, assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, limit, offset);//todo support checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        /// <summary>
        /// Retrieve action count statistics based on 'assigned to me' filter.
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date. (format: dd-MM-yyyy)</param>
        /// <param name="include">include: possible values are unviewedcommentnr, mainparent, assignedareas, assignedusers, userinformation, tags</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>Returns a IActionResult containing a ActionCountStatistics.</returns>
        /// <response code="200">Collection of actions</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="401">No rights to retrieve the list of actions.</response>
        //no assigned user id filter
        [Route("actions/assigned_to_me/counts")]
        [HttpGet]
        public async Task<IActionResult> GetActionCountsAssignedToMe([FromQuery] string filtertext, [FromQuery] bool? isresolved, [FromQuery] bool? isoverdue, [FromQuery] bool? isunresolved,
            [FromQuery] bool? hasunviewedcomments, [FromQuery] bool? createdByOrAssignedToMe, [FromQuery] int? createdbyid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid,
            [FromQuery] string tagids, [FromQuery] int? assigneduserid, [FromQuery] string assigneduserids, [FromQuery] FilterAreaTypeEnum? filterassignedareatype, [FromQuery] int? assignedareaid,
            [FromQuery] string assignedareaids, [FromQuery] string timestamp, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string resolvedfrom,
            [FromQuery] string resolvedto, [FromQuery] string overduefrom, [FromQuery] string overdueto, [FromQuery] string resolvedcutoffdate, [FromQuery] int? checklistid, [FromQuery] int? checklisttemplateid, [FromQuery] int? auditid, [FromQuery] int? audittemplateid, [FromQuery] int? parentareaid, 
            [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            _manager.Culture = TranslationLanguage;

            var filters = await GetActionFilters(filtertext, isresolved, isoverdue, isunresolved, hasunviewedcomments, assignedToMe: true, createdByOrAssignedToMe,
                createdbyid, taskid, tasktemplateid, tagids, assigneduserid, assigneduserids,
                filterassignedareatype, assignedareaid, assignedareaids, timestamp, createdfrom, createdto, resolvedfrom, resolvedto, overduefrom, overdueto, resolvedcutoffdate, checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid, limit, offset); //todo support checklistid, checklisttemplateid, auditid, audittemplateid, parentareaid

            if (!ActionFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        filters: filters,
                                                        include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("action/{actionid}")]
        [HttpGet]
        public async Task<IActionResult> GetAction([FromRoute] int actionid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_ACTION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionId: actionid, include: include, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("actions/assignedusers")]
        [HttpGet]
        public async Task<IActionResult> GetActionAssignedUsers()
        {
            _manager.Culture = TranslationLanguage;

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAsssignedUsersWithActions(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }


        [Route("actions/assignedareas")]
        [HttpGet]
        public async Task<IActionResult> GetActionAssignedAreas()
        {
            _manager.Culture = TranslationLanguage;

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAsssignedAreasWithActions(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        #endregion

        #region - POST routes actions -
        /// <summary>
        /// AddAction -> Add an action.
        /// The following rules must be met:
        /// - The company that is connected to the action must be the company that executes the check.
        /// - The userId that created the item is the same user that executes the check (unless ignoreUserIdCheck is true).
        /// - The Comment should be filled and can not contain script related tags and filters out html tags so they are filtered out if filled.
        /// - The Description can not contain script related tags and filters out html tags so they are filtered out if filled
        /// - The Images should contain valid media UriParts
        /// - The Videos should contain valid media UriParts
        /// - The VideoThumbnails should contain valid media UriParts
        /// </summary>
        /// <param name="action">ActionAction object, containing all data for that specific action. </param>
        /// <returns>Will return an int containing the action id. </returns>
        /// <response code="200">ActionId</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="403">Forbidden, no rights to process the action, company id possibly invalid.</response>
        [Route("action/add")]
        [Route("audit/actions/add")]
        [Route("checklist/actions/add")]
        [HttpPost]
        public async Task<IActionResult> AddAction([FromBody] ActionsAction action)
        {
            if (!await this.CurrentApplicationUser.CheckObjectCompanyRights(objectCompanyId: action.CompanyId, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (action.TaskId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: action.TaskId.Value, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (action.TaskTemplateId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: action.TaskTemplateId.Value, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if(!action.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                              userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), 
                                              messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                                              
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: action.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }


            var result = await _manager.AddActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), action: action);

            if (result > 0)
            {
                var actionForExternalSystem = await _manager.GetActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                            include: "unviewedcommentnr,comments,assignedareas,assignedusers,userinformation,tags",
                                                                                             actionId: result,
                                                                                             connectionKind: ConnectionKind.Writer);

                actionForExternalSystem.SendToUltimo = action.SendToUltimo;

                await _connectorManager.InitConnectors(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                       userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                       action: actionForExternalSystem);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);

        }

        /// <summary>
        /// ChangeAction -> Add an action.
        /// The following rules must be met:
        /// - The company that is connected to the action must be the company that executes the check.
        /// - The Comment should be filled and can not contain script related tags and filters out html tags so they are filtered out if filled.
        /// - The Description can not contain script related tags and filters out html tags so they are filtered out if filled
        /// - The Images should contain valid media UriParts
        /// - The Videos should contain valid media UriParts
        /// - The VideoThumbnails should contain valid media UriParts
        /// </summary>
        /// <param name="actionid">Action id of the action that is being updated.</param>
        /// <param name="action"></param>
        /// <returns>Will return an bool. </returns>
        /// <response code="200">true/false depending on outcome.</response>
        /// <response code="400">Filters didn't pass validation</response>
        /// <response code="403">Forbidden, no rights to process the action, company id possibly invalid.</response>
        [Route("action/change/{actionid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAction([FromRoute] int actionid, [FromBody] ActionsAction action)
        {
            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_ACTION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (actionid != action.Id || !await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (action.TaskId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: action.TaskId.Value, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (action.TaskTemplateId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: action.TaskTemplateId.Value, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!action.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  ignoreCreatedByCheck: true,
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: action.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            //current action already resolved, ignore submitted action.
            ActionsAction currentAction = await _manager.GetActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionId: action.Id);
            if (currentAction.IsResolved.HasValue && currentAction.IsResolved == true)
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.OK).ToString(), header: "N/A", request: action.ToJsonFromObject(), response: "Action already resolved, changes ignored.");
                return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject()); //return normal response, for backwards compatibility. 
            }

            if (!action.ValidateMutation(currentAction, out string messages))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: action.ToJsonFromObject(), response: messages);
                return StatusCode((int)HttpStatusCode.BadRequest, messages.ToJsonFromObject());
            }

            var result = await _manager.ChangeActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), actionId: actionid, action: action);

            if (result)
            {
                var actionForExternalSystem = await _manager.GetActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                            include: "unviewedcommentnr,comments,assignedareas,assignedusers,userinformation,tags",
                                                                                             actionId: action.Id,
                                                                                             connectionKind: ConnectionKind.Writer);

                actionForExternalSystem.SendToUltimo = action.SendToUltimo;

                await _connectorManager.InitConnectors(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                       userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                       action: actionForExternalSystem);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("action/setactive/{actionid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveAction([FromRoute] int actionid, [FromBody] object isActive)
        {
            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_ACTION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetActionActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), actionId: actionid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("action/setresolved/{actionid}")]
        [HttpPost]
        public async Task<IActionResult> SetResolvedAction([FromRoute] int actionid, [FromBody] object isResolved)
        {
            _manager.Culture = TranslationLanguage;

            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_ACTION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isResolved))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (BooleanConverter.ConvertObjectToBoolean(isResolved) == true)
            {
                //get action
                var existingAction = await _manager.GetActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionId: actionid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

                //current action already resolved, ignore submitted action.
                if (existingAction.IsResolved.HasValue && existingAction.IsResolved == true)
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.OK).ToString(), header: "N/A", request: actionid.ToJsonFromObject(), response: "Action already resolved, changes ignored.");
                    return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject()); //return normal response, for backwards compatibility. 
                }
            }

            var result = await _manager.SetActionResolvedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), actionId: actionid, isResolved: BooleanConverter.ConvertObjectToBoolean(isResolved), useAutoResolvedMessage: BooleanConverter.ConvertObjectToBoolean(isResolved));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }


        [Route("action/settask/{actionid}")]
        [HttpPost]
        public async Task<IActionResult> SetTaskAction([FromRoute] int actionid, [FromBody] int taskid)
        {
            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                if (_configurationHelper.GetValueAsBool("AppSettings:ActionSetTaskLogging")) _logger.LogInformation($"SetTaskAction failed with status: BadRequest (actionid: {actionid}, taskid: {taskid}, reason: invalid action id)");
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_ACTION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                if (_configurationHelper.GetValueAsBool("AppSettings:ActionSetTaskLogging")) _logger.LogInformation($"SetTaskAction failed with status: Forbidden (actionid: {actionid}, taskid: {taskid}, reason: action object rights)");
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                if (_configurationHelper.GetValueAsBool("AppSettings:ActionSetTaskLogging")) _logger.LogInformation($"SetTaskAction failed with status: Forbidden (actionid: {actionid}, taskid: {taskid}, reason: task object rights)");
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetActionTaskAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), actionId: actionid, taskId: taskid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("action/setviewed/{actionid}")]
        [HttpPost]
        public async Task<IActionResult> SetActionViewed([FromRoute] int actionid)
        {
            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_ACTION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetActionViewedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionId: actionid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }
        #endregion

        #region - GET routes comments -
        [Route("actioncomments")]
        [HttpGet]
        public async Task<IActionResult> GetActionComments([FromQuery] int? actionid, [FromQuery] int? userid, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new ActionFilters() { ActionId = actionid, UserId = userid }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            List<ActionComment> result;

            if (actionid.HasValue && actionid.Value > 0 && !userid.HasValue)
            {
                if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid.Value, objectType: ObjectTypeEnum.Action))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }

                result = await _manager.GetActionCommentsByActionIdAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionId: actionid.Value);
            }
            else
            {
                result = await _manager.GetActionCommentsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), filters: filters);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("actioncomment/{actioncommentid}")]
        [HttpGet]
        public async Task<IActionResult> GetActionComment([FromRoute] int actioncommentid)
        {
            if (!ActionValidators.CommentIdIsValid(actioncommentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_COMMENT_ID_IS_NOT_VALID);
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actioncommentid, objectType: ObjectTypeEnum.ActionComment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionCommentId: actioncommentid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("actioncomments/unread")]
        [HttpGet]
        public async Task<IActionResult> GetUnreadActionCommentCount()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetActionCommentStatisticsRelatedToUser(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        #endregion

        #region - POST routes comments  -
        [Route("actioncomment/add")]
        [HttpPost]
        public async Task<IActionResult> AddActionComment([FromBody] ActionComment comment)
        {
            if (comment.CompanyId.HasValue && !await this.CurrentApplicationUser.CheckObjectCompanyRights(objectCompanyId: comment.CompanyId.Value, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: comment.ActionId, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!comment.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: comment.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.AddActionCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), actionComment: comment);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("actioncomment/change/{actioncommentid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeActionComment([FromRoute] int actioncommentid, [FromBody] ActionComment comment)
        {
            if (!ActionValidators.CommentIdIsValid(actioncommentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_COMMENT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (actioncommentid != comment.Id || !await this.CurrentApplicationUser.CheckObjectRights(objectId: actioncommentid, objectType: ObjectTypeEnum.ActionComment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: comment.ActionId, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!comment.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: comment.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChangeActionCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), actionCommentId: actioncommentid, actionComment: comment);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("actioncomment/setactive/{actioncommentid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveActionComment([FromRoute] int actioncommentid, [FromBody] object isActive)
        {
            if (!ActionValidators.CommentIdIsValid(actioncommentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_COMMENT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actioncommentid, objectType: ObjectTypeEnum.ActionComment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }


            var result = await _manager.SetActionCommentActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), actionCommentId: actioncommentid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("actioncomment/setviewed/{actioncommentid}")]
        [HttpPost]
        public async Task<IActionResult> SetCommentViewed([FromRoute] int actioncommentid)
        {
            if (!ActionValidators.CommentIdIsValid(actioncommentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_COMMENT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actioncommentid, objectType: ObjectTypeEnum.ActionComment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetActionCommentViewedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionCommentId: actioncommentid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("actioncomment/setviewedall/{actionid}")]
        [HttpPost]
        public async Task<IActionResult> SetCommentViewedAll([FromRoute] int actionid)
        {
            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ActionValidators.MESSAGE_ACTION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }


            var result = await _manager.SetActionCommentViewedAllAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionId: actionid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }
        #endregion

        #region - check routes -

        [Route("actioncomments/updatecheck")]
        [HttpGet]
        public async Task<IActionResult> GetUpdatesActionsComment([FromQuery] string timestamp, [FromQuery] int? actionid = null)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            DateTime parsedTimeStamp = DateTime.MinValue;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var result = await _manager.CheckActionCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), actionId: actionid, timestamp: parsedTimeStamp);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        #endregion

        #region - health checks -
        /// <summary>
        /// GetActionsHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("actions/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetActionsHealth()
        {
            try
            {
                var result = await _manager.GetActionsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY),
                                                                       userId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_USER_ID_CONFIG_KEY),
                                                                       filters: new ActionFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

                if (result.Any() && result.Count > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject());
                }
            }
            catch
            {

            }
            return StatusCode((int)HttpStatusCode.Conflict, false.ToJsonFromObject());
        }

        /// <summary>
        /// GetActionCommentsHealth; Checks the basic actioncomment functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("actioncomments/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetActionCommentsHealth()
        {

            try
            {
                var result = await _manager.GetActionCommentsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY),
                                                                       filters: new ActionFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

                if (result.Any() && result.Count > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject());
                }
            }
            catch
            {

            }
            return StatusCode((int)HttpStatusCode.Conflict, false.ToJsonFromObject());

        }
        #endregion

        #region - private methods -
        /// <summary>
        /// Get ActionFilters based on given parameters
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// status filters
        /// <param name="isresolved">isresolved: checks whether is_resolved is true</param>
        /// <param name="isoverdue">isoverdue: takes the current date time, adds one day and compares that to the action due date (due date before now+1day)</param>
        /// <param name="isunresolved">isunresolved: takes the current date time and compares that to the action due date (due date after now)</param>
        /// end status filters
        /// <param name="hasunviewedcomments">hasunviewedcomments: checks if the actions have unviewed comments for the user obtaining the actions</param>
        /// <param name="assignedToMe">assignedToMe: returns actions for the 'assigned to me' filter. contains actions whith user in resources or an allowed area for the user in resources</param>
        /// <param name="createdByOrAssignedToMe">createdByOrAssignedToMe: returns my actions (either created by current user or action has assigned users containing this user)</param>
        /// <param name="createdbyid">createdbyid: checks if the action was created by createdbyid</param>
        /// <param name="taskid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tasktemplateid">taskid: checks if the action taskid equals this parameter</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="assigneduserid">assigneduserid: checks if the assigned users contains the assigneduserid</param>
        /// <param name="assigneduserids">assigneduserids: checks if the assigned users contains any of the assigneduserids (format: 1,2,3)</param>
        /// <param name="filterassignedareatype">Not used</param>
        /// <param name="assignedareaid">assignedareaid: checks if the assigned areas contains the assignedareaid</param>
        /// <param name="assignedareaids">assignedareaids: checks if the assigned areas contains any of the assignedareaids (format: 1,2,3)</param>
        /// <param name="timestamp">timestamp: checks if the created at is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the action was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedfrom">resolvedfrom: checks (only if resolvedto also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedto">resolvedto: checks (only if resolvedfrom also filled in) if the action was resolved at or after the resolvedfrom, but at or before the resolvedto (format: dd-MM-yyyy)</param>
        /// <param name="overduefrom">overduefrom: checks (only if overdueto also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="overdueto">overdueto: checks (only if overduefrom also filled in) if the action was overdue at or after the overduefrom, but at or before the overdueto (format: dd-MM-yyyy)</param>
        /// <param name="resolvedcutoffdate">resolvedcutoffdate: Will not return resolved actions that are resolved before the provided date. Results will include actions resolved on the provided date.</param>
        /// <param name="limit">limit: limit the maximum amount of actions that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of actions</param>
        /// <returns>ActionFilters object</returns>
        [NonAction]
        private async Task<ActionFilters> GetActionFilters(string filtertext, bool? isresolved, bool? isoverdue, bool? isunresolved, bool? hasunviewedcomments, bool? assignedToMe, bool? createdByOrAssignedToMe,
            int? createdbyid, int? taskid, int? tasktemplateid, string tagids, int? assigneduserid, string assigneduserids, FilterAreaTypeEnum? filterassignedareatype, int? assignedareaid,
            string assignedareaids, string timestamp, string createdfrom, string createdto, string resolvedfrom, string resolvedto, string overduefrom, string overdueto, string resolvedcutoffdate, int? checklistid,
            int? checklisttemplateid, int? auditid, int? audittemplateid, int? parentareaid, string sort, string direction, int? limit, int? offset)
        {
            DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTimeStamp);
            DateTime.TryParseExact(createdfrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime createdFromTimeStamp);
            DateTime.TryParseExact(createdto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime createdToTimeStamp);
            DateTime.TryParseExact(resolvedfrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resolvedFromTimeStamp);
            DateTime.TryParseExact(resolvedto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resolvedToTimeStamp);
            DateTime.TryParseExact(overduefrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime overdueFromTimeStamp);
            DateTime.TryParseExact(overdueto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime overdueToTimeStamp);
            DateTime.TryParseExact(resolvedcutoffdate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resolvedCutoffDateTimeStamp);

            var filters = new ActionFilters()
            {
                FilterText = filtertext,

                IsResolved = isresolved,
                IsOverdue = isoverdue,
                IsUnresolved = isunresolved,

                HasUnviewedComments = hasunviewedcomments,

                UserId = await this.CurrentApplicationUser.GetAndSetUserIdAsync(),

                CreatedByOrAssignedTo = createdByOrAssignedToMe.HasValue && createdByOrAssignedToMe.Value ? await this.CurrentApplicationUser.GetAndSetUserIdAsync() : null,

                CreatedById = createdbyid,

                TaskId = taskid,
                TaskTemplateId = tasktemplateid,

                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                AssignedToMeUserId = assignedToMe.HasValue && assignedToMe.Value ? await this.CurrentApplicationUser.GetAndSetUserIdAsync() : null,
                AssignedUserId = assigneduserid,
                AssignedUserIds = string.IsNullOrEmpty(assigneduserids) ? null : assigneduserids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                AssignedAreaType = filterassignedareatype,

                AssignedAreaId = assignedareaid,
                AssignedAreaIds = string.IsNullOrEmpty(assignedareaids) ? null : assignedareaids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                Timestamp = !string.IsNullOrEmpty(timestamp) && parsedTimeStamp != DateTime.MinValue ? parsedTimeStamp : new Nullable<DateTime>(),

                CreatedFrom = !string.IsNullOrEmpty(createdfrom) && createdFromTimeStamp != DateTime.MinValue ? createdFromTimeStamp : new Nullable<DateTime>(),
                CreatedTo = !string.IsNullOrEmpty(createdto) && createdToTimeStamp != DateTime.MinValue ? createdToTimeStamp : new Nullable<DateTime>(),

                ResolvedFrom = !string.IsNullOrEmpty(resolvedfrom) && resolvedFromTimeStamp != DateTime.MinValue ? resolvedFromTimeStamp : new Nullable<DateTime>(),
                ResolvedTo = !string.IsNullOrEmpty(resolvedto) && resolvedToTimeStamp != DateTime.MinValue ? resolvedToTimeStamp : new Nullable<DateTime>(),

                OverdueFrom = !string.IsNullOrEmpty(overduefrom) && overdueFromTimeStamp != DateTime.MinValue ? overdueFromTimeStamp : new Nullable<DateTime>(),
                OverdueTo = !string.IsNullOrEmpty(overdueto) && overdueToTimeStamp != DateTime.MinValue ? overdueToTimeStamp : new Nullable<DateTime>(),

                ResolvedCutoffDate = !string.IsNullOrEmpty(resolvedcutoffdate) && resolvedCutoffDateTimeStamp != DateTime.MinValue ? resolvedCutoffDateTimeStamp : new Nullable<DateTime>(),

                ChecklistId = checklistid,
                ChecklistTemplateId = checklisttemplateid,

                AuditId = auditid,
                AuditTemplateId = audittemplateid,

                ParentAreaId = parentareaid,

                SortColumn = sort,
                SortDirection = direction,

                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_ACTION_RETURN_ITEMS,
                Offset = offset
            };

            return filters;
        }
        #endregion
    }
}