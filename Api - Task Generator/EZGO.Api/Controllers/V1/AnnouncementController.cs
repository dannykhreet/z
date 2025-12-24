using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class AnnouncementController : BaseController<AnnouncementController>
    {

        #region - privates -
        private readonly IGeneralManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        #endregion

        #region - contructor(s) -
        public AnnouncementController(IUserManager userManager, IGeneralManager manager, ILogger<AnnouncementController> logger, IToolsManager toolsManager, IConfigurationHelper configurationHelper, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
        }
        #endregion

        #region - Announcements -
        [Route("announcements")]
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements([FromQuery] string announcementtype, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            AnnouncementTypeEnum announcementType;
            if (!Enum.TryParse(announcementtype, true, out announcementType))
            {
                announcementType = AnnouncementTypeEnum.ReleaseNotes;
            }
            var result = await _manager.GetAnnouncements(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), announcementType: announcementType, limit: limit, offset: offset);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("announcement/add")]
        [HttpPost]
        public async Task<IActionResult> AddAnnouncement([FromBody] Announcement announcement)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!announcement.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: announcement.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddAnnouncement(announcement: announcement);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("announcement/change/{id}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAnnouncement([FromRoute] int id, [FromBody] Announcement announcement)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!announcement.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: announcement.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeAnnouncement(announcementId: id, announcement: announcement);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();


            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("announcement/setactive/{announcementid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveAction([FromRoute] int announcementid, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!(announcementid>0))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Not valid".ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var result = await _manager.SetAnnouncementActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), announcementId: announcementid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion
    }
}
