using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.SapPmConnector;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.V1
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class SapPmController : BaseController<SapPmController>
    {
        #region - variables -
        private readonly ISapPmManager _manager;
        private readonly ISapPmConnectionManager _sapPmConnectionManager;
        #endregion

        #region - constructors -
        public SapPmController(ISapPmManager manager, ISapPmConnectionManager sapPmConnectionManager, ILogger<SapPmController> logger, IApplicationUser applicationuser, IConfigurationHelper configurationHelper) : base(logger, applicationuser, configurationHelper)
        {
            _manager = manager;
            _sapPmConnectionManager = sapPmConnectionManager;
        }
        #endregion

        #region - GET routes methods  for Frontend/CMS-
        [Route("locations/search")]
        [HttpGet]
        public async Task<IActionResult> SearchLocationsAsync([FromQuery] string searchtext, [FromQuery] int? functionallocationid)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            searchtext = TextValidator.StripRogueNewLineDataFromText(searchtext);

            var result = await _manager.SearchLocationsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), searchtext, functionallocationid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("locations/children")]
        [HttpGet]
        public async Task<IActionResult> GetChildrenOfLocation([FromQuery] int? functionallocationid)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetLocationChildren(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), functionallocationid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("notifications/options")]
        [HttpGet]
        public async Task<IActionResult> GetNotificationOptions([FromQuery] int? areaid = null)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _manager.GetSapPmNotificationOptionsAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), areaId:areaid);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("location/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetFunctionalLocationById([FromRoute] int id)
        {
            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            if (!SapPmValidators.LocationIdIsValid(id))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, SapPmValidators.MESSAGE_LOCATION_ID_IS_NOT_VALID.ToJsonFromObject());
            }

            var result = await _manager.GetSapPmFunctionalLocationAsync(companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), functionalLocationId: id);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }
        #endregion

        /// <summary>
        /// Clears functional locations, and all related data, for the specified companies.
        /// </summary>
        /// <remarks>This method requires the caller to have appropriate permissions to clear functional
        /// locations. The operation is restricted to users associated with the management company as defined in the
        /// application configuration.</remarks>
        /// <param name="companyIds">A comma-separated list of company IDs for which functional locations should be cleared.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns: <list type="bullet">
        /// <item><description><see cref="OkObjectResult"/> with the number of cleared locations if the operation is
        /// successful.</description></item> <item><description><see cref="StatusCodeResult"/> with <see
        /// cref="HttpStatusCode.BadRequest"/> if no functional locations are found for the specified
        /// companies.</description></item> <item><description><see cref="StatusCodeResult"/> with <see
        /// cref="HttpStatusCode.Forbidden"/> if the current user does not have permission to perform the
        /// operation.</description></item> </list></returns>
        #region - Routes for Imports -
        [Route("import/clearlocations")]
        [HttpPost]
        public async Task<IActionResult> ImportClearFunctionalLocations([FromQuery] string companyIds)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var result = (int)await _manager.ClearFunctionalLocationsInDatabase(companyIds);
            if (result > 0)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"No Functional locations found for companies {companyIds}.");
            }
        }

        /// <summary>
        /// Imports functional locations from a CSV file and processes the data for the specified companies.
        /// </summary>
        /// <remarks>The method performs the following steps: <list type="number">
        /// <item><description>Validates the user's permissions to perform the import.</description></item>
        /// <item><description>Validates the file format and size.</description></item> <item><description>Parses the
        /// CSV file, removes duplicate entries, and converts the data into models.</description></item>
        /// <item><description>Imports the functional locations into the database for the specified
        /// companies.</description></item> </list> If the import fails, an appropriate error message is
        /// returned.</remarks>
        /// <param name="file">The CSV file containing functional location data. The file must be in a supported CSV format.</param>
        /// <param name="companyIds">A comma-separated list of company IDs for which the functional locations should be imported.</param>
        /// <param name="recalculateTreeStructure">A boolean value indicating whether the tree structure of the functional locations should be recalculated
        /// after import. The default value is <see langword="true"/>.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns: <list type="bullet">
        /// <item><description><see cref="OkObjectResult"/> with the number of imported records if the operation
        /// succeeds.</description></item> <item><description><see cref="StatusCodeResult"/> with <see
        /// cref="HttpStatusCode.BadRequest"/> if the file is invalid, too large, or contains no functional
        /// locations.</description></item> <item><description><see cref="StatusCodeResult"/> with <see
        /// cref="HttpStatusCode.Forbidden"/> if the user does not have permission to perform the
        /// import.</description></item> </list></returns>
        [Route("import/locations")]
        [HttpPost]
        public async Task<IActionResult> ImportFunctionalLocations(IFormFile file, [FromQuery] string companyIds, [FromQuery] bool recalculateTreeStructure = true)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (!FileValidator.CheckCsvFormat(file.FileName))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_IS_NOT_A_SUPPORTED_CSV.ToJsonFromObject());
            }

            //if larger than 250MB, skip processing
            if (!FileValidator.CheckCsvFilesize(file.Length))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_SIZE_IS_TOO_LARGE);
            }

            //load csv
            //parse csv
            //convert to models
            var functionalLocations = await _manager.GetLocationImportDataForCsv(file.OpenReadStream());

            //remove duplicates, but still attempt to import
            var importData = functionalLocations.DistinctBy(d => d.FunctionalLocation).ToList();

            if (importData != null && importData.Count > 0)
            {
                //get json
                var locationsJson = importData.ToJsonFromObject();

                var jsonToImport = "{ \"data\":" + locationsJson + " }";

                var result = (int)await _manager.ImportFunctionalLocationsInDatabase(sapFunctionalLocations: jsonToImport, companyIds: companyIds, recalculateTreeStructure: recalculateTreeStructure);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, $"Functional locations found ({importData.Count}), but failed to import.");
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "No functional locations found in import csv.");
            }
        }

        /// <summary>
        /// Recalculates the functional locations tree structure for the specified companies.
        /// </summary>
        /// <remarks>This operation requires the user to have access to the specified companies. If any
        /// company ID in the input is invalid or cannot be parsed as an integer, a warning is logged, and that ID is
        /// skipped.</remarks>
        /// <param name="companyIds">A comma-separated list of company IDs for which the tree structure should be recalculated. Each ID must be a
        /// valid integer.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns: <list type="bullet"> <item>
        /// <description><see cref="OkObjectResult"/> with the total number of recalculated tree structures if
        /// successful.</description> </item> <item> <description><see cref="StatusCodeResult"/> with <see
        /// cref="HttpStatusCode.Forbidden"/> if the user does not have permission to perform the
        /// operation.</description> </item> <item> <description><see cref="StatusCodeResult"/> with <see
        /// cref="HttpStatusCode.BadRequest"/> if no functional locations are found for the specified
        /// companies.</description> </item> </list></returns>
        [Route("import/recalculatetreestructure")]
        [HttpPost]
        public async Task<IActionResult> RecalculateTreeStructure([FromQuery] string companyIds)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            int result = 0;

            foreach(string cId in companyIds.Split(',').ToList())
            {
                if (int.TryParse(cId, out int companyId))
                {
                    result += await _manager.RegenerateFunctionalLocationsTreeStructure(companyId);
                }
                else
                {
                    _logger.LogWarning($"Could not parse companyId {cId} to int when recalculating functional locations tree structure.");
                }
            }

            if (result > 0)
            {
                return Ok(result);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, $"No Functional locations found for companies {companyIds}.");
            }
        }
        #endregion

        #region - Routes for Settings and manuals synchs -

        [Route("settings/setsappmcredentials")]
        [HttpPost]
        public async Task<IActionResult> SetSapPmCredentials([FromQuery] int companyid, [FromQuery] int? holdingid, [FromBody] Login sappmcredentials)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }


            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            if (sappmcredentials == null || string.IsNullOrEmpty(sappmcredentials.UserName) || string.IsNullOrEmpty(sappmcredentials.Password))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid SAP PM credentials provided.".ToJsonFromObject());
            }

            bool result = await _manager.SetSapPmCredentialsAsync(userId: await this.CurrentApplicationUser.GetAndSetUserIdAsync(), companyId: companyid, holdingId: holdingid, sapPmCredentials: sappmcredentials);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("settings/synchlocations")]
        [HttpGet]
        public async Task<IActionResult> SynchFunctionalLocations([FromQuery] string companies = null)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _sapPmConnectionManager.SynchFunctionalLocations(companies);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }

        [Route("settings/sendNotifications")]
        [HttpGet]
        public async Task<IActionResult> SendNotifications([FromQuery] string companies = null)
        {
            if (await this.CurrentApplicationUser.GetAndSetCompanyIdAsync() != _configurationHelper.GetValueAsInteger(ApiSettings.MANAGEMENT_COMPANY_ID_CONFIG))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);

            var result = await _sapPmConnectionManager.SendNotificationMessagesToSapPM(companies);

            AppendCapturedExceptionToApm(_manager.GetPossibleExceptions());

            Agent.Tracer.CurrentSpan.End();

            return GetObjectResultJsonWithStatus(result);
        }
        #endregion
    }
}
