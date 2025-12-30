//TODO refactor entire page, its a mess, remove initialization of object in controller, split models, move several data get point to separate methods, remove unused methods, add non action tag to non action methods, add route as attribute.
using Amazon.S3.Model;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Audit;
using WebApp.Models.Checklist;
using WebApp.Models.Properties;
using WebApp.Models.Shared;
using WebApp.Models.Shift;
using WebApp.Models.Task;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Tasks)]
    public class TaskController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private TaskViewModel output;

        public TaskController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;

            output = new TaskViewModel();
            output.CmsLanguage = language.GetLanguageDictionaryAsync(_locale).Result;
            output.Locale = _locale;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.TASKS;
            output.NewInboxItemsCount = inboxService.GetSharedTemplatesCount().Result;
        }

        [Route("/tasks")]
        public async Task<IActionResult> Index()
        {
          
            output.TaskTemplates ??= new List<TaskTemplateModel>();
            output.Tags.TagGroups = await GetTagGroups();
            output.CurrentTaskTemplate = new TaskTemplateModel();
            output.EnableSearchFilters = _configurationHelper.GetValueAsBool("AppSettings:SearchFilteringEnabled");
           // string uri = Logic.Constants.Task.GetTaskAreas;
            //var arearesult = await _connector.GetCall(uri);
            //try
            //{
            //    output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            //}
            //catch
            //{
            //    //TODO log somewhere
            //    output.Areas = new List<Area>();
            //}

            output.Filter.Areas = output.Areas;
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.TagGroups = await this.GetTagGroupsForFilter();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.CurrentTaskTemplate.ApplicationSettings = output.ApplicationSettings;
            return View(output);
        }

        [Route("/gettasks")]
        public async Task<IActionResult> GetTasks([FromQuery] string filtertext, [FromQuery] int areaid, [FromQuery] string tagids, [FromQuery] string roles, [FromQuery] string recurrency, [FromQuery] bool? instructionsadded, [FromQuery] bool? photosadded, [FromQuery] bool? videosadded, [FromQuery] int offset, [FromQuery] int limit)
        {
            var uriParams = GetOverviewUriParams(filtertext: filtertext, areaid: areaid, tagids: tagids, roles: roles, recurrency: recurrency, instructionsadded: instructionsadded, photosadded: photosadded, videosadded: videosadded, offset: offset, limit: limit);
            var endpoint = @"/v1/tasktemplates";
            if (uriParams.Count > 0)
            {
                endpoint = string.Concat(endpoint, "?include=areapaths,areapathids,recurrecy,tags,recurrencyshifts,steps&", string.Join("&", uriParams));
            }
            var result = await _connector.GetCall(endpoint);
            output.TaskTemplates = JsonConvert.DeserializeObject<List<TaskTemplateModel>>(result.Message);
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.ApplicationSettings = await this.GetApplicationSettings();
            return PartialView("~/Views/Task/_overview.cshtml", output);
        }

        /// <summary>
        /// NOT YET ENABLED IN API
        /// </summary>
        /// <returns></returns>
        [Route("/gettaskscounts")]
        public async Task<IActionResult> GetTaskCounts([FromQuery] string filtertext, [FromQuery] int areaid, [FromQuery] string tagids, [FromQuery] string roles, [FromQuery] string recurrency, [FromQuery] bool? instructionsadded, [FromQuery] bool? photosadded, [FromQuery] bool? videosadded, [FromQuery] int offset, [FromQuery] int limit)
        {
            var uriParams = GetOverviewUriParams(filtertext: filtertext, areaid: areaid, tagids: tagids, roles: roles, recurrency: recurrency, instructionsadded: instructionsadded, photosadded: photosadded, videosadded: videosadded, offset: offset, limit: limit);
            var endpoint = @"/v1/tasktemplates_counts";
            if (uriParams.Count > 0)
            {
                endpoint = string.Concat(endpoint,"?",string.Join("&", uriParams));
            }
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                var stats = JsonConvert.DeserializeObject<TaskTemplateCountStatistics>(result.Message);
                return Ok(stats.TotalCount);
            }

            return BadRequest();
        }

        /// <summary>
        /// GetOverviewUriParams; Get filter list for querystring based on input of controler (via params)
        /// </summary>
        /// <returns>List of filter items which will be used for querystring buildup.</returns>
        [NonAction]
        private List<string> GetOverviewUriParams(string filtertext, int areaid, string tagids, string roles, string recurrency, bool? instructionsadded, bool? photosadded, bool? videosadded, int offset, int limit)
        {
            var uriParams = new List<string>();

            if (!string.IsNullOrEmpty(filtertext))
            {
                uriParams.Add("filtertext=" + System.Web.HttpUtility.UrlEncode(filtertext));
            }

            if (areaid > 0)
            {
                uriParams.Add("areaid=" + areaid);
            }

            if (!string.IsNullOrEmpty(tagids))
            {
                uriParams.Add("tagids=" + tagids);
            }

            if (!string.IsNullOrEmpty(recurrency))
            {
                /*
                 Based on recurreny type enum

                RecurrencyTypeEnum
                {
                    NoRecurrency = 0,
                    Week = 1,
                    Month = 2,
                    Shifts = 3,
                    PeriodDay = 4,
                    PeriodMinute = 5, NOT SUPPORTED YET
                    PeriodHour = 6, NOT SUPPORTED YET
                    DynamicDay = 7,
                    DynamicHour = 8, NOT SUPPORTED YET
                    DynamicMinute = 9, NOT SUPPORTED YET

                }
                 */

                var recurrencyParams = new List<int>();
                var recurrencySplit = recurrency.Split(',');

                if (recurrencySplit.Length > 0)
                {
                    foreach (var recurrencyParam in recurrencySplit)
                    {
                        switch (recurrencyParam)
                        {
                            case "no recurrency": recurrencyParams.Add(0); break;
                            case "shifts": recurrencyParams.Add(3); break;
                            case "week": recurrencyParams.Add(1); break;
                            case "month": recurrencyParams.Add(2); break;
                            case "periodday": recurrencyParams.Add(4); break;
                            case "dynamicday": recurrencyParams.Add(7); break;
                        }
                    }
                    uriParams.Add("recurrencytypes=" + string.Join(',', recurrencyParams));
                }

            }

            if (!string.IsNullOrEmpty(roles))
            {
                var rolesParams = new List<int>();
                var rolesSplit = roles.Split(',');

                if (rolesSplit.Length > 0)
                {
                    foreach (var rolesParam in rolesSplit)
                    {
                        if (rolesParam == "basic")
                        {
                            rolesParams.Add(0);
                        }
                        else if (rolesParam == "shift_leader")
                        {
                            rolesParams.Add(2);
                        }
                        else if (rolesParam == "manager")
                        {
                            rolesParams.Add(1);
                        }
                    }
                    uriParams.Add("roles=" + string.Join(',', rolesParams));
                }

            }

            if (instructionsadded.HasValue)
            {
                uriParams.Add("instructionsadded=" + instructionsadded.ToString().ToLower());
            }

            if (photosadded.HasValue)
            {
                uriParams.Add("imagesadded=" + photosadded.ToString().ToLower());
            }

            if (videosadded.HasValue)
            {
                uriParams.Add("videosadded=" + videosadded.ToString().ToLower());
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
            return uriParams;
        }

        // GET: Task/Details/5
        [HttpGet]
        [Route("/task/details")]
        [Route("/task/details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.PropertyStructureVersion = _configurationHelper.GetValueAsInteger("AppSettings:PropertyStructureVersion");
            output.TaskTemplateAttachmentsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments");
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");
            output.Tags.TagGroups = await GetTagGroups();

            if (User.IsInRole("serviceaccount") && id > 0)
            {
                output.EnableJsonExtraction = true;
                output.ExtractionData = new ExtractionModel();
                output.ExtractionData.TemplateId = id;
                output.ExtractionData.ExtractionUriPart = "tasktemplate";
                var resultVersions = await _connector.GetCall(string.Format("/v1/export/tasktemplate/{0}/versions", id));
                if (resultVersions.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(resultVersions.Message))
                {
                    SortedList<DateTime, string> retrievedVersions = resultVersions.Message.ToObjectFromJson<SortedList<DateTime, string>>();
                    if (retrievedVersions != null && retrievedVersions.Any())
                    {
                        output.ExtractionData.Versions = new List<ExtractionModel.VersionModel>();
                        foreach (DateTime key in retrievedVersions.Keys)
                        {
                            output.ExtractionData.Versions.Add(new ExtractionModel.VersionModel() { CreatedOn = key, Version = retrievedVersions[key].ToString() });
                        }
                    }
                }
            }

            if (id == 0)
            {
                var companyId = User.GetProfile().Company.Id;
                output.CurrentTaskTemplate = new TaskTemplateModel
                {
                    Id = id,
                    CompanyId = companyId,
                    Recurrency = new TaskRecurrencyModel { CompanyId = companyId, RecurrencyType = "no recurrency" }
                };
            }
            else
            {
                string endpoint = string.Format(Logic.Constants.Task.GetTaskTemplateDetailUrl, id.ToString());
                var result = await _connector.GetCall(endpoint);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var completedTaskResults = await _connector.GetCall(string.Format(Logic.Constants.Task.GetCompletedTasksByTemplateId, Logic.Constants.General.NumberOfLastCompletedOnDetailsPage, id));
                    if (completedTaskResults.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        output.CompletedTasks = JsonConvert.DeserializeObject<List<TasksTask>>(completedTaskResults.Message);
                    }
                    output.CurrentTaskTemplate = JsonConvert.DeserializeObject<TaskTemplateModel>(result.Message);

                    if (output.Tags != null && output.CurrentTaskTemplate != null)
                    {
                        output.Tags.SelectedTags = output.CurrentTaskTemplate.Tags;
                        output.Tags.itemId = output.CurrentTaskTemplate.Id;
                    }
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.Forbidden || result.StatusCode == HttpStatusCode.BadRequest)
                {
                    return View("~/Views/Shared/_template_not_found.cshtml", output);
                }
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

            output.Filter.Areas = output.Areas;
            output.Recurrency = new RecurrencyViewModel(output.CurrentTaskTemplate?.Recurrency);
            output.Recurrency._CmsLanguage = output.CmsLanguage;
            output.Recurrency.Locale = output.Locale;

            var companyResult = await _connector.GetCall(Logic.Constants.Company.GetCompanyWithShifts);
            if (companyResult.StatusCode == HttpStatusCode.OK)
            {
                var Company = JsonConvert.DeserializeObject<CompanyModel>(companyResult.Message);

                if (Company != null)
                {
                    Company.Shifts ??= new();
                    var days = Company.Shifts.GroupBy(x => x.DayOfWeek).ToDictionary(g => g.Key, g => new List<SelectListItem>(g.Select(shift => new ShiftSelectItem(shift, output?.CurrentTaskTemplate?.Recurrency?.Shifts).Item).ToList()));
                    if (days.Any())
                    {
                        var unselecteddays = Company.Shifts.GroupBy(x => x.DayOfWeek).ToDictionary(g => g.Key, g => new List<SelectListItem>(g.Select(shift => new ShiftSelectItem(shift, output?.CurrentTaskTemplate?.Recurrency?.Shifts, false).Item).ToList()));

                        if (output.Recurrency.RecurrencyType == "no recurrency")
                        {
                            output.Recurrency.OnlyOnceShiftDays = days;
                            if (!output.Recurrency.Schedule.Date.HasValue)
                            {
                                // we have a new task.. set date (day) to today, then find first corresponding shifts and select
                                output.Recurrency.Schedule.Date = DateTime.Today;
                                // find first day from today that has shifts
                                DateTime dt = GetFirstShiftDate(DateTime.Today, days);
                                // if any preselect the shifts and adjust date if necessary
                                if (days.Any(x => x.Key == dt.DayOfWeek))
                                {
                                    var dayshifts = days.FirstOrDefault(x => x.Key == dt.DayOfWeek);
                                    //dayshifts.Value.ForEach(x => x.Selected = true);
                                    days.Remove(dt.DayOfWeek);
                                    days.Add(dayshifts.Key, dayshifts.Value);
                                    output.Recurrency.OnlyOnceShiftDays = days;
                                    output.Recurrency.Schedule.Date = dt;
                                }
                            }
                            output.Recurrency.ShiftDays = unselecteddays;
                        }
                        else
                        {
                            output.Recurrency.OnlyOnceShiftDays = unselecteddays;
                            output.Recurrency.ShiftDays = days;
                        }
                    }

                    if (output.ApplicationSettings?.Features?.TemplateSharingEnabled == true)
                    {
                        //set list of companies to pick from when sharing template
                        var companiesResponse = await _connector.GetCall(Logic.Constants.Holding.CompanyBasicsWithTemplateSharingEnabled);
                        if (companiesResponse.StatusCode == HttpStatusCode.OK)
                        {
                            List<CompanyBasic> companyBasics = companiesResponse.Message.ToObjectFromJson<List<CompanyBasic>>();
                            if (companyBasics != null)
                            {
                                if (Company.Id > 0)
                                    companyBasics = companyBasics.Where(comp => comp.Id != Company.Id).ToList();
                                output.CompaniesInHolding = companyBasics;
                            }
                        }
                    }
                }
            }

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.WorkInstructions = JsonConvert.DeserializeObject<List<WorkInstructionTemplate>>(resultworkinstructions.Message);
            }
            if (output.WorkInstructions == null)
            {
                output.WorkInstructions = new List<WorkInstructionTemplate>();
            }
            else
            {
                //replace with query filter on api (parameter still needs to be checked, for now filter in code)
                output.WorkInstructions = output.WorkInstructions.Where(x => x.WorkInstructionType == EZGO.Api.Models.Enumerations.InstructionTypeEnum.BasicInstruction).ToList();
            }

            if(id > 0)
            {
                var resultCurrentTaskCount = await _connector.GetCall(string.Format("/v1/tasktemplate/{0}/taskcounts", id.ToString()));
                if(resultCurrentTaskCount.StatusCode == HttpStatusCode.OK)
                {
                    output.CurrentTaskTemplate.PreviousTasksCount = JsonConvert.DeserializeObject<int>(resultCurrentTaskCount.Message);
                    output.Recurrency.PreviousTaskCount = output.CurrentTaskTemplate.PreviousTasksCount;
                }
            }

            if (id == 0)
            {
                output.IsNewTemplate = true;
            }

            output.CurrentTaskTemplate.ApplicationSettings = output.ApplicationSettings;
            output.Recurrency.ApplicationSettings = output.ApplicationSettings;

            if (output.CurrentTaskTemplate.Properties != null)
            {
                output.CurrentTaskTemplate.Properties = output.CurrentTaskTemplate.Properties.OrderBy(x => x.Index).ToList();
            }

            return View("Details", output);
        }

        /// <summary>
        /// Load a shared task template in the details view
        /// </summary>
        /// <param name="id">id of shared template</param>
        /// <returns>details view of shared task template</returns>
        [HttpGet]
        [Route("/task/shared/{id}")]
        public async Task<IActionResult> SharedDetails(int id)
        {
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.SharedTemplateId = id;
            output.PropertyStructureVersion = _configurationHelper.GetValueAsInteger("AppSettings:PropertyStructureVersion");
            output.TaskTemplateAttachmentsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments");
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");
            output.Tags.TagGroups = await GetTagGroups();

            if (id == 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }
            else
            {
                string endpoint = string.Format(Logic.Constants.SharedTemplates.GetSharedTemplateDetails, id.ToString());
                var result = await _connector.GetCall(endpoint);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    output.CurrentTaskTemplate = JsonConvert.DeserializeObject<TaskTemplateModel>(result.Message);
                    output.Tags.SelectedTags = output.CurrentTaskTemplate.Tags;
                    output.Tags.itemId = output.CurrentTaskTemplate.Id;

                    var companyId = User.GetProfile().Company.Id;
                    output.CurrentTaskTemplate.Recurrency = new TaskRecurrencyModel { CompanyId = companyId, RecurrencyType = "no recurrency" };
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.Forbidden || result.StatusCode == HttpStatusCode.BadRequest)
                {
                    return View("~/Views/Shared/_template_not_found.cshtml", output);
                }
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

            output.Filter.Areas = output.Areas;
            output.Recurrency = new RecurrencyViewModel(output.CurrentTaskTemplate?.Recurrency);
            output.Recurrency._CmsLanguage = output.CmsLanguage;
            output.Recurrency.Locale = output.Locale;

            var companyResult = await _connector.GetCall(Logic.Constants.Company.GetCompanyWithShifts);
            var Company = JsonConvert.DeserializeObject<CompanyModel>(companyResult.Message);

            if (Company != null)
            {
                var days = Company.Shifts.GroupBy(x => x.DayOfWeek).ToDictionary(g => g.Key, g => new List<SelectListItem>(g.Select(shift => new ShiftSelectItem(shift, output?.CurrentTaskTemplate?.Recurrency?.Shifts).Item).ToList()));
                if (days.Any())
                {
                    var unselecteddays = Company.Shifts.GroupBy(x => x.DayOfWeek).ToDictionary(g => g.Key, g => new List<SelectListItem>(g.Select(shift => new ShiftSelectItem(shift, output?.CurrentTaskTemplate?.Recurrency?.Shifts, false).Item).ToList()));

                    if (output.Recurrency.RecurrencyType == "no recurrency")
                    {
                        output.Recurrency.OnlyOnceShiftDays = days;
                        if (!output.Recurrency.Schedule.Date.HasValue)
                        {
                            // we have a new task.. set date (day) to today, then find first corresponding shifts and select
                            output.Recurrency.Schedule.Date = DateTime.Today;
                            // find first day from today that has shifts
                            DateTime dt = GetFirstShiftDate(DateTime.Today, days);
                            // if any preselect the shifts and adjust date if necessary
                            if (days.Any(x => x.Key == dt.DayOfWeek))
                            {
                                var dayshifts = days.FirstOrDefault(x => x.Key == dt.DayOfWeek);
                                //dayshifts.Value.ForEach(x => x.Selected = true);
                                days.Remove(dt.DayOfWeek);
                                days.Add(dayshifts.Key, dayshifts.Value);
                                output.Recurrency.OnlyOnceShiftDays = days;
                                output.Recurrency.Schedule.Date = dt;
                            }
                        }
                        output.Recurrency.ShiftDays = unselecteddays;
                    }
                    else
                    {
                        output.Recurrency.OnlyOnceShiftDays = unselecteddays;
                        output.Recurrency.ShiftDays = days;
                    }
                }

                if (output.ApplicationSettings?.Features?.TemplateSharingEnabled == true)
                {
                    //set list of companies to pick from when sharing template
                    var companiesResponse = await _connector.GetCall(Logic.Constants.Holding.CompanyBasicsWithTemplateSharingEnabled);
                    List<CompanyBasic> companyBasics = companiesResponse.Message.ToObjectFromJson<List<CompanyBasic>>();
                    if (Company.Id > 0)
                        companyBasics = companyBasics.Where(comp => comp.Id != Company.Id).ToList();
                    output.CompaniesInHolding = companyBasics;
                }
            }

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.WorkInstructions = JsonConvert.DeserializeObject<List<WorkInstructionTemplate>>(resultworkinstructions.Message);
            }
            if (output.WorkInstructions == null)
            {
                output.WorkInstructions = new List<WorkInstructionTemplate>();
            }
            else
            {
                //replace with query filter on api (parameter still needs to be checked, for now filter in code)
                output.WorkInstructions = output.WorkInstructions.Where(x => x.WorkInstructionType == EZGO.Api.Models.Enumerations.InstructionTypeEnum.BasicInstruction).ToList();
            }

            output.CurrentTaskTemplate.ApplicationSettings = output.ApplicationSettings;
            output.Recurrency.ApplicationSettings = output.ApplicationSettings;
            output.CurrentTaskTemplate.Recurrency = null;

            if (output.CurrentTaskTemplate.Properties != null)
            {
                output.CurrentTaskTemplate.Properties = output.CurrentTaskTemplate.Properties.OrderBy(x => x.Index).ToList();
            }

            return View("~/Views/Task/Details.cshtml", output);
        }

        [HttpGet]
        [Route("/task/gettemplate/{id}")]
        public async Task<String> GetTemplate(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.Task.GetTaskTemplateDetailUrl, id.ToString()));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var currentTaskTemplate = JsonConvert.DeserializeObject<TaskTemplateModel>(result.Message);
                if (currentTaskTemplate.Properties != null)
                {
                    currentTaskTemplate.Properties = currentTaskTemplate.Properties.OrderBy(x => x.Index).ToList();
                }

                if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments"))
                {
                    currentTaskTemplate.Attachments = null;
                }

                return currentTaskTemplate.ToJsonFromObject();
            }
            return result.Message;
        }

        [HttpGet]
        [Route("/task/getsharedtemplate/{id}")]
        public async Task<String> GetSharedTemplate(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.SharedTemplates.GetSharedTemplateDetails, id.ToString()));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var currentTaskTemplate = JsonConvert.DeserializeObject<TaskTemplateModel>(result.Message);
                if (currentTaskTemplate.Properties != null)
                {
                    currentTaskTemplate.Properties = currentTaskTemplate.Properties.OrderBy(x => x.Index).ToList();
                }
                var companyId = User.GetProfile().Company.Id;
                currentTaskTemplate.Recurrency ??= new TaskRecurrencyModel 
                { 
                    CompanyId = companyId, 
                    RecurrencyType = "no recurrency",
                    Schedule = new TaskScheduleModel()
                    {
                        Date = DateTime.Today,
                        MonthRecurrencyType = "day_of_month",
                        Week = 1,
                        Day = 1,
                        Month = 1,
                        Weekday0 = false,
                        Weekday1 = false,
                        Weekday2 = false,
                        Weekday3 = false,
                        Weekday4 = false,
                        Weekday5 = false,
                        Weekday6 = false,
                        IsOncePerWeek = false,
                        IsOncePerMonth = false
                    }
                };
                currentTaskTemplate.RecurrencyType = "no recurrency";

                if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments"))
                {
                    currentTaskTemplate.Attachments = null;
                }

                return currentTaskTemplate.ToJsonFromObject();
            }
            return result.Message;
        }

        [HttpGet]
        [Route("/task/getchecklists")]
        public async Task<String> GetChecklists()
        {
            List<AutocompleteModel> output = new List<AutocompleteModel>();
            var result = await _connector.GetCall(Logic.Constants.Checklist.GetChecklistTemplatesSimple);
            var list = JsonConvert.DeserializeObject<List<ChecklistTemplateModel>>(result.Message);
            string term = Request.Query["term"].ToString();

            foreach (var item in list.Where(m => m.Name.ToLower().Contains(term.ToLower())).ToList())
            {
                output.Add(new AutocompleteModel
                {
                    label = item.Name,
                    value = item.Id.ToString()
                });
            }
            return JsonConvert.SerializeObject(output);
        }

        [HttpGet]
        [Route("/task/getaudits")]
        public async Task<String> GetAudits()
        {
            List<AutocompleteModel> output = new List<AutocompleteModel>();
            var result = await _connector.GetCall(Logic.Constants.Audit.GetAuditTemplatesSimple);
            var list = JsonConvert.DeserializeObject<List<AuditTemplateModel>>(result.Message);
            string term = Request.Query["term"].ToString();

            foreach (var item in list.Where(m => m.Name.ToLower().Contains(term.ToLower())).ToList())
            {
                output.Add(new AutocompleteModel
                {
                    label = item.Name,
                    value = item.Id.ToString()
                });
            }

            return JsonConvert.SerializeObject(output);
        }

        [HttpGet]
        [Route("/task/getareas")]
        public async Task<IActionResult> GetAreas()
        {
            string uri = Logic.Constants.Task.GetTaskAreas;
            var result = await _connector.GetCall(uri);
            List<Area> area = new List<Area>();

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                area = JsonConvert.DeserializeObject<List<Area>>(result.Message);
            }

            return Ok(area);
        }

        [HttpGet]
        [Route("/task/overview/{id}")]
        public async Task<IActionResult> Overview(int id)
        {
            var endpoint = string.Format(Logic.Constants.Task.GetTaskTemplatesByAreaId, id);
            var result = await _connector.GetCall(endpoint);
            output.Tags.TagGroups = await GetTagGroups();
            output.TaskTemplates = JsonConvert.DeserializeObject<List<TaskTemplateModel>>(result.Message);
            output.CurrentTaskTemplate = new TaskTemplateModel();
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

            output.Filter.Areas = output.Areas;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.CurrentTaskTemplate.ApplicationSettings = output.ApplicationSettings;
            return View("~/Views/Task/_overview.cshtml", output);
        }

        // template saving
        [HttpPost]
        [Route("/task/duplicate/{id}")]
        public async Task<IActionResult> Duplicate(int id)
        {
            int outputId = 0;
            var endpoint = string.Format(Logic.Constants.Task.GetTaskTemplateDetailUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                TaskTemplateModel tmpl = JsonConvert.DeserializeObject<TaskTemplateModel>(result.Message);
                tmpl.Id = 0;
                tmpl.Name = "Copy of " + tmpl.Name;
                if (tmpl.Name.Length > 255)
                {
                    tmpl.Name = tmpl.Name.Substring(0, 255);
                }
                tmpl.RecurrencyType = tmpl.Recurrency.RecurrencyType;
                if (tmpl.Steps != null)
                {
                    foreach (TaskStepModel step in tmpl.Steps)
                    {
                        step.Id = 0;
                        step.TaskTemplateId = 0;
                    }
                }

                if (tmpl.Properties != null)
                {
                    foreach (TemplatePropertyModel property in tmpl.Properties)
                    {
                        property.Id = 0;
                        property.TaskTemplateId = 0;
                    }
                }

                if (tmpl.WorkInstructionRelations != null)
                {
                    foreach (var instructionRelation in tmpl.WorkInstructionRelations)
                    {
                        instructionRelation.Id = 0;
                        instructionRelation.TaskTemplateId = 0;
                    }
                }

                if (tmpl.Recurrency?.Schedule?.StartDate != null && tmpl.Recurrency.Schedule.StartDate <= DateTime.Now)
                {
                    tmpl.Recurrency.Schedule.StartDate = null;
                }

                if (tmpl.Recurrency?.Schedule?.EndDate != null && tmpl.Recurrency.Schedule.EndDate <= DateTime.Now)
                {
                    tmpl.Recurrency.Schedule.EndDate = null;
                }

                var postEndpoint = Logic.Constants.Task.PostNewTaskTemplate;
                
                if(!_configurationHelper.GetValueAsBool("AppSettings:EnableInitialTaskGenerationOnAdd"))
                {
                    postEndpoint = string.Format(Logic.Constants.Task.DuplicateTaskTemplate, false);
                }

                var newTemplateResult = await _connector.PostCall(postEndpoint, tmpl.ToJsonFromObject());

                if (newTemplateResult.StatusCode == HttpStatusCode.OK)
                {
                    TaskTemplateModel newTemplate = JsonConvert.DeserializeObject<TaskTemplateModel>(newTemplateResult.Message);
                    outputId = newTemplate.Id;
                }
            }
            return RedirectToAction("Details", new { id = outputId });
        }

        [HttpPost]
        [Route("/task/delete/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var endpoint = string.Format(Logic.Constants.Task.PostDeleteTaskTemplate, id);
            var result = await _connector.PostCall(endpoint, "false");
            return RedirectToAction("index", "task");
        }

        [HttpPost]
        [Route("task/share/{templateid}")]
        public async Task<IActionResult> ShareTemplate([FromRoute] int templateid, [FromBody] List<int> companyIds)
        {
            string json = companyIds.ToJsonFromObject();
            var result = await _connector.PostCall(string.Format(Logic.Constants.Task.ShareTaskTemplate, templateid), json);
            return StatusCode((int)result.StatusCode, result.ToJsonFromObject());
            
        }

        [HttpPost]
        [RequestSizeLimit(52428800)]
        [Route("/task/upload")]
        public async Task<string> upload(IFormCollection data)
        {
            //if no data is provided, return empty string
            if (data == null || data.Files == null || data.Files.Count == 0)
            {
                return string.Empty;
            }

            foreach (IFormFile item in data.Files)
            {
                //var fileContent = item;
                if (item != null && item.Length > 0)
                {
                    // get a stream
                    using (var ms = new MemoryStream())
                    {
                        item.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        using var form = new MultipartFormDataContent();
                        using var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        form.Add(fileContent, "file", Path.GetFileName(item.FileName));
                        int mediaType = 4;

                        switch (data["itemtype"])
                        {
                            case "template":
                                mediaType = 1;
                                break;

                            case "item":
                                mediaType = 2;
                                break;

                            case "step":
                                mediaType = 2;
                                break;
                        }

                        var endpoint = string.Format(Logic.Constants.Task.UploadPictureUrl, mediaType);

                        switch (data["filekind"])
                        {
                            case "doc":
                                endpoint = string.Format(Logic.Constants.Task.UploadDocsUrl, mediaType);
                                break;

                            case "video":
                                endpoint = string.Format(Logic.Constants.Task.UploadVideoUrl, mediaType);
                                break;
                        }

                        ApiResponse filepath = await _connector.PostCall(endpoint, form);
                        if (filepath != null && filepath.StatusCode != HttpStatusCode.OK)
                        {
                            //something went wrong, ignore and return empty string
                            return string.Empty;
                        }

                        string output = filepath.Message;
                        if (data["filekind"] != "video")
                        {
                            output = filepath.Message.Replace("media/", "");
                        }
                        return output;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        [HttpPost]
        [Route("/task/settemplate")]
        public async Task<String> SetTemplate([FromBody] TaskTemplateModel task)
        {
            var indexStepCntr = 0;
            /// be aware that all id's should be 0 before posting it to the api.
            if (task.Steps != null)
            {
                foreach (TaskStepModel step in task.Steps)
                {
                    indexStepCntr++;
                    step.Index = indexStepCntr;
                    if (step.isNew)
                    {
                        step.Id = 0;
                    }
                }
            }

            if (task.Properties != null)
            {
                var indexCounter = 0;
                foreach (TemplatePropertyModel property in task.Properties)
                {
                    if (property.isNew)
                    {
                        property.Id = 0;
                    }
                    property.Index = indexCounter;
                    indexCounter = indexCounter + 1;
                }
            }

            if (task.WorkInstructionRelations != null)
            {
                var indexCounter = 0;
                foreach (var instructionRelation in task.WorkInstructionRelations)
                {
                    instructionRelation.Index = indexCounter;
                    indexCounter = indexCounter + 1;
                }
            }

            var endpoint = Logic.Constants.Task.PostNewTaskTemplate;
            if (task.Id > 0)
            {
                endpoint = string.Format(Logic.Constants.Task.PostChangeTaskTemplate, task.Id);
            }

            var result = await _connector.PostCall(endpoint, task.ToJsonFromObject());
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var currentTaskTemplate = JsonConvert.DeserializeObject<TaskTemplateModel>(result.Message);
                if (currentTaskTemplate.Properties != null)
                {
                    currentTaskTemplate.Properties = currentTaskTemplate.Properties.OrderBy(x => x.Index).ToList();
                }

                if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments"))
                {
                    currentTaskTemplate.Attachments = null;
                }

                return currentTaskTemplate.ToJsonFromObject();
            }
            return result.Message;
        }
        // end template saving

        //TODO COMPLETE REBUILD OF RECURRENCY ASAP. remove backend handling and add to ezgolist. Or even preferably rebuild the ezgo list.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        [Route("/task/recurrency")]
        public async Task<JsonResult> Recurrency(TaskRecurrencyModel model, IFormCollection collection)
        {
            await Task.CompletedTask;

            //Fix for model state issues when posting new task without refreshing screen and changing something on the recurrency.
            if (!string.IsNullOrEmpty(collection["Id"]))
            {
                if (Convert.ToInt32(collection["Id"]) != 0 && model.Id == 0)
                {
                    model.Id = Convert.ToInt32(collection["Id"]);
                }
            }

            if (!string.IsNullOrEmpty(collection["CompanyId"]))
            {
                if (Convert.ToInt32(collection["CompanyId"]) != 0 && model.CompanyId == 0)
                {
                    model.CompanyId = Convert.ToInt32(collection["CompanyId"]);
                }
            }

            if (!string.IsNullOrEmpty(collection["TemplateId"]))
            {
                if (Convert.ToInt32(collection["TemplateId"]) != 0 && model.TemplateId == 0)
                {
                    model.TemplateId = Convert.ToInt32(collection["TemplateId"]);
                }
            }

            if (!string.IsNullOrEmpty(collection["AreaId"]))
            {
                if (Convert.ToInt32(collection["AreaId"]) != 0 && model.AreaId == 0)
                {
                    model.AreaId = Convert.ToInt32(collection["AreaId"]);
                }
            }

            RecurrencyTypeEnum recurrenyType = model.RecurrencyType.GetRecurrencyType();
            model.Shifts ??= new List<int>();
            model.Shifts = model.Shifts.Distinct().ToList();
            bool forever = (string.IsNullOrWhiteSpace(collection["Schedule.StartDate"]) && string.IsNullOrWhiteSpace(collection["Schedule.EndDate"]));

            switch (recurrenyType)
            {
                case RecurrencyTypeEnum.NoRecurrency:
                case RecurrencyTypeEnum.Shifts:
                    // set properties not used for these types to null
                    model.Schedule.Day = 1;
                    model.Schedule.Month = 1;
                    model.Schedule.MonthRecurrencyType = "day_of_month";
                    model.Schedule.Week = 1;
                    model.Schedule.WeekDay = null;
                    model.Schedule.Weekday0 = false;
                    model.Schedule.Weekday1 = false;
                    model.Schedule.Weekday2 = false;
                    model.Schedule.Weekday3 = false;
                    model.Schedule.Weekday4 = false;
                    model.Schedule.Weekday5 = false;
                    model.Schedule.Weekday6 = false;
                    model.Schedule.WeekDayNumber = null;

                    switch (recurrenyType)
                    {
                        case RecurrencyTypeEnum.NoRecurrency:
                            model.RecurrencyType = "no recurrency";
                            model.Schedule.StartDate = null;
                            model.Schedule.EndDate = null;

                            if (model.Schedule.Date == null)
                            {
                                model.Schedule.Date = DateTime.Now.Date;
                            }
                            break;

                        case RecurrencyTypeEnum.Shifts:
                            if (forever)
                            {
                                model.Schedule.StartDate = null;
                                model.Schedule.EndDate = null;
                            }

                            if (!model.Shifts.Any())
                            {
                                ModelState.AddModelError("Shifts", "No shifts present");
                            }
                            break;
                    }
                    break;

                case RecurrencyTypeEnum.Week:
                case RecurrencyTypeEnum.Month:
                    // set properties not used for these types to null
                    model.Schedule.Date = null;
                    if (forever)
                    {
                        model.Schedule.StartDate = null;
                        model.Schedule.EndDate = null;
                    }

                    switch (recurrenyType)
                    {
                        case RecurrencyTypeEnum.Week:
                            model.Schedule.Day = 1;
                            model.Schedule.Month = 1;
                            model.Schedule.MonthRecurrencyType = "day_of_month";
                            model.Schedule.WeekDay = null;
                            model.Schedule.WeekDayNumber = null;

                            if (model.Schedule.Week < 1)
                                ModelState.AddModelError("Week", "No week iteration present");

                            if (model.Schedule.Weekday0.GetValueOrDefault() == false &&
                                model.Schedule.Weekday1.GetValueOrDefault() == false &&
                                model.Schedule.Weekday2.GetValueOrDefault() == false &&
                                model.Schedule.Weekday3.GetValueOrDefault() == false &&
                                model.Schedule.Weekday4.GetValueOrDefault() == false &&
                                model.Schedule.Weekday5.GetValueOrDefault() == false &&
                                model.Schedule.Weekday6.GetValueOrDefault() == false)
                                ModelState.AddModelError("Week", "No weekday selected");
                            break;

                        case RecurrencyTypeEnum.Month:
                            model.Schedule.Week = 1;
                            model.Schedule.Weekday0 = false;
                            model.Schedule.Weekday1 = false;
                            model.Schedule.Weekday2 = false;
                            model.Schedule.Weekday3 = false;
                            model.Schedule.Weekday4 = false;
                            model.Schedule.Weekday5 = false;
                            model.Schedule.Weekday6 = false;

                            if (model.Schedule.MonthRecurrencyType.IsNullOrEmpty())
                                ModelState.AddModelError("Month", "No recurrency type selected");
                            else
                            {
                                if (model.Schedule.MonthRecurrencyType == "day_of_month")
                                {
                                    // defaulted properties based on 'MonthRecurrencyType': 'day_of_month'
                                    model.Schedule.WeekDay = null;
                                    model.Schedule.WeekDayNumber = null;

                                    if (model.Schedule.Day < 1)
                                        ModelState.AddModelError("Month", "No day selected");

                                    if (model.Schedule.Month < 1)
                                        ModelState.AddModelError("Month", "No month selected");
                                }
                                else if (model.Schedule.MonthRecurrencyType == "weekday")
                                {
                                    // defaulted properties based on 'MonthRecurrencyType': 'weekday'
                                    model.Schedule.Day = 1;

                                    if (model.Schedule.WeekDayNumber == null)
                                        ModelState.AddModelError("Month", "Day of the week must be set");

                                    if (model.Schedule.WeekDay == null)
                                        ModelState.AddModelError("Month", "Week day number must be set");
                                }

                                if (model.Schedule.Month < 1)
                                    ModelState.AddModelError("Month", "Month must be set");
                            }
                            break;
                    }
                    break;

                case RecurrencyTypeEnum.PeriodDay:
                    if (forever)
                    {
                        model.Schedule.StartDate = null;
                        model.Schedule.EndDate = null;
                    }

                    if (model.Schedule.Day < 1)
                        ModelState.AddModelError("PeriodDay", "Day must be set");
                    break;

                case RecurrencyTypeEnum.DynamicDay:
                    if (forever)
                    {
                        model.Schedule.StartDate = null;
                        model.Schedule.EndDate = null;
                    }

                    if (model.Schedule.Day < 1)
                        ModelState.AddModelError("DynamicDay", "Day must be set");
                    break;
            }

            if (ModelState.IsValid)
            {
                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = null
                };

                return Json(model, options);
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(err => err.ErrorMessage).ToList();
                return Json(errors);
            }
        }

        [NonAction]
        private DateTime GetFirstShiftDate(DateTime start, Dictionary<DayOfWeek, List<SelectListItem>> shifts)
        {
            DateTime end = start.AddDays(7);
            var start_org = start;
            for (var i = 1; i <= 7; i++)
            {
                if (shifts.Any(x => x.Key == start.DayOfWeek))
                {
                    break;
                }
                start = start_org.AddDays(i);
            }
            return start;
        }

        #region - indices functionality -
        /// <summary>
        /// OverviewIndices; containing a list of tasktemplates for use with setting indices;
        /// </summary>
        /// <returns></returns>
        [Route("/task/indices")]
        [HttpGet]
        public async Task<IActionResult> OverviewIndices()
        {
            output.TaskTemplates = new List<TaskTemplateModel>();
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

            output.Filter.Areas = output.Areas;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.Module = FilterViewModel.ApplicationModules.TASKINDICES;
            output.EnableTaskIndexingButtons = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskIndexingButtons");

            return View("~/Views/Task/Indices/IndexIndices.cshtml", output);
        }

        [Route("/task/indices/gettasks")]
        [HttpGet]
        public async Task<IActionResult> GetTasksForIndices([FromQuery] int offset = 0, [FromQuery] int limit = 500)
        {
            string uri = $"{Logic.Constants.Task.GetTaskTemplatesUrl}&limit={limit}&offset={offset}";
            var result = await _connector.GetCall(uri);
    
            if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.TaskTemplates = JsonConvert.DeserializeObject<List<TaskTemplateModel>>(result.Message);
                output.ApplicationSettings = await this.GetApplicationSettings();
                return PartialView("~/Views/Task/Indices/_overview.cshtml", output);
            }
    
            return BadRequest();
        }

        [Route("/task/indices/save")]
        [HttpPost]
        public async Task<IActionResult> SaveIndices([FromBody] List<IndexItem> templateIndices)
        {
            if (templateIndices != null && templateIndices.Count > 0)
            {
                var result = await _connector.PostCall("/v1/tasktemplates/setindexes", templateIndices.ToJsonFromObject());
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return StatusCode((int)HttpStatusCode.OK, true.ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, "".ToJsonFromObject());
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NoContent);
            }
        }
        #endregion

        [HttpGet]
        [Route("/task/getlatestchange/{id}")]
        public async Task<IActionResult> GetLatestChange(int id)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLatestTaskTemplateUrl, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpGet]
        [Route("/task/getchanges/{id}")]
        public async Task<IActionResult> GetChanges(int id, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingTaskTemplateUrl, id, limit, offset));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [NonAction]
        private async Task<List<TagGroup>> GetTagGroups()
        {
            var result = await _connector.GetCall(Logic.Constants.Tags.GetTagGroups);
            var tagGroups = new List<TagGroup>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                tagGroups = JsonConvert.DeserializeObject<List<TagGroup>>(result.Message);
                //filter tags to only include tags that are allowed on tasks
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true || 
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Task))).ToList());
            }

            return tagGroups;
        }

        [NonAction]
        private async Task<List<TagGroup>> GetTagGroupsForFilter()
        {
            var result = await _connector.GetCall(Logic.Constants.Tags.GetTagGroups);
            var tagGroups = new List<TagGroup>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                tagGroups = JsonConvert.DeserializeObject<List<TagGroup>>(result.Message);
                //filter tags to only include tags that are allowed on tasks
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true || 
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Task))).ToList());
            }

            return tagGroups;
        }
    }
}
