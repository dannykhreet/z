using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.WorkInstructions;
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// ShiftsController; contains all routes based on shifts.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.WorkInstructions)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class WorkInstructionTemplatesController : BaseController<WorkInstructionTemplatesController>
    {
        #region - privates -
        private readonly IWorkInstructionManager _manager;
        private readonly IAssessmentManager _assessmentManager;
        private readonly IAreaManager _areaManager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly ISharedTemplateManager _sharedTemplateManager;
        private readonly ICompanyManager _companyManager;
        private readonly IGeneralManager _generalManager;
        private readonly IFlattenWorkInstructionManager _flattenedWorkInstructionManager;
        private readonly IFlattenAssessmentManager _flattenedAssessmentManager;
        #endregion

        #region - constructor(s) -
        public WorkInstructionTemplatesController(ICompanyManager companyManager, IAssessmentManager assessmentManager, IGeneralManager generalManager, IFlattenWorkInstructionManager flattenWorkInstructionManager, IFlattenAssessmentManager flattenAssessmentManager, ISharedTemplateManager sharedTemplateManager, IUserManager userManager, IWorkInstructionManager manager, IConfigurationHelper configurationHelper, IAreaManager areaManager, IToolsManager toolsManager, ILogger<WorkInstructionTemplatesController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _areaManager = areaManager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _sharedTemplateManager = sharedTemplateManager;
            _companyManager = companyManager;
            _generalManager = generalManager;
            _flattenedWorkInstructionManager = flattenWorkInstructionManager;
            _assessmentManager = assessmentManager;
            _flattenedAssessmentManager = flattenAssessmentManager;
        }
        #endregion

        #region - GET routes workinstructiontemplates - 
        [Route("workinstructiontemplates")]
        [HttpGet]
        public async Task<IActionResult> GetWorkInstructionTemplates([FromQuery] InstructionTypeEnum? instructiontype, [FromQuery] RoleTypeEnum? role, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null, bool? includeavailableforallareas = null, [FromQuery] string filtertext = null)
        {
            _manager.Culture = TranslationLanguage;

            var filters = new WorkInstructionFilters()
            {
                AreaId = areaid,
                RoleType = role,
                InstructionType = instructiontype,
                FilterAreaType = filterareatype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_WORKINSTRUCTIONTEMPLATES_RETURN_ITEMS,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                IncludeAvailableForAllAreas = includeavailableforallareas,
                FilterText = filtertext
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetWorkInstructionTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("workinstructiontemplates/availableforarea/{areaid}")]
        [HttpGet]
        public async Task<IActionResult> GetAvailableWorkInstructionTemplates([FromQuery] InstructionTypeEnum? instructiontype, [FromQuery] RoleTypeEnum? role, [FromRoute] int areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            //normal GetWorkInstructionTemplates but with mandatory area id and set IncludeAvailableForAllAreas to true
            return await GetWorkInstructionTemplates(instructiontype: instructiontype, role: role, areaid: areaid, filterareatype: filterareatype, tagids: tagids, include: include, limit: limit, offset: offset, allowedonly: allowedonly, includeavailableforallareas: true);
        }

        [Route("workinstructiontemplates/names")]
        [HttpGet]
        public async Task<IActionResult> GetWorkinstructionNames([FromQuery] List<int> ids)
        {
            if (ids == null || ids.Count == 0) { return BadRequest(); }

            _manager.Culture = TranslationLanguage;
            var result = await _manager.GetWorkInstructionsTemplateNames(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), workInstructionIds: ids);
            return Ok(result);
        }

        [Route("workinstructiontemplate/{workInstructionTemplateId}")]
        [HttpGet]
        public async Task<IActionResult> GetWorkInstructionTemplate([FromRoute] int workInstructionTemplateId, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!WorkInstructionValidators.WorkInstructionIdIsValid(workInstructionTemplateId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workInstructionTemplateId, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), workInstructionTemplateId: workInstructionTemplateId, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - GET routes workinstructiontemplate change notifications
        [Route("workinstructiontemplatechangenotifications")]
        [HttpGet]
        public async Task<IActionResult> GetWorkInstructionTemplateChangeNotifications([FromQuery] int? workInstructionTemplateId, [FromQuery] string? starttimestamp, [FromQuery] string? endtimestamp, [FromQuery] int? areaid, [FromQuery] int? userid, [FromQuery] bool? confirmed, [FromQuery]int? offset, [FromQuery] int? limit, [FromQuery] string include)
        {
            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            //construct filters
            var filters = new WorkInstructionTemplateChangeNotificationFilters()
            {
                WorkInstructionTemplateId = workInstructionTemplateId,
                StartTimestamp = parsedstarttimestamp,
                EndTimeStamp = parsedendtimestamp,
                AreaId = areaid,
                UserId = userid,
                Confirmed = confirmed,
                Limit = limit,
                Offset = offset
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            //get notifications
            var result = await _manager.GetWorkInstructionTemplateChangesNotificationsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            //return notifications
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("workinstructiontemplatechangenotification/{changeNotificationId}")]
        [HttpGet]
        public async Task<IActionResult> GetWorkInstructionTemplateChangeNotificationById([FromRoute] int changeNotificationId, [FromQuery] string include)
        {
            if (!WorkInstructionValidators.WorkInstructionChangeNotificationIdIsValid(changeNotificationId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_WORKINSTRUCTION_CHANGE_NOTIFICATION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            //get notification
            var result = await _manager.GetWorkInstructionTemplateChangeNotificationAsync(id: changeNotificationId, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            //return notification
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion
        
        #region - POST Routes workinstruction template change notification confirm
        [Route("workinstructiontemplatechangenotifications/{workInstructionTemplateId}/confirm")]
        [HttpPost]
        public async Task<IActionResult> ConfirmChangesNotificationsAsRead([FromRoute] int workInstructionTemplateId)
        {
            //check if workInstructionTemplateId is greater than 0
            if(!WorkInstructionValidators.TemplateIdIsValid(workInstructionTemplateId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            //check if user has rights to WI template with id workInstructionTemplateId
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workInstructionTemplateId, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            //confirm change notifications as read and add entries in work_instruction_notification_confirmed 
            var result = await _manager.ConfirmWorkInstructionTemplateChangesNotifications(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), 
                                                                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), 
                                                                                        workInstructionTemplateId: workInstructionTemplateId);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            return Ok(result);
        }
        #endregion

        #region - POST routes workinstructiontemplates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("workinstructiontemplate/add")]
        [HttpPost]
        public async Task<IActionResult> AddWorkInstruction([FromBody] WorkInstructionTemplate workInstructionTemplate, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!workInstructionTemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: workInstructionTemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  workInstructionTemplate: workInstructionTemplate);

            if (workInstructionTemplate.SharedTemplateId.HasValue && workInstructionTemplate.SharedTemplateId.Value > 0)
                await _sharedTemplateManager.AcceptSharedTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), sharedTemplateId: workInstructionTemplate.SharedTemplateId.Value);

            WorkInstructionTemplate retrievedWITemplate = null;
            //todo flatten
            if (result > 0 && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                retrievedWITemplate = await _manager.GetWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), workInstructionTemplateId: result, include: "items,tags,areapaths,parents", connectionKind: Data.Enumerations.ConnectionKind.Writer);
            }

            //flatten data
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedWorkInstructionManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), retrievedWITemplate);
            }

            if (fulloutput && result > 0 && retrievedWITemplate != null)
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (retrievedWITemplate).ToJsonFromObject());

            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("workinstructiontemplate/change/{workInstructionTemplateId}")]
        [HttpPost]
        public async Task<IActionResult> ChangeWorkInstruction([FromBody] WorkInstructionTemplate workInstructionTemplate, [FromRoute] int workInstructionTemplateId, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!WorkInstructionValidators.WorkInstructionIdIsValid(workInstructionTemplateId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (workInstructionTemplate.Id != workInstructionTemplateId)
            {
                return BadRequest();
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workInstructionTemplateId, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!workInstructionTemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
               userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
               messages: out var possibleMessages, ignoreCreatedByCheck: true,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: workInstructionTemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    workInstructionTemplateId: workInstructionTemplateId,
                                                                    workInstructionTemplate: workInstructionTemplate);

            WorkInstructionTemplate retrievedWITemplate = null;
            //todo flatten
            if (result > 0 && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                retrievedWITemplate = await _manager.GetWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), workInstructionTemplateId: result, include: "items,tags,areapaths,parents", connectionKind: ConnectionKind.Writer);
            }

            //flatten data
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedWorkInstructionManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), retrievedWITemplate);
            }

            //Create SP to retrieve connected assessment template id's after the WI was changed
            if (result > 0 && await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                var assessmentTemplateIds = await _assessmentManager.GetWorkInstructionConnectedAssessmentTemplateIds(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), retrievedWITemplate.Id);
                if(assessmentTemplateIds != null && assessmentTemplateIds.Count > 0)
                {
                    foreach(var assessmentTemplateId in assessmentTemplateIds)
                    {
                        //Then retrieve the assessment templates,
                        var assessmentTemplate = await _assessmentManager.GetAssessmentTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), assessmentTemplateId: assessmentTemplateId, include: Settings.ApiSettings.FULL_INCLUDE_LIST_ASSESSMENTS);

                        //Flatten the template
                        //Store the new version with the template
                        _ = await _flattenedAssessmentManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), assessmentTemplate);
                    }
                }
            }

            if (fulloutput && result > 0 && retrievedWITemplate != null)
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (retrievedWITemplate).ToJsonFromObject());

            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("workinstructiontemplate/extended/change/{workInstructionTemplateId}")]
        [HttpPost]
        public async Task<IActionResult> ChangeWorkInstructionWithNotificationData([FromBody] WorkInstructionTemplateWithNotificationData workInstructionTemplateWithNotificationData, [FromRoute] int workInstructionTemplateId, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!WorkInstructionValidators.WorkInstructionIdIsValid(workInstructionTemplateId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (workInstructionTemplateWithNotificationData == null || workInstructionTemplateWithNotificationData.WorkInstructionTemplate == null)
            {
                return BadRequest();
            }

            if(workInstructionTemplateWithNotificationData != null && 
                workInstructionTemplateWithNotificationData.WorkInstructionTemplate != null && 
                workInstructionTemplateWithNotificationData.WorkInstructionTemplate.Id != workInstructionTemplateId)
            {
                return BadRequest();
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workInstructionTemplateId, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!string.IsNullOrEmpty(workInstructionTemplateWithNotificationData.NotificationComment))
            {
                workInstructionTemplateWithNotificationData.NotificationComment = TextValidator.StripRogueDataFromText(workInstructionTemplateWithNotificationData.NotificationComment);
            }

            if (!workInstructionTemplateWithNotificationData.WorkInstructionTemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
               userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
               messages: out var possibleMessages, ignoreCreatedByCheck: true,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: workInstructionTemplateWithNotificationData.WorkInstructionTemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            //get WI before changes
            var originalWI = await _manager.GetWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    workInstructionTemplateId: workInstructionTemplateId, 
                                                                    include: "items,tags", 
                                                                    connectionKind: Data.Enumerations.ConnectionKind.Writer);

            var result = await _manager.ChangeWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    workInstructionTemplateId: workInstructionTemplateId,
                                                                    workInstructionTemplate: workInstructionTemplateWithNotificationData.WorkInstructionTemplate);

            //get WI after changes and also use it for fulloutput
            var modifiedWI = await _manager.GetWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    workInstructionTemplateId: workInstructionTemplateId, 
                                                                    include: "items,tags", 
                                                                    connectionKind: Data.Enumerations.ConnectionKind.Writer);

            //flatten data
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedWorkInstructionManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), modifiedWI);
            }

            //generate change notification
            var changeNotifResult = await _manager.AddWorkInstructionTemplateChangesNotification(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), newTemplate: modifiedWI, notificationComment: workInstructionTemplateWithNotificationData.NotificationComment, oldTemplate: originalWI);


            //Create SP to retrieve connected assessment template id's after the WI was changed
            if (result > 0 && await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                var assessmentTemplateIds = await _assessmentManager.GetWorkInstructionConnectedAssessmentTemplateIds(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), modifiedWI.Id);
                if (assessmentTemplateIds != null && assessmentTemplateIds.Count > 0)
                {
                    foreach (var assessmentTemplateId in assessmentTemplateIds)
                    {
                        //Then retrieve the assessment templates,
                        var assessmentTemplate = await _assessmentManager.GetAssessmentTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), assessmentTemplateId: assessmentTemplateId, include: "instructions,instructionitems,areapaths,tags");

                        //Flatten the template
                        //Store the new version with the template
                        _ = await _flattenedAssessmentManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), assessmentTemplate);
                    }
                }
            }

            if (fulloutput && result > 0 && modifiedWI != null)
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (modifiedWI).ToJsonFromObject());
            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("workinstructiontemplate/delete/{workinstructiontemplateid}")]
        [HttpPost]
        public async Task<IActionResult> DeleteWorkInstruction([FromRoute] int workInstructionTemplateId, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!WorkInstructionValidators.WorkInstructionIdIsValid(workInstructionTemplateId))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workInstructionTemplateId, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.SetWorkInstructionTemplateActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  workInstructionTemplateId: workInstructionTemplateId,
                                                                  isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Shares a work instruction template with other companies.
        /// The work instruction template will be shared in it's current state.
        /// The other companies will get the shared template in an inbox, to be accepted or declined.
        /// </summary>
        /// <param name="workinstructiontemplateid">id of the work instruction template to be shared</param>
        /// <param name="companyids">list of company ids to share the template to</param>
        /// <returns>true if successful</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("workinstructiontemplate/share/{workinstructiontemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ShareWorkInstruction([FromRoute] int workinstructiontemplateid, [FromBody] List<int> companyids)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }
            if (!WorkInstructionValidators.TemplateIdIsValid(workinstructiontemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, WorkInstructionValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: workinstructiontemplateid, objectType: ObjectTypeEnum.WorkInstructionTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var selectedCompanyIds = companyids;

            int companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();

            List<int> companyIdsInHolding = new();
            int holdingId = await _companyManager.GetCompanyHoldingIdAsync(companyId);
            if (holdingId > 0)
            {
                companyIdsInHolding = await _companyManager.GetCompanyIdsInHolding(holdingId);
            }

            foreach (int selectedCompanyId in selectedCompanyIds)
            {
                if (!companyIdsInHolding.Contains(selectedCompanyId))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "Request contains invalid or disallowed company id(s)");
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            List<int> resultsIds = new();
            WorkInstructionTemplate workinstructionTemplate = await _manager.GetWorkInstructionTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), 
                                                                                                             userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), 
                                                                                                             workInstructionTemplateId: workinstructiontemplateid, 
                                                                                                             include: "items,tags");

            foreach (int selectedCompanyId in selectedCompanyIds)
                resultsIds.Add(await _sharedTemplateManager.ShareWorkInstructionTemplateAsync(fromCompanyId: companyId, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), workInstructionTemplate: workinstructionTemplate, toCompanyId: selectedCompanyId));

            var result = resultsIds.Count > 0;

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region helper methods
        private async Task<List<WorkInstructionTemplate>> GetMockWorkInstructions(bool generateValues = false)
        {
            string[] images = new[] {"tasks/30852/799be628-c6cb-4f53-b815-60e18547a4c5.png",
                                     "tasks/30916/a5b2c203-daba-45c7-afb4-e3723fbb0370.jpg",
                                     "tasks/30930/cf9a78c9-0e32-4d0c-8a17-35e427a29bac.jpg",
                                     "tasks/30997/443a5969-e125-4a43-90b6-1a817a637f82.jpg",
                                     "tasks/30921/96c00fb4-bd43-47bb-b6dd-9b4967021fae.png",
                                     "tasks/31132/3c22df27-b055-4d95-b0c2-f4251bfb8e87.jpg",
                                     "tasks/31002/4dc6591f-b14d-49f7-9116-d7236698d1da.png",
                                     "tasks/30926/40ed00e8-5c14-4d77-a3a5-dfec79b71b68.png",
                                     "tasks/31005/2201a974-0733-4d37-929e-b489ceed401f.jpg",
                                     "tasks/30924/5c8b131d-afcc-45ed-a579-ee58c81ae460.jpg",
                                     "tasks/30940/d4ac4b1f-ebf1-4db4-9159-4d09eb7c0310.jpg",
                                     "tasks/30995/7de561ad-9f71-42a1-8b88-4ce10df245ff.png"};

            string[] images_w = new[] {  "lists/1703/00739107-c5ef-41b0-8d65-664f7655c288.jpg",
                                         "lists/1701/7d794da8-e58d-46e6-84c0-ff7ff885a96b.jpg",
                                         "lists/1690/1ad06a7f-d0b9-45ee-a949-6e59c0672014.jpg",
                                         "lists/1669/f6d7fdbe-b916-4f15-a6dc-b42835c0ce50.jpg",
                                         "lists/1623/728c16d5-303e-4813-9531-4b5cefd563a2.jpg"
                                         };

            string[] workInstructionNames = new[] { "Etikettenrol Wissel", "Inspectie & schoonmaak", "PCS - Dag Controle Lijst", "PCS - Shift Control Lijst", "Procesconfirmatie: A) Ochtenddienst", "Procesconfirmatie: B) Middagdienst", "Procesconfirmatie: C) Nachtdienst", "PCS - Dag Controle Lijst", "Werkplek opleiding -Etikettenrol Wissel", "Werkplek opleiding - Inspectie & schoonmaak", "LOTOTO Procedure" };

            int[] scores = new[]
{
                1,3,5,1,5,1,5,3,4,5,1,2,3,4,5,2,3,2,2,1,5,2,3,4,5,2,1,2,4,5,3,1,5,1,3,1,3,5,1,5,1,5,3,4,5,1,2,3,4,5,2,3,2,2,1,5,2,3,4,5,2,1,2,4,5,3,1,5,1,3,1,3,5,1,5,1,5,3,4,5,1,2,3,4,5,2,3,2,2,1,5,2,3,4,5,2,1,2,4,5,3,1,5,1,3
            };

            var output = new List<WorkInstructionTemplate>();

            for (var ii = 1; ii < 11; ii++)
            {
                var w = new WorkInstructionTemplate();
                w.AreaId = 2877;
                w.CompanyId = 136;
                w.CreatedAt = DateTime.Now;
                w.Description = string.Concat("This is a description of a work Instructions ", ii);
                w.Id = ii;
                ////w.MaxScore = 5;
                //w.MinScore = 1;
                w.ModifiedAt = DateTime.Now;
                w.Name = workInstructionNames[ii];//string.Concat("Work Instruction Name ", ii);
                w.Picture = images[ii];//"136/lists/0/d01372d3-bc9c-4813-bc89-865b44afba1c.png";
                w.Role = RoleTypeEnum.Basic;
                //w.ScoreType = ScoreTypeEnum.Score;
                w.WorkInstructionType = InstructionTypeEnum.SkillInstruction;
                w.Media = images.ToList();


                if (generateValues)
                {
                    w.InstructionItems = new List<InstructionItemTemplate>();
                    for (var iii = 1; iii < 11; iii++)
                    {
                        var wi = new InstructionItemTemplate();
                        wi.CompanyId = 136;
                        wi.Description = string.Concat("This is a description of a work instructions item ", iii);
                        wi.Id = iii;
                        wi.Name = string.Concat(workInstructionNames[ii], " ", iii);
                        wi.Picture = images[iii]; //"136/lists/0/d01372d3-bc9c-4813-bc89-865b44afba1c.png";

                        w.InstructionItems.Add(wi);
                    }

                    w.NumberOfInstructionItems = w.InstructionItems.Count;
                }
                else
                {
                    w.NumberOfInstructionItems = scores[ii];
                }

                output.Add(w);
            };

            output = await AppendAreaPathsAsync(objects: output);

            await Task.CompletedTask;

            return output;
        }

        private async Task<List<WorkInstructionTemplate>> AppendAreaPathsAsync(List<WorkInstructionTemplate> objects)
        {

            var areas = await _areaManager.GetAreasAsync(companyId: 136, maxLevel: 99, useTreeview: false);
            if (areas != null && areas.Count > 0)
            {
                foreach (var item in objects)
                {
                    var area = areas?.Where(x => x.Id == item.AreaId)?.FirstOrDefault();
                    if (area != null)
                    {
                        item.AreaPath = area?.FullDisplayName;
                        item.AreaPathIds = area?.FullDisplayIds;
                    }

                }
            }
            return objects;
        }
        #endregion
    }
}
