using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Users;
using EZGO.Api.Models.Skills;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
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
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class UserStandingController : BaseController<UserStandingController>
    {

        #region - privates -
        private readonly IUserStandingManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        #endregion

        public UserStandingController(IUserManager userManager, IUserStandingManager manager, IConfigurationHelper configurationHelper, IToolsManager toolsManager, ILogger<UserStandingController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
        }

        #region - User groups -
        [Route("usergroups")] //TODO remove understanding from routes; 
        [Route("userstanding/groups")]
        [HttpGet]
        public async Task<IActionResult> GetUserGroups()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUserGroupsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("userstanding/group/{userGroupId}")]
        [HttpGet]
        public async Task<IActionResult> GetUserGroup(int userGroupId)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userGroupId: userGroupId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("usergroups/add")] //TODO remove understanding from routes; 
        [Route("userstanding/group/add")]
        [HttpPost]
        public async Task<IActionResult> AddUserGroup([FromBody] UserGroup userGroup, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!userGroup.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userGroup.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  userGroup: userGroup);
            if (fulloutput && result > 0)
            {
                var resultfull = await _manager.GetUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userGroupId: result);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("usergroups/change/{userGroupId}")]
        [Route("userstanding/group/change/{userGroupId}")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserGroup([FromBody] UserGroup userGroup, int userGroupId, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!userGroup.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userGroup.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.ChangeUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  userGroupId: userGroupId,
                                                                  userGroup: userGroup);

            if (fulloutput && result > 0)
            {
                var resultfull = await _manager.GetUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userGroupId: result);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("usergroups/setactive/{userGroupId}")]
        [Route("usergroups/delete/{userGroupId}")]
        [HttpPost]

        public async Task<IActionResult> DeleteUserGroup([FromRoute] int userGroupId, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!UserStandingValidators.UserGroupIdIsValid(userGroupId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserStandingValidators.MESSAGE_USER_GROUP_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var userGroup = await _manager.GetUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                             userGroupId: userGroupId);

            if (userGroup != null && !userGroup.InUseInMatrix)
            {
                var result = await _manager.SetUserGroupActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                      userGroupId: userGroupId,
                                                                      isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserStandingValidators.MESSAGE_USER_GROUP_IN_USE_IN_MATRIX.ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userstanding/group/adduser")]
        [HttpPost]
        public async Task<IActionResult> AddUserToUserGroup([FromQuery] int userProfileId, [FromQuery] int userGroupId)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userProfileId, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.AddUserToUserGroup(userProfileId: userProfileId, userGroupId: userGroupId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userstanding/group/{groupid}/removeuser/{userid}")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserFromUserGroup([FromRoute] int userid, [FromRoute] int groupid)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: groupid, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.RemoveUserFromUserGroup(userGroupId: groupid, userProfileId: userid, id: 0);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - User skills -
        [Route("userskills")]
        [Route("userstanding/skills")]
        [HttpGet]
        public async Task<IActionResult> GetUserSkills()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUserSkills(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("userskill/{userSkillId}")]
        [Route("userstanding/skill/{userSkillId}")]
        [HttpGet]
        public async Task<IActionResult> GetUserSkill([FromRoute] int userSkillId)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userSkillId, objectType: ObjectTypeEnum.UserSkill))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetUserSkill(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userSkillId: userSkillId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userstanding/skill/add")]
        [HttpPost]
        public async Task<IActionResult> AddUserSkill([FromBody] UserSkill userSkill, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!userSkill.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userSkill.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddUserSkill(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  userSkill: userSkill);

            if (fulloutput && result > 0)
            {
                var resultfull = await _manager.GetUserSkill(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userSkillId: result);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userskill/change/{userSkillId}")]
        [Route("userstanding/skill/change")] //TODO refactory for correct input e.g. specific id supplied. 
        [HttpPost]
        public async Task<IActionResult> ChangeUserSkill([FromBody] UserSkill userSkill, [FromRoute] int userSkillId, [FromQuery] bool deleteoldvalues = false,[FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!userSkill.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userSkill.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userSkillId, objectType: ObjectTypeEnum.UserSkill))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.ChangeUserSkill(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  userSkillId: userSkillId,
                                                                  userSkill: userSkill,
                                                                  deleteOldUserSkillValues: deleteoldvalues);

            if (fulloutput && result > 0)
            {
                var resultfull = await _manager.GetUserSkill(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userSkillId: result);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userskill/setactive/{userSkillId}")]
        [Route("userskill/delete/{userSkillId}")]
        [HttpPost]

        public async Task<IActionResult> DeleteUserSkill([FromRoute] int userSkillId, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!UserStandingValidators.UserSkillIdIsValid(userSkillId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserStandingValidators.MESSAGE_USER_SKILL_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userSkillId, objectType: ObjectTypeEnum.UserSkill))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var userSkill = await _manager.GetUserSkill(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userSkillId: userSkillId);

            if (userSkill != null && !userSkill.InUseInMatrix)
            {
                var result = await _manager.SetUserSkillActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                      userSkillId: userSkillId,
                                                                      isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserStandingValidators.MESSAGE_USER_SKILL_IN_USE_IN_MATRIX.ToJsonFromObject());
            }
        }

        #endregion

        #region - User skill custom target - 
        [Route("userskillcustomtargets")]
        [HttpGet]
        public async Task<IActionResult> GetApplicabilities([FromQuery] int? userid = null)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUserSkillsCustomTargetApplicabilitiesForUser(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: userid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("userskillcustomtarget/setapplicability")]
        [HttpPost]
        public async Task<IActionResult> SetApplicability([FromBody] UserSkillCustomTargetApplicability userSkillCustomTargetApplicability)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (userSkillCustomTargetApplicability == null || userSkillCustomTargetApplicability.UserId <= 0 || userSkillCustomTargetApplicability.UserSkillId <= 0)
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userSkillCustomTargetApplicability.ToJsonFromObject(), response: "User skill custom target applicability didn't pass validation.");
                return StatusCode((int)HttpStatusCode.BadRequest, "User skill custom target applicability didn't pass validation.");
            }

            var result = await _manager.SetUserSkillCustomTargetApplicability(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userSkillCustomTargetApplicability: userSkillCustomTargetApplicability);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("userskillcustomtarget/remove")]
        [HttpPost]
        public async Task<IActionResult> RemoveCustomTarget([FromBody] UserSkillAndUserMetadata userSkillAndUserMetadata)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (userSkillAndUserMetadata.UserId <= 0 || userSkillAndUserMetadata.UserSkillId <= 0)
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userSkillAndUserMetadata.ToJsonFromObject(), response: "Remove custom target endpoint didn't pass validation.");
                return StatusCode((int)HttpStatusCode.BadRequest, "Remove custom target endpoint didn't pass validation.");
            }

            var result = await _manager.RemoveCustomTarget(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: userSkillAndUserMetadata.UserId, userSkillId: userSkillAndUserMetadata.UserSkillId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - User skill values - 
        [Route("userskillvalues")]
        [HttpGet]
        public async Task<IActionResult> GetUserSkillValues([FromQuery] int limit, [FromQuery] int offset)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (limit == 0)
                limit = ApiSettings.DEFAULT_MAX_NUMBER_OF_USERSKILLVALUES_RETURN_ITEMS;

            var result = await _manager.GetUserSkillValues(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), limit, offset);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("userskillvalues/byid/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetUserSkillValueById([FromRoute] int id)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: id, objectType: ObjectTypeEnum.UserSkillValue))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetUserSkillValue(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), id: id);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("userskillvalues/byuserskill/{userSkillId}/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetUserSkillValueByUserSkill([FromRoute] int userSkillId, [FromRoute] int userId)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            
            var result = await _manager.GetUserSkillValue(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userSkillId: userSkillId, userId: userId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        [Route("userskillvalues/remove")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserSkillValueForUserWithSkill([FromBody] UserSkillAndUserMetadata userSkillAndUserMetadata)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (userSkillAndUserMetadata.UserId <= 0 || userSkillAndUserMetadata.UserId <= 0)
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userSkillAndUserMetadata.ToJsonFromObject(), response: "Remove user skill value for user with skill endpoint didn't pass validation.");
                return StatusCode((int)HttpStatusCode.BadRequest, "Remove user skill value for user with skill endpoint didn't pass validation.");
            }

            var result = await _manager.RemoveUserSkillValueForUserWithSkill(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: userSkillAndUserMetadata.UserId, userSkillId: userSkillAndUserMetadata.UserSkillId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }
        #endregion
    }
}
