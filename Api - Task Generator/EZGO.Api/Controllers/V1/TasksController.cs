using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
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
using Microsoft.AspNetCore.Mvc.TagHelpers;
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
    /// TasksController; contains all routes based on tasks.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class TasksController : BaseController<TasksController>
    {
        #region - privates -
        private readonly IMemoryCache _cache;
        private readonly ITaskManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly IGeneralManager _generalManager;
        #endregion

        #region - contructor(s) -
        public TasksController(IUserManager userManager, ITaskManager manager, IGeneralManager generalManager, IMemoryCache memoryCache, IConfigurationHelper configurationHelper, IToolsManager toolsManager, ILogger<TasksController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _cache = memoryCache;
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _generalManager = generalManager;
        }
        #endregion

        #region - GET routes tasks -
        [Route("tasks")]
        [HttpGet]
        public async Task<IActionResult> GetTasks(string timestamp, string starttimestamp, string endtimestamp, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] bool? iscompleted = null)
        {
            _manager.Culture = TranslationLanguage;

            //TODO refactor
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                IsCompleted = iscompleted,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            var uniqueKey = string.Format("GET_TASKS_T{0}_C{1}_U{2}_L{3}_O{4}", (parsedstarttimestamp != DateTime.MinValue && parsedendtimestamp != DateTime.MinValue) ? string.Concat(parsedstarttimestamp.ToString("dd-MM-yyyy_HH:mm"), parsedendtimestamp.ToString("dd-MM-yyyy_HH:mm")) : parsedTimeStamp.ToString("dd-MM-yyyy_HH:mm:s"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
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

            var result = await _manager.GetTasksAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, starttimestamp: parsedstarttimestamp, endtimestamp: parsedendtimestamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }


        [Route("tasks/overdue")]
        [Route("tasksoverdue")]
        [HttpGet]
        public async Task<IActionResult> GetTasksOverdue(string timestamp, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            //TODO refactor
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksOverdueAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasks/statusses")]
        [HttpGet]
        public async Task<IActionResult> GetTasksStatusses(string timestamp, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksStatusAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //get tasks for period
        [Route("tasks/statusses/period")]
        [Route("tasks/statusses/shift")]
        [Route("tasks/statusses/lastweek")]
        [HttpGet]
        public async Task<IActionResult> GetTasksStatusThisPeriod(string from, string to, [FromQuery] int? areaid, [FromQuery] int? templateid, [FromQuery] int? limit, [FromQuery] string tagids, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            DateTime fromTimeStamp;
            if (DateTime.TryParseExact(from, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromTimeStamp)) { };
            DateTime toTimeStamp;
            if (DateTime.TryParseExact(to, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out toTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                TemplateId = templateid
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksStatisticsRelatedToPeriodAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), from: fromTimeStamp, to: toTimeStamp, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasks/extendeddata")]
        [HttpGet]
        public async Task<IActionResult> GetTasksExtendedData(string timestamp, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksExendedDataAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasks/history")]
        [HttpGet]
        public async Task<IActionResult> GetTasksHistory(string timestamp, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksHistoryAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasks/historyfirsts")]
        [HttpGet]
        public async Task<IActionResult> GetTasksHistoryFirsts(string timestamp, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksHistoryFirstsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasks/previousshift")]
        [HttpGet]
        public async Task<IActionResult> GetTasksPreviousShifts(string timestamp, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            //TODO refactor
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                ShiftId = shiftid,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksRelatedToPreviousShiftAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //get tasks for shift
        [Route("tasks/shift")]
        [HttpGet]
        public async Task<IActionResult> GetTasksThisShifts(string timestamp, [FromQuery] string filterText, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            //TODO refactor
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                FilterText = filterText,
                AreaId = areaid,
                FilterAreaType = filterareatype,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                ShiftId = shiftid,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksRelatedToShiftAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //get tasks for yesterday
        [Route("tasks/yesterday")]
        [HttpGet]
        public async Task<IActionResult> GetTasksYesterday(string timestamp, [FromQuery] string filterText, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            //TODO refactor
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                FilterText = filterText,
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            var uniqueKey = string.Format("GET_TASKS_YESTERDAY_T{0}_C{1}_U{2}_L{3}_O{4}", parsedTimeStamp.ToString("dd-MM-yyyy_HH"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks/yesterday", "TECH_TRAFFICSHAPING");
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

            var result = await _manager.GetTasksYesterdayAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //get tasks for yesterday
        [Route("tasks/period")]
        [HttpGet]
        public async Task<IActionResult> GetTasksPeriod(string from, string to, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            //TODO refactor
            DateTime fromTimeStamp;
            if (DateTime.TryParseExact(from, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out fromTimeStamp)) { };
            DateTime toTimeStamp;
            if (DateTime.TryParseExact(to, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out toTimeStamp)) { };

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksPeriodAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), from: fromTimeStamp, to: toTimeStamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //get tasks for last week
        [Route("tasks/lastweek")]
        [HttpGet]
        public async Task<IActionResult> GetTasksLastWeek(string timestamp, [FromQuery] string filterText, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            //TODO refactor
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            var filters = new TaskFilters()
            {
                FilterText = filterText,
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            var uniqueKey = string.Format("GET_TASKS_LAST_WEEK_T{0}_C{1}_U{2}_L{3}_O{4}", parsedTimeStamp.ToString("dd-MM-yyyy_HH"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("tasks/lastweek", "TECH_TRAFFICSHAPING");
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

            var result = await _manager.GetTasksLastWeekAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("taskslatest")]
        [HttpGet]
        public async Task<IActionResult> GetTasksLatest([FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] int? templateId, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetLatestTasks(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), limit: limit.HasValue ? limit.Value : 10, offset: offset.HasValue ? offset.Value : 0, templateId: templateId.HasValue ? templateId.Value : 0, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        //[Route("taskscompleted")]
        //[HttpGet]
        //public async Task<IActionResult> GetTasksCompleted([FromQuery] int? offset, [FromQuery] int? limit, [FromQuery] int? areaId, [FromQuery] int? shiftId)
        //{
        //    Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

        //    var result = await _manager.GetCompletedTasks(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), 
        //                                                  offset: offset.HasValue ? offset.Value : 0, 
        //                                                  limit: limit.HasValue ? limit.Value : 10, 
        //                                                  areaId: areaId.HasValue ? areaId.Value : 0, 
        //                                                  shiftId: shiftId.HasValue ? shiftId.Value : 0);

        //    Agent.Tracer.CurrentSpan.End();

        //    return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        //}

        [Route("tasksactions")]
        [HttpGet]
        public async Task<IActionResult> GetTasksActions([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksActionsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("taskschecklistactions")]
        [HttpGet]
        public async Task<IActionResult> GetTasksChecklistActions([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksChecklistActionsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasksauditactions")]
        [HttpGet]
        public async Task<IActionResult> GetTasksAuditActions([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksAuditActionsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }


        [Route("taskscomments")]
        [HttpGet]
        public async Task<IActionResult> GetTasksComments([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksCommentsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("tasksauditcomments")]
        [HttpGet]
        public async Task<IActionResult> GetTasksAuditComments([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksAuditCommentsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("taskschecklistcomments")]
        [HttpGet]
        public async Task<IActionResult> GetTasksChecklistComments([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] int? shiftid, [FromQuery] int? templateid, [FromQuery] int? recurrencyId, [FromQuery] TaskStatusEnum? status, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ShiftId = shiftid,
                TemplateId = templateid,
                Status = status,
                RecurrencyId = recurrencyId,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASK_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksChecklistCommentsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject(useCamelCasingProperties: false));
        }

        [Route("task/{taskid}")]
        [HttpGet]
        public async Task<IActionResult> GetTask([FromRoute] int taskid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!TaskValidators.TaskIdIsValid(taskid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        #endregion

        #region - POST routes tasks -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("task/add")]
        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] TasksTask task)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!task.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: task.ToJsonFromObject(), response: possibleMessages);
                    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                }

                var result = await _manager.AddTaskAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), possibleOwnerId: (task.Signature != null && task.Signature.SignedById.HasValue && task.Signature.SignedById > 0 ? task.Signature.SignedById.Value : await CurrentApplicationUser.GetAndSetUserIdAsync()), task: task);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("task/change/{taskid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeTask([FromRoute] int taskid, [FromBody] TasksTask task)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!TaskValidators.TaskIdIsValid(taskid))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
                }

                if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }

                if (!task.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: task.ToJsonFromObject(), response: possibleMessages);
                    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                }

                var result = await _manager.ChangeTaskAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), possibleOwnerId: (task.Signature != null && task.Signature.SignedById.HasValue && task.Signature.SignedById > 0 ? task.Signature.SignedById.Value : await CurrentApplicationUser.GetAndSetUserIdAsync()), taskId: taskid, task: task);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());
        }

        [Route("task/changestatus/{taskid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeTaskStatus([FromRoute] int taskid, [FromBody] TasksTask task, [FromQuery] bool fulloutput = false)
        {
            if (!TaskValidators.TaskIdIsValid(taskid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!task.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: task.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChangeTaskStatusAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid, task: task, userId: await CurrentApplicationUser.GetAndSetUserIdAsync());

            if (fulloutput && result)
            {
                var resultfull = await _manager.GetTaskAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid, include: "tasks,propertyuservalues,tags");

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Route("task/changeproperties/{taskid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeTaskProperties([FromRoute] int taskid, [FromBody] List<PropertyUserValue> propertyuservalues, [FromQuery] bool fulloutput = false)
        {
            if (!TaskValidators.TaskIdIsValid(taskid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            foreach (var prop in propertyuservalues)
            {
                if (!prop.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                                messages: out var possibleMessages,
                                                  validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: prop.ToJsonFromObject(), response: possibleMessages);
                    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                }
            }

            var result = await _manager.ChangeTaskPropertyUserValuesAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), taskId: taskid, propertyUserValues: propertyuservalues);
            if (fulloutput && result)
            {
                var currentTask = await _manager.GetTaskAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid, include: "tasks,propertyuservalues,tags");
                var resultfull = currentTask.PropertyUserValues;

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Route("task/setstatus/{taskid}")]
        [HttpPost]
        public async Task<IActionResult> SetStatusTask([FromRoute] int taskid, [FromBody] object status)
        {
            if (!TaskValidators.TaskIdIsValid(taskid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            bool result = false;
            //NOTE to be backwards compatible; The input is based on object (was a number representing a value of the TaskStatusEnum object);
            //A check is added: if still status enum value is posted, this will be used. If not the SignBasic object is used. This object also contains the same enum and extra data for posting.
            if (status != null)
            {
                int statusValue;
                if (int.TryParse(status.ToString(), out statusValue))
                {
                    var statusObject = (TaskStatusEnum)statusValue;
                    result = await _manager.SetTaskStatusAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid, userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), status: statusObject);
                }
                else
                {
                    var statusObject = new SignBasic();
#pragma warning disable CS0168 // Variable is declared but never used
                    try
                    {
                        statusObject = status.ToString().ToObjectFromJson<SignBasic>();
                    }
                    catch (Exception ex) {
                        //for some reason parser failed, build-up object ourselves. 
                        var statusBasicObject = status.ToString().ToObjectFromJson<StatusBasic>();
                        //try to do manual build up.
                        statusObject.Status = statusBasicObject.Status;
                    }
#pragma warning restore CS0168 // Variable is declared but never used


                    if (!string.IsNullOrEmpty(statusObject.Version) &&
                        await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA") &&
                        !(await _manager.GetAvailableTaskTemplateVersionsForTaskAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid)).Contains(statusObject.Version))
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_VERSION_IS_NOT_VALID.ToJsonFromObject());
                    }

                    result = await _manager.SetTaskStatusSignAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid, userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), signBasic: statusObject);

                }
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("task/setstatuswithreason/")]
        [HttpPost]
        public async Task<IActionResult> SetStatusWithReason([FromBody] TaskStatusWithReason taskStatus)
        {
            if (!TaskValidators.TaskIdIsValid(taskStatus.TaskId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskStatus.TaskId, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!taskStatus.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                               userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                               messages: out var possibleMessages,
                                                 validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: taskStatus.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var statusObject = (TaskStatusEnum)taskStatus.Status;
            var result = await _manager.SetTaskStatusWithReasonAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskStatus.TaskId, userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), status: statusObject, comment: taskStatus.Comment, signedAtUtc: taskStatus.SignedAtUtc.ToUniversalTime());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("tasks/setstatusseswithreason/")]
        [HttpPost]
        public async Task<IActionResult> SetStatussesWithReason([FromBody] MultiTaskStatusWithReason multiTaskStatus)
        {
            if (!multiTaskStatus.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                             userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                             messages: out var possibleMessages,
                                               validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: multiTaskStatus.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }


            var result = await _manager.SetTaskStatussesWithReasonAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), multiTaskStatus: multiTaskStatus);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("task/setpictureproof/{taskId}")]
        [HttpPost]
        public async Task<IActionResult> SetTaskPictureProof([FromRoute] int taskId, [FromBody] PictureProof pictureProof)
        {
            if (!TaskValidators.TaskIdIsValid(taskId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskId, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!pictureProof.ValidateAndClean(
                    companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                    userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                    messages: out var possibleMessages,
                    validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null)
                )
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: pictureProof.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChangeTaskPictureProofAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskId, userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), possibleOwnerId: await CurrentApplicationUser.GetAndSetUserIdAsync(), pictureProof: pictureProof);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("task/setactive/{taskid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveTask([FromRoute] int taskid, [FromBody] object isActive)
        {
            if (!TaskValidators.TaskIdIsValid(taskid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetTaskActiveAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), taskId: taskid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("task/settimerealized/{taskid}")]
        [HttpPost]
        public async Task<IActionResult> SetTimeRealizedTask([FromRoute] int taskid, [FromBody] TaskRelationTimeRealized realized)
        {
            if (!TaskValidators.TaskIdIsValid(taskid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TASK_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (realized == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "TaskRelationTimeRealized is not a valid object.".ToJsonFromObject());
            }

            var result = await _manager.SetTaskRealizedAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskId: taskid, realizedById: realized.RealizedById, timeRealized: realized.RealizedTime);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("task/setpropertyvalue/{taskid}")]
        [HttpPost]
        public async Task<IActionResult> SetPropertyValueTask([FromRoute] int taskid, [FromBody] PropertyUserValue propertyValue)
        {
            if (!await CurrentApplicationUser.CheckObjectRights(objectId: taskid, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!propertyValue.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: propertyValue.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = "NOT YET IMPLEMENTED";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetTasksHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("tasks/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetTasksHealth()
        {
            try
            {
                var result = await _manager.GetTasksAsync(timestamp: DateTime.Now.AddMonths(-5), companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new TaskFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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