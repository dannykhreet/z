using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Security.Helpers;
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
    /// <summary>
    /// TaskTemplatesController; contains all routes based on task templates.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class TaskTemplatesController : BaseController<TaskTemplatesController>
    {
        #region - privates -
        private readonly ITaskManager _manager;
        private readonly ITaskGenerationManager _taskGenerationManager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly ICompanyManager _companyManager;
        private readonly ISharedTemplateManager _sharedTemplateManager;
        private readonly IFlattenTaskManager _flattenTaskManager;
        private readonly IGeneralManager _generalManager;
        private readonly ITranslationManager _translationManager;

        #endregion

        #region - contructor(s) -
        public TaskTemplatesController(IGeneralManager generalManager, IFlattenTaskManager flattenTaskManager, ISharedTemplateManager sharedTemplateManager, ICompanyManager companyManager, IUserManager userManager, ITaskManager manager, ITaskGenerationManager taskGenerationManager, IToolsManager toolsManager, IConfigurationHelper configurationHelper, ITranslationManager translationManager, ILogger<TaskTemplatesController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _taskGenerationManager = taskGenerationManager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _companyManager = companyManager;
            _sharedTemplateManager = sharedTemplateManager;
            _flattenTaskManager = flattenTaskManager;
            _generalManager = generalManager;
            _translationManager = translationManager;
        }
        #endregion

        #region - GET routes tasktemplates -
        [Route("tasktemplates")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplates([FromQuery] string filterText, [FromQuery] string roles, [FromQuery] string recurrencytypes, [FromQuery] bool? instructionsAdded, [FromQuery] bool? imagesAdded, [FromQuery] bool? videosAdded, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] TaskTypeEnum? tasktype, [FromQuery] RoleTypeEnum? role, [FromQuery] string tagids, [FromQuery] string include, [FromQuery]int? limit, [FromQuery]int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] RecurrencyTypeEnum? recurrencytype = null)
        {
            _manager.Culture = TranslationLanguage;

            var filters = new TaskFilters() { 
                AreaId = areaid,
                FilterAreaType = filterareatype,
                TaskType = tasktype,
                RecurrencyType = recurrencytype,
                Role = role,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASKTEMPLATES_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                FilterText = filterText,
                Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").Select(id => (RoleTypeEnum)Convert.ToInt32(id)).ToList(),
                InstructionsAdded = instructionsAdded,
                ImagesAdded = imagesAdded,
                VideosAdded = videosAdded,
                RecurrencyTypes = string.IsNullOrEmpty(recurrencytypes) ? null : recurrencytypes.Split(",").Select(id => (RecurrencyTypeEnum)Convert.ToInt32(id)).ToList(),
            }; 

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include : include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("tasktemplates_counts")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplatesCounts([FromQuery] string filterText, [FromQuery] string roles, [FromQuery] string recurrencytypes, [FromQuery] bool? instructionsAdded, [FromQuery] bool? imagesAdded, [FromQuery] bool? videosAdded, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] TaskTypeEnum? tasktype, [FromQuery] RoleTypeEnum? role, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] RecurrencyTypeEnum? recurrencytype = null)
        {
            _manager.Culture = TranslationLanguage;

            var filters = new TaskFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                TaskType = tasktype,
                RecurrencyType = recurrencytype,
                Role = role,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASKTEMPLATES_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                FilterText = filterText,
                Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").Select(id => (RoleTypeEnum)Convert.ToInt32(id)).ToList(),
                InstructionsAdded = instructionsAdded,
                ImagesAdded = imagesAdded,
                VideosAdded = videosAdded,
                RecurrencyTypes = string.IsNullOrEmpty(recurrencytypes) ? null : recurrencytypes.Split(",").Select(id => (RecurrencyTypeEnum)Convert.ToInt32(id)).ToList(),
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplatesCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("tasktemplatesactions")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplatesActions([FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] TaskTypeEnum? tasktype, [FromQuery] RoleTypeEnum? role, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] RecurrencyTypeEnum? recurrencytype = null)
        {
            _manager.Culture = TranslationLanguage;

            var filters = new TaskFilters() { 
                AreaId = areaid,
                FilterAreaType = filterareatype,
                TaskType = tasktype,
                Role = role,
                RecurrencyType = recurrencytype,
                AllowedOnly = allowedonly,
                Offset = offset,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_TASKTEMPLATES_RETURN_ITEMS,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray()
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplatesActionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        [Route("tasktemplate/{tasktemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplate([FromRoute]int tasktemplateid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!TaskValidators.TemplateIdIsValid(tasktemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: tasktemplateid, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskTemplateId: tasktemplateid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("tasktemplate/{templateid}/taskcounts")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplates([FromRoute] int templateid)
        {
         
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskTemplatePreviousTaskCountAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskTemplateId: templateid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - POST routes tasktemplates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplate/add")]
        [HttpPost]
        public async Task<IActionResult> AddTaskTemplate([FromBody] TaskTemplate tasktemplate, [FromQuery] bool fulloutput = false, [FromQuery] bool generateTemplate = true)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!TaskTemplateValidators.TaskTemplateIsValid(tasktemplate))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskTemplateValidators.MESSAGE_TASKTEMPLATE_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!tasktemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                           messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: tasktemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.AddTaskTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskTemplate: tasktemplate);

            await _translationManager.TranslateAndSaveObjectAsync(result, "task");

            if (tasktemplate.SharedTemplateId.HasValue && tasktemplate.SharedTemplateId.Value > 0)
                await _sharedTemplateManager.AcceptSharedTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), sharedTemplateId: tasktemplate.SharedTemplateId.Value);

            TaskTemplate resultfull = null;

            if (result > 0 && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                resultfull = await _manager.GetTaskTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskTemplateId: result, include: "steps,propertyvalues,recurrecy,recurrencyshifts,properties,propertydetails,instructionrelations,tags", connectionKind: Data.Enumerations.ConnectionKind.Writer);
            }

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenTaskManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), flattenObject: resultfull);
            }

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableTaskGenerationOnNewChangedObjects") && generateTemplate)
            {
                var rectype = RecurrencyTypeEnumExtension.ConvertStringToRecurrencyTypeEnum(recurrencyTypeString: tasktemplate.RecurrencyType);
                if (rectype.HasValue)
                {
                    //add generation
                    await _taskGenerationManager.GenerateSpecificTemplate(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), templateId: result, recurrencyType: rectype.Value);
                }
            }

            if (fulloutput && result > 0)
            {
                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplate/change/{tasktemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAddTaskTemplate([FromRoute]int tasktemplateid, [FromBody] TaskTemplate tasktemplate, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!TaskValidators.TemplateIdIsValid(tasktemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!TaskTemplateValidators.TaskTemplateIsValid(tasktemplate))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskTemplateValidators.MESSAGE_TASKTEMPLATE_IS_NOT_VALID.ToJsonFromObject());
            }

            if (tasktemplateid > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: tasktemplateid, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (tasktemplate.Recurrency != null && tasktemplate.Recurrency.Id > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: tasktemplate.Recurrency.Id, objectType: ObjectTypeEnum.TaskRecurrency))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (tasktemplate.Recurrency!= null && tasktemplate.Recurrency.TemplateId > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: tasktemplate.Recurrency.TemplateId, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!tasktemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                          userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                          messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: tasktemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChangeTaskTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskTemplateId: tasktemplateid,taskTemplate: tasktemplate);

            await _translationManager.TranslateAndSaveObjectAsync(tasktemplateid, "task");

            TaskTemplate resultfull = null;

            if (result && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                resultfull = await _manager.GetTaskTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskTemplateId: tasktemplateid, include: "steps,propertyvalues,recurrency,recurrencyshifts,properties,propertydetails,instructionrelations,tags", connectionKind: Data.Enumerations.ConnectionKind.Writer);
            }

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenTaskManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), flattenObject: resultfull);
            }

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableTaskGenerationOnNewChangedObjects"))
            {
                var rectype = RecurrencyTypeEnumExtension.ConvertStringToRecurrencyTypeEnum(recurrencyTypeString: tasktemplate.RecurrencyType);
                if (rectype.HasValue)
                {
                    //add generation
                    await _taskGenerationManager.GenerateSpecificTemplate(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), templateId: tasktemplateid, recurrencyType: rectype.Value);
                }
            }

            if (fulloutput && result)
            {
                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplate/setactive/{tasktemplateid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveTaskTemplate([FromRoute]int tasktemplateid, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!TaskValidators.TemplateIdIsValid(tasktemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (tasktemplateid > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: tasktemplateid, objectType: ObjectTypeEnum.TaskTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetTaskTemplateActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskTemplateId: tasktemplateid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            if (!BooleanConverter.ConvertObjectToBoolean(isActive))
            {
                var resultDerivatives = await _manager.SetTaskTemplateDerivativeInActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskTemplateId: tasktemplateid);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplates/setindexes")]
        [HttpPost]
        public async Task<IActionResult> SetTaskTemplatesSetIndex([FromBody] List<IndexItem> templateIndices)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            foreach (var item in templateIndices)
            {
                if (!TaskValidators.TemplateIdIsValid(item.Id))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
                }

                if (item.Id > 0 && !await this.CurrentApplicationUser.CheckObjectRights(objectId: item.Id, objectType: ObjectTypeEnum.TaskTemplate))
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
                }
            }

            var result = await _manager.SetTemplateIndices(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), templateIndices: templateIndices);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Shares a task template with other companies.
        /// The tempplate will be shared in it's current state.
        /// The other companies will receive the template in an inbox, to be accepted of declined.
        /// </summary>
        /// <param name="tasktemplateid">id of the task template to be shared</param>
        /// <param name="companyids">list of company ids to share to</param>
        /// <returns>true if sharing was successful</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("tasktemplate/share/{tasktemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ShareTaskTemplate([FromRoute] int tasktemplateid, [FromBody] List<int> companyids)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!TaskValidators.TemplateIdIsValid(tasktemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, TaskValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: tasktemplateid, objectType: ObjectTypeEnum.TaskTemplate))
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
            TaskTemplate taskTemplate = await _manager.GetTaskTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), taskTemplateId: tasktemplateid, include: "steps,propertyvalues,recurrency,recurrencyshifts,properties,propertydetails,instructionrelations,tags");

            foreach (int selectedCompanyId in selectedCompanyIds)
                resultsIds.Add(await _sharedTemplateManager.ShareTaskTemplateAsync(fromCompanyId: companyId, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), taskTemplate: taskTemplate, toCompanyId: selectedCompanyId));

            var result = resultsIds.Count > 0;

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetTaskTemplatesHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("tasktemplates/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetTaskTemplatesHealth()
        {
            try
            {
                var result = await _manager.GetTaskTemplatesAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new TaskFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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
        #endregion
    }
}