using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.TaskGeneration;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


namespace EZGO.Api.Controllers.General
{
    //TODO determan if controller needs to move, for now its a general controller seeing there will be only one sync process (can not run multiple onces at the same time)
    //TODO add referrer checks
    /// <summary>
    /// GenerationController; Controller for calling several parts of the generation functionality.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class GenerationController : BaseController<GenerationController>
    {
        #region - privates -
        private readonly ITaskPlanningManager _taskPlanningManager;
        private readonly IUserManager _userManager;
        private readonly IToolsManager _toolsManager;
        #endregion
        #region - constructor(s) -
        public GenerationController(IUserManager userManager, IToolsManager toolsManager, ILogger<GenerationController> logger, IApplicationUser applicationUser, ITaskPlanningManager taskPlanningManager, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper: configurationHelper)
        {
            _taskPlanningManager = taskPlanningManager;
            _toolsManager = toolsManager;
            _userManager = userManager;
        }
        #endregion

        //NOTE DISABLED UNTIL GENERATION PROCESSES IS CORRECT AND THIS NEEDS TO BE ENABLED. (probably not going to do that).

        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/onetimeonly/{companyid}")]
        //public async Task<IActionResult> GenerateOneTimeOnlyCompany([FromRoute] int companyid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateOneTimeOnlyCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}

        //[HttpGet]
        //[Route("generation/generate/onetimeonly/{companyid}/tasktemplate/{templateid}")]
        //public async Task<IActionResult> GenerateOneTimeOnlyCompany([FromRoute] int companyid, [FromRoute] int templateid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateOneTimeOnlyCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}

        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/weekly/{companyid}")]
        //public async Task<IActionResult> GenerateWeeklyCompany([FromRoute]int companyid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateWeeklyCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}

        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/weekly/{companyid}/template/{templateid}")]
        //public async Task<IActionResult> GenerateWeeklyCompany([FromRoute] int companyid, [FromRoute] int templateid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateWeeklyCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}

        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/monthly/{companyid}")]
        //public async Task<IActionResult> GenerateMonthlyCompany([FromRoute]int companyid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateMonthlyCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}

        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/monthly/{companyid}/template/{templateid}")]
        //public async Task<IActionResult> GenerateMonthlyCompany([FromRoute] int companyid, [FromRoute] int templateid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateMonthlyCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}

        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/shifts/{companyid}")]
        //public async Task<IActionResult> GenerateShiftsCompany([FromRoute]int companyid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateShiftsCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());

        //}

        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/shifts/{companyid}/template/{templateid}")]
        //public async Task<IActionResult> GenerateShiftsCompany([FromRoute] int companyid, [FromRoute] int templateid)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        int generationCompanyId = companyid;
        //        if (User.IsInRole(RoleTypeEnum.Staff.ToString().ToLower()) || User.IsInRole(RoleTypeEnum.SuperUser.ToString().ToLower()))
        //        {
        //            //when superuser or staff e.g. ezcompany, than execute based on the supplied companyid
        //            generationCompanyId = companyid;
        //        }
        //        else
        //        {

        //            generationCompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
        //            //check if user and supplied user companyid are the same, if not someone is fiddling arround with the parameters.
        //            if (generationCompanyId != companyid)
        //            {
        //                return StatusCode((int)HttpStatusCode.Unauthorized, "User not authorized to generate tasks for this company.".ToJsonFromObject());
        //            }
        //        }

        //        var output = await _generationManager.GenerateShiftsCompany(companyId: generationCompanyId);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());

        //}


        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/all/{companyid}")]
        //public async Task<IActionResult> GenerateAllCompany([FromRoute] int companyid, CancellationToken stoppingToken)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        var output = await _generationManager.GenerateAllCompany(companyId: companyid, stoppingToken: stoppingToken);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}


        //[HttpGet]
        //[Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        //[Route("generation/generate/all")]
        //public async Task<IActionResult> GenerateAll(CancellationToken stoppingToken)
        //{
        //    if (CheckGenerationEnabled())
        //    {
        //        var output = await _generationManager.GenerateAll(stoppingToken: stoppingToken);
        //        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        //    }
        //    return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        //}

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("generation/set_planning")]
        public async Task<IActionResult> SetPlanning([FromBody]PlanningConfiguration planningConfiguration)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!planningConfiguration.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                          userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                          messages: out var possibleMessages,
                                  validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: planningConfiguration.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            //Add validation?

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var returnedId = await _taskPlanningManager.SavePlanningConfiguration(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), 
                                                                              userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), 
                                                                              planning: planningConfiguration);


            AppendCapturedExceptionToApm(_taskPlanningManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, returnedId.ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("generation/planning")]
        public async Task<IActionResult> GetPlanning()
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _taskPlanningManager.GetPlanningConfiguration(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_taskPlanningManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        }

        [HttpGet]
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("generation/logs")]
        public async Task<IActionResult> GetGenerationLogs()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.MethodNotAllowed, "Currently not available due to configuration settings.".ToJsonFromObject());
        }

        /// <summary>
        /// CheckGenerationEnabled, check if generation is enabled based on EnableTaskGenerationFromRoutes in appsettings.
        /// </summary>
        /// <returns>true/false</returns>
        private bool CheckGenerationEnabled()
        {
            var enabled = _configurationHelper.GetValueAsString("AppSettings:EnableTaskGenerationFromRoutes");
            if(enabled != null && Convert.ToBoolean(enabled))
            {
                return true;
            }
            return false;
        }

    }
}