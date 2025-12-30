using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Settings.Helpers;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class AuthenticationSettingManager  : BaseManager<AuthenticationSettingManager>, IAuthenticationSettingsManager
    {
        private const int EMAIL_TOKEN_VALID_MINUTES = 1140; //1 day
        private const int SMS_TOKEN_VALID_MINUTES = 1140; //1 day
        private const int SYNC_TOKEN_VALID_MINUTES = 14400; //10 days
        private const string TOKEN_SETUP_BASE = "{0}|{1}";

        private readonly IConfigurationHelper _configurationHelper;
        private readonly IDatabaseAccessHelper _manager;

        public AuthenticationSettingManager(IConfigurationHelper confighelper, IDatabaseAccessHelper databaseAccessHelper, ILogger<AuthenticationSettingManager> logger) : base(logger)
        {
            _configurationHelper = confighelper;
            _manager = databaseAccessHelper;
        }

        public async Task<int> CreateAuthenticationSettingsBaseAsync(int companyId, int userId, UserAuthenticationSettings baseUserAuthenticationSettings = null)
        {
            if (baseUserAuthenticationSettings == null)
            {
                baseUserAuthenticationSettings = new UserAuthenticationSettings();
                baseUserAuthenticationSettings.UserId = userId;
                baseUserAuthenticationSettings.CompanyId = companyId;
                baseUserAuthenticationSettings.MfaToptToken = Guid.NewGuid().ToString();
                baseUserAuthenticationSettings.MfaEmailToken = Guid.NewGuid().ToString();
                baseUserAuthenticationSettings.MfaSmsToken = Guid.NewGuid().ToString();
                baseUserAuthenticationSettings.MfaGeneralGuid = Guid.NewGuid().ToString();
                baseUserAuthenticationSettings.SyncGuid = Guid.NewGuid().ToString();

            }

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", baseUserAuthenticationSettings.CompanyId));
            parameters.Add(new NpgsqlParameter("@_userid", baseUserAuthenticationSettings.UserId));
            parameters.Add(new NpgsqlParameter("@_mfatopttoken", baseUserAuthenticationSettings.MfaToptToken));
            parameters.Add(new NpgsqlParameter("@_mfaemailtoken", baseUserAuthenticationSettings.MfaEmailToken));
            parameters.Add(new NpgsqlParameter("@_mfasmstoken", baseUserAuthenticationSettings.MfaSmsToken));
            parameters.Add(new NpgsqlParameter("@_mfageneralguid", baseUserAuthenticationSettings.MfaGeneralGuid));
            parameters.Add(new NpgsqlParameter("@_syncguid", baseUserAuthenticationSettings.SyncGuid));

            //will only add if not exists (handled in SP)
            var possibleId = (int)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "add_authentication_setting", parameters: parameters);

            return possibleId;
        }

        public async Task<string> GetOrCreateUserGuidAsync(int companyId, int userId)
        {
            var output = string.Empty;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (string)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_andor_create_user_guid", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.GetUserGuid(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<string> GetOrCreateSyncGuidAsync(int companyId, int userId)
        {
            var output = string.Empty;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (string)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_andor_create_user_sync_guid", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.GetSyncGuidAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<UserAuthenticationSettings> GetEnabledMfaOptionsAsync(int companyId, int userId)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<bool> GetForceUserMfaAsync(int companyId, int userId)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<UserAuthenticationSettings> GetUserAuthenticationSettingsAsync(int companyId, int userId)
        {
            var authenticationSetting = new UserAuthenticationSettings();

            authenticationSetting = await GetUserAuthenticationSettingsAsync(companyId: companyId, userId: userId);

            if (authenticationSetting == null || authenticationSetting.Id == 0)
            {
                int authenticationId = await CreateAuthenticationSettingsBaseAsync(companyId: companyId, userId: userId); //retrieve full version
                authenticationSetting = await GetUserAuthenticationSettingsAsync(companyId: companyId, userId: userId, authenticationSettingId: authenticationId);
            }

            return authenticationSetting;
        }

        private async Task<UserAuthenticationSettings> GetUserAuthenticationSettingsAsync(int companyId, int userId, int authenticationSettingId = 0)
        {
            var authenticationSetting = new UserAuthenticationSettings();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                if(authenticationSettingId > 0)
                {
                    parameters.Add(new NpgsqlParameter("@_userauthenticationsettingid", authenticationSettingId));
                }

                using (dr = await _manager.GetDataReader("get_user_authentication_settings", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    //NOTE SHOULD ONLY RETURN 1 ITEM, if not pick the last one.
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillActionFromReader(dr: dr, userAuthenticationSetting: authenticationSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.GetUserAuthenticationSettingsAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return authenticationSetting;
        }

        public async Task<bool> GetUserCanLoginAsync(int companyId, int userId)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "check_authorization_settings_user_can_login", parameters: parameters);
            } catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.GetUserCanLoginAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> GetUserMustSetMfaTopt(int companyId, int userId)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "check_authorization_settings_user_must_set_mfa_topt", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.GetUserMustSetMfaTopt(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> GetUserPasswordMustBeChangedAsync(int companyId, int userId)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "check_authorization_settings_user_must_change_password", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.GetUserPasswordMustBeChangedAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<UserAuthenticationSettings> GetUserSyncDataAsync(int companyId, int userId)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<bool> SetAddPwdLastHashesAsync(int companyId, int userId, string oldHash)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_oldhash", oldHash));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_old_hash", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetAddPwdLastHashesAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetCanLoginAsync(int companyId, int userId, bool canLogin)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_canlogin", canLogin));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_can_login", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetCanLoginAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetEmailMfaAsync(int companyId, int userId, string emailCode)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_emailCode", emailCode));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_emailcode", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetEmailMfaAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetMfaEmailLastUsedAsync(int companyId, int userId)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_mfa_email_last_used", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetMfaEmailLastUsedAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetMfaSmslLastUsedAsync(int companyId, int userId)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_mfa_sms_last_used", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetMfaSmslLastUsedAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetMfaToptLastUsedAsync(int companyId, int userId)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_mfa_topt_last_used", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetMfaToptLastUsedAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetPasswordMustBeChangedNextLoginAsync(int companyId, int userId, bool mustBeChanged)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_passwordchangenextlogin", mustBeChanged));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_change_pwd_next_login", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetPasswordMustBeChangedNextLoginAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetSmsMfaAsync(int companyId, int userId, string smsCode)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_smscode", smsCode));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_sms_code", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetSmsMfaAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetSyncGuidAsync(int companyId, int userId, string syncGuid)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_syncguid", syncGuid));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_sync_guid", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetSyncGuidAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> SetToptMfaAsync(int companyId, int userId, string toptCode)
        {
            var output = false;
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_toptcode", toptCode));

                output = (bool)await _manager.ExecuteScalarAsync(procedureNameOrQuery: "set_authorization_settings_user_toptcode", parameters: parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AuthenticationSettingManager.SetToptMfaAsync(): ", ex.Message));
                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return output;
        }

        public async Task<bool> ValidateEmailMfaAsync(int companyId, int userId, string emailCode)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<bool> ValidateSmsMfaAsync(int companyId, int userId, string smsCode)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<bool> ValidateToptMfaAsync(int companyId, int userId, string toptCode)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        private UserAuthenticationSettings CreateOrFillActionFromReader(NpgsqlDataReader dr, UserAuthenticationSettings userAuthenticationSetting = null)
        {
            if (userAuthenticationSetting == null) userAuthenticationSetting = new UserAuthenticationSettings();


            return userAuthenticationSetting;

            //if (dr["comment"] != DBNull.Value && !string.IsNullOrEmpty(dr["comment"].ToString()))
            //{
            //    action.Comment = dr["comment"].ToString();
            //}
            //action.CommentCount = Convert.ToInt32(dr["commentnr"]);
            //action.CompanyId = Convert.ToInt32(dr["company_id"]);
            //action.CreatedById = Convert.ToInt32(dr["created_by_id"]);
            //action.CreatedBy = dr["createdby"].ToString();
            //if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            //{
            //    action.Description = dr["description"].ToString();
            //}
            //action.DueDate = Convert.ToDateTime(dr["due_date"]);
            //action.Id = Convert.ToInt32(dr["id"]);
            //if (dr["is_resolved"] != DBNull.Value)
            //{
            //    action.IsResolved = Convert.ToBoolean(dr["is_resolved"]);
            //}
            //if (dr["resolved_at"] != DBNull.Value)
            //{
            //    action.ResolvedAt = Convert.ToDateTime(dr["resolved_at"]);
            //}
            //if (dr["task_id"] != DBNull.Value)
            //{
            //    action.TaskId = Convert.ToInt32(dr["task_id"]);
            //}
            //if (dr["task_template_id"] != DBNull.Value)
            //{
            //    action.TaskTemplateId = Convert.ToInt32(dr["task_template_id"]);
            //}
            //if (dr.HasColumn("lastcommentdate"))
            //{
            //    if (dr["lastcommentdate"] != DBNull.Value)
            //    {
            //        action.LastCommentDate = Convert.ToDateTime(dr["lastcommentdate"]);
            //    }
            //}

            //if (dr["created_at"] != DBNull.Value)
            //{
            //    action.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            //}
            //if (dr["modified_at"] != DBNull.Value)
            //{
            //    action.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);
            //}

            //return action;
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion

    }
}
