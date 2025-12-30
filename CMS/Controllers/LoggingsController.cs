using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class LoggingsController : BaseController
    {
        private readonly ILogger<LoggingsController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public LoggingsController(ILogger<LoggingsController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            // DI
            _logger = logger;
            _connector = connector;
            _languageService = language;

        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        public async Task<IActionResult> Index()
        {
            if (this.IsAdminCompany)
            {
                var output = new LoggingsViewModel();
                output.IsAdminCompany = this.IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.SETTINGS;
                output.ApplicationSettings = await this.GetApplicationSettings();
                return View(output);
            };

            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }
    }
}
