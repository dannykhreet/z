using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Users;
using EZGO.Api.Security.Helpers;
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
    /// <summary>
    /// MatrixController; contains all routes based on assessments.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class MatrixController : BaseController<MatrixController>
    {
        #region - privates -
        private readonly IAreaManager _areaManager;
        private readonly IGeneralManager _generalManager;
        private readonly IAssessmentManager _assessmentManager;
        private readonly IUserManager _userManager;
        private readonly IMatrixManager _matrixManager;
        private readonly IUserStandingManager _userStandingManager;
        private readonly IToolsManager _toolsManager;
        #endregion

        #region - contructor(s) -
        public MatrixController(IConfigurationHelper configurationHelper, IUserManager userManager, IToolsManager toolsManager, IUserStandingManager userStandingManager, IMatrixManager matrixManager, IAssessmentManager assessmentManager, IGeneralManager generalManager, IAreaManager areaManager, ILogger<MatrixController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _areaManager = areaManager;
            _generalManager = generalManager;
            _assessmentManager = assessmentManager;
            _userManager = userManager;
            _matrixManager = matrixManager;
            _userStandingManager = userStandingManager;
            _toolsManager = toolsManager;

        }
        #endregion

        #region - skills matrices -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrices")]
        [HttpGet]
        public async Task<IActionResult> GetSkillsMatrices([FromQuery] int? createdById, [FromQuery] int? modifiedById, [FromQuery] int? areaId, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new MatrixFilters()
            {
                CreatedById = createdById,
                ModifiedById = modifiedById,
                AreaId = areaId,
                Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_SKILLSMATRICES_RETURN_ITEMS,
                Offset = offset
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _matrixManager.GetMatricesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}")]
        [HttpGet]
        public async Task<IActionResult> GetSkillMatrix([FromRoute] int skillsmatrixid, [FromQuery] string include, [FromQuery] bool debug)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrixId: skillsmatrixid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/statistics/{skillsmatrixid}")]
        [HttpGet]
        public async Task<IActionResult> GetSkillMatrixStatistics([FromRoute] int skillsmatrixid)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixOperationalBehaviour(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), matrixId: skillsmatrixid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/totals/{skillsmatrixid}")]
        [HttpGet]
        public async Task<IActionResult> GetSkillMatrixTotals([FromRoute] int skillsmatrixid)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixTotals(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), matrixId: skillsmatrixid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/add")]
        [HttpPost]
        public async Task<IActionResult> AddSkillMatrix([FromBody] SkillsMatrix matrix)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!matrix.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: matrix.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var possibleId = await _matrixManager.AddMatrixAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrix: matrix);

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: possibleId, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrixId: possibleId, connectionKind: Data.Enumerations.ConnectionKind.Writer);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/change/{skillsmatrixid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeSkillMatrix([FromRoute] int skillsmatrixid, [FromBody] SkillsMatrix matrix)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            //validate id not negative and not 0
            if (!MatrixValidators.MatrixIdIsValid(skillsmatrixid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, MatrixValidators.MESSAGE_MATRIXID_IS_NOT_VALID.ToJsonFromObject());
            }


            if (matrix.Id != skillsmatrixid)
                matrix.Id = skillsmatrixid;

            //check object rights
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!matrix.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages, ignoreCreatedByCheck: true,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: matrix.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            //agent tracer startspan 
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            
            //changematrixasync
            var ok = await _matrixManager.ChangeMatrixAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), skillsmatrixid, matrix: matrix);
            if(!ok)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, MatrixValidators.MESSAGE_MATRIX_IS_NOT_VALID.ToJsonFromObject());
            }

            var output = await _matrixManager.GetMatrixAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrixId: skillsmatrixid, connectionKind: Data.Enumerations.ConnectionKind.Writer);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            //agent tracer endspan
            Agent.Tracer.CurrentSpan.End();
            //return statuscode
            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/setactive/{skillsmatrixid}")]
        [Route("skillsmatrix/delete/{skillsmatrixid}")]
        [HttpPost]
        public async Task<IActionResult> DeleteSkillMatrix([FromRoute] int skillsmatrixid, [FromBody] object isactive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!MatrixValidators.MatrixIdIsValid(skillsmatrixid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, MatrixValidators.MESSAGE_MATRIXID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isactive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _matrixManager.SetMatrixActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  matrixId: skillsmatrixid,
                                                                  isActive: BooleanConverter.ConvertObjectToBoolean(isactive));

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - matrix groups -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups/addrelation")]
        [HttpPost]
        public async Task<IActionResult> AddUserGroups([FromRoute] int skillsmatrixid, [FromBody] MatrixRelationUserGroup matrixusergroup, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixusergroup.UserGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (skillsmatrixid > 0 && matrixusergroup.UserGroupId > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var output = await _matrixManager.AddMatrixUserGroupRelationAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                                      matrixId: skillsmatrixid,
                                                                                      matrixRelationUserGroup: matrixusergroup);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                if (output == -1)
                {
                    return BadRequest(MatrixValidators.MESSAGE_USER_GROUP_ALREADY_IN_MATRIX);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Group or Matrix not valid.").ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups/changerelation")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserGroups([FromRoute] int skillsmatrixid, [FromBody] MatrixRelationUserGroup matrixusergroup, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixusergroup.UserGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (skillsmatrixid > 0 && matrixusergroup.UserGroupId > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var output = await _matrixManager.ChangeMatrixUserGroupRelationAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                      matrixId: skillsmatrixid,
                                                      matrixRelationUserGroupId: matrixusergroup.Id,
                                                      matrixRelationUserGroup: matrixusergroup);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Group or Matrix not valid.").ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups/removerelation")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserGroupsRelation([FromRoute] int skillsmatrixid, [FromBody] MatrixRelationUserGroup matrixusergrouprelation, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixusergrouprelation.UserGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (matrixusergrouprelation.UserGroupId > 0 && skillsmatrixid > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var result = await _matrixManager.RemoveMatrixUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                           matrixId: skillsmatrixid,
                                                           matrixRelationUserGroup: matrixusergrouprelation);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, "".ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, ("Group not correctly removed, or relation did not exist.").ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.NoContent, ("").ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups/users/add")]
        [HttpPost]
        public async Task<IActionResult> AddUserToGroup([FromRoute] int skillsmatrixid, [FromBody] UserGroupRelationUser usergroupuserrelation, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: usergroupuserrelation.GroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: usergroupuserrelation.UserId, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (usergroupuserrelation.GroupId > 0 && usergroupuserrelation.UserId > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var result = await _userStandingManager.AddUserToUserGroup(userGroupId: usergroupuserrelation.GroupId, userProfileId: usergroupuserrelation.UserId);

                AppendCapturedExceptionToApm(_userStandingManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, ("Success.").ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, ("Unable to save user.").ToJsonFromObject());
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Unable to determine relation.").ToJsonFromObject());
            }


        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups/users/remove")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserFrom([FromRoute] int skillsmatrixid, [FromBody] UserGroupRelationUser usergroupuserrelation, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: usergroupuserrelation.GroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: usergroupuserrelation.UserId, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (usergroupuserrelation.GroupId > 0 && usergroupuserrelation.UserId > 0)
            {
                //TODO add check object rights
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var result = await _userStandingManager.RemoveUserFromUserGroup(id: usergroupuserrelation.Id, userGroupId: usergroupuserrelation.GroupId, userProfileId: usergroupuserrelation.UserId);

                AppendCapturedExceptionToApm(_userStandingManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, ("Success.").ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Conflict, ("Unable to remove user.").ToJsonFromObject());
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Unable to determine relation.").ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups/add")]
        [HttpPost]
        public async Task<IActionResult> AddMatrixUserGroups([FromRoute] int skillsmatrixid, [FromBody] SkillsMatrixUserGroup matrixusergroup, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!matrixusergroup.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                messages: out var possibleMessages,
                                            validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: matrixusergroup.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _matrixManager.AddMatrixUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                        matrixId: skillsmatrixid,
                                                        matrixUserGroup: matrixusergroup);

            if (result > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var returnValue = await _matrixManager.GetMatrixUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrixId: skillsmatrixid, matrixUserGroupId: result, connectionKind: Data.Enumerations.ConnectionKind.Writer);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, returnValue.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Conflict, ("Group not correctly saved.").ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups/change")]
        [HttpPost]
        public async Task<IActionResult> ChangeMatrixUserGroup([FromRoute] int skillsmatrixid, [FromBody] SkillsMatrixUserGroup matrixusergroup, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixusergroup.UserGroupId, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!matrixusergroup.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                messages: out var possibleMessages,
                                            validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: matrixusergroup.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (matrixusergroup.UserGroupId > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var result = await _matrixManager.ChangeMatrixUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                           matrixId: skillsmatrixid,
                                                           matrixUserGroup: matrixusergroup);

                if (result > 0)
                {
                    var returnValue = await _matrixManager.GetMatrixUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrixId: skillsmatrixid, matrixUserGroupId: result, connectionKind: Data.Enumerations.ConnectionKind.Writer);

                    AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                    Agent.Tracer.CurrentSpan.End();

                    return StatusCode((int)HttpStatusCode.OK, returnValue.ToJsonFromObject());
                }
                else
                {
                    AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                    Agent.Tracer.CurrentSpan.End();

                    return StatusCode((int)HttpStatusCode.Conflict, ("Group not correctly saved.").ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.NoContent, ("").ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/group/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetUserGroups([FromRoute] int skillsmatrixid, [FromRoute] int matrixgroupid, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixgroupid, objectType: ObjectTypeEnum.UserGroup))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    matrixId: skillsmatrixid,
                                                                    matrixUserGroupId: matrixgroupid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/groups")]
        [HttpGet]
        public async Task<IActionResult> GetUserGroups([FromRoute] int skillsmatrixid, [FromQuery] string include)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixUserGroupsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    matrixId: skillsmatrixid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }
        #endregion

        #region - users -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/users")]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromRoute] int skillsmatrixid, [FromQuery] string include)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixUsersAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    matrixId: skillsmatrixid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/users")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] string include)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixUsersAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    matrixId: 0);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/uservalues")]
        [HttpGet]
        public async Task<IActionResult> GetUserValues([FromRoute] int skillsmatrixid, [FromQuery] string include)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixUserSkillValuesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                        matrixId: skillsmatrixid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        #endregion 

        #region - matrix skills -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/skills/addrelation")]
        [HttpPost]
        public async Task<IActionResult> AddUserSkillRelation([FromRoute] int skillsmatrixid, [FromBody] MatrixRelationUserSkill matrixuserskill, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixuserskill.UserSkillId, objectType: ObjectTypeEnum.UserSkill))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (skillsmatrixid > 0 && matrixuserskill.UserSkillId > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var output = await _matrixManager.AddMatrixUserSkillRelationAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                           matrixId: skillsmatrixid,
                                                           matrixRelationUserSkill: matrixuserskill);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                if (output == -1)
                {
                    return BadRequest(MatrixValidators.MESSAGE_USER_SKILL_ALREADY_IN_MATRIX);
                }
                else
                {

                    return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Skill or Matrix not valid.").ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/skills/removerelation")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserSkillRelation([FromRoute] int skillsmatrixid, [FromBody] MatrixRelationUserSkill matrixuserskill, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixuserskill.UserSkillId, objectType: ObjectTypeEnum.UserSkill))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (skillsmatrixid > 0 && matrixuserskill.UserSkillId > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var output = await _matrixManager.RemoveMatrixUserSkillRelationAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                          userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                          matrixId: skillsmatrixid,
                                                          matrixRelationUserSkillId: matrixuserskill.Id,
                                                          matrixRelationUserSkill: matrixuserskill);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Skill or Matrix not valid.").ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/skills/changerelation")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserSkillRelation([FromRoute] int skillsmatrixid, [FromBody] MatrixRelationUserSkill matrixuserskill, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixuserskill.UserSkillId, objectType: ObjectTypeEnum.UserSkill))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (skillsmatrixid > 0 && matrixuserskill.UserSkillId > 0)
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                var output = await _matrixManager.ChangeMatrixUserSkillRelationAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                          userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                          matrixId: skillsmatrixid,
                                                          matrixRelationUserSkillId: matrixuserskill.Id,
                                                          matrixRelationUserSkill: matrixuserskill);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("Skill or Matrix not valid.").ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/skills/add")]
        [HttpPost]
        public async Task<IActionResult> AddUserSkill([FromRoute] int skillsmatrixid, [FromBody] SkillsMatrixItem matrixuserskill, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!matrixuserskill.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                messages: out var possibleMessages,
                                            validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: matrixuserskill.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _matrixManager.AddMatrixUserSkillAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                      matrixId: skillsmatrixid,
                                                      matrixUserSkill: matrixuserskill);

            if (result > 0)
            {
                var returnValue = await _matrixManager.GetMatrixUserGroupAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrixId: skillsmatrixid, matrixUserGroupId: result, connectionKind: Data.Enumerations.ConnectionKind.Writer);

                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, returnValue.ToJsonFromObject());
            }
            else
            {
                AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.Conflict, ("Group not correctly saved.").ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/skills/change")]
        [HttpPost]
        public async Task<IActionResult> ChangeUserSkill([FromRoute] int skillsmatrixid, [FromBody] SkillsMatrixItem matrixuserskill, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!matrixuserskill.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                messages: out var possibleMessages,
                                            validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: matrixuserskill.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var output = "";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/skills")]
        [HttpGet]
        public async Task<IActionResult> GetSkills([FromRoute] int skillsmatrixid, [FromQuery] string include)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetMatrixUserSkillsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                           matrixId: skillsmatrixid);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/{skillsmatrixid}/skills/value/save")]
        [HttpPost]
        public async Task<IActionResult> SaveSkillsValues([FromRoute] int skillsmatrixid, [FromBody] SkillsMatrixItemValue skillValue, [FromQuery] string include)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillsmatrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: skillValue.UserSkillId, objectType: ObjectTypeEnum.UserSkill))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!skillValue.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
            userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
            messages: out var possibleMessages,
                                        validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: skillValue.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }


            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.SaveMatrixUserSkillValue(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), matrixId: skillsmatrixid, matrixItemValue: skillValue);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        #endregion

        #region - legend configuration -
        /// <summary>
        /// Gets the legend configuration for the current company.
        /// </summary>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/legend/{companyId}")]
        [HttpGet]
        public async Task<IActionResult> GetLegendConfiguration([FromRoute] int companyId)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != companyId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.GetLegendConfigurationAsync(companyId: companyId);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (output).ToJsonFromObject());
        }

        /// <summary>
        /// Saves the complete legend configuration for the current company.
        /// </summary>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("skillsmatrix/legend/{companyId}")]
        [HttpPost]
        public async Task<IActionResult> SaveLegendConfiguration([FromRoute] int companyId, [FromBody] SkillMatrixLegendConfiguration configuration)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != companyId)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _matrixManager.SaveLegendConfigurationAsync(
                companyId: companyId,
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                configuration: configuration);

            AppendCapturedExceptionToApm(_matrixManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            if (output)
            {
                return StatusCode((int)HttpStatusCode.OK, new { success = true }.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { success = false, message = "Failed to save legend configuration" }.ToJsonFromObject());
            }
        }
        #endregion

    }
}
