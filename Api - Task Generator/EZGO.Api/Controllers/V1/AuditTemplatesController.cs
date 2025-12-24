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
    /// AuditTemplatesController; contains all routes based on audit template.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Audits)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class AuditTemplatesController : BaseController<AuditTemplatesController>
    {
        #region - privates -
        private readonly IAuditManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly ICompanyManager _companyManager;
        private readonly ISharedTemplateManager _sharedTemplateManager;
        private readonly ITranslationManager _translationManager;
        private readonly IGeneralManager _generalManager;
        private readonly IFlattenAuditManager _flattenedAuditManager;
        #endregion

        #region - constructor(s) -
        public AuditTemplatesController(ICompanyManager companyManager, IGeneralManager generalManager, IFlattenAuditManager flattenAuditManager, ISharedTemplateManager sharedTemplateManager, IUserManager userManager, IAuditManager manager, IToolsManager toolsManager, IConfigurationHelper configurationHelper, ILogger<AuditTemplatesController> logger, IApplicationUser applicationUser, ITranslationManager translationManager) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _companyManager = companyManager;
            _sharedTemplateManager = sharedTemplateManager;
            _generalManager = generalManager;
            _flattenedAuditManager = flattenAuditManager;
            _translationManager = translationManager;
        }
        #endregion

        #region - GET routes audittemplate -

        /// <summary>
        /// GetAuditTemplates; Get audit templates with selected filters
        /// </summary>
        /// <param name="filterText"></param>
        /// <param name="scoretype"></param>
        /// <param name="role"></param>
        /// <param name="roles"></param>
        /// <param name="instructionsAdded"></param>
        /// <param name="imagesAdded"></param>
        /// <param name="areaid"></param>
        /// <param name="filterareatype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <returns></returns>
        [Route("audittemplates")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTemplates([FromQuery] string filterText, [FromQuery] ScoreTypeEnum? scoretype, [FromQuery] RoleTypeEnum? role, [FromQuery] string roles, [FromQuery] bool? instructionsAdded, [FromQuery] bool? imagesAdded, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery]int? limit, [FromQuery]int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            var filters = new AuditFilters() { 
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ScoreType = scoretype, RoleType = role,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_AUDITTEMPLATES_RETURN_ITEMS, Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                FilterText = filterText,
                Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").Select(id => (RoleTypeEnum)Convert.ToInt32(id)).ToList(),
                InstructionsAdded = instructionsAdded,
                ImagesAdded = imagesAdded
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAuditTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        /// <summary>
        /// GetAuditTemplateCounts; Get counts for audit templates with selected filters
        /// </summary>
        /// <param name="filterText"></param>
        /// <param name="scoretype"></param>
        /// <param name="role"></param>
        /// <param name="roles"></param>
        /// <param name="instructionsAdded"></param>
        /// <param name="imagesAdded"></param>
        /// <param name="areaid"></param>
        /// <param name="filterareatype"></param>
        /// <param name="tagids"></param>
        /// <param name="include"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="allowedonly"></param>
        /// <returns></returns>
        [Route("audittemplates_counts")]
        [Route("audittemplates/counts")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTemplateCounts([FromQuery] string filterText, [FromQuery] ScoreTypeEnum? scoretype, [FromQuery] RoleTypeEnum? role, [FromQuery] string roles, [FromQuery] bool? instructionsAdded, [FromQuery] bool? imagesAdded, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;

            var filters = new AuditFilters()
            {
                AreaId = areaid,
                FilterAreaType = filterareatype,
                ScoreType = scoretype,
                RoleType = role,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_AUDITTEMPLATES_RETURN_ITEMS,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                FilterText = filterText,
                Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").Select(id => (RoleTypeEnum)Convert.ToInt32(id)).ToList(),
                InstructionsAdded = instructionsAdded,
                ImagesAdded = imagesAdded
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAuditTemplateCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }


        [Route("audittemplate/{audittemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTemplate([FromRoute]int audittemplateid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;

            if (!AuditValidators.TemplateIdIsValid(audittemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audittemplateid, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAuditTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditTemplateId: audittemplateid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("audittemplates/names")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTemplateNames([FromQuery] List<int> ids)
        {
            if (ids == null || ids.Count == 0) { return BadRequest(); }

            var result = await _manager.GetAuditTemplateNamesAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), audittemplateIds: ids);
            return Ok(result);
        }
        #endregion

        #region - POST routes audittemplates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("audittemplate/add")]
        [HttpPost]
        public async Task<IActionResult> AddAuditTemplate([FromBody] AuditTemplate audittemplate, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AuditTemplateValidators.AuditTemplateIsValid(audittemplate))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditTemplateValidators.MESSAGE_AUDITTEMPLATE_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!audittemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                          userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                          messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: audittemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }


            var result = await _manager.AddAuditTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  auditTemplate: audittemplate);
             
            await _translationManager.TranslateAndSaveObjectAsync(result, "audit"); 


            AuditTemplate retrievedAuditTemplate = null;

            if (audittemplate.SharedTemplateId.HasValue && audittemplate.SharedTemplateId.Value > 0)
                await _sharedTemplateManager.AcceptSharedTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), sharedTemplateId: audittemplate.SharedTemplateId.Value);

            //if flatten data on or fulloutput, retrieve audit template for further processing

            if (result > 0 && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                retrievedAuditTemplate = await _manager.GetAuditTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditTemplateId: result, include: "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,tags", connectionKind: ConnectionKind.Writer);
            }

            //flatten data
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedAuditManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), retrievedAuditTemplate);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            if (fulloutput && result > 0 && retrievedAuditTemplate != null)
            {
                return StatusCode((int)HttpStatusCode.OK, (retrievedAuditTemplate).ToJsonFromObject());

            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("audittemplate/change/{audittemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeAuditTemplate([FromRoute]int audittemplateid, [FromBody] AuditTemplate audittemplate, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AuditValidators.TemplateIdIsValid(audittemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!AuditTemplateValidators.AuditTemplateIsValid(audittemplate))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditTemplateValidators.MESSAGE_AUDITTEMPLATE_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audittemplateid, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!audittemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                        userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                        messages: out var possibleMessages, ignoreCreatedByCheck: true,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: audittemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChangeAuditTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  auditTemplateId: audittemplateid,
                                                                  auditTemplate: audittemplate);

            await _translationManager.TranslateAndSaveObjectAsync(audittemplateid, "audit");

            AuditTemplate retrievedAuditTemplate = null;
            //if flatten data on or fulloutput, retrieve audit template for further processing
            if (result && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                retrievedAuditTemplate = await _manager.GetAuditTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditTemplateId: audittemplateid, include: "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,tags", connectionKind: ConnectionKind.Writer);
            }

            //flatten data
            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedAuditManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), retrievedAuditTemplate);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            if (fulloutput && result && retrievedAuditTemplate != null)
            {
                return StatusCode((int)HttpStatusCode.OK, (retrievedAuditTemplate).ToJsonFromObject());

            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("audittemplate/setactive/{audittemplateid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveAuditTemplate([FromRoute]int audittemplateid, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AuditValidators.TemplateIdIsValid(audittemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audittemplateid, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetAuditTemplateActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  auditTemplateId: audittemplateid,
                                                                  isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Shares an audit tempalte with other companies.
        /// The template will be shared in it's current state.
        /// The other companies will get the checklist in an inbox, to be accepted or declined.
        /// </summary>
        /// <param name="audittemplateid">The id of the template to be shared</param>
        /// <param name="companyids">List of company ids to share the template to</param>
        /// <returns>True if sharing was successful</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("audittemplate/share/{audittemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ShareAuditTemplate([FromRoute] int audittemplateid, [FromBody] List<int> companyids)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!AuditValidators.TemplateIdIsValid(audittemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audittemplateid, objectType: ObjectTypeEnum.AuditTemplate))
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
            AuditTemplate auditTemplate = await _manager.GetAuditTemplateAsync(companyId: companyId, auditTemplateId: audittemplateid, include: "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,tags");

            foreach (int selectedCompanyId in selectedCompanyIds)
                resultsIds.Add(await _sharedTemplateManager.ShareAuditTemplateAsync(fromCompanyId: companyId, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), auditTemplate: auditTemplate, toCompanyId: selectedCompanyId));

            var result = resultsIds.Count > 0;

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - GET routes linked checklisttemplates/tasktemplates - 
        [Route("audittemplate/connections/tasktemplates/{audittemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetConnectedTaskTemplateIds([FromRoute] int audittemplateid)
        {
            if (!AuditValidators.TemplateIdIsValid(audittemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, AuditValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: audittemplateid, objectType: ObjectTypeEnum.AuditTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetConnectedTaskTemplateIds(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditTemplateId: audittemplateid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetAuditTemplatesHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("audittemplates/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetAuditTemplatesHealth()
        {
            try
            {
                var result = await _manager.GetAuditTemplatesAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new AuditFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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