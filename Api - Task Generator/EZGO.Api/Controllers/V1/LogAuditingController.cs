using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class LogAuditingController : BaseController<LogAuditingController>
    {
        private readonly IDataAuditing _manager;

        #region - constructor(s) -
        public LogAuditingController(IDataAuditing manager, ILogger<LogAuditingController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }
        #endregion

        #region - audit templates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/audittemplate/{audittemplateid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingAuditTemplateLatest([FromRoute] int audittemplateid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!AuditValidators.TemplateIdIsValid(audittemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audittemplateid, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: audittemplateid, Models.Enumerations.TableNames.audits_audittemplate.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/audittemplate/{audittemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingAuditTemplate([FromRoute] int audittemplateid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!AuditValidators.TemplateIdIsValid(audittemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audittemplateid, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: audittemplateid, Models.Enumerations.TableNames.audits_audittemplate.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/audittemplate/{logauditingid}/details")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingAuditTemplateDetails([FromRoute] int logauditingid, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: logauditingid, objectType: ObjectTypeEnum.LogAuditing))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetAuditTemplateDataMutationsDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), logAuditingId: logauditingid, limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - checklist templates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/checklisttemplate/{checklisttemplateid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingChecklistTemplateLatest([FromRoute] int checklisttemplateid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!ChecklistValidators.TemplateIdIsValid(checklisttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklisttemplateid, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: checklisttemplateid, Models.Enumerations.TableNames.checklists_checklisttemplate.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/checklisttemplate/{checklisttemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingChecklistTemplate([FromRoute] int checklisttemplateid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!ChecklistValidators.TemplateIdIsValid(checklisttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklisttemplateid, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: checklisttemplateid, Models.Enumerations.TableNames.checklists_checklisttemplate.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/checklisttemplate/{logauditingid}/details")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingChecklistTemplateDetails([FromRoute] int logauditingid, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: logauditingid, objectType: ObjectTypeEnum.LogAuditing))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetChecklistTemplateDataMutationsDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), logAuditingId: logauditingid, limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - task templates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/tasktemplate/{tasktemplateid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingTaskTemplateLatest([FromRoute] int tasktemplateid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!TaskValidators.TemplateIdIsValid(tasktemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId:tasktemplateid, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: tasktemplateid, Models.Enumerations.TableNames.tasks_tasktemplate.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/tasktemplate/{tasktemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingTaskTemplate([FromRoute] int tasktemplateid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!TaskValidators.TemplateIdIsValid(tasktemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: tasktemplateid, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: tasktemplateid, Models.Enumerations.TableNames.tasks_tasktemplate.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/tasktemplate/{logauditingid}/details")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingTaskTemplateDetails([FromRoute] int logauditingid, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: logauditingid, objectType: ObjectTypeEnum.LogAuditing))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetTaskTemplateDataMutationsDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), logAuditingId: logauditingid, limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - work instructions -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/workinstructiontemplate/{workinstructiontemplateid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingWorkInstructionTemplateLatest([FromRoute] int workinstructiontemplateid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!WorkInstructionValidators.TemplateIdIsValid(workinstructiontemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workinstructiontemplateid, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: workinstructiontemplateid, Models.Enumerations.TableNames.workinstruction_templates.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/workinstructiontemplate/{workinstructiontemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingWorkInstructionTemplate([FromRoute] int workinstructiontemplateid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!WorkInstructionValidators.TemplateIdIsValid(workinstructiontemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workinstructiontemplateid, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: workinstructiontemplateid, Models.Enumerations.TableNames.workinstruction_templates.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/workinstructiontemplate/{logauditingid}/details")]
        [HttpGet]
        public async Task<IActionResult> GetLogWorkInstructionTemplateDetails([FromRoute] int logauditingid, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: logauditingid, objectType: ObjectTypeEnum.LogAuditing))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetWorkInstructionTemplateDataMutationsDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), logAuditingId: logauditingid, limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - assessment templates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/assessmenttemplate/{assessmenttemplateid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingAssessmentTemplateLatest([FromRoute] int assessmenttemplateid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!AssessmentValidators.TemplateIdIsValid(assessmenttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessmenttemplateid, objectType: ObjectTypeEnum.AssessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: assessmenttemplateid, Models.Enumerations.TableNames.assessment_templates.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/assessmenttemplate/{assessmenttemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingAssessmentTemplate([FromRoute] int assessmenttemplateid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!AssessmentValidators.TemplateIdIsValid(assessmenttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessmenttemplateid, objectType: ObjectTypeEnum.AssessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: assessmenttemplateid, Models.Enumerations.TableNames.assessment_templates.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/assessmenttemplate/{logauditingid}/details")]
        [HttpGet]
        public async Task<IActionResult> GetLogAssassmentTemplateDetails([FromRoute] int logauditingid, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: logauditingid, objectType: ObjectTypeEnum.LogAuditing))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetAssessmentTemplateDataMutationsDetails(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), logAuditingId: logauditingid, limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - actions -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/action/{actionid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingActionLatest([FromRoute] int actionid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: actionid, Models.Enumerations.TableNames.actions_action.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/action/{actionid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingAction([FromRoute] int actionid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!ActionValidators.ActionIdIsValid(actionid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: actionid, objectType: ObjectTypeEnum.Action))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: actionid, Models.Enumerations.TableNames.actions_action.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - comments -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/comment/{commentid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingCommentLatest([FromRoute] int commentid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!CommentValidators.CommentIdIsValid(commentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: commentid, objectType: ObjectTypeEnum.Comment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: commentid, Models.Enumerations.TableNames.comments.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/comment/{commentid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingComment([FromRoute] int commentid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!CommentValidators.CommentIdIsValid(commentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: commentid, objectType: ObjectTypeEnum.Comment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: commentid, Models.Enumerations.TableNames.comments.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - users -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/users/{userid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingUserLatest([FromRoute] int userid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!UserValidators.UserIdIsValid(userid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: userid, Models.Enumerations.TableNames.profiles_user.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/users/{userid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingUsers([FromRoute] int userid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!UserValidators.UserIdIsValid(userid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: userid, Models.Enumerations.TableNames.profiles_user.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - matrices -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/matrices/{matrixid}/latest")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingMatricesLatest([FromRoute] int matrixid)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!MatrixValidators.MatrixIdIsValid(matrixid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataLatestMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: matrixid, Models.Enumerations.TableNames.matrices.ToString());

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            } else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
           
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/matrices/{matrixid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingMatrices([FromRoute] int matrixid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!UserValidators.UserIdIsValid(matrixid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: matrixid, objectType: ObjectTypeEnum.Matrix))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutations(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: matrixid, Models.Enumerations.TableNames.matrices.ToString(), limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - auditing per user -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/user/{userid}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingUser([FromRoute] int userid, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!UserValidators.UserIdIsValid(userid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: userid, objectType: ObjectTypeEnum.ProfileUsers))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataUserHistory(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: userid, limit: limit, offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion


        #region - auditing overview -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/overview")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingOverview([FromQuery] string[] objecttypes, [FromQuery] string description, [FromQuery] string createdonstart = null, [FromQuery] string createdonend = null, [FromQuery] int? objectid = null, [FromQuery] int? userid = null, [FromQuery] int? limit = null, [FromQuery] int? offset = null)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if(objecttypes != null && objecttypes.Length == 0)
            {
                objecttypes = null;
            }

            DateTime parsedCreatedOnStart = DateTime.MinValue;
            if (!string.IsNullOrEmpty(createdonstart) && DateTime.TryParseExact(createdonstart, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedCreatedOnStart)) { };

            DateTime parsedCreatedOnEnd = DateTime.MinValue;
            if (!string.IsNullOrEmpty(createdonend) && DateTime.TryParseExact(createdonend, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedCreatedOnEnd)) { };

            var result = await _manager.GetObjectDataMutationsOverview(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        types: objecttypes,
                                                        description: description,
                                                        objectId: objectid,
                                                        userId: userid,
                                                        createdOnStart: !string.IsNullOrEmpty(createdonstart) && parsedCreatedOnStart != DateTime.MinValue ? parsedCreatedOnStart : new Nullable<DateTime>(),
                                                        createdOnEnd: !string.IsNullOrEmpty(createdonend) && parsedCreatedOnEnd != DateTime.MinValue ? parsedCreatedOnEnd : new Nullable<DateTime>(),
                                                        limit: limit,
                                                        offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion


        #region - auditing specific item -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditingId([FromRoute] int id)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: id, objectType: ObjectTypeEnum.LogAuditing))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.GetObjectDataMutation(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), id: id);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - auditing in general -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("logauditing")]
        [HttpGet]
        public async Task<IActionResult> GetLogAuditing([FromQuery] string objecttype, [FromQuery] string description,[FromQuery] string createdonstart = null, [FromQuery] string createdonend = null, [FromQuery] int? objectid = null, [FromQuery] int? userid = null, [FromQuery] int? limit = null, [FromQuery] int? offset = null)
        {
            if (!(this.IsCmsRequest || this.IsPostManRequest))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }


            DateTime parsedCreatedOnStart = DateTime.MinValue;
            if (!string.IsNullOrEmpty(createdonstart) && DateTime.TryParseExact(createdonstart, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedCreatedOnStart)) { };

            DateTime parsedCreatedOnEnd = DateTime.MinValue;
            if (!string.IsNullOrEmpty(createdonend) && DateTime.TryParseExact(createdonend, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedCreatedOnEnd)) { };

            var result = await _manager.GetObjectData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                        type: objecttype,
                                                        description: description,
                                                        objectId: objectid,
                                                        userId: userid,
                                                        createdOnStart: !string.IsNullOrEmpty(createdonstart) && parsedCreatedOnStart != DateTime.MinValue ? parsedCreatedOnStart : new Nullable<DateTime>(),
                                                        createdOnEnd: !string.IsNullOrEmpty(createdonend) && parsedCreatedOnEnd != DateTime.MinValue ? parsedCreatedOnEnd : new Nullable<DateTime>(),
                                                        limit: limit,
                                                        offset: offset);

            if (result == null)
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }
        #endregion

        #region - retrieve by parent data -
        //TODO add retrieval of parent data
        #endregion


    }
}
