using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Assessments;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Assessments;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.Assessments
{
    public class AssessmentsService : IAssessmentsService
    {
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly ISignatureService _signatureService;
        private readonly ICachingService _cachingService;
        private readonly IRoleFunctionsWrapper _roleFunctionsWrapper;
        private readonly IMessageService _messageService;

        public AssessmentsService(IApiRequestHandler apiRequestHandler, ISignatureService signatureService, IMessageService messageService, ICachingService cachingService, IRoleFunctionsWrapper roleFunctionsWrapper)
        {
            _apiRequestHandler = apiRequestHandler;
            _signatureService = signatureService;
            _cachingService = cachingService;
            _messageService = messageService;
            _roleFunctionsWrapper = roleFunctionsWrapper;
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }

        public async Task<AssessmentsTemplateModel> GetAssessmentTemplate(int id, bool refresh = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled)
                return new AssessmentsTemplateModel();

            var uri = $"assessmenttemplate/{id}?include=instructions,instructionitems,areapaths,areapathids";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri = $"assessmenttemplate/{id}?include=instructions,instructionitems,areapaths,areapathids,tags";

            var assessmentTemplate = await _apiRequestHandler.HandleRequest<AssessmentsTemplateModel>(uri, refresh).ConfigureAwait(false); ;
            return assessmentTemplate;
        }

        public async Task<List<AssessmentsTemplateModel>> GetAssessmentTemplates(int workAreaId, bool refresh = false, bool isFromSyncService = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled)
                return new List<AssessmentsTemplateModel>();

            var uri = $"assessmenttemplates?areaid={workAreaId}&include=instructions,instructionitems,areapaths,areapathids&limit=0";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri = $"assessmenttemplates?areaid={workAreaId}&include=instructions,instructionitems,areapaths,areapathids,tags&limit=0";

            string allowed = _roleFunctionsWrapper.checkRoleForAllowedOnlyFlag(UserSettings.userSettingsPrefs.RoleType).ToString().ToLower();
            uri = $"{uri}&allowedonly={allowed}";

            var assessmentTemplates = await _apiRequestHandler.HandleListRequest<AssessmentsTemplateModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);
            return assessmentTemplates;
        }

        public async Task<List<AssessmentsModel>> GetAssessments(int templateId = 0, bool refresh = false, bool isFromSyncService = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled)
                return new List<AssessmentsModel>();

            var uri = $"assessments?areaid={Settings.AreaSettings.AssessmentsWorkAreaId}&include=instructions,instructionitems,areapaths,areapathids&iscompleted=false&limit=0";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri = $"assessments?areaid={Settings.AreaSettings.AssessmentsWorkAreaId}&include=instructions,instructionitems,areapaths,areapathids,tags&iscompleted=false&limit=0";

            var assessments = await _apiRequestHandler.HandleListRequest<AssessmentsModel>(uri, refresh, isFromSyncService).ConfigureAwait(false);

            if (templateId != 0)
                assessments = assessments.Where(a => a.TemplateId == templateId).ToList();

            return assessments;
        }

        public async Task<AssessmentsModel> GetAssessment(int id, bool refresh = false, bool isFromSyncService = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled)
                return new AssessmentsModel();

            var assessments = await GetAssessments(refresh: refresh, isFromSyncService: isFromSyncService).ConfigureAwait(false);
            var assessment = assessments.FirstOrDefault(a => a.Id == id);
            return assessment;
        }

        public async Task<bool> PostChangeAssessment(BasicAssessmentModel assessmentsModel)
        {
            if (!CompanyFeatures.SkillAssessmentsEnabled)
                return false;

            string action = $"assessment/change/{assessmentsModel.Id}?fulloutput=true";

            var model = assessmentsModel.ToModel();


            var response = await _apiRequestHandler.HandlePostRequest(action, model).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (model.IsCompleted)
                {
                    await AlterAssessmentTemplatesOpenAssessmentsCacheDataAsync(model.TemplateId, -1).ConfigureAwait(false);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, Constants.AssessmentTemplateChanged);
                    });
                }
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, Constants.AssessmentChanged);
                });
                return true;
            }
            return false;
        }

        public async Task<bool> PostChangeAssessment(AssessmentsModel assessmentsModel, bool sendAssessmentChangedMessage = true)
        {
            if (!CompanyFeatures.SkillAssessmentsEnabled)
                return false;

            string action = $"assessment/change/{assessmentsModel.Id}?fulloutput=true";
            assessmentsModel.AssessorId = UserSettings.Id;
            assessmentsModel.AssessorPicture = UserSettings.UserPictureUrl;
            assessmentsModel.Assessor = UserSettings.Username;

            var response = await _apiRequestHandler.HandlePostRequest(action, assessmentsModel).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (sendAssessmentChangedMessage)
                    await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.AssessmentChanged); });

                return true;
            }
            return false;
        }

        public async Task<bool> HaveAnyCompletedAssessments(bool refresh = true)
        {
            if (!CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled)
                return false;

            var uri = $"assessments?iscompleted=true&limit=1";
            var assessments = await _apiRequestHandler.HandleListRequest<AssessmentsModel>(uri, refresh: refresh).ConfigureAwait(false);
            return assessments.Count > 0;
        }


        public async Task<bool> PostAddAssessments(AssessmentsTemplateModel assessmentsTemplateModel, IEnumerable<int> participantIds)
        {
            if (!CompanyFeatures.SkillAssessmentsEnabled)
                return false;

            static AssessmentsModel CreateAssessmentModel(AssessmentsTemplateModel assessmentsTemplateModel, int participantId)
            {
                return new AssessmentsModel()
                {
                    AreaId = Settings.AssessmentsWorkAreaId,
                    TemplateId = assessmentsTemplateModel.Id,
                    AssessorId = UserSettings.Id,
                    CompletedForId = participantId,
                    IsCompleted = false,
                    CompletedAt = DateTimeHelper.Now.ToDateTimeUnspecified(),
                    CompanyId = assessmentsTemplateModel.CompanyId,
                    SkillInstructions = assessmentsTemplateModel.SkillInstructions.Select(s => new AssessmentSkillInstructionModel()
                    {
                        WorkInstructionTemplateId = s.WorkInstructionTemplateId ?? 0,
                        AssessmentTemplateId = assessmentsTemplateModel.Id,
                        AssessmentTemplateSkillInstructionId = s.Id,
                        CompletedForId = participantId,
                        CompanyId = s.CompanyId,
                        CompletedAt = DateTimeHelper.Now.ToDateTimeUnspecified(),
                        InstructionItems = s.InstructionItems.Select(i => new BasicAssessmentInstructionItemModel()
                        {
                            Score = 0,
                            WorkInstructionTemplateItemId = i.Id,
                            CompanyId = i.CompanyId,
                            CompletedAt = DateTimeHelper.Now.ToDateTimeUnspecified(),
                            CompletedForId = participantId
                        }).ToList()
                    }).ToList()
                };
            }

            string action = $"assessment/add?fulloutput=true";

            int numberOfAddedAssessments = 0;

            foreach (var id in participantIds)
            {
                var model = CreateAssessmentModel(assessmentsTemplateModel, id);
                var result = await _apiRequestHandler.HandlePostRequest(action, model).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                    numberOfAddedAssessments++;
            }

            await AlterAssessmentTemplatesOpenAssessmentsCacheDataAsync(assessmentsTemplateModel.Id, numberOfAddedAssessments).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, Constants.AssessmentTemplateChanged);
            });
            return true;
        }

        public async Task UpdateAssessmentCache(int id)
        {
            if (!CompanyFeatures.SkillAssessmentsEnabled)
                return;

            using var scope = App.Container.CreateScope();
            ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();

            await syncService.LoadAssessmentAsync(id).ConfigureAwait(false);
        }

        private async Task UpdateAssessmentTemplatesCache()
        {
            using var scope = App.Container.CreateScope();
            ISyncService syncService = scope.ServiceProvider.GetService<ISyncService>();

            await syncService.LoadAssessmentTemplatesAsync().ConfigureAwait(false);
        }

        public async Task<bool> DeleteAssessments(IEnumerable<int> assessmentIds, AssessmentsTemplateModel assessmentsTemplateModel)
        {
            if (!CompanyFeatures.SkillAssessmentsEnabled)
                return false;

            int numberOfDeletedAssessments = 0;

            foreach (var id in assessmentIds)
            {
                string action = $"skillassessment/delete/{id}";
                var result = await _apiRequestHandler.HandlePostRequest(action, false).ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                    numberOfDeletedAssessments++;
            }

            await AlterAssessmentTemplatesOpenAssessmentsCacheDataAsync(assessmentsTemplateModel.Id, numberOfDeletedAssessments * -1).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.AssessmentTemplateChanged); });
            return true;
        }


        public async Task<List<AssessmentsModel>> GetCompletedAssessments(int workAreaId, int limit, int offset, bool refresh)
        {
            if (!CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled)
                return new List<AssessmentsModel>();

            var uri = $"assessments?include=instructions,instructionitems,areapaths,areapathids&iscompleted=true&limit={limit}&offset={offset}&areaId={workAreaId}";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri = $"assessments?include=instructions,instructionitems,areapaths,areapathids,tags&iscompleted=true&limit={limit}&offset={offset}&areaId={workAreaId}";
            var assessments = await _apiRequestHandler.HandleListRequest<AssessmentsModel>(uri, refresh: refresh).ConfigureAwait(false);
            return assessments;
        }

        public async Task<AssessmentsModel> GetCompletedAssessmentById(int id)
        {
            if (!CompanyFeatures.SkillAssessmentsEnabled)
                return new AssessmentsModel();

            var uri = $"skillassessment/{id}?include=instructions,instructionitems";
            var assessment = await _apiRequestHandler.HandleRequest<AssessmentsModel>(uri, true).ConfigureAwait(false);
            return assessment;
        }


        public async Task PostAndSignTemplateAsync(PostTemplateModel model)
        {
            if (!CompanyFeatures.SkillAssessmentsEnabled)
                return;

            try
            {
                if (model.Signatures.Any(x => !x.SignatureImage.IsNullOrEmpty()))
                {
                    await _signatureService.UploadSignaturesAsync(model.Signatures, MediaStorageTypeEnum.AssessmentSignature, 0).ConfigureAwait(false);
                }
                var assessment = await GetAssessment(model.TemplateId).ConfigureAwait(false);
                assessment.Signatures = model.Signatures.Select(s => new SignatureModel() { SignatureImage = s.SignatureImage, SignedAt = s.SignedAt, SignedBy = s.SignedBy, SignedById = s.SignedById }).ToList();
                assessment.IsCompleted = true;
                assessment.CompletedAt = DateTimeHelper.UtcNow.ToDateTimeUnspecified();
                assessment.TotalScore = assessment.SkillInstructions?.Sum(s => s.TotalScore) ?? 0;
                assessment.Version = model.Version;
                var result = await PostChangeAssessment(assessment, false).ConfigureAwait(false);
                if (result)
                {
                    await UpdateAssessmentCache(assessment.TemplateId).ConfigureAwait(false);
                    await AlterAssessmentTemplatesOpenAssessmentsCacheDataAsync(assessment.TemplateId, -1).ConfigureAwait(false);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, Constants.AssessmentSigned);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        //TODO: when Task PostAndSignTemplateAsync(PostTemplateModel model) tested remove this
        public async Task PostAndSignTemplateAsync(int assessmentId, string assessmentTemplateName, IEnumerable<UserValuesPropertyModel> userValues, IEnumerable<BasicTaskTemplateModel> tasks, IEnumerable<Signature> signatures)
        {
            var model = new PostTemplateModel(assessmentId, assessmentTemplateName, userValues, tasks);
            model.Signatures = signatures;
            await PostAndSignTemplateAsync(model).ConfigureAwait(false);
        }

        public async Task SendAllParticipantsAssessmentFinishedMessage()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MessagingCenter.Send(this, Constants.AllParticipantsAssessmentFinished);
            });
        }

        public async Task SetAssessmentScore(BasicAssessmentModel selectedUserAssessment, BasicAssessmentInstructionItemModel assessmentInstructionItem)
        {
            if (selectedUserAssessment.SkillInstructions.All(x => x.IsCompleted))
            {
                string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.completedAllInstructions);
                _messageService?.SendClosableInfo(message);
            }

            var assessmentFromApi = await GetAssessment(selectedUserAssessment.Id, true).ConfigureAwait(false);

            var instructionItem = assessmentFromApi
                   .SkillInstructions.FirstOrDefault(s => s.Id == assessmentInstructionItem.AssessmentSkillInstructionId)
                   .InstructionItems.FirstOrDefault(i => i.Id == assessmentInstructionItem.Id);

            if (instructionItem != null)
            {
                instructionItem.Score = assessmentInstructionItem.Score;
                instructionItem.Assessor = assessmentInstructionItem.Assessor;
                instructionItem.CompletedAt = assessmentInstructionItem.CompletedAt;
            }

            var result = await PostChangeAssessment(assessmentFromApi.ToBasic()).ConfigureAwait(false);

            if (result)
            {
                await AlterAssessmentScoreCacheDataAsync(assessmentInstructionItem).ConfigureAwait(false);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(this, Constants.AssessmentChangedScore);
                });
            }

            if (selectedUserAssessment.SkillInstructions.All(x => x.IsCompleted))
            {
                string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.completedAllInstructions);
                _messageService?.SendClosableInfo(message);
            }
        }

        private static void CalculateAssessmentDates(AssessmentsModel assessment)
        {
            assessment.StartDate = assessment.SkillInstructions.Select(s => s.StartDate).Min();
            assessment.EndDate = assessment.SkillInstructions.Select(s => s.EndDate).Max();
        }

        public async Task SetSkillInstructionStartDate(BasicAssessmentModel assessment, AssessmentSkillInstructionModel instruction)
        {
            var assessmentFromApi = await GetAssessment(assessment.Id, true).ConfigureAwait(false);
            var instructionFromApi = assessmentFromApi.SkillInstructions.FirstOrDefault(s => s.Id == instruction.Id);
            if (instructionFromApi != null)
            {
                instructionFromApi.StartDate = instruction.StartDate;
                CalculateAssessmentDates(assessmentFromApi);
                await PostChangeAssessment(assessmentFromApi.ToBasic()).ConfigureAwait(false);
                assessment.StartDate = assessmentFromApi.StartDate;
                assessment.EndDate = assessmentFromApi.EndDate;
            }
        }

        public async Task SetSkillInstructionEndDate(BasicAssessmentModel assessment, AssessmentSkillInstructionModel instruction)
        {
            var assessmentFromApi = await GetAssessment(assessment.Id, true).ConfigureAwait(false);
            var instructionFromApi = assessmentFromApi.SkillInstructions.FirstOrDefault(s => s.Id == instruction.Id);
            if (instructionFromApi != null)
            {
                instructionFromApi.EndDate = instruction.EndDate;
                CalculateAssessmentDates(assessmentFromApi);
                await PostChangeAssessment(assessmentFromApi.ToBasic()).ConfigureAwait(false);
                assessment.StartDate = assessmentFromApi.StartDate;
                assessment.EndDate = assessmentFromApi.EndDate;
            }
        }

        public async Task AlterAssessmentScoreCacheDataAsync(BasicAssessmentInstructionItemModel modified)
        {
            static void alteringFunction(AssessmentsModel assessment, BasicAssessmentInstructionItemModel modified)
            {
                var cachedInstruction = assessment
                    .SkillInstructions.FirstOrDefault(s => s.Id == modified.AssessmentSkillInstructionId)
                    .InstructionItems.FirstOrDefault(i => i.Id == modified.Id);

                if (cachedInstruction != null)
                    cachedInstruction.Score = modified.Score;
            }

            var uri = $"assessments?areaid={Settings.WorkAreaId}&include=instructions,instructionitems,areapaths,areapathids&iscompleted=false&limit=0";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                uri = $"assessments?areaid={Settings.WorkAreaId}&include=instructions,instructionitems,areapaths,areapathids,tags&iscompleted=false&limit=0";

            // Alter assessments cache
            uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: uri).AbsoluteUri;
            await _cachingService.AlterCachedRequestListAsync<AssessmentsModel>(uri, (assessment) => alteringFunction(assessment, modified), (assessment) => assessment.Id == modified.AssessmentId).ConfigureAwait(false);
        }

        public async Task AlterAssessmentTemplatesOpenAssessmentsCacheDataAsync(int assessmentTemplateId, int numberToAdd)
        {
            static void alteringFunction(AssessmentsTemplateModel assessment, int numberToAdd)
            {
                assessment.NumberOfOpenAssessments += numberToAdd;
            }

            // Alter assessment template cache
            var relativeUri = $"assessmenttemplates?areaid={Settings.AssessmentsWorkAreaId}&include=instructions,instructionitems,areapaths,areapathids&limit=0";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                relativeUri = $"assessmenttemplates?areaid={Settings.AssessmentsWorkAreaId}&include=instructions,instructionitems,areapaths,areapathids,tags&limit=0";

            string allowed = RoleFunctions.checkRoleForAllowedOnlyFlag(UserSettings.RoleType).ToString().ToLower();
            relativeUri = $"{relativeUri}&allowedonly={allowed}";

            var uri = new Uri(baseUri: new Uri(Statics.ApiUrl), relativeUri: relativeUri).AbsoluteUri;

            await _cachingService.AlterCachedRequestListAsync<AssessmentsTemplateModel>(uri, (assessmentTemplate) => alteringFunction(assessmentTemplate, numberToAdd), (assessmentTemplate) => assessmentTemplate.Id == assessmentTemplateId).ConfigureAwait(false);
        }

    }
}
