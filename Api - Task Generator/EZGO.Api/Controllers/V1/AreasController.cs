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
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
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
    /// AreasController; contains all routes based on area.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class AreasController : BaseController<AreasController>
    {
        #region - privates -
        private readonly IAreaManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        #endregion

        #region - constructor(s) -
        public AreasController(IUserManager userManager, IAreaManager manager, IConfigurationHelper configurationHelper, IToolsManager toolsManager, ILogger<AreasController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
        }
        #endregion

        #region - GET routes areas -
        [Route("areas")]
        [HttpGet]
        public async Task<IActionResult> GetAreas([FromQuery] bool? allowedonly = null, [FromQuery] int? maxlevel = 2, [FromQuery] bool usetreeview = true, string include = null)
        {
            //TODO validate maxlevel + usetreeview

            var filters = new AreaFilters() { AllowedOnly = allowedonly }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAreasAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), maxLevel: maxlevel.Value, useTreeview: usetreeview, filters: filters, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        //Gets all areas of a company no matter the userId.
        [Route("areascompany")]
        [HttpGet]
        public async Task<IActionResult> GetAreasCompany([FromQuery] bool? allowedonly = null, [FromQuery] int? maxlevel = 3, [FromQuery] bool usetreeview = false, string include = null)
        {
            //TODO validate maxlevel + usetreeview

            var filters = new AreaFilters() { AllowedOnly = allowedonly }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAreasAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), maxLevel: maxlevel.Value, useTreeview: usetreeview, filters: filters, include: include);

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("area/{areaid}")]
        [HttpGet]
        public async Task<IActionResult> GetArea([FromRoute] int areaid, [FromQuery] int? maxlevel = 2, [FromQuery] bool usetreeview = true, string include = null)
        {
            if (!AreaValidators.AreaIdIsValid(areaid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AreaValidators.MESSAGE_AREA_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (areaid > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: areaid, objectType: ObjectTypeEnum.Area))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAreaAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), areaId: areaid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Get the names of the areas based on provided list of ids
        /// </summary>
        /// <param name="ids">area ids to get the names for</param>
        /// <returns>dictionary of area ids with area names</returns>
        [Route("areas/names")]
        [HttpGet]
        public async Task<IActionResult> GetAreaNames([FromQuery] List<int> ids)
        {
            if (ids == null || ids.Count == 0) { return BadRequest(); }

            var result = await _manager.GetAreaNamesAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), areaIds: ids);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return Ok(result);
        }
        #endregion

        #region - POST routes area -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("area/add")]
        [HttpPost]
        public async Task<IActionResult> AddArea([FromBody] Area area, [FromQuery] bool fulloutput)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (area.ParentId.HasValue && area.ParentId > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: area.ParentId.Value, objectType: ObjectTypeEnum.Area))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (area.CompanyId != await this.CurrentApplicationUser.GetAndSetCompanyIdAsync())
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDENCOMPANY_OBJECT.ToJsonFromObject());
            }

            if (!area.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: area.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.AddAreaAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), area: area);

            if (fulloutput && result > 0)
            {
                var resultfull = await _manager.GetAreaAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), connectionKind: Data.Enumerations.ConnectionKind.Writer, areaId: result, include: ApiSettings.FULL_INCLUDE_LIST);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("area/change/{areaid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeArea([FromRoute] int areaid, [FromBody] Area area, [FromQuery] bool fulloutput)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AreaValidators.AreaIdIsValid(areaid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AreaValidators.MESSAGE_AREA_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (area.ParentId.HasValue && area.ParentId > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: area.ParentId.Value, objectType: ObjectTypeEnum.Area))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: area.Id, objectType: ObjectTypeEnum.Area) && !await this.CurrentApplicationUser.CheckObjectRights(objectId: areaid, objectType: ObjectTypeEnum.Area))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (area.CompanyId != await this.CurrentApplicationUser.GetAndSetCompanyIdAsync())
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDENCOMPANY_OBJECT.ToJsonFromObject());
            }

            if (!area.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: area.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChangeAreaAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId: areaid, area: area);

            if (fulloutput && result)
            {
                var resultfull = await _manager.GetAreaAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), connectionKind: Data.Enumerations.ConnectionKind.Writer, areaId: areaid, include: ApiSettings.FULL_INCLUDE_LIST);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("area/setactive/{areaid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveArea([FromRoute] int areaid, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AreaValidators.AreaIdIsValid(areaid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AreaValidators.MESSAGE_AREA_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: areaid, objectType: ObjectTypeEnum.Area))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetAreaActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId: areaid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            if(result) { }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - area checks and related routes -
        [Route("area/relations/check/{areaid}")]
        [HttpGet]
        public async Task<IActionResult> GetAreaHasRelations([FromRoute] int areaid)
        {
            if (!AreaValidators.AreaIdIsValid(areaid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AreaValidators.MESSAGE_AREA_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (areaid > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: areaid, objectType: ObjectTypeEnum.Area))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetAreaHasActiveRelations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), areaId: areaid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("area/relations/number/{areaid}")]
        [HttpGet]
        public async Task<IActionResult> GetAreaNrRelations([FromRoute] int areaid)
        {
            if (!AreaValidators.AreaIdIsValid(areaid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AreaValidators.MESSAGE_AREA_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (areaid > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: areaid, objectType: ObjectTypeEnum.Area))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetAreaNumberActiveRelations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), areaId: areaid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetAreasHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("areas/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetAreasHealth()
        {
            try
            {
                var result = await _manager.GetAreasAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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