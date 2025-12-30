using Elastic.Apm;
using Elastic.Apm.Api;
using EZ.Connector.Init.Interfaces;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Cleaners;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// ChecklistsController; contains all routes based on checklists.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Checklists)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ChecklistsController : BaseController<ChecklistsController>
    {
        #region - privates -
        private readonly IMemoryCache _cache;
        private readonly IChecklistManager _manager;
        private readonly IActionManager _actionManager;
        private readonly ITaskManager _taskManager;
        private readonly IVersionReleaseManager _versionReleaseManager;
        private readonly IGeneralManager _generalManager;
        private readonly IConnectorManager _connectorManager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly IFlattenChecklistManager _flattenedChecklistManager;
        #endregion

        #region - contructor(s) -
        public ChecklistsController(IFlattenChecklistManager flattenChecklistManager, IUserManager userManager, IChecklistManager manager, IMemoryCache memoryCache, IToolsManager toolsManager, IConnectorManager connectorManager, IVersionReleaseManager versionReleaseManager, IActionManager actionmanager, IConfigurationHelper configurationHelper, IGeneralManager generalManager, ILogger<ChecklistsController> logger, IApplicationUser applicationUser, ITaskManager taskManager) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _actionManager = actionmanager;
            _versionReleaseManager = versionReleaseManager;
            _generalManager = generalManager;
            _connectorManager = connectorManager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _cache = memoryCache;
            _taskManager = taskManager;
            _flattenedChecklistManager = flattenChecklistManager;
        }
        #endregion

        #region - GET routes checklists -
        [Route("checklists")]
        [HttpGet]
        public async Task<IActionResult> GetChecklists([FromQuery] string timestamp, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] string filterText, [FromQuery] int? signedbyid, [FromQuery] bool? iscompleted, [FromQuery] int? templateid, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery]int? limit, [FromQuery]int? offset, [FromQuery] bool? sortByModifiedAt = null, [FromQuery] bool? allowedonly = null, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] int? taskId = null)
        {
            _manager.Culture = TranslationLanguage;

            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            DateTime parsedStartTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimestamp)) { };

            DateTime parsedEndTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimestamp)) { };


            var filters = new ChecklistFilters() {  AreaId = areaid,
                                                    IsCompleted = iscompleted == null ? true : iscompleted,
                                                    SignedById = signedbyid,
                                                    TemplateId = templateid,
                                                    FilterAreaType = filterareatype,
                                                    AllowedOnly = allowedonly,
                                                    Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_CHECKLIST_RETURN_ITEMS,
                                                    Offset = offset,
                                                    Timestamp = !string.IsNullOrEmpty(timestamp) && parsedTimeStamp != DateTime.MinValue ? parsedTimeStamp : new Nullable<DateTime>(),
                                                    StartTimestamp = !string.IsNullOrEmpty(starttimestamp) && parsedStartTimestamp != DateTime.MinValue ? parsedStartTimestamp : new Nullable<DateTime>(),
                                                    EndTimestamp = !string.IsNullOrEmpty(endtimestamp) && parsedEndTimestamp != DateTime.MinValue ? parsedEndTimestamp : new Nullable<DateTime>(),
                                                    TimespanType = timespantype,
                                                    TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                                                    TaskId = taskId,
                                                    SortByModifiedAt = sortByModifiedAt,
                                                    FilterText = filterText
            }; //TODO refactor


            var uniqueKey = string.Format("GET_CHECKLISTS_T{0}_C{1}_U{2}_L{3}_O{4}", parsedTimeStamp.ToString("dd-MM-yyyy_HH:mm"), await CurrentApplicationUser.GetAndSetCompanyIdAsync(), await CurrentApplicationUser.GetAndSetUserIdAsync(), filters.Limit, filters.Offset);
            bool enableTrafficShaping = await _generalManager.GetIsSetInSetting("checklists", "TECH_TRAFFICSHAPING");
            if (enableTrafficShaping)
            {
                if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_COMPANIES"))
                {
                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_LIMITS"))
                    {
                        if (filters.Limit.HasValue && filters.Limit.Value == 0) { filters.Limit = ApiSettings.DEFAULT_MAX_NUMBER_OF_CHECKLIST_RETURN_ITEMS; }
                    }

                    if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), "TECH_TRAFFICSHAPING_CHECKLISTS"))
                    {
                        if (ProtectionHelper.CheckRunningRequest(_cache, uniqueKey)) return StatusCode((int)HttpStatusCode.TooManyRequests);
                    }
                    else
                    {
                        enableTrafficShaping = false;
                    }
                } else
                {
                    enableTrafficShaping = false;
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetChecklistsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                           filters: filters,
                                                           include: include,
                                                           useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                                                          featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_CHECKLIST_STORAGE));

            if(result.Any())
            {
                if(filters.Limit > 0) //only clean when specific filter number, and not retrieve all by 0
                {
                    result = ChecklistCleaner.CleanChecklistsForRetrieval(result);
                }
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            Agent.Tracer.CurrentTransaction.StartSpan("result.serialization", ApiConstants.ActionExec);

            var returnresult = (result).ToJsonFromObject();

            Agent.Tracer.CurrentSpan.End();

            if (enableTrafficShaping) ProtectionHelper.RemoveRunningRequest(_cache, uniqueKey);

            return StatusCode((int)HttpStatusCode.OK, returnresult);


        }

        [Route("checklist/{checklistid}")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistById([FromRoute]int checklistid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!ChecklistValidators.ChecklistIdIsValid(checklistid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_CHECKLIST_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklistid, objectType: ObjectTypeEnum.Checklist))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetChecklistAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                          checklistId: checklistid,
                                                          include: include,
                                                          useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                                                          featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_CHECKLIST_STORAGE));
           
            if(result != null)  result = ChecklistCleaner.CleanChecklistForRetrieval(result);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - POST routes checklists -
        //[Route("checklist/validate")]
        //[HttpPost]
        //public async Task<IActionResult> ValidateChecklist([FromBody] Checklist checklist)
        //{
        //    var validationResult = ChecklistValidators.ChecklistIsValidAndSetDefaults(checklist: checklist, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
        //    if (!string.IsNullOrEmpty(validationResult))
        //    {
        //        return StatusCode((int)HttpStatusCode.BadRequest, (validationResult).ToJsonFromObject());
        //    }

        //    if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklist.TemplateId, objectType: ObjectTypeEnum.ChecklistTemplate))
        //    {
        //        return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
        //    }

        //    return StatusCode((int)HttpStatusCode.OK, (true).ToJsonFromObject());
        //}

        [Route("checklist/add")]
        [HttpPost]
        public async Task<IActionResult> AddChecklist([FromBody] Checklist checklist, [FromQuery] bool fulloutput = false)
        {
            int companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            int userId = await this.CurrentApplicationUser.GetAndSetUserIdAsync();
            ChecklistTemplate versionedTemplate;

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklist.TemplateId, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!string.IsNullOrEmpty(checklist.Version)
                && checklist.Version != await _flattenedChecklistManager.RetrieveLatestAvailableVersion(checklist.TemplateId, companyId)
                && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
            {
                versionedTemplate = await _flattenedChecklistManager.RetrieveFlattenData(templateId: checklist.TemplateId, companyId: companyId, version: checklist.Version);
            }
            else
            {
                versionedTemplate = null;
            }

            if (!checklist.ValidateAndClean(companyId: companyId,
                      userId: userId,
                      messages: out var possibleMessages,
                      validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(companyId) : null,
                      stageTemplates: versionedTemplate != null ? versionedTemplate.StageTemplates : await _manager.GetStageTemplatesByChecklistTemplateIdAsync(companyId, checklist.TemplateId),
                      taskTemplates: versionedTemplate != null ? versionedTemplate.TaskTemplates : await _manager.GetTaskTemplatesWithChecklistTemplate(companyId: companyId, checklistTemplateId: checklist.TemplateId),
                      existingStages: null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: checklist.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.AddChecklistAsync(companyId: companyId, userId: userId, checklist:checklist);

            String includes = Settings.ApiSettings.FULL_INCLUDE_LIST;
            //if gen4 properties were passed into the function, add gen4 properties to the output for the include TODO check properties in tasks
            if ((checklist.PropertiesGen4 != null && checklist.PropertiesGen4.Count > 0) || 
                (checklist.OpenFieldsPropertiesGen4 != null && checklist.OpenFieldsPropertiesGen4.Count > 0) ||
                (checklist.Tasks.FindAll(t => t.PropertiesGen4 != null && t.PropertiesGen4.Count > 0).Count > 0)
                )
            {
                includes = string.Concat(includes,",", IncludesEnum.PropertiesGen4.ToString().ToLower());
            }

            var resultfull = await _manager.GetChecklistAsync(companyId: companyId, checklistId: result, include: includes, connectionKind: Data.Enumerations.ConnectionKind.Writer);

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableStaticStorageChecklistsAudits") && resultfull != null && resultfull.Id > 0)
            {
                var sv = await _versionReleaseManager.SaveStaticChecklistAsync(checklistJson: (resultfull).ToJsonFromObject(), id: resultfull.Id, companyId: companyId, userId: userId);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            if (fulloutput && result > 0)
            {
                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Route("checklist/change/{checklistid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeChecklist([FromRoute]int checklistid, [FromBody] Checklist checklist, [FromQuery] bool fulloutput = false)
        {
            int companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            int userId = await this.CurrentApplicationUser.GetAndSetUserIdAsync();
            ChecklistTemplate versionedTemplate;

            if (!ChecklistValidators.ChecklistIdIsValid(checklistid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_CHECKLIST_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!ChecklistValidators.ChecklistExists(checklist: checklist))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_CHECKLIST_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklist.Id, objectType: ObjectTypeEnum.Checklist))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklist.TemplateId, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            //try to get the version from db if the version was not provided
            if (string.IsNullOrEmpty(checklist.Version))
            {
                checklist.Version = await _flattenedChecklistManager.RetrieveVersionForExistingObjectAsync(checklist.Id, companyId);
            }

            if (!string.IsNullOrEmpty(checklist.Version) 
                && checklist.Version != await _flattenedChecklistManager.RetrieveLatestAvailableVersion(checklist.TemplateId, companyId) 
                && await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "TECH_FLATTEN_DATA"))
            {
                versionedTemplate = await _flattenedChecklistManager.RetrieveFlattenData(templateId: checklist.TemplateId, companyId: companyId, version: checklist.Version);
            }
            else
            {
                versionedTemplate = null;
            }

            if (!checklist.ValidateAndClean(companyId: companyId,
                      userId: userId,
                      messages: out var possibleMessages,
                      validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(companyId) : null,
                      stageTemplates: versionedTemplate != null ? versionedTemplate.StageTemplates : await _manager.GetStageTemplatesByChecklistTemplateIdAsync(companyId: companyId, checklistTemplateId: checklist.TemplateId),
                      taskTemplates: versionedTemplate != null ? versionedTemplate.TaskTemplates : await _manager.GetTaskTemplatesWithChecklistTemplate(companyId: companyId, checklistTemplateId: checklist.TemplateId),
                      existingStages: await _manager.GetStagesByChecklistIdAsync(companyId: companyId, checklistId: checklist.Id)))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: checklist.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Checklist currentChecklist = await _manager.GetChecklistAsync(companyId: companyId, checklistId: checklistid);
            if (!checklist.ValidateMutation(currentChecklist, out string messages))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: checklist.ToJsonFromObject(), response: messages);
                return StatusCode((int)HttpStatusCode.BadRequest, messages.ToJsonFromObject());
            }

            var result = await _manager.ChangeChecklistAsync(companyId: companyId, userId: userId, checklistId: checklistid, checklist: checklist);

            if(currentChecklist.LinkedTaskId != null && checklist.IsCompleted)
            {
                await _taskManager.setTaskCompletedFromChecklistIfAllowed(companyId: companyId, userId: userId, taskId: (int)currentChecklist.LinkedTaskId);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            if (fulloutput && result)
            {
                string includes = Settings.ApiSettings.FULL_INCLUDE_LIST;
                if ((checklist.PropertiesGen4 != null && checklist.PropertiesGen4.Count > 0) ||
                    (checklist.OpenFieldsPropertiesGen4 != null && checklist.OpenFieldsPropertiesGen4.Count > 0) ||
                    (checklist.Tasks.FindAll(t => t.PropertiesGen4 != null && t.PropertiesGen4.Count > 0).Count > 0)
                    )
                {
                    includes = string.Concat(includes, ",", IncludesEnum.PropertiesGen4.ToString().ToLower());
                }

                var resultfull = await _manager.GetChecklistAsync(companyId: companyId, checklistId: checklistid, include: includes, connectionKind: Data.Enumerations.ConnectionKind.Writer);
                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }


        [Route("checklist/setactive/{checklistid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveChecklist([FromRoute]int checklistid, [FromBody] object isActive)
        {
            if (!ChecklistValidators.ChecklistIdIsValid(checklistid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_CHECKLIST_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklistid, objectType: ObjectTypeEnum.Checklist))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetChecklistActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), checklistId: checklistid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("checklist/setcompleted/{checklistid}")]
        [HttpPost]
        public async Task<IActionResult> SetCompletedChecklist([FromRoute]int checklistid, [FromBody] object iscompleted)
        {
            if (!BooleanValidator.CheckValue(iscompleted))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklistid, objectType: ObjectTypeEnum.Checklist))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetChecklistCompletedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), checklistId: checklistid, isCompleted: BooleanConverter.ConvertObjectToBoolean(iscompleted));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("checklist/sign/{checklistid}")]
        [HttpPost]
        public async Task<IActionResult> SignChecklist([FromRoute]int checklistid, [FromBody] ChecklistRelationSigning signing)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklistid, objectType: ObjectTypeEnum.Checklist))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!signing.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: signing.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChecklistSigningAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), checklistId: checklistid, signing: signing);

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableStaticStorageChecklistsAudits") )
            {
                var resultfull = await _manager.GetChecklistAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), checklistId: checklistid, include: Settings.ApiSettings.FULL_INCLUDE_LIST, connectionKind: Data.Enumerations.ConnectionKind.Writer);
                if(resultfull != null && resultfull.Id > 0)
                {
                    var sv = await _versionReleaseManager.SaveStaticChecklistAsync(checklistJson: (resultfull).ToJsonFromObject(), id: resultfull.Id, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());
                }
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("checklist/tasks/setstatus")]
        [Route("checklist/create")]
        [HttpPost]
        public async Task<IActionResult> SetTaskStatusAndOrCreateChecklist([FromBody] ChecklistRelationStatus checklistrelationstatus)
        {
            if (!checklistrelationstatus.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: checklistrelationstatus.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = checklistrelationstatus;

            //TODO possible rename and move checks. Refactor.
            if (checklistrelationstatus.TaskId > 0 || checklistrelationstatus.TaskTemplateId > 0 && checklistrelationstatus.ChecklistId.HasValue && checklistrelationstatus.ChecklistId.Value > 0)
            {
                result = await _manager.SetChecklistTaskStatusAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), checklistRelation: checklistrelationstatus);
            }
            else
            {
                result = await _manager.CreateChecklistAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), checklistRelation: checklistrelationstatus);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetChecklistsHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("checklists/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistsHealth()
        {
            try
            {
                var result = await _manager.GetChecklistsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new ChecklistFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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