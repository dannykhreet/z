using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tags)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class TagsController : BaseController<TagsController>
    {
        #region - privates -
        private readonly ITagManager _manager;
        private readonly IUserManager _userManager;
        private readonly IGeneralManager _generalManager;
        private readonly IToolsManager _toolsManager;
        #endregion

        #region - contructor(s) -
        public TagsController(ITagManager manager, IUserManager userManager, IToolsManager toolsManager, IConfigurationHelper configurationHelper, IGeneralManager generalManager, ILogger<TagsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _userManager = userManager;
            _generalManager = generalManager;
            _toolsManager = toolsManager;
        }
        #endregion

        #region - GET routes tags -
        /// <summary>
        /// Gets all active tags for the company
        /// </summary>
        /// <returns>A list of tags in the body</returns>
        [Route("tags")]
        [HttpGet]
        public async Task<IActionResult> GetTags(int? areaId = null, string type = null, string include = null)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var companyId = await CurrentApplicationUser.GetAndSetCompanyIdAsync();

            Features features = await _generalManager.GetFeatures(companyId, 0);

            List<string> availableTypes = new()
            {
                "checklisttemplate",
                "assessmenttemplate",
                "audittemplate",
                "workinstructiontemplate",
                "action",
                "comment"
            };

            if(type == null && areaId.HasValue)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "When providing an areaid, please also provide a valid type.");
            }
            else if(!string.IsNullOrEmpty(type) && !availableTypes.Contains(type))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"Please provide a valid type, provided: {type}, available: {string.Join(',', availableTypes)}");
            }

            var filters = new TagsFilters()
            {
                AreaId = areaId,
                Type = type
            };

            _manager.Culture = TranslationLanguage;
            var result = await _manager.GetTagsAsync(companyId: companyId, filters: filters, features: features, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            Agent.Tracer.CurrentTransaction.StartSpan("result.serialization", ApiConstants.ActionExec);

            var returnresult = (result).ToJsonFromObject();

            Agent.Tracer.CurrentSpan.End();
            return Ok(returnresult);
        }

        /// <summary>
        /// Get the names of the tags with id in the list of ids
        /// </summary>
        /// <param name="ids">tag ids to get the names for</param>
        /// <returns>dictionary of tag ids with tag names</returns>
        [Route("tags/names")]
        [HttpGet]
        public async Task<IActionResult> GetTagNames([FromQuery] List<int> ids)
        {
            if (ids == null || ids.Count == 0) { return BadRequest(); }

            _manager.Culture = TranslationLanguage;
            var result = await _manager.GetTagNamesAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), tagIds: ids);
            return Ok(result);
        }

        /// <summary>
        /// Get the active tag groups with list of active tags per group.
        /// Optionally include data on usage per tag. This will include the type of oject where it is used and if it is in use in a template.
        /// </summary>
        /// <param name="include">'usage' will include information about usage of tag.</param>
        /// <returns>Json with all active tag groups including the tags</returns>
        [Route("taggroups")]
        [HttpGet]
        public async Task<IActionResult> GetTagGroups([FromQuery] string include)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var companyId = await CurrentApplicationUser.GetAndSetCompanyIdAsync();
            Features features = _generalManager.GetFeatures(companyId, 0).Result;

            _manager.Culture = TranslationLanguage;
            var result = await _manager.GetTagGroupsAsync(companyId: companyId, include: include, features: features);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            Agent.Tracer.CurrentTransaction.StartSpan("result.serialization", ApiConstants.ActionExec);

            var returnresult = (result).ToJsonFromObject();

            Agent.Tracer.CurrentSpan.End();
            return Ok(returnresult);
        }

        /// <summary>
        /// Gets all available tag groups, regardless of selected status. 
        /// Selected tag groups will have Taggroup.IsSelected = true.
        /// Optionally include information about the usage of tag groups and tags.
        /// </summary>
        /// <param name="include">'usage' will include information about usage of tag.</param>
        /// <returns>A list of all available TagGroup objects</returns>
        [Route("taggroups/all")]
        [HttpGet]
        public async Task<IActionResult> GetTagGroupsOverview([FromQuery] string include)
        {
            if (!IsCmsRequest)
            {
                return NotFound();
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var companyId = await CurrentApplicationUser.GetAndSetCompanyIdAsync();
            Features features = _generalManager.GetFeatures(companyId, 0).Result;

            _manager.Culture = TranslationLanguage;
            var result = await _manager.GetTagGroupsAsync(companyId: companyId, returnAllGroups: true, include: include, features: features);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            Agent.Tracer.CurrentTransaction.StartSpan("result.serialization", ApiConstants.ActionExec);

            var returnresult = (result).ToJsonFromObject();

            Agent.Tracer.CurrentSpan.End();
            return Ok(returnresult);
        }

        /// <summary>
        /// Gets a Tag by id
        /// </summary>
        /// <param name="tagId">Tag id</param>
        /// <returns>Tag object</returns>
        [Route("tag/{tagId}")]
        [HttpGet]
        public async Task<IActionResult> GetTag([FromRoute] int tagId)
        {

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: tagId, objectType: ObjectTypeEnum.Tag))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            _manager.Culture = TranslationLanguage;
            var result = await _manager.GetTagAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), tagId: tagId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            Agent.Tracer.CurrentTransaction.StartSpan("result.serialization", ApiConstants.ActionExec);

            var returnresult = (result).ToJsonFromObject();

            Agent.Tracer.CurrentSpan.End();
            return Ok(returnresult);
        }
        #endregion

        #region - POST routes tags -
        /// <summary>
        /// Adds a new tag
        /// </summary>
        /// <param name="tag">Tag object to be added</param>
        /// <returns>Id of the added Tag</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tags/add")]
        [HttpPost]
        public async Task<IActionResult> AddTag([FromBody] Tag tag)
        {
            if (!IsCmsRequest)
            {
                return NotFound();
            }

            if (!tag.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                            userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                            messages: out var possibleMessages,
                                              validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: tag.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var companyId = await CurrentApplicationUser.GetAndSetCompanyIdAsync();
            var userId = await CurrentApplicationUser.GetAndSetUserIdAsync();

            if (tag.IsHoldingTag == true)
            {
                //check if user has rights to do tagging related actions
                var userProfile = await _userManager.GetUserProfileAsync(companyId, userId);
                if (!userProfile.IsTagManager.HasValue || !userProfile.IsTagManager.Value)
                {
                    return Unauthorized("User is not authorized to manage items on holding level");
                }
            }

            if (await CompanyTagLimitReached(companyId, tag.IsHoldingTag == true))
            {
                return Conflict("Tag limit reached");
            }

            var result = await _manager.AddTagAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userid: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  tag: tag);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return Ok((result).ToJsonFromObject());
        }

        /// <summary>
        /// Update an existing Tag
        /// </summary>
        /// <param name="tagId">Id of the Tag to be updated</param>
        /// <param name="tag">The updated Tag object</param>
        /// <returns>Boolean, true if Tag was updated</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tags/change/{tagId}")]
        [HttpPost]
        public async Task<IActionResult> ChangeTag([FromRoute] int tagId, [FromBody] Tag tag)
        {
            if (!IsCmsRequest)
            {
                return NotFound();
            }

            if (tagId != tag.Id)
            {
                return BadRequest();
            }

            var companyId = await CurrentApplicationUser.GetAndSetCompanyIdAsync();
            var userId = await CurrentApplicationUser.GetAndSetUserIdAsync();

            if (!tag.ValidateAndClean(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                            userId: await CurrentApplicationUser.GetAndSetUserIdAsync(),
                            messages: out var possibleMessages,
                                              validUserIds: ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: tag.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            //NOTE: check_object_rights_tag also checks this, this check here may be redundant
            if (tag.IsHoldingTag == true)
            {
                //check if user has rights to do holding related actions
                var userProfile = await _userManager.GetUserProfileAsync(companyId, userId);
                if (!userProfile.IsTagManager.HasValue || !userProfile.IsTagManager.Value)
                {
                    return Unauthorized("User is not authorized to manage tags on holding level");
                }
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: tagId, objectType: ObjectTypeEnum.Tag))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.ChangeTagAsync(companyId: companyId,
                                                        userId: userId,
                                                        tagId: tagId,
                                                        tag: tag);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return Ok((result).ToJsonFromObject());
        }

        /// <summary>
        /// Sets a tag active or inactive (soft delete)
        /// </summary>
        /// <param name="tagId">Id of the tag to soft delete or restore</param>
        /// <param name="isActive">False for delete, true for restore</param>
        /// <returns>Boolean to indicate if opperation was successful</returns>
        [Route("tag/setactive/{tagId}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveTag([FromRoute] int tagId, [FromBody] object isActive)
        {
            if (!IsCmsRequest)
            {
                return NotFound();
            }

            //if (!TagValidators.TagIdIsValid(tagId))
            //{
            //    return StatusCode((int)HttpStatusCode.BadRequest, TagValidators.MESSAGE_TAG_ID_IS_NOT_VALID.ToJsonFromObject());
            //}

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await CurrentApplicationUser.CheckObjectRights(objectId: tagId, objectType: ObjectTypeEnum.Tag))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            //int companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            //if (isActive && await CompanyTagLimitReached(companyId, tag.IsHoldingTag == true))
            //{
            //    return BadRequest("Tag limit reached");
            //}

            var result = await _manager.SetTagActiveAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await CurrentApplicationUser.GetAndSetUserIdAsync(), tagId: tagId, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }

        /// <summary>
        /// Used to set the active tag groups for a company.
        /// This method looks at Taggroup.IsSelected to determine if it should be selected or not.
        /// </summary>
        /// <param name="taggroups">List of TagGroups with IsSelected set. if IsSelected is null, it will be treated as false.</param>
        /// <returns>Number of selected TagGroups</returns>
        [Route("taggroups/change")]
        [HttpPost]
        public async Task<IActionResult> SetTagGroups(List<TagGroup> taggroups)
        {
            if (!IsCmsRequest)
            {
                return NotFound();
            }

            foreach (TagGroup tagGroup in taggroups)
            {
                if (tagGroup.Id <= 0)
                {
                    return BadRequest();
                }
            }

            int companyId = await CurrentApplicationUser.GetAndSetCompanyIdAsync();
            int numberOfSelectedGroups = taggroups.Where(g => g.IsSelected == true).Count();
            if (await TagGroupLimitExceeded(companyId, numberOfSelectedGroups))
            {
                return BadRequest("Tag group limit exceeded.");
            }

            //todo: check rights and validate tag groups
            var userProfile = await _userManager.GetUserProfileAsync(await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync());
            if (!userProfile.IsTagManager.HasValue || !userProfile.IsTagManager.Value)
            {
                return Unauthorized("User is not authorized to manage items on holding level");
            }

            var result = await _manager.SetTagGroupsAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), tagGroups: taggroups);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return GetObjectResultJsonWithStatus(result);
        }
        #endregion

        #region - Private methods - 
        /// <summary>
        /// Checks if company has reached the set tag limit.
        /// If the tag
        /// </summary>
        /// <param name="companyId">Company to do the check for</param>
        /// <param name="countHoldingTags">If true, holding tags will be counted. If false, company tags will be counted</param>
        /// <returns>true if limit is reached, false if limit is not yet reached</returns>
        [NonAction]
        private async Task<bool> CompanyTagLimitReached(int companyId, bool countHoldingTags)
        {
            //TODO: move to manager and make in line with GetApplicationSettings in generalmanager
            //get tag limit for company
            int tagLimit = 25;
            int tagCount = countHoldingTags ? await _manager.GetTagsCountHolding(companyId) : await _manager.GetTagsCountCompany(companyId);

            //company specific limit
            var companyTagLimit = await _generalManager.GetSettingValueForCompanyByResourceId(companyId, 74);
            if (!string.IsNullOrEmpty(companyTagLimit))
            {
                if (int.TryParse(companyTagLimit, out tagLimit))
                {
                    return tagCount >= tagLimit;
                }
            }
            else
            {
                //general limit
                var generalTagLimit = await _generalManager.GetSettingResourceByKey("GENERAL_TAG_LIMIT");
                if (generalTagLimit != null && !string.IsNullOrEmpty(generalTagLimit.Value))
                {
                    if (int.TryParse(generalTagLimit.Value, out tagLimit))
                    {
                        return tagCount >= tagLimit;
                    }
                }
            }

            return tagCount >= tagLimit;
        }

        /// <summary>
        /// The company has exceeded the maximum number of tag groups allowed for this company
        /// </summary>
        /// <param name="companyId">company id</param>
        /// <param name="newNumberOfSelectedGroups">new number of selected tag groups</param>
        /// <returns>True if limit is exceeded</returns>
        private async Task<bool> TagGroupLimitExceeded(int companyId, int newNumberOfSelectedGroups)
        {
            int tagGroupLimit = 8; //default to 8 if eveything fails for any reason
            var companyTagGroupLimit = await _generalManager.GetSettingValueForCompanyByResourceId(companyId, 76);

            if (!string.IsNullOrEmpty(companyTagGroupLimit))
            {
                if (int.TryParse(companyTagGroupLimit, out tagGroupLimit))
                {
                    return newNumberOfSelectedGroups > tagGroupLimit;
                }
            }
            else
            {
                var generalTagGroupLimit = await _generalManager.GetSettingResourceByKey("GENERAL_TAGGROUP_LIMIT");
                if (generalTagGroupLimit != null && !string.IsNullOrEmpty(generalTagGroupLimit.Value))
                {
                    if (int.TryParse(generalTagGroupLimit.Value, out tagGroupLimit))
                    {
                        return newNumberOfSelectedGroups > tagGroupLimit;
                    }
                }
            }

            return newNumberOfSelectedGroups > tagGroupLimit;
        }
        #endregion
    }
}
