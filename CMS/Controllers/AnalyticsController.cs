using EZGO.Api.Models.Settings;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.User;

namespace WebApp.Controllers
{
    public class AnalyticsController : BaseController
    {

        private readonly ILogger<SkillsController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;
        private ApplicationSettings _applicationSettings;

        public AnalyticsController(ILogger<SkillsController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/analytics/logdata")]
        [HttpPost]
        public async Task<IActionResult> LogData([FromBody] Dictionary<string, object>[] dataLogArray)
        {
            //check for disabling logging, due to configuration to full uri, locally can be disabled by changing the appsettings:enableanalytics.
            if(_configurationHelper.GetValueAsBool("AppSettings:EnableAnalyticsCalls")) 
            {
                _applicationSettings ??= await GetApplicationSettings();
                bool analyticsEnabled = _applicationSettings.Features.AnalyticsEnabled ?? false;
                string endpoint = _applicationSettings.AnalyticsLocationUri;
                UserProfile currentUser = null;

                //if for any reason this method is called while analytics is disabled, return 503
                if (!analyticsEnabled)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "Analytics is disabled");
                }

                //get the current user
                var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
                if (!string.IsNullOrWhiteSpace(userprofile))
                {
                    currentUser = JsonConvert.DeserializeObject<UserProfile>(userprofile);
                }

                if (currentUser != null)
                {
                    foreach (Dictionary<string, object> dataLog in dataLogArray)
                    {
                        string jsonDataLog = dataLog.ToJsonFromObject();

                        //add UserId and CompanyId to the log entry object
                        dataLog.Add("UserId", currentUser.Id);
                        dataLog.Add("CompanyId", currentUser.Company.id);
                    }
                }

                var result = await _connector.PostCall(endpoint, dataLogArray.ToJsonFromObject());
            }
            return Ok();
        }
    }
}
