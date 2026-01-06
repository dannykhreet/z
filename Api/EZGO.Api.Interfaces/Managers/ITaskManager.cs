using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Reports;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// ITaskManager, Interface for use with the TaskManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface ITaskManager
    {
        string Culture { get; set; }
        Task<List<TasksTask>> GetTasksAsync(int companyId, int? userId = null, DateTime? timestamp = null, DateTime? starttimestamp = null, DateTime? endtimestamp = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksOverdueAsync(int companyId, int? userId = null, DateTime? timestamp = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksRelatedToPreviousShiftAsync(int companyId, int? userId = null, DateTime? timestamp = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksRelatedToShiftAsync(int companyId, int? userId = null, DateTime? timestamp = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksYesterdayAsync(int companyId, int? userId = null, DateTime? timestamp = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksPeriodAsync(int companyId, int? userId = null, DateTime? from = null, DateTime? to = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksLastWeekAsync(int companyId, int? userId = null, DateTime? timestamp = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksActionsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksAuditActionsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksChecklistActionsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksCommentsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksAuditCommentsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksChecklistCommentsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksByAuditIdAsync(int companyId, int auditId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<TasksTask>> GetTasksByChecklistIdAsync(int companyId, int checklistId, int? checklistTemplateId = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<TaskStatusBasic>> GetTasksStatusAsync(int companyId, DateTime timestamp, TaskFilters? filters = null);
        Task<List<TaskStatistics>> GetTasksStatisticsRelatedToPeriodAsync(int companyId, int? userId = null, DateTime? from = null, DateTime? to = null, TaskFilters? filters = null, string include = null);
        Task<List<TaskExtendedDataBasic>> GetTasksExendedDataAsync(int companyId, DateTime timestamp, TaskFilters? filters = null);
        Task<List<TaskPropertyUserValueBasic>> GetTasksPropertyUserValuesAsync(int companyId, DateTime timestamp, TaskFilters? filters = null);
        Task<List<TaskStatusBasic>> GetTasksHistoryAsync(int companyId, DateTime timestamp, TaskFilters? filters = null);
        Task<List<TaskStatusBasic>> GetTasksHistoryFirstsAsync(int companyId, DateTime timestamp, TaskFilters? filters = null);
        Task<TasksTask> GetTaskAsync(int companyId, int taskId, string include = null);
        Task<int> AddTaskAsync(int companyId, TasksTask task, int userId, int possibleOwnerId);
        Task<bool> ChangeTaskAsync(int companyId, int taskId, TasksTask task, int userId, int possibleOwnerId);
        Task<bool> ChangeTaskStatusAsync(int companyId, int taskId, TasksTask task, int userId);
        Task<bool> ChangeTaskPictureProofAsync(int companyId, int taskId, PictureProof pictureProof, int userId, int possibleOwnerId);
        Task<bool> ChangeTaskPropertyUserValuesAsync(int companyId, int taskId, List<PropertyUserValue> propertyUserValues, int userId);
        Task<bool> SetTaskActiveAsync(int companyId, int userId, int taskId, bool isActive = true);
        Task<bool> SetTaskStatusAsync(int companyId, int taskId, int userId, TaskStatusEnum status = TaskStatusEnum.Todo);
        Task<bool> SetTaskVersionAsync(int companyId, int userId, int taskId, string version);
        Task<List<string>> GetAvailableTaskTemplateVersionsForTaskAsync(int companyId, int taskId);
        Task<bool> SetTaskStatusWithReasonAsync(int companyId, int taskId, int userId, TaskStatusEnum status, string comment, DateTime signedAtUtc);
        Task<bool> SetTaskStatussesWithReasonAsync(int companyId, int userId, MultiTaskStatusWithReason multiTaskStatus);
        Task<bool> SetTaskStatusSignAsync(int companyId, int taskId, int userId, SignBasic signBasic);
        Task<bool> SetTaskRealizedAsync(int companyId, int taskId, int realizedById, int timeRealized);
        Task<List<TaskTemplate>> GetTaskTemplatesAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<TaskTemplateCountStatistics> GetTaskTemplatesCountsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TaskTemplate>> GetTaskTemplatesActionsAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<TaskTemplate> GetTaskTemplateAsync(int companyId, int taskTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<TaskTemplate>> GetTaskTemplatesByAuditTemplateIdAsync(int companyId, int auditTemplateId, string include = null);
        Task<List<TaskTemplate>> GetTaskTemplatesByChecklistTemplateIdAsync(int companyId, int checklistTemplateId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<TaskTemplateRelationShift>> GetShiftRelationsWithTaskTemplatesAsync(int companyId);
        Task<int> AddTaskTemplateAsync(int companyId, int userId, TaskTemplate taskTemplate);
        Task<bool> ChangeTaskTemplateAsync(int companyId, int userId, int taskTemplateId, TaskTemplate taskTemplate);
        Task<bool> SetTaskTemplateActiveAsync(int companyId, int userId, int taskTemplateId, bool isActive = true);
        Task<bool> SetTaskTemplateDerivativeInActiveAsync(int companyId, int userId, int taskTemplateId);
        Task<List<TaskRecurrency>> GetTaskRecurrenciesAsync(int companyId, TaskFilters? filters = null, string include = null);
        Task<TaskRecurrency> GetTaskRecurrencyAsync(int companyId, int taskRecurrencyId);
        Task<int> AddTaskRecurrencyAsync(int companyId, int userId, TaskRecurrency taskRecurrency);
        Task<bool> ChangeTaskRecurrencyAsync(int companyId, int userId, int taskRecurrencyId, TaskRecurrency taskRecurrency);
        Task<bool> SetTaskRecurrencyActiveAsync(int companyId, int userId, int taskRecurrencyId, bool isActive = true);
        Task<List<Step>> GetTaskTemplateStepsAsync(int companyId, int? userId = null, TaskFilters? filters = null, List<int> taskTemplateIds = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<Step>> GetTaskTemplateStepsWithAuditsAsync(int companyId, int? userId = null, TaskFilters? filters = null);
        Task<List<Step>> GetTaskTemplateStepsWithChecklistsAsync(int companyId, int? userId = null, TaskFilters? filters = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<Step> GetTaskTemplateStepAsync(int companyId, int stepId);
        Task<List<Step>> GetTaskTemplateStepsByTaskTemplateIdAsync(int companyId, int taskTemplateId);
        Task<int> AddTaskTemplateStepAsync(int companyId, int userId, Step step);
        Task<bool> ChangeTaskTemplateStepAsync(int companyId, int userId, int stepId, Step step);
        Task<bool> SetTaskTemplateStepActiveAsync(int companyId, int userId, int stepId, bool isActive = true);
        Task<List<TasksTask>> GetTasksWithAuditsAsync(int companyId, List<int> auditIds, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksWithChecklistsAsync(int companyId, List<int> checklistIds, List<int> checklistTemplateIds, int? userId = null, TaskFilters? filters = null, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<TaskTemplate>> GetTasksTemplatesWithAuditTemplatesAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TaskTemplate>> GetTasksTemplatesWithAuditTemplatesAsync(int companyId, List<int> auditIds, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TaskTemplate>> GetTasksTemplatesWithChecklistTemplatesAsync(int companyId, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<List<TaskTemplate>> GetTasksTemplatesWithChecklistTemplatesAsync(int companyId, List<int> checklistIds, int? userId = null, TaskFilters? filters = null, string include = null);
        Task<bool> SetTaskTemplateChecklistTemplateRelation(int companyId, int taskTemplateId, int checklistTemplateId);
        Task<bool> SetTaskTemplateAuditTemplateRelation(int companyId, int taskTemplateId, int auditTemplateId);
        Task<int> AddChangeTaskTemplatePropertiesAsync(int companyId, int userId, int templateId, List<PropertyTaskTemplate> templateProperties);
        Task<List<TasksTask>> GetLatestTasks(int companyId, int limit, int offset = 0, int templateId = 0, string include = null);
        //Task<List<TasksTask>> GetCompletedTasks(int companyId, int offset, int limit, int areaId, int shiftId);
        Task<bool> SetTemplateIndices(int companyId, int userId, List<IndexItem> templateIndices);
        Task<int> GetTaskTemplatePreviousTaskCountAsync(int companyId, int taskTemplateId);
        Task<List<TasksTask>> GetTasksSplitByTypeAsync(int companyId, int userId, DateTime? timestamp = null, DateTime? starttimestamp = null, DateTime? endtimestamp = null, TaskFilters? filters = null, string include = null);
        Task<List<TasksTask>> GetTasksByDayAsync(int companyId, int userId, DateTime timestamp, TaskFilters? filters = null, string include = null);
        Task<bool> setTaskCompletedFromChecklistIfAllowed(int companyId, int userId, int taskId);
        Task<List<TasksTaskStatus>> GetTaskStatusHistoryAsync(int companyId, int taskId);
        List<Exception> GetPossibleExceptions();
        Task<TaskStatisticsOverview> GetTaskLandingPageStats(int companyId, int userId, int areaid);
        Task<TasksWithMetaData> GetTasksGen4Async(int companyId, int userId, Gen4TaskFilters filters, string include);
        Gen4TaskFilters GetTaskFilters(TaskTimeSpanEnum? timespanType, int? timespanOffset, DateTime? startTimestamp, DateTime? endTimestamp, int areaId, string filtertext, string statusIds, string tagIds, bool? allowDuplicateTaskInstances, int? limit, int? offset, bool ShowSkippableTaskOverview);
    }

}
