using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Raw;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.GEN4
{
    [Route(ApiSettings.VERSION_GEN4_BASE_API_ROUTE)]
    [ApiController]
    public class StatisticsController : BaseController<StatisticsController>
    {
        private readonly IStatisticsManager _statsManager;

        #region - constructor(s) -
        public StatisticsController(ILogger<StatisticsController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper, IStatisticsManager statsManager) : base(logger, applicationUser, configurationHelper)
        {
            _statsManager = statsManager;
        }
        #endregion

        #region main graphs
        [HttpGet]
        [Route("reporting/statistics/tasks")]
        public async Task<IActionResult> GetTasksStatistics([FromQuery] string timestamp, [FromQuery] int? areaid = null)
        {
            DateTime parsedTimeStamp = new DateTime();
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };
            }
            else
            {
                parsedTimeStamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            var output = await _statsManager.GetTasksStatisticsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid);

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            return Ok(output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/audits")]
        public async Task<IActionResult> GetAuditsStatistics([FromQuery] string timestamp, [FromQuery] int? areaid = null, [FromQuery] int? templateid = null)
        {
            DateTime parsedTimeStamp = new DateTime();
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };
            }
            else
            {
                parsedTimeStamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            var output = await _statsManager.GetAuditsStatisticsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid, templateId: templateid);

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            return Ok(output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/checklists")]
        public async Task<IActionResult> GetChecklistsStatistics([FromQuery] string timestamp, [FromQuery] int? areaid = null)
        {
            DateTime parsedTimeStamp = new DateTime();
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };
            }
            else
            {
                parsedTimeStamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            var output = await _statsManager.GetChecklistsStatisticsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid);

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            return Ok(output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/actions")]
        public async Task<IActionResult> GetActionsStatistics([FromQuery] string timestamp, [FromQuery] int? areaid = null)
        {
            DateTime parsedTimeStamp = new DateTime();
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };
            }
            else
            {
                parsedTimeStamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            var output = await _statsManager.GetActionsStatisticsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp);

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            return Ok(output.ToJsonFromObject());
        }
        #endregion

        #region second layer graphs
        /// <summary>
        /// Retrieves extended second-layer statistics for the specified report type.
        /// </summary>
        /// <remarks>This method validates the input parameters and retrieves the corresponding statistics
        /// based on the specified report type and filters. If the parameters are invalid, an appropriate error message
        /// is returned.</remarks>
        /// <param name="reportType">The type of report to generate. Valid values are "tasks", "checklists", "audits", or "actions".</param>
        /// <param name="timestamp">The timestamp used as a reference point for the statistics, in the format "dd-MM-yyyy HH:mm:ss". If not
        /// provided, the current UTC time is used.</param>
        /// <param name="areaid">An optional area identifier to filter the statistics. If not provided, statistics are not filtered by area.</param>
        /// <param name="periodType">The time period for the statistics. Valid values are "last12days", "last12weeks", "last12months", or
        /// "thisyear". Defaults to "last12days".</param>
        /// <param name="templateid">An optional template identifier. This parameter is only valid when the <paramref name="reportType"/> is
        /// "audits".</param>
        /// <returns>An <see cref="IActionResult"/> containing the extended statistics for the specified report type and
        /// parameters. Returns a <see cref="BadRequestResult"/> if the input parameters are invalid.</returns>
        [HttpGet]
        [Route("reporting/statistics/{reportType}/extended")]
        public async Task<IActionResult> GetSecondLayerStatistics([FromRoute] string reportType, [FromQuery] string timestamp, [FromQuery] int? areaid = null, [FromQuery] string periodType = "last12days", [FromQuery] int? templateid = null)
        {

            DateTime parsedTimeStamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(timestamp))
            {
                if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { }
                ;
            }
            else
            {
                parsedTimeStamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            if (parsedTimeStamp == DateTime.MinValue)
            {
                return BadRequest("Please provide a valid timestamp in the format dd-MM-yyyy HH:mm:ss");
            }

            switch (periodType)
            {
                case "last12days":
                case "last12weeks":
                case "last12months":
                case "thisyear":
                    break;
                default:
                    return BadRequest("Period invalid, must be last12days, last12weeks, last12months or thisyear");
            }

            if(templateid.HasValue && templateid.Value != 0 && reportType != "audits")
            {
                return BadRequest("TemplateId parameter is only valid for audits report type");
            }

            switch (reportType)
            {
                case "tasks":
                case "checklists":
                    var tcOutput = await _statsManager.GetTaskChecklistsStatisticsExtendedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid, periodType: periodType, reportType: reportType);

                    AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                    return Ok(tcOutput.ToJsonFromObject());
                case "audits":
                    var auditOutput = await _statsManager.GetAuditsStatisticsExtendedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid, periodType: periodType, templateId: templateid);

                    AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                    return Ok(auditOutput.ToJsonFromObject());
                case "actions":
                    var actionsOutput = await _statsManager.GetActionsStatisticsExtendedAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), timestamp: parsedTimeStamp, areaId: areaid, periodType: periodType);

                    AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                    return Ok(actionsOutput.ToJsonFromObject());
                default:
                    return BadRequest("Report Type is invalid, must be tasks, checklists or audits");
            }

        }
        #endregion
    }
}
