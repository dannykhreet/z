using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.Users;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic;
using WebApp.Logic.Converters;
using WebApp.Logic.Interfaces;
using WebApp.Models;
using WebApp.Models.Shared;
using WebApp.Models.Skills;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class SkillsController : BaseController
    {
        private readonly ILogger<SkillsController> _logger;
        private readonly IApiConnector _connector;
        private readonly ILanguageService _languageService;

        public SkillsController(ILogger<SkillsController> logger, IApiConnector connector, ILanguageService language, IHttpContextAccessor httpContextAccessor, IConfigurationHelper configurationHelper, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _logger = logger;
            _connector = connector;
            _languageService = language;
        }

        #region - assessments -
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [Route("/skillassessments")]
        public async Task<IActionResult> Index()
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new SkillsViewModel();
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

            var result = await _connector.GetCall(Logic.Constants.Skills.SkillAssessmentTemplatesUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.SkillAssessmentTemplates = (JsonConvert.DeserializeObject<List<AssessmentTemplate>>(result.Message)).ToLocalAssessmentTemplates();
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
            return View("~/Views/Skills/Index.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [HttpPost]
        [Route("/skillassessments/viewer/{id}")]
        public async Task<IActionResult> Viewer([FromRoute] int id, SkillsViewerViewModel viewerViewModel)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            //TODO add user check
            //TODO add error handling

            var output = new SkillsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Skills viewer";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLASSESSMENTS;
            output.Locale = _locale;
            output.SelectedUserId = viewerViewModel.UserId;

            var result = await _connector.GetCall(string.Format(Logic.Constants.Skills.SkillAssessmentTemplateDetailsUrl, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentSkillAssessmentTemplate = (JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.AssessmentTemplate>(result.Message)).ToLocalAssessmentTemplate();
                output.Tags.SelectedTags = output.CurrentSkillAssessmentTemplate.Tags;
                output.Tags.itemId = output.CurrentSkillAssessmentTemplate.Id;
            }

            if (output.CurrentSkillAssessmentTemplate == null)
            {
                output.CurrentSkillAssessmentTemplate = new SkillAssessmentTemplate();
            }

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Users = (JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(resultUsers.Message)).OrderBy(x => x.LastName).ThenBy(y => y.FirstName).ToList();
            }

            var userId = User.GetProfile()?.Id;
            if (userId.HasValue)
            {
                output.LoggedInUserId = userId.Value;
            }

            output.ApplicationSettings = await this.GetApplicationSettings();
            return View("~/Views/Skills/Viewer.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [Route("/skills/add")]
        [Route("/skillassessments/add")]
        [Route("/skillassessments/details")]
        [Route("/skillassessments/details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new SkillsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.Tags.TagGroups = await GetTagGroups();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Skills details";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLASSESSMENTS;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.EnablingAuditing = output.ApplicationSettings?.Features?.AuditTrailDetailsEnabled == true && User.IsInRole("manager");

            if (User.IsInRole("serviceaccount") && id > 0)
            {
                output.EnableJsonExtraction = true;
                output.ExtractionData = new ExtractionModel();
                output.ExtractionData.TemplateId = id;
                output.ExtractionData.ExtractionUriPart = "assessmenttemplate";
                var resultVersions = await _connector.GetCall(string.Format("/v1/export/assessmenttemplate/{0}/versions", id));
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

            var result = await _connector.GetCall(string.Format(Logic.Constants.Skills.SkillAssessmentTemplateDetailsUrl, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentSkillAssessmentTemplate = (JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.AssessmentTemplate>(result.Message)).ToLocalAssessmentTemplate();
                output.Tags.SelectedTags = output.CurrentSkillAssessmentTemplate.Tags;
                output.Tags.itemId = output.CurrentSkillAssessmentTemplate.Id;
            }
            else if (result.StatusCode == HttpStatusCode.Forbidden || result.StatusCode == HttpStatusCode.BadRequest && id != 0)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }

            var completedAssessments = await _connector.GetCall(string.Format(Logic.Constants.Skills.GetCompletedAssessmentsWithTemplateId, Logic.Constants.General.NumberOfLastCompletedOnDetailsPage, id));

            if (completedAssessments.StatusCode == HttpStatusCode.OK)
            {
                output.CompletedAssessments = JsonConvert.DeserializeObject<List<SkillAssessment>>(completedAssessments.Message);
            }

            var scoreColorCalculator = ScoreColorCalculatorFactory.Default(1, 5);
            output.AssessmentScoreColorCalculator = scoreColorCalculator;

            if (output.CurrentSkillAssessmentTemplate == null)
            {
                output.CurrentSkillAssessmentTemplate = new SkillAssessmentTemplate { Id = 0 };
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

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.WorkInstructionTemplates = JsonConvert.DeserializeObject<List<WebApp.Models.Skills.SkillAssessmentTemplateWorkInstructionTemplate>>(resultworkinstructions.Message);
            }

            if (output.WorkInstructionTemplates == null)
            {
                output.WorkInstructionTemplates = new List<WebApp.Models.Skills.SkillAssessmentTemplateWorkInstructionTemplate>();
            }
            else
            {
                //replace with query filter on api (parameter still needs to be checked, for now filter in code)
                output.WorkInstructionTemplates = output.WorkInstructionTemplates.Where(x => x.WorkInstructionType == EZGO.Api.Models.Enumerations.InstructionTypeEnum.SkillInstruction).ToList();
            }

            if (id == 0)
            {
                output.IsNewTemplate = true;
            }

            return View("~/Views/Skills/Details.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [Route("/skillassessment/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var endpoint = string.Format(Logic.Constants.Skills.PostDeleteAssessment, id);
            var result = await _connector.PostCall(endpoint, "false");
            return RedirectToAction("index", "skills");
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [Route("/skillassessment/duplicate/{id}")]
        public async Task<IActionResult> Duplicate(int id)
        {
            var result = await _connector.GetCall(string.Format(Logic.Constants.Skills.SkillAssessmentTemplateDetailsUrl, id));
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var tmpl = (JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.AssessmentTemplate>(result.Message)).ToLocalAssessmentTemplate();
                tmpl.Id = 0;
                tmpl.Name = "Copy of " + tmpl.Name;
                if (tmpl.Name.Length > 255)
                {
                    tmpl.Name = tmpl.Name.Substring(0, 255);
                }
                foreach (var item in tmpl.SkillInstructions)
                {
                    item.Id = 0;
                    item.AssessmentTemplateId = 0;
                    item.InstructionItems.Clear();
                }
                int outputId = 0;

                var postEndpoint = Logic.Constants.Skills.PostNewAssessment;
                var newTemplateResult = await _connector.PostCall(postEndpoint, tmpl.ToJsonFromObject());
                if (newTemplateResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var newTemplate = (JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.AssessmentTemplate>(newTemplateResult.Message)).ToLocalAssessmentTemplate();
                    outputId = newTemplate.Id;

                }
                return RedirectToAction("Details", new { id = outputId });
            }
            return RedirectToAction("index", "assessment");
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillAssessments)]
        [Route("/skillassessment/settemplate")]
        [HttpPost]
        public async Task<String> SetTemplate([FromBody] SkillAssessmentTemplate template)
        {
            var indexCntr = 0;
            /// be aware that all id's should be 0 before posting it to the api.
            if (template.TaskTemplates != null)
            {
                foreach (SkillAssessmentTemplateSkillInstruction item in template.TaskTemplates)
                {
                    if (!item.AssessmentTemplateId.HasValue || item.AssessmentTemplateId.Value == 0) item.AssessmentTemplateId = template.Id;

                    if (item.InstructionItems != null) item.InstructionItems.Clear(); // clear data not used

                    indexCntr++;
                    item.Index = indexCntr;

                    if (item.isNew) item.Id = 0;
                }
            }

            if (template.Picture == null)
            {
                template.Media = new List<string>();
            }

            var endpoint = Logic.Constants.Skills.PostNewAssessment;
            if (template.Id > 0)
            {
                endpoint = string.Format(Logic.Constants.Skills.PostChangeAssessment, template.Id);
            }

            var apiObject = template.ToApiAssessmentTemplate();
            var result = await _connector.PostCall(endpoint, apiObject.ToJsonFromObject());
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var output = (JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.AssessmentTemplate>(result.Message)).ToLocalAssessmentTemplate().ToJsonFromObject();
                return output;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region - matrices -
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrices")]
        public async Task<IActionResult> MatrixIndex()
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new SkillsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Skills Matrices overview";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLSMATRIX;
            output.Locale = _locale;

            var result = await _connector.GetCall(Logic.Constants.Skills.SkillMatricesUrl);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.SkillsMatrices = JsonConvert.DeserializeObject<List<Models.Skills.SkillsMatrix>>(result.Message);
            }

            if (output.SkillsMatrices == null)
            {
                output.SkillsMatrices = new List<Models.Skills.SkillsMatrix>();
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

            return View("~/Views/Skills/Matrix/Index.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [HttpPost]
        [Route("/skillsmatrices/add")]
        public async Task<IActionResult> MatrixAdd([FromForm] EZGO.Api.Models.Skills.SkillsMatrix newMatrix)
        {
            //Add matrix
            int matrixId = 0;

            if (newMatrix != null)
            {
                if (newMatrix.AreaId == 0)
                {
                    newMatrix.AreaId = null;
                }

                if (newMatrix.Description == null)
                {
                    newMatrix.Description = "";
                }
            }

            var postEndpoint = "/v1/skillsmatrix/add";
            var newMatrixResult = await _connector.PostCall(postEndpoint, newMatrix.ToJsonFromObject());
            if (newMatrixResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var matrix = JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.SkillsMatrix>(newMatrixResult.Message);

                    matrixId = matrix.Id;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            if (matrixId > 0)
            {
                return Redirect(string.Concat("/skillsmatrices/details/", matrixId));
            }
            else
            {
                return Redirect("/skillsmatrices");
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [HttpPost]
        [Route("/skillsmatrices/change")]
        public async Task<IActionResult> MatrixChange([FromForm] EZGO.Api.Models.Skills.SkillsMatrix editedMatrix)
        {
            //Change matrix
            int matrixId = 0;

            if(editedMatrix.AreaId == 0)
            {
                editedMatrix.AreaId = null;
            }

            var changeEndpoint = string.Format("/v1/skillsmatrix/change/{0}", editedMatrix.Id);
            var newMatrixResult = await _connector.PostCall(changeEndpoint, editedMatrix.ToJsonFromObject());
            if (newMatrixResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var matrix = JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.SkillsMatrix>(newMatrixResult.Message);
                matrixId = matrix.Id;
            }
            return Redirect("/skillsmatrices");
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrices/details/{id}")]
        public async Task<IActionResult> MatrixDetails(int id, [FromQuery] bool debug = false)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new SkillsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Skills matrix details";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLSMATRIX;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();

            var uriMatrix = string.Format(Logic.Constants.Skills.SkillMatrixDetailsUrl, id);
            if (debug)
            {
                uriMatrix = uriMatrix + "?debug=true";
            }

            var result = await _connector.GetCall(uriMatrix);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentSkillsMatrix = JsonConvert.DeserializeObject<Models.Skills.SkillsMatrix>(result.Message);
            }

            if (output.CurrentSkillsMatrix == null)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
            }

            string uri = Logic.Constants.Task.GetTaskAreas;
            var arearesult = await _connector.GetCall(uri);
            if (arearesult.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    output.Areas = JsonConvert.DeserializeObject<List<Area>>(arearesult.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            if (output.Areas == null) { output.Areas = new List<Area>(); }
            output.Filter.Areas = output.Areas;

            var resultUsers = await _connector.GetCall(Logic.Constants.User.UserPermissionUrl);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.Users = (JsonConvert.DeserializeObject<List<Models.User.UserProfile>>(resultUsers.Message)).OrderBy(x => x.LastName).ThenBy(y => y.FirstName).ToList();
            }

            var resultSkillMatrix = await _connector.GetCall(Logic.Constants.Skills.SkillAssessmentTemplatesUrl);
            if (resultSkillMatrix.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.SkillAssessmentTemplates = JsonConvert.DeserializeObject<List<SkillAssessmentTemplate>>(resultSkillMatrix.Message);
            }

            if (output.SkillAssessmentTemplates == null)
            {
                output.SkillAssessmentTemplates = new List<SkillAssessmentTemplate>();
            }

            string uriSkills = Logic.Constants.Skills.UserSkills;
            var skillsResult = await _connector.GetCall(uriSkills);
            if (skillsResult.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    output.UserSkills = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Users.UserSkill>>(skillsResult.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            if (output.UserSkills == null) { output.UserSkills = new List<EZGO.Api.Models.Users.UserSkill>(); }

            string uriAllMatrixAllUsers = @"/v1/skillsmatrix/users";
            var matrixUsersResult = await _connector.GetCall(uriAllMatrixAllUsers);
            if (matrixUsersResult.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    output.MatrixUsers = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Skills.SkillsMatrixUser>>(matrixUsersResult.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            if (output.MatrixUsers == null) { output.MatrixUsers = new List<EZGO.Api.Models.Skills.SkillsMatrixUser>(); }


            string endpointApplicabilities = @"/v1/userskillcustomtargets";
            var resultApplicabilities = await _connector.GetCall(endpointApplicabilities);
            var applicabilities = new List<UserSkillCustomTargetApplicability>();

            if (resultApplicabilities.StatusCode == HttpStatusCode.OK)
            {
                output.Applicabilities = resultApplicabilities.Message.ToObjectFromJson<List<UserSkillCustomTargetApplicability>>();
            }

            string uriGroups = @"/v1/usergroups";
            var groupResult = await _connector.GetCall(uriGroups);
            if (groupResult.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    output.UserGroups = JsonConvert.DeserializeObject<List<EZGO.Api.Models.Users.UserGroup>>(groupResult.Message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            if (output.UserGroups == null) { output.UserGroups = new List<EZGO.Api.Models.Users.UserGroup>(); }

            return View("~/Views/Skills/Matrix/Details.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillmatrices/operationalbehaviour/{id}")]
        public async Task<IActionResult> MatrixOperationalBehaviour(int id)
        {
            var output = new SkillsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Skills matrix details";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLSMATRIX;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();

            if (output.CurrentSkillsMatrix == null)
            {
                output.CurrentSkillsMatrix = new Models.Skills.SkillsMatrix() { Id = id };
            }

            var uriMatrixStatistics = string.Format(Logic.Constants.Skills.SkillMatrixStatisticsUrl, id);

            var resultStatistics = await _connector.GetCall(uriMatrixStatistics);
            if (resultStatistics.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentSkillsMatrix.OperationalBehaviours = JsonConvert.DeserializeObject<List<SkillsMatrixBehaviourItem>>(resultStatistics.Message);
            }

            var uriMatrixTotals = string.Format(Logic.Constants.Skills.SkillMatrixTotalsUrl, id);

            var resultTotals = await _connector.GetCall(uriMatrixTotals);
            if (resultTotals.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentSkillsMatrix.MatrixTotals = JsonConvert.DeserializeObject<List<SkillsMatrixBehaviourItem>>(resultTotals.Message);
            }

            var uriMatrixUserGroups = string.Format(Logic.Constants.Skills.SkillMatrixUserGroupsUrl, id);

            var resultUserGroups = await _connector.GetCall(uriMatrixUserGroups);
            if (resultUserGroups.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentSkillsMatrix.UserGroups = JsonConvert.DeserializeObject<List<SkillsMatrixUserGroup>>(resultUserGroups.Message);
            }

            var uriMatrixUsers = string.Format(Logic.Constants.Skills.SkillMatrixUsers, id);
            
            var resultUsers = await _connector.GetCall(uriMatrixUsers);
            if (resultUsers.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var users = JsonConvert.DeserializeObject<List<SkillsMatrixUser>>(resultUsers.Message);
                foreach (var group in output.CurrentSkillsMatrix.UserGroups)
                {
                    var foundUsers = users.Where(x => x.GroupId == group.UserGroupId).ToList();
                    group.Users = foundUsers;
                }
            }

            return PartialView("~/Views/Skills/Matrix/_operationalbehaviour.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrices/add")]
        public async Task<IActionResult> MatrixAdd()
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var output = new SkillsViewModel();
            output.NewInboxItemsCount = await GetInboxCount();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.PageTitle = "Skills add";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLSMATRIX;
            output.Locale = _locale;

            output.CurrentSkillsMatrix = new Models.Skills.SkillsMatrix();

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
            return View("~/Views/Skills/Matrix/Details.cshtml", output);
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrices/delete/{id}")]
        public async Task<IActionResult> MatrixDelete(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var endpoint = string.Format(Logic.Constants.Skills.PostDeleteMatrix, id);
            var result = await _connector.PostCall(endpoint, "false");
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixid}/groups/addchange")]
        public async Task<IActionResult> MatrixAddChangeGroup(int matrixid, [FromBody] EZGO.Api.Models.Skills.SkillsMatrixUserGroup group)
        {
            // return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var postEndpoint = "";
            if (group.Id > 0)
            {
                postEndpoint = string.Format("/v1/usergroups/change/{0}?fulloutput=true", group.Id); //change group
            }
            else
            {
                postEndpoint = string.Format("/v1/skillsmatrix/{0}/groups/add", matrixid);//add new group + add to skillmatrix
            }

            //NOTE still do conversion, for now it will work
            var resultGroup = await _connector.PostCall(postEndpoint, group.ToJsonFromObject());
            if (resultGroup.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (group.Id > 0)
                {
                    var result = resultGroup.Message.ToObjectFromJson<EZGO.Api.Models.Users.UserGroup>();
                    return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
                }
                else
                {
                    var result = resultGroup.Message.ToObjectFromJson<EZGO.Api.Models.Skills.SkillsMatrixUserGroup>();
                    var converted = new EZGO.Api.Models.Users.UserGroup() { Id = result.UserGroupId, Name = result.Description, Description = result.Description };
                    return StatusCode((int)HttpStatusCode.OK, converted.ToJsonFromObject());
                }
            }
            else
            {
                return StatusCode((int)resultGroup.StatusCode, resultGroup.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{id}/groups/removerelation")]
        public async Task<IActionResult> MatrixRemoveGroupRelation(int matrixId, [FromBody] EZGO.Api.Models.Relations.MatrixRelationUserGroup group)
        {
            //return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var matrixGroupRelation = new EZGO.Api.Models.Relations.MatrixRelationUserGroup() { Id = group.Id, UserGroupId = group.UserGroupId, MatrixId = group.MatrixId };
            var postEndpoint = string.Format("/v1/skillsmatrix/{0}/groups/removerelation", matrixGroupRelation.MatrixId);
            var removeGroupFromMatrixResult = await _connector.PostCall(postEndpoint, matrixGroupRelation.ToJsonFromObject());
            if (removeGroupFromMatrixResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)HttpStatusCode.OK, removeGroupFromMatrixResult.Message);
            }
            else
            {
                return StatusCode((int)removeGroupFromMatrixResult.StatusCode, removeGroupFromMatrixResult.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{id}/groups/addrelation")]
        public async Task<IActionResult> MatrixAddGroupRelation(int matrixId, [FromBody] EZGO.Api.Models.Relations.MatrixRelationUserGroup group)
        {
            //return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var matrixGroupRelation = new EZGO.Api.Models.Relations.MatrixRelationUserGroup() { Id = group.Id, UserGroupId = group.UserGroupId, MatrixId = group.MatrixId };
            var postEndpoint = string.Format("/v1/skillsmatrix/{0}/groups/addrelation", matrixGroupRelation.MatrixId);
            var removeGroupFromMatrixResult = await _connector.PostCall(postEndpoint, matrixGroupRelation.ToJsonFromObject());

            if (removeGroupFromMatrixResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = removeGroupFromMatrixResult.Message.ToObjectFromJson<int>();
                return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)removeGroupFromMatrixResult.StatusCode, removeGroupFromMatrixResult.Message);
            }
        }

        //skillsmatrix/{skillsmatrixid}/groups/remove
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{id}/group/{groupid}/users/add/{userid}")]
        public async Task<IActionResult> MatrixAddUserWithGroup(int id, int userid, int groupid)
        {
            //return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (groupid > 0 && userid > 0)
            {
                var userGroupRelation = new EZGO.Api.Models.Relations.UserGroupRelationUser() { GroupId = groupid, UserId = userid };
                var postEndpoint = string.Format("/v1/skillsmatrix/{0}/groups/users/add", id);
                var addUserToGroupResult = await _connector.PostCall(postEndpoint, userGroupRelation.ToJsonFromObject());
                if (addUserToGroupResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = addUserToGroupResult.Message.ToObjectFromJson<string>();
                    return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
                }
                else
                {
                    return StatusCode((int)addUserToGroupResult.StatusCode, addUserToGroupResult.Message);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Can not process incoming request".ToJsonFromObject());
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{id}/group/{groupid}/users/remove/{userid}")]
        public async Task<IActionResult> MatrixRemoveUserWithGroup(int id, int userid, int groupid)
        {
            //return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            if (groupid > 0 && userid > 0)
            {
                var userGroupRelation = new EZGO.Api.Models.Relations.UserGroupRelationUser() { GroupId = groupid, UserId = userid };
                var postEndpoint = string.Format("/v1/skillsmatrix/{0}/groups/users/remove", id);
                var addUserToGroupResult = await _connector.PostCall(postEndpoint, userGroupRelation.ToJsonFromObject());
                if (addUserToGroupResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = addUserToGroupResult.Message;
                    return StatusCode((int)HttpStatusCode.OK, result);
                }
                else
                {
                    return StatusCode((int)addUserToGroupResult.StatusCode, addUserToGroupResult.Message);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Can not process incoming request".ToJsonFromObject());
            }
        }
        #endregion

        #region - matrix skills -
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixId}/skills/removerelation")]
        public async Task<IActionResult> MatrixRemoveSkillRelation(int matrixId, [FromBody] EZGO.Api.Models.Relations.MatrixRelationUserSkill skill)
        {
            //return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working
            if(skill == null)
            {
                return StatusCode((int)HttpStatusCode.OK, "Skill is empty.");
            }

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var matrixSkillRelation = new EZGO.Api.Models.Relations.MatrixRelationUserSkill() { Id = skill.Id, UserSkillId = skill.UserSkillId, MatrixId = skill.MatrixId, Index = 0 };
            var postEndpoint = string.Format("/v1/skillsmatrix/{0}/skills/removerelation", matrixSkillRelation.MatrixId);
            var removeSkillFromMatrixResult = await _connector.PostCall(postEndpoint, matrixSkillRelation.ToJsonFromObject());
            if (removeSkillFromMatrixResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)HttpStatusCode.OK, removeSkillFromMatrixResult.Message);
            }
            else
            {
                return StatusCode((int)removeSkillFromMatrixResult.StatusCode, removeSkillFromMatrixResult.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixId}/skills/addrelation")]
        public async Task<IActionResult> MatrixAddSkillRelation(int matrixId, [FromBody] EZGO.Api.Models.Relations.MatrixRelationUserSkill skill)
        {
            //return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var matrixSkillRelation = new EZGO.Api.Models.Relations.MatrixRelationUserSkill() { Id = skill.Id, UserSkillId = skill.UserSkillId, MatrixId = skill.MatrixId, Index = 0 };
            var postEndpoint = string.Format("/v1/skillsmatrix/{0}/skills/addrelation", matrixSkillRelation.MatrixId);
            var removeSkillFromMatrixResult = await _connector.PostCall(postEndpoint, matrixSkillRelation.ToJsonFromObject());
            if (removeSkillFromMatrixResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = removeSkillFromMatrixResult.Message.ToObjectFromJson<int>();
                return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)removeSkillFromMatrixResult.StatusCode, removeSkillFromMatrixResult.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("skillsmatrix/{matrixId}/skills/changerelation")]
        public async Task<IActionResult> MatrixChangeSkillRelation(int matrixId, [FromBody] EZGO.Api.Models.Relations.MatrixRelationUserSkill matrixRelationUserSkill)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var postEndpoint = string.Format("/v1/skillsmatrix/{0}/skills/changerelation", matrixRelationUserSkill.MatrixId);
            var result = await _connector.PostCall(postEndpoint, matrixRelationUserSkill.ToJsonFromObject());
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixid}/skills/addchange")]
        public async Task<IActionResult> MatrixAddChangeSkill(int matrixid, [FromBody] EZGO.Api.Models.Skills.SkillsMatrixItem skill, [FromQuery] bool deleteoldvalues = false)
        {
            // return StatusCode((int)HttpStatusCode.OK, "disabled".ToJsonFromObject()); //disabled until working

            if (!_configurationHelper.GetValueAsBool("AppSettings:EnableSkillWorkInstructions"))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Forbidden".ToJsonFromObject());
            }

            var postEndpoint = "";
            if (skill.Id > 0)
            {
                postEndpoint = string.Format("/v1/userskill/change/{0}?fulloutput=true&deleteoldvalues={1}", skill.Id, deleteoldvalues); //change Skill
            }
            else
            {
                postEndpoint = string.Format("/v1/skillsmatrix/{0}/skills/add", matrixid);//add new Skill + add to skillmatrix
            }

            var skillToPost = skill.ToJsonFromObject();

            var resultSkill = await _connector.PostCall(postEndpoint, skillToPost);
            if (resultSkill.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (skill.Id > 0)
                {
                    var result = resultSkill.Message.ToObjectFromJson<EZGO.Api.Models.Users.UserSkill>();
                    return StatusCode((int)HttpStatusCode.OK, result.ToJsonFromObject());
                }
                else
                {
                    var result = resultSkill.Message.ToObjectFromJson<EZGO.Api.Models.Skills.SkillsMatrixItem>();
                    var converted = new EZGO.Api.Models.Users.UserSkill() { Id = result.UserSkillId, Name = result.Description, Description = result.Description };
                    return StatusCode((int)HttpStatusCode.OK, converted.ToJsonFromObject());
                }
            }
            else
            {
                return StatusCode((int)resultSkill.StatusCode, resultSkill.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixid}/skills/{userid}")]
        public async Task<IActionResult> MatrixGetUserSkills(int matrixid)
        {
            var endpoint = string.Format("/v1/skillsmatrix/{0}/skills", matrixid);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var matrixSkills = result.Message.ToObjectFromJson<List<SkillsMatrixItem>>();
                matrixSkills = matrixSkills.OrderBy(s => s.SkillType).ThenBy(s => s.Index).ToList();
                return StatusCode((int)HttpStatusCode.OK, matrixSkills.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixid}/skillvalues/{userid}")]
        public async Task<IActionResult> MatrixGetUserSkillValues(int matrixid, int userid)
        {
            var endpoint = string.Format("/v1/skillsmatrix/{0}/uservalues", matrixid);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var skillvalues = result.Message.ToObjectFromJson<List<UserSkillValue>>();
                skillvalues = skillvalues.Where(s => s.UserId == userid).OrderByDescending(s => s.ValueDate).ToList();
                return StatusCode((int)HttpStatusCode.OK, skillvalues.ToJsonFromObject());
            }
            else
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }
        }




        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixid}/skillvalue/save")]
        public async Task<IActionResult> MatrixSaveSkillValue(int matrixid, [FromBody] EZGO.Api.Models.Skills.SkillsMatrixItemValue skillValue)
        {
            var output = new MatrixUserSkillValueDetailsViewModel();
            var postEndpoint = string.Format("/v1/skillsmatrix/{0}/skills/value/save", matrixid);

            var resultSkillValue = await _connector.PostCall(postEndpoint, skillValue.ToJsonFromObject());
            if (resultSkillValue.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = resultSkillValue.Message;

                var resultSkill = await _connector.GetCall($"/v1/userstanding/skill/{skillValue.UserSkillId}");

                output.UserSkill = resultSkill.Message.ToObjectFromJson<SkillsMatrixItem>();
                output.UserSkill.UserSkillId = output.UserSkill.Id;

                var endpointuser = string.Format("/v1/userprofile/{0}", skillValue.UserId);
                var resultuser = await _connector.GetCall(endpointuser);
                var matrixUser = new UserProfile();

                if (resultuser.StatusCode == HttpStatusCode.OK)
                {
                    matrixUser = resultuser.Message.ToObjectFromJson<UserProfile>();
                    output.User = matrixUser;
                }

                output.UserSkillValue = skillValue;


                string endpointApplicabilities = @"/v1/userskillcustomtargets?userId=" + skillValue.UserId;
                var resultApplicabilities = await _connector.GetCall(endpointApplicabilities);
                var applicabilities = new List<UserSkillCustomTargetApplicability>();

                if (resultApplicabilities.StatusCode == HttpStatusCode.OK)
                {
                    applicabilities = resultApplicabilities.Message.ToObjectFromJson<List<UserSkillCustomTargetApplicability>>();
                }

                output.Applicability = applicabilities.Where(a => a.UserId == skillValue.UserId && a.UserSkillId == skillValue.UserSkillId).FirstOrDefault();

                output.MatrixId = matrixid;
                output.ApplicationSettings = await this.GetApplicationSettings();
                output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

                return PartialView("~/Views/Skills/Matrix/_userskillvaluemandatory.cshtml", model: output);
            }
            else
            {
                return StatusCode((int)resultSkillValue.StatusCode, resultSkillValue.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/skillvalue/remove/{matrixid}")]
        [HttpPost]
        public async Task<IActionResult> MatrixRemoveSkillValue([FromBody]UserSkillAndUserMetadata userSkillAndUserMetadata, [FromRoute] int matrixid)
        {
            var output = new MatrixUserSkillValueDetailsViewModel();
            var postEndpoint = string.Format("/v1/userskillvalues/remove");

            var userSkillAndUserMetadataForApi = new UserSkillAndUserMetadata()
            {
                UserId = userSkillAndUserMetadata.UserId,
                UserSkillId = userSkillAndUserMetadata.UserSkillId
            };

            var resultRemoveUserSkillValue = await _connector.PostCall(postEndpoint, userSkillAndUserMetadataForApi.ToJsonFromObject());
            if (resultRemoveUserSkillValue.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultSkill = await _connector.GetCall($"/v1/userstanding/skill/{userSkillAndUserMetadata.UserSkillId}");

                output.UserSkill = resultSkill.Message.ToObjectFromJson<SkillsMatrixItem>();
                output.UserSkill.UserSkillId = output.UserSkill.Id;
                output.UserSkillValue = null;

                var endpointuser = string.Format("/v1/userprofile/{0}", userSkillAndUserMetadata.UserId);
                var resultuser = await _connector.GetCall(endpointuser);
                var matrixUser = new UserProfile();

                if (resultuser.StatusCode == HttpStatusCode.OK)
                {
                    matrixUser = resultuser.Message.ToObjectFromJson<UserProfile>();
                    output.User = matrixUser;
                }


                string endpointApplicabilities = @"/v1/userskillcustomtargets?userId=" + userSkillAndUserMetadata.UserId;
                var resultApplicabilities = await _connector.GetCall(endpointApplicabilities);
                var applicabilities = new List<UserSkillCustomTargetApplicability>();

                if (resultApplicabilities.StatusCode == HttpStatusCode.OK)
                {
                    applicabilities = resultApplicabilities.Message.ToObjectFromJson<List<UserSkillCustomTargetApplicability>>();
                }

                output.Applicability = applicabilities.Where(a => a.UserId == userSkillAndUserMetadata.UserId && a.UserSkillId == userSkillAndUserMetadata.UserSkillId).FirstOrDefault();

                output.MatrixId = matrixid;
                output.ApplicationSettings = await this.GetApplicationSettings();
                output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

                return PartialView("~/Views/Skills/Matrix/_userskillvaluemandatory.cshtml", model: output);
            }
            else
            {
                return StatusCode((int)resultRemoveUserSkillValue.StatusCode, resultRemoveUserSkillValue.Message);
            }
        }




        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/applicability/set/mandatory/{matrixid}")]
        [HttpPost]
        public async Task<IActionResult> MatrixSetApplicabilityForUserWithUserSkillMandatory([FromBody] UserSkillCustomTargetApplicability applicability, [FromRoute] int matrixid)
        {
            var output = new MatrixUserSkillValueDetailsViewModel();

            var postEndpoint = string.Format("/v1/userskillcustomtarget/setapplicability");

            var applicabilityForApi = new UserSkillCustomTargetApplicability()
            {
                UserId = applicability.UserId,
                UserSkillId = applicability.UserSkillId,
                IsApplicable = applicability.IsApplicable,
                CustomTarget = applicability.IsApplicable ? applicability.CustomTarget : null
            };

            var resultSetApplicability = await _connector.PostCall(postEndpoint, applicabilityForApi.ToJsonFromObject());
            if (resultSetApplicability.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = resultSetApplicability.Message;

                var resultSkill = await _connector.GetCall($"/v1/userstanding/skill/{applicability.UserSkillId}");

                output.UserSkill = resultSkill.Message.ToObjectFromJson<SkillsMatrixItem>();
                output.UserSkill.UserSkillId = output.UserSkill.Id;

                var endpointuser = string.Format("/v1/userprofile/{0}", applicability.UserId);
                var resultuser = await _connector.GetCall(endpointuser);
                UserProfile matrixUser = null;

                if (resultuser.StatusCode == HttpStatusCode.OK)
                {
                    output.User = resultuser.Message.ToObjectFromJson<UserProfile>();
                }

                var resultSkillValue = await _connector.GetCall($"/v1/userskillvalues/byuserskill/{applicability.UserSkillId}/{applicability.UserId}");

                UserSkillValue userSkillValue = null;
                if (resultSkillValue.StatusCode == HttpStatusCode.OK)
                {
                    output.UserSkillValue = resultSkillValue.Message.ToObjectFromJson<UserSkillValue>();
                    if (output.UserSkillValue.Id == 0)
                        output.UserSkillValue = null;
                }

                string endpointApplicabilities = @"/v1/userskillcustomtargets?userId=" + applicabilityForApi.UserId;
                var resultApplicabilities = await _connector.GetCall(endpointApplicabilities);
                var applicabilities = new List<UserSkillCustomTargetApplicability>();

                if (resultApplicabilities.StatusCode == HttpStatusCode.OK)
                {
                    applicabilities = resultApplicabilities.Message.ToObjectFromJson<List<UserSkillCustomTargetApplicability>>();
                }

                output.Applicability = applicabilities.Where(a => a.UserId == applicabilityForApi.UserId && a.UserSkillId == applicabilityForApi.UserSkillId).FirstOrDefault();

                output.MatrixId = matrixid;
                output.ApplicationSettings = await this.GetApplicationSettings();
                output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

                return PartialView("~/Views/Skills/Matrix/_userskillvaluemandatory.cshtml", model: output);
            }
            else
            {
                return StatusCode((int)resultSetApplicability.StatusCode, resultSetApplicability.Message);
            }
        }

        //TODO TEST
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/applicability/remove/mandatory/{matrixid}")]
        [HttpPost]
        public async Task<IActionResult> MatrixRemoveApplicabilityForUserWithUserSkillMandatory([FromBody] UserSkillAndUserMetadata userSkillAndUserMetadata, [FromRoute] int matrixid)
        {
            var output = new MatrixUserSkillValueDetailsViewModel();
            var postEndpoint = string.Format("/v1/userskillcustomtarget/remove");

            var userSkillAndUserMetadataForApi = new UserSkillAndUserMetadata()
            {
                UserId = userSkillAndUserMetadata.UserId,
                UserSkillId = userSkillAndUserMetadata.UserSkillId
            };

            var resultRemoveApplicability = await _connector.PostCall(postEndpoint, userSkillAndUserMetadataForApi.ToJsonFromObject());
            if (resultRemoveApplicability.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = resultRemoveApplicability.Message;

                var resultSkill = await _connector.GetCall($"/v1/userstanding/skill/{userSkillAndUserMetadata.UserSkillId}");

                output.UserSkill = resultSkill.Message.ToObjectFromJson<SkillsMatrixItem>();
                output.UserSkill.UserSkillId = output.UserSkill.Id;

                var endpointuser = string.Format("/v1/userprofile/{0}", userSkillAndUserMetadata.UserId);
                var resultuser = await _connector.GetCall(endpointuser);
                UserProfile matrixUser = null;

                if (resultuser.StatusCode == HttpStatusCode.OK)
                {
                    output.User = resultuser.Message.ToObjectFromJson<UserProfile>();
                }

                var resultSkillValue = await _connector.GetCall($"/v1/userskillvalues/byuserskill/{userSkillAndUserMetadata.UserSkillId}/{userSkillAndUserMetadata.UserId}");

                UserSkillValue userSkillValue = null;
                if (resultSkillValue.StatusCode == HttpStatusCode.OK)
                {
                    output.UserSkillValue = resultSkillValue.Message.ToObjectFromJson<UserSkillValue>();
                    if (output.UserSkillValue.Id == 0)
                        output.UserSkillValue = null;
                }

                output.Applicability = null;

                output.MatrixId = matrixid;
                output.ApplicationSettings = await this.GetApplicationSettings();
                output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

                return PartialView("~/Views/Skills/Matrix/_userskillvaluemandatory.cshtml", model: output);
            }
            else
            {
                return StatusCode((int)resultRemoveApplicability.StatusCode, resultRemoveApplicability.Message);
            }
        }




        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/applicability/set/operational/{matrixid}")]
        [HttpPost]
        public async Task<IActionResult> MatrixSetApplicabilityForUserWithUserSkillOperational([FromBody] UserSkillCustomTargetApplicability applicability, [FromRoute] int matrixid)
        {
            var output = new MatrixCustomTargetDetailsViewModel();

            var postEndpoint = string.Format("/v1/userskillcustomtarget/setapplicability");

            var applicabilityForApi = new UserSkillCustomTargetApplicability()
            {
                UserId = applicability.UserId,
                UserSkillId = applicability.UserSkillId,
                IsApplicable = applicability.IsApplicable,
                CustomTarget = applicability.IsApplicable ? applicability.CustomTarget : null
            };

            var resultSetApplicability = await _connector.PostCall(postEndpoint, applicabilityForApi.ToJsonFromObject());
            if (resultSetApplicability.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = resultSetApplicability.Message;

                var resultSkill = await _connector.GetCall($"/v1/userstanding/skill/{applicability.UserSkillId}");

                output.UserSkill = resultSkill.Message.ToObjectFromJson<SkillsMatrixItem>();
                output.UserSkill.UserSkillId = output.UserSkill.Id;

                var endpointuser = string.Format("/v1/userprofile/{0}", applicability.UserId);
                var resultuser = await _connector.GetCall(endpointuser);
                UserProfile matrixUser = null;

                if (resultuser.StatusCode == HttpStatusCode.OK)
                {
                    output.User = resultuser.Message.ToObjectFromJson<UserProfile>();
                }

                var resultSkillValue = await _connector.GetCall($"/v1/userskillvalues/byuserskill/{applicability.UserSkillId}/{applicability.UserId}");

                UserSkillValue userSkillValue = null;
                if (resultSkillValue.StatusCode == HttpStatusCode.OK)
                {
                    output.UserSkillValue = resultSkillValue.Message.ToObjectFromJson<UserSkillValue>();
                    if (output.UserSkillValue.Id == 0)
                        output.UserSkillValue = null;
                }

                string endpointApplicabilities = @"/v1/userskillcustomtargets?userId=" + applicabilityForApi.UserId;
                var resultApplicabilities = await _connector.GetCall(endpointApplicabilities);
                var applicabilities = new List<UserSkillCustomTargetApplicability>();

                if (resultApplicabilities.StatusCode == HttpStatusCode.OK)
                {
                    applicabilities = resultApplicabilities.Message.ToObjectFromJson<List<UserSkillCustomTargetApplicability>>();
                }

                output.Applicability = applicabilities.Where(a => a.UserId == applicabilityForApi.UserId && a.UserSkillId == applicabilityForApi.UserSkillId).FirstOrDefault();

                output.MatrixId = matrixid;
                output.ApplicationSettings = await this.GetApplicationSettings();
                output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

                return PartialView("~/Views/Skills/Matrix/_userskillvalueoperational.cshtml", model: output);
            }
            else
            {
                return StatusCode((int)resultSetApplicability.StatusCode, resultSetApplicability.Message);
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/applicability/remove/operational/{matrixid}")]
        [HttpPost]
        public async Task<IActionResult> MatrixRemoveApplicabilityForUserWithUserSkillOperational([FromBody] UserSkillAndUserMetadata userSkillAndUserMetadata, [FromRoute] int matrixid)
        {
            var output = new MatrixCustomTargetDetailsViewModel();

            var postEndpoint = string.Format("/v1/userskillcustomtarget/remove");

            var userSkillAndUserMetadataForApi = new UserSkillAndUserMetadata()
            {
                UserId = userSkillAndUserMetadata.UserId,
                UserSkillId = userSkillAndUserMetadata.UserSkillId
            };

            var resultRemoveApplicability = await _connector.PostCall(postEndpoint, userSkillAndUserMetadataForApi.ToJsonFromObject());
            if (resultRemoveApplicability.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = resultRemoveApplicability.Message;

                var resultSkill = await _connector.GetCall($"/v1/userstanding/skill/{userSkillAndUserMetadata.UserSkillId}");

                output.UserSkill = resultSkill.Message.ToObjectFromJson<SkillsMatrixItem>();
                output.UserSkill.UserSkillId = output.UserSkill.Id;

                var endpointuser = string.Format("/v1/userprofile/{0}", userSkillAndUserMetadata.UserId);
                var resultuser = await _connector.GetCall(endpointuser);
                UserProfile matrixUser = null;

                if (resultuser.StatusCode == HttpStatusCode.OK)
                {
                    output.User = resultuser.Message.ToObjectFromJson<UserProfile>();
                }

                var resultSkillValue = await _connector.GetCall($"/v1/userskillvalues/byuserskill/{userSkillAndUserMetadata.UserSkillId}/{userSkillAndUserMetadata.UserId}");

                UserSkillValue userSkillValue = null;
                if (resultSkillValue.StatusCode == HttpStatusCode.OK)
                {
                    output.UserSkillValue = resultSkillValue.Message.ToObjectFromJson<UserSkillValue>();
                    if (output.UserSkillValue.Id == 0)
                        output.UserSkillValue = null;
                }

                output.Applicability = null;

                output.MatrixId = matrixid;
                output.ApplicationSettings = await this.GetApplicationSettings();
                output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

                return PartialView("~/Views/Skills/Matrix/_userskillvalueoperational.cshtml", model: output);
            }
            else
            {
                return StatusCode((int)resultRemoveApplicability.StatusCode, resultRemoveApplicability.Message);
            }
        }




        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/skillsmatrix/{matrixid}/skillvalues/{userid}/modalcontent")]
        public async Task<IActionResult> MatrixGetUserSkillValuesModalContent(int matrixid, int userid)
        {
            var output = new MatrixUserSkillValuesViewModel();

            var endpointvalues = string.Format("/v1/skillsmatrix/{0}/uservalues", matrixid);
            var resultvalues = await _connector.GetCall(endpointvalues);
            var skillvalues = new List<UserSkillValue>();

            if (resultvalues.StatusCode == HttpStatusCode.OK)
            {
                skillvalues = resultvalues.Message.ToObjectFromJson<List<UserSkillValue>>();
                skillvalues = skillvalues.Where(s => s.UserId == userid).OrderByDescending(s => s.ValueDate).ToList();
            }
            else
            {
                return StatusCode((int)resultvalues.StatusCode, resultvalues.Message);
            }

            var endpointskills = string.Format("/v1/skillsmatrix/{0}/skills", matrixid);
            var resultskills = await _connector.GetCall(endpointskills);
            var matrixSkills = new List<SkillsMatrixItem>();

            if (resultskills.StatusCode == HttpStatusCode.OK)
            {
                matrixSkills = resultskills.Message.ToObjectFromJson<List<SkillsMatrixItem>>();
                matrixSkills = matrixSkills.OrderBy(s => s.SkillType).ThenBy(s => s.Index).ToList();
            }
            else
            {
                return StatusCode((int)resultskills.StatusCode, resultskills.Message);
            }

            var endpointuser = string.Format("/v1/userprofile/{0}", userid);
            var resultuser = await _connector.GetCall(endpointuser);
            var matrixUser = new UserProfile();

            if(resultuser.StatusCode == HttpStatusCode.OK)
            {
                matrixUser = resultuser.Message.ToObjectFromJson<UserProfile>();
            }

            string endpointApplicabilities = @"/v1/userskillcustomtargets?userId=" + userid;
            var resultApplicabilities = await _connector.GetCall(endpointApplicabilities);
            var applicabilities = new List<UserSkillCustomTargetApplicability>();

            if (resultApplicabilities.StatusCode == HttpStatusCode.OK)
            {
                applicabilities = resultApplicabilities.Message.ToObjectFromJson<List<UserSkillCustomTargetApplicability>>();
            }

            output.UserSkills = matrixSkills;
            output.UserSkillValues = skillvalues;
            output.Applicabilities = applicabilities;
            output.MatrixId = matrixid;

            output.ApplicationSettings = await this.GetApplicationSettings();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            output.User = matrixUser;

            return PartialView("~/Views/Skills/Matrix/_userskillvaluesmodalcontent.cshtml", output);
        }




        #endregion


        #region - user skills and user groups
        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/usergroup/setactive/{id}")]
        [HttpPost]
        public async Task<IActionResult> SetUserGroupActive(int id, [FromBody] bool isActive)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            //set usergroup inactive
            ApiResponse result = await _connector.PostCall(string.Concat("/v1/usergroups/setactive/", id), false.ToJsonFromObject());

            if (result.StatusCode == HttpStatusCode.OK)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [FeatureAttribute(Feature = FeatureAttribute.FeatureFiltersEnum.SkillMatrix)]
        [Route("/userskill/setactive/{id}")]
        [HttpPost]
        public async Task<IActionResult> SetUserSkillActive(int id, [FromBody]bool isActive)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            //set userskill inactive
            ApiResponse result = await _connector.PostCall(string.Concat("/v1/userskill/setactive/", id), false.ToJsonFromObject());

            if(result.StatusCode == HttpStatusCode.OK)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        [Route("/skillassessment/upload")]
        [HttpPost]
        [RequestSizeLimit(52428800)]
        public async Task<string> upload(IFormCollection data)
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

                        int mediaType = 24;
                        var endpoint = string.Format(Logic.Constants.Skills.UploadPictureUrl, mediaType);

                        ApiResponse filepath = await _connector.PostCall(endpoint, form);
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

        [HttpGet]
        public async Task<IActionResult> GetLatestChange(int id)
        {
            if (id <= 0) return StatusCode((int)HttpStatusCode.NoContent);

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingLatestAssessmentTemplateUrl, id));
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

            var result = await _connector.GetCall(string.Format(Logic.Constants.AuditingLog.AuditingAssessmentTemplateUrl, id, limit, offset));
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
                //filter tags to only include tags that are allowed on assessments
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
                //filter tags to only include tags that are allowed on assessments
                tagGroups.ForEach(tagGroup => tagGroup.Tags = tagGroup.Tags.Where(tag => tag.IsSystemTag == true ||
                ((tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.WorkInstruction)) ||
                (tag.AllowedOnObjectTypes != null && tag.AllowedOnObjectTypes.Contains(TagableObjectEnum.Assessment)))).ToList());
            }

            return tagGroups;
        }
    }
}
