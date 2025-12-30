using EZGO.Api.Models.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Utils
{
    public interface IDataAuditing
    {
        Task<bool> WriteDataAudit(string original, string mutated, string objecttype, int objectId, int userId, int companyId, string description);
        Task<bool> WriteDataAuditForSharedTemplate(int sharedTemplateId, string statusDescription);
        Task<AuditingObjectData> GetObjectDataLatestMutation(int companyId, int objectId, string type);
        Task<AuditingObjectData> GetObjectDataMutation(int companyId, int id);
        Task<List<AuditingObjectData>> GetObjectDataMutations(int companyId, int objectId, string type, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetObjectDataByParent(int companyId, int parentObjectId, string type, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetObjectDataUserHistory(int companyId, int userId, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetObjectData(int companyId,  string type, string description, int? objectId = null, int? userId = null, DateTime? createdOnStart = null, DateTime? createdOnEnd = null, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetObjectDataMutationsOverview(int companyId, string[] types, string description, int? objectId = null, int? userId = null, DateTime? createdOnStart = null, DateTime? createdOnEnd = null, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetChecklistTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetAuditTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetAssessmentTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetWorkInstructionTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null);
        Task<List<AuditingObjectData>> GetTaskTemplateDataMutationsDetails(int companyId, int logAuditingId, int? limit = null, int? offset = null);

    }
}
