using EZ.Connector.SAP.Helpers;
using EZ.Connector.SAP.Interfaces;
using EZ.Connector.SAP.Models;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EZ.Connector.SAP.Managers
{
    /// <summary>
    /// SAPConnector; Connector for SAP connectivity.
    /// NOTE! only for use within the SAP connector or SAP connector derivatives
    /// </summary>
    public class SAPConnector : ISAPConnector
    {
        protected readonly ILogger _logger;
        protected readonly IConfiguration _configuration;
        protected readonly IDatabaseAccessHelper _databaseAccessHelper;

        private static HttpClient _client; //NOTE! must be static due to setup .Net HttpClient. DO NOT CHANGE;

        public SAPConnector(ILogger<SAPConnector> logger, IConfiguration configuration, IDatabaseAccessHelper dataManager)
        {
            _logger = logger;
            _configuration = configuration;
            _databaseAccessHelper = dataManager;
            if (_client == null) { _client = new HttpClient(); } //note! only create if not already created.
        }

        /// <summary>
        /// SendToSapAsync; Send a action to a SAP api entry point.
        /// </summary>
        /// <param name="action">ActionsAction object.</param>
        /// <param name="companyId">CurrentCompanyId</param>
        /// <returns>true/false depending on outcome</returns>
        public async Task<bool> SendActionToSAPAsync(ActionsAction action, int companyId)
        {
            //fire and forget for current flow due to time taken to complete SAP call
            _logger.LogInformation("SAP Connector send to SAP started");
            //Due to the slow connection and process speeds of the SAP APIs the logic that starts the post and processes
            //the information is loaded to a separate thread; The API itself can continue;
            _ = Task.Run(() => SendActionAsync(action: action, companyId: companyId));
            _logger.LogInformation("SAP Connector send to SAP offloaded to background tasks.");
            await Task.CompletedTask;
            return true;
        }

        /// <summary>
        /// SendActionToSAPAsync; method for sending a action to a SAP entry point.
        /// </summary>
        /// <param name="action">ActionsAction object.</param>
        /// <param name="companyId">CurrentCompanyId</param>
        /// <returns>nothing, just a task</returns>
        private async Task SendActionAsync(ActionsAction action, int companyId)
        {
            _logger.LogInformation("SAP Connector post started");
            //TODO add advanced response handling, currently just statuscode handling.
            //TODO add context for db connection
            if (action.IsValidSAPAction())
            {
                try
                {
                    var companies = GetConfigurationSettings(Settings.SAPConnectorSettings.ACTION_CONNECTION_COMPANIES_CONFIG_KEY);
                    if (!string.IsNullOrEmpty(companies))
                    {
                        if (companies.Split(",").Select(x => Convert.ToInt32(x)).Contains(companyId))
                        {
                            //baseUrls for media
                            //_confighelper.GetValueAsString(MediaSettings.AWSS3_MEDIA_BUCKETREGIONURL_CONFIG_KEY) : _confighelper.GetValueAsString(MediaSettings.AWSS3_BUCKETREGIONURL_CONFIG_KEY)
                            var baseMediaUrl = GetConfigurationSettings("S3Config:MediaBucketRegionUrl");
                            var baseVideoUrl = GetConfigurationSettings("S3Config:BucketRegionUrl");

                            //get all relevant data for post.
                            var sapAction = action.ToSAPAriniBTP(baseUrlMedia: baseMediaUrl, baseUrlVideo: baseVideoUrl);
                            var connectionUrl = GetConfigurationSettings(Settings.SAPConnectorSettings.ACTION_CONNECTOR_URL_CONFIG_KEY);
                            var uid = GetConfigurationSettings(Settings.SAPConnectorSettings.UID_CONFIG_KEY);
                            var pwd = GetConfigurationSettings(Settings.SAPConnectorSettings.PWD_CONFIG_KEY);
                            var payload = string.Empty;
                            //TODO add validators on sapAction

                            HttpResponseMessage result;
                            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, connectionUrl))
                            {
                                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(pwd))
                                {
                                    //add headers if available. For internal tests not needed.
                                    var authHeaderValue = $"{uid}:{pwd}";
                                    var authHeaderByteArray = Encoding.ASCII.GetBytes(authHeaderValue);
                                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authHeaderByteArray));
                                }
                                payload = JsonSerializer.Serialize(sapAction);
                                //var content = new MultipartFormDataContent();
                                //content.Add(new StringContent(payload, Encoding.UTF8, "application/json"), "input");
                                requestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                                _logger.LogInformation("SAP Connector Start Posting");
                                result = await _client.SendAsync(requestMessage);
                                _logger.LogInformation("SAP Connector Done Posting");
                            }

                            string response = "";
                            if (result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.Created || result.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                response = await result.Content.ReadAsStringAsync();
                                _logger.LogInformation("SAP Connector post result: [{0}] - {1}", result, payload);
                                //TODO add converter -> result SAPAariniResponse
                                //TODO add saving of data to DB
                                

                            }
                            else
                            {
                                response = await result.Content.ReadAsStringAsync();

                                //extra error handling if response object is borked.
                                try
                                {
                                    _logger.LogError("SAP Connector Failed for {0} returns {1} : [{3}] - {2}", action.Id, result.StatusCode, payload, response);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "SAP Connector Failed for {0} error {1}", action.Id, ex.Message);
                                }

                            }
                            await SaveResponseInformation(actionId: action.Id, companyId: companyId, connectorType: ConnectorTypeEnum.SAP, requestedBody: payload, responseBody: response); //Add type
                        }
                    }
                }
                catch (Exception ex)
                {
                    //log error
                    _logger.LogError(exception: ex, "SAP Connector Failed: {0}", ex.Message);
                }
            }

            _logger.LogInformation("SAP Connector post ended");
        }

        /// <summary>
        /// CheckCompanyForConnector; Check if company uses the connector. (currently only for actions.
        /// </summary>
        /// <param name="companyId">Current company id to be checked.</param>
        /// <returns>true/false depending on outcome.</returns>
        public bool CheckCompanyForConnector(int companyId)
        {
            var companies = GetConfigurationSettings(Settings.SAPConnectorSettings.ACTION_CONNECTION_COMPANIES_CONFIG_KEY);
            if (!string.IsNullOrEmpty(companies))
            {
                return (companies.Split(",").Select(x => Convert.ToInt32(x)).Contains(companyId));

            }
            return false;
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <returns></returns>
        public async Task<SAPConfig> GetConfiguration(int companyId)
        {
            await Task.CompletedTask;
            return new SAPConfig();
        }

        /// <summary>
        /// GetConfigurationSettings; Get configuration key value.
        /// NOTE! this is based on the IConfigHelper in the Settings Library. This connector will not be used to make sure there are no dependencies to this library.
        /// Or as little possible for making it easy to convert to a plug-in like structure on a later date.
        /// </summary>
        /// <param name="keyname">Key to get</param>
        /// <returns>String value from config (based on key)</returns>
        private string GetConfigurationSettings(string keyname)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(keyname)))
            {
                return Environment.GetEnvironmentVariable(keyname);
            }
            else if (_configuration.GetSection(keyname) != null && !string.IsNullOrEmpty(_configuration.GetSection(keyname).Value))
            {
                return _configuration.GetSection(keyname).Value;
            }
            return "";
        }

        /// <summary>
        /// NOT YET AVAILABLE; Needs implementation on SAP handler side; Currently no SAP handler for checking connection available;
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckConnection()
        {
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// SaveResponseInformation; save extended information send back from post. 
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SaveResponseInformation(int actionId, int companyId, ConnectorTypeEnum connectorType, string requestedBody, string responseBody) 
        {
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_actionid", actionId));
                parameters.Add(new NpgsqlParameter("@_connectortype", connectorType));
                parameters.Add(new NpgsqlParameter("@_response", requestedBody));
                parameters.Add(new NpgsqlParameter("@_request", responseBody));

                var id = Convert.ToInt32(await _databaseAccessHelper.ExecuteScalarAsync("add_action_connector", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
                return (id > 0);
            } catch (Exception ex)
            {
                _logger.LogError(exception: ex, "SAPConnector.SaveResponseInformation() {0}", ex.Message);
            }
            return false;

        }
    }
}
