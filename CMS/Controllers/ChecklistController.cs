using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUglify.Html;
using WebApp.Attributes;
using WebApp.Logic.Converters;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Checklist;
using WebApp.Models.Properties;
using WebApp.Models.Shared;
using WebApp.Models.Skills;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Checklists)]
    public class ChecklistController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public ChecklistController(ILogger<HomeController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [Route("/checklist")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var output = new ChecklistViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.TagGroups = await this.GetTagGroupsForFilter();
            output.PageTitle = "Checklist overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CHECKLISTS;
            output.Locale = _locale;
            
            output.ChecklistTemplates ??= new List<ChecklistTemplateModel>();

            output.ApplicationSettings = await this.GetApplicationSettings();

            return View(output);
        }

        [HttpGet]
        [Route("/checklist/getchecklists")]
        public async Task<IActionResult> GetChecklists([FromQuery] string filterText, [FromQuery] int areaid, [FromQuery] string tagids, [FromQuery] string roles, [FromQuery] bool? instructionsadded, [FromQuery] bool? photosadded, [FromQuery] int offset, [FromQuery] int limit)
        {
            var uriParams = new List<string>();

            if (!string.IsNullOrEmpty(filterText))
            {
                uriParams.Add("filterText=" + System.Web.HttpUtility.UrlEncode(filterText));
            }

            if (areaid > 0)
            {
                uriParams.Add("areaid=" + areaid);
            }

            if (!string.IsNullOrEmpty(tagids))
            {
                uriParams.Add("tagids=" + tagids);
            }

            if (!string.IsNullOrEmpty(roles))
            {
                var rolesParams = new List<int>();
                var rolesSplit = roles.Split(',');

                if(rolesSplit.Length > 0)
                {
                    foreach(var rolesParam in rolesSplit)
                    {
                        if(rolesParam == "basic")
                        {
                            rolesParams.Add(0);
                        }
                        else if(rolesParam == "shift_leader")
                        {
                            rolesParams.Add(2);
                        }
                        else if(rolesParam == "manager")
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

            ChecklistViewModel output = new ChecklistViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;

            var endpoint = @"/v1/checklisttemplates?include=tasktemplates,steps,tags,areapaths,areapathids,propertyvalues,property,instructionrelations";

            endpoint += "&" + string.Join("&", uriParams);

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.ChecklistTemplates = JsonConvert.DeserializeObject<List<ChecklistTemplateModel>>(result.Message);
            }

            output.ChecklistTemplates ??= new List<ChecklistTemplateModel>();

            output.ApplicationSettings = await this.GetApplicationSettings();

            return PartialView("~/Views/Checklist/_overview.cshtml", output);
        }

        [HttpGet]
        [Route("/checklist/getchecklistcounts")]
        public async Task<IActionResult> GetChecklistCounts([FromQuery] string filterText, [FromQuery] int areaid, [FromQuery] string tagids, [FromQuery] string roles, [FromQuery] bool? instructionsadded, [FromQuery] bool? photosadded, [FromQuery] int offset, [FromQuery] int limit)
        {
            var uriParams = new List<string>();

            if (!string.IsNullOrEmpty(filterText))
            {
                uriParams.Add("filterText=" + System.Web.HttpUtility.UrlEncode(filterText));
            }

            if (areaid > 0)
            {
                uriParams.Add("areaid=" + areaid);
            }

            if (!string.IsNullOrEmpty(tagids))
            {
                uriParams.Add("tagids=" + tagids);
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

            var endpoint = @"/v1/checklisttemplates_counts";
            if (uriParams.Count > 0)
            {
                endpoint += "?" + string.Join("&", uriParams);
            }
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                var stats = JsonConvert.DeserializeObject<ChecklistTemplateCountStatistics>(result.Message);
                return Ok(stats.TotalCount);
            }

            return BadRequest();
        }

        public async Task<IActionResult> Viewer(int id)
        {
            var output = new ChecklistViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Tags.TagGroups = await GetTagGroups();
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.PageTitle = "Checklist overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CHECKLISTS;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.Locale = _locale;
            return View(output);
        }

        [Route("/checklist/details/{id}")]
        [Route("/checklist/details")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var output = new ChecklistViewModel();

            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.TaskTemplateAttachmentsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments");
            output.EnableStageTemplateShiftNotesAndSignatures = _configurationHelper.GetValueAsBool("AppSettings:EnableStageTemplateShiftNotesAndSignatures");
            output.Tags.TagGroups = await GetTagGroups();
            output.Tags.itemId = id;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.PageTitle = "Checklist overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CHECKLISTS;
            output.Locale = _locale;
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");

            if (User.IsInRole("serviceaccount") && id > 0)
            {
                output.EnableJsonExtraction = true;
                output.ExtractionData = new ExtractionModel();
                output.ExtractionData.TemplateId = id;
                output.ExtractionData.ExtractionUriPart = "checklisttemplate";
                var resultVersions = await _connector.GetCall(string.Format("/v1/export/checklisttemplate/{0}/versions", id));
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

            output.CurrentChecklistTemplate = new ChecklistTemplateModel
            {
                Id = id,
                CompanyId = User.GetProfile().Company.Id,
                TaskTemplates = new List<ChecklistTaskTemplatesModel>()
            };

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(resultworkinstructions.Message))
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

            output.CurrentChecklistTemplate.ApplicationSetttings = output.ApplicationSettings;
            var checklist = await _connector.GetCall(string.Format(Logic.Constants.Checklist.GetChecklistTemplateDetails, id));

            if (checklist.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(checklist.Message) && checklist.Message != "{}") //if message is empty object, means template not found
            {
                var checklistTemplate = JsonConvert.DeserializeObject<ChecklistTemplateModel>(checklist.Message);
                if(checklistTemplate != null)
                {
                    //added hasvalue check, shouldn't be an issue, but just to be sure
                    if (checklistTemplate.ModifiedAt.HasValue) output.CurrentChecklistTemplate.ModifiedAt = checklistTemplate.ModifiedAt;
                    output.CurrentChecklistTemplate.Name = checklistTemplate.Name;
                    output.CurrentChecklistTemplate.Picture = checklistTemplate.Picture;

                    if(output.Tags != null)
                    {
                        output.Tags.itemId = checklistTemplate.Id;
                        if (checklistTemplate.Tags?.Count > 0)
                        {
                            if(checklistTemplate.Tags != null)
                            {
                                output.Tags.SelectedTags = checklistTemplate.Tags;
                            }
                            
                        }
                    }
                }
            }
            
            if (output.ApplicationSettings?.Features?.TemplateSharingEnabled == true)
            {
                //get list of companies to share to
                var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
                UserProfile currentUser = null;
                int companyId = 0;

                if (!string.IsNullOrWhiteSpace(userprofile))
                {
                    currentUser = JsonConvert.DeserializeObject<UserProfile>(userprofile);
                    companyId = currentUser.Company.Id;
                }

                List<CompanyBasic> companyBasics = new List<CompanyBasic>();
                var companiesResponse = await _connector.GetCall(Logic.Constants.Holding.CompanyBasicsWithTemplateSharingEnabled);
                if (companiesResponse.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(companiesResponse.Message)) {
                    companyBasics = companiesResponse.Message.ToObjectFromJson<List<CompanyBasic>>();
                }

                if (companyId > 0)
                {
                    companyBasics = companyBasics.Where(comp => comp.Id != currentUser.Company.Id).ToList();
                }

                output.CompaniesInHolding = companyBasics;
            }

            var completedChecklists = await _connector.GetCall(string.Format(Logic.Constants.Checklist.GetCompletedChecklistsWithTemplateIdIncludeTasks, Logic.Constants.General.NumberOfLastCompletedOnDetailsPage, id));

            if (completedChecklists.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(completedChecklists.Message))
            {
                output.CompletedChecklists = JsonConvert.DeserializeObject<List<Checklist>>(completedChecklists.Message);
            }

            var connectedTaskTemplateIds = await _connector.GetCall(string.Format(Logic.Constants.Checklist.GetConnectedTaskTemplateIds, id));

            if (connectedTaskTemplateIds.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(connectedTaskTemplateIds.Message))
            {
                output.ConnectedTaskTemplateIds = JsonConvert.DeserializeObject<List<int>>(connectedTaskTemplateIds.Message);
            }

            if (id == 0)
            {
                output.IsNewTemplate = true;
            }

            if (checklist.StatusCode == HttpStatusCode.Forbidden || checklist.StatusCode == HttpStatusCode.BadRequest && id != 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }
            else
            {
                return View(output);
            }
        }


        
        /// <summary>
        /// Open a shared checklist template in de details view
        /// </summary>
        /// <param name="id">Id of the shared template</param>
        /// <returns>Details view of the shared template</returns>
        [HttpGet]
        [Route("/checklist/shared/{id}")]
        public async Task<IActionResult> Shared(int id)
        {
            var output = new ChecklistViewModel();

            output.SharedTemplateId = id;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.TaskTemplateAttachmentsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments");
            output.EnableStageTemplateShiftNotesAndSignatures = _configurationHelper.GetValueAsBool("AppSettings:EnableStageTemplateShiftNotesAndSignatures");
            output.Tags.TagGroups = await GetTagGroups();
            output.Tags.itemId = 0;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.PageTitle = "Shared checklist template";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CHECKLISTS;
            output.Locale = _locale;
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");

            output.CurrentChecklistTemplate = new ChecklistTemplateModel
            {
                Id = 0,
                CompanyId = User.GetProfile().Company.Id,
                TaskTemplates = new List<ChecklistTaskTemplatesModel>()
            };

            if (id == 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == HttpStatusCode.OK)
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

            if (output.ApplicationSettings?.Features?.TemplateSharingEnabled == true)
            {
                //get list of companies to share to
                var userprofile = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
                UserProfile currentUser = null;
                int companyId = 0;

                if (!string.IsNullOrWhiteSpace(userprofile))
                {
                    currentUser = JsonConvert.DeserializeObject<UserProfile>(userprofile);
                    companyId = currentUser.Company.Id;
                }

                var companiesResponse = await _connector.GetCall(Logic.Constants.Holding.CompanyBasicsWithTemplateSharingEnabled);
                List<CompanyBasic> companyBasics = companiesResponse.Message.ToObjectFromJson<List<CompanyBasic>>();

                if (companyId > 0)
                {
                    companyBasics = companyBasics.Where(comp => comp.Id != currentUser.Company.Id).ToList();
                }

                output.CompaniesInHolding = companyBasics;
            }

            output.CurrentChecklistTemplate.ApplicationSetttings = output.ApplicationSettings;
            var checklist = await _connector.GetCall(string.Format(Logic.Constants.SharedTemplates.GetSharedTemplateDetails, id));

            if (checklist.StatusCode == HttpStatusCode.OK)
            {
                var checklistTemplate = JsonConvert.DeserializeObject<ChecklistTemplateModel>(checklist.Message);
                output.CurrentChecklistTemplate.ModifiedAt = checklistTemplate.ModifiedAt;
                output.CurrentChecklistTemplate.Name = checklistTemplate.Name;
                output.CurrentChecklistTemplate.Picture = checklistTemplate.Picture;

                output.Tags.itemId = checklistTemplate.Id;
                if (checklistTemplate.Tags?.Count > 0)
                {
                    output.Tags.SelectedTags = checklistTemplate.Tags;
                }
            }

            if (checklist.StatusCode == HttpStatusCode.Forbidden || checklist.StatusCode == HttpStatusCode.BadRequest && id != 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }
            else
            {
                return View("~/Views/Checklist/Details.cshtml", output);
            }
        }

        [HttpPost]
        [Route("/checklist/duplicate/{id}")]
        public async Task<IActionResult> Duplicate(int id)
        {
            int outputId = 0;
            var endpoint = string.Format(Logic.Constants.Checklist.GetChecklistTemplateDetails, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                ChecklistTemplateModel tmpl = JsonConvert.DeserializeObject<ChecklistTemplateModel>(result.Message);
                tmpl.Id = 0;
                tmpl.Name = "Copy of " + tmpl.Name;
                if (tmpl.Name.Length > 255)
                {
                    tmpl.Name = tmpl.Name.Substring(0, 255);
                }

                if (tmpl.TaskTemplates != null)
                {
                    foreach (ChecklistTaskTemplatesModel item in tmpl.TaskTemplates)
                    {
                        item.Id = 0;
                        if (item.Steps != null)
                        {
                            foreach (ChecklistStepModel step in item.Steps)
                            {
                                step.Id = 0;
                                step.TaskTemplateId = 0;
                            }
                        }

                        if (item.Properties != null)
                        {
                            foreach (TemplatePropertyModel property in item.Properties)
                            {
                                property.Id = 0;
                                property.TaskTemplateId = 0;
                            }
                        }

                        if (item.WorkInstructionRelations != null)
                        {
                            foreach (var instructionRelation in item.WorkInstructionRelations)
                            {
                                instructionRelation.Id = 0;
                                instructionRelation.TaskTemplateId = 0;
                                instructionRelation.ChecklistTemplateId = 0;
                            }
                        }
                    }
                }
                if(tmpl.StageTemplates != null)
                {
                    foreach(StageTemplateModel stageTemplate in tmpl.StageTemplates)
                    {
                        stageTemplate.Id = 0;
                        stageTemplate.ChecklistTemplateId = 0;
                    }
                }

                if (tmpl.OpenFieldsProperties != null)
                {
                    foreach (TemplatePropertyModel openfield in tmpl.OpenFieldsProperties)
                    {
                        openfield.Id = 0;
                        openfield.ChecklistTemplateId = 0;
                    }
                }

                var postEndpoint = Logic.Constants.Checklist.PostNewChecklist;
                var newTemplateResult = await _connector.PostCall(postEndpoint, tmpl.ToJsonFromObject());

                if (newTemplateResult.StatusCode == HttpStatusCode.OK)
                {
                    ChecklistTemplateModel newTemplate = JsonConvert.DeserializeObject<ChecklistTemplateModel>(newTemplateResult.Message);
                    outputId = newTemplate.Id;
                }
            }
            return RedirectToAction("Details", new { id = outputId });
        }

        [HttpGet]
        [Route("/checklist/overview/{id}")]
        public async Task<IActionResult> Overview(int id)
        {
            var output = new ChecklistViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.PageTitle = "Checklist overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.CHECKLISTS;
            output.Locale = _locale;
            var endpoint = string.Format(Logic.Constants.Checklist.GetChecklistTemplatesByAreaId, id);
            var result = await _connector.GetCall(endpoint);

            if (result.StatusCode == HttpStatusCode.OK)
            {
                output.ChecklistTemplates = JsonConvert.DeserializeObject<List<ChecklistTemplateModel>>(result.Message);
                output.CurrentChecklistTemplate = new ChecklistTemplateModel();
            }

            output.ChecklistTemplates ??= new List<ChecklistTemplateModel>();

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
            return View("~/Views/Checklist/_overview.cshtml", output);
        }

        [HttpGet]
        [Route("/checklist/gettemplate/{id}")]
        public async Task<String> GetTemplate(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.Checklist.GetChecklistTemplateDetails, id.ToString()));

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments") && result.StatusCode == HttpStatusCode.OK)
            {
                var checklist = result.Message.ToObjectFromJson<ChecklistTemplate>();
                if (checklist != null && checklist.TaskTemplates != null && checklist.TaskTemplates.Count > 0)
                {
                    foreach (var taskTemplate in checklist.TaskTemplates)
                    {
                        taskTemplate.Attachments = null;
                    }
                    return checklist.ToJsonFromObject();
                }
            }

            return result.Message;
        }

        [HttpGet]
        [Route("/checklist/getsharedtemplate/{id}")]
        public async Task<String> GetSharedTemplate(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.SharedTemplates.GetSharedTemplateDetails, id.ToString()));

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments") && result.StatusCode == HttpStatusCode.OK)
            {
                var checklist = result.Message.ToObjectFromJson<ChecklistTemplate>();
                if (checklist != null && checklist.TaskTemplates != null && checklist.TaskTemplates.Count > 0)
                {
                    foreach (var taskTemplate in checklist.TaskTemplates)
                    {
                        taskTemplate.Attachments = null;
                    }
                    return checklist.ToJsonFromObject();
                }
            }

            return result.Message;
        }

        [HttpGet]
        [Route("/checklist/getname/{id}")]
        public async Task<String> GetName(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.Checklist.GetChecklistTemplateDetails, id.ToString()));

            if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                var tmpl = JsonConvert.DeserializeObject<IdNameModel>(result.Message);
                return tmpl?.Name ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        [HttpPost]
        [Route("/checklist/delete/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var endpoint = string.Format(Logic.Constants.Checklist.PostDeleteChecklist, id);
            var result = await _connector.PostCall(endpoint, "false");
            return RedirectToAction("index", "checklist");
        }

        [HttpPost]
        [RequestSizeLimit(52428800)]
        [Route("/checklist/upload")]
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
                                mediaType = 4;
                                break;

                            case "item":
                                mediaType = 5;
                                break;

                            case "step":
                                mediaType = 6;
                                break;
                        }

                        var endpoint = string.Format(Logic.Constants.Checklist.UploadPictureUrl, mediaType);
                        switch (data["filekind"])
                        {
                            case "doc":
                                endpoint = string.Format(Logic.Constants.Checklist.UploadDocsUrl, mediaType);
                                break;

                            case "video":
                                endpoint = string.Format(Logic.Constants.Checklist.UploadVideoUrl, mediaType);
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
        [Route("/checklist/settemplate")]
        public async Task<IActionResult> SetTemplate([FromBody] ChecklistTemplateModel checklist)
        {
            var indexStepCntr = 0;
            
            if (checklist == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Error occurred, checklist not complete.".ToJsonFromObject());
            }

            if (checklist.TaskTemplates?.Count > 0)
            {
                /// be aware that all id's should be 0 before posting it to the api.
                foreach (ChecklistTaskTemplatesModel item in checklist.TaskTemplates)
                {
                    if (item.isNew)
                    {
                        if (item.WorkInstructionRelations != null)
                        {
                            item.WorkInstructionRelations.ForEach(wr => { wr.TaskTemplateId = 0; wr.ChecklistTemplateId = 0; });
                        }
                        item.Id = 0;
                    }

                    indexStepCntr = 0;
                    foreach (ChecklistStepModel step in item.Steps)
                    {
                        indexStepCntr++;
                        step.Index = indexStepCntr;
                        if (step.isNew)
                        {
                            step.Id = 0;
                        }
                    }

                    if (item.Properties != null)
                    {
                        var indexCounter = 0;
                        foreach (TemplatePropertyModel property in item.Properties)
                        {
                            if (property.isNew)
                            {
                                property.Id = 0;
                                if (item.isNew)
                                {
                                    property.TaskTemplateId = 0;
                                }
                            }
                            property.Index = indexCounter;
                            indexCounter++;
                        }
                    }

                    if (item.WorkInstructionRelations != null)
                    {
                        var indexCounter = 0;
                        foreach (var instructionRelation in item.WorkInstructionRelations)
                        {
                            instructionRelation.Index = indexCounter;
                            indexCounter++;
                        }
                    }
                }
            }

            if (checklist.StageTemplates != null)
                foreach (StageTemplateModel stage in checklist.StageTemplates)
                {
                    if (stage.isNew)
                        stage.Id = 0;
                }

            var endpoint = Logic.Constants.Checklist.PostNewChecklist;
            if (checklist.Id > 0)
            {
                endpoint = string.Format(Logic.Constants.Checklist.PostChangeChecklist, checklist.Id);
            }

            var result = await _connector.PostCall(endpoint, checklist.ToJsonFromObject());

            if (!string.IsNullOrEmpty(result.Message))
            {
                if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments"))
                {
                    var newChecklist = result.Message.ToObjectFromJson<ChecklistTemplate>();

                    if (newChecklist != null && newChecklist.TaskTemplates != null && newChecklist.TaskTemplates.Count > 0)
                    {
                        foreach (var taskTemplate in newChecklist.TaskTemplates)
                        {
                            taskTemplate.Attachments = null;
                        }
                    }
                    return StatusCode((int)HttpStatusCode.OK, newChecklist.ToJsonFromObject());
                }
            }
            return StatusCode((int)result.StatusCode, result.Message);
        }

        [HttpPost]
        [Route("checklist/share/{templateid}")]
        public async Task<IActionResult> ShareTemplate([FromRoute] int templateid, [FromBody] List<int> companyIds)
        {
            string json = companyIds.ToJsonFromObject();
            var result = await _connector.PostCall(string.Format(Logic.Constants.Checklist.ShareChecklistTemplate, templateid), json);
            return StatusCode((int)result.StatusCode, result.ToJsonFromObject());
        }

        [HttpGet]
        [Route("/checklist/getlatestchange/{id}")]
        public async Task<IActionResult> GetLatestChange(int id)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLatestChecklistTemplateUrl, id));
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return StatusCode((int)result.StatusCode);
            }
        }

        [HttpGet]
        [Route("/checklist/getchanges/{id}")]
        public async Task<IActionResult> GetChanges(int id, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingChecklistTemplateUrl, id, limit, offset));
            if (result.StatusCode == HttpStatusCode.OK)
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
                //filter tags to only include tags that are allowed on checklists
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true ||
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Checklist))).ToList());
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
                //filter tags to only include tags that are allowed on checklists
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true ||
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Checklist))).ToList());
            }

            return tagGroups;
        }
    }
}
