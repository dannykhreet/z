using Autofac;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Messages;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Services.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.AllTasks;
using EZGO.Maui.Core.ViewModels.Tasks;
using NodaTime;
using PropertyChanged;
using Syncfusion.Maui.DataSource.Extensions;
using System.Diagnostics;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class TaskViewModel : BaseViewModel
    {
        #region shift

        public List<TaskOverviewReportItemModel> ReportItemsShift { get; set; }

        [DependsOn(nameof(ReportItemsShift))]
        public bool ShiftBarIsVisible => ReportItemsShift?.Any(x => x.NrOfItems > 0) ?? true;

        [DependsOn(nameof(ShiftBarIsVisible), nameof(GridBarHeight))]
        public GridLength ShiftBarGridLength => new GridLength((ShiftBarIsVisible ? GridBarHeight : 0), GridUnitType.Absolute);

        #endregion

        #region today

        public List<TaskOverviewReportItemModel> ReportItemsToday { get; set; }

        public bool TodayBarIsVisible => ReportItemsToday?.Any(x => x.NrOfItems > 0) ?? true;

        [DependsOn(nameof(TodayBarIsVisible), nameof(GridBarHeight))]
        public GridLength TodayBarGridLength => new GridLength((TodayBarIsVisible ? GridBarHeight : 0), GridUnitType.Absolute);

        #endregion

        #region week

        public List<TaskOverviewReportItemModel> ReportItemsWeek { get; set; }

        [DependsOn(nameof(ReportItemsWeek))]
        public bool WeekBarIsVisible => ReportItemsWeek?.Any(x => x.NrOfItems > 0) ?? true;

        [DependsOn(nameof(WeekBarIsVisible), nameof(GridBarHeight))]
        public GridLength WeekBarGridLength => new GridLength((WeekBarIsVisible ? GridBarHeight : 0), GridUnitType.Absolute);

        #endregion

        #region overdue

        public List<TaskOverviewReportItemModel> ReportItemsOverDue { get; set; }

        [DependsOn(nameof(ReportItemsOverDue))]
        public bool OverdueBarIsVisible => ReportItemsOverDue?.Any(x => x.NrOfItems > 0) ?? true;

        [DependsOn(nameof(OverdueBarIsVisible), nameof(GridBarHeight))]
        public GridLength OverdueBarGridLength => new GridLength((OverdueBarIsVisible ? GridBarHeight : 0), GridUnitType.Absolute);

        [DependsOn(nameof(ReportItemsOverDue))]
        public Color OverdueEmptyColor => ReportItemsOverDue?.Any(x => x.NrOfItems > 0) ?? false ? ResourceHelper.GetApplicationResource<Color>("GreyColor") : ResourceHelper.GetApplicationResource<Color>("GreenColor");

        #endregion

        #region Gauges

        public List<TaskOverviewReportItemModel> ReportItemsPreviousShift { get; set; }

        public List<TaskOverviewReportItemModel> ReportItemsYesterday { get; set; }

        public List<TaskOverviewReportItemModel> ReportItemsPreviousWeek { get; set; }

        #endregion

        #region Properties

        public double GridBarHeight { get; set; } = DeviceSettings.ScreenDencity > 8 ? 92 : 80;

        /// <summary>
        /// determines the visibility of the new task button
        /// </summary>
        public bool NewTaskButtonIsVisible => UserSettings.RoleType != RoleTypeEnum.Basic;

        [DoNotNotify]
        public TasksCollection Tasks { get; set; }

        #endregion

        #region Commands
        public ICommand NavigateToNewTaskCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToNewTaskAsync();
            });
        }, CanExecuteCommands);

        public ICommand NavigateToAllTasksCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToAllTasksAsync();
            });
        }, CanExecuteCommands);

        public ICommand NavigateToCompletedTaskCommand => new Command((param) =>
        {
            ExecuteLoadingAction(async () =>
            {
                var interval = param is AggregationTimeInterval inter ? inter : AggregationTimeInterval.Shift;
                await NavigateToCompletedTaskAsync(interval);
            });
        }, CanExecuteCommands);

        public ICommand NavigateToTaskBarCommand => new Command<object>(async (obj) =>
        {
            ExecuteLoadingAction(async () =>
            {
                if (!IsInitialized)
                    return;

                await NavigateToTaskBarListAsync(obj);
            });
        });

        protected override void RefreshCanExecute()
        {
            (NavigateToAllTasksCommand as Command)?.ChangeCanExecute();
            (NavigateToNewTaskCommand as Command)?.ChangeCanExecute();
            (NavigateToCompletedTaskCommand as Command)?.ChangeCanExecute();
            (NavigateToTaskBarCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        #endregion

        #region Private Members

        private readonly SemaphoreSlim tasksSemaphore = new SemaphoreSlim(1, 1);

        #endregion

        #region Init

        private readonly ITaskReportService _taskReportService;
        private readonly ITaskCommentService _taskCommentService;
        private readonly ITasksService _taskService;
        private readonly IUpdateService _updateService;
        private readonly ISyncService _syncService;
        private readonly SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);

        public TaskViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            ITaskReportService taskReport,
            ITasksService tasksService,
            IUpdateService updateService,
            ISyncService syncService,
            ITaskCommentService taskCommentService) : base(navigationService, userService, messageService, actionsService)
        {
            this._taskReportService = taskReport;
            this._taskService = tasksService;
            this._updateService = updateService;
            this._syncService = syncService;
            this._taskCommentService = taskCommentService;
        }

        public override async Task Init()
        {
            Settings.AppSettings.SubpageTasks = MenuLocation.None;

            Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.chooseTasksScreenTitle)} - {Settings.AreaSettings.WorkAreaName}";


            RegisterMessagingCenter();
            await LoadTasksAsync();
            var commentTask = _taskCommentService.LoadCommentCountForTasksAsync(Tasks.GetAllTasks());
            var dashboardTask = FillDashboard();
            await Task.WhenAll(commentTask, dashboardTask);
            await base.Init();
        }

        private void UnregisterMessagingCenter()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
                    MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.TasksChanged);
                    MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionsChanged);
                    MessagingCenter.Unsubscribe<ActionsService, ActionChangedMessageArgs>(this, Constants.ActionChanged);
                    MessagingCenter.Unsubscribe<TaskCommentEditViewModel, int>(this, Constants.TaskCommentAdded);
                    MessagingCenter.Unsubscribe<TaskTemplatesService, TaskTemplateModel>(this, Constants.TaskTemplatesChanged);
                });
            }
            catch (Exception ex)
            {

            }
        }

        private void RegisterMessagingCenter()
        {

            MessagingCenter.Subscribe<Application>(Application.Current, Constants.QuickTimer, async (sender) =>
            {
                try
                {
                    if (await FifteenSecondLock.WaitAsync(0))
                    {
                        await Task.Run(async () =>
                        {
                            if (await _updateService?.CheckForUpdatedTasksAsync())
                            {
                                var statuses = await _syncService.ReloadTasksStatussesAsync();
                                await UpdateTasksStatussesAsync(statuses);

                                RefreshOverviewCounts();
                            }

                            if (await _updateService?.CheckForUpdatedPropertyValues())
                            {
                                var propertyValuesUpdates = await _syncService.GetPropertyValuesUpdatesAsync();
                                await UpdatePropertyValuesAsync(propertyValuesUpdates);
                            }

                            if (await _updateService?.CheckForUpdatedTaskCommentsAsync())
                            {
                                await _syncService.LoadTaskCommentsAsync();
                                await _taskCommentService.LoadCommentCountForTasksAsync(Tasks.GetAllTasks());
                                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.TaskCommentChanged); });
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    //Debugger.Break();
                }
                finally
                {
                    if (FifteenSecondLock.CurrentCount == 0)
                        FifteenSecondLock.Release();
                }

            });

            MessagingCenter.Subscribe<Application>(Application.Current, Constants.TasksChanged, (sender) =>
            {
                RefreshOverviewCounts();
            });

            MessagingCenter.Subscribe<ActionsService, ActionChangedMessageArgs>(this, Constants.ActionChanged, HandleActionChangedMessage);

            // Listen for the events from task comments
            MessagingCenter.Subscribe<TaskCommentEditViewModel, int>(this, Constants.TaskCommentAdded, (_, id) =>
            {
                // When a comment is created we need to update the count number
                Tasks.GetAllTasks().Where(x => x.Id == id).ForEach(x => x.CommentCount++);
            });

            MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionsChanged, async _ =>
            {
                // Load action counts if needed
                try
                {
                    await tasksSemaphore.WaitAsync();
                    if (_taskService != null)
                        await _taskService.LoadOpenActionCountForTasksAsync(Tasks?.GetAllTasks(), refresh: false);
                }
                finally
                {
                    tasksSemaphore.Release();
                }
            });

            MessagingCenter.Subscribe<TaskTemplatesService, TaskTemplateModel>(this, Constants.TaskTemplatesChanged, async (sender, template) =>
            {
                if (template == null) return;
                await Task.Run(async () =>
                {
                    // Reload tasks but only the cache
                    // If we were to change the Tasks collections this changes would not propagate to the list page and the slide page
                    // and these three pages depend on sharing the same lists of the same instances of task objets
                    IsRefreshing = true;
                    await LoadTasksAsync(updateCacheOnly: true);
                    IsRefreshing = false;
                    Tasks.GetAllTasks().Where(task => task.TemplateId == template.Id).ForEach(task =>
                    {
                        // Update some of the properties
                        task.Name = template.Name;
                        task.Description = template.Description;
                        task.Picture = template.Picture;
                        task.Video = template.Video;
                        task.VideoThumbnail = template.VideoThumbnail;
                        task.DescriptionFile = template.DescriptionFile;
                        task.Steps = template.Steps?.Cast<StepModel>().ToList();
                    });
                });

            });
        }

        protected override void Dispose(bool disposing)
        {
            UnregisterMessagingCenter();
            _taskReportService.Dispose();
            _taskService.Dispose();
            _updateService.Dispose();
            _syncService?.Dispose();
            _taskCommentService.Dispose();
            ReportItemsShift = null;
            ReportItemsToday = null;
            ReportItemsWeek = null;
            ReportItemsYesterday = null;
            ReportItemsOverDue = null;
            ReportItemsPreviousShift = null;
            ReportItemsPreviousWeek = null;
            Tasks = null;
            base.Dispose(disposing);
        }

        #endregion

        private async Task UpdatePropertyValuesAsync(List<TaskExtendedDataBasic> updatedTasks)
        {
            if (updatedTasks.IsNullOrEmpty())
                await Task.CompletedTask;

            try
            {
                await tasksSemaphore.WaitAsync();

                // Get all tasks
                var allTasks = Tasks.GetAllTasks();

                var updates = updatedTasks
                  // Use group join because tasks with the same ID can be in two different places. 
                  // e.g. Shift tasks and Today tasks
                  .GroupJoin(allTasks, change => change.TaskId, task => task.Id, (change, tasks) => new
                  {
                      tasks,
                      change
                  })
                  .Where(x => x.tasks.Count() > 0)
                  .ToList();

                // Loop over the updates
                foreach (var pair in updates)
                {
                    // Loop over the affected tasks
                    foreach (var task in pair.tasks)
                    {
#if DEBUG
                        var updatedProperties = 0;
#endif
                        var needToReloadCache = false;

                        // If we have realized time
                        if (pair.change.TimeRealizedById.HasValue)
                        {
                            // Realized time has changed
                            // Determine if we need to reload cache afterwards
                            if (task.TimeTaken != pair.change.TimeTaken ||
                                task.TimeRealizedBy != pair.change.TimeRealizedBy ||
                                task.TimeRealizedById != pair.change.TimeRealizedById)
                            {
                                needToReloadCache = true;
#if DEBUG
                                updatedProperties++;
#endif
                            }

                            task.TimeTaken = pair.change.TimeTaken;
                            task.TimeRealizedBy = pair.change.TimeRealizedBy;
                            task.TimeRealizedById = pair.change.TimeRealizedById;
                            var plannedTimeProperty = task.PropertyList?.FirstOrDefault(x => x.IsPlannedTimeProperty);
                            if (plannedTimeProperty != null)
                            {
                                // For realized time create dummy property user value for updating
                                plannedTimeProperty.UpdateUserValue(new PropertyUserValue()
                                {
                                    UserValueTime = task.TimeTaken?.ToString(),
                                });
                            }
                        }
                        if (pair.change.PropertyUserValues != null)
                        {
                            foreach (var value in pair.change.PropertyUserValues)
                            {
                                var updateNeeded = task.AddOrUpdateUserProperty(value);

                                if (updateNeeded)
                                {
                                    needToReloadCache = true;
#if DEBUG
                                    updatedProperties++;
#endif
                                }
                            }

                            task.RefreshPropertyValueString();
                        }

                        if (pair.change.CompletedDeeplinkId != null)
                        {
                            task.CompletedDeeplinkId = pair.change.CompletedDeeplinkId;
                            needToReloadCache = true;
                            task.ValidateDeepLink();
                        }


                        if (needToReloadCache)
                        {
#if DEBUG
                            Debug.WriteLine($"\r\n****** '{updatedProperties}' PROPERTIES CHANGED FOR TASK {{{task.Id}}}: \"{task.Name}\" ******");
#endif
                            await _taskService.AlterTaskCacheDataAsync(task);
                        }
                    }
                }



            }
            finally
            {
                tasksSemaphore.Release();
            }
        }

        private async Task UpdateTasksStatussesAsync(List<BasicTaskStatusModel> statusses)
        {
            try
            {
                if (statusses.IsNullOrEmpty()) await Task.CompletedTask;

                await tasksSemaphore.WaitAsync();

                // Get all tasks
                var allTasks = Tasks.GetAllTasks();
                // count for understanding why sometimes the tasks loose their status
                var taskscount = allTasks.Count();

                // Determine all the differences
                var differences = statusses
                    // Use group join because tasks with the same ID can be in two different places. 
                    // e.g. Shift tasks and Today tasks
                    .GroupJoin(allTasks, change => change.TaskId, task => task.Id, (change, tasks) => new
                    {
                        tasks = tasks.Where(x => x.Status != change.Status),
                        change
                    })
                    .Where(x => x.tasks.Count() > 0)
                    .ToList();
#if DEBUG
                Debug.WriteLine($"\r\n****** CHANGED TASKS: {differences.Count} ******");
#endif
                // If we got any differences
                if (differences.Any())
                {
                    // Do a quick percentage count of tasks changed
                    if (taskscount > 0)
                    {
                        try
                        {
                            var diffcount = differences.Count;
                            var percentage = (double)diffcount / taskscount * 100;
                            //    if (percentage > 80)
                            //    {
                            //        Analytics.TrackEvent("TasksStatusses changes treshold (80%) reached", new Dictionary<string, string>() {
                            //    { "Company", string.Format("{0} ({1})", UserSettings.CompanyName.ToString(), UserSettings.CompanyId.ToString()) },
                            //    { "User", string.Format("{0} {1}", UserSettings.Fullname, UserSettings.Id) },
                            //    { "DeviceDateTime", DateTimeHelper.Now.ToString() },
                            //    { "Percentage tasks changed", string.Format("{0} status changes of {1} total tasks, {2}%", diffcount, taskscount, percentage)}
                            //});
                            //    }
                        }
                        catch { }
                    }

                    // Loop over the differences
                    foreach (var pair in differences)
                    {
                        // Loop over the affected tasks
                        foreach (var task in pair.tasks)
                        {
                            // Update the task with the new status
                            task.FilterStatus = pair.change.TaskStatus;
                            task.Status = pair.change.Status;

                            task.Comment = pair.change.Comment;

                            // Update the signature
                            task.Signature = new SignatureModel
                            {
                                SignedAt = pair.change.SignedAt,
                                SignedById = pair.change.SignedById,
                                SignedBy = pair.change.SignedBy,
                                Status = pair.change.Status
                            };

                            // Check for picture proof
                            if (CompanyFeatures.RequiredProof && pair.change.HasPictureProof)
                            {
                                task.PictureProof = pair.change.PictureProof;
                                task.SetPictureProofMediaItems();
                            }

                            await _taskService.AlterTaskCacheDataAsync(task);
                        }
                    }

                    // Set the message that the statuses have changed so that the other task pages can respond
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MessagingCenter.Send(this, Constants.RecalculateAmountsMessage);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                tasksSemaphore.Release();
            }
        }

        private async Task LoadTasksAsync(bool updateCacheOnly = false)
        {
            LocalDateTime? refreshDate = null;

            // If we don't check for Internet connection the refresh date will be different than the cached one resulting in no
            // data being displayed
            if (IsRefreshing && await InternetHelper.HasInternetConnection())
            {
                refreshDate = DateTimeHelper.Now;
            }

            var tasks = await _taskService?.GetTasksForPeriodsAsync(tasksTimestamp: refreshDate, refresh: IsRefreshing);
            var overdue = await _taskService?.GetTasksForPeriodsAsync(period: TaskPeriod.OverDue, tasksTimestamp: refreshDate, refresh: IsRefreshing);

            //workaround because backend sometimes returns wrong overdue data
            SetTasksOverdue(tasks, overdue);

            if (updateCacheOnly == false)
            {
                Tasks ??= new TasksCollection();
                try
                {
                    await tasksSemaphore.WaitAsync();
                    Tasks?.SetByPeriod(TaskPeriodTypes.Shift, tasks);
                    Tasks?.SetByPeriod(TaskPeriodTypes.Today, tasks);
                    Tasks?.SetByPeriod(TaskPeriodTypes.Week, tasks);
                    Tasks?.SetOverdue(overdue);
                    Tasks?.EnsureNoPastTasks();
                    Tasks?.EnsureNoOverdueDuplcates();
                }
                finally
                {
                    tasksSemaphore.Release();
                }
            }
        }

        private void SetTasksOverdue(List<BasicTaskModel> tasks, List<BasicTaskModel> overdue)
        {
            if (tasks.IsNullOrEmpty())
                return;

            var overdueTasks = tasks.Where(task => task.DueAt < DateTime.Now && task.TaskPeriods.HasFlag(TaskPeriodTypes.Week));
            var pastCompletedTasks = overdueTasks.Where(o => o.IsTaskMarked && o.TaskMarkedDate.Day <= DateTimeHelper.Today.AddDays(-1).Day);
            //don't display completed tasks older than yesterday
            overdueTasks = overdueTasks.Where(o => !pastCompletedTasks.Contains(o)).ToList();
            if (overdueTasks != null)
            {
                foreach (var task in overdueTasks)
                {
                    task.IsOverdue = true;
                    task.TaskPeriods = TaskPeriodTypes.OverDue;
                    if (overdue.FirstOrDefault(o => o.Id == task.Id) == null)
                        overdue.Add(task);
                }
            }
        }

        private async Task FillDashboard()
        {
            RefreshOverviewCounts();

            // If any bar is invisible the visible grid items heights are slightly increased.
            if (!OverdueBarIsVisible || !ShiftBarIsVisible || !TodayBarIsVisible || !WeekBarIsVisible)
            {
                GridBarHeight = 100;
            }

            var taskOverviewReport = await _taskReportService?.GetTaskOverviewReportOnlyAsync(refresh: IsRefreshing);
            if (taskOverviewReport != null)
            {
                ReportItemsPreviousShift = taskOverviewReport.LastShift ?? new List<TaskOverviewReportItemModel>();
                ReportItemsYesterday = taskOverviewReport.Yesterday ?? new List<TaskOverviewReportItemModel>();
                ReportItemsPreviousWeek = taskOverviewReport.LastWeek ?? new List<TaskOverviewReportItemModel>();
            }
        }

        private async Task RefreshOverviewCounts()
        {
            if (Tasks?.ListAreLoaded == false) await LoadTasksAsync();

            ReportItemsShift = GetOverviewFromTasks(Tasks?.Shift);
            ReportItemsToday = GetOverviewFromTasks(Tasks?.Today);
            ReportItemsWeek = GetOverviewFromTasks(Tasks?.Week);
            ReportItemsOverDue = GetOverviewFromTasks(Tasks?.Overdue);
        }

        private void HandleActionChangedMessage(object sender, ActionChangedMessageArgs msg)
        {
            if (msg == null)
                return;

            if (msg.TypeOfChange != ActionChangedMessageArgs.ChangeType.None)
            {
                var allTasks = Tasks?.GetAllTasks();
                if (allTasks == null)
                    return;

                var task = allTasks.FirstOrDefault(x => x.TemplateId == msg.TaskTemplateId);
                if (task != null)
                {
                    if (msg.TypeOfChange == ActionChangedMessageArgs.ChangeType.SetToResolved)
                    {
                        if (task.OpenActionCount > 0)
                            task.OpenActionCount--;
                    }
                    else if (msg.TypeOfChange == ActionChangedMessageArgs.ChangeType.Created)
                    {
                        _ = _taskService.LoadOpenActionCountForTaskAsync(task, true);
                    }
                }
            }
        }

        private List<TaskOverviewReportItemModel> GetOverviewFromTasks(List<BasicTaskModel> tasks)
        {
            var result = new List<TaskOverviewReportItemModel>();
            foreach (var status in (TaskStatusEnum[])Enum.GetValues(typeof(TaskStatusEnum)))
            {
                result.Add(new TaskOverviewReportItemModel()
                {
                    TaskStatus = status,
                    NrOfItems = tasks?.Count(x => x.FilterStatus == status) ?? 0,
                });
            }

            return result;
        }

        protected override async Task RefreshAsync()
        {
            await LoadTasksAsync();
            await _taskCommentService.LoadCommentCountForTasksAsync(Tasks.GetAllTasks());
            await FillDashboard();
            await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.RecalculateAmountsMessage); });
        }

        #region Navigation

        private async Task NavigateToNewTaskAsync()
        {
            await NavigationService.NavigateAsync<EditTaskViewModel>();
        }

        private async Task NavigateToAllTasksAsync()
        {
            await NavigationService.NavigateAsync<AllTasksViewModel>();
        }

        private async Task NavigateToCompletedTaskAsync(AggregationTimeInterval timeInterval)
        {
            using var scope = App.Container.CreateScope();
            var completedTaskViewModel = scope.ServiceProvider.GetService<CompletedTaskViewModel>();
            completedTaskViewModel.CurrentInterval = timeInterval;

            await NavigationService.NavigateAsync(viewModel: completedTaskViewModel);
        }

        private async Task NavigateToTaskBarListAsync(object obj)
        {
            if (obj is TaskPeriod taskPeriod)
            {
                using var scope = App.Container.CreateScope();
                var taskListViewModel = scope.ServiceProvider.GetService<TaskListViewModel>();
                taskListViewModel.AllTasks = Tasks;
                taskListViewModel.Period = taskPeriod;
                taskListViewModel.OverdueIsVisible = OverdueBarIsVisible;

                await NavigationService.NavigateAsync(viewModel: taskListViewModel);
            }
        }

        #endregion

    }
}