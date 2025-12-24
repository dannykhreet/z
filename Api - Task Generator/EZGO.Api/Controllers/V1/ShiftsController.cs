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
    /// ShiftsController; contains all routes based on shifts.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ShiftsController : BaseController<ShiftsController>
    {
        #region - privates -
        private readonly IShiftManager _manager;
        #endregion

        #region - contructor(s) -
        public ShiftsController(IShiftManager manager, IConfigurationHelper configurationHelper, ILogger<ShiftsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser,configurationHelper)
        {
            _manager = manager;
        }
        #endregion

        #region - GET routes shifts -
        [Route("shifts")]
        [HttpGet]
        public async Task<IActionResult> GetShifts([FromQuery] int? day, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype)
        {
            var filters = new ShiftFilters() { AreaId = areaid, FilterAreaType = filterareatype, Day = day }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetShiftsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("shift/{shiftid}")]
        [HttpGet]
        public async Task<IActionResult> GetShift([FromRoute]int shiftid)
        {
            if (!ShiftValidators.ShiftIdIsValid(shiftid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ShiftValidators.MESSAGE_SHIFT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetShiftAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), shiftId:shiftid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }
        #endregion

        #region - POST routes shift -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("shift/add")]
        [HttpPost]
        public async Task<IActionResult> AddShift([FromBody] Shift shift)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (shift.CompanyId.HasValue && shift.CompanyId.Value > 0 && !await this.CurrentApplicationUser.CheckObjectCompanyRights(objectCompanyId: shift.CompanyId.Value, objectType: ObjectTypeEnum.Shift))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.AddShiftAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), shift: shift);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("shift/change/{shiftid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeShift([FromRoute]int shiftid, [FromBody] Shift shift)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!ShiftValidators.ShiftIdIsValid(shiftid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ShiftValidators.MESSAGE_SHIFT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (shiftid != shift.Id || !await this.CurrentApplicationUser.CheckObjectRights(objectId: shiftid, objectType: ObjectTypeEnum.Shift))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (shift.CompanyId.HasValue && shift.CompanyId.Value > 0 && !await this.CurrentApplicationUser.CheckObjectCompanyRights(objectCompanyId: shift.CompanyId.Value, objectType: ObjectTypeEnum.Shift))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.ChangeShiftAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), shiftId: shiftid, shift: shift);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("shift/setactive/{shiftid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveShift([FromRoute]int shiftid, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!ShiftValidators.ShiftIdIsValid(shiftid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ShiftValidators.MESSAGE_SHIFT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: shiftid, objectType: ObjectTypeEnum.Shift))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetShiftActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), shiftId: shiftid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetShiftsAsync; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("shifts/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetShiftsHealth()
        {
            try
            {
                var result = await _manager.GetShiftsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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