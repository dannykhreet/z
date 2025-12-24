using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using EZGO.Api.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Logic.Generation;
using EZGO.Api.Logic.Helpers;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Reports;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Settings;
using EZGO.Api.Utils.Cache;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace EZGO.Api.Logic.Managers
{
    public class VirtualTeamLeadManager : BaseManager<VirtualTeamLeadManager>, IVirtualTeamLeadManager
    {
        #region - properties -
        private string culture;
        public string Culture
        {
            get { return culture; }
            set { culture = _tagManager.Culture = value; }
        }
        #endregion

        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IDataAuditing _dataAuditing;
        private readonly ITagManager _tagManager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IUserManager _userManager;
        private readonly IGeneralManager _generalManager;
        private readonly IUserDataManager _userDataManager;
        private IHttpContextAccessor _httpcontextaccessor;
        #endregion

        #region - constructor(s) -
        public VirtualTeamLeadManager(IDatabaseAccessHelper manager, IGeneralManager generalManager, ITagManager tagManager, IUserManager userManager, IHttpContextAccessor httpContextAccessor, IPropertyValueManager propertyValueManager, IWorkInstructionManager workInstructionManager, IActionManager actionManager, IAreaManager areaManager, ITaskGenerationManager taskGenerationManager, IDataAuditing dataAuditing, IConfigurationHelper configurationHelper, IUserAccessManager userAccessManager, ILogger<VirtualTeamLeadManager> logger, IMemoryCache memoryCache, IUserDataManager userdatamanager) : base(logger)
        {
            _manager = manager;
            _dataAuditing = dataAuditing;
            _configurationHelper = configurationHelper;
            _tagManager = tagManager;
            _userManager = userManager;
            _generalManager = generalManager;
            _userDataManager = userdatamanager;
            _httpcontextaccessor = httpContextAccessor;
        }
        #endregion

        public async Task<String> fetchShiftMessage()
        {
            string language = Culture;
            if (language == "" || language == null || language == String.Empty){language = "en_en";}
            var baseUrl = _configurationHelper.GetValueAsString("AppSettings:BaseUriSmartAPI");
            var url = $"{baseUrl}/api/shiftmessage/fetch?language={language}";

            string bearerToken = await getBearerToken();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("VirtualTeamLeadManager.fetchShiftMessage(): ", ex.Message));
            }

            return string.Empty;
        }

        public async Task<String> generateShiftMessage()
        {
            string language = Culture;
            if (language == "" || language == null || language == String.Empty) { language = "en_en"; }
            var baseUrl = _configurationHelper.GetValueAsString("AppSettings:BaseUriSmartAPI");
            var url = $"{baseUrl}/api/shiftmessage/generate?language={language}";
            string bearerToken = await getBearerToken();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("VirtualTeamLeadManager.generateShiftMessage(): ", ex.Message));
            }

            return string.Empty;
        }

        public async Task changeShift()
        {
            string bearerToken = await getBearerToken();
            var baseUrl = _configurationHelper.GetValueAsString("AppSettings:BaseUriSmartAPI");
            var url = $"{baseUrl}/api/shiftmessage/shiftchange";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("VirtualTeamLeadManager.changeShift(): ", ex.Message));
            }
        }

        public async Task<List<OptimizeData>> fetchOptimize(int week)
        {
            string bearerToken = await getBearerToken();
            List<OptimizeData> responseBody = new List<OptimizeData>();
            var baseUrl = _configurationHelper.GetValueAsString("AppSettings:BaseUriSmartAPI");
            var url = $"{baseUrl}/api/optimize/fetch?week={week}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    responseBody = await response.Content.ReadFromJsonAsync<List<OptimizeData>>();
                    return responseBody;
                }
                else
                {
                    return responseBody;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("VirtualTeamLeadManager.fetchOptimize(): ", ex.Message));
            }
            return responseBody; //Fix this about returning null, should be a 2D array with 0 values. 
        }

        public async Task<List<ReviewData>> fetchReview(int week)
        {
            string bearerToken = await getBearerToken();
            List<ReviewData> responseBody = new List<ReviewData>();
            var baseUrl = _configurationHelper.GetValueAsString("AppSettings:BaseUriSmartAPI");
            var url = $"{baseUrl}/api/review/fetch?week={week}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    responseBody = await response.Content.ReadFromJsonAsync<List<ReviewData>>();
                    return responseBody;
                }
                else
                {
                    return responseBody;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("VirtualTeamLeadManager.fetchReview(): ", ex.Message));
            }

            return responseBody;
        }

        private async Task<string> getBearerToken()
        {
            string bearerToken;
            StringValues currenttoken = string.Empty;
            var succes = _httpcontextaccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out currenttoken);

            await Task.CompletedTask;

            if (succes)
            {
                bearerToken = StringConverters.ConvertAuthorizationHeaderToToken(currenttoken.ToString());
            }
            else
            {
                _logger.LogWarning("VirtualTeamLeadManager.getBearerToken(): No token found. {0}", "Token not found returning empty string.");
                return string.Empty;
            }

            return bearerToken;
        }

        public async Task<Dictionary<string, int>> ComputePercentagesLandingPage(int companyId)
        {
            var now = DateTime.UtcNow;
            var pastWeekNumber = ISOWeek.GetWeekOfYear(now) - 1;
            var currentYear = ISOWeek.GetYear(now);

            // Handle week 0 (should be last week of previous year)
            if (pastWeekNumber < 1)
            {
                currentYear = ISOWeek.GetYear(now) - 1;
                pastWeekNumber = ISOWeek.GetWeeksInYear(currentYear);
            }

            var parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_company_id", companyId));
            parameters.Add(new NpgsqlParameter("@_week", pastWeekNumber));
            parameters.Add(new NpgsqlParameter("@_year", currentYear));

            var pastWeekTemplateIds = new List<int>();

            int predictedPastWeekCompletedTasksCount = 0;
            int predictedPastWeekSuccessfulTasksCount = 0;
            int predictedPastWeekNotSuccessfulTasksCount = 0;

            using (var reader = await _manager.GetDataReader("get_past_week_predicted_tasks", commandType: CommandType.StoredProcedure, parameters: parameters))
            {
                while (await reader.ReadAsync())
                {
                    pastWeekTemplateIds = reader.IsDBNull(reader.GetOrdinal("template_ids"))
                       ? new List<int>()
                       : reader.GetFieldValue<int[]>(reader.GetOrdinal("template_ids")).ToList();

                    predictedPastWeekCompletedTasksCount = reader.GetInt32(reader.GetOrdinal("completed_count"));
                    predictedPastWeekSuccessfulTasksCount = reader.GetInt32(reader.GetOrdinal("successful_count"));
                    predictedPastWeekNotSuccessfulTasksCount = reader.GetInt32(reader.GetOrdinal("not_successful_count"));
                }
            }

            var pastWeekMonday = ISOWeek.ToDateTime(currentYear, pastWeekNumber, DayOfWeek.Monday);
            var pastWeekSunday = pastWeekMonday.AddDays(6);

            _logger.LogInformation($"Past week range: {pastWeekMonday: yyyy-MM-dd} to {pastWeekSunday:yyyy-MM-dd}");
            _logger.LogInformation($"Template IDs: [{string.Join(", ", pastWeekTemplateIds)}]");


            var parametersPastWeek = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@_template_ids", NpgsqlDbType.Array | NpgsqlDbType.Integer)
                {
                    Value = pastWeekTemplateIds.ToArray()
                },
                new NpgsqlParameter("@_start_date", NpgsqlDbType.Date)
                {
                    Value = pastWeekMonday
                },
                new NpgsqlParameter("@_end_date", NpgsqlDbType.Date)
                {
                    Value = pastWeekSunday
                 }
            };

            int pastWeekCompletedTasksCount = 0;
            int pastWeekSuccessfulTasksCount = 0;
            int pastWeekNotSuccessfulTasksCount = 0;

                // different table
            using (var reader = await _manager.GetDataReader("get_past_week_actual_tasks", commandType: CommandType.StoredProcedure, parameters: parametersPastWeek))
            {
                if (await reader.ReadAsync())
                {
                    pastWeekCompletedTasksCount = Convert.ToInt32(reader.GetInt64(0));  // First column
                    pastWeekSuccessfulTasksCount = Convert.ToInt32(reader.GetInt64(1));  // Second column
                    pastWeekNotSuccessfulTasksCount = Convert.ToInt32(reader.GetInt64(2));  // Third column

                }
            }

            var numberOfPastTasks = pastWeekTemplateIds.Count();

            var percentagePredictedPastCompletedTasks = (predictedPastWeekCompletedTasksCount * 100) / numberOfPastTasks;
            var percentagePredictedPastSuccessfulTasks = (predictedPastWeekSuccessfulTasksCount * 100) / numberOfPastTasks;
            var percentagePredictedPastNotSuccessfulTasks = (predictedPastWeekNotSuccessfulTasksCount * 100) / numberOfPastTasks;

            var percentageCompletedTasks = (pastWeekCompletedTasksCount * 100) / numberOfPastTasks;
            var percentageSuccessfulTasks = (pastWeekSuccessfulTasksCount * 100) / numberOfPastTasks;
            var percentageNotSuccessfulTasks = (pastWeekNotSuccessfulTasksCount * 100) / numberOfPastTasks;

            var differenceTasksCompleted = percentageCompletedTasks - percentagePredictedPastCompletedTasks;
            // if negative say less tasks completed :)

            var differenceTasksSuccessful = percentageSuccessfulTasks - percentagePredictedPastSuccessfulTasks;
            // if negative say less tasks successful :)

            var differenceTasksNotSuccessful = percentageNotSuccessfulTasks - percentagePredictedPastNotSuccessfulTasks;
            // if negative say less tasks not successful :)

            int predictedNextWeekCompletedTasksCount = 0;
            int predictedNextWeekSuccessfulTasksCount = 0;
            int predictedNextWeekNotSuccessfulTasksCount = 0;

            var weeksInCurrentYear = ISOWeek.GetWeeksInYear(currentYear);
            var nextWeekNumber = pastWeekNumber + 2;
            var nextYear = currentYear;

            if (pastWeekNumber + 2 > weeksInCurrentYear)
            {
                nextYear = currentYear + 1;
                nextWeekNumber = 1;
            }

            var parametersNextWeek = new List<NpgsqlParameter>();
            parametersNextWeek.Add(new NpgsqlParameter("@_company_id", companyId));
            parametersNextWeek.Add(new NpgsqlParameter("@_week", nextWeekNumber));
            parametersNextWeek.Add(new NpgsqlParameter("@_year", nextYear));

            var templateIds = new List<int>();

            using (var reader = await _manager.GetDataReader("get_past_week_predicted_tasks", commandType: CommandType.StoredProcedure, parameters: parametersNextWeek))
            {
                while (await reader.ReadAsync())
                {
                    templateIds = reader.IsDBNull(reader.GetOrdinal("template_ids"))
                        ? new List<int>()
                        : reader.GetFieldValue<int[]>(reader.GetOrdinal("template_ids")).ToList();

                    predictedNextWeekCompletedTasksCount = reader.GetInt32(reader.GetOrdinal("completed_count"));
                    predictedNextWeekSuccessfulTasksCount = reader.GetInt32(reader.GetOrdinal("successful_count"));
                    predictedNextWeekNotSuccessfulTasksCount = reader.GetInt32(reader.GetOrdinal("not_successful_count"));
                } 
            }

            var numberOfNextTasks = templateIds.Count();

            var percentagePredictedNextCompletedTasks = (predictedNextWeekCompletedTasksCount * 100) / numberOfNextTasks;
            var percentagePredictedNextSuccessfulTasks = (predictedNextWeekSuccessfulTasksCount * 100) / numberOfNextTasks;
            var percentagePredictedNextNotSuccessfulTasks = (predictedNextWeekNotSuccessfulTasksCount * 100) / numberOfNextTasks;

            var result = new Dictionary<string, int> {
                { "companyId" , companyId },
                { "differenceTasksCompleted" , differenceTasksCompleted },
                { "differenceTasksSuccessful" , differenceTasksSuccessful },
                { "differenceTasksNotSuccessful" , differenceTasksNotSuccessful },
                { "percentagePredictedNextCompletedTasks" , percentagePredictedNextCompletedTasks },
                { "percentagePredictedNextSuccessfulTasks" , percentagePredictedNextSuccessfulTasks },
                { "percentagePredictedNextNotSuccessfulTasks" , percentagePredictedNextNotSuccessfulTasks }
            };

            return result;
        }
    }
}
