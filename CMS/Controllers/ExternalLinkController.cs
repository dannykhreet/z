using EZGO.Api.Models;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class ExternalLinkController : BaseController
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public ExternalLinkController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IConfigurationHelper configurationHelper, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [Route("externallink/{url}")]
        public async Task<IActionResult> CreateOrRetrieve(string url)
        {
            var output = new ExternalLinkViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.Module = FilterViewModel.ApplicationModules.EXTERNALLINK;
            output.Url = System.Uri.UnescapeDataString(url);
            return View("~/Views/Shared/_external_link.cshtml", output);
        }
    }
}
