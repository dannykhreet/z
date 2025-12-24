using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IVersionReleaseManager
    {
        Task<bool> CheckStaticAuditExistsAsync(int id, int companyId);
        Task<bool> CheckStaticChecklistExistsAsync(int id, int companyId);
        Task<bool> CheckStaticTaskExistsAsync(int id, int companyId);
        Task<bool> CheckStaticAssessmentExistsAsync(int id, int companyId);
        Task<bool> SaveStaticAuditAsync(string auditJson, int id, int userId, int companyId);
        Task<bool> SaveStaticChecklistAsync(string checklistJson, int id, int userId, int companyId);
        Task<bool> SaveStaticTaskAsync(string taskJson, int id, int userId, int companyId);
        Task<bool> SaveStaticAssessmentAsync(string assessmentJson, int id, int userId, int companyId);
        List<Exception> GetPossibleExceptions();
    }
}
