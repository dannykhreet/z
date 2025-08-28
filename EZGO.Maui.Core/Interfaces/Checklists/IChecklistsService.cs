using EZGO.Maui.Core.Interfaces.Sign;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Local;
using EZGO.Maui.Core.Models.Tasks;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Checklists
{
    public interface IChecklistService : ISignService, IOpenFieldLocalManager, IDisposable
    {
        Task<List<ChecklistModel>> GetChecklistsAsync(bool isComplete = false, bool useAreaId = true, bool refresh = false, int limit = 100, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false, long taskId = 0, int templateId = 0, bool showLocalChecklists = true, bool refreshActions = true);

        Task<List<ChecklistModel>> GetReportChecklistsAsync(DateTime? startTimeStamp, DateTime? endTimeStamp, bool refresh = false, int limit = 0, int offset = 0, LocalDateTime? timeStamp = null, bool isFromSyncService = false);

        Task<ChecklistTemplateModel> GetChecklistTemplateAsync(int id, bool refresh = false);

        Task<ChecklistTemplateModel> GetChecklistTemplateWithTaskTemplatesAsync(int id, bool refresh = false, bool isFromSyncService = false);

        Task<TaskTemplateModel> GetChecklistWithIncludes(int id, bool refresh = false, bool isFromSyncService = false);

        Task<List<ChecklistTemplateModel>> GetChecklistTemplatesAsync(bool includeTaskTemplates, bool refresh = false, bool isFromSyncService = false, bool includeIncompleteChecklists = true);

        Task PostChecklistAsync(PostTemplateModel model);

        Task<int> UploadLocalSignedChecklistsAsync();

        Task<List<LocalTemplateModel>> GetLocalChecklistTemplatesAsync();

        Task<bool> CheckHasCompletedChecklists(bool refresh);

        Task<List<SignedChecklistModel>> GetLocalSignedChecklistsAsync();

        Task<ChecklistModel> GetChecklistAsync(int? id, bool refresh = false, bool isFromSyncService = false);

        Task<List<ChecklistModel>> GetIncompleteChecklistsAsync(bool refresh = false, bool isFromSyncService = false, int checklistTemplateId = 0, long taskId = 0, bool refreshActions = true);
        Task<ChecklistModel> GetIncompleteChecklistAsync(bool refresh = false, bool isFromSyncService = false, int checklistId = 0);
        Task<List<ChecklistModel>> GetIncompleteDeeplinkChecklistsAsync(bool refresh = false, bool isFromSyncService = false, long taskId = 0);
        Task<bool> DeleteIncompletedChecklist(ChecklistModel item);
    }
}
