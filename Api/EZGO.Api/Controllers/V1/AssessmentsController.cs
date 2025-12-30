using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Skills;
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
    /// AssessmentController; contains all routes based on assessments.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class AssessmentsController : BaseController<AssessmentsController>
    {
        #region - privates -
        private readonly IUserManager _userManager;
        private readonly IAreaManager _areaManager;
        private readonly IVersionReleaseManager _versionReleaseManager;
        private readonly IGeneralManager _generalManager;
        private readonly IAssessmentManager _assessmentManager;
        private readonly IToolsManager _toolsManager;
        #endregion

        #region - contructor(s) -
        public AssessmentsController(IConfigurationHelper configurationHelper, IGeneralManager generalManager, IToolsManager toolsManager, IVersionReleaseManager versionReleaseManager, IAssessmentManager assessmentManager, IAreaManager areaManager, IUserManager userManager, ILogger<AssessmentsController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _userManager = userManager;
            _areaManager = areaManager;
            _assessmentManager = assessmentManager;
            _generalManager = generalManager;
            _versionReleaseManager = versionReleaseManager;
            _toolsManager = toolsManager;
        }
        #endregion

        #region - skills assessments - 
        [Route("assessments")]
        [Route("skillassessments")]
        [HttpGet]
        public async Task<IActionResult> GetAssessments([FromQuery] string timestamp, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp, [FromQuery] string tagids, [FromQuery] string assessorids, [FromQuery] string include, [FromQuery] int? completedforid = null, [FromQuery] int? assessorid = null, [FromQuery] bool? iscompleted = null, [FromQuery] int? templateid = null, [FromQuery] int? areaid = null, [FromQuery] FilterAreaTypeEnum? filterareatype = null, [FromQuery] int? limit = null, [FromQuery] int? offset = null, [FromQuery] bool? sortByModifiedAt = null, [FromQuery] bool? allowedonly = null, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] RoleTypeEnum? role = null, [FromQuery] AssessmentTypeEnum? assessmenttype = null, [FromQuery] bool? useDebug = null, [FromQuery] string filtertext = null)
        {
            _assessmentManager.Culture = TranslationLanguage;

            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { }
            ;

            DateTime parsedStartTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimestamp)) { }
            ;

            DateTime parsedEndTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimestamp)) { }
            ;

            var filters = new AssessmentFilters()
            {
                AreaId = areaid,
                Role = role,
                AssessmentType = assessmenttype,
                TemplateId = templateid,
                Timestamp = !string.IsNullOrEmpty(timestamp) && parsedTimeStamp != DateTime.MinValue ? parsedTimeStamp : new Nullable<DateTime>(),
                StartTimestamp = !string.IsNullOrEmpty(starttimestamp) && parsedStartTimestamp != DateTime.MinValue ? parsedStartTimestamp : new Nullable<DateTime>(),
                EndTimestamp = !string.IsNullOrEmpty(endtimestamp) && parsedEndTimestamp != DateTime.MinValue ? parsedEndTimestamp : new Nullable<DateTime>(),
                TimespanType = timespantype,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_ASSESSMENT_RETURN_ITEMS,
                Offset = offset,
                AllowedOnly = allowedonly,
                CompletedForId = completedforid,
                AssessorId = assessorid,
                IsCompleted = iscompleted,
                SortByModifiedAt = sortByModifiedAt,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                AssessorIds = string.IsNullOrEmpty(assessorids) ? null : assessorids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                FilterText = filtertext
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.GetAssessmentsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                           filters: filters,
                                                           include: include,
                                                           useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                                                          featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_ASSESSMENT_STORAGE));

            AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            Agent.Tracer.CurrentTransaction.StartSpan("result.serialization.getassessments", ApiConstants.ActionExec);

            var returnresult = (result).ToJsonFromObject();

            Agent.Tracer.CurrentSpan.End();
            return StatusCode((int)HttpStatusCode.OK, returnresult);

        }

        [Route("assessment/{assessmentid}")]
        [Route("skillassessment/{assessmentid}")]
        [HttpGet]
        public async Task<IActionResult> GetAssessment([FromRoute] int assessmentid, [FromQuery] string include, [FromQuery] bool? useDebug = null)
        {
            _assessmentManager.Culture = TranslationLanguage;

            if (!AssessmentValidators.AssessmentIdIsValid(assessmentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessmentid, objectType: ObjectTypeEnum.Assessment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.GetAssessmentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                          assessmentId: assessmentid,
                                                          include: include,
                                                          useStatic: await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                                                          featurekey: Settings.FeatureSettings.TECH_FEATURE_USE_STATIC_ASSESSMENT_STORAGE));

            AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("assessment/add")]
        [Route("skillassessment/add")]
        [HttpPost]
        public async Task<IActionResult> AddAssessment([FromBody] Assessment assessment, [FromQuery] bool fulloutput = false)
        {
            if (!assessment.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                            userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                            messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: assessment.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (!AssessmentValidators.AssessmentIsValid(assessment))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENT_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessment.TemplateId, objectType: ObjectTypeEnum.AssessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.AddAssessmentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), assessment: assessment);

            if (result == -1)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENT_ALREADY_EXISTS.ToJsonFromObject());
            }

            var resultfull = await _assessmentManager.GetAssessmentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), assessmentId: result, include: Settings.ApiSettings.FULL_INCLUDE_LIST_ASSESSMENTS, connectionKind: Data.Enumerations.ConnectionKind.Writer);

            AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            if (_configurationHelper.GetValueAsBool("AppSettings:EnableStaticStorageAssessments") && resultfull != null && resultfull.Id > 0)
            {
                //var sv = await _versionReleaseManager.SaveStaticAssessmentAsync(assessmentJson: (resultfull).ToJsonFromObject(), id: resultfull.Id, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());
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

        [Route("assessment/change/{assessmentid}")]
        [Route("skillassessment/change/{assessmentid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAssessment([FromBody] Assessment assessment, [FromRoute] int assessmentid, [FromQuery] bool fulloutput = false)
        {
            if (!assessment.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                            userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                            messages: out var possibleMessages, ignoreCreatedByCheck: true,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: assessment.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (!AssessmentValidators.AssessmentIdIsValid(assessmentid) || assessment.Id != assessmentid)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTID_IS_NOT_VALID.ToJsonFromObject());
            }
            if (assessment.AssessorId.HasValue && !AssessmentValidators.AssessorIdIsValid(assessment.AssessorId.Value))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!AssessmentValidators.AssessmentIsValid(assessment, isExisting: true))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENT_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessment.Id, objectType: ObjectTypeEnum.Assessment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessment.TemplateId, objectType: ObjectTypeEnum.AssessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Assessment currentAssessment = await _assessmentManager.GetAssessmentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), assessmentId: assessmentid);
            if (!assessment.ValidateMutation(currentAssessment, out string messages))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: assessment.ToJsonFromObject(), response: messages);
                return StatusCode((int)HttpStatusCode.BadRequest, messages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.ChangeAssessmentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), assessmentId: assessmentid, assessment: assessment);

            if (fulloutput && result)
            {
                var resultfull = await _assessmentManager.GetAssessmentAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), assessmentId: assessmentid, include: Settings.ApiSettings.FULL_INCLUDE_LIST_ASSESSMENTS, connectionKind: Data.Enumerations.ConnectionKind.Writer);

                AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (resultfull).ToJsonFromObject());

            }
            else
            {
                AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Route("assessment/setactive/{assessmentid}")]
        [Route("skillassessment/setactive/{assessmentid}")]
        [Route("skillassessment/delete/{assessmentid}")]
        [HttpPost]
        public async Task<IActionResult> DeleteAssessment([FromRoute] int assessmentid, [FromBody] object isactive, [FromQuery] bool? useDebug = null)
        {
            if (!AssessmentValidators.AssessmentIdIsValid(assessmentid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isactive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessmentid, objectType: ObjectTypeEnum.Assessment))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.SetAssessmentActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), assessmentId: assessmentid, isActive: BooleanConverter.ConvertObjectToBoolean(isactive));

            AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health -
        /// <summary>
        /// GetAssessmentsHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Ignore)]
        [AllowAnonymous]
        [Route("assessments/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetAssessHealth()
        {
            try
            {
                var result = await _assessmentManager.GetAssessmentsAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY),
                                                                       userId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_USER_ID_CONFIG_KEY),
                                                                       filters: new AssessmentFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

                AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

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
