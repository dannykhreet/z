using Elastic.Apm;
using Elastic.Apm.Api;
using EZ.Connector.Init.Interfaces;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// AuditsController; contains all routes based on audits.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Audits)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class AuditsController : BaseController<AuditsController>
    {
        #region - privates -
        private readonly IAuditManager _manager;
        private readonly IActionManager _actionManager;
        private readonly IVersionReleaseManager _versionReleaseManager;
        private readonly IGeneralManager _generalManager;
        private readonly IConnectorManager _connectorManager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        #endregion

        #region - contructor(s) -
        public AuditsController(IUserManager userManager, IAuditManager manager, IConnectorManager connectorManager, IToolsManager toolsManager, IVersionReleaseManager versionReleaseManager, IActionManager actionmanager, IConfigurationHelper configurationHelper, IGeneralManager generalManager,  ILogger<AuditsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _actionManager = actionmanager;
            _versionReleaseManager = versionReleaseManager;
            _generalManager = generalManager;
            _connectorManager = connectorManager;
            _toolsManager = toolsManager;
            _userManager = userManager;
        }
        #endregion

        #region - GET routes audits -
        [Route("audits")]
        [HttpGet]
        public async Task<IActionResult> GetAudits([FromQuery] string timestamp, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] int? signedbyid, [FromQuery] bool? iscompleted, [FromQuery] int? templateid, [FromQuery] ScoreTypeEnum? scoretype,[FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery]int? limit, [FromQuery]int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] string filtertext = null)
        {
            _manager.Culture = TranslationLanguage;

            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            DateTime parsedStartTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimestamp)) { };

            DateTime parsedEndTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimestamp)) { };


            var filters = new AuditFilters() {  AreaId = areaid,
                                                IsCompleted = iscompleted == null ? true : iscompleted,
                                                SignedById = signedbyid,
                                                TemplateId = templateid,
                                                ScoreType = scoretype,
                                                FilterAreaType = filterareatype,
                                                AllowedOnly = allowedonly,
                                                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_AUDIT_RETURN_ITEMS,
                                                Offset = offset,
                                                Timestamp = !string.IsNullOrEmpty(timestamp) && parsedTimeStamp!=DateTime.MinValue ? parsedTimeStamp : new Nullable<DateTime>(),
                                                StartTimestamp = !string.IsNullOrEmpty(starttimestamp) && parsedStartTimestamp!=DateTime.MinValue ? parsedStartTimestamp : new Nullable<DateTime>(),
                                                EndTimestamp = !string.IsNullOrEmpty(endtimestamp) && parsedEndTimestamp!=DateTime.MinValue ? parsedEndTimestamp : new Nullable<DateTime>(),
                                                TimespanType = timespantype,
                                                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                                                FilterText = filtertext

            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAuditsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                       userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                       filters: filters,
                                                       include: include,
                                                       useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                                                       featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_AUDIT_STORAGE));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("audit/{auditid}")]
        [HttpGet]
        public async Task<IActionResult> GetAudit([FromRoute]int auditid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!AuditValidators.AuditIdIsValid(auditid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_AUDIT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: auditid, objectType: ObjectTypeEnum.Audit))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                      auditId:auditid,
                                                      include: include,
                                                      useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                                                      featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_AUDIT_STORAGE));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("audit/{auditid}/template")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTemplate([FromRoute]int auditid)
        {
            _manager.Culture = TranslationLanguage;

            if (!AuditValidators.AuditIdIsValid(auditid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_AUDIT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: auditid, objectType: ObjectTypeEnum.Audit))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            //TODO fix
            await Task.CompletedTask;
            var result = new object();
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("audit/{auditid}/tasks")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTasks([FromRoute]int auditid)
        {
            _manager.Culture = TranslationLanguage;

            if (!AuditValidators.AuditIdIsValid(auditid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_AUDIT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: auditid, objectType: ObjectTypeEnum.Audit))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            //TODO fix
            await Task.CompletedTask;
            var result = new object();
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }


        #endregion

        #region - POST routes audit -
        [Route("audit/add")]
        [HttpPost]
        public async Task<IActionResult> AddAudit([FromBody] Audit audit, [FromQuery] bool fulloutput = false)
        {
            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audit.TemplateId, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!audit.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: audit.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.AddAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), audit: audit);

            String includes = Settings.ApiSettings.FULL_INCLUDE_LIST;
            //if gen4 properties were passed into the function, add gen4 properties to the output for the include TODO properties in tasks
            if ((audit.PropertiesGen4 != null && audit.PropertiesGen4.Count > 0) || 
                (audit.OpenFieldsPropertiesGen4 != null && audit.OpenFieldsPropertiesGen4.Count > 0) ||
                (audit.Tasks.FindAll(t => t.PropertiesGen4 != null && t.PropertiesGen4.Count > 0).Count > 0))
            {
                includes = string.Concat(includes, ",", IncludesEnum.PropertiesGen4.ToString().ToLower());
            }


            var resultfull = await _manager.GetAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditId: result, include: includes, connectionKind: Data.Enumerations.ConnectionKind.Writer);

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableStaticStorageChecklistsAudits") && resultfull != null && resultfull.Id > 0)
            {
                var sv = await _versionReleaseManager.SaveStaticAuditAsync(auditJson: (resultfull).ToJsonFromObject(), id: resultfull.Id, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());
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

        [Route("audit/change/{auditid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAudit([FromRoute]int auditid, [FromBody] Audit audit, [FromQuery] bool fulloutput = false)
        {
            if (!AuditValidators.AuditIdIsValid(auditid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_AUDIT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audit.Id, objectType: ObjectTypeEnum.Audit))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audit.TemplateId, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!audit.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                      userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                      messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: audit.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Audit currentAudit = await _manager.GetAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditId: auditid);
            if (!audit.ValidateMutation(currentAudit, out string messages))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: audit.ToJsonFromObject(), response: messages);
                return StatusCode((int)HttpStatusCode.BadRequest, messages.ToJsonFromObject());
            }

            var result = await _manager.ChangeAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditId:auditid, audit: audit);
            if (fulloutput && result)
            {
                var resultfull = await _manager.GetAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditId: auditid, include: Settings.ApiSettings.FULL_INCLUDE_LIST, connectionKind: Data.Enumerations.ConnectionKind.Writer);

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Route("audit/setactive/{auditid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveAudit([FromRoute]int auditid, [FromBody] object isActive)
        {
            if (!AuditValidators.AuditIdIsValid(auditid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_AUDIT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: auditid, objectType: ObjectTypeEnum.Audit))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetAuditActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditId: auditid, isActive: BooleanConverter.ConvertObjectToBoolean(isActive));
            
            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("audit/setscore/{auditid}")]
        [HttpPost]
        public async Task<IActionResult> SetScoreAudit([FromRoute] int auditid, [FromBody] object score)
        {
            if (!AuditValidators.AuditIdIsValid(auditid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_AUDIT_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!IntegerValidator.CheckValue(score))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: auditid, objectType: ObjectTypeEnum.Audit))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetAuditScoreAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditId: auditid, score: IntegerConverter.ConvertObjectToInteger(score));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }



        [Route("audit/sign/{auditid}")]
        [HttpPost]
        public async Task<IActionResult> SignAudit([FromRoute]int auditid, [FromBody] AuditRelationSigning signing)
        {
            if (!signing.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: signing.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: auditid, objectType: ObjectTypeEnum.Audit))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.AuditSigningAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditId: auditid, signing: signing);

            if (_configurationHelper.GetValueAsBool("AppSettings:EnableStaticStorageChecklistsAudits"))
            {
                var resultfull = await _manager.GetAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditId: auditid, include: Settings.ApiSettings.FULL_INCLUDE_LIST, connectionKind: Data.Enumerations.ConnectionKind.Writer);
                if(resultfull != null && resultfull.Id > 0)
                {
                    var sv = await _versionReleaseManager.SaveStaticAuditAsync(auditJson: (resultfull).ToJsonFromObject(), id: resultfull.Id, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());
                }
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("audit/tasks/setstatus")]
        [Route("audit/create")]
        [HttpPost]
        public async Task<IActionResult> SetTaskStatusAndOrCreateAudit([FromBody] AuditRelationStatus auditrelationstatus)
        {
            if (!auditrelationstatus.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: auditrelationstatus.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = auditrelationstatus;
            //TODO possible rename and move checks. Refactor.
            if (auditrelationstatus.TaskId > 0 || auditrelationstatus.TaskTemplateId > 0 && auditrelationstatus.AuditId.HasValue && auditrelationstatus.AuditId.Value > 0)
            {
                result = await _manager.SetAuditTaskStatusAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditRelation: auditrelationstatus);
            }
            else
            {
                result = await _manager.CreateAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditRelation: auditrelationstatus);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("audit/tasks/setstatusandscore")]
        [Route("audit/createandscore")]
        [HttpPost]
        public async Task<IActionResult> SetTaskStatusScoreAndOrCreateAudit([FromBody] AuditRelationStatusScore auditrelationstatus)
        {
            if (!auditrelationstatus.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: auditrelationstatus.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = auditrelationstatus;
            if(auditrelationstatus.Score.HasValue)
            {
                //TODO possible rename and move checks. Refactor.
                if (auditrelationstatus.TaskId > 0 || auditrelationstatus.TaskTemplateId > 0 && auditrelationstatus.AuditId.HasValue && auditrelationstatus.AuditId.Value > 0)
                {
                    result = await _manager.SetAuditTaskStatusScoreAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditRelation: auditrelationstatus);
                }
                else
                {
                    result = await _manager.CreateAuditAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditRelation: auditrelationstatus);
                }

                AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ("No score available").ToJsonFromObject());
            }
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetAuditsHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("audits/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetAuditsHealth()
        {
            try
            {
                var result = await _manager.GetAuditsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new AuditFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT});

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