using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IAuditManager, Interface for use with the AuditManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IAuditManager
    {
        string Culture { get; set; }
        Task<List<Audit>> GetAuditsAsync(int companyId, int? userId = null, AuditFilters? filters = null, string include = null, bool useStatic = false);
        Task<Audit> GetAuditAsync(int companyId, int auditId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false);
        Task<int> AddAuditAsync(int companyId, int userId, Audit audit);
        Task<bool> ChangeAuditAsync(int companyId, int userId, int auditId, Audit audit);
        Task<bool> SetAuditActiveAsync(int companyId, int userId, int auditId, bool isActive = true);
        Task<bool> SetAuditScoreAsync(int companyId, int userId, int auditId, int score);
        Task<bool> SetAuditCompleteAsync(int companyId, int userId, int auditId, bool isComplete = true);
        Task<bool> SetAuditCalculatedScoreAsync(int companyId, int userId,int auditId);
        Task<bool> AuditSigningAsync(int companyId, int userId, int auditId, AuditRelationSigning signing);
        Task<AuditRelationStatus> CreateAuditAsync(int companyId, int userId, AuditRelationStatus auditRelation);
        Task<AuditRelationStatusScore> CreateAuditAsync(int companyId, int userId, AuditRelationStatusScore auditRelation);
        Task<AuditRelationStatus> SetAuditTaskStatusAsync(int companyId, int userId, AuditRelationStatus auditRelation);
        Task<AuditRelationStatusScore> SetAuditTaskStatusScoreAsync(int companyId, int userId, AuditRelationStatusScore auditRelation);
        Task<List<AuditTemplate>> GetAuditTemplatesAsync(int companyId, int? userId = null, AuditFilters? filters = null, string include = null);
        Task<AuditTemplateCountStatistics> GetAuditTemplateCountsAsync(int companyId, int? userId = null, AuditFilters? filters = null, string include = null);
        Task<AuditTemplate> GetAuditTemplateAsync(int companyId, int auditTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<Dictionary<int, string>> GetAuditTemplateNamesAsync(int companyId, List<int> audittemplateIds);
        Task<int> AddAuditTemplateAsync(int companyId, int userId, AuditTemplate auditTemplate);
        Task<bool> ChangeAuditTemplateAsync(int companyId, int userId, int auditTemplateId, AuditTemplate auditTemplate);
        Task<bool> SetAuditTemplateActiveAsync(int companyId, int userId, int auditTemplateId, bool isActive = true);
        Task<List<int>> GetConnectedTaskTemplateIds(int companyId, int auditTemplateId);

        List<Exception> GetPossibleExceptions();
    }
}
