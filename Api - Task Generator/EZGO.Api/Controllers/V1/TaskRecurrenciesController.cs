using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZ.Connector.Init.Interfaces;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// TaskRecurrenciesController; contains all routes based on task recurrencies.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class TaskRecurrenciesController : BaseController<TaskRecurrenciesController>
    {
        #region - privates -
        private readonly ITaskManager _manager;
        private readonly IUserManager _userManager;
        private readonly IToolsManager _toolsManager;
        #endregion

        #region - contructor(s) -
        public TaskRecurrenciesController(ITaskManager manager, IUserManager userManager,  IToolsManager toolsManager, IConfigurationHelper configurationHelper, ILogger<TaskRecurrenciesController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _userManager = userManager;
            _toolsManager = toolsManager;
        }
        #endregion

        #region - GET routes taskrecurrencies -
        [Route("taskrecurrencies")]
        [HttpGet]
        public async Task<IActionResult> GetTaskRecurrencies([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] MonthRecurrencyTypeEnum? monthrecurrencytype, [FromQuery] RecurrencyTypeEnum? recurrencytype, [FromQuery] int? templateid, [FromQuery] int? shiftid, [FromQuery] string include, [FromQuery] string weekdays)
        {
            var filters = new TaskFilters() { AreaId = areaid, FilterAreaType = filterareatype, MonthRecurrencyType = monthrecurrencytype, RecurrencyType = recurrencytype, ShiftId = shiftid, TemplateId = templateid}; //TODO refactor
            if(!string.IsNullOrEmpty(weekdays)) { filters.Weekdays = weekdays.Split(",").Select(x => Convert.ToInt32(x)).ToList(); };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskRecurrenciesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("taskrecurrency/{taskrecurrencyid}")]
        [HttpGet]
        public async Task<IActionResult> GetTaskRecurrency([FromRoute]int taskrecurrencyid)
        {
            if (!TaskValidators.RecurrencyIdIsValid(taskrecurrencyid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_RECURRENCY_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskRecurrencyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskRecurrencyId: taskrecurrencyid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - POST routes taskrecurrencies -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("taskrecurrency/add")]
        [HttpPost]
        public async Task<IActionResult> AddTaskRecurrency([FromBody] TaskRecurrency taskrecurrency)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!this.IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!taskrecurrency.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                            userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                            messages: out var possibleMessages,
                                              validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: taskrecurrency.ToJsonFromObject(), response: possibleMessages);
                    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                }

                var result = await _manager.AddTaskRecurrencyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskRecurrency: taskrecurrency);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("taskrecurrency/change/{taskrecurrencyid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAddTaskRecurrency([FromRoute] int taskrecurrencyid, [FromBody] TaskRecurrency taskrecurrency)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!this.IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!taskrecurrency.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                           userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                           messages: out var possibleMessages,
                                             validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: taskrecurrency.ToJsonFromObject(), response: possibleMessages);
                    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                }

                if (!TaskValidators.RecurrencyIdIsValid(taskrecurrencyid))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_RECURRENCY_ID_IS_NOT_VALID.ToJsonFromObject());
                }

                var result = await _manager.ChangeTaskRecurrencyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskRecurrencyId: taskrecurrencyid, taskRecurrency: taskrecurrency);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("taskrecurrency/setactive/{taskrecurrencyid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveTaskRecurrency([FromRoute] int taskrecurrencyid, [FromBody] object isActive)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!this.IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!TaskValidators.RecurrencyIdIsValid(taskrecurrencyid))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_RECURRENCY_ID_IS_NOT_VALID.ToJsonFromObject());
                }

                if (!BooleanValidator.CheckValue(isActive))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
                }

                var result = await _manager.SetTaskRecurrencyActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskRecurrencyId: taskrecurrencyid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());

        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetTaskRecurrencies; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("taskrecurrencies/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetTaskRecurrenciesHealth()
        {
            try
            {
                var result = await _manager.GetTaskRecurrenciesAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new TaskFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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