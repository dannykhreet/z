using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.General;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings;
using EZGO.Api.Utils;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Cleaners;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// AuthenticationController; contains all routes based on authentication.
    /// All logout, login and validate token parts are located within this controller.
    /// </summary>
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class AuthenticationController : BaseController<AuthenticationController>
    {
        #region --
        private readonly IMemoryCache _cache;
        private readonly IAwsSecurityTokenStore _awsSecurityTokenStore;
        #endregion

        #region - constructor(s) -
        public AuthenticationController(ILogger<AuthenticationController> logger, IGeneralManager generalManager, IAwsSecurityTokenStore awsSecurityTokenStore, IConfigurationHelper configurationHelper, IApplicationUser applicationUser, IMemoryCache memoryCache) : base(logger, generalManager, applicationUser, configurationHelper)
        {
            _cache = memoryCache;
            _awsSecurityTokenStore = awsSecurityTokenStore;
        }
        #endregion

        #region - routes -
        /// <summary>
        /// Login; Main access route for getting acc token.
        /// NOTE! there is a specific hammer protection based on the UserName, depending on settings only a number of times within a certain time frame a login request for a specific UserName can be done.
        /// </summary>
        /// <param name="login">Login object based on <see cref="Login">Login</see> model within the API models.</param>
        /// <returns>Security token (JWT for now)</returns>
        [AllowAnonymous]
        [Route("authentication/login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]Login login)
        {
            var username = login.UserName;
            var password = login.Password;
            var source = (this.IsAppRequest) ? "APP" : (this.IsCmsRequest) ? "CMS" : "";

            if (!UserValidators.ValidateUserLogin(username: username, password: password))
            {
                if (string.IsNullOrEmpty(username))
                {
                    await _applicationuser.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: "Unsuccessful login; Empty username", eventId: 803, source: source);
                }
                else
                {
                    await _applicationuser.AddLoginSecurityLogEvent(message: "Unsuccessful login", description: "Unsuccessful login; Empty password", eventId: 804, source: source);
                }

                return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorDetails()
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = AuthenticationSettings.MESSAGE_FAILED_LOGIN
                }.ToJsonFromObject());
            }

            if (ProtectionHelper.IsHammering(memoryCache: _cache, hammerCheck: username))
            {
                await _applicationuser.AddLoginSecurityLogEvent(message: "Unsuccessful login too many attempts", description: string.Format("Unsuccessful login; Too many attempts are being done. ({0})", UserCleaner.CleanUserNameForDisplay(username)), eventId: 805, source: source);

                return StatusCode((int)HttpStatusCode.TooManyRequests, new ErrorDetails()
                {
                    StatusCode = (int)HttpStatusCode.TooManyRequests,
                    Message = ProtectionHelper.HAMMER_MESSAGE
                }.ToJsonFromObject());
            }

            var result = await _applicationuser.LoginAndGetSecurityToken(username, password, isCmsLogin: this.IsCmsRequest, isAppLogin: this.IsAppRequest, ips: this.RetrieveIp());
            if(result != null && result.IsValidLogin && !string.IsNullOrEmpty(result.Token))
            {
                if(_configurationHelper.GetValueAsBool("AppSettings:ApplicationValidationEnabled"))
                {
                    if (!await this.HasCompanyHasAccessToApplication(companyId: result.CompanyId))
                    {
                        return StatusCode((int)HttpStatusCode.MethodNotAllowed, new ErrorDetails()
                        {
                            StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                            Message = AuthenticationSettings.MESSAGE_FAILED_APP_HAS_NO_ACCESS
                        }.ToJsonFromObject());
                    }
                }

                try
#pragma warning disable S2486 // Generic exceptions should not be ignored
#pragma warning disable CS0168 // Variable is declared but never used
                {
                    var appInfo = RetrieveApplicationInfoFromHeader();
                    await _applicationuser.AddApplicationLogEvent(companyId: await _applicationuser.GetAndSetCompanyIdAsync(),
                                                                  userId: await _applicationuser.GetAndSetUserIdAsync(),
                                                                  userAgent: appInfo.UserAgent,
                                                                  appVersion: appInfo.Version,
                                                                  appOs: appInfo.OperatingSystem,
                                                                  app: appInfo.App,
                                                                  ip: RetrieveIp(),
                                                                  language: appInfo.Language,
                                                                  type: "LOGIN");
                } catch (Exception ex)
                {

                }
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore S2486 // Generic exceptions should not be ignored

                return StatusCode((int)HttpStatusCode.OK, (result.Token).ToJsonFromObject());
            } else
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorDetails()
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = AuthenticationSettings.MESSAGE_FAILED_LOGIN
                }.ToJsonFromObject());
            }

        }

        /// <summary>
        /// External login; Used for getting token through a external login. Depending on the source (e.g. MSAL, Google) will be validated through several structures.
        /// </summary>
        /// <param name="externaltokens">JWT token containing external information. Based on external login structure.</param>
        /// <returns>Internal access token also based on JWT</returns>
        [AllowAnonymous]
        [Route("authentication/external/login")]
        [HttpPost]
        public async Task<IActionResult> AutoLogin([FromBody] string externaltokens)
        {
            LoggedIn result = new LoggedIn();
            if(string.IsNullOrEmpty(externaltokens))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorDetails()
                { 
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = AuthenticationSettings.MESSAGE_FAILED_LOGIN
                }.ToJsonFromObject());
            } else if (_configurationHelper.GetValueAsInteger("AppSettings:ExternalLoginVersion") == 2) //login version 2 or json so automatically do 2. 
            {
                var loging = externaltokens.ToObjectFromJson<ExternalLogin>();
                result = await _applicationuser.LoginAndGetSecurityToken(external: loging, isCmsLogin: this.IsCmsRequest, isAppLogin: this.IsAppRequest, ips: this.RetrieveIp());
            } else
            {
                result = await _applicationuser.LoginAndGetSecurityToken(external: new ExternalLogin() { AccessToken = externaltokens }, isCmsLogin: this.IsCmsRequest, isAppLogin: this.IsAppRequest, ips: this.RetrieveIp());
            }

            if (result != null && result.IsValidLogin && !string.IsNullOrEmpty(result.Token))
            {
                if (_configurationHelper.GetValueAsBool("AppSettings:ApplicationValidationEnabled"))
                {
                    if (!await this.HasCompanyHasAccessToApplication(companyId: result.CompanyId))
                    {
                        return StatusCode((int)HttpStatusCode.MethodNotAllowed, new ErrorDetails()
                        {
                            StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                            Message = AuthenticationSettings.MESSAGE_FAILED_APP_HAS_NO_ACCESS
                        }.ToJsonFromObject());
                    }
                }

#pragma warning disable CS0168 // Variable is declared but never used
                try
#pragma warning disable S2486 // Generic exceptions should not be ignored
                {
                    var appInfo = RetrieveApplicationInfoFromHeader();
                    await _applicationuser.AddApplicationLogEvent(companyId: await _applicationuser.GetAndSetCompanyIdAsync(),
                                                                  userId: await _applicationuser.GetAndSetUserIdAsync(),
                                                                  userAgent: appInfo.UserAgent,
                                                                  appVersion: appInfo.Version,
                                                                  appOs: appInfo.OperatingSystem,
                                                                  app: appInfo.App,
                                                                  ip: RetrieveIp(),
                                                                  language: appInfo.Language,
                                                                  type: "LOGIN EXTERNAL");
                }
                catch (Exception ex)
                {

                }
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore S2486 // Generic exceptions should not be ignored

                return StatusCode((int)HttpStatusCode.OK, (result.Token).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorDetails()
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = AuthenticationSettings.MESSAGE_FAILED_LOGIN
                }.ToJsonFromObject());
            }
        }

        /// <summary>
        /// <see cref="AutoLogin">AutoLogin method</see> above. This is a overload for that method for directly parsing the external login object. 
        /// </summary>
        /// <param name="externaltokens"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("authentication/external/login/auto")]
        [HttpPost]
        public async Task<IActionResult> AutoLoginV2([FromBody] ExternalLogin externaltokens)
        {
            LoggedIn result = new LoggedIn();
            if (string.IsNullOrEmpty(externaltokens.AccessToken) || string.IsNullOrEmpty(externaltokens.IdToken))
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorDetails()
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = AuthenticationSettings.MESSAGE_FAILED_LOGIN
                }.ToJsonFromObject());
            }
            else //login version 2 or json so automatically do 2. 
            {
                var loging = externaltokens;
                result = await _applicationuser.LoginAndGetSecurityToken(external: loging, isCmsLogin: this.IsCmsRequest, isAppLogin: this.IsAppRequest, ips: this.RetrieveIp());
            }

            if (result != null && result.IsValidLogin && !string.IsNullOrEmpty(result.Token))
            {
                if (_configurationHelper.GetValueAsBool("AppSettings:ApplicationValidationEnabled"))
                {
                    if (!await this.HasCompanyHasAccessToApplication(companyId: result.CompanyId))
                    {
                        return StatusCode((int)HttpStatusCode.MethodNotAllowed, new ErrorDetails()
                        {
                            StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                            Message = AuthenticationSettings.MESSAGE_FAILED_APP_HAS_NO_ACCESS
                        }.ToJsonFromObject());
                    }
                }

#pragma warning disable CS0168 // Variable is declared but never used
                try
#pragma warning disable S2486 // Generic exceptions should not be ignored
                {
                    var appInfo = RetrieveApplicationInfoFromHeader();
                    await _applicationuser.AddApplicationLogEvent(companyId: await _applicationuser.GetAndSetCompanyIdAsync(),
                                                                  userId: await _applicationuser.GetAndSetUserIdAsync(),
                                                                  userAgent: appInfo.UserAgent,
                                                                  appVersion: appInfo.Version,
                                                                  appOs: appInfo.OperatingSystem,
                                                                  app: appInfo.App,
                                                                  ip: RetrieveIp(),
                                                                  language: appInfo.Language,
                                                                  type: "LOGIN EXTERNAL");
                }
                catch (Exception ex)
                {

                }
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore S2486 // Generic exceptions should not be ignored

                return StatusCode((int)HttpStatusCode.OK, (result.Token).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorDetails()
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = AuthenticationSettings.MESSAGE_FAILED_LOGIN
                }.ToJsonFromObject());
            }
        }

        [AllowAnonymous]
        [Route("authentication/external/check")]
        [HttpPost]
        public async Task<IActionResult> AutoLoginCheck([FromBody] string username)
        {
            string result = await _applicationuser.GetCheckAndGetExternalLogin(username: username);
            if (!string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotAcceptable, (false).ToJsonFromObject());
            }

        }

        [AllowAnonymous]
        [Route("authentication/logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await Task.CompletedTask;
            //_logger.LogInformation("logout");
            var result = true;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize]
        [Route("authentication/checkpassword")]
        [HttpPost]
        public async Task<IActionResult> CheckPassword([FromBody] string password)
        {
            await Task.CompletedTask;
            var result = PasswordValidators.PasswordIsValid(password);
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());

        }

        [Authorize]
        [Route("authentication/check")]
        [HttpPost]
        public async Task<IActionResult> AuthenticationCheck()
        {
            //Get ids to check if token data is correct.
            try
            {
                var companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
                var userId = await this.CurrentApplicationUser.GetAndSetUserIdAsync();
                if (companyId > 0 && userId > 0)
                {
                    return StatusCode((int)HttpStatusCode.OK);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized);
                }

#pragma warning disable CS0168 // Variable is declared but never used
            } catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
        }

        [Authorize]
        [Route("authentication/fetchmediatoken")]
        [HttpPost]
        public async Task<IActionResult> AuthenticationFetchTestToken([FromBody] string body)
        {
            //ignore body for now.
            try
            {
                //Get ids to check if token data is correct.
                var companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
                var userId = await this.CurrentApplicationUser.GetAndSetUserIdAsync();
                //If ids not larger 0 e.g. can not be retrieved, something wrong, do not continue. 
                if (companyId > 0 && userId > 0)
                {
                    var result = await _awsSecurityTokenStore.FetchMediaToken();
                    return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized);
                }

#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                _logger.LogWarning(ex, ex.Message);
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// Validates the provided IP address list for login authentication.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized. It checks the
        /// validity of the provided IP address list against the user's company and login authentication rules. If no IP
        /// list is provided, the caller’s IP address is retrieved and validated.</remarks>
        /// <param name="connectedIpList">A comma-separated list of IP addresses to validate. If null or empty, the caller's IP address is used.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the validation: <list type="bullet">
        /// <item><description><see cref="StatusCodeResult"/> with status code 200 (OK) if the IP address is
        /// valid.</description></item> <item><description><see cref="StatusCodeResult"/> with status code 403
        /// (Forbidden) if the IP address is invalid.</description></item> <item><description><see
        /// cref="StatusCodeResult"/> with status code 401 (Unauthorized) if the user or company ID cannot be
        /// determined.</description></item> </list></returns>
        [Authorize]
        [Route("authentication/ipcheck")]
        [HttpPost]
        public async Task<IActionResult> AuthenticationIpCheck([FromBody] string connectedIpList)
        {
            //Get ids to check if token data is correct.
            try
            {
                var companyId = await this.CurrentApplicationUser.GetAndSetCompanyIdAsync();
                var userId = await this.CurrentApplicationUser.GetAndSetUserIdAsync();
                if (companyId > 0 && userId > 0)
                {
                    if (await _applicationuser.ValidateIpForLoginAuthentication(companyId, string.IsNullOrEmpty(connectedIpList) ? this.RetrieveIp() : connectedIpList))
                    {
                        return StatusCode((int)HttpStatusCode.OK);
                    } else
                    {
                        return StatusCode((int)HttpStatusCode.Forbidden);
                    }  
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized);
                }

#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                _logger.LogError(ex, "Error in AuthenticationIpCheck");
                return StatusCode((int)HttpStatusCode.OK);
            }
        }
        #endregion
    }
}