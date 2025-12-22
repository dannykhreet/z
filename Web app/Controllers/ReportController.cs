using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Reports;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic;
using WebApp.Logic.Converters;
using WebApp.Logic.Interfaces;
using WebApp.Models.Audit;
using WebApp.Models.Checklist;
using WebApp.Models.Skills;
using WebApp.Models.Task;
using WebApp.Models.WorkInstructions;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Reports)]
    public class ReportController : BaseController
    {
        #region - privates / constants -
        public const int MAX_NR_OF_DYNAMIC_ITEMS = 10;
        public const int START_NR_OF_ITEMS = 10;

        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly IHttpContextAccessor _context;
        #endregion

        #region - constructor(s) -
        public ReportController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _context = httpContextAccessor;
        }
        #endregion

        #region - checklists -
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Checklists)]
        [Route("/report/checklist/completed")]
        public async Task<IActionResult> CompletedChecklists([FromQuery] int? templateId, [FromQuery] int? checklistId)
        {
            CompletedChecklistViewModel output = new CompletedChecklistViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;
            output.Filter.CmsLanguage = output.CmsLanguage;
            var endpoint = string.Format("/v1/checklists?include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields,userinformation&limit=10");

            if (templateId != null && templateId > 0)
            {
                endpoint = string.Format(Constants.Checklist.GetCompletedChecklistsWithTemplateId, 10, templateId);
                output.TemplateId = templateId.Value;
            }

            if (checklistId != null && checklistId > 0)
            {
                output.ChecklistId = checklistId.Value;
            }

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !result.Message.IsNullOrEmpty())
            {
                output.CompletedChecklists = JsonConvert.DeserializeObject<List<CompletedChecklistModel>>(result.Message);
            }
            else
            {
                output.CompletedChecklists = new();
            }
            foreach (var completedChecklist in output.CompletedChecklists)
            {
                completedChecklist.CmsLanguage = output.CmsLanguage;
            }

            output.Filter.Templates ??= new List<Logic.TemplateSummary>();

            //TODO optimize
            var response = await _connector.GetCall(@"/v1/checklisttemplates?limit=0");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                output.Filter.Templates = JsonConvert.DeserializeObject<List<TemplateSummary>>(response.Message);
                output.Filter.Templates = output.Filter.Templates.OrderBy(t => t.Id).ToList();
            }

            output.PageTitle = "Completed checklists";
            output.Filter.Module = FilterViewModel.ApplicationModules.COMPLETEDCHECKLISTS;
            output.ApplicationSettings = await GetApplicationSettings();
            output.Filter.ApplicationSettings = output.ApplicationSettings;
            if(output.CompletedChecklists != null && output.CompletedChecklists.Count > 0)
            {
                foreach(var completedChecklist in output.CompletedChecklists)
                {
                    completedChecklist.ApplicationSettings = output.ApplicationSettings;
                }
            }
            return View("~/Views/Report/Checklist/index.cshtml", output);
        }

        //TODO why is this different; structure is not the same as /report/audit/completed/{id} which should be.
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Checklists)]
        [Route("/report/checklist/completed/{id}")]
        public async Task<IActionResult> CompletedChecklistTasks(int id)
        {
            CompletedChecklistTaskViewModel output = new CompletedChecklistTaskViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Locale = _locale;
            var endpoint = string.Format(Constants.Checklist.GetCompletedChecklistTask, id);
            var result = await _connector.GetCall(endpoint);
            if(result.StatusCode == System.Net.HttpStatusCode.OK && !result.Message.IsNullOrEmpty()) output.Checklist = JsonConvert.DeserializeObject<CompletedChecklistModel>(result.Message);
            output.ApplicationSettings = await GetApplicationSettings();
            output.Checklist.ApplicationSettings = output.ApplicationSettings;
            if(output.Checklist != null && output.Checklist.Tasks != null && output.Checklist.Tasks.Count>0)
            {
                output.Checklist.Tasks = output.Checklist.Tasks.OrderBy(t => t.Index).ToList();

                foreach (var task in output.Checklist.Tasks)
                {
                    task.ApplicationSettings = output.ApplicationSettings;
                }
            }

            if(output.Checklist != null && output.Checklist.Stages != null && output.Checklist.Stages.Count > 0)
            {
                output.Checklist.Stages = output.Checklist.Stages.OrderBy(t => t.Index).ToList();
            }

            //while 5145 is still in development, this setting is used to determine the environments it should be available on
            output.EnablePropertyTiles = _configurationHelper.GetValueAsBool("AppSettings:EnablePropertyTiles");
            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");
            if(User.IsInRole("serviceaccount"))
            {
                output.EnableRemovalOfObject = true;
            }

            return PartialView("~/Views/Report/Checklist/_completed_checklist_tasks.cshtml", output);
        }

        [HttpPost]
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Checklists)]
        [Route("/report/checklist/delete/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id > 0 && User.IsInRole("serviceaccount"))
            {
                var endpoint = string.Format(Logic.Constants.Checklist.PostDeleteCompletedChecklist, id);
                var result = await _connector.PostCall(endpoint, "false");
                return StatusCode((int)result.StatusCode, result.Message.ToString());
            }
            return StatusCode((int)HttpStatusCode.Forbidden, "");
        }

        [HttpGet]
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Checklists)]
        [Route("/report/checklist/completeddetails")]
        public async Task<IActionResult> CompletedChecklists([FromQuery] int areaid, [FromQuery] int offset, [FromQuery] int limit, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int templateId, [FromQuery] bool? iscompleted)
        {
            var uriParams = new List<string>();
            if (offset <= 0)
            {
                offset = 0;
            }
            if (limit <= 0 || limit > MAX_NR_OF_DYNAMIC_ITEMS)
            {
                limit = MAX_NR_OF_DYNAMIC_ITEMS;
            }

            CompletedChecklistViewModel output = new CompletedChecklistViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;

            var endpoint = "/v1/checklists?include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields,userinformation";
            if (areaid > 0)
            {
                uriParams.Add("areaid=" + areaid);
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateTime) &&
                !string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateTime))
            {
                uriParams.Add("starttimestamp=" + startDateTime.ToString("dd-MM-yyyy HH:mm:ss"));
                uriParams.Add("endtimestamp=" + endDateTime.AddDays(1).ToString("dd-MM-yyyy HH:mm:ss"));
            }

            if (templateId > 0)
            {
                uriParams.Add("templateid=" + templateId);
            }

            if (iscompleted != null)
            {
                uriParams.Add("iscompleted=" + iscompleted.Value.ToString().ToLower());
                if(iscompleted.Value == false)
                {
                    uriParams.Add("sortByModifiedAt=true");
                }
            }

            if (limit > 0)
            {
                uriParams.Add("limit=" + limit);
            }

            if (offset > 0)
            {
                uriParams.Add("offset=" + offset);
            }

            endpoint += "&" + string.Join("&", uriParams);

            output.ApplicationSettings = await GetApplicationSettings();

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.CompletedChecklists = JsonConvert.DeserializeObject<List<CompletedChecklistModel>>(result.Message);

            }
            else { output.CompletedChecklists = new List<CompletedChecklistModel>(); }

            if (output.CompletedChecklists != null && output.CompletedChecklists.Count > 0)
            {
                foreach (var completedChecklist in output.CompletedChecklists)
                {
                    completedChecklist.ApplicationSettings = output.ApplicationSettings;
                }
            }

            return PartialView("~/Views/Report/Checklist/_completed_checklists.cshtml", output);
        }

        #endregion

        #region - audits -
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Audits)]
        [Route("/report/audit/completed")]
        public async Task<IActionResult> CompletedAudits([FromQuery] int? templateId, [FromQuery] int? auditId)
        {
            CompletedAuditViewModel output = new CompletedAuditViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Locale = _locale;
            var endpoint = string.Format(Constants.Audit.GetCompletedAudits, START_NR_OF_ITEMS);

            if (templateId != null && templateId > 0)
            {
                endpoint = string.Format(Constants.Audit.GetCompletedAuditsWithTemplateId, 10, templateId);
                output.TemplateId = templateId.Value;
            }

            if (auditId != null && auditId > 0)
            {
                output.AuditId = auditId.Value;
            }

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.CompletedAudits = JsonConvert.DeserializeObject<List<CompletedAuditModel>>(result.Message);
            }
            else { output.CompletedAudits = new List<CompletedAuditModel>(); }

            output.Filter.Templates ??= new List<Logic.TemplateSummary>();

            //TODO optimize
            var response = await _connector.GetCall(@"/v1/audittemplates?limit=0");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                output.Filter.Templates = JsonConvert.DeserializeObject<List<TemplateSummary>>(response.Message);
                output.Filter.Templates = output.Filter.Templates.OrderBy(t => t.Id).ToList();
            }

            output.PageTitle = "Completed audits";
            output.Filter.Module = FilterViewModel.ApplicationModules.COMPLETEDAUDITS;
            output.ApplicationSettings = await GetApplicationSettings();
            output.Filter.ApplicationSettings = output.ApplicationSettings;

            return View("~/Views/Report/Audit/index.cshtml", output);
        }

        [HttpGet]
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Audits)]
        [Route("/report/audit/completeddetails")]
        public async Task<IActionResult> CompletedAudits([FromQuery] int areaid, [FromQuery] int offset, [FromQuery] int limit, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int templateId)
        {
            var uriParams = new List<string>();
            if (offset <= 0)
            {
                offset = 0;
            }

            if (limit <= 0 || limit > MAX_NR_OF_DYNAMIC_ITEMS)
            {
                limit = MAX_NR_OF_DYNAMIC_ITEMS;
            }

            CompletedAuditViewModel output = new CompletedAuditViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;
            var endpoint = "/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields";
            

            if(areaid > 0)
            {
                uriParams.Add("areaid=" + areaid);
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateTime) &&
                !string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateTime))
            {
                uriParams.Add("starttimestamp=" + startDateTime.ToString("dd-MM-yyyy HH:mm:ss"));
                uriParams.Add("endtimestamp=" + endDateTime.AddDays(1).ToString("dd-MM-yyyy HH:mm:ss"));
            }

            if (templateId > 0)
            {
                uriParams.Add("templateid=" + templateId);
            }

            if (limit > 0)
            {
                uriParams.Add("limit=" + limit);
            }

            if (offset > 0)
            {
                uriParams.Add("offset=" + offset);
            }

            endpoint += "&" + string.Join("&", uriParams);

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.CompletedAudits = JsonConvert.DeserializeObject<List<CompletedAuditModel>>(result.Message);

            }
            else { output.CompletedAudits = new List<CompletedAuditModel>(); }

            return PartialView("~/Views/Report/Audit/_completed_audits.cshtml", output);
        }

        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Audits)]
        [Route("/report/audit/completed/{id}")]
        public async Task<IActionResult> CompletedAuditTasks(int id)
        {
            CompletedAuditTaskViewModel output = new CompletedAuditTaskViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Locale = _locale;
            var endpoint = string.Format(Constants.Audit.GetCompletedAuditTasks, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !result.Message.IsNullOrEmpty()) output.Audit = JsonConvert.DeserializeObject<CompletedAuditModel>(result.Message);

            if (output.Audit != null && output.Audit.Tasks != null && output.Audit.Tasks.Count > 0)
            {
                output.Audit.Tasks = output.Audit.Tasks.OrderBy(t => t.Index).ToList();
            }

            output.ApplicationSettings = await GetApplicationSettings();
            output.Audit.ApplicationSettings = output.ApplicationSettings;

            //while 5145 is still in development, this setting is used to determine the environments it should be available on
            output.EnablePropertyTiles = _configurationHelper.GetValueAsBool("AppSettings:EnablePropertyTiles");
            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            return PartialView("~/Views/Report/Audit/_completed_audit_tasks.cshtml", output);
        }
        #endregion

        #region - tasks - 

        //default route which returns the initial page
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
        [Route("/report/task/completed")]
        public async Task<IActionResult> CompletedTasks([FromQuery] int? templateid, [FromQuery] DateTime? timestamp)
        {
            CompletedTaskViewModel output = new CompletedTaskViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.CompletedTasks = new List<TasksTask>();
            output.PageTitle = "Completed tasks";
            output.Filter.Module = FilterViewModel.ApplicationModules.COMPLETEDTASKS;
            output.ApplicationSettings = await GetApplicationSettings();
            output.Locale = _locale;

            if (templateid.HasValue && templateid.Value > 0)
                output.TemplateId = templateid.Value;
            if (timestamp.HasValue)
                output.Timestamp = timestamp.Value;

            //TODO optimize
            var response = await _connector.GetCall(@"/v1/tasktemplates?limit=0");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                output.Filter.Templates = JsonConvert.DeserializeObject<List<TemplateSummary>>(response.Message);
                output.Filter.Templates = output.Filter.Templates.OrderBy(t => t.Id).ToList();
            }

            return View("~/Views/Report/Task/Index.cshtml", output);
        }

        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
        [Route("/report/task/completeddetails")]
        public async Task<IActionResult> CompletedTasksDetails([FromQuery] string filterType, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int offset, [FromQuery] int limit, [FromQuery] int? areaId, [FromQuery] int? templateid)
        {
            var model = new CompletedTaskViewModel();
            List<TasksTask> tasks = new List<TasksTask>();
            
            tasks = await GetTasksForPeriodAsync(from, to, areaId, templateid, limit, offset);

            model.CompletedTasks = tasks;
            model.ReferenceDate = to;
            model.FilterType = filterType;
            model.AreaId = areaId ?? 0;
            model.ApplicationSettings = await GetApplicationSettings();
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            model.Locale = _locale;

            //while 5145 is still in development, this setting is used to determine the environments it should be available on
            model.EnablePropertyTiles = _configurationHelper.GetValueAsBool("AppSettings:EnablePropertyTiles");

            return PartialView("~/Views/Report/Task/_tasks_detail.cshtml", model);
        }

        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
        [Route("/report/task/latestcompleteddetails")]
        public async Task<IActionResult> LatestCompletedTasksDetails([FromQuery] int offset, [FromQuery] int limit, [FromQuery] int templateid)
        {
            var model = new CompletedTaskViewModel();
            List<TasksTask> tasks = new List<TasksTask>();

            tasks = await GetLatestTasksAsync(templateid, limit, offset);

            model.CompletedTasks = tasks;
            model.ApplicationSettings = await GetApplicationSettings();
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            model.Locale = _locale;

            //while 5145 is still in development, this setting is used to determine the environments it should be available on
            model.EnablePropertyTiles = _configurationHelper.GetValueAsBool("AppSettings:EnablePropertyTiles");
            if (tasks != null && tasks.Count > 0)
            {
                return PartialView("~/Views/Report/Task/_tasks_detail.cshtml", model);
            }
            else
            {
                return Ok("No completed tasks found");
            }
        }

        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
        [Route("/report/task/completedstatistics")]
        public async Task<IActionResult> CompletedTasksStatistics([FromQuery] string filterType, [FromQuery] DateTime referenceDate, [FromQuery] int offset, [FromQuery] int limit, [FromQuery] int? areaId, [FromQuery] int? templateid, [FromQuery] int shiftId)
        {
            var refDate = DateTime.Now;
            if (referenceDate != DateTime.MinValue)
                refDate = referenceDate;

            List<DateTime> dateTimes = new List<DateTime>() { refDate };
            List<DateTime> endDateTimes = new List<DateTime>() { };
            Dictionary<TaskStatisticsPeriod, List<TaskStatistics>> taskStats = new Dictionary<TaskStatisticsPeriod, List<TaskStatistics>>();
            string partialViewName = "";

            var endpoint = string.Format(Constants.Shift.GetShifts, START_NR_OF_ITEMS);
            var result = await _connector.GetCall(endpoint);
            var shifts = JsonConvert.DeserializeObject<List<Shift>>(result.Message);

            var cmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            switch (filterType)
            {
                case "previousshifts":
                    //if no shifts display days
                    if (shifts == null || shifts.Count == 0)
                    {

                        //get dates for previous 7 days (current day and last 6 days)
                        dateTimes[0] = GetFirstShiftTimeOfDay(dateTimes[0].Date, shifts);
                        endDateTimes.Add(GetFirstShiftTimeOfDay(dateTimes[0].AddDays(1), shifts));
                        for (int i = 0; i < 6; i++)
                        {
                            dateTimes.Add(GetFirstShiftTimeOfDay(dateTimes[i].Subtract(TimeSpan.FromDays(1)), shifts));
                            endDateTimes.Add(GetFirstShiftTimeOfDay(dateTimes[i], shifts));
                        }

                        //get taskstatusbasic 7 times for each day corresponding to datetime (and areaid)
                        for (int i = 0; i < dateTimes.Count; i++)
                        {
                            taskStats.Add(new TaskStatisticsPeriod() { From = dateTimes[i], To = endDateTimes[i], CmsLanguage = cmsLanguage }, await GetTasksStatusForPeriod(dateTimes[i], endDateTimes[i], areaId, templateid));
                        }
                        partialViewName = "~/Views/Report/Task/_tasks_per_day.cshtml";
                        break;
                    }
                    //get shifts
                    var currentShiftIndex = -1;
                    for (int i = 0; i < shifts.Count; i++)
                    {
                        TimeSpan.TryParse(shifts[i].Start, out var shiftStart);
                        TimeSpan.TryParse(shifts[i].End, out var shiftEnd);

                        if (shifts[i].Day == (int)dateTimes[0].DayOfWeek + 1)
                        {
                            if (dateTimes[0].TimeOfDay > shiftStart || dateTimes[0].TimeOfDay < shiftEnd)
                            {
                                currentShiftIndex = i;
                                endDateTimes.Add(dateTimes[0].Date.Add(shiftEnd));
                                dateTimes[0] = dateTimes[0].Date.Add(shiftStart);
                                break;
                            }
                        }
                    }

                    //create datetimes based on shifts going backwards part 1
                    for (int i = currentShiftIndex - 1; i >= 0; i--)
                    {
                        TimeSpan.TryParse(shifts[i].Start, out var shiftStart);
                        TimeSpan.TryParse(shifts[i].End, out var shiftEnd);
                        var shiftStartDate = refDate.Date.AddDays(-(shifts[currentShiftIndex].Day - shifts[i].Day));
                        shiftStartDate = shiftStartDate.Add(shiftStart);

                        var shiftEndDate = refDate.Date.AddDays(-(shifts[currentShiftIndex].Day - shifts[i].Day));
                        shiftEndDate = shiftEndDate.Add(shiftEnd);

                        if (shiftEnd < shiftStart)
                            shiftEndDate = shiftEndDate.AddDays(1);

                        dateTimes.Add(shiftStartDate);
                        endDateTimes.Add(shiftEndDate);
                    }

                    //create datetimes based on shifts going backwards part 2
                    for (int i = shifts.Count - 1; i > currentShiftIndex; i--)
                    {
                        TimeSpan.TryParse(shifts[i].Start, out var shiftStart);
                        TimeSpan.TryParse(shifts[i].End, out var shiftEnd);
                        var shiftStartDate = refDate.Date.AddDays(-(shifts[currentShiftIndex].Day - shifts[i].Day));
                        shiftStartDate = shiftStartDate.Add(shiftStart);

                        var shiftEndDate = refDate.Date.AddDays(-(shifts[currentShiftIndex].Day - shifts[i].Day));
                        shiftEndDate = shiftEndDate.Add(shiftEnd);

                        if (shifts[i].Day >= shifts[currentShiftIndex].Day)
                        {
                            shiftStartDate = shiftStartDate.AddDays(-7);
                            shiftEndDate = shiftEndDate.AddDays(-7);
                        }
                        if (shiftEnd < shiftStart)
                            shiftEndDate = shiftEndDate.AddDays(1);

                        dateTimes.Add(shiftStartDate);
                        endDateTimes.Add(shiftEndDate);
                    }

                    //get taskstatusbasic 21 times for each shift corresponding to datetime
                    for (int i = 0; i < dateTimes.Count; i++)
                    {
                        taskStats.Add(new TaskStatisticsPeriod() { From = dateTimes[i], To = endDateTimes[i], CmsLanguage = cmsLanguage }, await GetTasksStatusForPeriod(dateTimes[i], endDateTimes[i], areaId, templateid));
                    }
                    partialViewName = "~/Views/Report/Task/_tasks_per_shift.cshtml";
                    //return PartialView("~/Views/Report/Task/_tasks_per_shift.cshtml", tasks);

                    break;
                case "previousdays":
                    //get dates for previous 7 days (current day and last 6 days)
                    dateTimes[0] = GetFirstShiftTimeOfDay(dateTimes[0].Date, shifts);
                    endDateTimes.Add(GetFirstShiftTimeOfDay(dateTimes[0].AddDays(1), shifts));
                    for (int i = 0; i < 6; i++)
                    {
                        dateTimes.Add(GetFirstShiftTimeOfDay(dateTimes[i].Subtract(TimeSpan.FromDays(1)), shifts));
                        endDateTimes.Add(GetFirstShiftTimeOfDay(dateTimes[i], shifts));
                    }

                    //get taskstatusbasic 7 times for each day corresponding to datetime (and areaid)
                    for (int i = 0; i < dateTimes.Count; i++)
                    {
                        taskStats.Add(new TaskStatisticsPeriod() { From = dateTimes[i], To = endDateTimes[i], CmsLanguage = cmsLanguage }, await GetTasksStatusForPeriod(dateTimes[i], endDateTimes[i], areaId, templateid));
                    }
                    partialViewName = "~/Views/Report/Task/_tasks_per_day.cshtml";
                    //return PartialView("~/Views/Report/Task/_tasks_per_day.cshtml", tasks);
                    break;
                case "previousweeks":
                    //get dates for previous 4 weeks? (current day and last 3 week days aka -7 days every time)
                    dateTimes[0] = dateTimes[0].Date;
                    if (dateTimes[0].DayOfWeek == DayOfWeek.Sunday)
                    {
                        dateTimes[0] = GetFirstShiftTimeOfDay(dateTimes[0].AddDays(-6), shifts);
                    }
                    else
                    {
                        dateTimes[0] = GetFirstShiftTimeOfDay(dateTimes[0].AddDays((int)dateTimes[0].DayOfWeek - 1), shifts);
                    }
                    endDateTimes.Add(dateTimes[0].AddDays(7));
                    for (int i = 0; i < 3; i++)
                    {
                        dateTimes.Add(dateTimes[i].Subtract(TimeSpan.FromDays(7)));
                        endDateTimes.Add(dateTimes[i]);
                    }

                    //get taskstatusbasic 4 times for each day corresponding to datetime (and areaid)
                    for (int i = 0; i < dateTimes.Count; i++)
                    {
                        taskStats.Add(new TaskStatisticsPeriod() { From = dateTimes[i], To = endDateTimes[i], CmsLanguage = cmsLanguage }, await GetTasksStatusForPeriod(dateTimes[i], endDateTimes[i], areaId, templateid));
                    }
                    partialViewName = "~/Views/Report/Task/_tasks_per_week.cshtml";
                    // return PartialView("~/Views/Report/Task/_tasks_per_week.cshtml", tasks);
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(partialViewName))
            {
                return PartialView(partialViewName, new TasksPerPeriodViewModel() { TaskStatisticsPerPeriod = taskStats, Locale = _locale });
            }
            else
            {
                return Ok();
            }

        }

        #region helper methods
        [NonAction]
        public DateTime GetFirstShiftTimeOfDay(DateTime day, List<Shift> shifts)
        {
            day = day.Date;

            for (int i = 0; i < shifts.Count; i++)
            {
                TimeSpan.TryParse(shifts[i].Start, out var shiftStart);

                if (shifts[i].Day == (int)day.DayOfWeek + 1)
                {
                    day = day.Date.Add(shiftStart);
                    break;
                }
            }

            return day;
        }
        #endregion

        #region task details
        [NonAction]
        public async Task<List<TasksTask>> GetTasksForPeriodAsync(DateTime? from, DateTime? to, int? areaId, int? templateId, int limit, int offset)
        {
            string fromTimestamp = from.Value.ToString("dd-MM-yyyy HH:mm:ss");
            string toTimestamp = to.Value.ToString("dd-MM-yyyy HH:mm:ss");
            string areaIdParam = areaId != null && areaId > 0 ? $"&areaid={areaId}" : "";
            string templateIdParam = templateId != null && templateId > 0 ? $"&templateid={templateId}" : "";

            string uri = $"/v1/tasks/period?from={fromTimestamp}&to={toTimestamp}{areaIdParam}{templateIdParam}&filterareatype=1&limit={limit}&offset={offset}&include=steps,tags,areapaths,properties,propertyvalues,propertyuservalues,pictureproof";

            var result = await _connector.GetCall(uri);
            var tasks = JsonConvert.DeserializeObject<List<TasksTask>>(result.Message);

            return tasks.ToList();
        }

        [NonAction]
        public async Task<List<TasksTask>> GetLatestTasksAsync(int templateId, int limit, int offset)
        {
            string templateIdParam = templateId != null && templateId > 0 ? $"templateid={templateId}&" : "";

            string uri = $"/v1/taskslatest?{templateIdParam}limit={limit}&offset={offset}&include=steps,tags,areapaths,properties,propertyvalues,propertyuservalues,pictureproof";

            var result = await _connector.GetCall(uri);
            var tasks = JsonConvert.DeserializeObject<List<TasksTask>>(result.Message);

            return tasks.ToList();
        }
        #endregion

        #region task status extended's
        [NonAction]
        public async Task<List<TaskStatistics>> GetTasksStatusForPeriod(DateTime? from, DateTime? to, int? areaId, int? templateId)
        {
            string fromTimestamp = from.Value.ToString("dd-MM-yyyy HH:mm:ss");
            string toTimestamp = to.Value.ToString("dd-MM-yyyy HH:mm:ss");
            string areaIdParamater = areaId != null ? $"&areaid={areaId}" : "";
            string templateIdParameter = templateId != null ? $"&templateid={templateId}" : "";

            string uri = $"/v1/tasks/statusses/period?from={fromTimestamp}&to={toTimestamp}{areaIdParamater}{templateIdParameter}&filterareatype=1&include=areapaths,recurrency,recurrencyshifts,propertyvalues&limit=0";

            var result = await _connector.GetCall(uri);
            var taskStatistics = JsonConvert.DeserializeObject<List<TaskStatistics>>(result.Message);

            return taskStatistics;
        }
        #endregion

        #endregion

        #region - completed assessments -
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [Route("/report/skillassessment/completed")]
        public async Task<IActionResult> CompletedAssessments([FromQuery] int? templateId, [FromQuery] int? assessmentId)
        {
            CompletedSkillAssessmentsViewModel output = new CompletedSkillAssessmentsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Locale = _locale;
            var endpoint = Logic.Constants.Assessments.GetAssessmentsNonFilter;

            if (templateId != null && templateId > 0)
            {
                output.TemplateId = templateId.Value;
                output.Filter.TemplateId = output.TemplateId;
            }

            if (assessmentId != null && assessmentId.Value > 0)
            {
                output.AssessmentId = assessmentId.Value;
            }

            output.CompletedAssessments = new List<SkillAssessment>();

            output.PageTitle = "Completed assessments";
            output.Filter.Module = FilterViewModel.ApplicationModules.REPORTSKILLASSESSMENTS;

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Filter.Users = (JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(resultUsers.Message)).OrderBy(x => x.LastName).ThenBy(y => y.FirstName).ToList();
            }

            output.Filter.Templates ??= new List<Logic.TemplateSummary>();

            //TODO optimize
            var response = await _connector.GetCall(@"/v1/assessmenttemplates?limit=0");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                output.Filter.Templates = JsonConvert.DeserializeObject<List<TemplateSummary>>(response.Message);
                output.Filter.Templates = output.Filter.Templates.OrderBy(t => t.Id).ToList();
            }

            output.ApplicationSettings = await GetApplicationSettings();
            return View("~/Views/Report/SkillAssessments/index.cshtml", output);
        }

        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [HttpGet]
        [Route("/report/skillassessment/completeddetails")]
        public async Task<IActionResult> CompletedSkillAssessmentsDetails([FromQuery] int areaid, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int templateid, [FromQuery] int assessorid, [FromQuery] string assessorids, [FromQuery] int assesseeid, [FromQuery] bool? iscompleted, [FromQuery] int offset, [FromQuery] int limit)
        {
            var uriParams = new List<string>();

            if (iscompleted != null)
            {
                uriParams.Add("iscompleted="+ (iscompleted.Value ? "true" : "false"));
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateTime) &&
                !string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateTime))
            {
                uriParams.Add("starttimestamp=" + startDateTime.ToString("dd-MM-yyyy HH:mm:ss"));
                uriParams.Add("endtimestamp=" + endDateTime.AddDays(1).ToString("dd-MM-yyyy HH:mm:ss"));
            }

            if (areaid > 0)
            {
                uriParams.Add("areaid=" + areaid);
            }

            if (templateid > 0)
            {
                uriParams.Add("templateid=" + templateid);
            }

            if (assessorid > 0)
            {
                uriParams.Add("assessorid=" + assessorid);
            }

            if (!string.IsNullOrEmpty(assessorids))
            {
                uriParams.Add("assessorids=" + assessorids);
            }

            if (assesseeid > 0)
            {
                uriParams.Add("completedforid=" + assesseeid);
            }

            if (iscompleted != null)
            {
                uriParams.Add("iscompleted=" + iscompleted.Value.ToString().ToLower());
                if (iscompleted.Value == false)
                {
                    uriParams.Add("sortByModifiedAt=true");
                }
            }

            //limit
            if (limit > 0)
            {
                uriParams.Add("limit=" + limit);
            }

            //offset
            if (offset > 0)
            {
                uriParams.Add("offset=" + offset);
            }

            CompletedSkillAssessmentsViewModel output = new CompletedSkillAssessmentsViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;

            var endpoint = "/v1/assessments?include=areapaths,areapathids,tags";

            endpoint += "&" + string.Join("&", uriParams);

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.CompletedAssessments = (JsonConvert.DeserializeObject<List<Assessment>>(result.Message)).ToLocalAssessments();

            }
            else { output.CompletedAssessments = new List<SkillAssessment>(); }

            return PartialView("~/Views/Report/SkillAssessments/_completed_skillassessments.cshtml", output);
        }

        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [Route("/report/skillassessment/completed/{id}")]
        public async Task<IActionResult> CompletedSkillAssessmentsTasks(int id)
        {
            SkillAssessment skillAssessment = new SkillAssessment();
            var endpoint = string.Format(Constants.Skills.SkillAssessmentDetailsUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                skillAssessment = (JsonConvert.DeserializeObject<Assessment>(result.Message)).ToLocalAssessment();
            }
            else
            {
                skillAssessment = new SkillAssessment();
            }

            var model = ViewModelConverters.ConvertToViewModel(skillAssessment);
            model.ApplicationSettings = await GetApplicationSettings();
            model.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            model.Locale = _locale;
            return PartialView("~/Views/Report/SkillAssessments/_completed_skillassessment_details.cshtml", model);
        }

        #endregion

        #region - work instruction change notifications - 
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.WorkInstructions)]
        [Route("/report/workinstructionchangenotifications")]
        public async Task<IActionResult> WorkInstructionChangeNotifications([FromQuery] int? templateId) //TODO implement templateid?
        {
            WorkInstructionChangeNotificationsViewModel output = new WorkInstructionChangeNotificationsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;
            output.Filter.CmsLanguage = output.CmsLanguage;

            var endpoint = string.Format("/v1/workinstructiontemplatechangenotifications?limit=10");

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !result.Message.IsNullOrEmpty())
            {
                output.ChangeNotifications = JsonConvert.DeserializeObject<List<WorkInstructionTemplateChangesNotificationModel>>(result.Message);
            }
            else
            {
                output.ChangeNotifications = new();
            }

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Filter.Users = (JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(resultUsers.Message)).OrderBy(x => x.LastName).ThenBy(y => y.FirstName).ToList();
            }

            output.PageTitle = (output.CmsLanguage.GetValue(LanguageKeys.WiChangeNotification.ReportPageTitle, "Last changed workinstructions") ?? "Last changed workinstructions");
            output.Filter.Module = FilterViewModel.ApplicationModules.WICHANGENOTIFICATIONS;
            output.ApplicationSettings = await GetApplicationSettings();
            output.Filter.ApplicationSettings = output.ApplicationSettings;

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == HttpStatusCode.OK)
            {
                output.WorkInstructions = JsonConvert.DeserializeObject<List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>>(resultworkinstructions.Message);
                output.Filter.Templates = JsonConvert.DeserializeObject<List<TemplateSummary>>(resultworkinstructions.Message);
            }

            return View("~/Views/Report/Work Instruction Change Notifications/Index.cshtml", output);

        }

        //TODO why is this different; structure is not the same as /report/audit/completed/{id} which should be.
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.WorkInstructions)]
        [Route("/report/workinstructionchangenotifications/details/{id}")]
        public async Task<IActionResult> WorkInstructionChangeNotificationsDetails(int id)
        {
            WorkInstructionChangeNotificationViewModel output = new WorkInstructionChangeNotificationViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Locale = _locale;
            
            var endpoint = string.Format($"/v1/workinstructiontemplatechangenotification/{id}?include=userinformation");
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !result.Message.IsNullOrEmpty())
            {
                output.ChangeNotification = JsonConvert.DeserializeObject<WorkInstructionTemplateChangesNotificationModel>(result.Message);
                output.ModifiedTextValue = (output.CmsLanguage.GetValue(LanguageKeys.WiChangeNotification.ChangedLabel, "Changed") ?? "Changed");
                output.ByTextValue = (output.CmsLanguage.GetValue(LanguageKeys.WiChangeNotification.ByLabel, "by") ?? "by");
                output.UsersThatHaveConfirmedThisChangeValue = (output.CmsLanguage.GetValue(LanguageKeys.WiChangeNotification.UsersThatHaveConfirmedThisChangeValueLabel, "Users that have confirmed viewing these changes") ?? "Users that have confirmed viewing these changes");
            }
            else
            {
                output.ChangeNotification = new();
            }

            var resultworkinstruction = await _connector.GetCall(string.Format(Logic.Constants.WorkInstructions.WorkInstructionDetailsUrl, output.ChangeNotification.WorkInstructionTemplateId));
            if (resultworkinstruction.StatusCode == HttpStatusCode.OK)
            {
                output.WorkInstruction = JsonConvert.DeserializeObject<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>(resultworkinstruction.Message);
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                //TODO log somewhere
                output.Areas = new List<Area>();
            }

            var tagsResult = await _connector.GetCall(Logic.Constants.Tags.GetTags);
            var tags = new List<Tag>();

            if (tagsResult.StatusCode == HttpStatusCode.OK)
            {
                output.Tags = JsonConvert.DeserializeObject<List<Tag>>(tagsResult.Message);
            }

            output.ApplicationSettings = await GetApplicationSettings();

            //while 5145 is still in development, this setting is used to determine the environments it should be available on
            output.EnablePropertyTiles = _configurationHelper.GetValueAsBool("AppSettings:EnablePropertyTiles");
            output.EnableInBrowserPdfPrint = _configurationHelper.GetValueAsBool("AppSettings:EnableInBrowserPDFPrint");

            return PartialView("~/Views/Report/Work Instruction Change Notifications/_wi_change_notification_details_single.cshtml", output);
        }

        [HttpGet]
        [Feature(Feature = FeatureAttribute.FeatureFiltersEnum.WorkInstructions)]
        [Route("/report/workinstructionchangenotifications/overview")]
        public async Task<IActionResult> WorkInstructionChangeNotificationsOverview([FromQuery] int areaid, [FromQuery] int offset, [FromQuery] int limit, [FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int templateId, [FromQuery] int notificationAuthorId, [FromQuery] bool? iscompleted)
        {
            var uriParams = new List<string>();
            if (offset <= 0)
            {
                offset = 0;
            }
            if (limit <= 0 || limit > MAX_NR_OF_DYNAMIC_ITEMS)
            {
                limit = MAX_NR_OF_DYNAMIC_ITEMS;
            }

            var output = new WorkInstructionChangeNotificationsViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;

            var endpoint = "/v1/workinstructiontemplatechangenotifications";

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateTime) &&
                !string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateTime))
            {
                uriParams.Add("starttimestamp=" + startDateTime.ToString("dd-MM-yyyy HH:mm:ss"));
                uriParams.Add("endtimestamp=" + endDateTime.AddDays(1).ToString("dd-MM-yyyy HH:mm:ss"));
            }

            if (templateId > 0)
            {
                uriParams.Add("workInstructionTemplateId=" + templateId);
            }

            if (areaid > 0)
            {
                uriParams.Add("areaid=" + areaid);
            }

            if (notificationAuthorId > 0)
            {
                uriParams.Add("userid=" + notificationAuthorId);
            }

            if (limit > 0)
            {
                uriParams.Add("limit=" + limit);
            }

            if (offset > 0)
            {
                uriParams.Add("offset=" + offset);
            }

            endpoint += "?" + string.Join("&", uriParams);

            output.ApplicationSettings = await GetApplicationSettings();

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.ChangeNotifications = JsonConvert.DeserializeObject<List<WorkInstructionTemplateChangesNotificationModel>>(result.Message);

            }
            else { output.ChangeNotifications = new List<WorkInstructionTemplateChangesNotificationModel>(); }

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == HttpStatusCode.OK)
            {
                output.WorkInstructions = JsonConvert.DeserializeObject<List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>>(resultworkinstructions.Message);
            }

            return PartialView("~/Views/Report/Work Instruction Change Notifications/_wi_change_notification_overview.cshtml", output);
        }


        #endregion
    }
}
