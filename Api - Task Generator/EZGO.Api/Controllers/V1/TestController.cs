using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZ.Connector.SAP.Interfaces;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using static Elastic.Apm.Config.ConfigConsts;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class TestController : BaseController<TestController>
    {
        #region - privates -
        private readonly IGeneralManager _generalManager;
        private readonly IDatabaseLogWriter _databaseLogWriter;
        private readonly IConfiguration _configuration;
        private readonly IApiConnectorAnalytics _apiConnectorAnalytics;
        #endregion

        #region - contructor(s) -
        public TestController(IGeneralManager generalManager, IConfigurationHelper configurationHelper, IApiConnectorAnalytics apiConnectorAnalytics, IConfiguration configuration, IDatabaseLogWriter databaseLogWriter, ILogger<TestController> logger, IApplicationUser applicationUser) : base(logger, applicationUser, configurationHelper)
        {
            _generalManager = generalManager;
            _databaseLogWriter = databaseLogWriter;
            _configuration = configuration;
            _apiConnectorAnalytics = apiConnectorAnalytics;
        }
        #endregion

        #region - GET routes -
        [AllowAnonymous]
        [Route("test/headers")]
        [HttpGet]
        public async Task<IActionResult> DoHEaders()
        {
            var headers = Request.Headers;
            var items = "";
            foreach (var item in headers)
            {
                items = items + string.Concat(item.Key, ":" , item.Value.ToString(), " | ");
            }

            await Task.CompletedTask;

            return StatusCode((int)HttpStatusCode.OK, GetAgentHeader().Contains("EZ-GO PORTAL").ToJsonFromObject());
        }

        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("test/config")]
        [HttpGet]
        public async Task<IActionResult> DoTestConfig()
        {
            var sb = new StringBuilder();
            await Task.CompletedTask;
            //foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            //{
            //    sb.AppendFormat("{0} : {1}", e.Key, e.Value != null ? e.Value.ToString().Length : "EMPTY" );
            //    sb.AppendLine("");
            //}

            //sb.AppendLine(_configurationHelper.GetValueAsString(AuthenticationSettings.PROTECTION_CONFIG_KEY).Substring(0, 4));
            //sb.AppendLine(_configurationHelper.GetValueAsString(AuthenticationSettings.SECURITY_TOKEN_CONFIG_KEY).Substring(0, 4));

            return StatusCode((int)HttpStatusCode.OK, sb.ToString());
        }




        [Route("test/speedtest")]
        [HttpGet]
        public async Task<IActionResult> DoSpeedTest()
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, "".ToJsonFromObject());
        }

        [AllowAnonymous]
        [Route("test/capture")]
        [HttpGet]
        public async Task<IActionResult> DoTestCapture()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            Agent.Tracer.CurrentSpan.SetLabel("TEST", "TEST");

            try
            {
                await _generalManager.DoTestCall(false);

                Agent.Tracer?.CurrentTransaction?.CaptureSpan("some logic", ApiConstants.ActionExec, async (s) =>
                {
                    await Task.Delay(2000);
                }, ApiConstants.SubtypeHttp, ApiConstants.ActionQuery);

            }
            catch (Exception ex)
            {
                Agent.Tracer?.CurrentTransaction?.CaptureException(ex);
            }

            Agent.Tracer.CurrentSpan.End();
            return StatusCode((int)HttpStatusCode.OK, "test".ToJsonFromObject());
        }


        [AllowAnonymous]
        [Route("test/capture_error")]
        [HttpGet]
        public async Task<IActionResult> DoTestCaptureError()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            Agent.Tracer.CurrentSpan.SetLabel("TEST ERROR", "TEST ERROR");

            try
            {
                await _generalManager.DoTestCall(true);

                Agent.Tracer?.CurrentTransaction?.CaptureSpan("some logic", ApiConstants.ActionExec, async (s) =>
                {
                    await Task.Delay(2000);
                    throw new ApplicationException("A application exception");
                }, ApiConstants.SubtypeHttp, ApiConstants.ActionQuery);

            }
            catch (Exception ex)
            {
                Agent.Tracer?.CurrentTransaction?.CaptureException(ex);
            }

            Agent.Tracer.CurrentSpan.End();
            return StatusCode((int)HttpStatusCode.OK, "test error".ToJsonFromObject());
        }

        [AllowAnonymous]
        [Route("test/check_param_manager")]
        [HttpGet]
        public async Task<IActionResult> CheckParamManager()
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            string output = _configurationHelper.GetValueAsString("TestKey");

            Agent.Tracer.CurrentSpan.End();

            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

       

        //[Route("test/sapconnector/action/simulate_receive")]
        //[HttpPost]
        //public async Task<IActionResult> SAPConnectorActionCheck([FromBody] object body)
        //{
        //    var headers = Request.Headers;
        //    var headerItems = "incoming headers -> ";
        //    foreach (var item in headers)
        //    {
        //        headerItems = headerItems + string.Concat(item.Key, ":", item.Value.ToString(), " | ");
        //    }
        //    var result = string.Concat(headerItems, "body payload raw no conversion -> ",body);

        //    _logger.LogDebug(message: string.Concat(result));

        //    await Task.CompletedTask;

        //    return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
        //}

        //[Route("test/sapconnector/action/simulate_post")]
        //[HttpPost]
        //public async Task<IActionResult> SAPConnectorActionSimulateAutomaticPost([FromBody] object body)
        //{
        //    var result = false;
        //    if (_configHelper.GetValueAsBool(Settings.Connectors.ENABLE_SAP_CONFIG_KEY))
        //    {
        //        //create dummy action based on payload of this controller route
        //        var action = body.ToString().ToObjectFromJson<ActionsAction>();

        //        //check if the SAP connector is enabled and the company is active for using the SAP connector.
        //        if (_SAPConnector.CheckCompanyForConnector(action.CompanyId))
        //        {

        //            //use company from action, and action object.
        //            result = await _SAPConnector.SendToSAPAsync(companyId: action.CompanyId, action: action);
        //        }
        //    }

        //    _logger.LogDebug(message: string.Concat("SAP connector payload delivery:", result));

        //    return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
        //}
        #endregion

        #region - POST routes -
        //move to own controller, and later move to own api project
        [Route("test/header-sync-test")]
        [HttpPost]
        public async Task<IActionResult> HeaderTest([FromBody] object data)
        {
            List<object> datalist = new List<object>();

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            try
            {
                datalist.Add(this.GetSyncGuidHeader());
                datalist.Add(this.GetUserGuidHeader());
                datalist.Add(data);

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToJsonFromObject());
            }

            Agent.Tracer.CurrentSpan.End();

            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, datalist.ToJsonFromObject());
        }
        #endregion
    }
}