using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Data
{
    public interface IDataMigrationManager
    {
        Task<int> MigrationAuditsToStaticAsync(int companyId, int userId);
        Task<int> MigrationChecklistsToStaticAsync(int companyId, int userId);
        Task<int> ActionCommentCorrectionForActionResolvedIssue();
        Task<int> DataAuditingCorrectionForAuditTemplateSetActive();
        Task<int> DataAuditingCorrectionForWorkInstructionTemplateItems();
        List<Exception> GetPossibleExceptions();

    }
}
