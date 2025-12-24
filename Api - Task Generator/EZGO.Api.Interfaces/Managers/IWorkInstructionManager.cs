using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.WorkInstructions;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IWorkInstructionManager, Interface for use with the WorkInstructionManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IWorkInstructionManager
    {
        #region - properties -
        string Culture { get; set; }
        #endregion

        #region - Workinstructions -
        Task<List<WorkInstruction>> GetWorkInstructionsAsync(int companyId, int? userId = null, WorkInstructionFilters? filters = null, string include = null, bool useStatic = false);
        Task<WorkInstruction> GetWorkInstructionAsync(int companyId, int workInstructionId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader, bool useStatic = false);
        Task<int> AddWorkInstructionAsync(int companyId, int userId, WorkInstruction workInstruction);
        Task<bool> ChangeWorkInstructionAsync(int companyId, int userId, int workInstructionId, WorkInstruction workInstruction);
        Task<bool> SetWorkInstructionActiveAsync(int companyId, int userId, int workInstructionId, bool isActive = true);
        Task<bool> SetWorkInstructionCompletedAsync(int companyId, int userId, int workInstructionId, bool isCompleted = true);
        #endregion

        #region - templates -
        Task<List<WorkInstructionTemplate>> GetWorkInstructionTemplatesAsync(int companyId, int? userId = null, WorkInstructionFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<Dictionary<int, string>> GetWorkInstructionsTemplateNames(int companyId, List<int> workInstructionIds);
        Task<WorkInstructionTemplate> GetWorkInstructionTemplateAsync(int companyId, int workInstructionTemplateId, int userId = 0, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<int> AddWorkInstructionTemplateAsync(int companyId, int userId, WorkInstructionTemplate workInstructionTemplate);
        Task<int> AddWorkInstructionTemplateChangesNotification(int companyId, int userId, WorkInstructionTemplate oldTemplate, WorkInstructionTemplate newTemplate, string notificationComment = null);
        Task<int> ChangeWorkInstructionTemplateAsync(int companyId, int userId, int workInstructionTemplateId, WorkInstructionTemplate workInstructionTemplate);
        Task<bool> SetWorkInstructionTemplateActiveAsync(int companyId, int userId, int workInstructionTemplateId, bool isActive = true);

        Task<bool> ConfirmWorkInstructionTemplateChangesNotifications(int companyId, int userId, int workInstructionTemplateId);
        #endregion

        #region - relations -
        /*
         TaskTemplates
         await _workInstructionManager.RemoveTaskTemplateWorkInstructionRelation(companyId: companyId, taskTemplateWorkInstructionRelationId: instructionRelation.Id);
         await _workInstructionManager.ChangeTaskTemplateWorkInstructionRelationAsync(companyId: companyId, userId: userId, workInstructionRelationId: instructionRelation.Id, workInstructionRelation: instructionRelation);
         await _workInstructionManager.AddTaskTemplateWorkInstructionRelationAsync(companyId: companyId, userId: userId, workInstructionRelation: instructionRelation);
        */

        /*
            AuditTemplateItems (tasktemplates)
         */

        /*
            ChecklistTemplateItems (tasktemplates)
         */
        Task<List<WorkInstructionTemplateChangeNotification>> GetWorkInstructionTemplateChangesNotificationsAsync(int companyId, int? userId = null, WorkInstructionTemplateChangeNotificationFilters? filters = null);
        Task<WorkInstructionTemplateChangeNotification> GetWorkInstructionTemplateChangeNotificationAsync(int id, int companyId, string include = null);
        Task<bool> RemoveTaskTemplateWorkInstructionRelation(int companyId, int taskTemplateWorkInstructionRelationId, int taskTemplateId, int? auditTemplateId = null, int? checklistTemplateId = null);
        Task<bool> ChangeTaskTemplateWorkInstructionRelationAsync(int companyId, int userId, int workInstructionRelationId, TaskTemplateRelationWorkInstructionTemplate workInstructionRelation);
        Task<int> AddTaskTemplateWorkInstructionRelationAsync(int companyId, int userId, TaskTemplateRelationWorkInstructionTemplate workInstructionRelation);

        #endregion
        List<Exception> GetPossibleExceptions();
    }
}
