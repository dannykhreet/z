using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.General
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class PropertyValueController : BaseController<PropertyValueController>
    {
        #region - variables -
        private readonly IPropertyValueManager _manager;
        private readonly IUserManager _userManager;
        #endregion

        #region - constructor(s) -
        public PropertyValueController(IUserManager userManager, IConfigurationHelper configurationHelper, IPropertyValueManager manager, ILogger<PropertyValueController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _userManager = userManager;

        }
        #endregion

        #region - gets-
        [Route("properties")]
        [HttpGet]
        public async Task<IActionResult> GetProperties([FromQuery] int? propertygroupid, [FromQuery] string propertygroupids, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            if(!string.IsNullOrEmpty(propertygroupids)) {
                filters.PropertyGroupIds = propertygroupids.Split(",").Where(x => !string.IsNullOrEmpty(x) && int.TryParse(x, out int _empty)).Select(x => Convert.ToInt32(x)).ToArray();
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertiesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("property/{propertyid}")]
        [HttpGet]
        public async Task<IActionResult> GetProperty([FromRoute] int propertyid)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyId: propertyid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("propertygroups")]
        [HttpGet]
        public async Task<IActionResult> GetPropertyGroups([FromQuery] int? propertygroupid, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertyGroupsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("properties/audittemplates")]
        [HttpGet]
        public async Task<IActionResult> GetPropertiesAuditTemplates([FromQuery] int? propertygroupid, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertiesAuditTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("properties/checklisttemplates")]
        [HttpGet]
        public async Task<IActionResult> GetPropertiesChecklistTemplates([FromQuery] int? propertygroupid, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertiesChecklistTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("properties/tasktemplates")]
        [HttpGet]
        public async Task<IActionResult> GetPropertiesTaskTemplates([FromQuery] int? propertygroupid, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertiesTaskTemplatesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("properties/tasks")]
        [HttpGet]
        public async Task<IActionResult> GetPropertiesTasks([FromQuery] int? propertygroupid, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertyUserValuesWithTasks(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("properties/actions")]
        [HttpGet]
        public async Task<IActionResult> GetPropertiesActions([FromQuery] int? propertygroupid, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPropertiesActionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("property/values")]
        [HttpGet]
        public async Task<IActionResult> GetPropertiesValues([FromQuery] int? propertygroupid, [FromQuery] string include, [FromQuery] int? limit, [FromQuery] int? offset)
        {
            //var filters = new PropertyFilters() { PropertyGroupId = propertygroupid, Limit = limit.HasValue ? limit.Value : ApiSettings.DEFAULT_MAX_NUMBER_OF_RETURN_ITEMS };

            //Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            //var result = await _manager.GetPropertiesActionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), filters: filters, include: include);

            //Agent.Tracer.CurrentSpan.End();
            await Task.CompletedTask;

            return GetObjectResultJsonWithStatus("");

        }
        #endregion

        #region - posts -
        //TODO a few controllers will be removed due to all in one handling of data of templates

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("property/add")]
        [HttpPost]
        public async Task<IActionResult> AddProperty([FromBody] Property property)
        {
            if (!property.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddPropertyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), property: property);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("propertyuservalue/add")]
        [Route("propertyuservalue/tasks/add")]
        [HttpPost]
        public async Task<IActionResult> AddPropertyValue([FromBody] PropertyUserValue propertyValue)
        {
            if (!propertyValue.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                  userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                  messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddPropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValue: propertyValue);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("propertyuservalue/checklist/add")]
        [HttpPost]
        public async Task<IActionResult> AddPropertyValueChecklist([FromBody] PropertyUserValue propertyValue)
        {
            if (!propertyValue.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (!await ValidateChecklistPropertyUserValue(propertyUserValue: propertyValue))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddChecklistPropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValue: propertyValue);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("propertyuservalues/checklist")]
        [Route("propertyuservalues/checklist/addchange")]
        [HttpPost]
        public async Task<IActionResult> AddChangePropertyValuesChecklist([FromBody] List<PropertyUserValue> propertyValues)
        {
            if (!await ValidateChecklistPropertyUserCollectionCollection(collection: propertyValues))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if(propertyValues != null)
            {
                foreach(PropertyUserValue item in propertyValues)
                {
                    if (!item.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                    }
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);


            var result = await _manager.AddChangeChecklistPropertyUserValuesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValues: propertyValues);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("propertyuservalue/audit/add")]
        [HttpPost]
        public async Task<IActionResult> AddPropertyValueAudit([FromBody] PropertyUserValue propertyValue)
        {
            if (!propertyValue.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            if (!await ValidateAuditPropertyUserValue(propertyUserValue: propertyValue))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddAuditPropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValue: propertyValue);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("propertyuservalues/audit")]
        [Route("propertyuservalues/audit/addchange")]
        [HttpPost]
        public async Task<IActionResult> AddChangePropertyValuesAudits([FromBody] List<PropertyUserValue> propertyValues)
        {
            if (!await ValidateAuditPropertyUserCollectionCollection(collection: propertyValues))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (propertyValues != null)
            {
                foreach (PropertyUserValue item in propertyValues)
                {
                    if (!item.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
                    {
                        return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
                    }
                }
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddChangeAuditPropertyUserValuesAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValues: propertyValues);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);

        }

        [Route("propertyuservalue/change/{propertyuservalueid}")]
        [Route("propertyuservalue/tasks/change/{propertyuservalueid}")]
        [HttpPost]
        public async Task<IActionResult> ChangePropertyValue([FromBody] PropertyUserValue propertyValue, [FromRoute] int propertyuservalueid)
        {
            if (!propertyValue.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                 userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                 messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangePropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), propertyUserValueId: propertyuservalueid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValue: propertyValue);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("propertyuservalue/checklist/change/{propertyuservalueid}")]
        [HttpPost]
        public async Task<IActionResult> ChangePropertyValueChecklist([FromBody] PropertyUserValue propertyValue, [FromRoute] int propertyuservalueid)
        {
            if (!await ValidateChecklistPropertyUserValue(propertyUserValue: propertyValue))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!propertyValue.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeChecklistPropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), propertyUserValueId: propertyuservalueid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValue: propertyValue);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("propertyuservalue/audit/change/{propertyuservalueid}")]
        [HttpPost]
        public async Task<IActionResult> ChangePropertyValueAudit([FromBody] PropertyUserValue propertyValue, [FromRoute] int propertyuservalueid)
        {
            if (!await ValidateAuditPropertyUserValue(propertyUserValue: propertyValue))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, AuthenticationSettings.MESSAGE_FORBIDDEN_OBJECT.ToJsonFromObject());
            }

            if (!propertyValue.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                messages: out var possibleMessages,
                                              validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangeAuditPropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), propertyUserValueId: propertyuservalueid, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), propertyValue: propertyValue);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("propertygroup/add")]
        [HttpPost]
        public async Task<IActionResult> AddPropertyGroupValue([FromBody] PropertyGroup propertyValue)
        {
            var result = ""; //await _manager.AddActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), action: action);
            await Task.CompletedTask;
            return GetObjectResultJsonWithStatus(result);

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("property/change/{propertyid}")]
        [HttpPost]
        public async Task<IActionResult> ChangeProperty([FromRoute] int propertyid, [FromBody] Property property)
        {
            var result = ""; //await _manager.AddActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), action: action);
            await Task.CompletedTask;
            return GetObjectResultJsonWithStatus(result);

        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES)]
        [Route("propertygroup/change/{propertygroupid}")]
        [HttpPost]
        public async Task<IActionResult> ChangePropertyGroup([FromRoute] int propertygroupid, [FromBody] PropertyGroup propertyGroup)
        {
            var result = ""; //await _manager.AddActionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), action: action);
            await Task.CompletedTask;
            return GetObjectResultJsonWithStatus(result);

        }
        #endregion

        #region - health checks -
        /// <summary>
        /// GetPropertiesHealth; Checks the basic property functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("properties/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetPropertiesHealth()
        {
            try
            {
                var result = await _manager.GetPropertiesAsync(_configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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

        /// <summary>
        /// GetPropertyValuesHealth; Checks the basic propertyvalues functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("propertyvalues/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetPropertyValuesHealth()
        {

            try
            {
                var result = await _manager.GetPropertyValues(_configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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


        /// <summary>
        /// GetPropertyValuesHealth; Checks the basic propertyvalues functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("propertyvaluekinds/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetPropertyValueKindsHealth()
        {

            try
            {
                var result = await _manager.GetPropertyValueKinds(_configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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

        /// <summary>
        /// GetPropertyGroupsHealth; Checks the basic propertyvalues functionality by running a part of the logic with specific id's.
        /// Depending on result this route will return a true / false and a httpstatus.
        /// This route can be used for remote monitoring partial functionalities of the API.
        /// </summary>
        [AllowAnonymous]
        [Route("propertygroups/healthcheck")]
        [HttpGet]
        public async Task<IActionResult> GetPropertyGroupsHealth()
        {

            try
            {
                var result = await _manager.GetPropertyGroupsAsync(_configurationHelper.GetValueAsInteger(Settings.ApiSettings.HEALTHCHECK_COMPANY_ID_CONFIG_KEY));

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

        #region - controller input validator -
        /// <summary>
        /// ValidateAuditPropertyUserCollectionCollection; For each item the parent object are checked if there are rights for using these.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateAuditPropertyUserCollectionCollection(List<PropertyUserValue> collection)
        {
            if(collection != null)
            {
                var output = true;
                foreach(var item in collection)
                {
                    if(output == true)
                    {
                        output = await ValidateAuditPropertyUserValue(propertyUserValue: item);
                    }

                }
                return output;
            } else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="propertyUserValue"></param>
        /// <returns></returns>
        private async Task<bool> ValidateAuditPropertyUserValue(PropertyUserValue propertyUserValue)
        {
            var output = true;

            if (output == true && propertyUserValue.Id > 0)
            {
                output = await this.CurrentApplicationUser.CheckObjectRights(objectId: propertyUserValue.Id, objectType: ObjectTypeEnum.AuditProperties);
            }

            if (output == true && propertyUserValue.TemplatePropertyId > 0)
            {
                output = await this.CurrentApplicationUser.CheckObjectRights(objectId: propertyUserValue.TemplatePropertyId, objectType: ObjectTypeEnum.AuditTemplateProperties);
            }

            if (output == true && propertyUserValue.AuditId.HasValue && propertyUserValue.AuditId.Value > 0)
            {
                output = await this.CurrentApplicationUser.CheckObjectRights(objectId: propertyUserValue.AuditId.Value, objectType: ObjectTypeEnum.Audit);
            }

            return output;
        }

        /// <summary>
        /// ValidateAuditPropertyUserCollectionCollection; For each item the parent object are checked if there are rights for using these.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateChecklistPropertyUserCollectionCollection(List<PropertyUserValue> collection)
        {
            if (collection != null)
            {
                var output = true;
                foreach (var item in collection)
                {
                    if(output == true)
                    {
                        output = await ValidateChecklistPropertyUserValue(propertyUserValue: item);
                    }

                }
                return output;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="propertyUserValue"></param>
        /// <returns></returns>
        private async Task<bool> ValidateChecklistPropertyUserValue(PropertyUserValue propertyUserValue)
        {
            var output = true;

            if (output == true && propertyUserValue.Id > 0)
            {
                output = await this.CurrentApplicationUser.CheckObjectRights(objectId: propertyUserValue.Id, objectType: ObjectTypeEnum.ChecklistProperties);
            }

            if (output == true && propertyUserValue.TemplatePropertyId > 0)
            {
                output = await this.CurrentApplicationUser.CheckObjectRights(objectId: propertyUserValue.TemplatePropertyId, objectType: ObjectTypeEnum.ChecklistTemplateProperties);
            }

            if (output == true && propertyUserValue.ChecklistId.HasValue && propertyUserValue.ChecklistId.Value > 0)
            {
                output = await this.CurrentApplicationUser.CheckObjectRights(objectId: propertyUserValue.ChecklistId.Value, objectType: ObjectTypeEnum.Checklist);
            }

            return output;
        }

        #endregion
    }
}
