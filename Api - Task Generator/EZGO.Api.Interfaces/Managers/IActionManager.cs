using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IActionManager, Interface for use with the ActionManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IActionManager
    {
        string Culture { get; set; }
        Task<List<ActionsAction>> GetActionsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string include = null);
        Task<List<ActionsAction>> GetLatestActionsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string include = null);
        Task<ActionCountStatistics> GetActionCountsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string include = null);
        Task<List<ActionRelation>> GetActionRelationsAsync(int companyId, int? userId = null, ActionFilters? filters = null, string include = null);
        Task<List<ActionsAction>> GetActionsByTaskIdAsync(int companyId, long taskId);
        Task<List<ActionsAction>> GetActionsByTaskTemplateIdAsync(int companyId, int taskTemplateId);
        Task<ActionsAction> GetActionAsync(int companyId, int actionId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, int? userId = null);
        Task<int> AddActionAsync(int companyId, int userId, ActionsAction action);
        Task<bool> ChangeActionAsync(int companyId, int userId, int actionId, ActionsAction action);
        Task<bool> SetActionTaskAsync(int companyId, int userId, int actionId, int taskId);
        Task<bool> SetActionActiveAsync(int companyId, int userId, int actionId, bool isActive = true);
        Task<bool> SetActionResolvedAsync(int companyId, int userId, int actionId, bool isResolved = true, bool useAutoResolvedMessage = false);
        Task<bool> SetActionViewedAsync(int companyId, int actionId, int userId);
        Task<List<ActionComment>> GetActionCommentsAsync(int companyId, ActionFilters? filters = null, string include = null);
        Task<List<ActionComment>> GetActionCommentsByActionIdAsync(int companyId, int actionId);
        Task<ActionComment> GetActionCommentAsync(int companyId,int actionCommentId, string include = null);
        Task<int> AddActionCommentAsync(int companyId, int userId, ActionComment actionComment);
        Task<bool> ChangeActionCommentAsync(int companyId, int userId, int actionCommentId, ActionComment actionComment);
        Task<bool> SetActionCommentActiveAsync(int companyId, int userId, int actionCommentId, bool isActive = true);
        Task<bool> SetActionCommentViewedAsync(int companyId, int actionCommentId, int userId);
        Task<bool> SetActionCommentViewedAllAsync(int companyId, int actionId, int userId);
        Task<List<ActionCommentViewedStatsItem>> GetActionCommentStatisticsRelatedToUser(int companyId, int userId);
        Task<int> AddActionAssignedUserAsync(int actionId, int userId);
        Task<int> AddActionAssignedAreaAsync(int actionId, int areaId);
        Task<int> RemoveActionAssignedUserAsync(int companyId, int actionId, int userId);
        Task<int> RemoveActionAssignedAreaAsync(int companyId, int actionId, int areaId);
        Task<List<ActionRelationArea>> GetAsssignedAreasWithActions(int companyId);
        Task<List<ActionRelationUser>> GetAsssignedUsersWithActions(int companyId);
        Task<List<UpdateCheckCommentItem>> CheckActionCommentAsync(int companyId, int? actionId = null, DateTime? timestamp = null);
        List<Exception> GetPossibleExceptions();
        Task<int> AddSapPmNotificationAsync(int companyId, int userId, ActionsAction action);
        Task<SapPmNotification> GetSapPmNotificationAsync(int companyId, int actionId);
    }
}
