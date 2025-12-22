using EZGO.Api.Models;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Tools;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class RawViewerController : BaseController
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public RawViewerController(ILogger<SettingsController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            // DI
            _logger = logger;
            _connector = connector;
            _languageService = language;

        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/raw/viewer")]
        public async Task<IActionResult> Index()
        {
            var output = new RawViewModel();

            output.IsAdminCompany = this.IsAdminCompany;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.RAWVIEWER;
            var companiesResult = await _connector.GetCall(@"/v1/companies");
            if (companiesResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
            }

            output.ChoosenStartDateTime = DateTime.Now.Date;
            output.ChoosenEndDateTime = DateTime.Now.AddDays(1).Date;
            output.ChoosenRawViewerType = "tasks";
            output.ChoosenCompanyId = 136;

            var dataResult = await _connector.GetCall(string.Format("/v1/tools/raw/{2}/tasks?starttimestamp={0}&endtimestamp={1}", output.ChoosenStartDateTime.ToString("dd-MM-yyyy HH:mm"), output.ChoosenEndDateTime.ToString("dd-MM-yyyy HH:mm"), output.ChoosenCompanyId));
            if (dataResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Data = dataResult.Message.ToObjectFromJson<RawData>();
            }

            output.ApplicationSettings = await this.GetApplicationSettings();


            return View("~/Views/RawViewer/Index.cshtml", output);
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/raw/viewer/{companyid}/{viewtype}")]
        public async Task<IActionResult> RetrieveInformation([FromRoute] string viewtype, [FromRoute] int companyid, [FromQuery] string start, [FromQuery] string end)
        {
            DateTime parsedStartTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(start) && DateTime.TryParseExact(start, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStartTimestamp)) { };

            DateTime parsedEndTimestamp = DateTime.MinValue;
            if (!string.IsNullOrEmpty(end) && DateTime.TryParseExact(end, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEndTimestamp)) { };


            var output = new RawViewModel();

            output.IsAdminCompany = this.IsAdminCompany;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.RAWVIEWER;

            output.ChoosenStartDateTime = parsedStartTimestamp == DateTime.MinValue ? DateTime.Now.Date : parsedStartTimestamp;
            output.ChoosenEndDateTime = parsedEndTimestamp == DateTime.MinValue ? DateTime.Now.AddDays(1).Date : parsedEndTimestamp;
            output.ChoosenRawViewerType = viewtype;
            output.ChoosenCompanyId = companyid;


            var dataResult = await _connector.GetCall(string.Format("/v1/tools/raw/{2}/{3}?starttimestamp={0}&endtimestamp={1}", output.ChoosenStartDateTime.ToString("dd-MM-yyyy HH:mm"), output.ChoosenEndDateTime.ToString("dd-MM-yyyy HH:mm"), output.ChoosenCompanyId, output.ChoosenRawViewerType));
            if (dataResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    output.Data = dataResult.Message.ToObjectFromJson<RawData>();
#pragma warning disable CS0168 // Variable is declared but never used
                } catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                {

                }
              
            }

            output.ApplicationSettings = await this.GetApplicationSettings();

            return PartialView("~/Views/RawViewer/_report_table.cshtml", output);
        }

        [Authorize(Roles = AUTHORIZATION_ADMINISTRATOR_ROLES)]
        [Route("/raw/scheduler")]
        public async Task<IActionResult> Scheduler()
        {
            var output = new RawSchedulerViewModel();

            output.IsAdminCompany = this.IsAdminCompany;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.Module = FilterViewModel.ApplicationModules.RAWSCHEDULER;
            var companiesResult = await _connector.GetCall(@"/v1/companies");
            if (companiesResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(companiesResult.Message))
            {
                output.Companies = companiesResult.Message.ToObjectFromJson<List<Company>>();
            }

            output.ChoosenStartDateTime = DateTime.Now.Date;
            output.ChoosenEndDateTime = DateTime.Now.AddDays(1).Date;
            //output.ChoosenRawViewerType = "tasks";
            //output.ChoosenCompanyId = 136;

            //replace with dynamic code
            //var schedule = new CalendarSchedule();
            //schedule.Days = new List<CalendarDay>();

            //var calendarDay = new CalendarDay();
            //calendarDay.ScheduleItems = new List<CalendarItem>();
            //calendarDay.ScheduleItems.Add(new CalendarItem() { Color = "#FF0000", Title = "Export", Description = "Export for xx", StartTime = "00:00", EndTime = "00:30", ItemType = "EXPORT" });
            //calendarDay.DayName = "Monday";
            //calendarDay.DayNumber = Convert.ToInt32(DayOfWeek.Monday);

            //schedule.Days.Add(calendarDay);

            //output.Schedule = schedule;
            //schedule.
            var dataResult = await _connector.GetCall(string.Format("/v1/tools/raw/schedule?starttimestamp={0}&endtimestamp={1}", output.ChoosenStartDateTime.ToString("dd-MM-yyyy HH:mm"), output.ChoosenEndDateTime.ToString("dd-MM-yyyy HH:mm")));
            if (dataResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dataResult.Message))
            {
                output.Schedule = dataResult.Message.ToObjectFromJson<CalendarSchedule>();
            } else
            {
                output.Schedule = new CalendarSchedule();
            }

            output.ApplicationSettings = await this.GetApplicationSettings();

            return View("~/Views/RawViewer/Scheduler.cshtml", output);
        }

    }
}
