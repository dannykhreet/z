using Elastic.Apm.Api;
using Elastic.Apm;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Utils.Json;
using EZGO.Api.Models.Logs;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class LoggingController : BaseController<LoggingController>
    {
        #region - privates -
        private readonly IDatabaseLogWriter _databaseLogWriter;
        #endregion

        #region - contructor(s) -
        public LoggingController(IDatabaseLogWriter databaseLogWriter, ILogger<LoggingController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _databaseLogWriter = databaseLogWriter;
        }
        #endregion

        /// <summary>
        /// LogAnomaly; Log possible anomalies that occur within the client applications. 
        /// These will include, possible connection anomalies, local validation issues etc.
        /// </summary>
        /// <param name="data">List of objects, preferable: List of anomaly objects <see cref="LogAnomaly"/>. List of string can be posted and differently handles, route will more or less parse anything as long as it is a list of 'something'</param>
        /// <returns>true/false depending on outcome, can be ignored by client.</returns>
        /// <response code="200">true/false, can be ignored by client.</response>
        /// <response code="401">No rights to use this controller action route.</response>
        [Route("logging/anomaly")]
        [HttpPost]
        public async Task<IActionResult> LogAnomalyData([FromBody] object data)
        {
            if (_configurationHelper.GetValueAsBool("AppSettings:AnomalyLoggingEnabled"))
            {
                List<object> possibleAnomalies = new List<object>();
                List<LogAnomaly> anomalies = new List<LogAnomaly>();

                Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                     //TODO add parser to anomaly data

                     //TODO replace by own table
                     await _databaseLogWriter.WriteToLog(message: "Possible anomaly found", "INFORMATION", eventid: "0", eventname: "LOG_ANOMALY", description: data.ToJsonFromObject(), source: "ezgo.api");
                }
                catch (Exception ex)
                {
                   
                    return StatusCode((int)HttpStatusCode.OK, "false".ToJsonFromObject()); //return OK, but return false seeing it is not processed. Possible issues are logged. 
                }
#pragma warning restore CS0168 // Variable is declared but never used
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
