using Elastic.Apm;
using Elastic.Apm.Api;
using EZ.Connector.Init.Interfaces;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Controllers.V1;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.General
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class AnalyticsController : BaseController<AnalyticsController>
    {
        #region - privates -
        private readonly IDatabaseLogWriter _databaseLogWriter;
        private readonly IApiConnectorAnalytics _apiConnectorAnalytics;
        #endregion

        #region - contructor(s) -
        public AnalyticsController(IApiConnectorAnalytics apiConnectorAnalytics, IDatabaseLogWriter databaseLogWriter, ILogger<AnalyticsController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _databaseLogWriter = databaseLogWriter;
            _apiConnectorAnalytics = apiConnectorAnalytics;
        }
        #endregion

        //move to own controller, and later move to own api project
        [Route("analytics/ingest")]
        [HttpPost]
        public async Task<IActionResult> IngestAnalytics([FromBody] List<object> data)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:AnalyticsEnabled"))
            {
                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

                try
                {
                    //Log to DB, normally only on test or acc enabled.
                    if (_configurationHelper.GetValueAsBool("AppSettings:AnalyticsDbEnabled"))
                    {
                        await _databaseLogWriter.WriteToLog(message: "Ingest es", "INFORMATION", eventid: "0", eventname: "ES_INGEST", description: data.ToJsonFromObject(), source: "ezgo.api");
                    }

                    if (_configurationHelper.GetValueAsBool("AppSettings:AnalyticsESEnabled")) {
                        var elasticUri = _configurationHelper.GetValueAsString("AppSettings:AnalyticsESBaseUri");
                        if (!string.IsNullOrEmpty(elasticUri))
                        {
                            var uri = "";
                            if (this.IsCmsRequest)
                            {
                                uri = "analytics/ingestcms";
                            }
                            else
                            {
                                uri = "analytics/ingestweb";
                            }

                            var response = await _apiConnectorAnalytics.PostCall(uri, data.ToJsonFromObject());
                            if (response != null && response.StatusCode != HttpStatusCode.OK)
                            {
                                await _databaseLogWriter.WriteToLog(message: "Ingest es", "ERROR", eventid: "0", eventname: "ES_INGEST", description: string.Concat(response.StatusCode, " | ", response.Message, " | ", data.ToJsonFromObject()), source: "ezgo.api");
                            }
                        }
                    }
                   
                }
                catch (Exception ex)
                {
                    await _databaseLogWriter.WriteToLog(message: "Ingest es", "ERROR", eventid: "0", eventname: "ES_INGEST", description: string.Concat(ex.Message.ToJsonFromObject(), " | ", data.ToJsonFromObject()), source: "ezgo.api");
                    return StatusCode((int)HttpStatusCode.OK, "false".ToJsonFromObject()); //return OK, but return false seeing it is not processed. Possible issues are logged. 
                }

                Agent.Tracer.CurrentSpan.End();

                return StatusCode((int)HttpStatusCode.OK, "true".ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.OK, "false".ToJsonFromObject());
            }

        }
    }
}
