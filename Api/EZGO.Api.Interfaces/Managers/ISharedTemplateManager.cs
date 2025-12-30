using EZGO.Api.Models;
using EZGO.Api.Models.TemplateSharing;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ISharedTemplateManager
    {
        Task<int> ShareChecklistTemplateAsync(int fromCompanyId, int userId, ChecklistTemplate checklistTemplate, int toCompanyId);
        Task<int> ShareAuditTemplateAsync(int fromCompanyId, int userId, AuditTemplate auditTemplate, int toCompanyId);
        Task<int> ShareTaskTemplateAsync(int fromCompanyId, int userId, TaskTemplate taskTemplate, int toCompanyId);
        Task<int> ShareWorkInstructionTemplateAsync(int fromCompanyId, int userId, WorkInstructionTemplate workInstructionTemplate, int toCompanyId);
        Task<bool> AcceptSharedTemplateAsync(int companyId, int userId, int sharedTemplateId);
        Task<bool> RejectSharedTemplateAsync(int companyId, int userId, int sharedTemplateId);
        Task<TemplateJson> GetSharedTemplateAsync(int companyId, int sharedTemplateId);
        Task<int> GetSharedTemplatesCountAsync(int companyId);
        Task<List<SharedTemplate>> GetSharedTemplatesAsync(int companyId);
    }
}
