using EZGO.Api.Models;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using EZGO.CMS.LIB.Enumerators;
using EZGO.CMS.LIB.Extensions;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Converters;
using WebApp.Logic.Interfaces;
using WebApp.Models.Pdf;
using WebApp.Models.Shared;
using WebApp.Models.Skills;
using WebApp.Models.WorkInstructions;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class PdfController : BaseController
    {
        private readonly IApiConnector _connector;
        private string locale;
        private readonly ILogger<PdfController> _logger;
        private PdfViewModel output;
        private IHttpContextAccessor _contextAccessor;
        private readonly ILanguageService _languageService;
        private readonly IMediaService _mediaService;

        public PdfController(ILogger<PdfController> logger, IConfigurationHelper configurationHelper, IApiConnector connector, ILanguageService language, IMediaService mediaService, IHttpContextAccessor httpContextAccessor, IApplicationSettingsHelper applicationSettingsHelper, IInboxService inboxService) : base(language, configurationHelper, httpContextAccessor, applicationSettingsHelper, inboxService)
        {
            _connector = connector;
            _logger = logger;
            _contextAccessor = httpContextAccessor;
            _languageService = language;
            _mediaService = mediaService;
            locale = _locale ?? "en-US";
            output = new PdfViewModel();
            output.Locale = locale;
        }

        [Route("/pdf/viewer/completedaudit/{id}")]
        public async Task<IActionResult> ViewAuditPDF(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            //completed pdf info
            string completedListName = "";
            string completedByUserName = "";
            DateTime completedAt = DateTime.MinValue;
            int auditScore = 0;

            string endpoint = string.Format(Constants.Audit.GetCompletedAuditTasks, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            var audit = JsonConvert.DeserializeObject<EZGO.Api.Models.Audit>(result.Message);
            if (audit?.Signatures?.Count > 0 && audit.Signatures.First().SignedAt.HasValue)
            {
                //signedDate = audit.Signatures.First().SignedAt.Value;
                completedByUserName = audit.Signatures.First().SignedBy;
                completedAt = audit.Signatures.First().SignedAt.Value;
            }
            completedListName = audit.Name;

            auditScore = audit.TotalScore;

            var actions = await getActionsForAudit(audit);
            var comments = await getCommentsForAudit(audit);

            var scoreColorCalculator = new Lazy<IScoreColorCalculator>(() => ScoreColorCalculatorFactory.Default(audit.MinTaskScore ?? 1, audit.MaxTaskScore ?? 10));

            //headerText = "COMPLETED AUDIT";

            var output = new PdfAuditCompletedViewModel()
            {
                Audit = audit,
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                Actions = actions,
                Comments = comments,
                MediaService = _mediaService,
                PageTitle = completedListName,
                ScoreColorCalculator = scoreColorCalculator.Value,
                CompletedByUserName = completedByUserName,
                CompletedAt = completedAt,
                AuditTotalScorePercentage = auditScore
            };
            if (output.Audit != null && output.Audit.Tasks != null && output.Audit.Tasks.Count > 0)
            {
                output.Audit.Tasks = output.Audit.Tasks.OrderBy(t => t.Index).ToList();
            }

            return View(@"~/Views/Pdf/ViewerAuditCompleted.cshtml", output);
        }

        [Route("/pdf/viewer/completedchecklist/{id}")]
        public async Task<IActionResult> ViewChecklistPDF(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            //completed pdf info
            string completedListName = "";
            string completedByUserName = "";
            DateTime completedAt = DateTime.MinValue;
            int auditScore = 0;

            string endpoint = string.Format(Constants.Checklist.GetCompletedChecklistTask, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            var checklist = JsonConvert.DeserializeObject<EZGO.Api.Models.Checklist>(result.Message);
            if (checklist?.Signatures?.Count > 0 && checklist.Signatures.First().SignedAt.HasValue)
            {
                //signedDate = audit.Signatures.First().SignedAt.Value;
                completedByUserName = checklist.Signatures.First().SignedBy;
                completedAt = checklist.Signatures.First().SignedAt.Value;
            }
            completedListName = checklist.Name;

            var actions = await getActionsForChecklist(checklist);
            var comments = await getCommentsForChecklist(checklist);


            //headerText = "COMPLETED AUDIT";

            var output = new PdfChecklistCompletedViewModel()
            {
                Checklist = checklist,
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                Actions = actions,
                Comments = comments,
                MediaService = _mediaService,
                PageTitle = completedListName,
                CompletedByUserName = completedByUserName,
                CompletedAt = completedAt
            };

            if (output.Checklist != null && output.Checklist.Tasks != null && output.Checklist.Tasks.Count > 0)
            {
                output.Checklist.Tasks = output.Checklist.Tasks.OrderBy(t => t.Index).ToList();
            }

            if (output.Checklist != null && output.Checklist.Stages != null && output.Checklist.Stages.Count > 0)
            {
                output.Checklist.Stages = output.Checklist.Stages.OrderBy(t => t.Index).ToList();
            }


            return View(@"~/Views/Pdf/ViewerChecklistCompleted.cshtml", output);
        }


        [Route("/pdf/viewer/matrix/{id}")]
        public async Task<IActionResult> ViewMatrixPDF(int id)
        {

            var output = new SkillsViewModel();
            output.CmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            output.Filter.CmsLanguage = output.CmsLanguage;
            output.MediaService = _mediaService;
            output.PageTitle = "Skills matrix details";
            output.ApiBaseUrl = _configurationHelper.GetValueAsString("AppSettings:ApiUri");
            output.Filter.Module = FilterViewModel.ApplicationModules.SKILLSMATRIX;
            output.Locale = _locale;
            output.ApplicationSettings = await this.GetApplicationSettings();
            output.CurrentUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value.ToObjectFromJson<UserProfile>();

            var uriMatrix = string.Format(Logic.Constants.Skills.SkillMatrixDetailsUrl, id);
            var result = await _connector.GetCall(uriMatrix);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output.CurrentSkillsMatrix = JsonConvert.DeserializeObject<Models.Skills.SkillsMatrix>(result.Message);
            }

            if (output.CurrentSkillsMatrix == null)
            {
                return View("~/Views/Shared/_template_not_found.cshtml", output);
                //output.CurrentSkillsMatrix = new Models.Skills.SkillsMatrix();
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

            return View(@"~/Views/Pdf/ViewerMatrix.cshtml", output);
        }


        [Route("/pdf/viewer/checklisttemplate/{id}")]
        public async Task<IActionResult> ViewChecklistTemplatePDF(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            string endpoint = string.Format(Constants.Checklist.GetChecklistTemplateDetails, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            var checklistTemplate = JsonConvert.DeserializeObject<EZGO.Api.Models.ChecklistTemplate>(result.Message);
            var areaRequest = await _connector.GetCall(string.Format(Constants.Task.GetTaskAreaById, checklistTemplate.AreaId));

            if (areaRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Response.Clear();
                Response.StatusCode = (int)areaRequest.StatusCode;
                await Response.WriteAsync("Error while trying to get area, please contact the system administrator.");
            }

            var area = JsonConvert.DeserializeObject<EZGO.Api.Models.Area>(areaRequest.Message);

            var output = new PdfChecklistTemplateViewModel()
            {
                ChecklistTemplate = checklistTemplate,
                ChecklistArea = area,
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                MediaService = _mediaService
            };

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(resultworkinstructions.Message))
            {
                output.WorkInstructions = JsonConvert.DeserializeObject<List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>>(resultworkinstructions.Message);
            }

            if (output.WorkInstructions == null)
            {
                output.WorkInstructions = new List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>();
            }
            else
            {
                //replace with query filter on api (parameter still needs to be checked, for now filter in code)
                output.WorkInstructions = output.WorkInstructions.Where(x => x.WorkInstructionType == EZGO.Api.Models.Enumerations.InstructionTypeEnum.BasicInstruction).ToList();
            }

            return View(@"~/Views/Pdf/ViewerChecklistTemplate.cshtml", output);
        }


        [Route("/pdf/viewer/wichangenotification/{id}")]
        public async Task<IActionResult> ViewWorkInstructionChangeNotificationPDF(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            var output = new PdfWorkInstructionChangeNotificationViewModel()
            {
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                MediaService = _mediaService
            };

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

            var endpoint = string.Format($"/v1/workinstructiontemplatechangenotification/{id}?include=userinformation");
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && !result.Message.IsNullOrEmpty())
            {
                output.ChangeNotification = JsonConvert.DeserializeObject<WorkInstructionTemplateChangesNotificationModel>(result.Message);
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

            var tagsResult = await _connector.GetCall(Logic.Constants.Tags.GetTags);
            var tags = new List<Tag>();

            if (tagsResult.StatusCode == HttpStatusCode.OK)
            {
                output.Tags = JsonConvert.DeserializeObject<List<Tag>>(tagsResult.Message);
            }

            output.Locale = _locale;

            return View(@"~/Views/Pdf/ViewerWIChangeNotification.cshtml", output);
        }

        [Route("/pdf/viewer/audittemplate/{id}")]
        public async Task<IActionResult> ViewAuditTemplatePDF(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);
            //tasktemplates,areapaths,actions,steps,propertyvalues,properties,openfields,instructionrelations,tags
            string endpoint = string.Format(Constants.Audit.GetAuditTemplatesMoreDetailUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            var auditTemplate = JsonConvert.DeserializeObject<EZGO.Api.Models.AuditTemplate>(result.Message);
            var areaRequest = await _connector.GetCall(string.Format(Constants.Task.GetTaskAreaById, auditTemplate.AreaId));

            if (areaRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Response.Clear();
                Response.StatusCode = (int)areaRequest.StatusCode;
                await Response.WriteAsync("Error while trying to get area, please contact the system administrator.");
            }

            var area = JsonConvert.DeserializeObject<EZGO.Api.Models.Area>(areaRequest.Message);

            var output = new PdfAuditTemplateViewModel()
            {
                AuditTemplate = auditTemplate,
                AuditArea = area,
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                MediaService = _mediaService
            };

            var resultworkinstructions = await _connector.GetCall(Logic.Constants.WorkInstructions.WorkInstructionTemplatesUrl.Replace("include=items", "include="));
            if (resultworkinstructions.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(resultworkinstructions.Message))
            {
                output.WorkInstructions = JsonConvert.DeserializeObject<List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>>(resultworkinstructions.Message);
            }

            if (output.WorkInstructions == null)
            {
                output.WorkInstructions = new List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>();
            }
            else
            {
                //replace with query filter on api (parameter still needs to be checked, for now filter in code)
                output.WorkInstructions = output.WorkInstructions.Where(x => x.WorkInstructionType == EZGO.Api.Models.Enumerations.InstructionTypeEnum.BasicInstruction).ToList();
            }

            return View(@"~/Views/Pdf/ViewerAuditTemplate.cshtml", output);
        }

        [Route("/pdf/viewer/tasktemplate/{id}")]
        public async Task<IActionResult> ViewTaskTemplatePdf(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }


            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            string endpoint = string.Format(Constants.Task.GetTaskTemplateDetailUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            var taskTemplate = JsonConvert.DeserializeObject<EZGO.Api.Models.TaskTemplate>(result.Message);

            var areaRequest = await _connector.GetCall(string.Format(Constants.Task.GetTaskAreaById, taskTemplate.AreaId));
            if (areaRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Response.Clear();
                Response.StatusCode = (int)areaRequest.StatusCode;
                await Response.WriteAsync("Error while trying to get area, please contact the system administrator.");
            }

            var companyRequest = await _connector.GetCall(Constants.Company.GetCompanyWithShifts);

            if (companyRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Response.Clear();
                Response.StatusCode = (int)areaRequest.StatusCode;
                await Response.WriteAsync("Error while trying to get shifts for task template, please contact the system administrator.");
            }

            var company = JsonConvert.DeserializeObject<EZGO.Api.Models.Company>(companyRequest.Message);

            var shifts = new Dictionary<int, Shift>();

            if (company.Shifts != null && taskTemplate.Recurrency != null && taskTemplate.Recurrency.Shifts != null)
            {
                shifts = company.Shifts.Where(s => taskTemplate.Recurrency.Shifts.Contains(s.Id)).ToDictionary(s => s.Id, s => s);
            }

            var area = JsonConvert.DeserializeObject<EZGO.Api.Models.Area>(areaRequest.Message);

            var output = new PdfTaskTemplateViewModel()
            {
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                MediaService = _mediaService,
                TaskTemplate = taskTemplate,
                TaskArea = area,
                TaskShifts = shifts
            };

            return View(@"~/Views/Pdf/ViewerTaskTemplate.cshtml", output);
        }

        [Route("/pdf/viewer/workinstructiontemplate/{id}")]
        public async Task<IActionResult> ViewWorkInstructionTemplatePdf(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }


            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            string endpoint = string.Format(Constants.WorkInstructions.WorkInstructionTemplateDetailsUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            var workInstructionTemplate = JsonConvert.DeserializeObject<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate>(result.Message);

            var areaRequest = await _connector.GetCall(string.Format(Constants.Task.GetTaskAreaById, workInstructionTemplate.AreaId));
            if (areaRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Response.Clear();
                Response.StatusCode = (int)areaRequest.StatusCode;
                await Response.WriteAsync("Error while trying to get area, please contact the system administrator.");
            }

            var area = JsonConvert.DeserializeObject<EZGO.Api.Models.Area>(areaRequest.Message);

            var output = new PdfWorkInstructionTemplateViewModel()
            {
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                MediaService = _mediaService,
                WorkInstructionTemplate = workInstructionTemplate,
                WorkInstructionTemplateArea = area
            };

            return View(@"~/Views/Pdf/ViewerWorkInstructionTemplate.cshtml", output);
        }

        [Route("/pdf/viewer/assessmenttemplate/{id}")]
        public async Task<IActionResult> ViewAssessmentTemplatePdf(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            string endpoint = string.Format(Constants.Assessments.GetAssessmentTemplateForCreation, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }

            var assessmentTemplate = JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.AssessmentTemplate>(result.Message);

            var areaRequest = await _connector.GetCall(string.Format(Constants.Task.GetTaskAreaById, assessmentTemplate.AreaId));
            if (areaRequest.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Response.Clear();
                Response.StatusCode = (int)areaRequest.StatusCode;
                await Response.WriteAsync("Error while trying to get area, please contact the system administrator.");
            }

            var area = JsonConvert.DeserializeObject<EZGO.Api.Models.Area>(areaRequest.Message);

            var output = new PdfAssessmentTemplateViewModel()
            {
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage, 
                MediaService = _mediaService,
                AssessmentArea = area,
                AssessmentTemplate = assessmentTemplate
            };

            return View(@"~/Views/Pdf/ViewerAssessmentTemplate.cshtml", output);
        }

        [Route("/pdf/viewer/completedassessment/{id}")]
        public async Task<IActionResult> ViewCompletedAssessmentPdf(int id)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var applicationSettings = await this.GetApplicationSettings();
            var cmsLanguage = await _languageService.GetLanguageDictionaryAsync(_locale);

            string endpoint = string.Format(Constants.Skills.SkillAssessmentDetailsUrl, id);
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Message.IsNullOrEmpty())
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Item is invalid or not available.");
            }


            WebApp.Models.Skills.SkillAssessment skillAssessment = new SkillAssessment();
            if (result.StatusCode == HttpStatusCode.OK)
            {
                skillAssessment = (JsonConvert.DeserializeObject<EZGO.Api.Models.Skills.Assessment>(result.Message)).ToLocalAssessment();
            }

            int totalInstructions = 0;
            foreach (var skillInstruction in skillAssessment.SkillInstructions)
            {
                totalInstructions += skillInstruction.InstructionItems?.Count ?? 1; //prevent possible divide by zero exception
            }


            var output = new PdfAssessmentCompletedViewModel()
            {
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                MediaService = _mediaService,
                Assessment = skillAssessment,
                HeaderInfo = new CompletedAssessmentPdfHeaderModel()
                {
                    CompletedAt = skillAssessment.CompletedAt ?? DateTime.MinValue,
                    CompletedBy = skillAssessment.CompletedFor,
                    Assessor = skillAssessment.Assessor,
                    AssessmentScore = string.Format("{0:0.00}", (skillAssessment.TotalScore.HasValue ? Math.Round(((double)skillAssessment.TotalScore.Value / totalInstructions), 2) : 0.0d)),
                    CompletedAssessmentName = skillAssessment.Name,
                    CompletedAssessmentDescription = skillAssessment.Description
                }
            };

            return View(@"~/Views/Pdf/ViewerAssessmentCompleted.cshtml", output);
        }

        [HttpGet]
        [Route("/pdf/viewer/tasks")]
        public async Task<IActionResult> ViewerTasksPdf([FromQuery] string filterType, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? areaId, [FromQuery] int? templateid)
        {
            if (!_configurationHelper.GetValueAsBool("AppSettings:PDFGenerationEnabled"))
            {
                return StatusCode((int)HttpStatusCode.OK, "Temporary Unavailable, please try again later.");
            }

            var cmsLanguage = await _language.GetLanguageDictionaryAsync(locale);
            DateTime SignedDate = DateTime.Now;
            var headerText = "";
            List<SharedTaskModel> tasks = await GetTasksForPeriodAsync(from, to, areaId, templateid);

            switch (filterType)
            {
                case "previousshifts":
                    var endpoint = string.Format(Constants.Shift.GetShifts);
                    var result = await _connector.GetCall(endpoint);
                    var shifts = JsonConvert.DeserializeObject<List<Shift>>(result.Message);
                    var currentShiftIndex = -1;
                    for (int i = 0; i < shifts.Count; i++)
                    {
                        TimeSpan.TryParse(shifts[i].Start, out var shiftStart);
                        TimeSpan.TryParse(shifts[i].End, out var shiftEnd);

                        var fromUtc = from.ToUniversalTime();
                        var toUtc = to.ToUniversalTime();

                        if (shifts[i].Day == (int)fromUtc.DayOfWeek + 1)
                        {
                            if (fromUtc.TimeOfDay == shiftStart && toUtc.TimeOfDay == shiftEnd)
                            {
                                currentShiftIndex = i;
                                break;
                            }
                        }
                    }
                    if (currentShiftIndex != -1)
                    {
                        TimeSpan.TryParse(shifts[currentShiftIndex].Start, out var shiftStart);
                        TimeSpan.TryParse(shifts[currentShiftIndex].End, out var shiftEnd);

                        var fromUtc = from.ToUniversalTime();
                        var toUtc = to.ToUniversalTime();

                        headerText = "Tasks for shift " +
                            fromUtc.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " " +
                            shifts[currentShiftIndex].Start + " - " +
                            toUtc.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " " +
                            shifts[currentShiftIndex].End;
                    }

                    break;
                case "previousdays":
                    headerText = "Tasks for day " + from.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case "previousweeks":
                    headerText = "Tasks for week " + System.Globalization.ISOWeek.GetWeekOfYear(from);
                    break;
                default:
                    break;
            }

            var actions = await getActionsForTasks(tasks);
            var comments = await getCommentsForTasks(tasks);

            if (tasks != null)
            {
                if (tasks.Any())
                {
                    tasks.ForEach(t =>
                    {
                        t.PropertyString = buildPropertyString(t);
                    });
                }
            }

            var applicationSettings = await this.GetApplicationSettings();

            var output = new PdfTasksCompletedViewModel()
            {
                Tasks = tasks,
                ApplicationSettings = applicationSettings,
                CmsLanguage = cmsLanguage,
                Actions = actions,
                Comments = comments,
                MediaService = _mediaService,
                Locale = _locale,
                HeaderInfo = new CompletedTasksPdfHeaderModel() { FilterType = filterType, From = from.ToUniversalTime(), To = to.ToUniversalTime(), HeaderText = headerText }
            };

            return View(@"~/Views/Pdf/ViewerTasksCompleted.cshtml", output);
        }

        [NonAction]
        public string GetBaseUrl()
        {
            var request = HttpContext.Request;
            var host = request.Host.ToUriComponent();
            var pathBase = request.PathBase.ToUriComponent();

            return $"{request.Scheme}://{host}{pathBase}";
        }

        [NonAction]
        private string buildPropertyString(SharedTaskModel task)
        {
            var tz = User.FindFirst(System.Security.Claims.ClaimTypes.Country)?.Value ?? "Europe/Amsterdam";
            TimeZoneInfo timezone = TZConvert.EzFindTimeZoneInfoById(tz);
            return WebApp.Helpers.CompletedItemDisplayHelpers.GetPropertiesAsString(completedTask: task, timezone: timezone, locale: locale);
        }

        [NonAction]
        private async Task addActionsAndComments()
        {
            if (output.Item != null)
            {
                if (output.Item.Tasks.Any())
                {
                    List<PdfActionModel> actions = new List<PdfActionModel>();
                    var result = await _connector.GetCall(Logic.Constants.Action.GetActionsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        actions = JsonConvert.DeserializeObject<List<PdfActionModel>>(result.Message);
                        actions ??= new List<PdfActionModel>();
                    }
                    List<PdfCommentModel> comments = new List<PdfCommentModel>();
                    var result2 = await _connector.GetCall(Logic.Constants.Comment.GetCommentsUrl);
                    if (result2.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        comments = JsonConvert.DeserializeObject<List<PdfCommentModel>>(result2.Message);
                        comments ??= new List<PdfCommentModel>();
                    }

                    foreach (SharedTaskModel task in output.Item.Tasks)
                    {
                        // actions
                        var taskactions = actions.Where(x => x.TaskId == task.Id || (x.Parent != null && x.Parent.TaskId == task.Id)).ToList();
                        taskactions ??= new List<PdfActionModel>();
                        if (taskactions.Any())
                        {
                            taskactions.ForEach(x => x.TaskIndex = task.Index);
                            output.Actions.AddRange(taskactions);
                        }
                        //var result = await _connector.GetCall(string.Format(Logic.Constants.Action.GetTaskActionsUrl, task.Id));
                        //if (result.StatusCode == System.Net.HttpStatusCode.OK)
                        //{
                        //    var actionList = JsonConvert.DeserializeObject<List<ActionModel>>(result.Message);
                        //    actionList ??= new List<ActionModel>();
                        //    output.Actions.AddRange(actionList);
                        //}
                        // comments
                        var taskcomments = comments.Where(x => (x.TaskId.HasValue && x.TaskId == task.Id)).ToList();
                        taskcomments ??= new List<PdfCommentModel>();
                        if (taskcomments.Any())
                        {
                            taskcomments.ForEach(x => x.TaskIndex = task.Index);
                            output.Comments.AddRange(taskcomments);
                        }
                        //var result2 = await _connector.GetCall(string.Format(Logic.Constants.Comment.GetTaskCommentsUrl, task.Id));
                        //if (result2.StatusCode == System.Net.HttpStatusCode.OK)
                        //{
                        //    var commentList = JsonConvert.DeserializeObject<List<CommentModel>>(result2.Message);
                        //    commentList ??= new List<CommentModel>();
                        //    output.Comments.AddRange(commentList);
                        //}
                    }
                }
            }
        }

        [NonAction]
        private async Task<List<PdfActionModel>> getActionsForChecklist(EZGO.Api.Models.Checklist checklist)
        {
            var pdfActions = new List<PdfActionModel>();
            if (checklist != null)
            {
                if (checklist.Tasks != null && checklist.Tasks.Any())
                {
                    List<PdfActionModel> actions = new List<PdfActionModel>();
                    var result = await _connector.GetCall(Logic.Constants.Action.GetActionsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        actions = JsonConvert.DeserializeObject<List<PdfActionModel>>(result.Message);
                        actions ??= new List<PdfActionModel>();
                    }

                    foreach (var task in checklist.Tasks)
                    {
                        // actions
                        var taskactions = actions.Where(x => x.TaskId == task.Id || (x.Parent != null && x.Parent.TaskId == task.Id)).ToList();
                        taskactions ??= new List<PdfActionModel>();
                        if (taskactions.Any())
                        {
                            taskactions.ForEach(x => x.TaskIndex = task.Index ?? 0);
                            pdfActions.AddRange(taskactions);
                        }
                    }
                }
            }
            return pdfActions;
        }

        [NonAction]
        private async Task<List<PdfCommentModel>> getCommentsForChecklist(EZGO.Api.Models.Checklist checklist)
        {
            var pdfComments = new List<PdfCommentModel>();
            if (checklist != null)
            {
                if (checklist.Tasks != null && checklist.Tasks.Any())
                {
                    List<PdfCommentModel> comments = new List<PdfCommentModel>();
                    var result = await _connector.GetCall(Logic.Constants.Comment.GetCommentsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        comments = JsonConvert.DeserializeObject<List<PdfCommentModel>>(result.Message);
                        comments ??= new List<PdfCommentModel>();
                    }

                    foreach (var task in checklist.Tasks)
                    {
                        // comments
                        var taskcomments = comments.Where(x => (x.TaskId.HasValue && x.TaskId == task.Id)).ToList();
                        taskcomments ??= new List<PdfCommentModel>();
                        if (taskcomments.Any())
                        {
                            taskcomments.ForEach(x => x.TaskIndex = task.Index ?? 0);
                            pdfComments.AddRange(taskcomments);
                        }
                    }
                }
            }
            return pdfComments;
        }

        [NonAction]
        private async Task<List<PdfActionModel>> getActionsForTasks(List<SharedTaskModel> tasks)
        {
            var pdfActions = new List<PdfActionModel>();
            if (tasks != null)
            {
                if (tasks.Any())
                {
                    List<PdfActionModel> actions = new List<PdfActionModel>();
                    var result = await _connector.GetCall(Logic.Constants.Action.GetActionsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        actions = JsonConvert.DeserializeObject<List<PdfActionModel>>(result.Message);
                        actions ??= new List<PdfActionModel>();
                    }

                    foreach (var task in tasks)
                    {
                        // actions
                        var taskactions = actions.Where(x => x.TaskId == task.Id || (x.Parent != null && x.Parent.TaskId == task.Id)).ToList();
                        taskactions ??= new List<PdfActionModel>();
                        if (taskactions.Any())
                        {
                            taskactions.ForEach(x => x.TaskIndex = task.Index);
                            pdfActions.AddRange(taskactions);
                        }
                    }
                }
            }
            return pdfActions;
        }

        [NonAction]
        private async Task<List<PdfCommentModel>> getCommentsForTasks(List<SharedTaskModel> tasks)
        {
            var pdfComments = new List<PdfCommentModel>();
            if (tasks != null)
            {
                if (tasks.Any())
                {
                    List<PdfCommentModel> comments = new List<PdfCommentModel>();
                    var result = await _connector.GetCall(Logic.Constants.Comment.GetCommentsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        comments = JsonConvert.DeserializeObject<List<PdfCommentModel>>(result.Message);
                        comments ??= new List<PdfCommentModel>();
                    }

                    foreach (var task in tasks)
                    {
                        // comments
                        var taskcomments = comments.Where(x => (x.TaskId.HasValue && x.TaskId == task.Id)).ToList();
                        taskcomments ??= new List<PdfCommentModel>();
                        if (taskcomments.Any())
                        {
                            taskcomments.ForEach(x => x.TaskIndex = task.Index);
                            pdfComments.AddRange(taskcomments);
                        }
                    }
                }
            }
            return pdfComments;
        }

        [NonAction]
        private async Task<List<PdfActionModel>> getActionsForAudit(EZGO.Api.Models.Audit audit)
        {
            var pdfActions = new List<PdfActionModel>();
            if (audit != null)
            {
                if (audit.Tasks != null && audit.Tasks.Any())
                {
                    List<PdfActionModel> actions = new List<PdfActionModel>();
                    var result = await _connector.GetCall(Logic.Constants.Action.GetActionsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        actions = JsonConvert.DeserializeObject<List<PdfActionModel>>(result.Message);
                        actions ??= new List<PdfActionModel>();
                    }

                    foreach (var task in audit.Tasks)
                    {
                        // actions
                        var taskactions = actions.Where(x => x.TaskId == task.Id || (x.Parent != null && x.Parent.TaskId == task.Id)).ToList();
                        taskactions ??= new List<PdfActionModel>();
                        if (taskactions.Any())
                        {
                            taskactions.ForEach(x => x.TaskIndex = task.Index ?? 0);
                            pdfActions.AddRange(taskactions);
                        }
                    }
                }
            }
            return pdfActions;
        }

        [NonAction]
        private async Task<List<PdfCommentModel>> getCommentsForAudit(EZGO.Api.Models.Audit audit)
        {
            var pdfComments = new List<PdfCommentModel>();
            if (audit != null)
            {
                if (audit.Tasks != null && audit.Tasks.Any())
                {
                    List<PdfCommentModel> comments = new List<PdfCommentModel>();
                    var result = await _connector.GetCall(Logic.Constants.Comment.GetCommentsUrl);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        comments = JsonConvert.DeserializeObject<List<PdfCommentModel>>(result.Message);
                        comments ??= new List<PdfCommentModel>();
                    }

                    foreach (var task in audit.Tasks)
                    {
                        // comments
                        var taskcomments = comments.Where(x => (x.TaskId.HasValue && x.TaskId == task.Id)).ToList();
                        taskcomments ??= new List<PdfCommentModel>();
                        if (taskcomments.Any())
                        {
                            taskcomments.ForEach(x => x.TaskIndex = task.Index ?? 0);
                            pdfComments.AddRange(taskcomments);
                        }
                    }
                }
            }
            return pdfComments;
        }

        #region task details

        [NonAction]
        public async Task<List<SharedTaskModel>> GetTasksForPeriodAsync(DateTime? from, DateTime? to, int? areaId, int? templateId)
        {
            string fromTimestamp = from.Value.ToString("dd-MM-yyyy HH:mm:ss");
            string toTimestamp = to.Value.ToString("dd-MM-yyyy HH:mm:ss");
            string areaIdParamater = areaId != null ? $"&areaid={areaId}" : "";
            string templateIdParameter = templateId != null ? $"&templateid={templateId}" : "";

            string uri = $"/v1/tasks/period?from={fromTimestamp}&to={toTimestamp}{areaIdParamater}{templateIdParameter}&filterareatype=1&limit=0&include=steps,areapaths,properties,propertyvalues,propertyuservalues,pictureproof,tags";

            var result = await _connector.GetCall(uri);
            var tasks = JsonConvert.DeserializeObject<List<SharedTaskModel>>(result.Message);

            return tasks.ToList();
        }
        #endregion

    }
}
