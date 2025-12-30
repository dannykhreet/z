using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models;
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
    /// TaskTemplateStepsController; Steps are usually not directly called. But this controller will add functionality to do so.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class TaskTemplateStepsController : BaseController<TaskTemplateStepsController>
    {
        #region - privates -
        private readonly ITaskManager _manager;
        private readonly IUserManager _userManager;
        private readonly IToolsManager _toolsManager;
        #endregion

        #region - contructor(s) -
        public TaskTemplateStepsController(ITaskManager manager, IUserManager userManager, IToolsManager toolsManager, IConfigurationHelper configurationHelper, ILogger<TaskTemplateStepsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _userManager = userManager;
            _toolsManager = toolsManager;
        }
        #endregion

        #region - GET routes tasktemplatesteps -
        [Route("tasktemplatesteps")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplateSteps()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplateStepsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("tasktemplatestep/{stepid}")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplateStep([FromRoute]int stepid)
        {
            if (!TaskValidators.StepIdIsValid(stepid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_STEP_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplateStepAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), stepId: stepid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - POST routes tasktemplatesteps -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplatestep/add")]
        [HttpPost]
        public async Task<IActionResult> AddTaskTemplateSte([FromBody] Step step)
        {
            if(_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!this.IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!step.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                         userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                         messages: out var possibleMessages,
                                            validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: step.ToJsonFromObject(), response: possibleMessages);
                    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                }

                var result = await _manager.AddTaskTemplateStepAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), step: step);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplatestep/change/{stepid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeTaskTemplateStep([FromRoute]int stepId, [FromBody] Step step)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!this.IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!TaskValidators.StepIdIsValid(stepId))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_STEP_ID_IS_NOT_VALID.ToJsonFromObject());
                }

                if (!step.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                         validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                {
                    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: step.ToJsonFromObject(), response: possibleMessages);
                    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                }

                var result = await _manager.ChangeTaskTemplateStepAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), stepId: stepId, step: step);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplatestep/setactive/{stepid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveTaskTemplate([FromRoute]int stepid, [FromBody] object isActive)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:PartialUpdatesOfObjectsEnabled"))
            {
                if (!this.IsCmsRequest)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
                }

                if (!TaskValidators.StepIdIsValid(stepid))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_STEP_ID_IS_NOT_VALID.ToJsonFromObject());
                }

                if (!BooleanValidator.CheckValue(isActive))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
                }


                var result = await _manager.SetTaskTemplateStepActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), stepId: stepid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            return StatusCode((int)HttpStatusCode.NotFound, ("").ToJsonFromObject());

        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetTaskTemplateStepsHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("tasktemplatesteps/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplateStepsHealth()
        {
            try
            {
                var result = await _manager.GetTaskTemplateStepsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new TaskFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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