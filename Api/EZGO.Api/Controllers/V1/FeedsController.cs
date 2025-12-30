using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Helper;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Feed;
using EZGO.Api.Models.Filters;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.FactoryFeed)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class FeedsController : BaseController<FeedsController>
    {
        private readonly IFeedManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly RateLimiterHelper _rateLimiter;

        #region - constructor(s) -
        public FeedsController(IUserManager userManager, IFeedManager manager,IToolsManager toolsManager, ILogger<FeedsController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper,  RateLimiterHelper rateLimiter) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _rateLimiter = rateLimiter;
        }
        #endregion

        [Route("feeds")]
        [HttpGet]
        public async Task<IActionResult> GetFeed([FromQuery] string timestamp, [FromQuery] string include = null, [FromQuery] FeedTypeEnum? feedtype = null, [FromQuery] int? limit = null, [FromQuery] int? offset = null, [FromQuery] bool usetreeview = true)
        {

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var filters = new FeedFilters() { FeedType = feedtype, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_AUDITTEMPLATES_RETURN_ITEMS, Offset = offset };

            var result = await _manager.GetFeedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), useTreeView: usetreeview, include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("feeds/items/{feedId}")]
        [HttpGet]
        public async Task<IActionResult> GetFeedItems([FromRoute] int feedId, [FromQuery] string include = null, [FromQuery] FeedTypeEnum? feedtype = null, [FromQuery] int? limit = null, [FromQuery] int? offset = null, [FromQuery] bool usetreeview = true)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var filters = new FeedFilters() { FeedType = feedtype, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_AUDITTEMPLATES_RETURN_ITEMS, Offset = offset, FactoryFeedId = feedId };

            var result = await _manager.GetFeedItemsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), useTreeView: usetreeview, include: include, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("feeds/itemcomments/{feedId}/{feedItemId}")]
        [HttpGet]
        public async Task<IActionResult> GetFeedItemComments([FromRoute] int feedId, [FromRoute] int feedItemId, [FromQuery] int? limit = null, [FromQuery] int? offset = null, [FromQuery] bool usetreeview = true)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var filters = new FeedFilters() { Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_AUDITTEMPLATES_RETURN_ITEMS, Offset = offset, FactoryFeedId = feedId, FeedMessageId=feedItemId };

            var result = await _manager.GetFeedItemCommentsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), useTreeView: usetreeview, filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("feeds/item/{itemId}")]
        [HttpGet]
        public async Task<IActionResult> GetFeedItem([FromRoute] int itemId, [FromQuery] string include = null, [FromQuery] bool usetreeview = true)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetFeedItemAsync(
                companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                feedItemId: itemId,
                useTreeView: usetreeview,
                include: include
            );

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            // Expectation: 404 when itemId does not exist
            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("feeds/change/{feedid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeFeed([FromRoute] int feedid, [FromBody] FactoryFeed feed)
        {
            if (!feed.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: feed.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (feedid != feed.Id || !await this.CurrentApplicationUser.CheckObjectRights(objectId: feed.Id, objectType: ObjectTypeEnum.Factoryfeed))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeFeedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), feedId: feedid, feed: feed);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("feeds/add")]
        [HttpPost]
        public async Task<IActionResult> AddFeed([FromBody] FactoryFeed feed)
        {
         
            if (!feed.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: feed.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddFeedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), feed: feed);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("feeds/item/add")]
        [HttpPost]
        public async Task<IActionResult> AddFeedItem([FromBody] FeedMessageItem feeditem)
        {
            var userId = (await this.CurrentApplicationUser.GetAndSetUserIdAsync()).ToString();
            var (isLimited, message) = _rateLimiter.CheckLimit(userId, "feeds:item:add");

            if (isLimited)
            {
                return StatusCode((int)HttpStatusCode.TooManyRequests, message);
            }

            if (!feeditem.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: feeditem.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddFeedItemAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), feedItem: feeditem);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("feeds/item/change/{feeditemid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeFeedItem([FromRoute] int feeditemid, [FromBody] FeedMessageItem feeditem)
        {
            if (!feeditem.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: feeditem.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (feeditemid != feeditem.Id || !await this.CurrentApplicationUser.CheckObjectRights(objectId: feeditem.Id, objectType: ObjectTypeEnum.FactoryfeedMessage))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeFeedItemAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), feedItemId: feeditemid, feedItem: feeditem);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("feeds/item/setactive/{feeditemid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveFeedItem([FromRoute] int feeditemid, [FromBody] object isActive)
        {
            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: feeditemid, objectType: ObjectTypeEnum.FactoryfeedMessage))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetFeedItemActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), feedItemId: feeditemid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }


        [Route("feeds/item/setliked/{feeditemid}")]
        [HttpPost]
        public async Task<IActionResult> SetLikedItem([FromRoute] int feeditemid, [FromBody] object isLiked)
        {
            if (!BooleanValidator.CheckValue(isLiked))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: feeditemid, objectType: ObjectTypeEnum.FactoryfeedMessage))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetFeedItemLikedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), feedItemId: feeditemid, isLiked: BooleanConverter.ConvertObjectToBoolean(isLiked));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("feeds/item/setviewed/{feeditemid}")]
        [HttpPost]
        public async Task<IActionResult> SetViewedItem([FromRoute] int feeditemid)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: feeditemid, objectType: ObjectTypeEnum.FactoryfeedMessage))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetFeedItemViewedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), feedItemId: feeditemid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

    }
}
