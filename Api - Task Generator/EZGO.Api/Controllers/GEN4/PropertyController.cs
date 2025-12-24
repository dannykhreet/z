using Elastic.Apm.Api;
using Elastic.Apm;
using System.Net;
using System.Threading.Tasks;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Json;
using System;

namespace EZGO.Api.Controllers.GEN4
{
    /// <summary>
    /// PropertyController; contains all routes based on properties.
    /// Can be used for GEN4 or new/optimized implementations on existing clients.
    /// </summary>
    [Route(ApiSettings.VERSION_GEN4_BASE_API_ROUTE)]
    [ApiController]
    public class PropertyController : BaseController<PropertyController>
    {
        #region - privates -
        private readonly IPropertyValueManager _manager;
        private readonly IUserManager _userManager;
        #endregion

        #region - contructor(s) -
        public PropertyController(IPropertyValueManager manager,  ILogger<PropertyController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper, IUserManager userManager) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
            _userManager = userManager;
        }
        #endregion

        /// <summary>
        /// Sets the value of a property for the current user and company.
        /// </summary>
        /// <remarks>This method validates the provided property data, ensuring it is associated with the
        /// current user and company. If the property already exists, its value is updated; otherwise, a new property
        /// value is added.</remarks>
        /// <param name="property">An object containing the property details, including the value to be set and associated metadata.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a JSON response with the
        /// status code: <list type="bullet"> <item><description><see cref="HttpStatusCode.OK"/> if the operation
        /// succeeds.</description></item> <item><description><see cref="HttpStatusCode.BadRequest"/> if the provided
        /// property data is invalid, along with validation error messages.</description></item> </list></returns>
        [Route("property/setValue")]
        [HttpPost]
        public async Task<IActionResult> SetPropertyValue([FromBody] PropertyDTO property)
        {
            if (!property.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                           messages: out var possibleMessages,
                                           validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            int result = 0;

            if(property.UserValue.Id != null)
            {
                result = await _manager.ChangePropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), propertyUserValueId: (int)property.UserValue.Id, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), property: property);
            } 
            else
            {
                result = await _manager.AddPropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), property: property);
            }


            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        /// <summary>
        /// Updates the value of a property associated with a specific user.
        /// </summary>
        /// <remarks>
        /// <b>Deprecated.</b> This method is deprecated and may be removed in future versions.
        /// This method validates the provided property details against the current user's
        /// company and user context.  If validation fails, a list of error messages is returned. The method also tracks
        /// the operation's execution  using the application's tracing system.
        /// </remarks>
        /// <param name="property">An object containing the updated property details. The object must pass validation before the update is
        /// applied.</param>
        /// <param name="propertyUserValueId">The unique identifier of the property value to be updated.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation. Returns a 400 Bad Request status if
        /// the validation fails,  or the updated property value in JSON format with the appropriate status code upon
        /// success.</returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("property/changeValue/{propertyUserValueId}")]
        [HttpPost]
        public async Task<IActionResult> ChangePropertyValue([FromBody] PropertyDTO property, [FromRoute] int propertyUserValueId)
        {
            if (!property.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                           messages: out var possibleMessages,
                                           validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.ChangePropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), propertyUserValueId: propertyUserValueId, userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), property: property);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        /// <summary>
        /// Adds a new property value for the current user and company.
        /// </summary>
        /// <remarks>
        /// <b>Deprecated.</b> This method is deprecated and may be removed in future versions.
        /// This method validates the provided property data against the current user's company
        /// and user context.  If validation fails, the method returns a 400 Bad Request response with detailed
        /// validation messages.  On success, the property value is added, and the result is returned as a JSON
        /// response.
        /// </remarks>
        /// <param name="property">The property data to be added, represented as a <see cref="PropertyDTO"/> object.  The property must be
        /// valid and associated with the current user's company.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns a <see
        /// cref="BadRequestObjectResult"/> with validation messages if the input is invalid,  or a JSON response with
        /// the operation result and appropriate HTTP status code.</returns>
        [Obsolete("This method is deprecated and may be removed in future versions.")]
        [Route("property/addValue")]
        [HttpPost]
        public async Task<IActionResult> AddPropertyValue([FromBody] PropertyDTO property)
        {
            if (!property.ValidateAndClean(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                           userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(),
                                           messages: out var possibleMessages,
                                           validUserIds: this.ValidateUserBasedOnCompany ? await _userManager.GetUsersIdsAsync(await this.CurrentApplicationUser.GetAndSetCompanyIdAsync()) : null))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, possibleMessages.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.AddPropertyUserValueAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), property: property);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }
    }
}
