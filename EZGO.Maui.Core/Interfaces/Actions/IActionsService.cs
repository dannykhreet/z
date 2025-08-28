using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Interfaces.Actions
{
    public interface IActionsService : IDisposable
    {
        Task<List<ActionsModel>> GetCreatedActionsAsync(int id, bool refresh = false, bool isFromSyncService = false);
        Task<List<ActionsModel>> GetAssignedUserActionsAsync(int id, bool refresh = false, bool isFromSyncService = false);
        Task<List<ActionsModel>> GetMyActionsAsync(bool createdByOrAssignedToMe = false, bool refresh = false, bool isFromSyncService = false);

        Task<List<ActionsModel>> GetReportActionsAsync(int tasktemplateId = 0, int assignedUserId = 0, bool withIncludes = true, bool applySort = true, bool refresh = false, bool isFromSyncService = false);
        Task<ActionCountersModel> GetActionsCount(bool refresh = false, bool isFromSyncService = false);
        Task<List<ActionsModel>> GetActionsAsync(int userId = 0, int assignedUserId = 0, int tasktemplateId = 0, bool withIncludes = true, bool applySort = true, bool includeLocalActions = true, bool refresh = false, int assignedAreaId = 0, bool isFromSyncService = false, bool createdByOrAssignedToMe = false, int limit = 0);
        Task<ActionsModel> GetOnlineActionAsync(int id, bool isFromSyncService = false);
        Task<bool> SetActionResolvedAsync(BasicActionsModel action);

        Task<bool> AddActionCommentAsync(ActionCommentModel comment);
        Task<bool> SetActionCommentsViewedAsync(int id);

        Task<bool> UpdateActionAsync(ActionsModel action);

        Task<bool> AddActionToAuditAsync(ActionsModel action);
        Task<bool> AddActionToChecklistAsync(ActionsModel action);
        Task<List<ActionsModel>> GetOpenActionsForTaskAsync(long taskId);
        Task<List<ActionsModel>> GetOpenActionsForTaskTemplateAsync(int taskTemplateId, List<ActionsModel> actions = null);
        Task<List<ActionsModel>> GetAllActionsForTaskAsync(long taskId);
        Task<List<ActionsModel>> GetAllActionsForTaskTemplateAsync(int taskTemplateId);

        Task<List<ActionCommentModel>> GetActionCommentsAsync(int actionId = 0, bool includeLocalActionComments = true, bool refresh = false, bool isFromSyncService = false);

        Task<List<ActionUser>> GetAssignedUsersAsync(bool refresh = false, bool isFromSyncService = false);

        Task<List<ActionArea>> GetAssignedAreasActionsAsync(bool refresh = false, bool isFromSyncService = false);

        Task<bool> AddActionAsync(ActionsModel action);

        Task UploadLocalActionsWithComments();

        Task UploadLocalActionsAsync();

        Task<List<string>> UploadLocalActionCommentsAsync();

        Task UploadLocalResolvedActionIdsAsync();

        Task UploadLocalCommentsViewedActionIdsAsync();

        Task<ActionsModel> GetLocalActionAsync(int localId);
        Task<IEnumerable<ActionsModel>> GetActionsWithAssignedAreaAsync(bool refresh, int assignedAreaId, bool excludeSelfCreatedActions = false);

        void SendActionsChangedMessage(string messageKey = Constants.ActionsChanged);

        Task LoadAssignedAreasForActionsAsync(IEnumerable<ActionsModel> actions, bool refresh);
        Task LoadAssignedUsersForActionsAsync(IEnumerable<ActionsModel> actions, bool refresh);

        Task<List<ActionsModel>> GetResolvedActionsForTaskTemplateAsync(int taskTemplateId, DateTime? startedAt = null, List<ActionsModel> actions = null);
    }
}
