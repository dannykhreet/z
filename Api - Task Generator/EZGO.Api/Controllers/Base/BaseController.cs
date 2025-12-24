using Elastic.Apm;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models.Tools;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Utils.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EZGO.Api.Controllers.Base
{
    /// <summary>
    /// BaseController(Of T); used for base of each controller in the project.
    /// This class contains a basic route named health that can be called on controller level (for external monitoring), it contains basic functionality for checking if static output is needed and it sets and gets the basic Logger and CurrentApplicationUser.
    ///
    /// Make sure when you implement a new controller that you will add this base class.
    ///
    /// All routes within the controllers must implement the IActionResult as output. So we can add statuses with our responses.
    ///
    /// NOTE! all controllers must inherit from this controller and are therefor default authorized. If unauthorized access is needed add [AllowAnonymous] to the route.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [EnableCors("CorrsPolicy")]
    [Authorize]
    [ApiController]
    public class BaseController<T> : ControllerBase
    {
        #region - privates and properties -
        protected readonly ILogger<T> _logger;
        protected readonly IApplicationUser _applicationuser;
        protected readonly IConfigurationHelper _configurationHelper;
        private readonly IGeneralManager _generalManager;
      

        /// <summary>
        /// UseStaticOutput; calls  <see cref="CheckStaticOutput">CheckStaticOutput</see> which checks the QueryString if static is available and true, if so returns true or false so a static string located in <see cref="EZGO.Api.Settings.StaticOutput">StaticOutput</see>.
        /// </summary>
        public bool UseStaticOutput { get {
                return this.CheckStaticOutput();
            }
        }

        /// <summary>
        /// CurrentApplicationUser; current application used is the user that is currently connecting with the API based on the user Token that is supplied within the headers of the call.
        /// The user contains basic information like its CompanyId.
        /// NOTE! the user is only filled when 1) the token supplied exists. 2) the right constructor is used with implementation.
        /// </summary>
        public IApplicationUser CurrentApplicationUser
        {
            get
            {
                return _applicationuser;
            }
        }

        /// <summary>
        /// IsAppRequest; Checks if request is a request from one of the apps.
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsAppRequest
        {
            get {
                try
                {
                    return (GetAgentHeader().Contains("EZ-GO APP"));
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// IsAppRequest; Checks if request is a request from one of the apps.
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsIosAppRequest
        {
            get
            {
                try
                {
                    return (GetAgentHeader().Contains("EZ-GO APP"));
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// IsAppRequest; Checks if request is a request from one of the apps.
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsAndroidAppRequest
        {
            get
            {
                try
                {
                    return (GetAgentHeader().Contains("EZ-GO APP"));
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// IsWebAppRequest; Checks if request is a request from one of the web apps.
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsWebAppRequest
        {
            get
            {
                try
                {
                    return (GetEzAgentHeader().Contains("EZ-GO WEBAPP"));
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// IsCmsRequest; Checks if request is a request from the cms.
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsCmsRequest
        {
            get {
                try
                {
                    return (GetAgentHeader().Contains("MY EZ-GO")); //TODO make constant
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// IsDashboardRequest; Checks if request is a request from the dashboard.
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsDashboardRequest
        {
            get
            {
                try
                {
                    return (GetEzAgentHeader().Contains("EZ-GO DASHBOARD")); //TODO make constant
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// IsPostManRequest; Checks if request is a request from postman.
        /// </summary>
        /// <returns>true/false</returns>
        public bool IsPostManRequest
        {
            get
            {
                try
                {
                    return (GetAgentHeader().Contains("Postman")); //TODO make constant
                }
                catch
                {
                    return false;
                }
            }
        }

        public string TranslationLanguage
        {
            get
            {
                try
                {
                    return GetLanguageHeader()?.Replace('-','_')?.ToLower();
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool ValidateUserBasedOnCompany
        {
            get
            {
                try
                {
                    
                    var possibleUriCollection = _configurationHelper.GetValueAsString("AppSettings:EnableCustomerWideDataProcessingUris").Split(",").ToList();
                    if(possibleUriCollection.Any())
                    {
                        var possiblePath = Request?.Path.Value;
                        var possibleUriFound = false;
                        foreach( var possibleUri in possibleUriCollection)
                        {
                            if(possiblePath.StartsWith(possibleUri))
                            {
                                possibleUriFound = true;
                            }
                        }
                        
                        if(possibleUriFound)
                        {
                            return _configurationHelper.GetValueAsBool("AppSettings:EnableCustomerWideDataProcessing");
                        }
                    } 
                   
                }
                catch
                {
                    return false;
                }
                return false;
            }
        }
        #endregion

        #region - constructor(s) -
        /// <summary>
        /// BaseController; Implementation for controllers that use a logger and application user (e.g. contain some kind of database connectivity).
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="applicationuser"></param>
        /// <param name="configurationHelper"></param>
        public BaseController(ILogger<T> logger, IApplicationUser applicationuser, IConfigurationHelper configurationHelper)
        {
            this._logger = logger;
            this._applicationuser = applicationuser;
            this._configurationHelper = configurationHelper;
        }

        /// <summary>
        /// BaseController; Implementation for controllers that only use the logger.
        /// </summary>
        /// <param name="logger"></param>
        public BaseController(ILogger<T> logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="generalManager"></param>
        /// <param name="applicationuser"></param>
        /// <param name="configurationHelper"></param>
        public BaseController(ILogger<T> logger, IGeneralManager generalManager, IApplicationUser applicationuser, IConfigurationHelper configurationHelper)
        {
            this._logger = logger;
            this._applicationuser = applicationuser;
            this._generalManager = generalManager;
            this._configurationHelper = configurationHelper;
        }
        #endregion

        #region - routes -
        /// <summary>
        /// Base route, returns the name of the controller when its active and no other routing parameters are used.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("[controller]/health")]
        [HttpGet]
        public async Task<string> Get()
        {
            string controllertype = this.ToString();
            await Task.CompletedTask; //just for making it runnable asynchronously.
            return string.Concat(controllertype.Replace("EZGO.Api.Controllers.", ""), " is active.");
        }
        #endregion

        #region - methods -
        /// <summary>
        /// CheckStaticOutput; check if static output must be generated based on QueryString parameter 'static'
        /// The static output is based on strings, and only used for the development of the API, will probably be removed before release candidate is published.
        /// </summary>
        /// <returns>true/false depending on parameter.</returns>
        private bool CheckStaticOutput()
        {
            if (Request.QueryString.HasValue) {
                if(Request.Query.TryGetValue("static", out var st))
                {
                    return Convert.ToBoolean(st.ToString());
                }
            }
            return false;
        }

        /// <summary>
        /// GetObjectResultWithStatus; Gets a object result based on the supplied object. Depending if object is null a certain status will be returned.
        /// </summary>
        /// <param name="o">The object that needs to be checked and returned</param>
        /// <returns>ObjectResult, containing a status code, and content (as JSON)</returns>
        [NonAction]
        public ObjectResult GetObjectResultJsonWithStatus(object o)
        {
            return StatusCode((int)HttpStatusCode.OK, (o).ToJsonFromObject());
        }

        /// <summary>
        /// GetAgentHeader; Get agent headers
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public string GetAgentHeader()
        {
            if(Request?.Headers != null)
            {
                var agent = Request?.Headers["User-Agent"];
                if(agent.HasValue && agent.Value.Any())
                {
                    return agent.Value.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public string GetEzAgentHeader()
        {
            if (Request?.Headers != null)
            {
                var agent = Request?.Headers["User-Agent-EZ"];
                if (agent.HasValue && agent.Value.Any())
                {
                    return agent.Value.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetDeterminedEzAgentHeader(); Gets the user agent header, by firstly checking the EZ user agent and if not available the normal user agent. 
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public string GetDeterminedEzAgentHeader()
        {
            string ezHeader = GetEzAgentHeader();
            if(string.IsNullOrEmpty(ezHeader))
            {
                ezHeader = GetAgentHeader();
            }
            return ezHeader;
        }

        /// <summary>
        /// GetSyncHeader value
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public string GetSyncGuidHeader()
        {
            if (Request?.Headers != null)
            {
                var syncGuid = Request?.Headers["Sync-GUID"];
                if (syncGuid.HasValue && syncGuid.Value.Any())
                {
                    return syncGuid.Value.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetSyncHeader value
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public string GetUserGuidHeader()
        {
            if (Request?.Headers != null)
            {
                var userGuid = Request?.Headers["User-GUID"];
                if (userGuid.HasValue && userGuid.Value.Any())
                {
                    return userGuid.Value.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetLanguageHeader value
        /// </summary>
        /// <returns>locale string (e.g. "en_us" or "nl_nl"). Null if header is missing</returns>
        [NonAction]
        public string GetLanguageHeader()
        {
            if (Request?.Headers != null)
            {
                var language = Request?.Headers["Language"];
                if (language.HasValue && language.Value.Any())
                {
                    return language.Value.ToString();
                }
                else
                {
                    return "en-us";
                }
            }
            return "en-us";
        }

        /// <summary>
        /// HasCompanyHasAccessToApplication; Check based on headers if company has access through a specific app to the API.
        /// NOTE! method is used, implement the correct base class contructor within the controller.
        /// </summary>
        /// <param name="companyId">CompanyId to be checked.</param>
        /// <returns>true/false depending on outcome.</returns>
        [NonAction]
        public async Task<bool> HasCompanyHasAccessToApplication(int companyId)
        {
            if(companyId > 0)
            {
                string keyToCheck = "";
                if (this.IsIosAppRequest)
                {
                    keyToCheck = "APP_XAMIOS";
                } else if (this.IsAndroidAppRequest)
                {
                    keyToCheck = "APP_XAMANDROID";
                }
                else if (this.IsCmsRequest)
                {
                    keyToCheck = "APP_MYEZGO";
                }
                else if (this.IsWebAppRequest)
                {
                    keyToCheck = "APP_WEB";
                }
                else if (this.IsDashboardRequest)
                {
                    keyToCheck = "APP_DASHBOARD";
                }
                else if (this.IsPostManRequest)
                {
                    keyToCheck = "APP_POSTMAN";
                }

                return await _generalManager.GetHasAccessToFeatureByCompany(companyId: companyId, featurekey: keyToCheck);

                //Not implemented:
                //keyToCheck = "APP_XAMMACOS";
                //keyToCheck = "APP_XAMWINDOWS";
                //keyToCheck = "APP_EZWORKINSTRUCTION";
                //keyToCheck = "APP_EZASSESSMENT";
                //keyToCheck = "APP_EZTASK";
                //keyToCheck = "APP_EZCHECKLIST";
                //keyToCheck = "APP_EZAUDIT";

            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validIPs"></param>
        /// <returns></returns>
        [NonAction]
        public bool CheckIfIPHasAccess(string validIPs)
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
                        return false; //should not occur, something probably wrong with load balancer.
                    };

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

                    if (!string.IsNullOrEmpty(validIPs))
                    {
                        bool ipValid = false;
                        foreach (var ip in validIPs.Split(',').ToList())
                        {
                            //if ip header has valid ip then set ipvalid to true,
                            if (currentIPs.Contains(ip)) ipValid = true;
                        }

                        return ipValid;
                    }

                }
            }
           
            return false;
        }

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
                    };

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

        [NonAction]
        public EZApplicationHeaderInfo RetrieveApplicationInfoFromHeader()
        {
            EZApplicationHeaderInfo appInfo = new EZApplicationHeaderInfo();

            appInfo.Language = GetLanguageHeader();
            appInfo.UserAgent = GetDeterminedEzAgentHeader();
            appInfo.App = RetrieveApplicationName(appInfo.UserAgent);
            appInfo.Version = RetrieveApplicationVersion(appInfo.UserAgent);
            appInfo.OperatingSystem = RetrieveApplicationOS(appInfo.UserAgent);

            return appInfo;
        }

        [NonAction]
        private string RetrieveApplicationName(string userAgent)
        {
            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                if (string.IsNullOrEmpty(userAgent)) return string.Empty;
                if (userAgent.IndexOf("(") > -1)
                {
                    return userAgent.Substring(0, userAgent.IndexOf("(")).Trim();
                }
            } catch(Exception e)
            {
                return string.Empty;
            }
#pragma warning restore CS0168 // Variable is declared but never used
            return string.Empty;
        }

        [NonAction]
        private string RetrieveApplicationVersion(string userAgent)
        {
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
               
                if (string.IsNullOrEmpty(userAgent)) return string.Empty;
                if (userAgent.LastIndexOf("/") > -1)
                {
                    //Get last part of user agent (from last / ) which should contain the version number.
                    return userAgent.Substring(userAgent.LastIndexOf("/") + 1, userAgent.Length - userAgent.LastIndexOf("/") - 1).Trim();
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
#pragma warning restore CS0168 // Variable is declared but never used
            return string.Empty;
        }

        [NonAction]
        private string RetrieveApplicationOS(string userAgent)
        {
            if (!string.IsNullOrEmpty(userAgent))
            {
                /*
                    Windows -> Windows
                    Macintosh -> Macintosh
                    Android -> Android
                    iPhone -> iOS
                    iPad -> iOS
                    Linux (and not android) -> Linux
                    Unix -> Unix
                 */
                if (userAgent.ToLower().Contains("windows"))
                {
                    return "Windows";
                } else if(userAgent.ToLower().Contains("macintosh"))
                {
                    return "Mac OS";
                } else if (userAgent.ToLower().Contains("android"))
                {
                    return "Android";
                } else if (userAgent.ToLower().Contains("iphone"))
                {
                    return "iOS";
                } else if (userAgent.ToLower().Contains("ipad"))
                {
                    return "iOS";
                }
                else if (userAgent.ToLower().Contains("linux"))
                {
                    return "Linux";
                }
                else if (userAgent.ToLower().Contains("unix"))
                {
                    return "Unix";
                }
                else if (userAgent.ToLower().Contains("ios"))
                {
                    return "iOS";
                }
                else 
                {
                    return "Other";
                }
            }
            return "Unknown";
        }

        [NonAction] 
        public void AppendCapturedExceptionToApm(List<Exception> exceptions)
        {
            try
            {
                if (exceptions != null && exceptions.Any())
                {
                    //only add error one time, due to some functionality called in loops errors can occur many times within same call. 
                    //current APM is limited in resources, therefor distinct the exceptions not to overload messages to APM. 
                    foreach (var ex in exceptions.Distinct()) { Agent.Tracer?.CurrentTransaction?.CaptureException(ex); }
                }
            }
            catch (Exception ex) {
                Agent.Tracer?.CurrentTransaction?.CaptureException(ex);
            }
        }
        #endregion

    }
}