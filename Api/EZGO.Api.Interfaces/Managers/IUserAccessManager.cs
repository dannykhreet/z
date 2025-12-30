using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IUserAccessManager
    {
        Task<List<int>> GetAllowedTaskTemplateIdsWithUserAsync(int companyId, int userId);
        Task<List<int>> GetAllowedChecklistTemplateIdsWithUserAsync(int companyId, int userId);
        Task<List<int>> GetAllowedAuditTemplateIdsWithUserAsync(int companyId, int userId);
        Task<List<int>> GetAllowedAreaIdsWithUserAsync(int companyId, int userId);
        Task<List<int>> GetAllowedWorkInstructionTemplateIdsWithUserAsync(int companyId, int userId);
        List<Exception> GetPossibleExceptions();
    }
}
