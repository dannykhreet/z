using EZGO.Api.Data.Base;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Models.Relations;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Data.Users
{
    /// <summary>
    /// UserDataManager; Contains specific methods for getting data from the database based on user tokens and user.
    /// These methods will be used internally in several managers, security checks and other functionalities.
    /// </summary>
    public class UserDataManager : BaseManager<UserDataManager>, IUserDataManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        #endregion

        #region - constructor(s) -
        public UserDataManager(IDatabaseAccessHelper manager, ILogger<UserDataManager> logger) : base(logger)
        {
            _manager = manager;
        }
        #endregion

        #region - public methods -
        /// <summary>
        /// GetCompanyIdByUserAuthenticationTokenAsync; Gets the company id from the database based on the user token that is supplied.
        /// </summary>
        /// <param name="token">Token based on DJANGO security token.</param>
        /// <returns>CompanyId, based on db: [companies_company.id].</returns>
        public async Task<int> GetCompanyIdByUserAuthenticationTokenAsync(string token)
        {
            if(!string.IsNullOrEmpty(token))
            {
                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@token", token));

                    var o = (await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_companyid_by_token", parameters: parameters)).ToString();

                    int.TryParse(o, out int companyid);

                    return companyid;
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: "Error occurred GetCompanyIdByUserAuthenticationTokenAsync()");
                }
                finally
                {
                }
            }
            return 0;
        }

        /// <summary>
        /// GetUserIdByUserAuthenticationTokenAsync; Gets the user id from the database based on the user token that is supplied.
        /// </summary>
        /// <param name="token">Token based on DJANGO security token.</param>
        /// <returns>UserId, based on db: [profile_user.id].</returns>
        public async Task<int> GetUserIdByUserAuthenticationTokenAsync(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@token", token));

                    var o = (await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_userid_by_token", parameters: parameters)).ToString();

                    int.TryParse(o, out int userid);

                    return userid;
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: "Error occurred GetUserIdByUserAuthenticationTokenAsync()");
                }
                finally
                {
                }
            }
            return 0;
        }

        /// <summary>
        /// GetUserIdByUserAuthenticationTokenAsync; Gets the user id from the database based on the user token that is supplied.
        /// </summary>
        /// <param name="token">Token based on DJANGO security token.</param>
        /// <returns>UserId and CompanyId in relation object, based on db: [profile_user.id / profile_user.company.id].</returns>
        public async Task<UserRelationCompany> GetUserCompanyRelationByAuthenticationTokenAsync(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                NpgsqlDataReader dr = null;

                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_token", token));

                    using (dr = await _manager.GetDataReader("get_userid_companyid_by_token", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                    {
                        UserRelationCompany usercompany = new UserRelationCompany();

                        while (await dr.ReadAsync())
                        {
                            usercompany.CompanyId = Convert.ToInt32(dr["company_id"]);
                            usercompany.UserId = Convert.ToInt32(dr["user_id"]);
                        }
                        return usercompany;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: string.Concat("UserDataManager.GetUserCompanyRelationByAuthenticationTokenAsync(): ", ex.Message));
                }
                finally
                {
                    if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
                }

            }
            return null;
        }
        //

        /// <summary>
        /// GetTokenByUserNameAndPassword; Get's the user authentication token from the DB based on the user name and password.
        /// The token is located in the database: [authtoken_token.key] table and column.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <param name="hashedpassword">Hashed password. Needs to be hashed with the DJANGO hashing functionality.</param>
        /// <returns>The authentication token.</returns>
        public async Task<string> GetTokenByUserNameAndPassword(string username, string hashedpassword)
        {

            if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(hashedpassword))
            {
                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_username", username));
                    parameters.Add(new NpgsqlParameter("@_password", hashedpassword));

                    var o = await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_token_by_username_and_password", parameters: parameters);
                    return o.ToString();

                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: "Error occurred GetTokenByUserNameAndPassword()");
                }
                finally
                {

                }
            }
            return string.Empty;
        }

        /// <summary>
        ///GetTokenByUserName; Get token based on the username and companyId
        /// </summary>
        /// <param name="username">Username to be checked</param>
        /// <param name="companyId">CompanyId to be checked</param>
        /// <returns>token for further processing.</returns>
        public async Task<string> GetTokenByUserName(string username, int companyId)
        {

            if (!string.IsNullOrEmpty(username) && companyId > 0)
            {
                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_username", username));
                    parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                    var o = await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_token_by_username_and_company", parameters: parameters);
                    return o.ToString();

                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: "Error occurred GetTokenByUserNameAndPassword()");
                }
                finally
                {

                }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetUserPasswordByUserName; Gets the encrypted user password from the database. This method can be validated against incoming UserName and Passwords after encryption to check if the supplied information is correct.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <returns>A encrypted password from the database.</returns>
        public async Task<string> GetUserPasswordByUserName(string username)
        {
            string pwd = string.Empty;
            if (!string.IsNullOrEmpty(username))
            {
                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_username", username));

                    var result = (await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_userpassword_by_username", parameters: parameters));
                    if (result != null)
                    {
                        pwd = result.ToString();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: "Error occurred GetUserPasswordByUserName({0})", username);
                }
                finally
                {
                }
            }
            return pwd;
        }

        /// <summary>
        /// GetUserPasswordByUserId; Get user password (encrypted) for checking against newly encrypted password to validate if it is correct.
        /// </summary>
        /// <param name="userId">UserId for user.</param>
        /// <returns>Encrypted user password.</returns>
        public async Task<string> GetUserPasswordByUserId(int userId)
        {
            string pwd = string.Empty;
            if (!string.IsNullOrEmpty(userId.ToString()))
            {

                try
                {
                    List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                    parameters.Add(new NpgsqlParameter("@_id", userId));

                    var result = (await _manager.ExecuteScalarAsync(procedureNameOrQuery: "get_userpassword_by_userid", parameters: parameters)).ToString();
                    if (result != null)
                    {
                        pwd = result.ToString();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: "Error occurred GetUserPasswordByUserId()");
                }
                finally
                {
                }
            }
            return pwd;
        }

        /// <summary>
        /// CheckRecentlyExpiredToken; Check if token is recently expired, if so, login on other device probably occurred.
        /// </summary>
        /// <param name="token">Token to be checked.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> CheckRecentlyExpiredToken(string token)
        {
            var expiredLastTwentyFourHours = false;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_token", token));

            try
            {
                expiredLastTwentyFourHours = Convert.ToBoolean(await _manager.ExecuteScalarAsync("check_token_expired_longer_than_day", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: "Error occurred CheckRecentlyExpiredToken()");
                expiredLastTwentyFourHours = false;
            }

            return expiredLastTwentyFourHours;
        }
        #endregion
    }
}
