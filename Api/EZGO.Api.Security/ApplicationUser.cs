using Elastic.Apm.Api;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Users;
using EZGO.Api.Security;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Cleaners;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Media;
using EZGO.Api.Utils.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

//******************
//TODO ADD ADVANCED ERROR HANDLING AND NULL CHECKS
//******************
namespace EZGO.Api.Security
{
    /// <summary>
    /// ApplicationUser; application user, containing a user profile and specific functionality for use with authentication.
    /// Normal user functionality can be found in the <see cref="EZGO.Api.Logic.Managers.UserManager">UserManager</see>.
    /// </summary>
    public class ApplicationUser : IApplicationUser
    {
        #region - private / variables -
        private IObjectRights _objectRights;
        private ILogger _logger;
        private IHttpContextAccessor _httpcontextaccessor;
        private readonly IUserDataManager _userdatamanager;
        private readonly IUserManager _userManager;
        private readonly ICryptography _cryptography;
        private readonly IConfigurationHelper _confighelper;
        private readonly IDatabaseAccessHelper _manager;
        private readonly IAuthenticationSettingsManager _userAuthenticationSettingManager;
        private readonly IGeneralManager _generalManager;

        //TODO add user profile basic information.

        public int CompanyId { get; private set; }
        public int UserId { get; private set; }
        #endregion

        #region - constructor -
        public ApplicationUser(IUserDataManager userdatamanager, IDatabaseAccessHelper manager, IAuthenticationSettingsManager userAuthenticationSettingManager, IHttpContextAccessor httpContextAccessor, IUserManager userManager, ICryptography cryptography, IConfigurationHelper configurationhelper, IObjectRights objectRights, IGeneralManager generalManager, ILogger<ApplicationUser> logger)
        {
            _userdatamanager = userdatamanager;
            _httpcontextaccessor = httpContextAccessor;
            _logger = logger;
            _confighelper = configurationhelper;
            _userManager = userManager;
            _cryptography = cryptography;
            _manager = manager;
            _objectRights = objectRights;
            _userAuthenticationSettingManager = userAuthenticationSettingManager;
            _generalManager = generalManager;

        }
        #endregion

        #region - methods -
        /// <summary>
        /// GetAndSetCompanyId; Get a CompanyId (db. companies_company.id) with a user token for use within queries and checks.
        /// When called the ApplicationUser.CompanyId is also set for use if needed within the GetAndSetCompanyIdByTokenAsync() method.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetAndSetCompanyIdAsync()
        {
            var currentToken = await GetAuthTokenFromSidClaimUserAsync();
            if (CompanyId <= 0 && !string.IsNullOrEmpty(currentToken))
            {
                //await GetAndSetCompanyIdByDjangoTokenAsync(currentToken);
                await GetAndSetUserAndCompanyIdByDjangoTokenAsync(token:currentToken);
            }
            if (CompanyId <= 0)
            {
                throw (new UnauthorizedAccessException(message: "Unauthorized. User session is not valid or expired."));
            }
            return CompanyId;

        }

        /// <summary>
        /// GetAndSetCompanyIdByDjangoTokenAsync; Get the user ID based on the Django token in header. NOTE! only use for specific functionality (external logins on old system).
        /// </summary>
        /// <returns>int companyId</returns>
        public async Task<int> GetAndSetCompanyIdByDjangoTokenAsync()
        {
            var currentToken = await GetUserTokenFromHeaders();
            if (CompanyId <= 0 && !string.IsNullOrEmpty(currentToken))
            {
                //await GetAndSetCompanyIdByDjangoTokenAsync(currentToken);
                await GetAndSetUserAndCompanyIdByDjangoTokenAsync(token: currentToken);
            }
            if (CompanyId <= 0)
            {
                throw (new UnauthorizedAccessException(message: "Unauthorized. User session is not valid or expired."));
            }
            return CompanyId;

        }

        /// <summary>
        /// GetAndSetUserIdAsync; Get a UserId with a token.
        /// </summary>
        /// <returns>The user id.</returns>
        public async Task<int> GetAndSetUserIdAsync()
        {
            var currentToken = await GetAuthTokenFromSidClaimUserAsync();
            if (UserId <= 0 && !string.IsNullOrEmpty(currentToken))
            {
                //await GetAndSetUserIdByDjangoTokenAsync(currentToken);
                await GetAndSetUserAndCompanyIdByDjangoTokenAsync(token: currentToken);
            }
            if (UserId <= 0)
            {
                throw (new UnauthorizedAccessException(message: "Unauthorized. User session is not valid or expired."));
            }
            return UserId;

        }

        /// <summary>
        /// LoginAndGetDjangoSecurityToken; Gets the security token string by UserName and Password.
        /// </summary>
        /// <param name="username">Incoming UserName as string</param>
        /// <param name="password">Incoming Password as string</param>
        /// <returns>Django sec token.</returns>
        public async Task<string> LoginAndGetDjangoSecurityToken(string username, string password)
        {
            var token = string.Empty;

            if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && string.IsNullOrEmpty(await GetUserTokenFromHeaders()))
            {
                // TODO refactor
                // Init authenticator
                var authenticator = new Authenticator();
                // Get current password by user name
                var currentPassword = await _userdatamanager.GetUserPasswordByUserName(username: username);
                // Get salt from current password
                var currentSalt = authenticator.GetSaltFromPassword(hashedpassword: currentPassword);
                // Hash incoming password with current salt
                var encryptedPasswordByte = Encryptor.PBKDF2_Sha256_GetBytes(256 / 8, authenticator.GetBytePassword(password), authenticator.GetBytePassword(currentSalt), Authenticator.iterations);
                // Reconstruct full password
                var generatedPassword = authenticator.GeneratePasswordForStorage(currentSalt, authenticator.GetBase64PasswordHash(encryptedPasswordByte));
                // Call GetTokenByUserNameAndPassword for token
                token = await _userdatamanager.GetTokenByUserNameAndPassword(username: username, hashedpassword: generatedPassword);
            }

            return token;
        }

        /// <summary>
        /// LoginAndGetSecurityToken; Gets the security token string by UserName and Password.
        /// </summary>
        /// <param name="username">Incoming UserName as string</param>
        /// <param name="password">Incoming Password as string</param>
        /// <param name="isCmsLogin">Incoming login request is a request from the CMS (portal)</param>
        /// <param name="isAppLogin">Incoming login request is a request from the App (xamerin or otherwise)</param>
        /// <returns>Token string containing a jwt token.</returns>
        public async Task<LoggedIn> LoginAndGetSecurityToken(string username, string password, bool? isCmsLogin = false, bool? isAppLogin = false, string ips = null)
        {
            LoggedIn loggedIn = new LoggedIn();
            int companyId = 0;
            var token = string.Empty;
            var possibleMessage = string.Empty;
            var source = (isAppLogin.HasValue && isAppLogin.Value) ? "APP" : (isCmsLogin.HasValue && isCmsLogin.Value) ? "CMS" : "";
            UserProfile user = null;

            try
            {
                // TODO refactor
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    // Init authenticator.
                    var authenticator = new Authenticator();
                    // Get current password by user name.
                    var currentPassword = await _userdatamanager.GetUserPasswordByUserName(username: username);
                    // Get salt from current password.
                    var currentSalt = authenticator.GetSaltFromPassword(hashedpassword: currentPassword);
                    // Hash incoming password with current salt.
                    var encryptedPasswordByte = Encryptor.PBKDF2_Sha256_GetBytes(256 / 8, authenticator.GetBytePassword(password), authenticator.GetBytePassword(currentSalt), Authenticator.iterations);
                    // Reconstruct full password.
                    var generatedPassword = authenticator.GeneratePasswordForStorage(currentSalt, authenticator.GetBase64PasswordHash(encryptedPasswordByte));
                    // check password against generated password.
                    if (generatedPassword == currentPassword)
                    {
                        // if enabled reset auth token after login.
                        if (!_confighelper.GetValueAsBool(Settings.ApiSettings.ENABLE_MULTI_DEVICE_LOGIN))
                        {

                            if (isCmsLogin.HasValue && isCmsLogin.Value)
                            {
                                if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("CMS LOGIN: {0}", UserCleaner.CleanUserNameForDisplay(username)), type: "", eventid: "", eventname: "", description: "", source: "");

                                //only reset token if expired
                                bool generationsuccesfull = await _userManager.ResetOrCreateAuthenticationDbTokenIfExpired(userName: username, encryptedPassword: generatedPassword);
                                if (!generationsuccesfull) return loggedIn; //if not successfull return nothing, can not login, see logs
                            }
                            else
                            {
                                if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("APP LOGIN: {0}", UserCleaner.CleanUserNameForDisplay(username)), type: "", eventid: "", eventname: "", description: "", source: "");

                                bool generationsuccesfull = await _userManager.ResetOrCreateAuthenticationDbToken(userName: username, encryptedPassword: generatedPassword);
                                if (!generationsuccesfull) return loggedIn; //if not successfull return nothing, can not login, see logs
                            }
                        }
                        else
                        {
                            //only reset token if expired
                            bool generationsuccesfull = await _userManager.ResetOrCreateAuthenticationDbTokenIfExpired(userName: username, encryptedPassword: generatedPassword);
                            if (!generationsuccesfull) return loggedIn; //if not successfull return nothing, can not login, see logs
                        }

                        // Get the database token.
                        var databaseToken = await _userdatamanager.GetTokenByUserNameAndPassword(username: username, hashedpassword: generatedPassword);

                        if (!string.IsNullOrEmpty(databaseToken))
                        {
                            // Get company id based on the token.
                            companyId = await GetAndSetCompanyIdByDjangoTokenAsync(token: databaseToken);

                            // Validate if ip address is valid

                            if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("CID LOGIN: {0}", companyId), type: "", eventid: "", eventname: "", description: "", source: "");

                            // Get user based on the token and the company id.
                            user = await _userManager.GetUserProfileByTokenAsync(companyId: companyId, userToken: databaseToken, tokenIsEncrypted: false, include: "roles", connectionKind: ConnectionKind.Writer);

                            if (user == null || user.Id <= 0)
                            {
                                possibleMessage = string.Concat("Token can not be generated correctly; User is unknown:", databaseToken);

                                if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: possibleMessage, type: "", eventid: "", eventname: "", description: "", source: "");

                            }

                            user.CurrentIps = ips;

                            if (isCmsLogin.Value && _confighelper.GetValueAsBool("AppSettings:EnableDirectIpCheckForCMS") || !isCmsLogin.Value)
                            {
                                if (!await this.ValidateIpForLoginAuthentication(companyId: companyId, possibleIps: user.CurrentIps))
                                {
                                    possibleMessage = "Invalid IP for login; Login not allowed from this IP address";
                                    if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("INVALID IP FOR LOGIN: {0} - {1}", companyId, user.Id), type: "INFORMATION", eventid: "", eventname: "", description: "", source: "");
                                    await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: string.Format("Unsuccessful login; ({0}); {1} - {2} - {3}", user.Id, UserCleaner.CleanUserNameForDisplay(username), possibleMessage, user.CurrentIps), eventId: 806, source: source);
                                    return loggedIn; //invalid ip, stop processing
                                }
                            }

                            //check if user token exists, if not retrieve it separately and or create and retrieve it. 
                            if (string.IsNullOrEmpty(user.UserGUID))
                            {
                                user.UserGUID = await _userAuthenticationSettingManager.GetOrCreateUserGuidAsync(companyId: companyId, userId: user.Id);
                            }
                            //check if sync token exists, if not retrieve it and or create and retrieve it.
                            if (string.IsNullOrEmpty(user.SyncGUID))
                            {
                                user.SyncGUID = await _userAuthenticationSettingManager.GetOrCreateSyncGuidAsync(companyId: companyId, userId: user.Id);
                            }

                            this.CompanyId = companyId;
                            this.UserId = user.Id;

                            // Create the JWT token.
                            token = CreateToken(user: user, databaseToken: databaseToken);
                        } else
                        {
                            possibleMessage = "Database token can not be retrieved; User unknown or password incorrect";
                        }
                    } else
                    {
                        if (string.IsNullOrEmpty(currentPassword)) possibleMessage = "User unknown; Incorrect Password or Username;";
                    }

                    if (!string.IsNullOrEmpty(token) && user != null)
                    {
                        await _userManager.SetLastLoggedInDate(user.Id);
                        await _userManager.AddLoginSecurityLogEvent(message: "Successful login", description: string.Format("Successful login ({0}); {1}", user.Id, possibleMessage), eventId: 800, source: source);
                        loggedIn.CompanyId = companyId;
                        loggedIn.UserId = user.Id;
                        loggedIn.Token = token;
                    } else
                    {
                        await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: string.Format("Unsuccessful login ({0}); {1}", UserCleaner.CleanUserNameForDisplay(username), possibleMessage), eventId: 802, source: source);
                    }
                }
                else
                {
                    if(string.IsNullOrEmpty(username))
                    {
                        await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: "Unsuccessful login; Empty username", eventId: 803, source: source);
                    } else
                    {
                        await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: "Unsuccessful login; Empty password", eventId: 804, source: source);
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError(message: "Error occurred on LogIn.", exception: ex);

                await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: string.Format("Unsuccessful login; ({0}); {1} - {2}", ex.Message, UserCleaner.CleanUserNameForDisplay(username), possibleMessage), eventId: 802, source: source);

                await WriteToDBLog(message: string.Format("Error occurred LogIn() : {0} - {1}", ex.Message, possibleMessage), type: "", eventid: "", eventname: "", ex.StackTrace.ToString(), string.Empty);
            }

            return loggedIn;
        }

        /// <summary>
        /// LoginAndGetSecurityToken;
        /// </summary>
        /// <param name="externaltoken">External login tokens (based on JwT containing all information for logging in.</param>
        /// <param name="isCmsLogin">From app</param>
        /// <param name="isAppLogin">From cms</param>
        /// <returns>Sec token</returns>
        public async Task<LoggedIn> LoginAndGetSecurityToken(ExternalLogin external, bool? isCmsLogin = false, bool? isAppLogin = false, string ips = null)
        {
            LoggedIn loggedIn = new LoggedIn();
            int companyId = 0;
            var source = (isAppLogin.HasValue && isAppLogin.Value) ? "APP" : (isCmsLogin.HasValue && isCmsLogin.Value) ? "CMS" : "";
            var possibleMessage = string.Empty;
            var token = string.Empty;
            var username = string.Empty;

            try
            {

                if(_confighelper.GetValueAsInteger("AppSettings:ExternalLoginVersion") == 2) //implemented for backwards compatibility
                {
                    if(string.IsNullOrEmpty(external.AccessToken) || string.IsNullOrEmpty(external.IdToken) || !await ValidateToken(accessToken:external.AccessToken, idToken: external.IdToken))
                    {
                        return loggedIn;
                    }
                } else if (_confighelper.GetValueAsInteger("AppSettings:ExternalLoginVersion") == 1)
                {
                    if (string.IsNullOrEmpty(external.AccessToken)) {
                        return loggedIn;
                    }
                } 

                var handler = new JwtSecurityTokenHandler();
                var jwtObject = handler.ReadJwtToken(external.AccessToken); //create a jwt token object so claims if needed can be read.
                if(jwtObject != null)
                {
                    string upn = jwtObject.GetClaim("upn");
                    string tenantId = jwtObject.GetClaim("tid");


                    if(!string.IsNullOrEmpty(upn) && !string.IsNullOrEmpty(tenantId))
                    {
                        if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("UPN {0} + TENANT {1}", upn, tenantId), type: "", eventid: "", eventname: "", description: "", source: "");

                        //check upn / get company id from db
                        var upnCompanyId = await _userManager.GetCompanyIdByUPN(upn: upn, connectionKind: ConnectionKind.Writer);
                        if(upnCompanyId > 0)
                        {
                            if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: "UPN + COMPANYID", type: "", eventid: "", eventname: "", description: "", source: "");

                            var tenant = await _userManager.GetTenantByCompanyId(companyId: upnCompanyId, connectionKind: ConnectionKind.Writer);
                            if (upnCompanyId > 0 && tenant.ToLower() == tenantId.ToLower())
                            {
                                if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: "COMPANYID + TENANT", type: "", eventid: "", eventname: "", description: "", source: "");

                                username = await _userManager.GetUserNameByUPN(upn: upn, companyId: upnCompanyId, connectionKind: ConnectionKind.Writer);

                                if(!string.IsNullOrEmpty(username))
                                {
                                    if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: "USERNAME FOUND", type: "", eventid: "", eventname: "", description: "", source: "");

                                    if (!_confighelper.GetValueAsBool(Settings.ApiSettings.ENABLE_MULTI_DEVICE_LOGIN))
                                    {

                                        if (isCmsLogin.HasValue && isCmsLogin.Value)
                                        {
                                            if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("CMS LOGIN: {0}", UserCleaner.CleanUserNameForDisplay(username)), type: "", eventid: "", eventname: "", description: "", source: "");

                                            //only reset token if expired
                                            bool generationsuccesfull = await _userManager.ResetOrCreateAuthenticationDbTokenIfExpiredByUserName(userName: username);
                                            if (!generationsuccesfull) return loggedIn; //if not successfull return nothing, can not login, see logs
                                        }
                                        else
                                        {
                                            if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("APP LOGIN: {0}", UserCleaner.CleanUserNameForDisplay(username)), type: "", eventid: "", eventname: "", description: "", source: "");

                                            bool generationsuccesfull = await _userManager.ResetOrCreateAuthenticationDbTokenByUserName(userName: username);
                                            if (!generationsuccesfull) return loggedIn; //if not successfull return nothing, can not login, see logs
                                        }
                                    }
                                    else
                                    {
                                        //only reset token if expired
                                        bool generationsuccesfull = await _userManager.ResetOrCreateAuthenticationDbTokenIfExpiredByUserName(userName: username);
                                        if (!generationsuccesfull) return loggedIn; //if not successfull return nothing, can not login, see logs
                                    }

                                    // Get the database token.
                                    var databaseToken = await _userdatamanager.GetTokenByUserName(username: username, companyId: upnCompanyId);

                                    if (!string.IsNullOrEmpty(databaseToken))
                                    {
                                        // Get company id based on the token.
                                        companyId = await GetAndSetCompanyIdByDjangoTokenAsync(token: databaseToken);

                                        if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("CID LOGIN: {0}", companyId), type: "", eventid: "", eventname: "", description: "", source: "");

                                        // Get user based on the token and the company id.
                                        var user = await _userManager.GetUserProfileByTokenAsync(companyId: companyId, userToken: databaseToken, tokenIsEncrypted: false, include: "roles", connectionKind: ConnectionKind.Writer);

                                        if (user == null || user.Id <= 0)
                                        {
                                            possibleMessage = string.Concat("Token can not be generated correctly; User is unknown:", databaseToken);

                                            if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: possibleMessage, type: "", eventid: "", eventname: "", description: "", source: "");

                                        }

                                        user.CurrentIps = ips;
                                        
                                        if (isCmsLogin.Value && _confighelper.GetValueAsBool("AppSettings:EnableDirectIpCheckForCMS") || !isCmsLogin.Value)
                                        {
                                            if (!await this.ValidateIpForLoginAuthentication(companyId: companyId, possibleIps: user.CurrentIps))
                                            {
                                                possibleMessage = "Invalid IP for login; Login not allowed from this IP address";
                                                if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: string.Format("INVALID IP FOR LOGIN: {0} - {1}", companyId, user.Id), type: "INFORMATION", eventid: "", eventname: "", description: "", source: "");
                                                await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: string.Format("Unsuccessful login; ({0}); {1} - {2} - {3}", user.Id, UserCleaner.CleanUserNameForDisplay(username), possibleMessage, user.CurrentIps), eventId: 806, source: source);
                                                return loggedIn; //invalid ip, stop processing
                                            }
                                        }

                                        //check if user token exists, if not retrieve it seperately and or create and retrieve it. 
                                        if (string.IsNullOrEmpty(user.UserGUID))
                                        {
                                            user.UserGUID = await _userAuthenticationSettingManager.GetOrCreateUserGuidAsync(companyId: companyId, userId: user.Id);
                                        }

                                        //check if sync token exists, if not retrieve it and or create and retrieve it.
                                        if (string.IsNullOrEmpty(user.SyncGUID))
                                        {
                                            user.SyncGUID = await _userAuthenticationSettingManager.GetOrCreateSyncGuidAsync(companyId: companyId, userId: user.Id);
                                        }

                                        this.CompanyId = companyId;
                                        this.UserId = user.Id;

                                        // Create the JWT token.
                                        token = CreateToken(user: user, databaseToken: databaseToken);

                                        if (!string.IsNullOrEmpty(token) && user != null)
                                        {
                                            await _userManager.SetLastLoggedInDate(user.Id);
                                            await _userManager.AddLoginSecurityLogEvent(message: "Successful login", description: string.Format("Successful login ({0}); {1}", user.Id, possibleMessage), eventId: 800, source: source);
                                            loggedIn.CompanyId = companyId;
                                            loggedIn.UserId = user.Id;
                                            loggedIn.Token = token;
                                        }
                                        else
                                        {
                                            await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: string.Format("Unsuccessful login ({0}); {1}", UserCleaner.CleanUserNameForDisplay(username), possibleMessage), eventId: 802, source: source);
                                        }
                                    }
                                    else
                                    {
                                        possibleMessage = "Database token can not be retrieved; User unknown or password incorrect";

                                        if (_confighelper.GetValueAsBool(ApiSettings.AUTHENTICATION_LOGGING_CONFIG_KEY)) await WriteToDBLog(message: possibleMessage, type: "", eventid: "", eventname: "", description: "", source: "");
                                    }
                                } else
                                {
                                    await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: "Unsuccessful login; Empty username", eventId: 803, source: source);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message: "Error occurred on LogIn.", exception: ex);

                await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: string.Format("Unsuccessful login; ({0}); {1} - {2}", ex.Message, UserCleaner.CleanUserNameForDisplay(username), possibleMessage), eventId: 802, source: source);

                await WriteToDBLog(message: string.Format("Error occurred LogIn() : {0} - {1}", ex.Message, possibleMessage), type: "", eventid: "", eventname: "", ex.StackTrace.ToString(), string.Empty);
            }

            return loggedIn;
        }

        /// <summary>
        /// GetCheckHasExternalLogin; Checks if a user exists, and can login with MSAL or other systems.
        /// </summary>
        /// <param name="username">Username, usually known as company UPN</param>
        /// <returns>Code for the system. e.g. MSAL</returns>
        public async Task<string> GetCheckAndGetExternalLogin(string username)
        {
            await Task.CompletedTask;
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_useridentifier", username));
            parameters.Add(new NpgsqlParameter("@_externalkey", ""));//TODO fill external key when determined what this should be exactly
            var system = await _manager.ExecuteScalarAsync("check_and_get_username_external_login", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            if(system == null)
            {
                return "";
            } else
            {
                return system.ToString();
            }
        }

        /// <summary>
        /// GetAndSetCompanyIdByDjangoTokenAsync; Get the company Id based on a token. If found return it and also set the CompanyId property of the ApplicationUser object.
        /// </summary>
        /// <param name="token">Token (db: authtoken_token); Based on a </param>
        /// <returns>Integer containing the company id. When not found it will return a 0 or integer default.</returns>
        private async Task<int> GetAndSetCompanyIdByDjangoTokenAsync(string token)
        {
            if(!string.IsNullOrEmpty(token))
            {
                CompanyId = await _userdatamanager.GetCompanyIdByUserAuthenticationTokenAsync(token: token);
            }

            if (CompanyId <= 0)
            {
                //check if user has a recently expired token and multi-device login is enabled.
                if (await _userdatamanager.CheckRecentlyExpiredToken(token: token) && (!_confighelper.GetValueAsBool(Settings.ApiSettings.ENABLE_MULTI_DEVICE_LOGIN)))
                {
                    throw (new UnauthorizedAccessException(message: AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN));
                }

                throw (new UnauthorizedAccessException(message: AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS));
            }
            return CompanyId;
        }

        /// <summary>
        /// GetAndSetUserIdByDjangoTokenAsync; Get the User Id based on a token. If found return it and also set the UserId property of the ApplicationUser object.
        /// </summary>
        /// <param name="token">Token (db: authtoken_token); Based on a </param>
        /// <returns>integer containing the user id. When not found it will return a 0 or integer default.</returns>
        private async Task<int> GetAndSetUserIdByDjangoTokenAsync(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                UserId = await _userdatamanager.GetUserIdByUserAuthenticationTokenAsync(token: token);
            }
            //TODO refactor
            if(UserId <= 0)
            {
                //check if user has a recently expired token and multi-device login is enabled.
                if (await _userdatamanager.CheckRecentlyExpiredToken(token: token) && (!_confighelper.GetValueAsBool(Settings.ApiSettings.ENABLE_MULTI_DEVICE_LOGIN)))
                {
                    throw (new UnauthorizedAccessException(message: AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN));
                }


                throw (new UnauthorizedAccessException(message: AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS));
            }
            return UserId;

        }

        /// <summary>
        /// GetAndSetUserAndCompanyIdByDjangoTokenAsync; Get company and user id based on the supplied token. Used for filling the ApplicationUserData;
        /// </summary>
        /// <param name="token">Token to be used for getting user company relation</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> GetAndSetUserAndCompanyIdByDjangoTokenAsync(string token)
        {
            var item = await _userdatamanager.GetUserCompanyRelationByAuthenticationTokenAsync(token: token);
            if (item != null)
            {
                //internal vars are use, so if multiple calls not all calls have to go through the API
                UserId = item.UserId;
                CompanyId = item.CompanyId;
            }

            if (UserId <= 0 || CompanyId <= 0)
            {
                //check if user has a recently expired token and multi-device login is enabled.
                if (await _userdatamanager.CheckRecentlyExpiredToken(token: token) && (!_confighelper.GetValueAsBool(Settings.ApiSettings.ENABLE_MULTI_DEVICE_LOGIN)))
                {
                    throw (new UnauthorizedAccessException(message: AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN));
                }

                throw (new UnauthorizedAccessException(message: AuthenticationSettings.MESSAGE_UNAUTHORIZED_ACCESS));

                //return false;
            } else
            {
                return true;
            }
        }

        /// <summary>
        /// GetUserTokenFromHeaders(); Gets the user token from the request headers. The token is converted to a valid token format for use with the database.
        /// </summary>
        /// <returns>String containing a token hash.</returns>
        private async Task<string> GetUserTokenFromHeaders()
        {
            StringValues currenttoken = string.Empty; //default Headers.TryGetValue from headers returns a primitive object. This will be converted later on.
            var succes = _httpcontextaccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out currenttoken);

            await Task.CompletedTask; //added for threading;

            if (succes)
            {
                return StringConverters.ConvertAuthorizationHeaderToToken(currenttoken.ToString());
            } else
            {
                _logger.LogWarning("ApplicationUser.GetUserTokenFromHeaders(): No token found. {0}", "Token not found returning empty string.");
                return string.Empty;
            }

        }
        #endregion

        #region - authorization -
        /// <summary>
        /// CreateToken; Create token based on claims. Claims will consist of user data and the auth_token (django token) in the database.
        /// </summary>
        /// <param name="user">User information (DB:profiles_user.*)</param>
        /// <param name="databaseToken">Django Token (DB:auth_authtoken)</param>
        /// <returns>Token string for use with authorization.</returns>
        private string CreateToken(UserProfile user, string databaseToken)
        {

            var claims = GetAndCreateClaims(user: user, databaseToken: databaseToken);
            var key = GetSecurityKey();
            var credentials = GetSigningCredentials(key: key);
            var tokenDescriptor = GetSecurityTokenDescriptor(signingCredentials: credentials, claims: claims);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// GetAndCreateClaims; Create a list of claims for use with authentication. Claims are based on user and a database token (Django token).
        /// </summary>
        /// <param name="user">User information (DB:profiles_user.*)</param>
        /// <param name="databaseToken">Django Token (DB:auth_authtoken)</param>
        /// <returns>List of filled claims for futher processing.</returns>
        private List<Claim> GetAndCreateClaims(UserProfile user, string databaseToken)
        {
            var protectedToken = "";
            if (!string.IsNullOrEmpty(databaseToken)) {
                try
                {
                    //For use with build-in .net protection api.
                    //protectedToken = _protector.Protect(databaseToken);
                    protectedToken = _cryptography.Encrypt(databaseToken);
                } catch (Exception ex)
                {
                    _logger.LogError(message:"Error occurred on token protection.", exception: ex);
                }
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, string.Concat(user.FirstName, " ", user.LastName)),
                new Claim(ClaimTypes.Sid, protectedToken),
                new Claim(ClaimTypes.Email, !string.IsNullOrEmpty(user.Email) ? user.Email : string.Empty),
                new Claim(ClaimTypes.Upn, !string.IsNullOrEmpty(user.UPN) ? user.UPN : string.Empty)
            };

            if (!string.IsNullOrEmpty(user.CompanyTimezone)) claims.Add(new Claim("companytimezone", user.CompanyTimezone));
            if (!string.IsNullOrEmpty(user.CompanyLanguageCulture)) claims.Add(new Claim("companylanguageculture", user.CompanyLanguageCulture));

            if (!string.IsNullOrEmpty(user.Role) || user.IsStaff || user.IsSuperUser || user.Roles != null)
            {
                //add user roles based on role or roles property
                if ((user.Roles != null && user.Roles.Contains(RoleTypeEnum.Basic)) || (user.Role == RoleTypeEnum.Basic.ToDatabaseString()))
                {
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.Basic.ToString().ToLower()));
                }
                if ((user.Roles != null && user.Roles.Contains(RoleTypeEnum.ShiftLeader)) || (user.Role == RoleTypeEnum.ShiftLeader.ToDatabaseString()))
                {
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.ShiftLeader.ToString().ToLower()));
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.ShiftLeader.ToDatabaseString())); //backwards compatible with older notation in DB
                }
                if ((user.Roles != null && user.Roles.Contains(RoleTypeEnum.Manager)) || (user.Role == RoleTypeEnum.Manager.ToDatabaseString()))
                {
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.Manager.ToString().ToLower()));
                }
                //add system roles based on user property or roles
                if ((user.Roles != null && user.Roles.Contains(RoleTypeEnum.Staff)) || (user.IsStaff)) 
                {
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.Staff.ToString().ToLower()));
                }
                if ((user.Roles != null && user.Roles.Contains(RoleTypeEnum.SuperUser)) || (user.IsSuperUser)) 
                { 
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.SuperUser.ToString().ToLower())); 
                }
                if ((user.Roles != null && user.Roles.Contains(RoleTypeEnum.TagManager)) || (user.IsTagManager.HasValue && user.IsTagManager.Value))
                { 
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.TagManager.ToString().ToLower())); 
                }
                if ((user.Roles != null && user.Roles.Contains(RoleTypeEnum.ServiceAccount)) || (user.IsServiceAccount)) 
                { 
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.ServiceAccount.ToString().ToLower())); 
                }
                //add system roles based on roles property or specific user properties
                if (user.Roles != null && user.Roles.Contains(RoleTypeEnum.UserManager)) {
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.UserManager.ToString().ToLower()));
                }
                if (user.Roles != null && user.Roles.Contains(RoleTypeEnum.RoleManager))
                {
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.RoleManager.ToString().ToLower()));
                }
                if (user.Roles != null && user.Roles.Contains(RoleTypeEnum.ExtendedUserManager))
                {
                    claims.Add(new Claim(ClaimTypes.Role, RoleTypeEnum.ExtendedUserManager.ToString().ToLower()));
                }
           
            }

            if(!string.IsNullOrEmpty(user.UserGUID))
            {
                claims.Add(new Claim("guid", user.UserGUID));
            }

            if (!string.IsNullOrEmpty(user.SyncGUID))
            {
                claims.Add(new Claim("syncguid", user.SyncGUID));
            }

            if (!string.IsNullOrEmpty(user.CurrentIps))
            {
                claims.Add(new Claim("currentips", user.CurrentIps));
            }

            return claims;
        }

        /// <summary>
        /// GetSecurityKey; Get Security Key based on the AppSettings secret token used.
        /// </summary>
        /// <returns>SymmetricSecurityKey</returns>
        private SymmetricSecurityKey GetSecurityKey()
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_confighelper.GetValueAsString(AuthenticationSettings.SECURITY_TOKEN_CONFIG_KEY)));
            return key;
        }

        /// <summary>
        /// GetSigningCredentials; Get SigningCredentials based on a  SymmetricSecurityKey;
        /// </summary>
        /// <param name="key">SymmetricSecurityKey <see cref="GetSecurityKey">GetSecurityKey()</see></param>
        /// <returns>SigningCredentials</returns>
        private SigningCredentials GetSigningCredentials(SymmetricSecurityKey key)
        {
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            return creds;
        }

        /// <summary>
        /// GetSecurityTokenDescriptor; Get a token descriptor for use with a token handler for creating a security token.
        /// </summary>
        /// <param name="signingCredentials">SigningCredentials</param>
        /// <param name="claims">List<Claim></param>
        /// <returns>SecurityTokenDescriptor</returns>
        private SecurityTokenDescriptor GetSecurityTokenDescriptor(SigningCredentials signingCredentials, List<Claim> claims)
        {
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(AuthenticationSettings.TOKEN_EXPERATION_IN_HOURS),
                SigningCredentials = signingCredentials
            };
            return tokenDescriptor;
        }

        /// <summary>
        /// GetAuthTokenFromSidClaimUserAsync; Authtoken via Sid Claim from the user claims. This will be filled with the Authentication Token used in the database (DB:authtoken_token).
        /// </summary>
        /// <returns>String containing the auth token.</returns>
        private async Task<string> GetAuthTokenFromSidClaimUserAsync()
        {
            string encryptedToken = _httpcontextaccessor.HttpContext.User.GetClaim(ClaimTypes.Sid);
            var decryptedToken = "";
            if (!string.IsNullOrEmpty(encryptedToken))
            {
                try
                {
                    //For use with build-in .net protection api.
                    //decryptedToken = _protector.Unprotect(encryptedToken);
                    decryptedToken = _cryptography.Decrypt(encryptedToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError(message: "Error occurred on token un-protection.", exception: ex);
                }
            }
            await Task.CompletedTask;
            return decryptedToken;
        }

        /// <summary>
        /// CheckObjectRights; Check if user has access to object
        /// </summary>
        /// <param name="objectId">Object id to check</param>
        /// <param name="objectType">Object type to check.</param>
        /// <returns>true/false if has rights to object.</returns>
        public async Task<bool> CheckObjectRights(int objectId, ObjectTypeEnum objectType, [System.Runtime.CompilerServices.CallerMemberName] string referrer = "")
        {

            var userid = await GetAndSetUserIdAsync();
            var companyid = await GetAndSetCompanyIdAsync();

            return await _objectRights.CheckObjectRights(objectId: objectId, objectType: objectType, companyId: companyid, userId: userid, referrer: referrer);

            //NOTE: moved to objectrights class: 
            //var sp = string.Concat("check_object_rights_", objectType.ToString().ToLower());

            ////"check_object_rights_action"("_id" int4, "_companyid" int4, "_userid" int4)
            //List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            //parameters.Add(new NpgsqlParameter("@_companyid", companyid));
            //parameters.Add(new NpgsqlParameter("@_id", objectId));
            //parameters.Add(new NpgsqlParameter("@_userid", userid));
            //var ok = Convert.ToBoolean(await _manager.ExecuteScalarAsync(sp, parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            //if(!ok) _logger.LogWarning(message: "Warning, no rights to ObjectId [{0}, {1}] for company [{2}]|user [{3}].", objectId, objectType.ToString(), CompanyId, UserId);

            //return ok;

        }

        /// <summary>
        /// CheckObjectCompanyRights; Check incoming companyId (from object) against user company.
        /// </summary>
        /// <param name="objectCompanyId">CompanyId to check</param>
        /// <param name="objectType">Type to check (not used currenlty)</param>
        /// <returns>true/false if user companyid is the same as object companyid</returns>
        public async Task<bool> CheckObjectCompanyRights(int objectCompanyId, ObjectTypeEnum objectType, [System.Runtime.CompilerServices.CallerMemberName] string referrer = "")
        {
            //var userid = await GetAndSetUserIdAsync();
            var companyid = await GetAndSetCompanyIdAsync(); //user company id
            return await _objectRights.CheckObjectCompanyRights(companyId: objectCompanyId, companyIdToCheck: companyid);
        }

        /// <summary>
        /// CheckAreaRightsForWorkinstruction; Check if the user has enough rights to retrieve work instruction based on the allowed areas of the user an the settings of the work instruction.
        /// Not a replacement for CheckObjectRights.
        /// </summary>
        /// <param name="workInstructionId">Work instruction id to check</param>
        /// <returns>True if user has right to retrieve work instruction</returns>
        public async Task<bool> CheckAreaRightsForWorkinstruction(int workInstructionId)
        {
            int companyId = await GetAndSetCompanyIdAsync();
            int userId = await GetAndSetUserIdAsync();

            var user = await _userManager.GetUserProfileAsync(companyId, userId);

            //only check for basic users as manager and shift leader will aways have access because they have access to the CMS
            if (user != null && user.Role.Equals("basic"))
            {
                return await _objectRights.CheckAreaRightsForWorkinstruction(companyId: companyId, userId: userId, workInstructionId: workInstructionId);
            }

            return true;
        }
        #endregion

        #region - login -
        /// <summary>
        /// AddLoginSecurityLogEvent; Add a security log item.
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="description">Description to add</param>
        /// <param name="eventId">EventId (int)</param>
        /// <param name="type">Type, information. error etc.</param>
        /// <param name="source">Source of request</param>
        /// <returns>true if added.</returns>
        public async Task<bool> AddLoginSecurityLogEvent(string message, string description, int eventId = 0, string type = "INFORMATION", string source = null)
        {
            return await _userManager.AddLoginSecurityLogEvent(message: message, description: description, eventId: eventId, type: type, source: source);
        }

        /// <summary>
        /// AddApplicationLogEvent; Add application log event. 
        /// </summary>
        /// <param name="companyId">CompanyId: company id of user.</param>
        /// <param name="userId">UserId: user id of user.</param>
        /// <param name="userAgent">UserAgent: browser user agent.</param>
        /// <param name="appVersion">AppVersion: app version.</param>
        /// <param name="appOs">AppOS: App OS.</param>
        /// <param name="app">App: app settings.</param>
        /// <param name="ip">Ip of specific user.</param>
        /// <param name="language">Language specific.</param>
        /// <param name="type">Type</param>
        /// <returns>return bool true/false.</returns>
        public async Task<bool> AddApplicationLogEvent(int companyId, int userId, string userAgent, string appVersion, string appOs, string app, string ip, string language, string type = "APP")
        {
            try
            {
                //_companyid,   _userid,    _useragent,         _appversion,    _appos,     _app,   _ip,        _language,  _type = "APP"
                //1	            11	        something something	1.111	        WINDOWS	    WEBAPP	127.0.0.1	nl-nl	    LOGIN	        
                var parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_userid", userId));
                parameters.Add(new NpgsqlParameter("@_useragent", userAgent));
                parameters.Add(new NpgsqlParameter("@_appversion", appVersion));
                parameters.Add(new NpgsqlParameter("@_appos", appOs));
                parameters.Add(new NpgsqlParameter("@_app", app));
                parameters.Add(new NpgsqlParameter("@_ip", ip));
                parameters.Add(new NpgsqlParameter("@_language", language));
                parameters.Add(new NpgsqlParameter("@_type", type));
                //
                await _manager.ExecuteScalarAsync("add_log_app", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                // Error occurred within the logging. Log it to normal logger.
                _logger.LogError(exception: ex, message: string.Concat("ApplicationUser.AddApplicationLogEvent(): ", ex.Message));
            }
            finally
            {
                
            }

            return true;
        }
        #endregion

        #region - custom logging -
        /// <summary>
        /// WriteToDBLog; Write to DB logging, based on general logging item.
        /// This is a custom implementation that uses the normal connection and not the logging 'random' connection. So it's more efficient.
        /// </summary>
        /// <param name="message">Message to add</param>
        /// <param name="description">Description to add</param>
        /// <param name="eventId">EventId (depending on implementation)</param>
        /// <param name="eventName">Event name for reference purposes.</param>
        /// <param name="type">Type, information. error etc.</param>
        /// <param name="source">Source of request</param>
        /// <returns>inserted id.</returns>
        private async Task<int> WriteToDBLog(string message, string type, string eventid, string eventname, string description, string source)
        {

            if (string.IsNullOrEmpty(source)) source = _confighelper.GetValueAsString(ApiSettings.APPLICATION_NAME_CONFIG_KEY);

            NpgsqlCommand cmd = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_message", message));
                parameters.Add(new NpgsqlParameter("@_type", string.IsNullOrEmpty(type) ? "INFORMATION" : type.ToUpper()));
                parameters.Add(new NpgsqlParameter("@_eventid", eventid));
                if (string.IsNullOrEmpty(eventname))
                {
                    parameters.Add(new NpgsqlParameter("@_eventname", ""));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_eventname", eventname));
                }
                if (string.IsNullOrEmpty(source))
                {
                    parameters.Add(new NpgsqlParameter("@_source", ""));
                }
                else
                {
                    parameters.Add(new NpgsqlParameter("@_source", source));
                }
                parameters.Add(new NpgsqlParameter("@_description", description));

                var returnValue = await _manager.ExecuteScalarAsync(procedureNameOrQuery: "add_log", parameters: parameters);

                if (cmd != null) await cmd.DisposeAsync();

                return (int)returnValue;

#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
            }


            return 0;
        }

        #endregion

        #region - external login -
        /// <summary>
        /// ValidateToken; Validate token based on access token and id token. Data from the access token will be used for validating the id token. If validated, data for the access token can be used for validating.
        /// </summary>
        /// <param name="accessToken">Access Token containing data to validate the id token.</param>
        /// <param name="idToken">Id Token used to validate</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> ValidateToken(string accessToken, string idToken)
        {
            try
            {
                string tennant = string.Empty;

                var baseHandler = new JwtSecurityTokenHandler();
                var currentAccessJwtToken = baseHandler.ReadJwtToken(accessToken); //create a jwt token object so claims if needed can be read.
                if (currentAccessJwtToken != null)
                {
                    tennant = currentAccessJwtToken.GetClaim("tid");
                }

                var currentIdJwtToken = baseHandler.ReadJwtToken(idToken); //create a jwt token object so claims if needed can be read.

                if (currentIdJwtToken == null || currentAccessJwtToken == null)
                {
                    return false;
                }

                if(currentAccessJwtToken.GetClaim("oid") != currentIdJwtToken.GetClaim("oid") || currentAccessJwtToken.GetClaim("tid") != currentIdJwtToken.GetClaim("tid"))
                {
                    return false;
                }

                string discoveryEndpoint = string.Concat("https://login.microsoftonline.com/", tennant, "/v2.0/.well-known/openid-configuration");

                ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(discoveryEndpoint, new OpenIdConnectConfigurationRetriever());

                OpenIdConnectConfiguration config = configManager.GetConfigurationAsync().Result;

                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = currentIdJwtToken.Issuer,
                    ValidAudiences = currentIdJwtToken.Audiences,
                    IssuerSigningKeys = config.SigningKeys,
                    ValidateLifetime = true
                };

                JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();

                SecurityToken jwt;

                var result = tokendHandler.ValidateToken(idToken, validationParameters, out jwt);

                return jwt != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: "Error occurred on LogIn.", exception: ex);

                await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: string.Format("Unsuccessful login; (TOKEN) {0}", ex.Message), eventId: 802, source: "");

                await WriteToDBLog(message: string.Format("Error occurred LogIn() : {0}", ex.Message), type: "", eventid: "", eventname: "", description: ex.StackTrace.ToString(), source: string.Empty);

                return false;
            }
        }
        #endregion

        #region - extra user data -
        /// <summary>
        /// RetrieveUserDataForClaims;
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<UserData> RetrieveUserDataForClaims(int userId, int companyId)
        {
            UserData userData = new UserData();

            if(userData.MediaAccessLocations != null) { userData.MediaAccessLocations = new List<string>(); }

            var mediaStorageTypes = Enum.GetValues(typeof(MediaStorageTypeEnum)).Cast<MediaStorageTypeEnum>();

            foreach (var item in mediaStorageTypes) {
                userData.MediaAccessLocations.Add(item.ToStorageLocation());
            }

            userData.MediaAccessLocations.Add(companyId.ToString());

            await Task.CompletedTask;

            return userData; 
        }

        #endregion

        #region - validate ips -
        /// <summary>
        /// Validates whether the provided IP addresses are authorized for login authentication based on the company's
        /// IP restriction settings.
        /// </summary>
        /// <remarks>This method checks if the IP restriction feature is enabled for the specified
        /// company. If enabled,  it validates the provided IP addresses against the list of allowed IPs configured for
        /// the company.  If the feature is disabled or no allowed IPs are configured, the method permits all
        /// IPs.</remarks>
        /// <param name="companyId">The unique identifier of the company for which the IP validation is performed.</param>
        /// <param name="possibleIps">A comma-separated string or json array of IP addresses to validate against the company's allowed IP addresses.</param>
        /// <returns><see langword="true"/> if the IP restriction feature is disabled, no allowed IPs are configured,  or at
        /// least one of the provided IP addresses matches the allowed IPs; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> ValidateIpForLoginAuthentication(int companyId, string possibleIps)
        {
            await _userManager.AddLoginSecurityLogEvent(message: "Validate ip login", description: string.Format("IP Restriction ({0}); possibleIps:{1};", companyId, possibleIps), eventId: 806, source: "");

            //if no ips are given, return true, no checks need to be done.
            if (string.IsNullOrEmpty(possibleIps))
            {
                return true;
            }

            //ip check not enabled, return true
            if (!_confighelper.GetValueAsBool("AppSettings:EnableIpCheck"))
            {
                return true;
            }

            //if feature not enabled, return true, no checks need to be done. 
            if (!await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: "FEATURE_IP_RESTRICTION")) {
                return true;
            }

            var possibleValidIps = await _generalManager.GetSettingValueForCompanyOrHoldingByResourceId(companyid: companyId, resourcesettingid: 131);

            //if no ips are set, return true, no checks need to be done.
            if (string.IsNullOrEmpty(possibleValidIps))
            {
                return true;
            }
           
            var possibleValidIpList = possibleValidIps.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            //check if possible ips is json array or comma separated list.
            var possibleIpList = possibleIps.Contains("{") || possibleIps.Contains("[") ? possibleIps.ToObjectFromJson<List<string>>() : possibleIps.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
          
            if(possibleIpList != null && possibleIpList.Any())
            {
                foreach (var possibleIp in possibleIpList)
                {
                    //if found return true
                    if (possibleValidIpList.Contains(possibleIp))
                    {
                        return true;
                    }
                }            
            }
            else
            {
                return true; //no ip to check, could not be retrieved, continue as normal
            }

            await _userManager.AddLoginSecurityLogEvent(message: "Unsuccessfull login", description: string.Format("IP Restriction not met ({0}); valid:{1}; invalid:{2}", companyId, possibleIps, possibleValidIps), eventId: 806, source: "");

            return false;
        }
        #endregion
    }
}
