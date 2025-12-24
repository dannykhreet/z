using System;
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
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.GEN4
{
    /// <summary>
    /// Provides API endpoints for managing shift-related operations.
    /// </summary>
    /// <remarks>This controller handles requests related to shift management, such as retrieving shift times.
    /// It is part of the API version defined by <see cref="ApiSettings.VERSION_GEN4_BASE_API_ROUTE"/>.</remarks>
    [Route(ApiSettings.VERSION_GEN4_BASE_API_ROUTE)]
    [ApiController]
    public class ShiftsController : BaseController<PropertyController>
    {
        #region - privates -
        private readonly IShiftManager _manager;
        #endregion

        #region - contructor(s) -
        public ShiftsController(IShiftManager manager, ILogger<PropertyController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }
        #endregion

        /// <summary>
        /// Retrieves task templates based on a specified timestamp and offset.
        /// </summary>
        /// <remarks>This method interacts with the shift management system to obtain task templates
        /// relevant to the specified time. It uses the current user's company context to filter the results.</remarks>
        /// <param name="timestamp">The optional timestamp to filter the task templates. If null, the current time is used.</param>
        /// <param name="offset">The offset in hours to adjust the timestamp. Defaults to 0.</param>
        /// <returns>An <see cref="IActionResult"/> containing the task templates in JSON format.</returns>
        [Route("shift/getshifttimes")]
        [HttpGet]
        public async Task<IActionResult> getShiftTimes([FromQuery] DateTime? timestamp, [FromQuery] int offset = 0)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetShiftTimestampsByOffsetAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: timestamp, shiftOffset: offset);
            
            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        /// <summary>
        /// Retrieves times for the shift, day and week based on the specified timestamp.
        /// </summary>
        /// <remarks>This endpoint fetches times for the shift, day and week associated with the specified
        /// timestamp. If no timestamp is provided, the current date and time are used as the reference point.</remarks>
        /// <param name="timestamp">The optional timestamp used to determine the day and week for which shift times are retrieved. If null, the
        /// current date and time are used.</param>
        /// <returns>An <see cref="IActionResult"/> containing the shift times for the day and week in JSON format.</returns>
        [Route("shift/getshiftdayweektimes")]
        [HttpGet]
        public async Task<IActionResult> getShiftDayWeekTimes([FromQuery] DateTime? timestamp)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetShiftDayWeekTimesByTimestamp(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: timestamp);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

    }
}
