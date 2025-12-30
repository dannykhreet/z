using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Shared;
using WebApp.ViewModels;

//TODO Add route attribute to all routes. 
namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.WorkInstructions)]
    public class WorkInstructionsController : BaseController
    {
        private readonly ILogger<WorkInstructionsController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public WorkInstructionsController(ILogger<WorkInstructionsController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [Route("/workinstructions")]
        public async Task<IActionResult> Index([FromQuery]string filter)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new WorkInstructionsViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;

            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.TagGroups = await this.GetTagGroupsForFilter();
            output.PageTitle = "Work instruction overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.WORKINSTRUCTIONS;
            output.Locale = _locale;

            var result = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.WorkInstructionTemplates = JsonConvert.DeserializeObject<List<WebApp.Models.WorkInstructions.WorkInstructionTemplate>>(result.Message);
            }
            if (output.WorkInstructionTemplates == null)
            {
                output.WorkInstructionTemplates = new List<WebApp.Models.WorkInstructions.WorkInstructionTemplate>();
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                output.Areas = new List<Area>();
            }
            output.Filter.Areas = output.Areas;
            output.WorkInstructionTypeFilter = filter ?? "";

            if (output.ApplicationSettings != null && output.ApplicationSettings.Features.SkillAssessments != null && !output.ApplicationSettings.Features.SkillAssessments.Value) {
                output.WorkInstructionTemplates = output.WorkInstructionTemplates.Where(w => w.WorkInstructionType != InstructionTypeEnum.SkillInstruction).ToList();
            }

            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/WorkInstructions/Index.cshtml", output);
        }

        [Route("/workinstructions/assessmentinstructions")]
        public async Task<IActionResult> AssessmentInstructions()
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new WorkInstructionsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Work instruction overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.WORKINSTRUCTIONS;
            output.Locale = _locale;

            var result = await _connector.GetCall(Logic.Constants.WorkInstructions.AssessmentInstructionTemplatesUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {

                output.WorkInstructionTemplates = JsonConvert.DeserializeObject<List<WebApp.Models.WorkInstructions.WorkInstructionTemplate>>(result.Message);
            }
            if (output.WorkInstructionTemplates == null)
            {
                output.WorkInstructionTemplates = new List<WebApp.Models.WorkInstructions.WorkInstructionTemplate>();
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                output.Areas = new List<Area>();
            }
            output.Filter.Areas = output.Areas;
            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/WorkInstructions/AssessmentInstructions.cshtml", output);
        }

        [Route("/workinstructions/workinstructions")]
        public async Task<IActionResult> WorkInstructions()
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new WorkInstructionsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Work instruction overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.WORKINSTRUCTIONS;
            output.Locale = _locale;

            var result = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionsTemplatesUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.WorkInstructionTemplates = JsonConvert.DeserializeObject<List<WebApp.Models.WorkInstructions.WorkInstructionTemplate>>(result.Message);
            }
            if (output.WorkInstructionTemplates == null)
            {
                output.WorkInstructionTemplates = new List<WebApp.Models.WorkInstructions.WorkInstructionTemplate>();
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                output.Areas = new List<Area>();
            }
            output.Filter.Areas = output.Areas;

            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/WorkInstructions/WorkInstructions.cshtml", output);
        }

        [Route("/workinstructions/get")]
        public async Task<IActionResult> GetWorkInstructions()
        {
            var result = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(result.Message);
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("/workinstructions/viewer/{id}")]
        public async Task<IActionResult> Viewer(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new WorkInstructionsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Work instruction viewer";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.WORKINSTRUCTIONS;
            output.Locale = _locale;

            var result = await _connector.GetCall(string.Format(Logic.Constants.WorkInstructions.WorkInstructionTemplateDetailsUrl, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentWorkInstructionTemplate = JsonConvert.DeserializeObject<WebApp.Models.WorkInstructions.WorkInstructionTemplate>(result.Message);
                output.Tags.SelectedTags = output.CurrentWorkInstructionTemplate.Tags;
                output.Tags.itemId = output.CurrentWorkInstructionTemplate.Id;
            }
            if (output.CurrentWorkInstructionTemplate == null)
            {
                output.CurrentWorkInstructionTemplate = new WebApp.Models.WorkInstructions.WorkInstructionTemplate();
            }

            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/WorkInstructions/Viewer.cshtml", output);
        }

        [Route("/workinstructions/add")]
        [Route("/workinstructions/details")]
        [Route("/workinstructions/details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new WorkInstructionsViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Work instruction details";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.WORKINSTRUCTIONS;
            output.Locale = _locale;
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");
            output.ShowAvailableForAllAreasToggle = _configurationHelper.GetValueAsBool("AppSettings:EnableWorkinstructionForAllAreasToggle");

            if (User.IsInRole("serviceaccount") && id > 0)
            {
                output.EnableJsonExtraction = true;
                output.ExtractionData = new ExtractionModel();
                output.ExtractionData.TemplateId = id;
                output.ExtractionData.ExtractionUriPart = "workinstructiontemplate";
                var resultVersions = await _connector.GetCall(string.Format("/v1/export/workinstructiontemplate/{0}/versions", id));
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

            var result = await _connector.GetCall(string.Format(Logic.Constants.WorkInstructions.WorkInstructionTemplateDetailsUrl, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentWorkInstructionTemplate = JsonConvert.DeserializeObject<WebApp.Models.WorkInstructions.WorkInstructionTemplate>(result.Message);
                output.Tags.SelectedTags = output.CurrentWorkInstructionTemplate.Tags;
                output.Tags.itemId = output.CurrentWorkInstructionTemplate.Id;
            }
            else if (result.StatusCode == HttpStatusCode.Forbidden || result.StatusCode == HttpStatusCode.BadRequest && id != 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }

            if (output.CurrentWorkInstructionTemplate == null)
            {
                output.CurrentWorkInstructionTemplate = new WebApp.Models.WorkInstructions.WorkInstructionTemplate();
                output.CurrentWorkInstructionTemplate.Id = 0;
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                output.Areas = new List<Area>();
            }
            output.Filter.Areas = output.Areas;

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
                companyBasics = companyBasics.Where(comp => comp.Id != companyId).ToList();
            }
            output.CompaniesInHolding = companyBasics;

            PrepareForOutput(output.CurrentWorkInstructionTemplate);

            if (id == 0)
            {
                output.IsNewTemplate = true;
            }

            return View("~/Views/WorkInstructions/Details.cshtml", output);
        }


        /// <summary>
        /// PrepareForOutput; Used for script backwards compatibility. This should be removed and optimized away ASAP;
        /// </summary>
        /// <param name="workinstructionTemplate"></param>
        [NonAction]
        public void PrepareForOutput(Models.WorkInstructions.WorkInstructionTemplate workinstructionTemplate)
        {
            workinstructionTemplate.TaskTemplates = workinstructionTemplate.InstructionItems;
            if (workinstructionTemplate.Role == "0")
            {
                workinstructionTemplate.Role = "basic";
            }
            if (workinstructionTemplate.Role == "1")
            {
                workinstructionTemplate.Role = "manager";
            }
            if (workinstructionTemplate.Role == "2")
            {
                workinstructionTemplate.Role = "shift_leader";
            }
        }


        [Route("/workinstruction/duplicate/{id}")]
        [HttpPost]
        public async Task<IActionResult> Duplicate(int id)
        {
            int outputId = 0;
            var endpoint = string.Format(Logic.Constants.WorkInstructions.WorkInstructionTemplateDetailsUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                WorkInstructionTemplate tmpl = JsonConvert.DeserializeObject<WorkInstructionTemplate>(result.Message);
                tmpl.Id = 0;
                tmpl.Name = "Copy of " + tmpl.Name;
                if (tmpl.Name.Length > 255)
                {
                    tmpl.Name = tmpl.Name.Substring(0, 255);
                }

                var postEndpoint = Logic.Constants.WorkInstructions.PostNewWorkInstruction;
                var newTemplateResult = await _connector.PostCall(postEndpoint, tmpl.ToJsonFromObject());
                if (newTemplateResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    WorkInstructionTemplate newTemplate = JsonConvert.DeserializeObject<WorkInstructionTemplate>(newTemplateResult.Message);
                    outputId = newTemplate.Id;
                }
            }
            return RedirectToAction("Details", new { id = outputId });
        }

        [Route("/workinstruction/delete/{id}")]
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var endpoint = string.Format(Logic.Constants.WorkInstructions.WorkInstructionTemplateDeleteUrl, id);
            var result = await _connector.PostCall(endpoint, "false");
            return RedirectToAction("index", "workinstructions");
        }

        /// <summary>
        /// Load shared work instruction tempalte into the details view
        /// </summary>
        /// <param name="sharedTemplateId">shared template id</param>
        /// <returns>Details view with shared template</returns>
        [Route("/workinstruction/shared/{sharedTemplateId}")]
        [HttpGet]
        public async Task<IActionResult> SharedDetails(int sharedTemplateId)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new WorkInstructionsViewModel();
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Work instruction details";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.WORKINSTRUCTIONS;
            output.Locale = _locale;
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");
            output.ShowAvailableForAllAreasToggle = _configurationHelper.GetValueAsBool("AppSettings:EnableWorkinstructionForAllAreasToggle");

            if (sharedTemplateId == 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }

            var result = await _connector.GetCall(string.Format(Logic.Constants.SharedTemplates.GetSharedTemplateDetails, sharedTemplateId));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentWorkInstructionTemplate = JsonConvert.DeserializeObject<WebApp.Models.WorkInstructions.WorkInstructionTemplate>(result.Message);
                output.Tags.SelectedTags = output.CurrentWorkInstructionTemplate.Tags;
                output.Tags.itemId = output.CurrentWorkInstructionTemplate.Id;
                output.SharedTemplateId = sharedTemplateId;
            }
            else if (result.StatusCode == HttpStatusCode.Forbidden || result.StatusCode == HttpStatusCode.BadRequest && sharedTemplateId != 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }

            if (output.CurrentWorkInstructionTemplate == null)
            {
                output.CurrentWorkInstructionTemplate = new WebApp.Models.WorkInstructions.WorkInstructionTemplate();
                output.CurrentWorkInstructionTemplate.Id = 0;
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            try
            {
                output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            catch
            {
                output.Areas = new List<Area>();
            }
            output.Filter.Areas = output.Areas;

            var companiesResponse = await _connector.GetCall(Logic.Constants.Holding.CompanyBasicsWithTemplateSharingEnabled);
            output.CompaniesInHolding = companiesResponse.Message.ToObjectFromJson<List<CompanyBasic>>();

            PrepareForOutput(output.CurrentWorkInstructionTemplate);

            return View("~/Views/WorkInstructions/Details.cshtml", output);
        }

        [HttpPost]
        [RequestSizeLimit(52428800)]
        [Route("/workinstruction/upload")]
        public async Task<string> upload(IFormCollection data)
        {
            if(data != null && data.Count > 0)
            {
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
                                    mediaType = 22;
                                    break;
                                case "item":
                                    mediaType = 23;
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
                            string output = string.Empty;
                            if (filepath.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(filepath.Message))
                            {
                                output = filepath.Message;
                                if (data["filekind"] != "video")
                                {
                                    output = filepath.Message.Replace("media/", "");
                                }
                            }

                            return output;
                        }

                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            
            return string.Empty;
        }

        //save
        [HttpPost]
        [Route("/workinstruction/settemplate")]
        public async Task<IActionResult> Save([FromBody] WebApp.Models.WorkInstructions.WorkInstructionTemplateWithNotificationData workinstructionWithNotificationData, [FromQuery] bool sendChangesNotification = false)
        {
            var indexStepCntr = 0;
            var workinstruction = workinstructionWithNotificationData?.WorkInstructionTemplate;
            var applicationSettings = await this.GetApplicationSettings();

            if (workinstruction == null)
            {
                return BadRequest("No work instruction data was provided. Work instruction not saved.");
            }

            /// be aware that all id's should be 0 before posting it to the api.
            if (workinstruction.InstructionItems != null && workinstruction.TaskTemplates != null)
            {
                workinstruction.InstructionItems = workinstruction.TaskTemplates; //for script compatibility
                for (int i = 0; i < workinstruction.InstructionItems.Count; i++)
                {
                    indexStepCntr++;
                    workinstruction.InstructionItems[i].Index = indexStepCntr;
                    if (workinstruction.InstructionItems[i].isNew)
                    {
                        workinstruction.InstructionItems[i].Id = 0;
                    }

                    if (workinstruction.InstructionItems[i].Picture == null && workinstruction.InstructionItems[i].Video != null && workinstruction.InstructionItems[i].VideoThumbnail != null)
                    {
                        workinstruction.InstructionItems[i].Media = new List<string>();
                        workinstruction.InstructionItems[i].Media.Add(workinstruction.InstructionItems[i].VideoThumbnail);
                        workinstruction.InstructionItems[i].Media.Add(workinstruction.InstructionItems[i].Video);
                    }
                }
            }

            var endpoint = "";
            if (workinstruction.Id == 0)
            {
                endpoint = Logic.Constants.WorkInstructions.PostNewWorkInstruction;
            }
            else if (workinstruction.Id > 0 && applicationSettings.Features.WorkInstructionsChangedNotificationsEnabled != null && applicationSettings.Features.WorkInstructionsChangedNotificationsEnabled.Value == true && sendChangesNotification)
            {
                endpoint = string.Format(Logic.Constants.WorkInstructions.PostChangeWorkInstructionExtended, workinstruction.Id);
            }
            else if (workinstruction.Id > 0)
            {
                endpoint = string.Format(Logic.Constants.WorkInstructions.PostChangeWorkInstruction, workinstruction.Id);
            }

            var role = GetRole(workinstruction.Role);
            workinstruction.Role = ""; //for jsonconvert compatibility

            var workInstructionJson = JsonConvert.SerializeObject(workinstruction);
            var instructionItemsJson = JsonConvert.SerializeObject(workinstruction.TaskTemplates);
            var workInstructionTemplate = JsonConvert.DeserializeObject<WorkInstructionTemplate>(workInstructionJson);
            workInstructionTemplate.InstructionItems = JsonConvert.DeserializeObject<List<InstructionItemTemplate>>(instructionItemsJson);
            workInstructionTemplate.Role = role;

            var wiTemplateWithNotificationData = new WorkInstructionTemplateWithNotificationData() 
            { 
                WorkInstructionTemplate = workInstructionTemplate, 
                NotificationComment = workinstructionWithNotificationData.NotificationComment 
            };

            ApiResponse result = null;

            if(workinstruction.Id > 0 && applicationSettings.Features.WorkInstructionsChangedNotificationsEnabled != null && applicationSettings.Features.WorkInstructionsChangedNotificationsEnabled.Value == true && sendChangesNotification)
            {
                result = await _connector.PostCall(endpoint, JsonConvert.SerializeObject(wiTemplateWithNotificationData));
            }
            else
            {
                result = await _connector.PostCall(endpoint, JsonConvert.SerializeObject(workInstructionTemplate));
            }

            //set tasktemplates for script compatibility
            if (result != null && result.StatusCode == HttpStatusCode.OK)
            {
                var newWorkInstructionTemplate = JsonConvert.DeserializeObject<WebApp.Models.WorkInstructions.WorkInstructionTemplate>(result.Message);
                newWorkInstructionTemplate.TaskTemplates = newWorkInstructionTemplate.InstructionItems;

                return Ok(JsonConvert.SerializeObject(newWorkInstructionTemplate, typeof(WebApp.Models.WorkInstructions.WorkInstructionTemplate), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong, workinstruction not saved");
            }
        }

        [HttpPost]
        [Route("/workinstruction/getchangesnotificationtable")]
        public async Task<IActionResult> GetWiChangesNotificationTable([FromBody] Models.WorkInstructions.WorkInstructionTemplatesComparison workInstructionTemplatesComparison)
        {
            var oldTemplate = workInstructionTemplatesComparison.OldTemplate;
            var newTemplate = workInstructionTemplatesComparison.NewTemplate;

            if (oldTemplate == null)
            {
                oldTemplate = new Models.WorkInstructions.WorkInstructionTemplate();
            }

            if (newTemplate == null)
            {
                newTemplate = new Models.WorkInstructions.WorkInstructionTemplate();
            }

            if (oldTemplate.TaskTemplates != null && oldTemplate.TaskTemplates.Count > 0)
            {
                for (int i = 1; i <= oldTemplate.TaskTemplates.Count; i++) 
                {
                    oldTemplate.TaskTemplates[i - 1].Index = i;
                }
            }

            if (newTemplate.TaskTemplates != null && newTemplate.TaskTemplates.Count > 0)
            {
                for (int i = 1; i <= newTemplate.TaskTemplates.Count; i++)
                {
                    newTemplate.TaskTemplates[i - 1].Index = i;
                }
            }

            var output = new WorkInstructionsChangedViewModel()
            {
                OldTemplate = oldTemplate,
                NewTemplate = newTemplate
            };


            output.ApplicationSettings = await this.GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();

            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            var resultTags = await _connector.GetCall(WebApp.Logic.Constants.Tags.GetTags);
            if (resultTags.StatusCode == HttpStatusCode.OK)
            {
                output.Tags = JsonConvert.DeserializeObject<List<Tag>>(resultTags.Message);
            }
            else
            {
                output.Tags = new List<Tag>();
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            var areas = new List<Area>();
            output.Areas = new List<Area>();
            if (resultTags.StatusCode == HttpStatusCode.OK)
            { 
                areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
            }
            else
            {
                areas = new List<Area>();
            }
            foreach (var area in areas)
            {
                output.Areas.AddRange(await GetAreasWithChildren(area));
            }

            output.NotificationData = GetChangesForWorkInstructionTemplate(oldTemplate, newTemplate);
            if (output.NotificationData != null && output.NotificationData.Count > 0)
            {
                return PartialView("_wi_changed_table", output);
            }
            return Ok();
        }

        [NonAction]
        public List<WorkInstructionTemplateChange> GetChangesForWorkInstructionTemplate(Models.WorkInstructions.WorkInstructionTemplate oldTemplate, Models.WorkInstructions.WorkInstructionTemplate newTemplate)
        {
            var workInstructionChanges = new List<WorkInstructionTemplateChange>();
            
            //gather changes with oldvalue not null
            if (oldTemplate.Name != newTemplate.Name)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "Name",
                    OldValue = oldTemplate.Name,
                    NewValue = newTemplate.Name,
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_NAME"
                });
            }

            if (oldTemplate.Description != newTemplate.Description)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "Description",
                    OldValue = oldTemplate.Description,
                    NewValue = newTemplate.Description,
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_DESCRIPTION"
                });
            }

            if (oldTemplate.AreaId != newTemplate.AreaId)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "AreaId",
                    OldValue = oldTemplate.AreaId.ToString(),
                    NewValue = newTemplate.AreaId.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_AREA_ID"
                });
            }

            if (!string.IsNullOrEmpty(oldTemplate.Picture) || !string.IsNullOrEmpty(newTemplate.Picture))
            {
                oldTemplate.Media = new List<string>() { oldTemplate.Picture };
                newTemplate.Media = new List<string>() { newTemplate.Picture };


                if (!oldTemplate.Media.SequenceEqual(newTemplate.Media))
                {
                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    var oldMedia = System.Text.Json.JsonSerializer.Serialize(oldTemplate.Media, options);
                    var newMedia = System.Text.Json.JsonSerializer.Serialize(newTemplate.Media, options);

                    workInstructionChanges.Add(new WorkInstructionTemplateChange()
                    {
                        PropertyName = "Media",
                        OldValue = oldMedia,
                        NewValue = newMedia,
                        TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_MEDIA"
                    });
                }
            }

            if (oldTemplate.WorkInstructionType != newTemplate.WorkInstructionType)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "WorkInstructionType",
                    OldValue = oldTemplate.WorkInstructionType.ToString(),
                    NewValue = newTemplate.WorkInstructionType.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_WORK_INSTRUCTION_TYPE"
                });
            }

            if (oldTemplate.Role != newTemplate.Role)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "Role",
                    OldValue = oldTemplate.Role?.ToString() ?? null,
                    NewValue = newTemplate.Role?.ToString() ?? null,
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_ROLE"
                });
            }

            if (oldTemplate.Tags != null || newTemplate.Tags != null)
            {
                if (oldTemplate.Tags == null)
                    oldTemplate.Tags = new List<Tag>();
                if (newTemplate.Tags == null)
                    newTemplate.Tags = new List<Tag>();

                var oldIds = oldTemplate.Tags.Select(x => x.Id).OrderBy(x => x).ToList();
                var newIds = newTemplate.Tags.Select(x => x.Id).OrderBy(x => x).ToList();

                if (!oldIds.SequenceEqual(newIds))
                {
                    workInstructionChanges.Add(new WorkInstructionTemplateChange()
                    {
                        PropertyName = "Tags",
                        OldValue = oldIds.ToJsonFromObject(),
                        NewValue = newIds.ToJsonFromObject(),
                        TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_TAGS"
                    });
                }
            }

            if (oldTemplate.IsAvailableForAllAreas != newTemplate.IsAvailableForAllAreas)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "IsAvailableForAllAreas",
                    OldValue = oldTemplate.IsAvailableForAllAreas.ToString(),
                    NewValue = newTemplate.IsAvailableForAllAreas.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_IS_AVAILABLE_FOR_ALL_AREAS"
                });
            }

            if (oldTemplate.TaskTemplates != null && newTemplate.TaskTemplates != null && oldTemplate.TaskTemplates.Count != newTemplate.TaskTemplates.Count)
            {
                workInstructionChanges.Add(new WorkInstructionTemplateChange()
                {
                    PropertyName = "NumberOfInstructionItems",
                    OldValue = oldTemplate.TaskTemplates?.Count.ToString() ?? 0.ToString(),
                    NewValue = newTemplate.TaskTemplates?.Count.ToString() ?? 0.ToString(),
                    TranslationKey = "CMS_WORKINSTRUCTIONS_CHANGES_NOTIFICATION_NUMBER_OF_INSTRUCTION_ITEMS"
                });
            }

            var itemChanges = GetChangesForWorkInstructionTemplateInstructionItems(oldItems: oldTemplate.TaskTemplates, newItems: newTemplate.TaskTemplates);
            if (itemChanges != null && itemChanges.Count > 0)
            {
                workInstructionChanges.AddRange(itemChanges);
            }

            return workInstructionChanges;
        }

        [NonAction]
        public List<WorkInstructionTemplateChange> GetChangesForWorkInstructionTemplateInstructionItems(List<Models.WorkInstructions.WorkInstructionItem> oldItems, List<Models.WorkInstructions.WorkInstructionItem> newItems)
        {
            var output = new List<WorkInstructionTemplateChange>();

            var oldIds = new List<int>();
            var newIds = new List<int>();

            if (oldItems != null)
            {
                oldIds = oldItems.Select(i => i.Id).ToList();
            }
            if (newItems != null)
            {
                newIds = newItems.Select(i => i.Id).ToList();
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            foreach (var oldId in oldIds)
            {
                if (newIds.Contains(oldId))
                {
                    var oldWiItemOnlyChanges = new InstructionItemTemplate();
                    var newWiItemOnlyChanges = new InstructionItemTemplate();

                    var oldWiItem = oldItems.Where(i => i.Id == oldId).FirstOrDefault();
                    var newWiItem = newItems.Where(i => i.Id == oldId).FirstOrDefault();

                    bool oldWiChanged = false;
                    //determine changed properties
                    //skip id, companyid, createdat, modifiedat, createdbyid, modifiedbyid for comparison
                    if (oldWiItem != null && newWiItem != null)
                    {
                        if (oldWiItem.InstructionTemplateId != newWiItem.InstructionTemplateId)
                        {
                            oldWiItemOnlyChanges.InstructionTemplateId = oldWiItem.InstructionTemplateId;
                            newWiItemOnlyChanges.InstructionTemplateId = newWiItem.InstructionTemplateId;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.AssessmentTemplateId != newWiItem.AssessmentTemplateId)
                        {
                            oldWiItemOnlyChanges.AssessmentTemplateId = oldWiItem.AssessmentTemplateId;
                            newWiItemOnlyChanges.AssessmentTemplateId = newWiItem.AssessmentTemplateId;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Name != newWiItem.Name)
                        {
                            oldWiItemOnlyChanges.Name = oldWiItem.Name;
                            newWiItemOnlyChanges.Name = newWiItem.Name;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Description != newWiItem.Description)
                        {
                            oldWiItemOnlyChanges.Description = oldWiItem.Description;
                            newWiItemOnlyChanges.Description = newWiItem.Description;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Picture != newWiItem.Picture)
                        {
                            oldWiItemOnlyChanges.Picture = oldWiItem.Picture;
                            newWiItemOnlyChanges.Picture = newWiItem.Picture;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Video != newWiItem.Video)
                        {
                            oldWiItemOnlyChanges.Video = oldWiItem.Video;
                            newWiItemOnlyChanges.Video = newWiItem.Video;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.VideoThumbnail != newWiItem.VideoThumbnail)
                        {
                            oldWiItemOnlyChanges.VideoThumbnail = oldWiItem.VideoThumbnail;
                            newWiItemOnlyChanges.VideoThumbnail = newWiItem.VideoThumbnail;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Media?.ToJsonFromObject() != newWiItem.Media?.ToJsonFromObject())
                        {
                            oldWiItemOnlyChanges.Media = oldWiItem.Media;
                            newWiItemOnlyChanges.Media = newWiItem.Media;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Index != newWiItem.Index)
                        {
                            oldWiItemOnlyChanges.Index = oldWiItem.Index;
                            newWiItemOnlyChanges.Index = newWiItem.Index;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Tags != null && newWiItem.Tags != null && oldWiItem.Tags.ToJsonFromObject() != newWiItem.Tags.ToJsonFromObject())
                        {
                            oldWiItemOnlyChanges.Tags = oldWiItem.Tags;
                            newWiItemOnlyChanges.Tags = newWiItem.Tags;
                            oldWiChanged = true;
                        }

                        if (oldWiItem.Attachments != null && newWiItem.Attachments != null && oldWiItem.Attachments.ToJsonFromObject() != newWiItem.Attachments.ToJsonFromObject())
                        {
                            oldWiItemOnlyChanges.Attachments = oldWiItem.Attachments;
                            newWiItemOnlyChanges.Attachments = newWiItem.Attachments;
                            oldWiChanged = true;
                        }

                        //store changed properties of item as json in one workinstructiontemplatechange
                        if (oldWiChanged)
                        {
                            output.Add(new WorkInstructionTemplateChange()
                            {
                                PropertyName = $"InstructionItem{oldWiItem.Id}",
                                OldValue = System.Text.Json.JsonSerializer.Serialize(oldWiItemOnlyChanges, options),
                                NewValue = System.Text.Json.JsonSerializer.Serialize(newWiItemOnlyChanges, options),
                                TranslationKey = ""
                            });
                        }
                    }
                }
                else
                {
                    var oldWiItem = oldItems.Where(i => i.Id == oldId).FirstOrDefault();
                    //item doesnt exist anymore
                    //new is empty, old is old wi template item
                    if (oldWiItem != null)
                    {
                        output.Add(new WorkInstructionTemplateChange()
                        {
                            PropertyName = $"InstructionItem{oldWiItem.Id}",
                            OldValue = System.Text.Json.JsonSerializer.Serialize(oldWiItem, options),
                            NewValue = null,
                            TranslationKey = ""
                        });
                    }
                }
            }
            foreach (var newId in newIds.Except(oldIds))
            {
                //new added item
                //new is new wi template item
                var newWiItem = newItems.Where(i => i.Id == newId).FirstOrDefault();

                //old is empty
                if (newWiItem != null)
                {
                    output.Add(new WorkInstructionTemplateChange()
                    {
                        PropertyName = $"InstructionItem{newWiItem.Id}",
                        OldValue = null,
                        NewValue = System.Text.Json.JsonSerializer.Serialize(newWiItem, options),
                        TranslationKey = ""
                    });
                }
            }

            return output;
        }

        public RoleTypeEnum GetRole(string role)
        {
            if (role.IsNullOrEmpty() || role.ToLower().Equals("basic"))
                return RoleTypeEnum.Basic;
            else if (role.ToLower().Equals("shift_leader"))
                return RoleTypeEnum.ShiftLeader;
            else if (role.ToLower().Equals("manager"))
                return RoleTypeEnum.Manager;

            return RoleTypeEnum.Basic;
        }

        [HttpPost]
        [Route("workinstruction/share/{templateid}")]
        public async Task<IActionResult> Share([FromRoute] int templateid, [FromBody] List<int> companyIds)
        {
            string json = companyIds.ToJsonFromObject();
            var result = await _connector.PostCall(string.Format(Logic.Constants.WorkInstructions.ShareWorkInstruction, templateid), json);
            return StatusCode((int)result.StatusCode, result.ToJsonFromObject());
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestChange(int id)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLatestWorkInstructionUrl, id));
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
        public async Task<IActionResult> GetChanges(int id, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingWorkInstructionUrl, id, limit, offset));
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
                //filter tags to only include tags that are allowed on workinstructions
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true || 
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.WorkInstruction))).ToList());
            }

            return tagGroups;
        }

        [NonAction]
        private async Task<List<Area>> GetAreasWithChildren(Area area) 
        {
            if(area == null) 
            { 
                return new List<Area>();
            }
            var areas = new List<Area>() { area };
            if(area.Children != null && area.Children.Count > 0)
            {
                foreach(var subArea in area.Children)
                {
                    areas.AddRange(await GetAreasWithChildren(subArea));
                }
            }
            return areas;
        }

        [NonAction]
        private async Task<List<TagGroup>> GetTagGroupsForFilter()
        {
            var result = await _connector.GetCall(Logic.Constants.Tags.GetTagGroups);
            var tagGroups = new List<TagGroup>();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                tagGroups = JsonConvert.DeserializeObject<List<TagGroup>>(result.Message);
                //filter tags to only include tags that are allowed on workinstructions
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true || 
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.WorkInstruction))).ToList());
            }

            return tagGroups;
        }
    }
}
