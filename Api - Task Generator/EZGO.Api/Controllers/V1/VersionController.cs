using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Versions;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class VersionController : BaseController<VersionController>
    {
        IVersionManager _manager;

        public VersionController(IVersionManager versionManager, ILogger<VersionController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = versionManager;
        }

        [Route("versions/app")]
        [HttpGet]
        public async Task<IActionResult> GetVersionsAppAsync()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            //_manager.Culture = TranslationLanguage;
            var versionApp = await _manager.GetVersionsAppAsync();

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return Ok(versionApp.ToJsonFromObject());
        }

        [Route("version/app/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetVersionAppAsync(int id)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            //_manager.Culture = TranslationLanguage;
            var versionApp = await _manager.GetVersionAppAsync(id);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (versionApp).ToJsonFromObject());
        }

        [Route("version/app/add")]
        [HttpPost]
        public async Task<IActionResult> AddVersionAppAsync([FromBody] VersionApp versionApp, [FromQuery] bool fulloutput)
        {
            //if (!versionApp.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
            //userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
            //messages: out var possibleMessages,
            //                                  validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            //{
            //    await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: versionApp.ToJsonFromObject(), response: possibleMessages);
            //    return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            //}

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddVersionAppAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), versionApp: versionApp);

            if (fulloutput && result > 0)
            {
                var resultfull = await _manager.GetVersionAppAsync(versionAppId: result, connectionKind: Data.Enumerations.ConnectionKind.Writer);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("version/app/change/{id}")]
        [HttpPost]
        public async Task<IActionResult> ChangeVersionAppAsync([FromRoute] int id, [FromBody] VersionApp versionApp)
        {
            if (id != versionApp.Id)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Given ids do not match"));
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeVersionAppAsync(versionApp: versionApp, userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return Ok(result);
        }

        [Route("version/app/setactive/{versionid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveVersion([FromRoute] int versionid, [FromBody] object isActive)
        {
            if (versionid <= 0) return BadRequest();

            if (!BooleanValidator.CheckValue(isActive))
            {
                return BadRequest(BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            var result = await _manager.SetVersionActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), versionId: versionid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

    }
}
