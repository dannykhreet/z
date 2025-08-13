using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models.Tasks;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Tasks
{
    public interface ITasksService : IDisposable
    {
        Task<TaskTemplateModel> GetTaskTemplateAsync(int id, bool refresh = false, bool isFromSyncService = false);

        Task<BasicTaskModel> GetTaskAsync(long id, string type, bool refresh = false);
        Task<List<BasicTaskModel>> GetTasksAsync(LocalDateTime? tasksTimestamp = null, bool loadActionCount = true, bool refresh = false, bool isFromSyncService = false);
        Task<List<BasicTaskStatusModel>> GetTaskStatusses(LocalDateTime? nowTimestamp = null, bool isFromSyncService = false);
        Task<List<TaskExtendedDataBasic>> GetTaskExtendedData(LocalDateTime? nowTimestamp, bool isFromSyncService = false);
        Task GetTaskHistoryFirsts(IEnumerable<BasicTaskModel> tasks = null, LocalDateTime? tasksTimestamp = null, bool refresh = false, bool isFromSyncService = false);

        //Task<List<BasicTaskModel>> GetTasksForShiftAsync(LocalDateTime tasksTimestamp, int shiftId);
        Task<List<BasicTaskModel>> GetTasksForShiftAsync(LocalDateTime? timeStamp, bool refresh = false, bool isFromSyncService = false, bool filterTasksFromTheFuture = true);
        Task<List<BasicTaskModel>> GetTasksForYesterday(LocalDateTime? timeStamp, bool refresh = false, bool isFromSyncService = false, bool filterTasksFromTheFuture = true);
        Task<List<BasicTaskModel>> GetTasksForLastWeek(LocalDateTime? timeStamp, bool refresh = false, bool isFromSyncService = false, bool filterTasksFromTheFuture = true);


        Task SetTaskStatusAsync(TaskStatusEnum status, BasicTaskModel modifiedTask, bool postTaskStatus = true);

        Task PostTaskStatusWithReasonAsync(MultiTaskStatusWithReason multiTaskStatus);

        Task AlterTaskCacheDataAsync(BasicTaskModel modifiedTask);

        Task SetTaskRealizedTimeAsync(long taskId, int realizedTime);

        Task LoadOpenActionCountForTaskTemplatesAsync(IEnumerable<BasicTaskTemplateModel> taskTemplates, bool refresh = false);

        Task LoadOpenActionCountForTaskAsync(BasicTaskModel task, bool refresh = false);

        Task LoadActionCountForTasksAsync(IEnumerable<TasksTaskModel> tasks, bool refresh = false);

        Task<List<BasicTaskModel>> GetTasksForPeriodAsync(TaskPeriod period, LocalDateTime? tasksTimestamp = null, bool refresh = false, bool includeProperties = false);

        Task<List<BasicTaskModel>> GetOverdueTasksAsync(LocalDateTime? tasksTimestamp = null, bool loadActionCount = true, bool refresh = false, bool isFromSyncService = false);

        Task<List<BasicTaskModel>> GetTasksWithActionsAsync(string uri, bool refresh = false, bool isFromSyncService = false);

        Task<List<TaskTemplateModel>> GetTaskTemplatesWithActionsAsync(bool refresh = false, bool isFromSyncService = false);

        Task LoadOpenActionCountForTasksAsync(IEnumerable<BasicTaskModel> tasks, bool refresh = false);

        // Completed tasks
        Task<List<BasicTaskModel>> GetPreviousByRangeAsync(LocalDateTime from, LocalDateTime to, bool refresh = false, bool isFromSyncService = false);

        Task UploadLocalTaskTimeModelsAsync();

        Task UploadLocalTaskStatusModelsAsync();
        Task<List<BasicTaskModel>> GetTasksForPeriodsAsync(LocalDateTime? tasksTimestamp, bool refresh, TaskPeriod? period = null);
        Task SetTaskPictureProof(BasicTaskModel task);
        Task SaveLocalPictureProof(BasicTaskModel task);
        Task UploadLocalPictureProofAsync();

        //Task<List<MultiTaskStatusWithReason>> GetLocalMultiskipTaskAsync();
        //Task AddLocalMultiskipTaskAsync(MultiTaskStatusWithReason multiTaskStatus);
        Task<List<MultiTaskStatusWithReason>> GetLocalMultiskipTaskAsync();
        Task AddLocalMultiskipTaskAsync(MultiTaskStatusWithReason multiTaskStatus);
        Task UploadLocalMultiTaskStatusAsync();
        Task SetMandatoryItemToTask(long? taskId, int itemId);
    }
}
