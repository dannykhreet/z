using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Managers;
using EZGO.Api.Models;
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
using System.Collections.Generic;
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
    public class AssessmentTemplatesController : BaseController<AssessmentTemplatesController>
    {
        #region - privates -
        private readonly IAreaManager _areaManager;
        private readonly IGeneralManager _generalManager;
        private readonly IAssessmentManager _assessmentManager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly IFlattenAssessmentManager _flattenedAssessmentManager;
        #endregion

        #region - contructor(s) -
        public AssessmentTemplatesController(IUserManager userManager, IFlattenAssessmentManager flattenedAuditManager, IConfigurationHelper configurationHelper, IToolsManager toolsManager, IAssessmentManager assessmentManager, IGeneralManager generalManager, IAreaManager areaManager, ILogger<AssessmentTemplatesController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _areaManager = areaManager;
            _generalManager = generalManager;
            _assessmentManager = assessmentManager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _flattenedAssessmentManager = flattenedAuditManager;
        }
        #endregion

        [Route("assessmenttemplates")]
        [Route("skillassessmenttemplates")]
        [HttpGet]
        public async Task<IActionResult> GetAssessmentTemplates([FromQuery] AssessmentTypeEnum? assessmenttype, [FromQuery] RoleTypeEnum? role, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null, [FromQuery] bool? useDebug = null, [FromQuery] string filtertext = null)
        {
            _assessmentManager.Culture = TranslationLanguage;

            var filters = new AssessmentFilters()
            {
                AreaId = areaid,
                Role = role,
                AssessmentType = assessmenttype,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_ASSESSMENT_RETURN_ITEMS,
                Offset = offset,
                AllowedOnly = allowedonly,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),
                FilterText = filtertext
            };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.GetAssessmentTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                           filters: filters,
                                                           include: include);

            AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();
            Agent.Tracer.CurrentTransaction.StartSpan("result.serialization.getassessmenttemplates", ApiConstants.ActionExec);

            var returnresult = (result).ToJsonFromObject();

            Agent.Tracer.CurrentSpan.End();
            return StatusCode((int)HttpStatusCode.OK, returnresult);


        }

        [Route("assessmenttemplate/{assessmenttemplateid}")]
        [Route("skillassessmenttemplate/{assessmenttemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetAssessmentTemplate([FromRoute] int assessmenttemplateid, [FromQuery] string include, [FromQuery] bool? useDebug = null)
        {
            _assessmentManager.Culture = TranslationLanguage;

            if (!AssessmentValidators.TemplateIdIsValid(assessmenttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessmenttemplateid, objectType: ObjectTypeEnum.AssessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.GetAssessmentTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                          assessmentTemplateId: assessmenttemplateid,
                                                          include: include);

            AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("assessmenttemplate/add")]
        [Route("skillassessmenttemplate/add")]
        [HttpPost]
        public async Task<IActionResult> AddAssessment([FromBody] AssessmentTemplate assessmentTemplate, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AssessmentValidators.AssessmentTemplateIsValid(assessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTTEMPLATE_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!assessmentTemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                           messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: assessmentTemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var result = await _assessmentManager.AddAssessmentTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                              userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                              assessmentTemplate: assessmentTemplate);

            AssessmentTemplate retrievedAssessmentTemplate = null;

            //if flatten data on or fulloutput, retrieve audit template for further processing
            if (result > 0 && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                retrievedAssessmentTemplate = await _assessmentManager.GetAssessmentTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), assessmentTemplateId: result, include: "instructions,instructionitems,areapaths,tags", connectionKind: ConnectionKind.Writer);
            }

            //flatten data
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedAssessmentManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), retrievedAssessmentTemplate);
            }

            if (fulloutput && result > 0 && retrievedAssessmentTemplate != null)
            {
                AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (retrievedAssessmentTemplate).ToJsonFromObject());
            }
            else
            {
                AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }


        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("assessmenttemplate/change/{assessmenttemplateid}")]
        [Route("skillassessmenttemplate/change/{assessmenttemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAssessment([FromBody] AssessmentTemplate assessmentTemplate, [FromRoute] int assessmenttemplateid, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (assessmenttemplateid != assessmentTemplate.Id) { return BadRequest(); }

            if (!AssessmentValidators.TemplateIdIsValid(assessmenttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTTEMPLATE_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!AssessmentValidators.AssessmentTemplateIsValid(assessmentTemplate, isExisting: true))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTTEMPLATE_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessmenttemplateid, objectType: ObjectTypeEnum.AssessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!assessmentTemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                            userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                            messages: out var possibleMessages, ignoreCreatedByCheck: true,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: assessmentTemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            var result = await _assessmentManager.ChangeAssessmentTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    assessmentTemplateId: assessmenttemplateid,
                                                                    assessmentTemplate: assessmentTemplate);


            AssessmentTemplate retrievedAssessmentTemplate = null;

            //if flatten data on or fulloutput, retrieve audit template for further processing
            if (result && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                retrievedAssessmentTemplate = await _assessmentManager.GetAssessmentTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), assessmentTemplateId: assessmenttemplateid, include: "instructions,instructionitems,areapaths,tags", connectionKind: ConnectionKind.Writer);
            }

            //flatten data
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedAssessmentManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), retrievedAssessmentTemplate);
            }

            if (fulloutput && result && retrievedAssessmentTemplate != null)
            {
                AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (retrievedAssessmentTemplate).ToJsonFromObject());

            }
            else
            {

                AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("assessmenttemplate/setactive/{assessmenttemplateid}")]
        [Route("assessmenttemplate/delete/{assessmenttemplateid}")]
        [Route("skillassessmenttemplate/delete/{assessmenttemplateid}")]
        [HttpPost]
        public async Task<IActionResult> DeleteAssessment([FromRoute] int assessmenttemplateid, [FromBody] object isactive, [FromQuery] bool? useDebug = null)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AssessmentValidators.TemplateIdIsValid(assessmenttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AssessmentValidators.MESSAGE_ASSESSMENTID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isactive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: assessmenttemplateid, objectType: ObjectTypeEnum.AssessmentTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _assessmentManager.SetAssessmentTemplateActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), assessmentTemplateId: assessmenttemplateid, isActive: BooleanConverter.ConvertObjectToBoolean(isactive));

            AppendCapturedExceptionToApm(_assessmentManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        private async Task<List<AssessmentTemplate>> AppendAreaPathsAsync(List<AssessmentTemplate> objects)
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
    }
}
