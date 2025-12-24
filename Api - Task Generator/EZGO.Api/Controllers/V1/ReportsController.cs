using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class ReportsController : BaseController<ReportsController>
    {
        private readonly IReportManager _manager;

        public ReportsController(IReportManager manager, ILogger<ReportsController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _manager = manager;
        }

        [Route("reporting/tasks")]
        [HttpGet]
        public async Task<IActionResult> GetTaskReports()
        {
            await Task.CompletedTask;
            var result = "test"; //await _manager.GetShiftsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("reporting/audits")]
        [HttpGet]
        public async Task<IActionResult> GetAuditReports()
        {
            //concatted report calls for cms
            await Task.CompletedTask;
            var result = "test"; //await _manager.GetShiftsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("reporting/checklists")]
        [HttpGet]
        public async Task<IActionResult> GetChecklistReports()
        {
            //concatted report calls for cms
            await Task.CompletedTask;
            var result = "test"; //await _manager.GetShiftsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("reporting/actions")]
        [HttpGet]
        public async Task<IActionResult> GetActionReports()
        {
            //concatted report calls for cms
            await Task.CompletedTask;
            var result = "test"; //await _manager.GetShiftsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Route("reporting/taskoverdue")]
        [HttpGet]
        public async Task<IActionResult> GetOverdueTasks([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetOverdueTaskReportsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, areaid:areaid, allowedOnly: allowedonly);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        [Route("reporting/taskscurrentoverview")]
        [HttpGet]
        public async Task<IActionResult> GetCurrentTasksOverview([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetCurrentTaskOverviewReportAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, areaid: areaid, allowedOnly: allowedonly);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("reporting/tasksoverview")]
        [HttpGet]
        public async Task<IActionResult> GetTasksOverview([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTaskOverviewReportAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, areaid: areaid, allowedOnly: allowedonly);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        [Route("reporting/taskspastoverview")]
        [HttpGet]
        public async Task<IActionResult> GetTaskPastOverview([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPastTaskOverviewReportAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, areaid: areaid, allowedOnly: allowedonly);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("reporting/taskspastoverview/previousshift")]
        [HttpGet]
        public async Task<IActionResult> GetTaskPastOverviewPreviousShift([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPastTaskOverviewReportPreviousShiftAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, areaid: areaid, allowedOnly: allowedonly);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("reporting/taskspastoverview/lastweek")]
        [HttpGet]
        public async Task<IActionResult> GetTaskPastOverviewLastWeek([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPastTaskOverviewReportLastWeekAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, areaid: areaid, allowedOnly: allowedonly);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("reporting/taskspastoverview/yesterday")]
        [HttpGet]
        public async Task<IActionResult> GetTaskPastOverviewYesterday([FromQuery] string timestamp, [FromQuery] int? areaid, [FromQuery] bool? allowedonly = null)
        {
            DateTime parsedTimeStamp;
            if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetPastTaskOverviewReportYesterdayAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), timestamp: parsedTimeStamp, areaid: areaid, allowedOnly: allowedonly);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }


        [Route("reporting/deviations/audits")]
        [HttpGet]
        public async Task<IActionResult> GetTopxAudits([FromQuery] int? areaid = null, [FromQuery] int? audittemplateid = null, [FromQuery] int? tasktemplateid = null, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] bool? allowedonly = null)
        {
            await Task.CompletedTask;

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetAuditsDeviationReportAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), auditTemplateId: audittemplateid, taskTemplateId: tasktemplateid, areaId: areaid, timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("reporting/deviations/checklists")]
        [HttpGet]
        public async Task<IActionResult> GetTopxChecklists([FromQuery] int? areaid = null, [FromQuery] int? checklisttemplateid = null, [FromQuery] int? tasktemplateid = null, [FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] bool? allowedonly = null)
        {
            await Task.CompletedTask;

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetChecklistDeviationReportAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), checklistTemplateId: checklisttemplateid, taskTemplateId: tasktemplateid, areaId: areaid, timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("reporting/deviations/tasks")]
        [HttpGet]
        public async Task<IActionResult> GetTopxTasks([FromQuery] int? areaid = null, [FromQuery] int? tasktemplateid = null,[FromQuery] TimespanTypeEnum? timespantype = null, [FromQuery] bool? allowedonly = null)
        {
            await Task.CompletedTask;

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetTasksDeviationReportAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), areaId: areaid, timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
    }
}