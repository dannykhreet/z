using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Users;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Security;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// UsersController; contains all routes based on users.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class UsersController : BaseController<UsersController>
    {
        #region - privates -
        private readonly IUserManager _manager;
        private readonly IToolsManager _toolsManager;
        #endregion

        #region - contructor(s) -
        public UsersController(IUserManager manager, IConfigurationHelper configurationHelper, IToolsManager toolsManager, ILogger<UsersController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _toolsManager = toolsManager;
        }
        #endregion

        #region - GET routes userprofiles -
        [Route("userprofiles")]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] bool? isstaff, [FromQuery] bool? issuperuser, [FromQuery] RoleTypeEnum? role, [FromQuery] string include)
        {
            var filters = new UserFilters() { IsStaff = isstaff, IsSuperUser = issuperuser, RoleType = role};

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUserProfilesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        //Gets all usperprofiles of a company no matter the userId.
        [Route("userprofilescompany")]
        [HttpGet]
        public async Task<IActionResult> GetUsersCompany([FromQuery] bool? isstaff, [FromQuery] bool? issuperuser, [FromQuery] RoleTypeEnum? role, [FromQuery] string include)
        {
            var filters = new UserFilters() { IsStaff = isstaff, IsSuperUser = issuperuser, RoleType = role };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUsersBasicAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("userprofile/{userid}")]
        [HttpGet]
        public async Task<IActionResult> GetUserProfile([FromRoute]int userid, [FromQuery] string include)
        {
            if (!UserValidators.UserIdIsValid(userid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserValidators.MESSAGE_USER_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (this.IsCmsRequest)
            {
                var currentLoggedInUser = await _manager.GetUserProfileAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());
                if(currentLoggedInUser.Role == RoleTypeEnum.Manager.ToDatabaseString() || currentLoggedInUser.Role == RoleTypeEnum.ShiftLeader.ToDatabaseString())
                {
                    UserProfile result;

                    Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                    result = await _manager.GetUserProfileAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: userid, include: include);

                    AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                    Agent.Tracer.CurrentSpan.End();

                    return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
                } else
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }
            } else
            {
                if(await this.CurrentApplicationUser.GetAndSetUserIdAsync() != userid)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }

                UserProfile result;

                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                result = await _manager.GetUserProfileAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: userid, include: include);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.UserExtendedDetails)]
        [Route("userprofile/extendeddetails")]
        [HttpGet]
        public async Task<IActionResult> GetUserProfileExtendedDetails()
        {
           
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            //get user profile extended data, if no id supplied current connected profile data will be retrieved.
            var result = await _manager.GetExtendedUserProfileDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userProfileId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.OK, (new UserExtendedDetails() { UserId = await this.CurrentApplicationUser.GetAndSetUserIdAsync() }).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.UserExtendedDetails)]
        [Route("userprofile/{userprofileid}/extendeddetails")]
        [HttpGet]
        public async Task<IActionResult> GetUserProfileExtendedDetails([FromRoute] int userprofileid)
        {

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userprofileid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            //get user profile extended data, if no id supplied current connected profile data will be retrieved.
            var result = await _manager.GetExtendedUserProfileDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userProfileId: userprofileid , userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.OK, (new UserExtendedDetails() { UserId = await this.CurrentApplicationUser.GetAndSetUserIdAsync() }).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.UserExtendedDetails)]
        [Route("userprofile/apppreferences")]
        [HttpGet]
        public async Task<IActionResult> GetUserProfileAppPreferences()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUserProfileAppPreferences(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userProfileId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.UserExtendedDetails)]
        [Route("userprofile/{userprofileid}/apppreferences")]
        [HttpGet]
        public async Task<IActionResult> GetUserProfileAppPreferences([FromRoute] int userprofileid)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userprofileid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUserProfileAppPreferences(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userProfileId: userprofileid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - POST routes userprofiles -
        [Route("userprofile")]
        [HttpPost]
        public async Task<IActionResult> GetUserProfile([FromBody]string usertoken, [FromQuery] string include)
        {
            if (!UserValidators.UserTokenIsValid(usertoken))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserValidators.MESSAGE_USER_TOKEN_IS_NOT_VALID.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetUserProfileByTokenAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userToken: HttpContext.User.GetClaim(ClaimTypes.Sid), tokenIsEncrypted: true, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Invalid user or user can not be retrieved.").ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.UserExtendedDetails)]
        [Route("userprofile/change/extendeddetails")]
        [HttpPost]
        public async Task<IActionResult> AddChangeUserProfileExtendedDetails([FromBody] UserExtendedDetails details)
        {
            if(details == null || details.UserId <= 0)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserValidators.MESSAGE_USER_TOKEN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (details.UserId > 0)
            {
                if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: details.UserId, objectType: ObjectTypeEnum.ProfileUsers))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }
            }

            if (!details.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: details.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddOrChangeExtendedUserProfileDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userProfileId: details.UserId, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), details: details);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (!result)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Details not saved.").ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Roles)]
        [Route("userprofile/change/roles")]
        [HttpPost]
        public async Task<IActionResult> AddChangeUserProfileRoles([FromBody] UserProfile userprofile)
        {
            if (userprofile == null || userprofile.Id <= 0 || userprofile.Roles == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserValidators.MESSAGE_USER_TOKEN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (userprofile.Id > 0)
            {
                if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userprofile.Id, objectType: ObjectTypeEnum.ProfileUsers))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            //create new user settings (values are part of user profile properties
            //when user settings are extended this will need to change.
            //this later also will be integrated in normal saving of users.
            var userSettings = new UserSettings() { Roles = userprofile.Roles, UserId = userprofile.Id };
            var result = await _manager.AddOrChangeUserSettings(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userProfileId: userprofile.Id, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userSettings: userSettings);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (!result)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Details not saved.").ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.UserExtendedDetails)]
        [Route("userprofile/change/apppreferences")]
        [HttpPost]
        public async Task<IActionResult> AddChangeUserAppPreferences([FromBody] UserAppPreferencesWithMetadata userAppPreferences)
        {
            if (userAppPreferences == null || userAppPreferences.UserId == 0 || userAppPreferences.UserAppPreferences == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserValidators.MESSAGE_USER_TOKEN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (userAppPreferences.UserId > 0)
            {
                if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userAppPreferences.UserId, objectType: ObjectTypeEnum.ProfileUsers))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddOrChangeUserAppPreferences(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userProfileId: userAppPreferences.UserId, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userAppPreferences: userAppPreferences.UserAppPreferences);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (!result)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("App preferences not saved.").ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Route("userprofile/change/password")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserProfilePassword([FromBody] ChangePassword changepassword)
        {

            if(!PasswordValidators.PasswordIsValid(changepassword.CurrentPassword))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, PasswordValidators.MESSAGE_PASSWORD_IS_NOT_VALID.ToJsonFromObject());
            }

            if(!PasswordValidators.PasswordIsValid(changepassword.NewPassword))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, PasswordValidators.MESSAGE_NEW_PASSWORD_IS_NOT_VALID.ToJsonFromObject());
            }

            if(!PasswordValidators.PasswordNewPasswordIsValid(changepassword.CurrentPassword, changepassword.NewPassword))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, PasswordValidators.MESSAGE_PASSWORD_AND_NEW_PASSWORD_SAME.ToJsonFromObject());
            }

            if(!PasswordValidators.NewPasswordNewPasswordConfirmationIsValid(changepassword.NewPassword, changepassword.NewPasswordValidation))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, PasswordValidators.MESSAGE_NEW_PASSWORD_AND_NEW_PASSWORD_VALIDATION_NOT_SAME.ToJsonFromObject());
            }

            // ONLY USE WITH NON TOKEN ONLY CALLS.
            //if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: changepassword.UserId, objectType: ObjectTypeEnum.ProfileUsers))
            //{
            //    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            //}

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeUserPasswordAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                userProfileId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                currentUserPassword: changepassword.CurrentPassword,
                                                                userPassword: changepassword.NewPassword,
                                                                userPasswordConfirmation: changepassword.NewPasswordValidation);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userprofile/change/{userprofileid}/password")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserProfilePassword([FromRoute]int userprofileid, [FromBody] ChangePassword changepassword)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!PasswordValidators.PasswordIsValid(changepassword.NewPassword))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, PasswordValidators.MESSAGE_NEW_PASSWORD_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!PasswordValidators.PasswordNewPasswordIsValid(changepassword.CurrentPassword, changepassword.NewPassword))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, PasswordValidators.MESSAGE_PASSWORD_AND_NEW_PASSWORD_SAME.ToJsonFromObject());
            }

            if (!PasswordValidators.NewPasswordNewPasswordConfirmationIsValid(changepassword.NewPassword, changepassword.NewPasswordValidation))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, PasswordValidators.MESSAGE_NEW_PASSWORD_AND_NEW_PASSWORD_VALIDATION_NOT_SAME.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userprofileid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (changepassword.CompanyId > 0) {
                if (changepassword.CompanyId != await this.CurrentApplicationUser.GetAndSetCompanyIdAsync())
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }
            } else
            {
                changepassword.CompanyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(); //force company connected user.
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeUserPasswordAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                userProfileId: userprofileid,
                                                                userPassword: changepassword.NewPassword,
                                                                userPasswordConfirmation: changepassword.NewPasswordValidation);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userprofile/add")]
        [HttpPost]
        public async Task<IActionResult> AddUserProfile([FromBody] UserProfile userprofile)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!userprofile.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _manager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userprofile.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var userNameAllreadyFound = await _manager.CheckUserName(userName: userprofile.UserName);
            if(userNameAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Username already exists.".ToJsonFromObject());
            }

            var emailAllreadyFound = await _manager.CheckEmail(email: userprofile.Email);
            if (emailAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Email address already exists.".ToJsonFromObject());
            }

            var upnAllreadyFound = await _manager.CheckUPN(upn: userprofile.UPN);
            if (upnAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, "UPN already exists.".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddUserProfileAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userProfile: userprofile);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (result > 0)
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Issue while saving user profile".ToJsonFromObject());
            }


        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userprofile/setactive/{id}")]
        [HttpPost] //TODO rename
        public async Task<IActionResult> SetActiveAuditTemplate([FromRoute] int id, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!UserValidators.UserIdIsValid(id))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: id, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetUserProfileActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userProfileId: id,
                                                                  isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("userprofile/change")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserProfile([FromBody] UserProfile userprofile)
        {

            if (userprofile.Id > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: userprofile.Id, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (userprofile.Id > 0 && userprofile.Id != await this.CurrentApplicationUser.GetAndSetUserIdAsync())
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!userprofile.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _manager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userprofile.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeUserProfileAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userProfile: userprofile);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (result)
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Issue while saving user profile".ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userprofile/change/{userid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserProfile([FromRoute] int userid, [FromBody] UserProfile userprofile)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!UserValidators.UserIdIsValid(userid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserValidators.MESSAGE_USER_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!userprofile.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _manager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: userprofile.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var userNameAllreadyFound = await _manager.CheckUserName(userName: userprofile.UserName, userId: userid, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            if (userNameAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, ("Username already exists.").ToJsonFromObject());
            }

            var emailAllreadyFound = await _manager.CheckEmail(email: userprofile.Email, userId: userid, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            if (emailAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, ("Email address already exists.").ToJsonFromObject());
            }

            var upnAllreadyFound = await _manager.CheckUPN(upn: userprofile.UPN, userId: userid, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            if (upnAllreadyFound)
            {
                return StatusCode((int)HttpStatusCode.Conflict, ("UPN already exists.").ToJsonFromObject());
            }

            if(userprofile.Id != userid)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "User can not be updated.".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeUserProfileIncludingAreasAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), userProfileId: userid, userProfile: userprofile);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (result)
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Conflict, "Issue while saving user profile".ToJsonFromObject());
            }
        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("userprofile/setsuccessor/{userid}")]
        [HttpPost]
        public async Task<IActionResult> SetUserSuccessor([FromRoute] int userid, [FromBody] int successorid)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!UserValidators.UserIdIsValid(userid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, UserValidators.MESSAGE_USER_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: successorid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetSuccessor(companyid: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userid: userid, successorid: successorid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - User CMS checks -
        [Route("userprofile/{userid}/check/{propertyname}")]
        [Route("userprofile/check/{propertyname}")]
        [HttpPost]
        public async Task<IActionResult> CheckIfPropertyValueValid([FromBody] string value, [FromRoute] string propertyname, [FromRoute] int userid)
        {
            if (propertyname == "username")
            {
                var userNameAllreadyFound = await _manager.CheckUserName(userName: value, userId: userid, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
                if (userNameAllreadyFound)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, ("Username already exists.").ToJsonFromObject());
                }
            }

            if (propertyname == "email")
            {
                var emailAllreadyFound = await _manager.CheckEmail(email: value, userId: userid, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
                if (emailAllreadyFound)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, ("Email address already exists.").ToJsonFromObject());
                }
            }

            if(propertyname == "upn")
            {
                var upnAllreadyFound = await _manager.CheckUPN(upn: value, userId: userid, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
                if (upnAllreadyFound)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, ("UPN already exists.").ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.OK, ("").ToJsonFromObject());
        }

        #endregion

        #region - health checks -
        /// <summary>
        /// GetUsersHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("users/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetUsersHealth()
        {
            try
            {
                var result = await _manager.GetUserProfilesAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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