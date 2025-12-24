using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Relations;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
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

namespace EZGO.Api.Controllers.GEN4
{
    /// <summary>
    /// TasksController; contains all routes based on tasks.
    /// Can be used for GEN4 or new/optimized implementations on existing clients.
    /// </summary>
    [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
    [Route(ApiSettings.VERSION_GEN4_BASE_API_ROUTE)]
    [ApiController]
    public class TasksController : BaseController<TasksController>
    {
        #region - privates -
        private readonly IMemoryCache _cache;
        private readonly ITaskManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly IGeneralManager _generalManager;
        private readonly IShiftManager _shiftManager;


        #endregion

        #region - contructor(s) -
        public TasksController(IUserManager userManager, ITaskManager manager, IGeneralManager generalManager, IMemoryCache memoryCache, IConfigurationHelper configurationHelper, IToolsManager toolsManager, ILogger<TasksController> logger, IApplicationUser applicationUser, IShiftManager shiftManager) : base(logger, applicationUser, configurationHelper)
        {
            _cache = memoryCache;
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _generalManager = generalManager;
            _shiftManager = shiftManager;
        }
        #endregion

        #region - GET routes tasks -

        /// <summary>
        /// Retrieves a list of tasks based on the specified filters and parameters.
        /// </summary>
        /// <remarks>This method supports filtering tasks by various criteria, including area, time span,
        /// status, tags, and text search. It ensures that only valid combinations of parameters are accepted. For
        /// example, <paramref name="timespantype"/> cannot be used together with <paramref name="starttimestamp"/> or
        /// <paramref name="endtimestamp"/>.  If traffic shaping is enabled, the method enforces rate-limiting based on
        /// the current user's company and task-related settings.</remarks>
        /// <param name="areaid">The identifier of the area to filter tasks by. This parameter is required.</param>
        /// <param name="timespantype">An optional enumeration value specifying the predefined time span type to filter tasks. Cannot be combined
        /// with <paramref name="starttimestamp"/> or <paramref name="endtimestamp"/>.</param>
        /// <param name="timespanoffset">An optional offset value to adjust the time span specified by <paramref name="timespantype"/>.</param>
        /// <param name="starttimestamp">An optional start timestamp for filtering tasks. Must be supplied together with <paramref
        /// name="endtimestamp"/>. Cannot be combined with <paramref name="timespantype"/>.</param>
        /// <param name="endtimestamp">An optional end timestamp for filtering tasks. Must be supplied together with <paramref
        /// name="starttimestamp"/>. Cannot be combined with <paramref name="timespantype"/>.</param>
        /// <param name="filtertext">An optional text filter to search tasks by name, description, or other relevant fields.</param>
        /// <param name="statusids">An optional comma-separated list of status identifiers to filter tasks by their status.</param>
        /// <param name="tagids">An optional comma-separated list of tag identifiers to filter tasks by their associated tags.</param>
        /// <param name="include">An optional parameter specifying additional related data to include in the response.</param>
        /// <param name="limit">An optional parameter specifying the maximum number of tasks to return. Defaults to 200 if not
        /// provided.</param>
        /// <param name="offset">An optional parameter specifying the number of tasks to skip before starting to return results. Defaults to
        /// 0 if not provided.</param>
        /// <returns>An <see cref="IActionResult"/> containing the list of tasks that match the specified filters, along with
        /// metadata. The metadata is not influenced by <paramref name="statusids"/> or <paramref name="limit"/>.Returns a <see cref="StatusCodeResult"/> with the appropriate HTTP status code in case of invalid
        /// input or other errors.</returns>
        [Route("tasks")]
        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] int areaid, [FromQuery] TaskTimeSpanEnum? timespantype, [FromQuery] int? timespanoffset, [FromQuery] DateTime? starttimestamp, [FromQuery] DateTime? endtimestamp, [FromQuery] string? filtertext, [FromQuery] string? statusids, [FromQuery] string? tagids, [FromQuery] string? include, [FromQuery] bool? allowduplicatetaskinstances, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var result = new TasksWithMetaData();
            DateTime timestamp = DateTime.Now;

            _manager.Culture = TranslationLanguage;

            if(areaid <= 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No areaid supplied.");
            }

            if (timespantype == null && starttimestamp == null && endtimestamp == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            if(timespantype != null && (starttimestamp != null || endtimestamp != null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Cannot combine timespan and start/end timestamp.");
            }

            if((starttimestamp != null && endtimestamp == null) || (starttimestamp == null && endtimestamp != null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Both starttimestamp and endtimestamp must be supplied.");
            }

            if ((timespantype.HasValue &&  timespantype.Value == TaskTimeSpanEnum.Overdue  && allowduplicatetaskinstances.HasValue && allowduplicatetaskinstances.Value))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Cannot retrieve duplicates on overdue tasks.");
            }


            Gen4TaskFilters filters = _manager.GetTaskFilters(timespanType: timespantype, timespanOffset: timespanoffset, startTimestamp: starttimestamp, endTimestamp: endtimestamp, areaId: areaid, filtertext: filtertext, statusIds: statusids, tagIds: tagids, allowDuplicateTaskInstances: allowduplicatetaskinstances, limit: limit, offset:offset);


            var uniqueKey = string.Format("GET_TASKS_GEN4_TASKS_{0}_T{1}_C{2}_U{3}_L{4}_O{5}",
                                                filters.Timespan.HasValue ? filters.Timespan.Value.ToString().ToUpper() : "CUSTOM",
                                                filters.StartTimestamp != null ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), 
                                                                                                            filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) 
                                                                                            : timestamp.ToString("dd-MM-yyyy_HH:mm:s"), 
                                                await CurrentApplicationUser.GetAndSetCompanyIdAsync(), 
                                                await CurrentApplicationUser.GetAndSetUserIdAsync(), 
                                                filters.Limit, 
                                                filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if(await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
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

            result = await _manager.GetTasksGen4Async(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksByDay; Will return all task related to the date in timestamp. 
        /// timestamp will be set to today if not supplied.
        /// </summary>
        /// <param name="starttimestamp"></param>
        /// <param name="endtimestamp">If this timestamp is supplied all tasks for the days between both timestamps are returned</param>
        /// <param name="filtertext"></param>
        /// <param name="areaid"></param>
        /// <param name="statusids"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/day")]
        [Route("tasks/days")]
        [HttpGet]
        public async Task<IActionResult> GetTasksByDay(string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] string? statusids, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            var result = new List<TasksTask>();

            _manager.Culture = TranslationLanguage;

            starttimestamp ??= endtimestamp ?? DateTime.Now.ToString("yyyy-MM-dd");
            endtimestamp ??= starttimestamp;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.Default, timestamp: null, starttimestamp: starttimestamp, endtimestamp: endtimestamp, shiftid: null, templateid: null, filtertext: filtertext, recurrencytype: null, areaid: areaid, statusids: statusids, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.StartTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No day supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TASKS_DAY_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            result = await _manager.GetTasksByDayAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: (DateTime)filters.StartTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksToday; Will return ALL tasks that have a start_at date on TODAY.
        /// Start datetime and end datetime will be calculated based on either the timestamp supplied or based on now server time converted to company timezone. 
        /// If needed specific filters can be used, no filters are required.
        /// This route can be used as a short form, result can also be retrieved on the /tasks route. 
        /// 
        /// The return data is based on paged data and will default to 200 items. Afterwards can be retrieved per x-number of items. 
        /// Limit 0 or larger that 200 will not be supported. 
        /// 
        /// The returned data will consist of all tasks, of all types that need to be started/finished today based on the company shift times. 
        /// 
        /// NOTE! logic not fully finished, weirdness can occur. 
        /// </summary>
        /// <param name="timestamp">WILL BE IGNORED</param>
        /// <param name="starttimestamp">WILL BE IGNORED</param>
        /// <param name="endtimestamp">WILL BE IGNORED</param>
        /// <param name="areaid"></param>
        /// <param name="shiftid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="filtertext"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <param name="periodoffset"></param>
        /// <param name="deduplicatedForDisplay">If set to True results will have deduplicated shift tasks</param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/today")]
        [HttpGet]
        public async Task<IActionResult> GetTasksToday(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] int? periodoffset,[FromQuery] bool? allowedonly = null, [FromQuery] bool deduplicatedForDisplay = true)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.Today, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: recurrencytype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly, periodoffset: periodoffset, deduplicatedForDisplay: deduplicatedForDisplay);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TODAY_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksShift; Get tasks current shift. Current shift is based on the timeset of the company
        /// Start-datetime and end-datetime will be calculated based on either the timestamp supplied or based on now server time converted to company timezone. 
        /// If needed specific filters can be used, no filters are required.
        /// This route can be used as a short form, result can also be retrieved on the /tasks route. 
        /// 
        /// The return data is based on paged data and will default to 200 items. Afterwards can be retrieved per x-number of items. 
        /// Limit 0 or larger that 200 will not be supported. 
        /// 
        /// The returned data will consist of all tasks, of all types that need to be started/finished in this shift based on the company shift times. 
        /// 
        /// NOTE! logic not fully finished, weirdness can occur. 
        /// </summary>
        /// <param name="timestamp">Can be used to change the start point for the offse, defaults to NOWt</param>
        /// <param name="starttimestamp">WILL BE IGNORED</param>
        /// <param name="endtimestamp">WILL BE IGNORED</param>
        /// <param name="areaid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="filtertext"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <param name="periodoffset"></param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/current_shift")]
        [Route("tasks/this_shift")]
        [HttpGet]
        public async Task<IActionResult> GetTasksShift(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] int? periodoffset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.ThisShift, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: null, templateid: templateid, statusids: statusids, recurrencytype: recurrencytype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly, periodoffset: periodoffset);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

                var uniqueKey = string.Format("GET_TASKS_GEN4_SHIFT_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksThisWeek; Will return ALL tasks that have a start_at date on this WEEK (start on monday) 
        /// Start-datetime and end-datetime will be calculated based on either the timestamp supplied or based on now server time converted to company timezone. 
        /// If needed specific filters can be used, no filters are required.
        /// This route can be used as a short form, result can also be retrieved on the /tasks route. 
        /// 
        /// The return data is based on paged data and will default to 200 items. Afterwards can be retrieved per x-number of items. 
        /// Limit 0 or larger that 200 will not be supported. 
        /// 
        /// NOTE! logic not fully finished, weirdness can occur. 
        /// </summary>
        /// <param name="timestamp">WILL BE IGNORED</param>
        /// <param name="starttimestamp">WILL BE IGNORED</param>
        /// <param name="endtimestamp">WILL BE IGNORED</param>
        /// <param name="areaid"></param>
        /// <param name="shiftid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="filtertext"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <param name="deduplicatedForDisplay">If set to True results will have deduplicated future tasks</param>
        /// <param name="removeOverdueTasks">If set to True results will have the overdue tasks removed</param>
        /// <param name="periodoffset"></param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/current_week")]
        [Route("tasks/this_week")]
        [HttpGet]
        public async Task<IActionResult> GetTasksThisWeek(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] int? periodoffset, [FromQuery] bool? allowedonly = null, [FromQuery] bool deduplicatedForDisplay = true, [FromQuery] bool removeOverdueTasks = true, [FromQuery] bool removePastTasks = true)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.ThisWeek, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: recurrencytype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly, periodoffset: periodoffset, deduplicatedForDisplay: deduplicatedForDisplay, removeOverdueTasks: removeOverdueTasks, removePastTasks: removePastTasks);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_WEEK_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/week/{weeknr}")]
        [HttpGet]
        public async Task<IActionResult> GetTasksWeekBasedOnNumber([FromRoute]int weeknr, string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.ByWeekNr, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: recurrencytype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly, nr: weeknr);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_BYWEEKNR_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/day/{daynr}")]
        [HttpGet]
        public async Task<IActionResult> GetTasksDayBasedOnNumber([FromRoute] int daynr, string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.ByDayOfYearNr, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: recurrencytype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly,nr: daynr);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_BYDAYNR_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksThisMonth; Will return ALL tasks that have a start_at date on this month (start on 1st of month) 
        /// Start-datetime and end-datetime will be calculated based on either the timestamp supplied or based on now server time converted to company timezone. 
        /// If needed specific filters can be used, no filters are required.
        /// This route can be used as a short form, result can also be retrieved on the /tasks route. 
        /// 
        /// The return data is based on paged data and will default to 200 items. Afterwards can be retrieved per x-number of items. 
        /// Limit 0 or larger that 200 will not be supported. 
        /// 
        /// NOTE! logic not fully finished, weirdness can occur. 
        /// </summary>
        /// <param name="timestamp">WILL BE IGNORED</param>
        /// <param name="starttimestamp">WILL BE IGNORED</param>
        /// <param name="endtimestamp">WILL BE IGNORED</param>
        /// <param name="areaid"></param>
        /// <param name="shiftid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="filtertext"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/current_month")]
        [Route("tasks/this_month")]
        [HttpGet]
        public async Task<IActionResult> GetTasksThisMonth(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.ThisMonth, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: recurrencytype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_MONTH_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksOverdue; Get all overdue tasks based on the current date (timestamp). 
        /// 
        /// The return data is based on paged data and will default to 200 items. Afterwards can be retrieved per x-number of items. 
        /// Limit 0 or page size that 200 will not be supported. 
        /// 
        /// Will return a list of all tasks, of all types (that can be overdue), containing tasks that are not finished yet which do not have a alternative task scheduled for today. 
        /// 
        /// NOTE! logic not fully finished, weirdness can occur. 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="starttimestamp"></param>
        /// <param name="endtimestamp"></param>
        /// <param name="areaid"></param>
        /// <param name="shiftid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="filtertext"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/overdue")]
        [HttpGet]
        public async Task<IActionResult> GetTasksOverdue(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.OverDue, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: recurrencytype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_OVERDUE_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            //TODO add overdue logic.
            var result = await _manager.GetTasksOverdueAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksTypeWeekly; Get all tasks of type weekly
        /// 
        /// The return data is based on paged data and will default to 200 items. Afterwards can be retrieved per x-number of items. 
        /// Limit 0 or page size larger that 200 will not be supported. 
        /// 
        /// The returned data will consist of all tasks of type WEEK that need to be started/finished this week based on the company shift times. 
        /// The time frame can be overridden with a start and end time stamp. 
        /// 
        /// Weekly tasks will map current week for retrieval; Monday till Sunday by default. Shift calculations will be added if needed. 
        /// If you want a specific time-frame use the starttimestamp and endtimestamp. 
        /// All retrieval will be mapped against the task.start_at time. 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="starttimestamp"></param>
        /// <param name="endtimestamp"></param>
        /// <param name="areaid"></param>
        /// <param name="shiftid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="filtertext"></param>
        /// <param name="include"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/type/weekly")]
        [HttpGet]
        public async Task<IActionResult> GetTasksTypeWeekly(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.WeekType, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: RecurrencyTypeEnum.Week, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TYPEWEEKLY_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/type/shift")]
        [HttpGet]
        public async Task<IActionResult> GetTasksTypeShift(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid,  [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.ShiftType, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: RecurrencyTypeEnum.Shifts, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TYPESHIFT_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        /// <summary>
        /// GetTasksMonthly; Get all tasks of type monthly or month
        /// 
        /// The return data is based on paged data and will default to 200 items. Afterwards can be retrieved per x-number of items. 
        /// Limit 0 or page size larger that 200 will not be supported. 
        /// 
        /// The returned data will consist of all tasks of type MONTH that need to be started/finished this WEEK based on the company shift times. 
        /// The time frame can be overridden with a start and end time stamp. 
        /// 
        /// Monthly tasks will map current week for retrieval; Monday till Sunday by default. Shift calculations will be added if needed. 
        /// If you want a specific time-frame use the starttimestamp and endtimestamp. 
        /// All retrieval will be mapped against the task.start_at time. 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="starttimestamp"></param>
        /// <param name="endtimestamp"></param>
        /// <param name="filtertext"></param>
        /// <param name="areaid"></param>
        /// <param name="shiftid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <returns></returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/type/monthly")]
        [HttpGet]
        public async Task<IActionResult> GetTasksMonthly(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid,  [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.MonthType, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: RecurrencyTypeEnum.Month, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TYPEMONTHLY_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/type/daily")]
        [HttpGet]
        public async Task<IActionResult> GetTasksDaily(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid,  [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.MonthType, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: RecurrencyTypeEnum.PeriodDay, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TYPEDAILY_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/type/dailydynamic")]
        [HttpGet]
        public async Task<IActionResult> GetTasksDailyDynamic(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.DailyDynamicType, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: RecurrencyTypeEnum.DynamicDay, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TYPEDAILYDYNAMIC_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("tasks/type/onetimeonly")]
        [HttpGet]
        public async Task<IActionResult> GetTasksOneTimeOnly(string timestamp, string starttimestamp, string endtimestamp, string filtertext, [FromQuery] int? areaid, [FromQuery] int? shiftid, [FromQuery] int? templateid,  [FromQuery] string? statusids, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            TaskFilters filters = await GetTaskFilters(RetrievalTypeEnum.OneTimeOnlyType, timestamp: timestamp, starttimestamp: starttimestamp, endtimestamp: endtimestamp, filtertext: filtertext, areaid: areaid, shiftid: shiftid, templateid: templateid, statusids: statusids, recurrencytype: RecurrencyTypeEnum.NoRecurrency, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly);

            if (filters.Timestamp == DateTime.MinValue && filters.StartTimestamp == DateTime.MinValue && filters.EndTimestamp == DateTime.MinValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No date/time or start date / end date combination supplied.");
            }

            var uniqueKey = string.Format("GET_TASKS_GEN4_TYPEONETIMEONLY_T{0}_C{1}_U{2}_L{3}_O{4}", filters.StartTimestamp != DateTime.MinValue && filters.EndTimestamp != DateTime.MinValue ? string.Concat(filters.StartTimestamp.Value.ToString("dd-MM-yyyy_HH:mm"), filters.EndTimestamp.Value.ToString("dd-MM-yyyy_HH:mm")) : filters.Timestamp.Value.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksSplitByTypeAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: filters.Timestamp, starttimestamp: filters.StartTimestamp, endtimestamp: filters.EndTimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasks/landingpagestats")]
        [HttpGet]
        public async Task<IActionResult> GetTasksLandingPageStats([FromQuery] int areaid)
        {
            _manager.Culture = TranslationLanguage;

            var uniqueKey = string.Format("GET_TASKS_GEN4_LANDINGPAGE_T{0}_C{1}_U{2}_P{3}", DateTime.UtcNow, await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), Request.HttpContext.Request.Path.ToString());
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_TASKS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskLandingPageStats(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), areaid: areaid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("task{taskId}/statushistory")]
        [HttpGet]

        public async Task<IActionResult> GetTaskStatusHistory([FromRoute] int taskId)
        {
            var result = new List<TasksTaskStatus>();

            if (!TaskValidators.TaskIdIsValid(taskId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskId, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }


            result = await _manager.GetTaskStatusHistoryAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
        }
        #endregion

        #region - POST routes tasks -

        [Route("task/setstatus/{taskId}")]
        [HttpPost]
        public async Task<IActionResult> ChangeTaskStatus([FromRoute] int taskId, [FromQuery] TaskStatusEnum status, [FromQuery] String? version)
        {
            if (!TaskValidators.TaskIdIsValid(taskId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskId, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            SignBasic signature = new SignBasic()
            {
                Status = status,
                SignedAtUtc = DateTime.UtcNow,
                SignedById = await CurrentApplicationUser.GetAndSetUserIdAsync(),
                Version = version
            };

            List<String> possibleMessages = await ValidateStatusChange(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                       taskId: taskId,
                                                                       signature: signature);

            if (possibleMessages.Count>0)
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: signature.ToJsonFromObject(), response: String.Join(", ", possibleMessages));
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.SetTaskStatusSignAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskId, userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), signBasic: signature);

            if (result)
            {
                var resultfull = await _manager.GetTaskAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskId, include: "tasks,propertyuservalues,tags");
                
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, (result).ToJsonFromObject());
            }

        }

        #endregion

        #region - private functions - 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="retrievalType"></param>
        /// <param name="timestamp"></param>
        /// <param name="starttimestamp"></param>
        /// <param name="endtimestamp"></param>
        /// <param name="filtertext"></param>
        /// <param name="areaid"></param>
        /// <param name="shiftid"></param>
        /// <param name="templateid"></param>
        /// <param name="statusids"></param>
        /// <param name="recurrencytype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <param name="periodoffset"></param>
        /// <param name="nr"></param>
        /// <param name="deduplicatedForDisplay"></param>
        /// <param name="removeOverdueTasks"></param>
        /// <returns></returns>
        [NonAction]
        private async Task<TaskFilters> GetTaskFilters(RetrievalTypeEnum retrievalType, string timestamp, string? starttimestamp, string? endtimestamp, string filtertext, int? areaid, int? shiftid, int? templateid, string? statusids, RecurrencyTypeEnum? recurrencytype, string tagids, string include, int? limit, int? offset, bool? allowedonly, int? periodoffset = 0,int? nr = 0, bool deduplicatedForDisplay = false, bool removeOverdueTasks = false, bool removePastTasks  = false)
        {
            await Task.CompletedTask;
            //TODO add datetime created based on retrieval type if dates are not supplied. 

            DateTime parsedTimeStamp = DateTime.MinValue;
            DateTime parsedStartTimeStamp = DateTime.MinValue;
            DateTime parsedEndTimeStamp = DateTime.MinValue;

            //try parse certain timestamp values. 
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { }
                else if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { }
                else if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { }
                else if (DateTime.TryParseExact(timestamp, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { }

            }
            else if (!string.IsNullOrEmpty(starttimestamp) && !string.IsNullOrEmpty(endtimestamp))
            {
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimeStamp)) { }
                else if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimeStamp)) { }
                else if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimeStamp)) { }
                else if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimeStamp)) { };

                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimeStamp)) { }
                else if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimeStamp)) { }
                else if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimeStamp)) { }
                else if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimeStamp)) { };
            }

            //TODO move date calcs to extensions of feasible. 

            //set time frames for retrieval
            if (retrievalType == RetrievalTypeEnum.Today)
            {
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                if (periodoffset.HasValue && periodoffset.Value != 0) { parsedTimeStamp = parsedTimeStamp.AddDays(periodoffset.Value);  }
            }
            else if (retrievalType == RetrievalTypeEnum.ThisShift)
            {
                if(parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                ShiftTimestamps shiftTimes = await _shiftManager.GetShiftTimestampsByOffsetAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, shiftOffset: periodoffset ?? 0);
                parsedStartTimeStamp = shiftTimes.Start;
                parsedEndTimeStamp = shiftTimes.End;
                parsedTimeStamp = shiftTimes.Start;
            }
            else if (retrievalType == RetrievalTypeEnum.ThisWeek)
            {
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                if (periodoffset.HasValue && periodoffset.Value != 0) { parsedTimeStamp = parsedTimeStamp.AddDays(periodoffset.Value * 7); }
                //get monday
                parsedStartTimeStamp = parsedTimeStamp.Date.AddDays(-(int)parsedTimeStamp.Date.DayOfWeek + (int)DayOfWeek.Monday);
                //cals next monday
                parsedEndTimeStamp = parsedStartTimeStamp.AddDays(7);
                if (!removePastTasks)
                {
                    parsedTimeStamp = DateTime.MinValue;
                }
            }
            else if (retrievalType == RetrievalTypeEnum.ThisMonth)
            {
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                if (periodoffset.HasValue && periodoffset.Value != 0) { parsedTimeStamp.AddMonths(periodoffset.Value); }

                //get first day of month
                parsedStartTimeStamp = new DateTime(parsedTimeStamp.Year, parsedTimeStamp.Month, 1);
                //get last day of month
                parsedEndTimeStamp = parsedStartTimeStamp.AddMonths(1);
            }
            else if (retrievalType == RetrievalTypeEnum.ThisQuarter)
            {
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                if (periodoffset.HasValue && periodoffset.Value != 0) { parsedTimeStamp.AddMonths(periodoffset.Value * 3); }
                int quarterNumber = (parsedTimeStamp.Month - 1) / 3 + 1; //get quarter number
                //calc first day
                parsedStartTimeStamp = new DateTime(parsedTimeStamp.Year, (quarterNumber - 1) * 3 + 1, 1);
                //calc last day (start of next quarter at 00:00:00
                parsedEndTimeStamp = parsedStartTimeStamp.AddMonths(3);

            }
            else if (retrievalType == RetrievalTypeEnum.ThisYear)
            {
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                if (periodoffset.HasValue && periodoffset.Value != 0) { parsedTimeStamp.AddYears(periodoffset.Value); }
                parsedStartTimeStamp = new DateTime(parsedTimeStamp.Year, 1, 1);
                //calc last day (start of next quarter at 00:00:00
                parsedEndTimeStamp = parsedStartTimeStamp.AddYears(1);
            }
            else if (retrievalType == RetrievalTypeEnum.ByWeekNr)
            {
                //based on ISO 8601
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }

                DateTime firstDayOfYear = new DateTime(parsedTimeStamp.Year, 1, 1);
                int daysOffset = DayOfWeek.Thursday - firstDayOfYear.DayOfWeek;

                // Use first Thursday in January to get first week of the year as it will never be in Week 52/53
                DateTime firstThursday = firstDayOfYear.AddDays(daysOffset);
                var cal = CultureInfo.CurrentCulture.Calendar;
                int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                var weekNum = nr.HasValue ? nr.Value : 1;
                // As we're adding days to a date in Week 1,
                // we need to subtract 1 in order to get the right date for week #1
                if (firstWeek == 1)
                {
                    weekNum -= 1;
                }

                // Using the first Thursday as starting week ensures that we are starting in the right year
                // then we add number of weeks multiplied with days
                var resultWeek = firstThursday.AddDays(weekNum * 7);

                if (periodoffset.HasValue && periodoffset.Value != 0) { resultWeek.AddDays(periodoffset.Value * 7); }

                parsedStartTimeStamp = resultWeek.AddDays(-3); // Subtract 3 days from Thursday to get Monday, which is the first weekday in ISO8601
                parsedEndTimeStamp = parsedStartTimeStamp.AddDays(7);
            }
            else if (retrievalType == RetrievalTypeEnum.ByMonthNr) 
            {
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                if (periodoffset.HasValue && periodoffset.Value != 0) { parsedTimeStamp.AddDays(periodoffset.Value); }

                parsedStartTimeStamp = new DateTime(parsedTimeStamp.Year, nr.HasValue ? nr.Value : 1, 1); 
                parsedEndTimeStamp = parsedStartTimeStamp.AddMonths(1);
            }
            else if (retrievalType == RetrievalTypeEnum.ByYearNr)
            {
                if (parsedTimeStamp == DateTime.MinValue) { parsedTimeStamp = DateTime.Now; }
                if (periodoffset.HasValue && periodoffset.Value != 0) { parsedTimeStamp.AddYears(periodoffset.Value); }

                parsedStartTimeStamp = new DateTime(nr.HasValue ? nr.Value : parsedTimeStamp.Year, 1, 1);
                parsedEndTimeStamp = parsedStartTimeStamp.AddYears(1);
            }

            if (parsedTimeStamp == DateTime.MinValue && removePastTasks)
            {
                parsedTimeStamp = DateTime.Now; //default to now
            }

            //Adding recurrencyTypes
            List<RecurrencyTypeEnum> recurrencyTypes = new List<RecurrencyTypeEnum>();
            if (retrievalType == RetrievalTypeEnum.ThisWeek)
            {
                recurrencyTypes.Add(RecurrencyTypeEnum.Week);
                recurrencyTypes.Add(RecurrencyTypeEnum.Month);
                recurrencyTypes.Add(RecurrencyTypeEnum.PeriodDay);
                recurrencyTypes.Add(RecurrencyTypeEnum.DynamicDay);

            }
            if (recurrencytype != null)
            {
                recurrencyTypes.RemoveAll(x => x != recurrencytype);
                recurrencyTypes.Add((RecurrencyTypeEnum)recurrencytype);
            }



            var filters = new TaskFilters()
                {
                    AreaId = areaid,
                    ShiftId = shiftid,
                    TemplateId = templateid,
                    Statuses = string.IsNullOrEmpty(statusids) ? null : statusids.Split(",").Select(id => (TaskStatusEnum)Convert.ToInt32(id)).ToList<TaskStatusEnum>(),
                    RecurrencyTypes = recurrencyTypes,
                    AllowedOnly = allowedonly, //TODO need to check
                    FilterText = filtertext,
                    Offset = offset,
                    Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                    TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                    Timestamp = parsedTimeStamp,
                    StartTimestamp = parsedStartTimeStamp,
                    EndTimestamp = parsedEndTimeStamp,
                    DeduplicatedForDisplay = deduplicatedForDisplay,
                    RemoveOverDueTasks = removeOverdueTasks
                };

            return filters;
        }

        [NonAction]
        private RetrievalTypeEnum PathToRetrievalType(string path)
        {

            RetrievalTypeEnum output = RetrievalTypeEnum.Default;

            switch (path)
            {
                case "/gen4/tasks_counts": output = RetrievalTypeEnum.Default; break;
                case "/gen4/tasks/counts": output = RetrievalTypeEnum.Default; break;
                case "/gen4/tasks/today/counts": output = RetrievalTypeEnum.Today; break;
                case "/gen4/tasks/this_shift/counts": output = RetrievalTypeEnum.ThisShift; break;
                case "/gen4/tasks/this_week/counts": output = RetrievalTypeEnum.ThisWeek; break;
                case "/gen4/tasks/this_month/counts": output = RetrievalTypeEnum.ThisMonth; break;
                case "/gen4/tasks/overdue/counts": output = RetrievalTypeEnum.OverDue; break;
                case "/gen4/tasks/type/weekly/counts": output = RetrievalTypeEnum.WeekType; break;
                case "/gen4/tasks/type/shift/counts": output = RetrievalTypeEnum.ShiftType; break;
                case "/gen4/tasks/type/monthly/counts": output = RetrievalTypeEnum.MonthType; break;
                case "/gen4/tasks/type/daily/counts": output = RetrievalTypeEnum.DailyPeriodType; break;
                case "/gen4/tasks/type/dailydynamic/counts": output = RetrievalTypeEnum.DailyDynamicType; break;
                case "/gen4/tasks/type/onetimeonly/counts": output = RetrievalTypeEnum.OneTimeOnlyType; break;
            }

            return output;
        }

        private async Task<List<string>> ValidateStatusChange(int companyId, int taskId, SignBasic signature)
        {
            List<String> result = new List<String>();

            List<TasksTaskStatus> history = await _manager.GetTaskStatusHistoryAsync(companyId, taskId);

            //cannot set to todo if no previous states have been set
            if (history.Count == 0 && signature.Status == TaskStatusEnum.Todo)
            {
                result.Add("Cannot untap if task has not been tapped yet");
            }

            //Following checks are only logical if there is history
            if (history.Count > 0)
            {
                //when user untaps (sets to todo) the user must have already made changes to the status of the task
                if (history.FindAll(x => x.SignedById == signature.SignedById).Count() == 0 &&
                   signature.Status == TaskStatusEnum.Todo)
                {
                    result.Add("Cannot untap if user has not set status of task before");
                }

                //cannot change to status that already is last set
                if (history.Last().Status == signature.Status)
                {
                    result.Add(string.Concat("Cannot set status to ", signature.Status.ToString(), " because that status is already set"));
                }
            }

            return result;
        }

        #endregion


        #region - health checks -
        /// <summary>
        /// GetTasksHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("tasks/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetTasksHealth()
        {
            try
            {
                var result = await _manager.GetTasksSplitByTypeAsync(timestamp: DateTime.Now.AddMonths(-5), companyId: _configurationHelper.GetValueAsInteger(ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), userId:0, filters: new TaskFilters() { Limit = ApiSettings.HEALTHCHECK_ITEM_LIMIT });

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

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
    }
}