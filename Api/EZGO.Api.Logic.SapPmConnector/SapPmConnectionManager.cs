using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml.Spreadsheet;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.SapPmConnector;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.SapPmConnector.Base;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using static QRCoder.PayloadGenerator;

namespace EZGO.Api.Logic.SapPmConnector
{
    public class SapPmConnectionManager : BaseManager<SapPmConnectionManager>, ISapPmConnectionManager
    {
        #region Fields
        private IGeneralManager _generalManager;
        private ISapPmProcessingManager _sapPmProcessingManager;
        private ISapPmManager _sapPmManager;
        private IConfigurationHelper _configurationHelper;
        #endregion

        #region Constructors
        public SapPmConnectionManager(ILogger<SapPmConnectionManager> logger, IGeneralManager generalManager, ISapPmManager sapPmManager, ISapPmProcessingManager sapPmProcessingManager, IConfigurationHelper configurationHelper) : base(logger)
        {
            _generalManager = generalManager;
            _sapPmProcessingManager = sapPmProcessingManager;
            _sapPmManager = sapPmManager;
            _configurationHelper = configurationHelper;
        }
        #endregion

        #region public methods
        public async Task<int> SynchFunctionalLocations(String companiesList = "")
        {
            int result = 0;

            //if no companies are provided, use all companies that have the Market SAP setting enabled
            if (String.IsNullOrEmpty(companiesList))
            {
                companiesList = (await _generalManager.GetSettingResourceByKey("MARKET_SAP")).Value;
            }

            List<int> companies = companiesList.Split(',').Select(Int32.Parse).ToList();

            foreach (var company in companies)
            {
                result += await SynchFunctionalLocationsForCompany(company);
            }

            return result;
        }

        public async Task<int> SendNotificationMessagesToSapPM(String companiesList = "")
        {
            int result = 0;
            List<SapPmNotificationMessage> messages = new List<SapPmNotificationMessage>();
            if (!String.IsNullOrEmpty(companiesList))
            {
                foreach(int company in companiesList.Split(',').Select(Int32.Parse).ToList()) { 
                    messages.AddRange(await _sapPmManager.GetSapPmNotificationMessagesAsync(company));
                }
            } else
            {
                messages = await _sapPmManager.GetSapPmNotificationMessagesAsync(null);
            }

            List<SapPmNotificationFailure> failures = await _sapPmManager.GetSapPmNotificationMessageFailures();

            int minutelyRetries = _configurationHelper.GetValueAsInteger("AppSettings:MinutelyRetries");
            int hourlyRetries = _configurationHelper.GetValueAsInteger("AppSettings:HourlyRetries");

            messages.RemoveAll(m => failures.Exists(f => m.ActionId == f.ActionId && f.FailureCount > minutelyRetries + hourlyRetries && f.MinutesSinceLastFailure < 60*24)); //remove messages that have failed more than the allowed minutely and hourly retries, and have been sent in the last 24 hours.
            messages.RemoveAll(m => failures.Exists(f => m.ActionId == f.ActionId && f.FailureCount > minutelyRetries && f.MinutesSinceLastFailure < 60)); //remove messages that have failed more than the allowed minutely retries, and have been sent in the last hour.

            if (messages.Count == 0)
            {
                _logger.LogInformation("No SAP PM notification messages to send.");
                return result;
            }

            foreach (SapPmNotificationMessage message in messages)
            {
                result += await SendAndProcessSapPmNotificationMessageAsync(message);
            }

            return result;
        }

        #endregion

        #region private methods

        private async Task<int> SynchFunctionalLocationsForCompany(int companyId)
        {
            int result = 0;
            string sapCompanyId = await _generalManager.GetSettingValueForCompanyByResourceId(companyId, 112); //112 is the resource ID for SAP Company ID
            string bearerToken = await GetSapPmBearerToken(companyId);


            if (!string.IsNullOrEmpty(sapCompanyId) && !string.IsNullOrEmpty(bearerToken))
            {
                int companyResult = 0;

                int retrievalMonthLimit = 3; // Default to 3 months
                DateTime lastChange = await _sapPmManager.GetLastChangeDateForFunctionalLocationsByCompanyId(companyId);


                while (lastChange < DateTime.Now)
                {
                    String SapFunctionalLocations = await FetchFunctionalLocationsFromSapApi(companyId, bearerToken, sapCompanyId, lastChange, lastChange.AddMonths(3));

                    if (!string.IsNullOrEmpty(SapFunctionalLocations))
                    {
                        companyResult += (int)await _sapPmManager.SendFunctionalLocationsToDatabase(companyId, SapFunctionalLocations);
                    }

                    //check for next timerange
                    lastChange = lastChange.AddMonths(retrievalMonthLimit);
                }

                if (companyResult > 0)
                {
                    _logger.LogInformation($"Successfully synchronized {companyResult} functional locations for company {companyId}.");
                    int restructureResult = await _sapPmManager.RegenerateFunctionalLocationsTreeStructure(companyId);
                    _logger.LogInformation($"Tree structure for functional locations has been regenerated for company {companyId}. Result: {restructureResult} changed locations.");
                }

                result += companyResult;
            }
            else
            {
                _logger.LogWarning($"Problem with SAP PM settings for {companyId}. Skipping synchronization.");
            }

            return result;
        }

        private async Task<string> FetchFunctionalLocationsFromSapApi(int companyId, string bearerToken, string sapCompanyId, DateTime lastChangeFrom, DateTime lastChangeTo)
        {
            string dateFormat = "dd.MM.yyyy";
            string locationUrl = await _generalManager.GetSettingValueForCompanyOrHoldingByResourceId(companyid: companyId, resourcesettingid: 120); // 120 is the resource ID for SAP PM Functional Locations URL

            //replace placeholders in the URL
            locationUrl = locationUrl.Replace("{SAPPMID}", sapCompanyId);
            locationUrl = locationUrl.Replace("{FROMDATE}", lastChangeFrom.ToString(dateFormat));
            locationUrl = locationUrl.Replace("{TODATE}", lastChangeTo.ToString(dateFormat));

            try
            {


                var request = new HttpRequestMessage(HttpMethod.Get, locationUrl);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(responseBody) || responseBody.Length < 100)
                    {
                        _logger.LogWarning($"No functional locations found for company {sapCompanyId} between {lastChangeFrom.ToShortDateString()} and {lastChangeTo.ToShortDateString()}. Response body: {responseBody}");
                        return string.Empty;
                    }

                    _logger.LogInformation($"Successfully fetched functional locations from SAP API for company {sapCompanyId} between {lastChangeFrom.ToShortDateString()} and {lastChangeTo.ToShortDateString()}");
                    // Process the response body as needed
                    return responseBody;
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch functional locations from SAP API. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    // Handle the error as needed
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmConnectionManager.FetchFunctionalLocationsFromSapApi(): ", ex.Message));
            }



            return string.Empty;
        }


        private async Task<string> GetSapPmBearerToken(int companyId)
        {
            string result = String.Empty;

            string authUrl = await _generalManager.GetSettingValueForCompanyOrHoldingByResourceId(companyid: companyId, resourcesettingid: 119); // 119 is the resource ID for SAP PM Bearer Token URL
            string authcreds = await _generalManager.GetSettingValueForCompanyOrHoldingByResourceId(companyid: companyId, resourcesettingid: 122, decryptValue: true);// 122 is the resource ID for SAP PM Authorisation Credentials
            if (String.IsNullOrEmpty(authUrl) || String.IsNullOrEmpty(authcreds))
            {
                _logger.LogError($"SapPmConnectionManager.GetSapPmBearerToken(): Missing SAP PM API URL or credentials for company {companyId}. Cannot retrieve bearer token.");
                return result;
            }

            try
            {
                Login sappmCredentials = authcreds.ToObjectFromJson<Login>();
                string encodedCreds = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(sappmCredentials.UserName + ":" + sappmCredentials.Password));

                var request = new HttpRequestMessage(HttpMethod.Get, authUrl);
                request.Headers.Add("Authorization", "Basic " + encodedCreds);


                request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                });


                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        // Parse JSON and extract access_token
                        try
                        {
                            using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
                            if (doc.RootElement.TryGetProperty("access_token", out var tokenElement))
                            {
                                result = tokenElement.GetString();
                            }
                            else
                            {
                                _logger.LogError($"SapPmConnectionManager.GetSapPmBearerToken(): access_token not found in response for company {companyId}. Response: {responseBody}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"SapPmConnectionManager.GetSapPmBearerToken(): Failed to parse bearer token JSON for company {companyId}. Response: {responseBody}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"SapPmConnectionManager.GetSapPmBearerToken(): No bearer token received from SAP PM API for company {companyId}. Response body is empty.");
                    }
                }
                else
                {
                    _logger.LogError($"SapPmConnectionManager.GetSapPmBearerToken(): Failed to retrieve bearer token from SAP PM API. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmConnectionManager.GetSapPmBearerToken(): ", ex.Message));
            }

            return result;
        }
        private async Task<int> SendAndProcessSapPmNotificationMessageAsync(SapPmNotificationMessage message)
        {
            int result = 0;
            string bearerToken = await GetSapPmBearerToken(message.CompanyId);
            string notificationUrl = await _generalManager.GetSettingValueForCompanyOrHoldingByResourceId(companyid: message.CompanyId, resourcesettingid: 121); // 121 is the resource ID for SAP PM Notification URL
            
            try
            {
                if (String.IsNullOrEmpty(notificationUrl) || String.IsNullOrEmpty(bearerToken))
                {
                    throw(new Exception($"Missing SAP PM API Notifcation URL or credentials for company { message.CompanyId}."));
                }

                var request = new HttpRequestMessage(HttpMethod.Post, notificationUrl);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");
                request.Content = new StringContent(message.Payload.ToJsonFromObject(), Encoding.UTF8, "application/json");

                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.SendAsync(request);

                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument jsonResponseBody = null;

                try
                {
                    jsonResponseBody = JsonDocument.Parse(responseBody);
                }
                catch
                {
                    //ignore JSON parse errors
                }

                JsonElement jsonData;
                if (response.IsSuccessStatusCode && //is statuc code 200
                    jsonResponseBody != null && //is valid JSON response
                    jsonResponseBody.RootElement.TryGetProperty("data", out jsonData) //has data property
                    && jsonData.TryGetProperty("SUCCESS", out JsonElement succesData)) //has SUCCESS property
                {
                    _logger.LogInformation($"Successfully sent SAP PM notification message for company {message.CompanyId}. Response: {responseBody}");
                    long? sapNotificationId = null;

                    if(jsonData.TryGetProperty("NOTIFICATION", out JsonElement jsonNotificationId)){
                        sapNotificationId = Int64.Parse(jsonNotificationId.GetString());                        
                    }

                    await _sapPmProcessingManager.ProcessSapPmNotificationResponseAsync(notificationId: message.Id, actionId: message.ActionId, companyId: message.CompanyId, success: true, response: succesData.GetString(), sapNotificationId: sapNotificationId);
                    return 1;
                }
                else
                {
                    _logger.LogError($"Failed to send SAP PM notification message for company {message.CompanyId} with id {message.Id}. Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}, Reponse: {responseBody}");

                    string? errorMessage = string.Empty;
                    if (jsonResponseBody != null && //is valid JSON response
                        jsonResponseBody.RootElement.TryGetProperty("data", out jsonData) //has data property
                        && jsonData.TryGetProperty("ERROR", out jsonData)) //has ERROR property

                    {
                        errorMessage = jsonData.GetString();
                    }
                    await _sapPmProcessingManager.ProcessSapPmNotificationResponseAsync(notificationId: message.Id, actionId: message.ActionId, companyId: message.CompanyId, success: false, response: errorMessage, sapNotificationId: null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SapPmConnectionManager.SendAndProcessSapPmNotificationMessageAsync(): ", ex.Message));
                await _sapPmProcessingManager.ProcessSapPmNotificationResponseAsync(notificationId: message.Id, actionId: message.ActionId, companyId: message.CompanyId, success: false, response: null, sapNotificationId: null);
                return result;
            }
            return result;
        }
        #endregion
    }
}
