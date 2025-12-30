using System;
using System.Collections.Generic;
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
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Attributes;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Audit;
using WebApp.Models.Properties;
using WebApp.Models.Shared;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.Audits)]
    public class AuditController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public AuditController(ILogger<HomeController> logger,
                                IApiConnector connector,
                                ILanguageService language,
                                IHttpContextAccessor httpContextAccessor,
                                IConfigurationHelper configurationHelper,
                                IApplicationSettingsHelper applicationSettingsHelper,
                                IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [HttpGet]
        [Route("/audit")]
        public async Task<IActionResult> Index()
        {
            
            var output = new AuditViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.TagGroups = await this.GetTagGroupsForFilter();
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.AUDITS;
            output.Locale = _locale;

            output.CurrentAudit = new AuditTemplateModel();

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
            output.ApplicationSettings = await GetApplicationSettings();
            return View(output);
        }

        [HttpGet]
        [Route("/audit/getaudits")]
        public async Task<IActionResult> GetAudits([FromQuery] string filterText, [FromQuery] int areaid, [FromQuery] string tagids, [FromQuery] string roles, [FromQuery] bool? instructionsadded, [FromQuery] bool? photosadded, [FromQuery] int offset, [FromQuery] int limit)
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

            AuditViewModel output = new AuditViewModel();
            output.CmsLanguage = await _language.GetLanguageDictionaryAsync(_locale);
            output.Locale = _locale;

            var endpoint = @"/v1/audittemplates?include=tasktemplates,steps,tags,areapaths,areapathids,propertyvalues,property,instructionrelations";

            endpoint += "&" + string.Join("&", uriParams);

            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                output.AuditTemplates = JsonConvert.DeserializeObject<List<AuditTemplateModel>>(result.Message);
            }

            output.AuditTemplates ??= new List<AuditTemplateModel>();

            output.ApplicationSettings = await this.GetApplicationSettings();

            return PartialView("~/Views/Audit/_overview.cshtml", output);
        }

        [HttpGet]
        [Route("/audit/getauditcounts")]
        public async Task<IActionResult> GetAuditCounts([FromQuery] string filterText, [FromQuery] int areaid, [FromQuery] string tagids, [FromQuery] string roles, [FromQuery] bool? instructionsadded, [FromQuery] bool? photosadded, [FromQuery] int offset, [FromQuery] int limit)
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

            var endpoint = @"/v1/audittemplates_counts";
            if (uriParams.Count > 0)
            {
                endpoint += "?" + string.Join("&", uriParams);
            }
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(result.Message))
            {
                var stats = JsonConvert.DeserializeObject<AuditTemplateCountStatistics>(result.Message);
                return Ok(stats.TotalCount);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("/audit/details")]
        [Route("/audit/details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var output = new AuditViewModel();
            output.ApplicationSettings = await GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.TaskTemplateAttachmentsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments");
            output.Tags.itemId = id;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.AUDITS;
            output.Locale = _locale;
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");

            if (User.IsInRole("serviceaccount") && id > 0)
            {
                output.EnableJsonExtraction = true;
                output.ExtractionData = new ExtractionModel();
                output.ExtractionData.TemplateId = id;
                output.ExtractionData.ExtractionUriPart = "audittemplate";
                var resultVersions = await _connector.GetCall(string.Format("/v1/export/audittemplate/{0}/versions", id));
                if(resultVersions.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(resultVersions.Message))
                {
                    SortedList<DateTime, string> retrievedVersions = resultVersions.Message.ToObjectFromJson<SortedList<DateTime, string>>();
                    if(retrievedVersions != null && retrievedVersions.Any())
                    {
                        output.ExtractionData.Versions = new List<ExtractionModel.VersionModel>();
                        foreach (DateTime key in retrievedVersions.Keys)
                        {
                            output.ExtractionData.Versions.Add(new ExtractionModel.VersionModel() { CreatedOn = key, Version = retrievedVersions[key].ToString() });
                        }
                    }
                }
            }

            output.CurrentAudit = new AuditTemplateModel
            {
                Id = id,
                CompanyId = User.GetProfile().Company.Id,
                TaskTemplates = new List<AuditTaskTemplatesModel>()
            };

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

            var audit = await _connector.GetCall(string.Format(Logic.Constants.Audit.GetAuditTemplatesDetailUrl, id));
            if (audit.StatusCode == HttpStatusCode.OK)
            {
                var auditTemplate = JsonConvert.DeserializeObject<AuditTemplateModel>(audit.Message);
                output.CurrentAudit.ModifiedAt = auditTemplate.ModifiedAt;
                output.CurrentAudit.Name = auditTemplate.Name;
                output.CurrentAudit.Picture = auditTemplate.Picture;
                output.CurrentAudit.TaskTemplates = auditTemplate.TaskTemplates;
                output.Tags.SelectedTags = auditTemplate.Tags;
                output.Tags.itemId = auditTemplate.Id;
            }

            var completedAudits = await _connector.GetCall(string.Format(Logic.Constants.Audit.GetCompletedAuditsWithTemplateId, Logic.Constants.General.NumberOfLastCompletedOnDetailsPage, id));

            if (completedAudits.StatusCode == HttpStatusCode.OK)
            {
                output.CompletedAudits = JsonConvert.DeserializeObject<List<Audit>>(completedAudits.Message);
            }
            var scoreColorCalculator = ScoreColorCalculatorFactory.Default(1, 100);
            output.PercentageScoreColorCalculator = scoreColorCalculator;

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

            var connectedTaskTemplateIds = await _connector.GetCall(string.Format(Logic.Constants.Audit.GetConnectedTaskTemplateIds, id));

            if (connectedTaskTemplateIds.StatusCode == HttpStatusCode.OK)
            {
                output.ConnectedTaskTemplateIds = JsonConvert.DeserializeObject<List<int>>(connectedTaskTemplateIds.Message);
            }

            if (id == 0)
            {
                output.IsNewTemplate = true;
            }

            if (audit.StatusCode == HttpStatusCode.Forbidden || audit.StatusCode == HttpStatusCode.BadRequest && id != 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }
            else
            {
                return View("Details", output);
            }
        }

        /// <summary>
        /// Load shared audit template in the details view
        /// </summary>
        /// <param name="id">id of the shared template</param>
        /// <returns>Details view with shared template</returns>
        [HttpGet]
        [Route("/audit/shared/{id}")]
        public async Task<IActionResult> Shared(int id)
        {
            var output = new AuditViewModel();
            output.ApplicationSettings = await GetApplicationSettings();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.TaskTemplateAttachmentsEnabled = _configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments");
            output.Tags.itemId = 0;
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.AUDITS;
            output.Locale = _locale;
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");
            output.SharedTemplateId = id;

            output.CurrentAudit = new AuditTemplateModel
            {
                Id = 0,
                CompanyId = User.GetProfile().Company.Id,
                TaskTemplates = new List<AuditTaskTemplatesModel>()
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

            var audit = await _connector.GetCall(string.Format(Logic.Constants.SharedTemplates.GetSharedTemplateDetails, id));
            if (audit.StatusCode == HttpStatusCode.OK)
            {
                var auditTemplate = JsonConvert.DeserializeObject<AuditTemplateModel>(audit.Message);
                output.CurrentAudit.ModifiedAt = auditTemplate.ModifiedAt;
                output.CurrentAudit.Name = auditTemplate.Name;
                output.CurrentAudit.Picture = auditTemplate.Picture;
                output.Tags.SelectedTags = auditTemplate.Tags;
                output.Tags.itemId = auditTemplate.Id;
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

            if (audit.StatusCode == HttpStatusCode.Forbidden || audit.StatusCode == HttpStatusCode.BadRequest && id != 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }
            else
            {
                return View("~/Views/Audit/Details.cshtml", output);
            }
        }

        [HttpPost]
        [Route("/audit/duplicate/{id}")]
        public async Task<IActionResult> Duplicate(int id)
        {
            int outputId = 0;
            var endpoint = string.Format(Logic.Constants.Audit.GetAuditTemplatesDetailUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                AuditTemplateModel tmpl = JsonConvert.DeserializeObject<AuditTemplateModel>(result.Message);
                tmpl.Id = 0;
                tmpl.Name = "Copy of " + tmpl.Name;
                if (tmpl.Name.Length > 255)
                {
                    tmpl.Name = tmpl.Name.Substring(0, 255);
                }

                if (tmpl.TaskTemplates != null)
                {
                    foreach (AuditTaskTemplatesModel item in tmpl.TaskTemplates)
                    {
                        item.Id = 0;
                        if (item.Steps != null)
                        {
                            foreach (AuditStepModel step in item.Steps)
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
                                instructionRelation.AuditTemplateId = 0;
                            }
                        }

                    }
                }

                if (tmpl.OpenFieldsProperties != null)
                {
                    foreach (TemplatePropertyModel openfield in tmpl.OpenFieldsProperties)
                    {
                        openfield.Id = 0;
                        openfield.AuditTemplateId = 0;
                    }
                }

                var postEndpoint = Logic.Constants.Audit.PostNewAudit;
                var newTemplateResult = await _connector.PostCall(postEndpoint, tmpl.ToJsonFromObject());
                if (newTemplateResult.StatusCode == HttpStatusCode.OK)
                {
                    AuditTemplateModel newTemplate = JsonConvert.DeserializeObject<AuditTemplateModel>(newTemplateResult.Message);
                    outputId = newTemplate.Id;
                }
            }

            return RedirectToAction("Details", new { id = outputId });
        }

        [HttpGet]
        [Route("/audit/gettemplate/{id}")]
        public async Task<String> GetTemplate(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.Audit.GetAuditTemplatesDetailUrl, id.ToString()));

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments") && result.StatusCode == HttpStatusCode.OK)
            {
                var audit = result.Message.ToObjectFromJson<AuditTemplate>();
                if (audit != null && audit.TaskTemplates != null && audit.TaskTemplates.Count > 0)
                {
                    foreach (var taskTemplate in audit.TaskTemplates)
                    {
                        taskTemplate.Attachments = null;
                    }
                    return audit.ToJsonFromObject();
                }
            }

            return result.Message;
        }

        [HttpGet]
        [Route("/audit/getsharedtemplate/{id}")]
        public async Task<String> GetSharedTemplate(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.SharedTemplates.GetSharedTemplateDetails, id.ToString()));

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments") && result.StatusCode == HttpStatusCode.OK)
            {
                var audit = result.Message.ToObjectFromJson<AuditTemplate>();
                if (audit != null && audit.TaskTemplates != null && audit.TaskTemplates.Count > 0)
                {
                    foreach (var taskTemplate in audit.TaskTemplates)
                    {
                        taskTemplate.Attachments = null;
                    }
                    return audit.ToJsonFromObject();
                }
            }

            return result.Message;
        }

        [HttpGet]
        [Route("/audit/getname/{id}")]
        public async Task<String> GetName(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.Audit.GetAuditTemplatesDetailUrl, id.ToString()));
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

        [HttpGet]
        [Route("/audit/overview/{id}")]
        public async Task<IActionResult> Overview(int id)
        {
            var output = new AuditViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = _languageService.GetLanguageDictionaryAsync(_locale).Result;
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.AUDITS;
            output.Locale = _locale;

            var endpoint = string.Format(Logic.Constants.Audit.GetAuditTemplatesByAreaId, id);
            var result = await _connector.GetCall(endpoint);

            output.AuditTemplates = JsonConvert.DeserializeObject<List<AuditTemplateModel>>(result.Message);
            output.CurrentAudit = new AuditTemplateModel();

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

            output.ApplicationSettings = await GetApplicationSettings();
            return View("~/Views/Audit/_overview.cshtml", output);
        }

        [HttpPost]
        [Route("/audit/delete/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var endpoint = string.Format(Logic.Constants.Audit.PostDeleteAudit, id);
            var result = await _connector.PostCall(endpoint, "false");
            return RedirectToAction("index", "audit");
        }

        [HttpPost]
        [Route("audit/share/{templateid}")]
        public async Task<IActionResult> ShareTemplate([FromRoute] int templateid, [FromBody] List<int> companyIds)
        {
            string json = companyIds.ToJsonFromObject();
            var result = await _connector.PostCall(string.Format(Logic.Constants.Audit.ShareAuditTemplate, templateid), json);
            return StatusCode((int)result.StatusCode, result.ToJsonFromObject());
        }

        [HttpPost]
        [RequestSizeLimit(52428800)]
        [Route("/audit/upload")]
        public async Task<string> upload(IFormCollection data)
        {
            if (data != null && data.Files != null) {
                foreach (IFormFile item in data.Files)
                {
                    if (item != null && item.Length > 0)
                    {
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
                                    mediaType = 9;
                                    break;
                                case "item":
                                    mediaType = 10;
                                    break;
                                case "step":
                                    mediaType = 11;
                                    break;
                            }

                            var endpoint = string.Format(Logic.Constants.Audit.UploadPictureUrl, mediaType);
                            switch (data["filekind"])
                            {
                                case "doc":
                                    endpoint = string.Format(Logic.Constants.Audit.UploadDocsUrl, mediaType);
                                    break;
                                case "video":
                                    endpoint = string.Format(Logic.Constants.Audit.UploadVideoUrl, mediaType);
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
            }

            return string.Empty;
        }

        [HttpPost]
        [Route("/audit/settemplate")]
        public async Task<String> SetTemplate([FromBody] AuditTemplateModel audit)
        {
            var indexCntr = 0;
            var indexStepCntr = 0;
            if(audit != null)
            {
                /// be aware that all id's should be 0 before posting it to the api.
                if(audit.TaskTemplates != null)
                {

                    foreach (AuditTaskTemplatesModel item in audit.TaskTemplates)
                    {
                        indexCntr++;
                        item.Index = indexCntr;

                        if (item.isNew)
                        {
                            if (item.WorkInstructionRelations != null)
                            {
                                item.WorkInstructionRelations.ForEach(wr => { wr.TaskTemplateId = 0; wr.AuditTemplateId = 0; });
                            }
                            item.Id = 0;
                        }

                        indexStepCntr = 0;
                        if (item.Steps != null)
                        {
                            foreach (AuditStepModel step in item.Steps)
                            {
                                indexStepCntr++;
                                step.Index = indexStepCntr;
                                if (step.isNew)
                                {
                                    step.Id = 0;
                                }
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
                                indexCounter = indexCounter + 1;
                            }
                        }

                        if (item.WorkInstructionRelations != null)
                        {
                            var indexCounter = 0;
                            foreach (var instructionRelation in item.WorkInstructionRelations)
                            {
                                instructionRelation.Index = indexCounter;
                                indexCounter = indexCounter + 1;
                            }
                        }
                    }
                }

                var endpoint = Logic.Constants.Audit.PostNewAudit;
                if (audit.Id > 0)
                {
                    endpoint = string.Format(Logic.Constants.Audit.PostChangeAudit, audit.Id);
                }
                var result = await _connector.PostCall(endpoint, audit.ToJsonFromObject());

                if (!string.IsNullOrEmpty(result.Message))
                {
                    if (!_configurationHelper.GetValueAsBool("AppSettings:EnableTaskTemplateAttachments"))
                    {
                        var newAudit = result.Message.ToObjectFromJson<AuditTemplate>();

                        if (newAudit != null && newAudit.TaskTemplates != null && newAudit.TaskTemplates.Count > 0)
                        {
                            foreach (var taskTemplate in newAudit.TaskTemplates)
                            {
                                taskTemplate.Attachments = null;
                            }
                        }
                        return newAudit.ToJsonFromObject();
                    }
                }
                return result.Message;
            } else
            {
                return "Audit not saved, no information available.";
            }

 
        }

        [HttpGet]
        [Route("/audit/getlatestchange/{id}")]
        public async Task<IActionResult> GetLatestChange(int id)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLatestAuditTemplateUrl, id));
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
        [Route("/audit/getchanges/{id}")]
        public async Task<IActionResult> GetChanges(int id, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingAuditTemplateUrl, id, limit, offset));
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
                //filter tags to only include tags that are allowed on audits
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true ||
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Audit))).ToList());
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
                //filter tags to only include tags that are allowed on audits
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true ||
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Audit))).ToList());
            }

            return tagGroups;
        }
    }

}
