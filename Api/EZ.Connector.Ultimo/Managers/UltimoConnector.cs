using EZ.Connector.Ultimo.Helpers;
using EZ.Connector.Ultimo.Interfaces;
using EZ.Connector.Ultimo.Models;
using EZ.Connector.Ultimo.Settings;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models;
using EZGO.Api.Models.ExternalRelations;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace EZ.Connector.Ultimo.Managers
{
    /// <summary>
    /// UltimoConnector; Connector for Ultimo connectivity.
    /// NOTE! only for use within the Ultimo connector or Ultimo connector derivatives
    /// NOTE! currently this is a proof of concept and/or proof of technology.
    /// </summary>
    public class UltimoConnector : IUltimoConnector
    {
        protected readonly ILogger _logger;
        protected readonly IConfiguration _configuration;
        private readonly IGeneralManager _generalManager;
        private readonly ICryptography _cryptography;
        public IServiceProvider _services { get; }

        private static HttpClient _client; //NOTE! must be static due to setup .Net HttpClient. DO NOT CHANGE;

        #region - constructor -
        public UltimoConnector(IServiceProvider services, IConfiguration configuration, IGeneralManager generalManager, ICryptography cryptography, ILogger<UltimoConnector> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _generalManager = generalManager;
            _cryptography = cryptography;
            _services = services;
            if (_client == null) { _client = new HttpClient(); } //note! only create if not already created.
        }
        #endregion

        /// <summary>
        /// CheckCompanyForConnector; Check if a company has access to this connector.
        /// </summary>
        /// <param name="companyId">CompanyId to check.</param>
        /// <returns>true/false depending on check.</returns>
        public bool CheckCompanyForConnector(int companyId)
        {
            //TODO make dynamic
            if (companyId == 136)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// CheckConnection; Check connection to the Ultimo connector. 
        /// This is not yet available.
        /// NOTE! not yet implemented.
        /// </summary>
        /// <returns>true/false depending on check.</returns>
        public Task<bool> CheckConnection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SendActionToUltimoAsync; Send action to a external Ultimo connector. 
        /// </summary>
        /// <param name="action">Action that needs to be send to ultimo.</param>
        /// <param name="companyId">CompanyId to check, if feature is enabled.</param>
        /// <returns></returns>
        public async Task<bool> SendActionToUltimoAsync(ActionsAction action, int companyId, int userId)
        {
            var ultimoIsActive = await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "MARKET_ULTIMO");
            if (ultimoIsActive)
            {
                //fire and forget for current flow due to time taken to complete ULTIMO call
                _logger.LogInformation("Ultimo Connector send to Ultimo started");

                //Due to the slow connection and process speeds of the Ultimo APIs the logic that starts the post and processes
                //the information is loaded to a separate thread; The API itself can continue;
                _ = Task.Run(() => SendActionAsync(action: action, companyId: companyId, userId));
                //NOTE! due to the inner workings of the connector if a specific post to the database needs to be done then a separate connector needs to be used and initiated!.

                _logger.LogInformation("Ultimo Connector send to Ultimo offloaded to background tasks.");
            }

            return true;
        }

        /// <summary>
        /// GetConfiguration; Get configuration 
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id)</param>
        /// <returns>A configuration object for further processing</returns>
        public async Task<UltimoConfig> GetConfiguration(int companyId)
        {
            var settingValue = await _generalManager.GetSettingValueForCompanyByResourceId(companyid: companyId, resourcesettingid: UltimoConnectorSettings.ULTIMO_RESOURCE_SETTING_ID); //TODO make dynamic

            var config = new UltimoConfig();
            if (!string.IsNullOrEmpty(settingValue))
            {
                config = settingValue.ToString().ToObjectFromJson<UltimoConfig>();
            }

            return config;
        }

        /// <summary>
        /// HandleOutput; Handle response output of connector.
        /// NOTE! NOT YET IMPLEMENTED.
        /// </summary>
        /// <param name="responseMessage">Response message to be handled.</param>
        /// <returns></returns>
        public async Task<bool> HandleOutput(string responseMessage)
        {
            try
            {
                //NOTE IF DB CALL IS NEEDED CREATE A SEPERATE METHOD FOR DOING THIS.
                //NORMAL DATABASE CONNECTOR CAN BE DISPOSED OR CLOSED ALREADY WHEN READING DATA.
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }
            await Task.CompletedTask;
            return true;
        }

        #region - privates -
        /// <summary>
        /// SendActionAsync; method for sending a action to a Ultimo entry point.
        /// Ultimo 3rd party services uses a ApiKey and ApplicationElementId; These are company specific and based on the configuration/settings of a company.
        /// </summary>
        /// <param name="action">ActionsAction object.</param>
        /// <param name="companyId">Current CompanyId</param>
        /// <returns>nothing, just a task</returns>
        private async Task SendActionAsync(ActionsAction action, int companyId, int userId)
        {
            using (var scope = _services.CreateScope())
            {
                var configError = false;
                var scopedDatabaseAccessHelper = scope.ServiceProvider.GetRequiredService<IDatabaseAccessHelper>();
                var scopedGeneralManager = scope.ServiceProvider.GetRequiredService<IGeneralManager>();

                var companySettings = await scopedGeneralManager.GetSettingResourceItemForCompany(companyid: companyId);

                var uripart1 = "";

                var settingUri = companySettings.Where(s => s.ResourceId == 82).FirstOrDefault();
                if (settingUri != null && !string.IsNullOrEmpty(settingUri.Value))
                {
                    uripart1 = settingUri.Value.TrimEnd('/');
                }

                var uripart2 = "object/Job";

                var ultimoApiUrl = "";
                if (!string.IsNullOrEmpty(uripart1) && !string.IsNullOrEmpty(uripart2))
                {
                    ultimoApiUrl = string.Format("{0}/{1}", uripart1, uripart2);
                }

                var ultimoApiKey = companySettings.Where(s => s.ResourceId == 83).FirstOrDefault()?.Value;

                if (string.IsNullOrEmpty(ultimoApiUrl) || string.IsNullOrEmpty(ultimoApiKey))
                {
                    configError = true;
                }

                var ultimoAction = action.ToUltimoAction();

                NpgsqlDataReader dr = null;

                var externalRelationParams = new List<NpgsqlParameter>();

                ExternalRelation externalRelation = null;

                //try get existing external relation
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    externalRelationParams.Add(new NpgsqlParameter("@_companyid", companyId));
                    externalRelationParams.Add(new NpgsqlParameter("@_objectid", action.Id));
                    externalRelationParams.Add(new NpgsqlParameter("@_objecttype", "actions_action"));

                    using (dr = await scopedDatabaseAccessHelper.GetDataReader("get_external_relation", commandType: System.Data.CommandType.StoredProcedure, parameters: externalRelationParams, connectionKind: ConnectionKind.Writer))
                    {
                        while (await dr.ReadAsync())
                        {
                            externalRelation = CreateOrFillExternalRelationFromReader(dr, externalRelation);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO log error?
                    return;
                }
                finally
                {
                    if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
                }
#pragma warning restore CS0168 // Variable is declared but never used

                if (externalRelation != null && externalRelation.Status == UltimoConstants.EXTERNAL_RELATION_STATUS_SENT && externalRelation.ExternalId != null && externalRelation.ExternalId > 0) //external relation present so action is already in ultimo
                {
                    if (configError)
                    {
                        var responseContent = "API URL or Token not configured!";

                        var errorLog = new LoggingExternalRequestResponse()
                        {
                            ConnectorType = "ULTIMO",
                            CompanyId = companyId,
                            ObjectId = action.Id,
                            ObjectType = "actions_action",
                            Request = "",
                            Response = null
                        };
                        //error logging
                        errorLog.Description = "EZ-GO Action not sent to Ultimo! Ultimo API URL or Token not configured!";
                        errorLog.Response = responseContent;

                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(errorLog, userId);
                        errorLog.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR;
                        externalRelation.StatusMessage = string.Format(UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR_DESCRIPTION, "n.a.", responseContent);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                        return;
                    }

                    //update external relation with status READY_TO_BE_SENT
                    externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT;
                    externalRelation.StatusMessage = UltimoConstants.EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT_DESCRIPTION;

                    var externalRelationParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                    var externalRelationsChanged = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationParameters, commandType: System.Data.CommandType.StoredProcedure));

                    //TODO add error handling if change_external_relation goes wrong?

                    //update existing ultimo job 
                    ultimoApiUrl += $"('{externalRelation.ExternalId}')";

                    HttpResponseMessage result;
                    var requestPayload = string.Empty;
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, ultimoApiUrl))
                    {
                        requestMessage.Headers.Add("ApiKey", ultimoApiKey);

                        requestPayload = JsonSerializer.Serialize(ultimoAction);
                        requestMessage.Content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                        result = await _client.SendAsync(requestMessage);
                    }

                    var log = new LoggingExternalRequestResponse()
                    {
                        ConnectorType = "ULTIMO",
                        CompanyId = companyId,
                        ObjectId = action.Id,
                        ObjectType = "actions_action",
                        Request = requestPayload,
                        Response = null
                    };

                    if (result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.Created || result.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        var responseContent = await result.Content.ReadAsStringAsync();
                        log.Description = UltimoConstants.LOGGING_STATUS_SENT_DESCRIPTION;
                        log.Response = responseContent;

                        //logging
                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(log, userId);

                        log.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_SENT;
                        externalRelation.StatusMessage = UltimoConstants.EXTERNAL_RELATION_STATUS_SENT_DESCRIPTION;

                        //set external id of external relation
                        var ultimoResponse = responseContent.ToObjectFromJson<UltimoActionResponse>();
                        externalRelation.ExternalId = Convert.ToInt32(ultimoResponse.Id);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                    }
                    else
                    {
                        var responseContent = await result.Content.ReadAsStringAsync();
                        //error logging
                        log.Description = "EZ-GO Action not sent to Ultimo!";
                        log.Response = responseContent;

                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(log, userId);
                        log.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR;
                        externalRelation.StatusMessage = string.Format(UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR_DESCRIPTION, result.StatusCode, responseContent);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                    }
                }
                else if (externalRelation == null) //external relation doesnt exist so action not in ultimo
                {
                    //add new external relation with status READY_TO_BE_SENT
                    externalRelation = new ExternalRelation()
                    {
                        CompanyId = companyId,
                        ConnectorType = "ULTIMO",
                        ObjectId = action.Id,
                        ObjectType = "actions_action",
                        Status = UltimoConstants.EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT,
                        StatusMessage = UltimoConstants.EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT_DESCRIPTION,
                    };

                    if (configError)
                    {
                        var responseContent = "API URL or Token not configured!";

                        var errorLog = new LoggingExternalRequestResponse()
                        {
                            ConnectorType = "ULTIMO",
                            CompanyId = companyId,
                            ObjectId = action.Id,
                            ObjectType = "actions_action",
                            Request = "",
                            Response = null
                        };
                        //error logging
                        errorLog.Description = "EZ-GO Action not sent to Ultimo! Ultimo API URL or Token not configured!";
                        errorLog.Response = responseContent;

                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(errorLog, userId);
                        errorLog.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR;
                        externalRelation.StatusMessage = string.Format(UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR_DESCRIPTION, "n.a.", responseContent);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToAddExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                        return;
                    }

                    var externalRelationParameters = GetNpgsqlParametersToAddExternalRelation(externalRelation, userId);

                    externalRelation.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_relation", parameters: externalRelationParameters, commandType: System.Data.CommandType.StoredProcedure));

                    //TODO add error handling if add_external_relation goes wrong?

                    //post new ultimo job
                    HttpResponseMessage result;
                    var requestPayload = string.Empty;
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, ultimoApiUrl))
                    {
                        requestMessage.Headers.Add("ApiKey", ultimoApiKey);

                        requestPayload = JsonSerializer.Serialize(ultimoAction);
                        requestMessage.Content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                        result = await _client.SendAsync(requestMessage);
                    }

                    var log = new LoggingExternalRequestResponse()
                    {
                        ConnectorType = "ULTIMO",
                        CompanyId = companyId,
                        ObjectId = action.Id,
                        ObjectType = "actions_action",
                        Request = requestPayload,
                        Response = null
                    };

                    if (result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.Created || result.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        var responseContent = await result.Content.ReadAsStringAsync();
                        log.Description = UltimoConstants.LOGGING_STATUS_SENT_DESCRIPTION;
                        log.Response = responseContent;

                        //logging
                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(log, userId);

                        log.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_SENT;
                        externalRelation.StatusMessage = UltimoConstants.EXTERNAL_RELATION_STATUS_SENT_DESCRIPTION;

                        //set external id of external relation
                        var ultimoResponse = responseContent.ToObjectFromJson<UltimoActionResponse>();
                        externalRelation.ExternalId = Convert.ToInt32(ultimoResponse.Id);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                    }
                    else
                    {
                        var responseContent = await result.Content.ReadAsStringAsync();
                        //error logging
                        log.Description = string.Format(UltimoConstants.LOGGING_STATUS_ERROR_DESCRIPTION, result.StatusCode);
                        log.Response = responseContent;

                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(log, userId);
                        log.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR;
                        externalRelation.StatusMessage = string.Format(UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR_DESCRIPTION, result.StatusCode, responseContent);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                    }
                }
                else //external relation not null but action not in ultimo
                {
                    if (configError)
                    {
                        var responseContent = "API URL or Token not configured!";

                        var errorLog = new LoggingExternalRequestResponse()
                        {
                            ConnectorType = "ULTIMO",
                            CompanyId = companyId,
                            ObjectId = action.Id,
                            ObjectType = "actions_action",
                            Request = "",
                            Response = null
                        };
                        //error logging
                        errorLog.Description = "EZ-GO Action not sent to Ultimo! Ultimo API URL or Token not configured!";
                        errorLog.Response = responseContent;

                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(errorLog, userId);
                        errorLog.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR;
                        externalRelation.StatusMessage = string.Format(UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR_DESCRIPTION, "n.a.", responseContent);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                        return;
                    }

                    //update existing external relation with status READY_TO_BE_SENT
                    externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT;
                    externalRelation.StatusMessage = UltimoConstants.EXTERNAL_RELATION_STATUS_READY_TO_BE_SENT_DESCRIPTION;

                    var externalRelationParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                    var externalRelationsChanged = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationParameters, commandType: System.Data.CommandType.StoredProcedure));

                    //TODO add error handling if change_external_relation goes wrong?

                    //post new ultimo job
                    HttpResponseMessage result;
                    var requestPayload = string.Empty;
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, ultimoApiUrl))
                    {
                        requestMessage.Headers.Add("ApiKey", ultimoApiKey);

                        requestPayload = JsonSerializer.Serialize(ultimoAction);
                        requestMessage.Content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                        result = await _client.SendAsync(requestMessage);
                    }

                    var log = new LoggingExternalRequestResponse()
                    {
                        ConnectorType = "ULTIMO",
                        CompanyId = companyId,
                        ObjectId = action.Id,
                        ObjectType = "actions_action",
                        Request = requestPayload,
                        Response = null
                    };

                    if (result.StatusCode == System.Net.HttpStatusCode.OK || result.StatusCode == System.Net.HttpStatusCode.Created || result.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        var responseContent = await result.Content.ReadAsStringAsync();
                        log.Description = UltimoConstants.LOGGING_STATUS_SENT_DESCRIPTION;
                        log.Response = responseContent;

                        //logging
                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(log, userId);

                        log.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_SENT;
                        externalRelation.StatusMessage = UltimoConstants.EXTERNAL_RELATION_STATUS_SENT_DESCRIPTION;

                        //set external id of external relation
                        var ultimoResponse = responseContent.ToObjectFromJson<UltimoActionResponse>();
                        externalRelation.ExternalId = Convert.ToInt32(ultimoResponse.Id);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                    }
                    else
                    {
                        var responseContent = await result.Content.ReadAsStringAsync();
                        //error logging
                        log.Description = string.Format(UltimoConstants.LOGGING_STATUS_ERROR_DESCRIPTION, result.StatusCode);
                        log.Response = responseContent;

                        var logParameters = GetNpgsqlParametersToAddLoggingExternalRequestResponse(log, userId);
                        log.Id = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("add_external_logging_requestresponse", parameters: logParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if add_external_logging_requestresponse goes wrong?

                        //update external relation
                        externalRelation.Status = UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR;
                        externalRelation.StatusMessage = string.Format(UltimoConstants.EXTERNAL_RELATION_STATUS_ERROR_DESCRIPTION, result.StatusCode, responseContent);

                        var externalRelationUpdateParameters = GetNpgsqlParametersToUpdateExternalRelation(externalRelation, userId);

                        var externalRelationsUpdated = Convert.ToInt32(await scopedDatabaseAccessHelper.ExecuteScalarAsync("change_external_relation", parameters: externalRelationUpdateParameters, commandType: System.Data.CommandType.StoredProcedure));

                        //TODO add error handling if change_external_relation goes wrong?
                    }
                }
            }
        }

        private List<NpgsqlParameter> GetNpgsqlParametersToAddExternalRelation(ExternalRelation externalRelation, int userId)
        {
            //add_external_relation
            //"_object_type" varchar, "_object_id" int4, "_external_id" int4, "_status" varchar, "_status_message" text, "_connector_type" varchar, "_companyid" int4, "_userid" int4
            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_object_type", externalRelation.ObjectType));
            parameters.Add(new NpgsqlParameter("@_object_id", externalRelation.ObjectId));
            parameters.Add(new NpgsqlParameter("@_external_id", externalRelation.ExternalId ?? 0));
            parameters.Add(new NpgsqlParameter("@_status", externalRelation.Status));
            parameters.Add(new NpgsqlParameter("@_status_message", externalRelation.StatusMessage));
            parameters.Add(new NpgsqlParameter("@_connector_type", externalRelation.ConnectorType));
            parameters.Add(new NpgsqlParameter("@_companyid", externalRelation.CompanyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));

            return parameters;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersToUpdateExternalRelation(ExternalRelation externalRelation, int userId)
        {
            //change_external_relation
            //"_id" int4, "_external_id" int4, "_status" varchar, "_status_message" text, "_companyid" int4, "_userid" int4
            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_id", externalRelation.Id));
            parameters.Add(new NpgsqlParameter("@_external_id", externalRelation.ExternalId ?? 0));
            parameters.Add(new NpgsqlParameter("@_status", externalRelation.Status));
            parameters.Add(new NpgsqlParameter("@_status_message", externalRelation.StatusMessage));
            parameters.Add(new NpgsqlParameter("@_companyid", externalRelation.CompanyId));
            parameters.Add(new NpgsqlParameter("@_userid", userId));

            return parameters;
        }

        private List<NpgsqlParameter> GetNpgsqlParametersToAddLoggingExternalRequestResponse(LoggingExternalRequestResponse loggingExternalRequestResponse, int userId)
        {
            //add_external_logging_requestresponse
            //"_object_type" varchar, "_object_id" int4, "_companyid" int4, "_request" text, "_response" text, "_connector_type" varchar, "_description" text, "_userid" int4
            var parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_object_type", loggingExternalRequestResponse.ObjectType));
            parameters.Add(new NpgsqlParameter("@_object_id", loggingExternalRequestResponse.ObjectId));
            parameters.Add(new NpgsqlParameter("@_companyid", loggingExternalRequestResponse.CompanyId));
            parameters.Add(new NpgsqlParameter("@_request", loggingExternalRequestResponse.Request));
            parameters.Add(new NpgsqlParameter("@_response", loggingExternalRequestResponse.Response));
            parameters.Add(new NpgsqlParameter("@_connector_type", loggingExternalRequestResponse.ConnectorType));
            parameters.Add(new NpgsqlParameter("@_description", loggingExternalRequestResponse.Description));
            parameters.Add(new NpgsqlParameter("@_userid", userId));

            return parameters;
        }

        private ExternalRelation CreateOrFillExternalRelationFromReader(NpgsqlDataReader dr, ExternalRelation externalRelation)
        {
            //returns a table
            //"id" int4,
            //"object_type" varchar,
            //"object_id" int4,
            //"external_id" int4,
            //"status" varchar,
            //"status_message" text,
            //"connector_type" varchar,
            //"company_id" int4,
            //"created_at" timestamp,
            //"modified_at" timestamp,
            //"created_by_id" int4,
            //"modified_by_id" int4

            if (externalRelation == null) externalRelation = new ExternalRelation();


            externalRelation.Id = Convert.ToInt32(dr["id"]);

            externalRelation.ObjectType = dr["object_type"].ToString();

            externalRelation.ObjectId = Convert.ToInt32(dr["object_id"]);

            if (dr["external_id"] != DBNull.Value)
            {
                externalRelation.ExternalId = Convert.ToInt32(dr["external_id"]);
            }

            externalRelation.Status = dr["status"].ToString();

            externalRelation.StatusMessage = dr["status_message"].ToString();

            externalRelation.ConnectorType = dr["connector_type"].ToString();

            externalRelation.CompanyId = Convert.ToInt32(dr["company_id"]);

            externalRelation.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            externalRelation.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);

            externalRelation.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            externalRelation.ModifiedById = Convert.ToInt32(dr["modified_by_id"]);

            return externalRelation;
        }

        #endregion
    }
}
