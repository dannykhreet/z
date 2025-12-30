using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Utils.Validators;
using System.Net;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Converters;
using System.Globalization;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Security.Helpers;
using Microsoft.AspNetCore.Authorization;
using Amazon.Runtime.Internal.Util;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace EZGO.Api.Controllers.V1
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Comments)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class CommentsController : BaseController<CommentsController>
    {
        private readonly IMemoryCache _cache;
        private readonly ICommentManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly IGeneralManager _generalManager;

        #region - constructor(s) -
        public CommentsController(IUserManager userManager, ICommentManager manager, IGeneralManager generalManager, IToolsManager toolsManager, IMemoryCache memoryCache, ILogger<CommentsController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _cache = memoryCache;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _generalManager = generalManager;
        }
        #endregion

        /// <summary>
        /// GetComments -> retrieve comments based on several parameters.
        /// </summary>
        /// <param name="filtertext">filtertext: the action id, action text and comment text can be searched (not the action comments)</param>
        /// <param name="userid">userid: the user that created the comment</param>
        /// <param name="taskid">taskid: the id of the task linked to this comment</param>
        /// <param name="tasktemplateid">tasktemplateid: the id of the tasktemplate linked to this comment</param>
        /// <param name="tagids">tagids: checks if any of the tags supplied in this parameter are added to an action (format: 1,2,3)</param>
        /// <param name="limit">limit: limit the maximum amount of comments that this call returns</param>
        /// <param name="offset">offset: skip the first x results and return the next limit amount of comments</param>
        /// <param name="include">include: the only include value that is supported for comments is 'tags'</param>
        /// <param name="createdfrom">createdfrom: checks (only if createdto also filled in) if the comment was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="createdto">createdto: checks (only if createdfrom also filled in) if the comment was created at or after the createdfrom, but at or before the createdto (format: dd-MM-yyyy)</param>
        /// <param name="timestamp">timestamp: checks if the comment date is before the timestamp (format: dd-MM-yyyy HH:mm:ss)</param>
        /// <returns></returns>
        [Route("comments")]
        [HttpGet]
        public async Task<IActionResult> GetComments([FromQuery] string filtertext, [FromQuery] int? userid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, [FromQuery] string tagids, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string include, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string timestamp = null)
        {
            _manager.Culture = TranslationLanguage;

            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            DateTime createdFromTimeStamp;
            if (DateTime.TryParseExact(createdfrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdFromTimeStamp)) { };
            DateTime createdToTimeStamp;
            if (DateTime.TryParseExact(createdto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdToTimeStamp)) { };

            var filters = new CommentFilters() 
            {
                FilterText = filtertext,

                CreatedFrom = !string.IsNullOrEmpty(createdfrom) && createdFromTimeStamp != DateTime.MinValue ? createdFromTimeStamp : new Nullable<DateTime>(),
                CreatedTo = !string.IsNullOrEmpty(createdto) && createdToTimeStamp != DateTime.MinValue ? createdToTimeStamp : new Nullable<DateTime>(),

                UserId = userid,
                TaskId = taskid,
                TaskTemplateId = tasktemplateid,
                Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_COMMENT_RETURN_ITEMS,
                Timestamp = parsedTimeStamp,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            if (!CommentFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            var uniqueKey = string.Format("GET_COMMENTS_T{0}_C{1}_U{2}_L{3}_O{4}", parsedTimeStamp.ToString("dd-MM-yyyy_HH:mm"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("comments", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_LIMITS"))
                    {
                        if (filters.Limit.HasValue && filters.Limit.Value == 0) { filters.Limit = ApiSettings.DEFAULT_MAX_NUMBER_OF_COMMENT_RETURN_ITEMS; }
                    }

                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMMENTS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                }
                else {
                    enableTrafficShaping = false;
                }
                
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCommentsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return GetObjectResultJsonWithStatus(result);

        }

        /// <summary>
        /// GetCommentsRelations; Retrieved comment relations, will return a list of commentid, taskid, tasktemplateid and if available checklist/auditid
        /// Can be used with same filters as commetns call if needed to get specific sets. 
        /// </summary>
        /// <param name="filtertext"></param>
        /// <param name="userid"></param>
        /// <param name="taskid"></param>
        /// <param name="tasktemplateid"></param>
        /// <param name="tagids"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="include"></param>
        /// <param name="createdfrom"></param>
        /// <param name="createdto"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        [Route("comments_relations")]
        [Route("comments/relations")]
        [HttpGet]
        public async Task<IActionResult> GetCommentsRelations([FromQuery] string filtertext, [FromQuery] int? userid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, [FromQuery] string tagids, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string include, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string timestamp = null)
        {
            _manager.Culture = TranslationLanguage;

            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            DateTime createdFromTimeStamp;
            if (DateTime.TryParseExact(createdfrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdFromTimeStamp)) { };
            DateTime createdToTimeStamp;
            if (DateTime.TryParseExact(createdto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdToTimeStamp)) { };

            var filters = new CommentFilters()
            {
                FilterText = filtertext,

                CreatedFrom = !string.IsNullOrEmpty(createdfrom) && createdFromTimeStamp != DateTime.MinValue ? createdFromTimeStamp : new Nullable<DateTime>(),
                CreatedTo = !string.IsNullOrEmpty(createdto) && createdToTimeStamp != DateTime.MinValue ? createdToTimeStamp : new Nullable<DateTime>(),

                UserId = userid,
                TaskId = taskid,
                TaskTemplateId = tasktemplateid,
                Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_COMMENT_RETURN_ITEMS,
                Timestamp = parsedTimeStamp,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            if (!CommentFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCommentsRelationsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        /// <summary>
        /// GetCommentsCounts; Retrieved statistics object for comments.
        /// Can be used with same filters as commetns call if needed to get specific sets. 
        /// </summary>
        /// <param name="filtertext"></param>
        /// <param name="userid"></param>
        /// <param name="taskid"></param>
        /// <param name="tasktemplateid"></param>
        /// <param name="tagids"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="include"></param>
        /// <param name="createdfrom"></param>
        /// <param name="createdto"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        [Route("comments_counts")]
        [Route("comments/counts")]
        [HttpGet]
        public async Task<IActionResult> GetCommentsCounts([FromQuery] string filtertext, [FromQuery] int? userid, [FromQuery] int? taskid, [FromQuery] int? tasktemplateid, [FromQuery] string tagids, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string include, [FromQuery] string createdfrom, [FromQuery] string createdto, [FromQuery] string timestamp = null)
        {
            _manager.Culture = TranslationLanguage;

            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            DateTime createdFromTimeStamp;
            if (DateTime.TryParseExact(createdfrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdFromTimeStamp)) { };
            DateTime createdToTimeStamp;
            if (DateTime.TryParseExact(createdto, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out createdToTimeStamp)) { };

            var filters = new CommentFilters()
            {
                FilterText = filtertext,

                CreatedFrom = !string.IsNullOrEmpty(createdfrom) && createdFromTimeStamp != DateTime.MinValue ? createdFromTimeStamp : new Nullable<DateTime>(),
                CreatedTo = !string.IsNullOrEmpty(createdto) && createdToTimeStamp != DateTime.MinValue ? createdToTimeStamp : new Nullable<DateTime>(),

                UserId = userid,
                TaskId = taskid,
                TaskTemplateId = tasktemplateid,
                Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_COMMENT_RETURN_ITEMS,
                Timestamp = parsedTimeStamp,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            if (!CommentFiltersValidators.ValidateAndClean(filters, out var messages))
            {
                return BadRequest(messages);
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCommentCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("comment/{commentid}")]
        [HttpGet]
        public async Task<IActionResult> GetComment([FromRoute] int commentid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), commentId: commentid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("comment/add")]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] Comment comment)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (comment.TaskId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: comment.TaskId.Value, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (comment.TaskTemplateId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: comment.TaskTemplateId.Value, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }


            if (!comment.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: comment.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.AddCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), comment: comment);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("comment/change/{commentid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeComment([FromRoute] int commentid, [FromBody] Comment comment)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (comment.TaskId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: comment.TaskId.Value, objectType: ObjectTypeEnum.Task))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (comment.TaskTemplateId.HasValue && !await this.CurrentApplicationUser.CheckObjectRights(objectId: comment.TaskTemplateId.Value, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (commentid != comment.Id || !await this.CurrentApplicationUser.CheckObjectRights(objectId: comment.Id, objectType: ObjectTypeEnum.Comment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!comment.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                     userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                     ignoreCreatedByCheck: true,
                     messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: comment.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Comment currentComment = await _manager.GetCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), commentId: commentid);
            if (!comment.ValidateMutation(currentComment, out string messages))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: comment.ToJsonFromObject(), response: messages);
                return StatusCode((int)HttpStatusCode.BadRequest, messages.ToJsonFromObject());
            }

            var result = await _manager.ChangeCommentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), commentId: commentid, comment: comment);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("comment/setactive/{commentid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveComment([FromRoute] int commentid, [FromBody] object isActive)
        {

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: commentid, objectType: ObjectTypeEnum.Comment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetCommentActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), commentId: commentid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        /// <summary>
        /// GetCommentsHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("comments/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetCommentsHealth()
        {
            try
            {
                var result = await _manager.GetCommentsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY),
                                                                       userId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_USER_ID_CONFIG_KEY),
                                                                       filters: new CommentFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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


    }
}
