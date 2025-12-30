using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// OfflineController; For downloading a set of data for use with the application when off-line.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class OfflineController : BaseController<OfflineController>
    {
        #region - private(s) -
        IOfflineManager _manager;
        #endregion

        #region - constructor(s) -
        public OfflineController(IOfflineManager manager, ILogger<OfflineController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }
        #endregion
        [Route("offline/actions")]
        [HttpGet]
        public async Task<IActionResult> GetActions()
        {
            var result = "My offline data";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("offline/audits")]
        [HttpGet]
        public async Task<IActionResult> GetAudits()
        {
            var result = "My offline data";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("offline/checklists")]
        [HttpGet]
        public async Task<IActionResult> GetChecklists()
        {
            var result = "Something Something";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("offline/media")]
        [HttpGet]
        public async Task<IActionResult> GetMedia(string fromtimestamp, int areaid)
        {
            //TODO make smart filter based on date and change/create dates.
            var result = await _manager.GetMediaUriAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("offline/tasks")]
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var result = "My offline data";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("offline/company")]
        [HttpGet]
        public async Task<IActionResult> GetCompany()
        {
            var result = "My offline data";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

    }
}