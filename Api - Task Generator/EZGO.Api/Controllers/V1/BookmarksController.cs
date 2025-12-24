using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// ChecklistTemplatesController; contains all routes based on checklist templates.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class BookmarksController : BaseController<BookmarksController>
    {
        #region - privates - 
        IBookmarkManager _manager;
        #endregion

        #region - constructor(s) -
        public BookmarksController(ILogger<BookmarksController> logger, IBookmarkManager bookmarkManager, IGeneralManager generalManager, IApplicationUser applicationuser, IConfigurationHelper configurationHelper) : base(logger, generalManager, applicationuser, configurationHelper)
        {
            _manager = bookmarkManager;
        }
        #endregion

        #region - GET routes bookmarks -
        [Route("bookmark/{guid}")]
        [HttpGet]
        public async Task<IActionResult> GetBookmarkAsync([FromRoute] Guid guid)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            if (guid == Guid.Empty)
            { 
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetBookmarkAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), guid);

            //check bookmark object rights
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: result.ObjectId, objectType: result.ObjectType))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            //check bookmark object type and allowed areas only for basic user and shift leader
            if ((this.User.IsInRole("basic") || this.User.IsInRole("shift_leader")) && 
                !await _manager.CheckBookmarkAllowedAreas(result, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync()))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
        }
        #endregion

        #region - POST routes bookmarks - 
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("bookmark/createorretrieve/{bookmarkType}/{objectType}/{objectId}")]
        [HttpPost]
        public async Task<IActionResult> CreateOrRetrieveBookmarkAsync([FromRoute] ObjectTypeEnum objectType, [FromRoute] int objectId, [FromRoute] BookmarkTypeEnum bookmarkType)
        {
            if (!BookmarkValidators.BookmarkObjectIdIsValid(objectId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BookmarkValidators.MESSAGE_OBJECT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.CreateOrRetrieveBookmarkAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                objectType: objectType,
                                                                objectId: objectId,
                                                                bookmarkType: bookmarkType);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("bookmark/setactive/{bookmarkGuid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveChecklistTemplate([FromRoute] Guid bookmarkGuid, [FromBody] object isActive)
        {
            var result = await _manager.SetBookmarkActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  bookmarkGuid: bookmarkGuid,
                                                                  isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks - 

        #endregion
    }
}
