using EZGO.Api.Models.General;
using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using EZGO.CMS.LIB.Utils;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models.Authentication;
using WebApp.ViewModels;
using WebApp.ViewModels.Authentication;

namespace WebApp.Controllers
{
    /// <summary>
    /// AuthenticationController; contains routes for logging in and out of the application.
    /// NOTE! this is a demo controller, when used for production make sure you remove the basic logic and checks to a service/manager class and add proper logging.
    /// </summary>
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _language;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public AuthenticationController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IApiConnector connector, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, ILanguageService language, IConfigurationHelper configurationHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _connector = connector;
            _language = language;
            _configurationHelper = configurationHelper;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }


        [HttpGet]
        [Route("login")]
        public async Task<IActionResult> Login()
        {
            await Task.CompletedTask;

            string culture = _httpContextAccessor.HttpContext.Request.Cookies["language"];
            string currentApiConnectionKey = _httpContextAccessor.HttpContext.Request.Cookies["connectionkey"];

            if (string.IsNullOrWhiteSpace(culture))
            {
                var culturereq = _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>();
                culture = culturereq?.RequestCulture?.Culture.Name;
            }

            LoginViewModel model = new LoginViewModel();
            model.Languages = await GetLanguages(culture);
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(culture);
            model.UseApiKey = _configurationHelper.GetValueAsBool("AppSettings:EnableApiConnectionKey"); // string.IsNullOrEmpty(currentApiConnectionKey);
            model.UseExternalLogin = _configurationHelper.GetValueAsBool("AppSettings:EnableExternalLogin"); // string.IsNullOrEmpty(currentApiConnectionKey);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View(model);
        }

        [HttpGet]
        [Route("account/accessdenied")]
        public async Task<IActionResult> AccessDenied()
        {
            var output = new AuthenticationViewModel();

            output.Locale = GetCookie(Constants.General.LANGUAGE_COOKIE_STORAGE_KEY) ?? "en-US";
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(output.Locale);

            await Task.CompletedTask;

            return View("~/Views/Authentication/AccessDenied.cshtml", output);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> AuthenticateLogin(LoginViewModel model)
        {
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(model.Locale);
            //handle Authentication, no for this demo implementation no validation checks will be done.
            if (ModelState.IsValid)
            {

                if (!string.IsNullOrEmpty(model.Locale))
                    SetCookie(Constants.General.LANGUAGE_COOKIE_STORAGE_KEY, model.Locale);

                if (!string.IsNullOrEmpty(model.ApiConnectionKey)) SetCookie("connectionkey", model.ApiConnectionKey); //check existance of API connection key

                //create valid object for login procedure api. (based on models of API)
                var apiLogin = new EZGO.Api.Models.Authentication.Login() { Password = model.LoginPassword, UserName = model.LoginName };

                //json that is going to be send to the api: "{\"username\":\"User.Basic.61\",\"password\":\"HSCzc5ah\"}"
                var loginJWTResponse = await _connector.PostCall("v1/authentication/login", apiLogin.ToJsonFromObject());
                if (loginJWTResponse.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(loginJWTResponse.Message))
                {
                    bool loginSucces = await HandleLogin(message: loginJWTResponse.Message.ToString(), locale: model.Locale);
                    if (loginSucces)
                    {
                        model.LoggedIn = true;

                        Response.Cookies.Append("logged_in", DateTime.Now.ToString(), new CookieOptions() { Expires = DateTime.Now.AddDays(1), Path = "/" });
                        Response.Cookies.Delete("logged_out");

                        if (Request.QueryString.HasValue && !string.IsNullOrEmpty(Request.Query["ReturnUrl"]))
                        {
                            if (Uri.IsWellFormedUriString(Request.Query["ReturnUrl"], UriKind.Relative) && TextValidators.ValidateUriStringOnRogueData(text: Request.Query["ReturnUrl"]) && IsValidRedirectRoute(Request.Query["ReturnUrl"]))
                            {
                                return Redirect(Request.Query["ReturnUrl"]);
                            }
                            else
                            {
                                return Redirect("/dashboard");
                            }
                        }
                        else
                        {
                            return Redirect("/dashboard");
                        }
                    }
                    else
                    {
                        model.LoggedIn = false;
                        return Redirect("/account/accessdenied");
                    }
                }
                else if (loginJWTResponse.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
                {
                    var errorDetails = loginJWTResponse.Message.ToString().ToObjectFromJson<ErrorDetails>();
                    model.Message = errorDetails.Message.ToString();
                }
                else
                {
                    model.Message = (model.CmsLanguage?.GetValue(LanguageKeys.Authentication.UnknownUsernameOrPassword, "Unknown user name or password.") ?? "Unknown user name or password.");
                }
            }
            else
            {
                model.Message = (model.CmsLanguage?.GetValue(LanguageKeys.Authentication.EmptyUsernameOrPassword, "Please enter username and password.") ?? "Please enter username and password.");
            }

            var culturereq = _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = culturereq?.RequestCulture?.Culture.Name;

            model.Languages = await GetLanguages(model.Locale ?? culture);

            if (!string.IsNullOrEmpty(model.Locale))
                SetCookie(Constants.General.LANGUAGE_COOKIE_STORAGE_KEY, model.Locale);

            return View("Login", model);
        }

        [Route("logout")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        { 
            //SignOut from the context when calling the logout route.
            string culture = _httpContextAccessor.HttpContext.Request.Cookies["language"];
            string currentApiConnectionKey = _httpContextAccessor.HttpContext.Request.Cookies["connectionkey"];

            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                // do nothign, user signin already failed, so no need to remove it. 
            }

            LoginViewModel model = new LoginViewModel();
            model.Languages = await GetLanguages(culture);
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(culture);
            model.UseApiKey = _configurationHelper.GetValueAsBool("AppSettings:EnableApiConnectionKey"); // string.IsNullOrEmpty(currentApiConnectionKey);
            model.UseExternalLogin = _configurationHelper.GetValueAsBool("AppSettings:EnableExternalLogin"); // string.IsNullOrEmpty(currentApiConnectionKey);

            Response.Cookies.Delete("logged_in");
            Response.Cookies.Append("logged_out", DateTime.Now.ToString(), new CookieOptions() { Expires = DateTime.Now.AddDays(1), Path = "/" });

            return View("Logout", model);
        }

        [Route("token")]
        [HttpGet]
        public IActionResult Token([FromServices] IAntiforgery antiForgery)
        {
            var tokens = antiForgery.GetAndStoreTokens(HttpContext);
            return PartialView("~/Views/Shared/_logout_form.cshtml");
        }

        [HttpGet]
        [Route("check/activitystatus")]
        public async Task<IActionResult> CheckActivityStatus()
        {
            var returnedCode = await _connector.CheckConnectorAuthorized();
            if (returnedCode == HttpStatusCode.OK)
            {
                return StatusCode((int)HttpStatusCode.OK, "Just checking.");
            }
            else
            {
                return StatusCode((int)returnedCode, "Possible issue occurred.");
            }
        }

        [NonAction]
        /// <summary>
        /// GetCookie; Get cookie value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetCookie(string key)
        {
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request != null && _httpContextAccessor.HttpContext.Request.Cookies != null)
            {
                return _httpContextAccessor.HttpContext.Request.Cookies[key];
            }
            else return string.Empty;
        }
        #region - external logins -
        [HttpGet]
        [Route("login/consent")]
        public async Task<IActionResult> LoginConsent()
        {
            string culture = _httpContextAccessor.HttpContext.Request.Cookies["language"];

            MSALViewModel model = new MSALViewModel();
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(culture);
            model.ExternalRedirectUrl = _configurationHelper.GetValueAsString("AppSettings:ExternalLoginRedirectUrl");
            return View("~/Views/Authentication/MSAL/Consent.cshtml", model);
        }

        [HttpGet]
        [Route("login/external")]
        public async Task<IActionResult> LoginMsal()
        {
            string culture = _httpContextAccessor.HttpContext.Request.Cookies["language"];

            if (string.IsNullOrWhiteSpace(culture))
            {
                var culturereq = _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>();
                culture = culturereq?.RequestCulture?.Culture.Name;
            }

            MSALViewModel model = new MSALViewModel();
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(culture);
            model.ExternalRedirectUrl = _configurationHelper.GetValueAsString("AppSettings:ExternalLoginRedirectUrl");
            return View("~/Views/Authentication/MSAL/Index.cshtml", model);
        }

        [HttpPost]
        [Route("login/external")]
        public async Task<IActionResult> AuthenticateLoginExternal(ExternalLogin login)
        {
            string culture = _httpContextAccessor.HttpContext.Request.Cookies["language"];

            try
            {
                if (!ValidateToken(accessToken: login.AccessToken, idToken: login.IdToken, userName: login.UserName, tennantId: login.ExternalIdentifier1, uniqueId: login.ExternalIdentifier2))
                {
                    return View("~/Views/Authentication/AccessDenied.cshtml");
                }
                ;

                //TODO; NOTE; When login has changed to full object (e.g. not a json string but a json object) change url to v1/authentication/external/login/auto
                var loginJWTResponse = await _connector.PostCall("v1/authentication/external/login", _configurationHelper.GetValueAsInteger("AppSettings:ExternalLoginVersion") == 2 ? login.ToJsonFromObject().ToString().ToJsonFromObject() : login.AccessToken.ToJsonFromObject()); ;
                if (loginJWTResponse.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(loginJWTResponse.Message))
                {
                    bool loginSucces = await HandleLogin(message: loginJWTResponse.Message.ToString(), locale: "");
                    if (loginSucces)
                    {
                        Response.Cookies.Append("logged_in", DateTime.Now.ToString(), new CookieOptions() { Expires = DateTime.Now.AddDays(1), Path = "/" });
                        Response.Cookies.Delete("logged_out");

                        if (Request.QueryString.HasValue && !string.IsNullOrEmpty(Request.Query["ReturnUrl"]) && IsValidRedirectRoute(Request.Query["ReturnUrl"]))
                        {
                            return Redirect(Request.Query["ReturnUrl"]);
                        }
                        else
                        {
                            return Redirect("/dashboard");
                        }
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
            }
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }

            return View("~/Views/Authentication/AccessDenied.cshtml");

        }

        #endregion
        [NonAction]
        private async Task<List<SelectListItem>> GetLanguages(string culture = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                var culturereq = _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>();
                culture = culturereq?.RequestCulture?.Culture.Name;
            }

            if (string.IsNullOrWhiteSpace(culture)) { culture = null; }

            List<SelectListItem> result = await _language.GetLanguageSelectorItems();
            result ??= new List<SelectListItem>();
            result.ForEach(x => { x.Selected = (x.Value.ToLower() == (culture ?? "en-US").ToLower()); });
            return result;
        }

        /// <summary>
        /// HandleLogin; Handle login structure based on login response message from the API.
        /// Method created a .net login based on claims and signs the user in based on data from the API.
        /// </summary>
        /// <param name="message">Response message from API, contains a access key (based on JWT)</param>
        /// <param name="locale">Locale used for setting the language. </param>
        /// <returns>true/false if login succeeded</returns>
        [NonAction]
        private async Task<bool> HandleLogin(string message, string locale)
        {
            //deserialize the response
            var token = System.Text.Json.JsonSerializer.Deserialize<string>(message);

            var handler = new JwtSecurityTokenHandler();
            var jwtObject = handler.ReadJwtToken(token); //create a jwt token object so claims if needed can be read.

            var validateIpResult = await _connector.PostTokenCall("/v1/authentication/ipcheck", token, this.RetrieveIp().ToJsonFromObject());
            if (validateIpResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }

            //get user profile with company data
            var profileResult = await _connector.PostTokenCall("/v1/userprofile?include=company", token, token.ToJsonFromObject());

            var appSettings = new ApplicationSettings();
            var settingsResult = await _connector.GetCall(Logic.Constants.AppSettings.ApplicationSettingsUri, token);
            if (settingsResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                appSettings = JsonConvert.DeserializeObject<ApplicationSettings>(settingsResult.Message);
            }

            //creating set of claims for cookie token authentication and adding the jwt token as internal claim.
            var claims = new List<Claim>
                    {
                      new Claim(ClaimTypes.Name, jwtObject.GetClaim("unique_name")),
                      new Claim(ClaimTypes.Email, jwtObject.GetClaim("email")),
                      new Claim(ClaimTypes.Sid, token), //TODO add encryption to API jwt token.
                      new Claim(ClaimTypes.Locality, locale),
                      new Claim(ClaimTypes.UserData, profileResult.Message),
                      new Claim(ClaimTypes.Country, appSettings?.CompanyTimezone ?? "Europe/Amsterdam")
                    };

            //add roles to claims, can be more then 1 role.
            foreach (Claim claim in jwtObject.Claims)
            {
                if (claim.Type == "role")
                {
                    claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                }
                if (claim.Type == "guid")
                {
                    claims.Add(new Claim("guid", claim.Value));
                }
                if (claim.Type == "syncguid")
                {
                    claims.Add(new Claim("syncguid", claim.Value));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties();
            //TODO possible add the jwt expiration data to the cookie auth property so they expire at the same time.


            if (jwtObject.GetClaim("role", "basic") == "basic")
            {
                //model.LoggedIn = false;
                foreach (var cookieKey in _httpContextAccessor.HttpContext.Request.Cookies.Keys)
                {
                    if (cookieKey != "language")
                    {
                        Response.Cookies.Delete(cookieKey);
                    }
                }
                return false;
            }
            else
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return true;
            }
        }


        /// <summary>
        /// set the cookie
        /// </summary>
        /// <param name="key">key (unique indentifier)</param>
        /// <param name="value">value to store in cookie object</param>
        /// <param name="expireTime">expiration time</param>
        [NonAction]
        private void SetCookie(string key, string value, int? expireTime = null)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Today.AddMonths(1);

            option.SameSite = SameSiteMode.Strict;

            Response.Cookies.Append(key, value, option);
        }

        /// <summary>
        /// Delete the key
        /// </summary>
        /// <param name="key">Key</param>
        [NonAction]
        private void RemoveCookie(string key)
        {
            Response.Cookies.Delete(key);
        }

        /// <summary>
        /// ValidateToken; Validate token based on access token and id token. Data from the access token will be used for validating the id token. If validated, data for the access token can be used for validating.
        /// </summary>
        /// <param name="accessToken">Access Token containing data to validate the id token.</param>
        /// <param name="idToken">Id Token used to validate</param>
        /// <returns>true/false depending on outcome.</returns>
        [NonAction]
        private bool ValidateToken(string accessToken, string idToken, string userName, string tennantId, string uniqueId)
        {
            try
            {
                string tennant = string.Empty;
                string oid = string.Empty;
                string upn = string.Empty;

                var baseHandler = new JwtSecurityTokenHandler();
                var currentAccessJwtToken = baseHandler.ReadJwtToken(accessToken); //create a jwt token object so claims if needed can be read.
                if (currentAccessJwtToken != null)
                {
                    tennant = currentAccessJwtToken.GetClaim("tid");
                    oid = currentAccessJwtToken.GetClaim("oid");
                    upn = currentAccessJwtToken.GetClaim("upn");
                }

                if (currentAccessJwtToken == null || upn != userName || tennantId != tennant || oid != uniqueId)
                {
                    return false;
                }

                if (_configurationHelper.GetValueAsInteger("AppSettings:ExternalLoginVersion") == 2)
                {
                    var currentIdJwtToken = baseHandler.ReadJwtToken(idToken); //create a jwt token object so claims if needed can be read.

                    if (currentIdJwtToken == null)
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
                        ValidateLifetime = false
                    };

                    JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();

                    SecurityToken jwt;

                    var result = tokendHandler.ValidateToken(idToken, validationParameters, out jwt);

                    return jwt != null;
                }
                else
                {
                    return true;
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return false;
            }

        }

        [NonAction]
        private bool IsValidRedirectRoute(string possibleRoute)
        {
            //Add caching
            var routes = GetAvailableRedirectRoutesList(useOnlyStartWithUntilParam: true);
            foreach (var route in routes)
            {
                if (route.StartsWith(possibleRoute))
                {
                    return true;
                }

            }
            return false;

        }

        [NonAction]
        private List<string> GetAvailableRedirectRoutesList(bool useOnlyStartWithUntilParam = false)
        {
            //Add caching
            var routes = new List<string>();
            foreach (var possibleUri in _actionDescriptorCollectionProvider.ActionDescriptors.Items.Where(ad => ad.AttributeRouteInfo != null).Select(ad => ad.AttributeRouteInfo.Template))
            {
                var uri = possibleUri;

                if (uri != "/") { uri = string.Concat("/", uri); }

                if (useOnlyStartWithUntilParam)
                {
                    if (uri.Contains("{"))
                    {
                        //remove everything after the first parameter, only use start part of uri
                        routes.Add(uri.Substring(0, uri.IndexOf("{")));
                    }
                    else
                    {
                        routes.Add(uri);
                    }

                }
                else
                {
                    routes.Add(uri);
                }
            }
            return routes.ToList();
        }

        /// <summary>
        /// Retrieves the IP address or addresses from the request headers, typically used to identify the client IP in
        /// scenarios where the request may pass through proxies or load balancers.
        /// </summary>
        /// <remarks>This method checks for the presence of the "X-Forwarded-For" header
        /// (case-insensitive) in the request headers. If the header is found, it extracts and processes the IP
        /// addresses, handling scenarios where multiple IPs are present (e.g., due to proxy chains). The IPs are
        /// returned as a JSON-formatted string.</remarks>
        /// <returns>A JSON-formatted string containing the extracted IP address or addresses. Returns an empty string if no IP
        /// information is available, or "no data" if the header is present but contains no valid data.</returns>
        [NonAction]
        public string RetrieveIp()
        {
            const string IP_KEY = "X-Forwarded-For";

            if (Request?.Headers != null)
            {
                //check multiple header parameters (due to http 1.0,1.1,2.0,2.1 handling header names differently)
                if (Request.Headers.Keys.Contains(IP_KEY) || Request.Headers.Keys.Contains(IP_KEY.ToLower()) || Request.Headers.Keys.Contains(IP_KEY.ToUpper()))
                {
                    var currentIPs = new List<string>();

                    Microsoft.Extensions.Primitives.StringValues ipHeader = new Microsoft.Extensions.Primitives.StringValues();
                    if (Request.Headers.Keys.Contains(IP_KEY))
                    {
                        ipHeader = Request.Headers[IP_KEY];
                    }
                    else if (Request.Headers.Keys.Contains(IP_KEY.ToLower()))
                    {
                        ipHeader = Request.Headers[IP_KEY.ToLower()];
                    }
                    else if (Request.Headers.Keys.Contains(IP_KEY.ToUpper()))
                    {
                        ipHeader = Request.Headers[IP_KEY.ToUpper()];
                    }

                    if (!ipHeader.Any())
                    {
                        return "no data"; //should not occur, something probably wrong with load balancer.
                    }
                    ;

                    var potentialCollection = ipHeader.ToList();
                    if (potentialCollection != null && potentialCollection.Any())
                    {
                        //checking all IPs, add split construction for handling proxy/multiple proxy servers.
                        foreach (var potentialIPs in potentialCollection)
                        {
                            if (!string.IsNullOrEmpty(potentialIPs))
                            {
                                if (potentialIPs.Contains(","))
                                {
                                    currentIPs.AddRange(potentialIPs.Split(',').Select(x => x.Trim()).ToList());
                                    //multi ips so split and trim
                                }
                                else
                                {
                                    currentIPs.Add(potentialIPs.Trim()); //single ip
                                }
                            }
                        }
                    }

                    return currentIPs.ToJsonFromObject();

                }
            }

            return "";
        }

    }
}
