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
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.MarketPlace)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class MarketPlaceController : BaseController<MarketPlaceController>
    {
        #region - privates -
        private readonly IMarketPlaceManager _manager;
        #endregion

        #region - contructor(s) -
        public MarketPlaceController(IMarketPlaceManager manager, IConfigurationHelper configurationHelper, ILogger<MarketPlaceController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }
        #endregion

        #region - GET routes marketplace -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("marketplace")]
        [HttpGet]
        public async Task<IActionResult> GetMarketPlace()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetMarketPlace(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        #endregion

        #region - POST routes marketplace -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("marketplace/config/save")]
        [HttpPost]
        public async Task<IActionResult> PostMarketplaceConfig([FromBody] string marketconfiguration)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            await Task.CompletedTask;
            var result = "";

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetShiftsAsync; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("marketplace/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetMarketPlaceHealth()
        {
            try
            {
                var result = await _manager.GetMarketPlace(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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