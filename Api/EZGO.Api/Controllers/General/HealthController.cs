using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EZGO.Api.Controllers.Base;
//using EZGO.Api.Interfaces.Raw;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Settings;
using Microsoft.AspNetCore.Cors;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Utils.Security;
using EZGO.Api.Interfaces.Raw;

namespace EZGO.Api.Controllers.General
{
    /// <summary>
    /// HealthController; health controller calls different functionality to determine basic availability of the API
    /// </summary>
    [Route("")]
    [Route("health")]
    [ApiController]
    public class HealthController : BaseController<HealthController>
    {
        #region - properties -
        private readonly IDataCheckManager _dataManager;
        private readonly IApplicationUser _applicationUser;
        private readonly IDatabaseLogWriter _databaseLogWriter;
        #endregion

        #region - constructor(s) -
        public HealthController(ILogger<HealthController> logger, IDataCheckManager dataManager, IDatabaseLogWriter databaseLogWriter, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _dataManager = dataManager;
            _applicationUser = applicationUser;
            _databaseLogWriter = databaseLogWriter;
        }
        #endregion

        #region - routes -

        /// <summary>
        /// Default route, when working will return true.
        /// </summary>
        /// <returns>true/or standard http error.</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetItWorksCheck()
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject());
        }

        /// <summary>
        /// GetDatabaseUpAndRunning; checks if the db is available based on a query through entity framework. When there are results the basics works as it should
        /// </summary>
        /// <returns>true/false</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("db")]
        public async Task<IActionResult> GetDatabaseUpAndRunning()
        {
            try
            {
                return StatusCode((int)HttpStatusCode.OK, await _dataManager.GetCompanies());

            } catch (Exception ex)
            {
                base._logger.LogError(exception: ex, message: "HealthController.GetDatabaseUpAndRunning()");
                return StatusCode((int)HttpStatusCode.NotFound, false.ToJsonFromObject());
            }
        }

        /// <summary>
        /// GetUserConnection; checks if a prepared user can be validated through its company id, if so the db and basic authentication work based on request token header.
        /// </summary>
        /// <returns>true/false</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("userconnection")]
        public async Task<IActionResult> GetUserConnection()
        {
            try
            {
                return StatusCode((int)HttpStatusCode.OK, ((await _applicationUser.GetAndSetCompanyIdAsync()) > 0));
            }
            catch (Exception ex)
            {
                base._logger.LogError(exception: ex, message: "HealthController.GetUserConnection()");
                return StatusCode((int)HttpStatusCode.NotFound, false.ToJsonFromObject()); ;
            }

        }

        /// <summary>
        /// GetVersion; get version of application assembly.
        /// Version: [major].[minor].[yyMM].[ddHH(mm rounded)]
        /// e.g. when it's 21th of march 2020 15:33
        /// The minor version = 1
        /// The major version = 2
        /// The total version will be 2.1.2003.21153
        /// </summary>
        /// <returns>Active version number</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("version")]
        public async Task<IActionResult> GetVersion()
        {
            if (!CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, GetType().Assembly.GetName().Version.ToString());

        }

        /// <summary>
        /// GetLogicVersions; Get all versions of the logic parts of the EZGO api.
        /// In normal conditions these would be the same as the API version.
        /// </summary>
        /// <returns>String containing all EZGO logic (dlls) with their versions.</returns>
        [Authorize(Roles = AuthenticationSettings.AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpGet]
        [Route("logicversions")]
        public async Task<IActionResult> GetLogicVersions()
        {
            if (!CheckIfIPHasAccess(_configurationHelper.GetValueAsString("AppSettings:ValidIpForValidation")))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized.");
            }
            StringBuilder sb = new StringBuilder();
            foreach (AssemblyName an in GetType().Assembly.GetReferencedAssemblies().OrderBy(x => x.Name))
            {
                if(an.Name.StartsWith("EZGO"))
                {
                    sb.AppendFormat("{0} | version:{1} \r\n", an.Name, an.Version);
                }
            }
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, sb.ToString());

        }

        /// <summary>
        /// GetAuthorization; Checks Authorization, should return 401 when not supplying a bearer token.
        /// </summary>
        /// <returns>Returns string with Ok when authorized.</returns>
        [HttpGet]
        [Route("authorization")]
        public async Task<IActionResult> GetAuthorization()
        {
            await Task.CompletedTask;
            var foundClaim = User.GetClaim (ClaimTypes.Sid);
            if(foundClaim != null && !string.IsNullOrEmpty(foundClaim))
            {
                _logger.LogInformation("Authorization test success!");
            } else
            {
                _logger.LogWarning("Authorization test failure!");
            }
            return StatusCode((int)HttpStatusCode.OK, "Ok");
        }

        /// <summary>
        /// GetEnvironmentDb; Gets the current running environment (based on db connection parts)
        /// </summary>
        /// <returns>A string containing the technical name of the current environment.</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("environmentdb")]
        public async Task<IActionResult> GetEnvironmentDb()
        {
            var output = _dataManager.GetEnvironment();
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        /// <summary>
        /// GetEnvironment; Gets the current running environment (based on config)
        /// </summary>
        /// <returns>A string containing the technical name of the current environment.</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("environment")]
        public async Task<IActionResult> GetEnvironment()
        {
            var output = _configurationHelper.GetValueAsString(ApiSettings.ENVIRONMENT_CONFIG_KEY);
            if(string.IsNullOrEmpty(output))
            {
                output = "unknown";
            }
           await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, output.ToUpper().ToJsonFromObject());
        }

        /// <summary>
        /// GetEnvironmentVariables; Gets the API_ENVIRONMENTAL_VARIABLES_ACTIVE variable from the environmental variables settings.
        /// </summary>
        /// <returns>Should return true. If not available will return false or generate an error..</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("environmentalvariables")]
        public async Task<IActionResult> GetEnvironmentVariables()
        {
            var output = Environment.GetEnvironmentVariable("API_ENVIRONMENTAL_VARIABLES_ACTIVE");
            if (string.IsNullOrEmpty(output))
            {
                output = "false";
            }
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, output.ToJsonFromObject());
        }

        /// <summary>
        /// GetApiTime(); Gets the API time.
        /// </summary>
        /// <returns>DateTime.Now</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("apitime")]
        public async Task<IActionResult> GetApiTime()
        {
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, DateTime.Now.ToJsonFromObject());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("checkwrite")]
        public async Task<IActionResult> GetCheckWriter()
        {
            var id = await _databaseLogWriter.WriteToLog("HEALTH CHECK", "INFORMATION", "0", "HEALTHCHECK", "check health write action", "API");
            return StatusCode((int)HttpStatusCode.OK, (id > 0).ToJsonFromObject());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("checkread")]
        public async Task<IActionResult> GetCheckRead()
        {
            var id = await _databaseLogWriter.GetLatestLogId();
            if (id > 0)
            {
                return StatusCode((int)HttpStatusCode.OK, (id > 0).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, (id > 0).ToJsonFromObject());
            }

        }

        #endregion

    }
}