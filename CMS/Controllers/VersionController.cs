using EZGO.Api.Models.Versions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class VersionController : BaseController
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public VersionController(ILogger<CompanyController> logger, IApiConnector connector, ILanguageService languageService, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) 
            : base(languageService, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = languageService;
        }

        [Route("version")]
        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        public async Task<IActionResult> Index()
        {
            List<VersionApp> appVersions = new();
            var versionResponse = await _connector.GetCall(@"/v1/versions/app");
            if (versionResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                appVersions = versionResponse.Message.ToObjectFromJson<List<VersionApp>>();
            }

            VersionOverviewViewModel viewModel = new()
            {
                Versions = appVersions,
                ApplicationSettings = await GetApplicationSettings(),
                IsAdminCompany = IsAdminCompany,
                Locale = _locale
            };
            viewModel.Filter.Module = FilterViewModel.ApplicationModules.VERSIONS;

            return View(viewModel);
        }

        [Route("version/add")]
        [HttpPost]
        public async Task<IActionResult> AddVersion([FromBody] VersionApp versionApp)
        {
            var result = await _connector.PostCall(@"/v1/version/app/add", versionApp.ToJsonFromObject());
            return StatusCode((int)result.StatusCode, result.Message);
        }

        [Route("version/change/{id}")]
        [HttpPost]
        public async Task<IActionResult> ChangeVersion([FromRoute] int id, [FromBody] VersionApp versionApp)
        {
            var result = await _connector.PostCall(@"/v1/version/app/change/"+id, versionApp.ToJsonFromObject());
            return StatusCode((int)result.StatusCode, result.Message);
        }

        [Route("version/delete/{id}")]
        [HttpPost]
        public async Task<IActionResult> DeleteVersion([FromRoute] int id, [FromBody] VersionApp versionApp)
        {
            var result = await _connector.PostCall(@"/v1/version/app/setactive/" + id, "false");
            return StatusCode((int)result.StatusCode, result.Message);
        }
    }
}
