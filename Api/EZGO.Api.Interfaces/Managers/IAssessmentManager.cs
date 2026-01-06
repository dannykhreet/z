using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IAssessmentManager
    {
        #region - properties -
        string Culture { get; set; }
        #endregion
        #region - assessments - 
        Task<List<Assessment>> GetAssessmentsAsync(int companyId, int? userId = null, AssessmentFilters? filters = null, string include = null, bool useStatic = false);
        Task<Assessment> GetAssessmentAsync(int companyId, int assessmentId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false);
        Task<int> AddAssessmentAsync(int companyId, int userId, Assessment assessment);
        Task<bool> ChangeAssessmentAsync(int companyId, int userId, int assessmentId, Assessment assessment);
        Task<bool> SetAssessmentActiveAsync(int companyId, int userId, int assessmentId, bool isActive = true);
        Task<bool> FreeLinkedAssessmentInstruction(int companyId, int assessmentId);
        Task<bool> SetAssessmentCompletedAsync(int companyId, int userId, int assessmentId, bool isCompleted = true);
        #endregion
        #region - templates-
        Task<List<AssessmentTemplate>> GetAssessmentTemplatesAsync(int companyId, int? userId = null, AssessmentFilters? filters = null, string include = null);
        Task<AssessmentTemplate> GetAssessmentTemplateAsync(int companyId, int assessmentTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<int>> GetWorkInstructionConnectedAssessmentTemplateIds(int companyId, int workInstructionTemplateId);
        Task<int> AddAssessmentTemplateAsync(int companyId, int userId, AssessmentTemplate assessmentTemplate);
        Task<bool> ChangeAssessmentTemplateAsync(int companyId, int userId, int assessmentTemplateId, AssessmentTemplate assessmentTemplate);
        Task<bool> SetAssessmentTemplateActiveAsync(int companyId, int userId, int assessmentTemplateId, bool isActive = true);
        #endregion
        List<Exception> GetPossibleExceptions();
    }
}
