using DocumentFormat.OpenXml.Spreadsheet;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.FlattenDataManagers;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
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
    /// ChecklistTemplatesController; contains all routes based on checklist templates.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Checklists)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ChecklistTemplatesController : BaseController<ChecklistTemplatesController>
    {
        #region - privates -
        private readonly IChecklistManager _manager;
        private readonly IToolsManager _toolsManager;
        private readonly IUserManager _userManager;
        private readonly ICompanyManager _companyManager;
        private readonly ISharedTemplateManager _sharedTemplateManager;
        private readonly IFlattenChecklistManager _flattenedChecklistManager;
        private readonly IGeneralManager _generalManager;
        #endregion

        #region - constructor(s) -
        public ChecklistTemplatesController(IGeneralManager generalManager, IFlattenChecklistManager flattenChecklistManager, ISharedTemplateManager sharedTemplateManager, IUserManager userManager, ICompanyManager companyManager, IChecklistManager manager, IToolsManager toolsManager, IConfigurationHelper configurationHelper, ILogger<ChecklistTemplatesController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _toolsManager = toolsManager;
            _userManager = userManager;
            _companyManager = companyManager;
            _sharedTemplateManager = sharedTemplateManager;
            _flattenedChecklistManager = flattenChecklistManager;
            _generalManager = generalManager;
        }
        #endregion

        #region - GET routes checklisttemplates -
        /// <summary>
        /// GetChecklistTemplates; Retrieve checklist templates with selected filters
        /// </summary>
        /// <param name="filterText"></param>
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
        [Route("checklisttemplates")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistTemplates([FromQuery] string filterText, [FromQuery] RoleTypeEnum? role, [FromQuery] string roles, [FromQuery] bool? instructionsAdded, [FromQuery] bool? imagesAdded, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;
            var filters = new ChecklistFilters()
            {
                AreaId = areaid,
                RoleType = role,
                FilterAreaType = filterareatype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_CHECKLISTTEMPLATES_RETURN_ITEMS,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                FilterText = filterText,
                Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").Select(id => (RoleTypeEnum)Convert.ToInt32(id)).ToList(),
                InstructionsAdded = instructionsAdded,
                ImagesAdded = imagesAdded
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetChecklistTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// GetChecklistTemplatesCounts; Retrieve counts of checklisttemplates with selected filtering rules
        /// </summary>
        /// <param name="filterText"></param>
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
        [Route("checklisttemplates_counts")]
        [Route("checklisttemplates/counts")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistTemplatesCounts([FromQuery] string filterText, [FromQuery] RoleTypeEnum? role, [FromQuery] string roles, [FromQuery] bool? instructionsAdded, [FromQuery] bool? imagesAdded, [FromQuery] int? areaid, [FromQuery] FilterAreaTypeEnum? filterareatype, [FromQuery] string tagids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] bool? allowedonly = null)
        {
            _manager.Culture = TranslationLanguage;
            var filters = new ChecklistFilters()
            {
                AreaId = areaid,
                RoleType = role,
                FilterAreaType = filterareatype,
                AllowedOnly = allowedonly,
                Limit = limit ?? ApiSettings.DEFAULT_MAX_NUMBER_OF_CHECKLISTTEMPLATES_RETURN_ITEMS,
                Offset = offset,
                TagIds = string.IsNullOrEmpty(tagids) ? null : tagids.Split(",").Select(id => Convert.ToInt32(id)).ToArray(),

                FilterText = filterText,
                Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").Select(id => (RoleTypeEnum)Convert.ToInt32(id)).ToList(),
                InstructionsAdded = instructionsAdded,
                ImagesAdded = imagesAdded
            }; //TODO refactor

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetChecklistTemplateCountsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        [Route("checklisttemplate/{checklisttemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistTemplateById([FromRoute] int checklisttemplateid, [FromQuery] string include)
        {
            _manager.Culture = TranslationLanguage;
            if (!ChecklistValidators.TemplateIdIsValid(checklisttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklisttemplateid, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetChecklistTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), checklistTemplateId: checklisttemplateid, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("checklisttemplates/names")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistTemplateNames([FromQuery] List<int> ids)
        {
            if (ids == null || ids.Count == 0) { return BadRequest(); }

            var result = await _manager.GetChecklistTemplateNamesAsync(companyId: await CurrentApplicationUser.GetAndSetCompanyIdAsync(), checklistTemplateIds: ids);
            return Ok(result);
        }
        #endregion

        #region - POST routes checklisttemplates -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("checklisttemplate/add")]
        [HttpPost]
        public async Task<IActionResult> AddChecklistTemplate([FromBody] ChecklistTemplate checklisttemplate, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!checklisttemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                            userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                            messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), path: Request.Path, query: Request.QueryString.ToString(), status: ((int)HttpStatusCode.BadRequest).ToString(), header: "N/A", request: checklisttemplate.ToJsonFromObject(), response: possibleMessages);
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }


            var result = await _manager.AddChecklistTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  checklistTemplate: checklisttemplate);

            if (checklisttemplate.SharedTemplateId.HasValue && checklisttemplate.SharedTemplateId.Value > 0)
            {
                await _sharedTemplateManager.AcceptSharedTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), 
                                                                       userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), 
                                                                       sharedTemplateId: checklisttemplate.SharedTemplateId.Value);
            }

            ChecklistTemplate resultfull = null;

            if (result > 0 && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                //todo use full include list and extend full include list
                resultfull = await _manager.GetChecklistTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                      checklistTemplateId: result,
                                                                      include: "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,instructions,openfieldspropertydetails,tags",
                                                                      connectionKind: ConnectionKind.Writer);
            }

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedChecklistManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), flattenObject: resultfull);
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

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("checklisttemplate/change/{checklisttemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeChecklist([FromRoute] int checklisttemplateid, [FromBody] ChecklistTemplate checklisttemplate, [FromQuery] bool fulloutput = false)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!ChecklistValidators.TemplateIdIsValid(checklisttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklisttemplateid, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!checklisttemplate.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                           messages: out var possibleMessages, ignoreCreatedByCheck: true,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                await _toolsManager.WriteToLog(domain: string.Concat(Request.Scheme, "//", Request.Host), 
                                               path: Request.Path, 
                                               query: Request.QueryString.ToString(), 
                                               status: ((int)HttpStatusCode.BadRequest).ToString(), 
                                               header: "N/A", 
                                               request: checklisttemplate.ToJsonFromObject(), 
                                               response: possibleMessages);

                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            var result = await _manager.ChangeChecklistTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                    userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                    checklistTemplateId: checklisttemplateid,
                                                                    checklistTemplate: checklisttemplate);

            ChecklistTemplate resultfull = null;

            if (result && (fulloutput || await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA")))
            {
                //todo use full include list and extend full include list
                resultfull = await _manager.GetChecklistTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                      checklistTemplateId: checklisttemplateid,
                                                                      include: "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,instructions,openfieldspropertydetails,tags",
                                                                      connectionKind: ConnectionKind.Writer);
            }

            if (await _generalManager.GetHasAccessToFeatureByCompany(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), featurekey: "TECH_FLATTEN_DATA"))
            {
                _ = await _flattenedChecklistManager.SaveFlattenData(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), flattenObject: resultfull);
            }

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

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
        [Route("checklisttemplate/setactive/{checklisttemplateid}")]
        [HttpPost]
        public async Task<IActionResult> SetActiveChecklistTemplate([FromRoute] int checklisttemplateid, [FromBody] object isActive)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!ChecklistValidators.TemplateIdIsValid(checklisttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!BooleanValidator.CheckValue(isActive))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, BooleanValidator.MESSAGE_BOOLEAN_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklisttemplateid, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.SetChecklistTemplateActiveAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  checklistTemplateId: checklisttemplateid,
                                                                  isActive: BooleanConverter.ConvertObjectToBoolean(isActive));

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Shares a checklist template with other companies. 
        /// The checklist template will be shared in it's current state.
        /// The other companies will recieve the checklist template in an inbox to be accepted or declined.
        /// </summary>
        /// <param name="checklisttemplateid">id of the checklist template to be shared</param>
        /// <param name="companyids">comma sepparated ids of companies to share the checklist template to</param>
        /// <returns>true if the template was successfully shared</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("checklisttemplate/share/{checklisttemplateid}")]
        [HttpPost]
        public async Task<IActionResult> ShareChecklistTemplate([FromRoute] int checklisttemplateid, [FromBody] List<int> companyids)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!ChecklistValidators.TemplateIdIsValid(checklisttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklisttemplateid, objectType: ObjectTypeEnum.ChecklistTemplate))
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
            ChecklistTemplate checklistTemplate = await _manager.GetChecklistTemplateAsync(companyId: companyId, checklistTemplateId: checklisttemplateid, include: "tasktemplates,steps,properties,propertyvalues,propertydetails,openfields,instructionrelations,tags");

            foreach (int selectedCompanyId in selectedCompanyIds)
                resultsIds.Add(await _sharedTemplateManager.ShareChecklistTemplateAsync(fromCompanyId: companyId, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), checklistTemplate: checklistTemplate, toCompanyId: selectedCompanyId));

            var result = resultsIds.Count > 0;

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - GET routes linked checklisttemplates/tasktemplates - 
        [Route("checklisttemplate/connections/tasktemplates/{checklisttemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetConnectedTaskTemplateIds([FromRoute] int checklisttemplateid)
        {
            if (!ChecklistValidators.TemplateIdIsValid(checklisttemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: checklisttemplateid, objectType: ObjectTypeEnum.ChecklistTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetConnectedTaskTemplateIds(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), checklistTemplateId: checklisttemplateid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetChecklistTemplatesHealth; Checks the basic action functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("checklisttemplates/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistTemplatesHealth()
        {
            try
            {
                var result = await _manager.GetChecklistTemplatesAsync(companyId: _configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY), filters: new ChecklistFilters() { Limit = Settings.ApiSettings.HEALTHCHECK_ITEM_LIMIT });

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