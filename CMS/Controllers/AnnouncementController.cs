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
using WebApp.Models;
using WebApp.Models.Announcements;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class AnnouncementController : BaseController
    {
        private readonly ILogger<AnnouncementController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public AnnouncementController(ILogger<AnnouncementController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            // DI
            _logger = logger;
            _connector = connector;
            _languageService = language;

        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("announcement")]
        public async Task<IActionResult> Index()
        {
            if(this.IsAdminCompany)
            {
                var output = new AnnouncementViewModel();
                output.IsAdminCompany = this.IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.ANNOUNCEMENTS;
                output.Locale = _locale;
                var announcementResult = await _connector.GetCall(string.Concat(Logic.Constants.Announcements.GetAnnouncements, "?announcementtype=all"));
                if (announcementResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.Announcements = announcementResult.Message.ToObjectFromJson<List<AnnouncementModel>>();
                }

                output.ApplicationSettings = await this.GetApplicationSettings();
                return View(output);
            }
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("announcement/save")]
        public async Task<IActionResult> Save([FromBody] AnnouncementModel announcement)
        {
            if (this.IsAdminCompany)
            {
                if (announcement != null)
                {
                    ApiResponse result = null;

                    if (announcement.Id > 0)
                    {
                        result = await _connector.PostCall(string.Concat("/v1/announcement/change/", announcement.Id, "?fulloutput=true"), announcement.ToJsonFromObject());
                    }
                    else
                    {
                        result = await _connector.PostCall("/v1/announcement/add?fulloutput=true", announcement.ToJsonFromObject());
                    }

                    if (result != null && result.StatusCode == HttpStatusCode.OK)
                    {
                        return StatusCode((int)HttpStatusCode.OK, result.Message); //note! message contains json when oke, so return for further processing in JS;
                    }
                    else
                    {
                        //other status returned, somethings wrong or can not continue due to business logic.
                        return StatusCode((int)result.StatusCode, result.Message != null ? result.Message.ToJsonFromObject() : false.ToJsonFromObject());
                    }
                }

                return StatusCode((int)HttpStatusCode.NoContent);
            }
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());

        }


        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [HttpPost]
        [Route("announcement/delete/{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (this.IsAdminCompany)
            {
                if(id > 0)
                {
                    var result = await _connector.PostCall(string.Concat("/v1/announcement/setactive/", id), false.ToJsonFromObject());

                    if (result != null && result.StatusCode == HttpStatusCode.OK)
                    {
                        return StatusCode((int)HttpStatusCode.OK, result.Message); //note! message contains json when oke, so return for further processing in JS;
                    }
                    else
                    {
                        //other status returned, somethings wrong or can not continue due to business logic.
                        return StatusCode((int)result.StatusCode, result.Message != null ? result.Message.ToJsonFromObject() : false.ToJsonFromObject());
                    }
                }

                return StatusCode((int)HttpStatusCode.NoContent);
            }
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());

        }

        [Route("announcements/latest")]
        public async Task<IActionResult> GetAnnouncements()
        {
            //for now only admin, remove when announcement will be displayed in the normal CMS.
            if (this.IsAdminCompany)
            {
                var output = new AnnouncementViewModel();
                output.IsAdminCompany = this.IsAdminCompany;
                output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
                output.Filter.Module = FilterViewModel.ApplicationModules.ANNOUNCEMENTS;
                output.Locale = _locale;
                var announcementResult = await _connector.GetCall(string.Concat(Logic.Constants.Announcements.GetAnnouncements, "?announcementtype=all&limit=10"));
                if (announcementResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.Announcements = announcementResult.Message.ToObjectFromJson<List<AnnouncementModel>>();
                }
                output.ApplicationSettings = await this.GetApplicationSettings();
                return PartialView("~/Views/Shared/_announcements.cshtml", output);
            }
            return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
        }
    }
}
