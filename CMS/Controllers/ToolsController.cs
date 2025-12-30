using EZGO.Api.Models.Provisioner;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.User;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class ToolsController : BaseController
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public ToolsController(ILogger<CompanyController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;

        }


        //TODO MOVE to own later on
        [HttpGet]
        [Route("/tools/overview")]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("serviceaccount")) {
                return NoContent();
            }
            var output = new ToolsViewModel();

            output.ApplicationSettings = await this.GetApplicationSettings();
            output.IsAdminCompany = this.IsAdminCompany;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.TOOLS;

            return View("~/Views/Tools/Index.cshtml", output);
        }

        [HttpGet]
        [Route("/tools/provisioner/overview")]
        public async Task<IActionResult> ProvisionerOverview()
        {
            if (!User.IsInRole("serviceaccount"))
            {
                return NoContent();
            }
            var output = new ToolsViewModel();

            output.ApplicationSettings = await this.GetApplicationSettings();
            output.IsAdminCompany = this.IsAdminCompany;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.TOOLS;

            return View("~/Views/Tools/Provisioner.cshtml", output);
        }

        [HttpPost]
        [Route("/tools/provisioner/ezgo/processing")]
        public async Task<IActionResult> ProvisionerEzgoProcessing([FromBody] string content)
        {
            if (!User.IsInRole("serviceaccount"))
            {
                return NoContent();
            }

            if(string.IsNullOrEmpty(content))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid data".ToJsonFromObject());
            }

            var result = await _connector.PostCall("/v1/provisioner/ezgo", content.ToJsonFromObject());

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
               return StatusCode((int)result.StatusCode, result.Message.ToString().ToJsonFromObject());
            } else
            {
               return StatusCode((int)result.StatusCode, result.Message.ToString().ToJsonFromObject());
            }
        }

        [HttpPost]
        [Route("/tools/provisioner/atoss/processing")]
        public async Task<IActionResult> ProvisionerAtossProcessing([FromBody] string content)
        {
            if (!User.IsInRole("serviceaccount"))
            {
                return NoContent();
            }

            if (string.IsNullOrEmpty(content))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Invalid data".ToJsonFromObject());
            }

            var result = await _connector.PostCall("/v1/provisioner/atoss", content.ToJsonFromObject());

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)result.StatusCode, result.Message.ToString().ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)result.StatusCode, result.Message.ToString().ToJsonFromObject());
            }
        }

        //TODO MOVE to own later on
        [HttpGet]
        [Route("/tools/noc/overview")]
        public async Task<IActionResult> NocOverview()
        {
            var currentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
            if (this.IsAdminCompany || (currentUser != null && currentUser.IsServiceAccount))
            {
                if (Logic.Statics.AvailableLanguages == null)
                {
                    await _languageService.GetLanguageSelectorItems(); //TODO refactor
                }

                var output = new NocViewModel();
                output.IsAdminCompany = this.IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.TOOLS;

                output.ShowTextDebug = this.IsAdminCompany;

                if(_configurationHelper.GetValueAsString("AppSettings:EnvironmentConfig") == "development" || _configurationHelper.GetValueAsString("AppSettings:EnvironmentConfig") == "localdevelopment")
                {
                    output.ShowAccNoc = true;
                    output.ShowTestNoc = true;
                    output.ShowProdNoc = true;
                } else
                {
                    if(_configurationHelper.GetValueAsString("AppSettings:EnvironmentConfig") == "test") {
                        output.ShowTestNoc = true;
                    }

                    if (_configurationHelper.GetValueAsString("AppSettings:EnvironmentConfig") == "acceptance") {
                        output.ShowAccNoc = true;
                    }

                    if (_configurationHelper.GetValueAsString("AppSettings:EnvironmentConfig") == "production") {
                        output.ShowProdNoc = true;
                    }
                }

                output.ApplicationSettings = await this.GetApplicationSettings();

                return View("~/Views/Tools/noc.cshtml", output);
            };
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }
    }
}
