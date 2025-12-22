using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Stats;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Logic.Interfaces;
using WebApp.Models.Announcements;
using WebApp.Models.Audit;
using WebApp.Models.Checklist;
using WebApp.Models.Statistics;
using WebApp.Models.Task;
using WebApp.Models.User;
using WebApp.ViewModels;


namespace WebApp.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly IActionService _actionService;
        private readonly IConfigurationHelper _configHelper;
        private readonly ILanguageService _languageService;

        public DashboardController(ILogger<HomeController> logger, ILanguageService languageService, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IActionService actionService, IInboxService inboxService, IApplicationSettingsHelper applicationSettingsHelper) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _actionService = actionService;
            _configHelper = configurationHelper;
            _languageService = languageService;
        }

        [HttpGet]
        [Route("/dashboard")]
        public async Task<IActionResult> Index()
        {
            DashboardViewModel output;
            output = new DashboardViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Locale = _locale;
            output.Filter.Module = FilterViewModel.ApplicationModules.DASHBOARD;
            output.IsAdminCompany = IsAdminCompany; //normally = false; only needs to be filled on controller/views that are used with admin company

            if (_configHelper.GetValueAsBool("AppSettings:UseDashboardMergeCall")) //NOTE! for backwards compatibility older code still active for acceptance and production. Remove when not needed anymore.
            {
                //create dashboard data collection based on merge call
                var callParameters = "";
                var dashboardDataCollection = new Dashboard();
                if (IsAdminCompany)
                {
                    callParameters = "?usestatictotals=true&useannouncements=true";
                }
                else
                {
                    callParameters = "?usecompanyoverview=true";
                }

                var dashBoardResult = await _connector.GetCall(string.Concat("/v1/dashboard", callParameters));
                if (dashBoardResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardResult.Message))
                {
                    dashboardDataCollection = JsonConvert.DeserializeObject<Dashboard>(dashBoardResult.Message); //dashBoardResult.Message.ToObjectFromJson<Dashboard>();
                }

                //get announcemetns (for admin and non-admin companies)
                if (dashboardDataCollection.Announcements != null)
                {
                    output.Announcements = dashboardDataCollection.Announcements.ToJsonFromObject().ToObjectFromJson<List<AnnouncementModel>>();
                }

                if (IsAdminCompany)
                {
                    //get admin company data
                    if (dashboardDataCollection.StatisticTotals != null)
                    {
                        output.StatisticTotals = dashboardDataCollection.StatisticTotals.ToJsonFromObject().ToObjectFromJson<StatsTotals>();
                    }
                }
                else
                {
                    //get non admin company data
                    output.NewComments = await _actionService.MyCommentsCount(); //TODO make more efficient, now very not efficient.
                    output.NewInboxItemsCount = await GetInboxCount();

                    if (dashboardDataCollection.CompanyOverview != null)
                    {
                        output.CompanyTotals = dashboardDataCollection.CompanyOverview.ToJsonFromObject().ToObjectFromJson<List<CompanyOverviewModel>>();
                    }

                    if (dashboardDataCollection.CompletedChecklists != null)
                    {
                        //due to weird datetime conversion with the json converter use newton soft
                        //TODO figure out why this happens and fully disable this, current solution (forcdeUtc) does not work properly.
                        output.CompletedChecklists = JsonConvert.DeserializeObject<List<CompletedChecklistModel>>(dashboardDataCollection.CompletedChecklists.ToJsonFromObject());
                        output.CompletedChecklists.ForEach(x => { x.Tasks = x.Tasks?.OrderBy(t => t.Index).ToList(); });
                    }

                    if (dashboardDataCollection.CompletedAudits != null)
                    {
                        //due to weird datetime conversion with the json converter use newton soft
                        //TODO figure out why this happens and fully disable this, current solution (forcdeUtc) does not work properly.
                        output.CompletedAudits = JsonConvert.DeserializeObject<List<CompletedAuditModel>>(dashboardDataCollection.CompletedAudits.ToJsonFromObject());
                        output.CompletedAudits.ForEach(x => { x.Tasks = x.Tasks?.OrderBy(t => t.Index).ToList(); });
                    }

                    if (dashboardDataCollection.CompletedTasks != null)
                    {
                        output.CompletedTasks = dashboardDataCollection.CompletedTasks.ToJsonFromObject().ToObjectFromJson<List<CompletedTaskModel>>();
                    }

                    if (dashboardDataCollection.Actions != null)
                    {
                        output.Actions = dashboardDataCollection.Actions.ToJsonFromObject().ToObjectFromJson<List<Models.Action.ActionModel>>();
                    }
                }
            }
            else
            {
                //Legacy, replaced by code above, if enabled fully enabled on all platforms delete code below.
                if (!IsAdminCompany) output.NewComments = await _actionService.MyCommentsCount(); //only get when not a admin company.

                var announcementResult = await _connector.GetCall(Logic.Constants.Announcements.GetAnnouncements);
                if (announcementResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(announcementResult.Message))
                {
                    output.Announcements = JsonConvert.DeserializeObject<List<AnnouncementModel>>(announcementResult.Message);
                }
                output.Announcements ??= new List<AnnouncementModel>();

                if (IsAdminCompany)
                {
                    var totalResult = await _connector.GetCall("/v1/reporting/statisticstotals");
                    if (totalResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(totalResult.Message))
                    {
                        output.StatisticTotals = totalResult.Message.ToObjectFromJson<StatsTotals>();
                    }
                    output.StatisticTotals ??= new StatsTotals();
                }
                else
                {
                    var result = await _connector.GetCall(Logic.Constants.Statistics.GetCompanyOverviewTotalsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
                    {
                        output.CompanyTotals = JsonConvert.DeserializeObject<List<CompanyOverviewModel>>(result.Message);
                    }
                    output.CompanyTotals ??= new List<CompanyOverviewModel>();

                    var completedChecklistResult = await _connector.GetCall(string.Format(Logic.Constants.Checklist.GetCompletedChecklistsTopList, Logic.Constants.General.NumberOfLastCompletedOnDashboard));
                    if (completedChecklistResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(completedChecklistResult.Message))
                    {
                        output.CompletedChecklists = JsonConvert.DeserializeObject<List<CompletedChecklistModel>>(completedChecklistResult.Message);
                    }
                    output.CompletedChecklists ??= new List<CompletedChecklistModel>();

                    var completedAuditResult = await _connector.GetCall(string.Format(Logic.Constants.Audit.GetCompletedAudits, Logic.Constants.General.NumberOfLastCompletedOnDashboard));
                    if (completedAuditResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(completedAuditResult.Message))
                    {
                        output.CompletedAudits = JsonConvert.DeserializeObject<List<CompletedAuditModel>>(completedAuditResult.Message);
                        output.CompletedAudits.ForEach(x => { x.Tasks = x.Tasks?.OrderBy(t => t.Index).ToList(); });
                    }
                    output.CompletedAudits ??= new List<CompletedAuditModel>();

                    var completedTaskResults = await _connector.GetCall(string.Format(Logic.Constants.Task.GetCompletedTasks, Logic.Constants.General.NumberOfLastCompletedOnDashboard));
                    if (completedTaskResults.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(completedTaskResults.Message))
                    {
                        output.CompletedTasks = JsonConvert.DeserializeObject<List<CompletedTaskModel>>(completedTaskResults.Message);
                    }

                    var lastActionsResult = await _connector.GetCall(string.Format(Logic.Constants.Action.GetLastActionsUrl, Logic.Constants.General.NumberOfLastCompletedOnDashboard));
                    if (lastActionsResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(lastActionsResult.Message))
                    {
                        output.Actions = JsonConvert.DeserializeObject<List<Models.Action.ActionModel>>(lastActionsResult.Message);
                        if(output.Actions != null && output.Actions.Count > 0)
                        {
                            output.Actions = output.Actions.OrderByDescending(x => x.CreatedAt).Take(3).ToList();
                        }
                        
                    }
                    output.Actions ??= new List<Models.Action.ActionModel>();
                }
            }

            try
#pragma warning disable CS0168 // Variable is declared but never used
            {
                var retrievedUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();
                output.ShowNocNavigation = retrievedUser.IsServiceAccount || this.IsAdminCompany;
            } catch(Exception ex)
            {
                //do nothing, ignore it. 
            }
#pragma warning restore CS0168 // Variable is declared but never used

            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            output.ApplicationSettings = await GetApplicationSettings();
                        
            return View(output);
        }

        [HttpGet]
        [Route("/dashboardcompleteditems")]
        public async Task<IActionResult> GetCompletedItems()
        {
            var dashboardDataCollection = new Dashboard();

            DashboardViewModel output;
            output = new DashboardViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Locale = _locale;
            output.Filter.Module = FilterViewModel.ApplicationModules.DASHBOARD;
            output.ApplicationSettings = await GetApplicationSettings();

            var dashBoardAuditsResult = await _connector.GetCall("/v1/dashboard/completed/audits");

            if (dashBoardAuditsResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardAuditsResult.Message))
            {
                dashboardDataCollection.CompletedAudits = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Audit>>(dashBoardAuditsResult.Message);
            }

            var dashBoardChecklistsResult = await _connector.GetCall("/v1/dashboard/completed/checklists");

            if (dashBoardChecklistsResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardChecklistsResult.Message))
            {
                dashboardDataCollection.CompletedChecklists = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Checklist>>(dashBoardChecklistsResult.Message);
            }

            var dashBoardTasksResult = await _connector.GetCall("/v1/dashboard/completed/tasks");

            if (dashBoardTasksResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardTasksResult.Message))
            {
                dashboardDataCollection.CompletedTasks = JsonConvert.DeserializeObject<List<EZGO.Api.Models.TasksTask>>(dashBoardTasksResult.Message);
            }

            var dashBoardActionsResult = await _connector.GetCall("/v1/dashboard/actions");

            if (dashBoardActionsResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardActionsResult.Message))
            {
                dashboardDataCollection.Actions = JsonConvert.DeserializeObject<List<EZGO.Api.Models.ActionsAction>>(dashBoardActionsResult.Message);
            }

            if (dashboardDataCollection.CompletedChecklists != null)
            {
                //due to weird datetime conversion with the json converter use newton soft
                //TODO figure out why this happens and fully disable this, current solution (forcdeUtc) does not work properly.
                output.CompletedChecklists = JsonConvert.DeserializeObject<List<CompletedChecklistModel>>(dashboardDataCollection.CompletedChecklists.ToJsonFromObject());
                output.CompletedChecklists.ForEach(x => { x.Tasks = x.Tasks?.OrderBy(t => t.Index).ToList(); });
            }

            if (dashboardDataCollection.CompletedAudits != null)
            {
                //due to weird datetime conversion with the json converter use newton soft
                //TODO figure out why this happens and fully disable this, current solution (forcdeUtc) does not work properly.
                output.CompletedAudits = JsonConvert.DeserializeObject<List<CompletedAuditModel>>(dashboardDataCollection.CompletedAudits.ToJsonFromObject());
                output.CompletedAudits.ForEach(x => { x.Tasks = x.Tasks?.OrderBy(t => t.Index).ToList(); });
            }

            if (dashboardDataCollection.CompletedTasks != null)
            {
                output.CompletedTasks = dashboardDataCollection.CompletedTasks.ToJsonFromObject().ToObjectFromJson<List<CompletedTaskModel>>();
            }

            if (dashboardDataCollection.Actions != null)
            {
                output.Actions = dashboardDataCollection.Actions.ToJsonFromObject().ToObjectFromJson<List<Models.Action.ActionModel>>();
            }

            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            return PartialView("~/Views/Dashboard/_last_completed_items.cshtml", output);
        }

        [HttpGet]
        [Route("/dashboardcompleted/audits")]
        public async Task<IActionResult> GetCompletedAudits()
        {
            var dashboardDataCollection = new Dashboard();

            DashboardViewModel output;
            output = new DashboardViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Locale = _locale;
            output.Filter.Module = FilterViewModel.ApplicationModules.DASHBOARD;
            output.ApplicationSettings = await GetApplicationSettings();

            var dashBoardAuditsResult = await _connector.GetCall("/v1/dashboard/completed/audits");

            if (dashBoardAuditsResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardAuditsResult.Message))
            {
                dashboardDataCollection.CompletedAudits = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Audit>>(dashBoardAuditsResult.Message);
            }

            if (dashboardDataCollection.CompletedAudits != null)
            {
                //due to weird datetime conversion with the json converter use newton soft
                //TODO figure out why this happens and fully disable this, current solution (forcdeUtc) does not work properly.
                output.CompletedAudits = JsonConvert.DeserializeObject<List<CompletedAuditModel>>(dashboardDataCollection.CompletedAudits.ToJsonFromObject());
                output.CompletedAudits.ForEach(x => { x.Tasks = x.Tasks?.OrderBy(t => t.Index).ToList(); });
            }

            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            return PartialView("~/Views/Dashboard/_last_completed_audits.cshtml", output);
        }

        [HttpGet]
        [Route("/dashboardcompleted/checklists")]
        public async Task<IActionResult> GetCompletedChecklists()
        {
            var dashboardDataCollection = new Dashboard();

            DashboardViewModel output;
            output = new DashboardViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Locale = _locale;
            output.Filter.Module = FilterViewModel.ApplicationModules.DASHBOARD;
            output.ApplicationSettings = await GetApplicationSettings();

            var dashBoardChecklistsResult = await _connector.GetCall("/v1/dashboard/completed/checklists");

            if (dashBoardChecklistsResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardChecklistsResult.Message))
            {
                dashboardDataCollection.CompletedChecklists = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Checklist>>(dashBoardChecklistsResult.Message);
            }

            if (dashboardDataCollection.CompletedChecklists != null)
            {
                //due to weird datetime conversion with the json converter use newton soft
                //TODO figure out why this happens and fully disable this, current solution (forcdeUtc) does not work properly.
                output.CompletedChecklists = JsonConvert.DeserializeObject<List<CompletedChecklistModel>>(dashboardDataCollection.CompletedChecklists.ToJsonFromObject());
                output.CompletedChecklists.ForEach(x => { x.Tasks = x.Tasks?.OrderBy(t => t.Index).ToList(); });
            }

            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            return PartialView("~/Views/Dashboard/_last_completed_checklists.cshtml", output);
        }

        [HttpGet]
        [Route("/dashboardcompleted/tasks")]
        public async Task<IActionResult> GetCompletedTasks()
        {
            var dashboardDataCollection = new Dashboard();

            DashboardViewModel output;
            output = new DashboardViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Locale = _locale;
            output.Filter.Module = FilterViewModel.ApplicationModules.DASHBOARD;
            output.ApplicationSettings = await GetApplicationSettings();

            var dashBoardTasksResult = await _connector.GetCall("/v1/dashboard/completed/tasks");

            if (dashBoardTasksResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardTasksResult.Message))
            {
                dashboardDataCollection.CompletedTasks = JsonConvert.DeserializeObject<List<EZGO.Api.Models.TasksTask>>(dashBoardTasksResult.Message);
            }

            if (dashboardDataCollection.CompletedTasks != null)
            {
                output.CompletedTasks = dashboardDataCollection.CompletedTasks.ToJsonFromObject().ToObjectFromJson<List<CompletedTaskModel>>();
            }

            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            return PartialView("~/Views/Dashboard/_last_completed_tasks.cshtml", output);
        }

        [HttpGet]
        [Route("/dashboard/actions")]
        public async Task<IActionResult> GetActions()
        {
            var dashboardDataCollection = new Dashboard();

            DashboardViewModel output;
            output = new DashboardViewModel();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Locale = _locale;
            output.Filter.Module = FilterViewModel.ApplicationModules.DASHBOARD;
            output.ApplicationSettings = await GetApplicationSettings();

            var dashBoardActionsResult = await _connector.GetCall("/v1/dashboard/actions");

            if (dashBoardActionsResult.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(dashBoardActionsResult.Message))
            {
                dashboardDataCollection.Actions = JsonConvert.DeserializeObject<List<EZGO.Api.Models.ActionsAction>>(dashBoardActionsResult.Message);
            }

            if (dashboardDataCollection.Actions != null)
            {
                output.Actions = dashboardDataCollection.Actions.ToJsonFromObject().ToObjectFromJson<List<Models.Action.ActionModel>>();
            }

            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            return PartialView("~/Views/Dashboard/_last_actions.cshtml", output);
        }

    }
}
