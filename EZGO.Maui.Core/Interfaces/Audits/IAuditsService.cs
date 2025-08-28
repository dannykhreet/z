using EZGO.Maui.Core.Interfaces.Sign;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Local;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Audits
{
    public interface IAuditsService : IOpenFieldLocalManager, ISignService, IDisposable
    {
        Task<List<AuditTemplateModel>> GetAuditTemplatesAsync(bool includeTaskTemplates, bool refresh = false, bool isFromSyncService = false);
        Task<List<AuditTemplateModel>> GetReportAuditTemplatesAsync(bool refresh = false, bool isFromSyncService = false);
        Task<AuditTemplateModel> GetAuditTemplateAsync(int id, bool refresh = false);
        Task<AuditTemplateModel> GetAuditTemplateWithIncludesAsync(int id, bool refresh = false, bool isFromSyncService = false);
        Task<List<AuditsModel>> GetAuditsAsync(bool isComplete = false, bool useAreaId = true, bool refresh = false, int limit = 0, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false);
        Task<List<AuditsModel>> GetReportAuditsAsync(LocalDateTime? startTimeStamp, LocalDateTime? endTimeStamp, bool refresh = false, int limit = 0, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false, int auditTemplateId = 0);
        Task PostAuditAsync(PostTemplateModel model);
        Task<int> UploadLocalSignedAuditsAsync();
        Task<List<LocalTemplateModel>> GetLocalAuditTemplates();
        Task<bool> CheckHasCompletedAudits(bool refresh, bool isFromSyncService = false);
        Task<List<AuditsModel>> GetAuditAsync(int id, bool refresh = false, bool isFromSyncService = false);
    }
}
