using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Logic.Raw;
using EZGO.Api.Models;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Settings;
using EZGO.Api.Utils.Json;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class SapPmProcessingManager : BaseManager<SapPmProcessingManager>, ISapPmProcessingManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IActionManager _actionManager;
        private readonly IUserManager _userManager;

        #endregion

        #region - constructors -
        public SapPmProcessingManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, IActionManager actionManager, IUserManager userManager, ILogger<SapPmProcessingManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _actionManager = actionManager;
            _userManager = userManager;
        }
        #endregion

        #region - public methods notifications -

        public async Task<bool> ProcessSapPmNotificationResponseAsync(int notificationId, int actionId, int companyId, bool success, string response, long? sapNotificationId)
        {
            bool result = true;
            if (!string.IsNullOrEmpty(response)) //notification sucessfully sent to DB, in case of failure likely due to validation error. Do not resend. 
            {
                try
                {
                    //update the notification with the response
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter> { new NpgsqlParameter("@_notificationid", notificationId) };
                    if (success)
                    {
                        if (sapNotificationId.HasValue)
                        {
                            parameters.Add(new NpgsqlParameter("@_sapnotificationid", sapNotificationId.Value));
                        }
                        else
                        {
                            parameters.Add(new NpgsqlParameter("@_sapnotificationid", Convert.ToInt64(response.Split(" ")[1])));
                        }
                    }
                    else
                    {
                        parameters.Add(new NpgsqlParameter("@_sapnotificationid", DBNull.Value));
                    }
                    int rowsUpdated = Convert.ToInt32(await _manager.ExecuteScalarAsync("update_sap_notification_sent_to_sap", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

                    if (rowsUpdated != 1)
                    {
                        _logger.LogWarning($"Something went wrong updating notification ID {notificationId}. Action ID: {actionId}. Success: {success}. Response: {response}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("SapPmProcessingManager.ProcessSapPmNotificationResponseAsync(): ", ex.Message));
                    if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
                    return false;
                }
            }

            //update the action comments with the response
            ActionsAction action = await _actionManager.GetActionAsync(companyId: companyId, actionId: actionId);
            var systemUserId = await _userManager.RetrieveSystemUserId(companyId: companyId);
            
            ActionComment comment = new ActionComment
            {
                ActionId = actionId,
                CreatedAt = DateTime.UtcNow,
                UserId = systemUserId
            };

            if (!success && !string.IsNullOrEmpty(response)) //vailidation error, do not resend message
            {
                comment.Comment = String.Concat(response, ", action failed to send to SAP, will only be sent again after action has changed.");
            } else if (success)
            {
                await _actionManager.SetActionResolvedAsync(companyId: companyId, userId: systemUserId, actionId: actionId, isResolved: true, useAutoResolvedMessage: false);
                comment.Comment = String.Concat("Action has been resolved and sent to SAP - ", response);
            } else
            {
                comment.Comment = _configurationHelper.GetValueAsString("AppSettings:NotificationFailureMessage");
            }
            
            int possibleId = await _actionManager.AddActionCommentAsync(companyId: companyId, userId: systemUserId, actionComment: comment);
            if(possibleId == 0)
            {
                _logger.LogWarning($"Something went wrong adding action comment for action ID {actionId}.");
                return false;
            }


            return result;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }

        #endregion
    }
}
