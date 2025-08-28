using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Statuses;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Classes
{
    public static class TaskHelper
    {
        public static async Task SetTaskStatusAsync(BasicTaskModel task, TaskStatusEnum status, FilterControl<BasicTaskModel, TaskStatusEnum> taskFilter, bool useDataSource = true, bool postTaskStatus = true)
        {
            if (task == null)
                return;

            // toggle status if similar
            if (task.FilterStatus == status)
                status = TaskStatusEnum.Todo;

            bool allowed = true;
            var setToOriginal = false;

            // Set original signature for checking
            var originalSignature = task.OriginalSignature;
            var originalStatus = (TaskStatusEnum)Enum.Parse(typeof(TaskStatusEnum), originalSignature?.Status?.Replace(" ", string.Empty) ?? "todo", true);
            bool owner = (originalSignature?.SignedById ?? UserSettings.Id) == UserSettings.Id;

            // set the selected task signature to null so it will be refreshed
            task.Signature = null;

            if (task.FilterStatus != TaskStatusEnum.Todo)
            {
                // checking if we are allowed to reset this item
                if (status == TaskStatusEnum.Todo || status == originalStatus)
                {
                    if (!owner)
                    {
                        setToOriginal = true;
                        // you are not the owner of the first status, you want to set the status to todo .. so we restore the original status
                        status = originalStatus;
                    }
                }
                allowed = owner || status != TaskStatusEnum.Todo;
            }
            else
            {
                // if the status is todo, even when historyfirst signature exists, we should treat this tapping as the first.
                task.OriginalSignature = null;
            }

            if (allowed)
            {

                await UploadTaskStatusAsync(task, status, setToOriginal, originalSignature, postTaskStatus).ConfigureAwait(false);
                if (taskFilter != null && !taskFilter.StatusFilters.IsNullOrEmpty())
                {
                    taskFilter.Filter(taskFilter.StatusFilters, resetIfTheSame: false, useDataSource: useDataSource);
                }

                CalculateTaskAmounts(taskFilter);
            }
        }

        public static void SetTaskStatusAsync(BasicTaskTemplateModel taskTemplate, TaskStatusEnum status, FilterControl<BasicTaskTemplateModel, TaskStatusEnum> taskFilter, bool useDataSource = true)
        {
            if (taskTemplate == null)
                return;

            // toggle status if similar
            if (taskTemplate.FilterStatus == status)
            {
                taskTemplate.FilterStatus = TaskStatusEnum.Todo;
                taskTemplate.Signature = null;
            }
            else
            {
                taskTemplate.FilterStatus = status;

                taskTemplate.Signature = new SignatureModel
                {
                    SignedById = UserSettings.Id,
                    SignedAt = DateTime.UtcNow,
                    SignedBy = UserSettings.Fullname
                };
            }

            taskTemplate.Score = null;

            if (taskFilter != null && !taskFilter.StatusFilters.IsNullOrEmpty())
            {
                taskFilter.Filter(taskFilter.StatusFilters, resetIfTheSame: false, useDataSource: useDataSource);
            }

            CalculateTaskAmounts(taskFilter);
        }

        public static void SetTaskScore(BasicTaskTemplateModel taskTemplate, int score)
        {
            if (taskTemplate == null)
                return;

            // toggle score if similar
            if (taskTemplate.Score == score)
            {
                taskTemplate.Score = null;
                taskTemplate.Signature = null;
            }
            else
            {
                taskTemplate.Score = score;

                taskTemplate.Signature = new SignatureModel
                {
                    SignedById = UserSettings.Id,
                    SignedAt = DateTime.UtcNow,
                    SignedBy = UserSettings.Fullname
                };
            }

            taskTemplate.FilterStatus = TaskStatusEnum.Todo;
        }


        public static async Task UploadTaskStatusAsync(BasicTaskModel task, TaskStatusEnum status, bool setToOriginal, SignatureModel originalSignature, bool postTaskStatus = true)
        {
            while (Statics.TaskSyncRunning)
            {
                await Task.Delay(10);
                // wait a for sync to finish
            }

            try
            {
                Statics.TaskSyncRunning = true;

                // set the new status
                task.FilterStatus = status;
                task.Status = status.ToString().ToLower();

                if (task.FilterStatus == TaskStatusEnum.Todo)
                {
                    task.Signature = null;
                }
                else
                {
                    if (setToOriginal)
                    {
                        task.Signature = originalSignature;
                    }
                    else
                    {
                        task.Signature = new SignatureModel
                        {
                            SignedById = UserSettings.Id,
                            SignedAt = DateTime.UtcNow,
                            SignedBy = UserSettings.Fullname
                        };
                    }
                    task.OriginalSignature ??= task.Signature;
                }

                using var scope = App.Container.CreateScope();
                var taskService = scope.ServiceProvider.GetService<ITasksService>();

                await taskService.AlterTaskCacheDataAsync(task);

                _ = Task.Run(async () => { await taskService?.SetTaskStatusAsync(status, task, postTaskStatus); }).ConfigureAwait(false);

                if (!OnlineShiftCheck.IsShiftChangeAllowed)
                {
                    OnlineShiftCheck.IsShiftChangeAllowed = true;
                    await OnlineShiftCheck.CheckCycleChange();
                }
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessagingCenter.Send(Application.Current, Constants.TasksChanged);
                });
            }
            catch { }
            finally { Statics.TaskSyncRunning = false; }
        }

        public static async Task UploadTaskPictureProofAsync(BasicTaskModel task)
        {
            if (!task.PictureProofMediaItems.Any(p => p.IsLocalFile))
                return;

            try
            {
                if (await InternetHelper.HasInternetConnection())
                {
                    using var scope = App.Container.CreateScope();
                    var taskService = scope.ServiceProvider.GetService<ITasksService>();

                    var mediaService = scope.ServiceProvider.GetService<IMediaService>();

                    var mediaItems = task.PictureProofMediaItems.Where(p => p.IsLocalFile);
                    foreach (var mediaItem in mediaItems)
                    {

                        await mediaService.UploadMediaItemAsync(mediaItem, MediaStorageTypeEnum.PictureProof, 0, true);
                    }

                    await taskService.SetTaskPictureProof(task);
                }
                else
                {
                    using var scope = App.Container.CreateScope();
                    var taskService = scope.ServiceProvider.GetService<ITasksService>();

                    await taskService.SaveLocalPictureProof(task);
                }
            }
            catch { throw; }
            finally { }
        }

        public static void CalculateTaskAmounts<T>(FilterControl<T, TaskStatusEnum> taskFilter) where T : IItemFilter<TaskStatusEnum>
        {
            if (taskFilter == null)
                return;

            var statusList = StatusFactory.CreateStatus<TaskStatusEnum>();

            taskFilter.CountItemsByStatus(TaskStatusEnum.Ok, statusList);
            taskFilter.CountItemsByStatus(TaskStatusEnum.NotOk, statusList);
            taskFilter.CountItemsByStatus(TaskStatusEnum.Skipped, statusList);
            taskFilter.CountItemsByStatus(TaskStatusEnum.Todo, statusList);
            taskFilter.SetStatusSelected(statusList);
            taskFilter.SetStatusPercentages(statusList);

            taskFilter.TaskStatusList = statusList;
        }
    }
}
