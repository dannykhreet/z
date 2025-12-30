using Amazon.Runtime.Internal.Util;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZ.Connector.Init.Interfaces;
using EZ.Connector.Init.Managers;
using EZ.Connector.SAP.Interfaces;
using EZ.Connector.Ultimo.Interfaces;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
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
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// VirtualTeamLeadController; contains all routes based on virtual team lead.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class VirtualTeamLeadController : BaseController<VirtualTeamLeadController>
    {
        #region - privates -
        private readonly IMemoryCache _cache;
        private readonly IVirtualTeamLeadManager _manager;
        private readonly IGeneralManager _generalManager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        #endregion

        #region - constructor(s) -
        public VirtualTeamLeadController(IUserManager userManager, IVirtualTeamLeadManager manager, IGeneralManager generalManager, IMemoryCache memoryCache, IConfigurationHelper configurationHelper, IToolsManager toolsManager, ILogger<VirtualTeamLeadController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _cache = memoryCache;
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _generalManager = generalManager;
        }
        #endregion

        [Route("fetchshiftmessage")]
        [HttpGet]
        public async Task<IActionResult> FetchShiftMessage()
        {
            var result = await _manager.fetchShiftMessage();
            if (String.IsNullOrEmpty(result))
            {
                return NotFound(new { message = $"No shiftmessage found" });
            }
            return StatusCode((int)HttpStatusCode.OK, result);
        }

        [Route("generateshiftmessage")]
        [HttpGet]
        public async Task<IActionResult> GenerateShiftMessage()
        {
            var result = await _manager.generateShiftMessage();
            if (String.IsNullOrEmpty(result))
            {
                return NotFound(new { message = $"No shiftmessage found" });
            }
            return StatusCode((int)HttpStatusCode.OK, result);
        }

        [Route("shiftchange")]
        [HttpGet]
        public async Task<IActionResult> ShiftChange()
        {
            await _manager.changeShift();
            return StatusCode((int)HttpStatusCode.OK);
        }

        [Route("fetchreview")]
        [HttpGet]
        public async Task<IActionResult> Review([FromQuery] int week)
        {
            var result = await _manager.fetchReview(week);

            if (result.Count <= 0)
            {
                return NotFound(new { message = $"No review data for week {week} is found" });
            }

            return StatusCode((int)HttpStatusCode.OK, result);
        }

        [Route("fetchoptimize")]
        [HttpGet]
        public async Task<IActionResult> Optimize([FromQuery] int week)
        {
            var result = await _manager.fetchOptimize(week);

            if (result.Count <= 0)
            {
                return NotFound(new { message = $"No optimize data for week {week} is found" });
            }

            return StatusCode((int)HttpStatusCode.OK, result);
        }

        [Route("taskcompletionpredictions")]
        [HttpGet]
        public async Task<IActionResult> TaskCompletionPrediction([FromQuery] int companyId)
        {
            var result = await _manager.ComputePercentagesLandingPage(companyId);
            return StatusCode((int)HttpStatusCode.OK, result);
        }
    }
}
