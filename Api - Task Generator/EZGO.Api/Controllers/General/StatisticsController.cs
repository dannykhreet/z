using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Raw;
using EZGO.Api.Interfaces.Reporting;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Stats;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.General
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class StatisticsController :  BaseController<StatisticsController>
    {
        private readonly IStatisticsManager _statsManager;
        private readonly IDataCheckManager _dataCheckManager;

        #region - constructor(s) -
        public StatisticsController(ILogger<StatisticsController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper, IStatisticsManager statsManager, IDataCheckManager dataCheckManager) : base(logger, applicationUser, configurationHelper)
        {
            _statsManager = statsManager;
            _dataCheckManager = dataCheckManager;
        }
        #endregion

        [HttpGet]
        [Route("reporting/statistics/companyoverview")]
        public async Task<IActionResult> GetCompanyOverview()
        {

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _statsManager.GetTotalsOverviewByCompanyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/useractivitytotals")]
        public async Task<IActionResult> GetUserActivityTotals()
        {

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _statsManager.GetUserActivityTotalsByCompanyAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync());

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/generic/{statisticreference}")]
        public async Task<IActionResult> GetGenericStatistics([FromRoute] string statisticreference, [FromQuery] string timestamp, [FromQuery] int? areaid = null, [FromQuery] int? audittemplateid = null, [FromQuery] int? checklisttemplateid = null, [FromQuery] int? tasktemplateid = null, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            if(Settings.StatisticSettings.StatisticReferences.Contains(statisticreference))
            {
                DateTime parsedTimeStamp = new DateTime();
                if(!string.IsNullOrEmpty(timestamp))
                {
                    if (DateTime.TryParseExact(timestamp, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTimeStamp)) { };
                }

                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                //NOTE! this route uses a statistic reference (which refers to a database function, this will always output the same structure (list of items) but depending on the reference this will include different data
                var output = await _statsManager.GetGenericStatisticsCollectionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(),
                                                                                     storedProcedureReference: statisticreference,
                                                                                     parsedTimeStamp != DateTime.MinValue ?  parsedTimeStamp : new Nullable<DateTime>(),
                                                                                     areaId: areaid,
                                                                                     auditTemplateId: audittemplateid,
                                                                                     checklistTemplateId: checklisttemplateid,
                                                                                     taskTemplateId: tasktemplateid,
                                                                                     timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

                AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Statistics Reference not found or not available".ToJsonFromObject());
            }

        }

        [HttpGet]
        [Route("reporting/statistics/average/{statisticreference}")]
        public async Task<IActionResult> GetAverageStatistics([FromRoute] string statisticreference, [FromQuery] int? areaid = null, [FromQuery] int? audittemplateid = null, [FromQuery] int? checklisttemplateid = null, [FromQuery] int? tasktemplateid = null,[FromQuery] TimespanTypeEnum? timespantype = null)
        {
            if (Settings.StatisticSettings.StatisticReferences.Contains(statisticreference))
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                //NOTE! this route uses a statistic reference (which refers to a database function, this will always output the same structure (list of items) but depending on the reference this will include different data
                var output = await _statsManager.GetAverageStatisticsCollectionAsync (companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), storedProcedureReference: statisticreference, areaId: areaid, auditTemplateId: audittemplateid, checklistTemplateId: checklisttemplateid, taskTemplateId: tasktemplateid, timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

                AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Statistics Reference not found or not available".ToJsonFromObject());
            }

        }

        [HttpGet]
        [Route("reporting/statistics/my/{statisticreference}")]
        [Route("reporting/statistics/my")]
        public async Task<IActionResult> GetMyStatics([FromRoute]string statisticreference, [FromQuery] int? areaid = null, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            List<StatisticGenericItem> output;
            if (string.IsNullOrEmpty(statisticreference))
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                output = await _statsManager.GetMyStatisticsCollectionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId: areaid, timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

                AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
            }
            else {
                statisticreference = string.Concat("my_", statisticreference);
                if (Settings.StatisticSettings.StatisticReferences.Contains(statisticreference))
                {
                    Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                    //NOTE! this route uses a statistic reference (which refers to a database function, this will always output the same structure (list of items) but depending on the reference this will include different data
                    output = await _statsManager.GetMyStatisticsCollectionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), storedProcedureReference: statisticreference, areaId: areaid, timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

                    AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                    Agent.Tracer.CurrentSpan.End();

                    return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
                }
            }

            return StatusCode((int)HttpStatusCode.BadRequest, "Statistics Reference not found or not available".ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/my/ezfeed")]
        public async Task<IActionResult> GetMyEZFeedStatistics()
        {
            List<StatisticGenericItem> output;
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            output = await _statsManager.GetMyEZFeedStatisticsCollectionAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync());

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/count/actions")]
        public async Task<IActionResult> GetCountActions([FromQuery] int? areaid = null, [FromQuery] TimespanTypeEnum? timespantype = null)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var output = await _statsManager.GetActionCountStatistics(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), areaId:areaid, timespanInDays: timespantype.HasValue ? timespantype.Value.ToDays() : new Nullable<int>());

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        [HttpGet]
        [Route("reporting/statistics/logging/requestcounts")]
        public async Task<IActionResult> GetLoggingRequestCounts()
        {
            var appEnvironment = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            var dbEnvironment = _dataCheckManager.GetEnvironment();
            //check if environments can be used for getting log information.
            //Only when the app is in development or test and the database connections are on development or test logging may be retrieved.
            if ((new[] { "development", "localdevelopment", "test" }).Contains(appEnvironment.ToLower())
             && (new[] { "DEVELOPMENT", "TESTING PRODUCTION", "TESTING", "LOCAL", "UNKNOWN" }).Contains(dbEnvironment.ToUpper()))
            {
                var possibleUserForAccess = _configurationHelper.GetValueAsString(ApiSettings.ENABLE_LOG_READ_FOR_USER_CONFIG_KEY);
                if (!string.IsNullOrEmpty(possibleUserForAccess))
                {
                    if (await this.CurrentApplicationUser.GetAndSetUserIdAsync() == Convert.ToInt32(possibleUserForAccess))
                    {
                        //NOTE! this route is only available for certain users and on certain environments
                        var output = await _statsManager.GetLoggingRequestStatisticsCollectionAsync();

                        AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                        return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
                    }

                }
            };

            return StatusCode((int)HttpStatusCode.NotFound, "".ToJsonFromObject());

        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("reporting/statisticstotals")]
        public async Task<IActionResult> GetStatisticTotals()
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = await _statsManager.GetTotalStatisticsAsync();

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        }


        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("reporting/companystatistics")]
        public async Task<IActionResult> GetCompanyStatistics(string starttime, string endtime)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            DateTime parsedstarttimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(starttime) && DateTime.TryParseExact(starttime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedstarttimestamp)) { };

            DateTime parsedendtimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(endtime) && DateTime.TryParseExact(endtime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedendtimestamp)) { };

            var output = await _statsManager.GetCompanyReports(parsedstarttimestamp, parsedendtimestamp);

            AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

        }

        #region - datawarehouse statistics -
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("statistics/datawarehouse/{holdingid}/{companyid}/{statreference}")]
        public async Task<IActionResult> GetStatsDataDW([FromRoute] string statreference, [FromRoute] int holdingid, [FromRoute] int companyid, [FromQuery] string starttimestamp, [FromQuery] string endtimestamp)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, CompanyValidators.MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS.ToJsonFromObject());
            }

            if (Settings.StatisticSettings.StatisticReferencesDatawarehouse.Contains(statreference))
            {

                DateTime parsedStartTimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(starttimestamp) && DateTime.TryParseExact(starttimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimestamp)) { }
                ;

                DateTime parsedEndTimestamp = DateTime.MinValue;
                if (!string.IsNullOrEmpty(endtimestamp) && DateTime.TryParseExact(endtimestamp, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimestamp)) { }
                ;

                var output  = await _statsManager.GetStatisticsDataWarehouse(holdingId: holdingid, companyId: companyid, statsReference: statreference, startDateTime: parsedStartTimestamp, endDateTime: parsedEndTimestamp);

                AppendCapturedExceptionToApm(_statsManager.GetPossibleExceptions());

                return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());

            }

            return StatusCode((int)HttpStatusCode.OK, "".ToJsonFromObject());
        }
        #endregion


    }
}
