using EZGO.Maui.Core.Interfaces.Sign;
using EZGO.Maui.Core.Models.Assessments;

namespace EZGO.Maui.Core.Interfaces.Assessments
{
    public interface IAssessmentsService : IDisposable, ISignService
    {
        Task<List<AssessmentsTemplateModel>> GetAssessmentTemplates(int workAreaId, bool refresh = false, bool isFromSyncService = false);
        Task<AssessmentsTemplateModel> GetAssessmentTemplate(int id, bool refresh = false);
        Task<List<AssessmentsModel>> GetAssessments(int templateId = 0, bool refresh = false, bool isFromSyncService = false);
        Task<AssessmentsModel> GetAssessment(int id, bool refresh = false, bool isFromSyncService = false);
        Task<List<AssessmentsModel>> GetCompletedAssessments(int workAreaId, int limit, int offset, bool refresh);
        Task<AssessmentsModel> GetCompletedAssessmentById(int id);
        Task<bool> PostAddAssessments(AssessmentsTemplateModel assessmentsTemplateModel, IEnumerable<int> participantIds);
        Task<bool> DeleteAssessments(IEnumerable<int> assessmentIds, AssessmentsTemplateModel assessmentsTemplateModel);
        Task<bool> PostChangeAssessment(BasicAssessmentModel assessmentsModel);
        async Task SendAllParticipantsAssessmentFinishedMessage() { }
        Task SetAssessmentScore(BasicAssessmentModel selectedUserAssessment, BasicAssessmentInstructionItemModel assessmentInstructionItem);
        Task UpdateAssessmentCache(int id);
        Task<bool> HaveAnyCompletedAssessments(bool refresh = true);
        Task SetSkillInstructionStartDate(BasicAssessmentModel assessment, AssessmentSkillInstructionModel instruction);
        Task SetSkillInstructionEndDate(BasicAssessmentModel assessment, AssessmentSkillInstructionModel instruction);
    }
}
