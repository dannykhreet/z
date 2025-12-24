using Elastic.Apm.Api;
using Elastic.Apm;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Models.TemplateSharing;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// InboxController; contains all routes based on shared templates.
    /// </summary>
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.TemplateSharing)]
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class SharedTemplateController : BaseController<SharedTemplateController>
    {
        #region - privates -
        private readonly ISharedTemplateManager _manager;
        #endregion

        #region - constructor(s) -
        public SharedTemplateController( ISharedTemplateManager manager, IConfigurationHelper configurationHelper, ILogger<SharedTemplateController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }
        #endregion

        #region - GET -
        /// <summary>
        /// Get a list of information on incoming shared templates
        /// </summary>
        /// <returns>A list of SharedTemplates representing incoming shared templates</returns>
        [Route("inbox")]
        [HttpGet]
        public async Task<IActionResult> GetSharedTemplates()
        {
            int companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetSharedTemplatesAsync(companyId: companyId);

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Count the number of incoming shared templates.
        /// </summary>
        /// <returns>Number of incoming shared templates</returns>
        [Route("inbox/count")]
        [HttpGet]
        public async Task<IActionResult> GetSharedTemplatesCount()
        {
            int companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetSharedTemplatesCountAsync(companyId: companyId);
            
            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Use shared template id to get the json for the shared template object representing the template at the moment it was shared.
        /// </summary>
        /// <param name="sharedtemplateid">The id of the shared template</param>
        /// <returns>Json representation of the shared template at the moment it was shared</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("inbox/{sharedtemplateid}")]
        [HttpGet]
        public async Task<IActionResult> GetSharedTemplate([FromRoute] int sharedtemplateid)
        {
            if (!ChecklistValidators.SharedTemplateIdIsValid(sharedtemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: sharedtemplateid, objectType: ObjectTypeEnum.SharedTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            TemplateJson templateJson = await _manager.GetSharedTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), sharedTemplateId: sharedtemplateid);

            if (string.IsNullOrEmpty(templateJson.Json))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Template not found with given id".ToJsonFromObject());
            }

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, templateJson.Json);
        }
        #endregion

        #region - POST -
        /// <summary>
        /// Using a shared template id, declines a shared template and removes it from the inbox
        /// </summary>
        /// <param name="sharedtemplateid">Id of the shared template to decline</param>
        /// <returns>True if successful, otherwise false</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("inbox/reject/{sharedtemplateid}")]
        [HttpPost]
        public async Task<IActionResult> DeclineSharedTemplate([FromRoute] int sharedtemplateid)
        {
            if (!this.IsCmsRequest)
            {
                return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());
            }

            if (!ChecklistValidators.TemplateIdIsValid(sharedtemplateid))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ChecklistValidators.MESSAGE_TEMPLATE_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            if (!await this.CurrentApplicationUser.CheckObjectRights(objectId: sharedtemplateid, objectType: ObjectTypeEnum.SharedTemplate))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            var result = await _manager.RejectSharedTemplateAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                                                  sharedTemplateId: sharedtemplateid);

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
        #endregion
    }
}
