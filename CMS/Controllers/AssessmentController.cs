using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Attributes;
using WebApp.Logic.Converters;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Assessments;
using WebApp.Models.Skills;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class AssessmentController : BaseController
    {

        private readonly ILogger<SkillsController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public AssessmentController(ILogger<SkillsController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS)]
        [HttpGet]
        [Route("/assessment/index")]
        public async Task<IActionResult> Index()
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new AssessmentViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;

            output.Filter.ApplicationSettings = await this.GetApplicationSettings();
            output.Filter.TagGroups = await this.GetTagGroupsForFilter();

            output.PageTitle = "Skills overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLASSESSMENTS;
            output.Locale = _locale;

            var result = await _connector.GetCall(Logic.Constants.Assessments.GetAssessmentTemplates);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {

                output.SkillAssessmentTemplates = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.Skills.AssessmentTemplate>>(result.Message)).ToLocalAssessmentTemplates();
            }
            if (output.SkillAssessmentTemplates == null)
            {
                output.SkillAssessmentTemplates = new List<SkillAssessmentTemplate>();
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

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Users = (JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(resultUsers.Message)).OrderBy(x => x.LastName).ThenBy(y => y.FirstName).ToList();
            }

            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/Skills/Assessment/Index.cshtml", output);
        }

        // GET: /<controller>/
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS)]
        [HttpGet]
        [HttpPost]
        [Route("/assessment/viewer/{id}")]
        public async Task<IActionResult> Viewer([FromRoute] int id, SkillsViewerViewModel viewerViewModel)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new AssessmentViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Skills overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLASSESSMENTS;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();

            var result = await _connector.GetCall(string.Format(Logic.Constants.Assessments.GetAssessments, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {

                //output.SkillAssessmentTemplates = (JsonConvert.DeserializeObject<List<EZGO.Api.Models.Skills.AssessmentTemplate>>(result.Message)).ToLocalAssessmentTemplates();
                output.SkillAssessments = JsonConvert.DeserializeObject<List<SkillAssessment>>(result.Message).Where(m => m.IsCompleted.Equals(false)).ToList();
            }
            if (output.SkillAssessments == null)
            {
                output.SkillAssessments = new List<SkillAssessment>();
            }

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Users = (JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(resultUsers.Message)).OrderBy(x => x.LastName).ThenBy(y => y.FirstName).ToList();
            }

            return View("~/Views/Skills/Assessment/Viewer.cshtml", output);
        }



        [HttpPost]
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS)]
        [RequestSizeLimit(52428800)]
        [Route("/assessment/upload")]
        public async Task<IActionResult> Upload(IFormCollection data)
        {
            DateTime signDate = DateTime.UtcNow;
            AssessmentSignatureModel output = new AssessmentSignatureModel();
            var endpoint = Logic.Constants.Assessments.UploadPictureUrl;
            //foreach (IFormFile item in data.Files)
            //{
            var item = data.Files[0];
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

                    ApiResponse filepath = await _connector.PostCall(endpoint, form);

                    output = new AssessmentSignatureModel
                    {
                        SignatureImage = JsonConvert.DeserializeObject<string>(filepath.Message),
                        SignedAt = signDate,
                        SignedById = User.GetProfile().Id,
                        SignedBy = string.Format("{0} {1}", User.GetProfile().FirstName, User.GetProfile().LastName)
                    };

                }

                //}

            }

            return Ok(output);


        }



        //POST:
        [HttpPost]
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS)]
        [Route("/assessment/execute/{id}")]
        public async Task<IActionResult> Execute([FromRoute] int id, [FromBody] SkillsViewerViewModel users)
        {
            SkillAssessmentTemplate template = null;
            if (id > 0)
            {
                var result = await _connector.GetCall(string.Format(Logic.Constants.Assessments.GetAssessmentTemplateForCreation, id));
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    template = (JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.AssessmentTemplate>(result.Message)).ToLocalAssessmentTemplate();
                }
            }

            foreach (int participant in users.participants)
            {
                var output = await PrepareAssessmentForParticipant(template, participant);
                var endpoint = Logic.Constants.Assessments.PostNewAssessmentUrl;
                if (output.Id > 0)
                {
                    endpoint = string.Format(Logic.Constants.Assessments.PostChangeAssessmentUrl, output.Id);
                }
                var postresult = await _connector.PostCall(endpoint, output.ToJsonFromObject());
            }


            //todo: Add view

            return Ok(new { id = template.Id });
        }


        [NonAction]
        private async Task<AssessmentModel> PrepareAssessmentForParticipant(SkillAssessmentTemplate template, int participant)
        {

            AssessmentModel output = new AssessmentModel();

            List<AssessmentInstructionModel> assessmentInstructions = new List<AssessmentInstructionModel>();
            foreach (AssessmentTemplateSkillInstruction item in template.SkillInstructions)
            {
                var newInstruction = new AssessmentInstructionModel
                {
                    WorkInstructionTemplateId = (int)item.WorkInstructionTemplateId,
                    AssessmentTemplateSkillInstructionId = (int)item.Id,
                    CompletedForId = participant,
                    IsCompleted = false,
                    TotalScore = 0,
                    CompletedAt = DateTime.UtcNow
                };

                var instructionItems = new List<AssessmentInstructionItemModel>();
                foreach (InstructionItemTemplate instructionitem in item.InstructionItems)
                {
                    var newInstructionItem = new AssessmentInstructionItemModel
                    {
                        Score = 0,
                        WorkInstructionTemplateItemId = (int)instructionitem.Id,
                        IsCompleted = false,
                        CompletedForId = participant,
                        CompletedAt = DateTime.UtcNow

                    };
                    instructionItems.Add(newInstructionItem);
                }

                newInstruction.InstructionItems = instructionItems;
                assessmentInstructions.Add(newInstruction);
            }
            output.Id = 0;
            output.CompletedAt = DateTime.UtcNow;
            output.SkillInstructions = assessmentInstructions;
            output.IsCompleted = false;
            output.CompletedForId = participant;
            output.AssessorId = User.GetProfile().Id;
            output.TemplateId = template.Id;
            output.CompanyId = User.GetProfile().Company.Id;

            await Task.CompletedTask;

            return output;

        }


        //POST:
        [HttpPost]
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS)]
        [Route("/assessment/save")]
        public async Task<IActionResult> Save([FromBody] SkillAssessment assessment)
        {

            var endpoint = Logic.Constants.Assessments.PostChangeAssessmentUrl;
            if (assessment.Id > 0)
            {
                endpoint = string.Format(Logic.Constants.Assessments.PostChangeAssessmentUrl, assessment.Id);
            }

            var postresult = await _connector.PostCall(endpoint, assessment.ToJsonFromObject());


            return Ok(postresult.Message);
        }

        [HttpPost]
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS)]
        [Route("/assessment/delete/{assessmentid}")]
        public async Task<IActionResult> Delete([FromRoute] int assessmentid)
        {
            var endpoint = string.Format(Logic.Constants.Assessments.DeleteAssessment, assessmentid);
            await Task.CompletedTask;

            var postresult = await _connector.PostCall(endpoint, false.ToJsonFromObject());
            if (postresult.StatusCode == HttpStatusCode.OK)
            {
                return Ok(postresult.Message);
            }
            else
            {
                return BadRequest();
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
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Assessment))).ToList());
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
                //filter tags to only include tags that are allowed on workinstructions
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true || 
                ((tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.WorkInstruction)) || 
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Assessment)))).ToList());
            }

            return tagGroups;
        }

    }
}
