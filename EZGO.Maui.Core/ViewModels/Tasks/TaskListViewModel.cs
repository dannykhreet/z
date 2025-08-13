using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Messages;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Services.Message;
using EZGO.Maui.Core.Services.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using NodaTime;
using PropertyChanged;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.TreeView.Engine;
using System.Diagnostics;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class TaskListViewModel : BasicTaskViewModel
    {
        /// <summary>
        /// Stores the task on which the action button was pressed
        /// </summary>
        private BasicTaskModel lastTappedActionTask;

        private BasicTaskModel SelectedTask = new BasicTaskModel();

        private readonly ITaskTemplatesSerivce _taskTemplateService;

        #region dropdown
        public bool OverdueIsVisible { get; set; } = true;

        string shift => TranslateExtension.GetValueFromDictionary(LanguageConstants.tasksScreenShiftFilterText);

        string today => TranslateExtension.GetValueFromDictionary(LanguageConstants.tasksScreenTodayFilterText);

        string week => TranslateExtension.GetValueFromDictionary(LanguageConstants.tasksScreenWeekFilterText);

        string overdue => TranslateExtension.GetValueFromDictionary(LanguageConstants.tasksScreenShiftOverdueText);

        string alltasks => TranslateExtension.GetValueFromDictionary(LanguageConstants.tasksScreenNoFilterText);

        public Rect Rect { get; set; } = new Rect(113, .2, .4, .3);

        public IScoreColorCalculator ScoreColorCalculator { get => ScoreColorCalculatorFactory.Default(0, 1); }

        #endregion

        /// <summary>
        /// Gets or sets the ListView layout.
        /// </summary>
        /// <value>
        /// The ListView layout.
        /// </value>
        public ListViewLayout ListViewLayout { get; set; }

        public bool IsListVisible { get; set; }

        /// <summary>
        /// Gets or sets the task period.
        /// </summary>
        /// <value>
        /// The task period.
        /// </value>
        public TaskPeriod Period { get; set; }

        public string ReasonInput { get; set; }

        public MultiTaskStatusWithReason MultiTaskStatusWithReason { get; set; } = new MultiTaskStatusWithReason();

        public bool IsMultiSkipAvailable { get; set; } = CompanyFeatures.TaskMultiskipEnabled;

        [DoNotNotify]
        public TasksCollection AllTasks { get; set; }

        public bool IsActionPopupOpen { get; set; }

        #region Search

        public bool IsSearchBarVisible { get; set; }

        public FilterControl<BasicTaskModel, TaskStatusEnum> TaskFilter { get; set; } = new FilterControl<BasicTaskModel, TaskStatusEnum>(null);
        #endregion

        #region Commands

        /// <summary>
        /// Gets the task skipped command.
        /// </summary>
        /// <value>
        /// The task skipped command.
        /// </value>
        public ICommand TaskSkippedCommand { get; private set; }

        /// <summary>
        /// Gets the task not ok command.
        /// </summary>
        /// <value>
        /// The task not ok command.
        /// </value>
        public ICommand TaskNotOkCommand { get; private set; }

        /// <summary>
        /// Gets the task ok command.
        /// </summary>
        /// <value>
        /// The task ok command.
        /// </value>
        public ICommand TaskOkCommand { get; private set; }

        /// <summary>
        /// Gets the ListView layout command.
        /// </summary>
        /// <value>
        /// The ListView layout command.
        /// </value>
        public ICommand ListViewLayoutCommand { get; private set; }

        public ICommand FilterCommand { get; private set; }

        /// <summary>
        /// Gets the search text changed command.
        /// </summary>
        /// <value>
        /// The search text changed command.
        /// </value>
        public ICommand SearchTextChangedCommand { get; private set; }

        /// <summary>
        /// Gets the detail command.
        /// </summary>
        /// <value>
        /// The detail command.
        /// </value>
        public ICommand DetailCommand { get; private set; }

        public ICommand DeepLinkCommand { get; private set; }

        public ICommand ActionCommand { get; private set; }

        public ICommand NavigateToNewActionCommand { get; private set; }

        public ICommand NavigateToNewCommentCommand { get; private set; }

        /// <summary>
        /// Gets the dropdown tap command.
        /// </summary>
        /// <value>
        /// The dropdown tap command.
        /// </value>
        public ICommand DropdownTapCommand { get; private set; }

        public ICommand CloseSelectionCommand { get; private set; }

        protected override void RefreshCanExecute()
        {
            (TaskSkippedCommand as Command<BasicTaskModel>)?.ChangeCanExecute();
            (TaskNotOkCommand as Command<BasicTaskModel>)?.ChangeCanExecute();
            (TaskOkCommand as Command<BasicTaskModel>)?.ChangeCanExecute();
            (ListViewLayoutCommand as Command)?.ChangeCanExecute();
            (SearchTextChangedCommand as Command)?.ChangeCanExecute();
            (DetailCommand as Command<object>)?.ChangeCanExecute();
            (DeepLinkCommand as Command<BasicTaskModel>)?.ChangeCanExecute();
            (ActionCommand as Command<BasicTaskModel>)?.ChangeCanExecute();
            (NavigateToNewActionCommand as Command)?.ChangeCanExecute();
            (NavigateToNewCommentCommand as Command)?.ChangeCanExecute();
            (DropdownTapCommand as Command)?.ChangeCanExecute();
            (CloseSelectionCommand as Command)?.ChangeCanExecute();
            (FilterCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        #endregion

        #region Services
        private readonly ITasksService _taskService;

        public TaskListViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            ITasksService tasksService,
            ITaskTemplatesSerivce taskTemplatesService) : base(navigationService, userService, messageService, actionsService)
        {
            _taskService = tasksService;
            _taskTemplateService = taskTemplatesService;

            Task.Run(() =>
            {
                TaskSkippedCommand = new Command<BasicTaskModel>(task =>
                {
                    ExecuteLoadingAction(async () =>
                    {
                        if (task.HasPictureProof)
                            await SetTaskWithPictureProofStatus(task, TaskStatusEnum.Skipped);
                        else
                        {
                            await TaskHelper.SetTaskStatusAsync(task, TaskStatusEnum.Skipped, TaskFilter, false);
                            SetSkipAllAvailability();
                        }
                        task.ResetValidation();
                    });
                }, CanExecuteCommands);


                TaskNotOkCommand = new Command<BasicTaskModel>(task =>
                {
                    ExecuteLoadingAction(async () =>
                    {
                        if (!await Validate(task))
                            return;

                        if (task.HasPictureProof)
                            await SetTaskWithPictureProofStatus(task, TaskStatusEnum.NotOk);
                        else
                        {
                            await TaskHelper.SetTaskStatusAsync(task, TaskStatusEnum.NotOk, TaskFilter, false);
                            SetSkipAllAvailability();
                        }
                    });
                }, CanExecuteCommands);

                TaskOkCommand = new Command<BasicTaskModel>(task =>
                {
                    ExecuteLoadingAction(async () =>
                    {
                        if (!await Validate(task))
                            return;

                        if (task.HasPictureProof)
                            await SetTaskWithPictureProofStatus(task, TaskStatusEnum.Ok);
                        else
                        {
                            await TaskHelper.SetTaskStatusAsync(task, TaskStatusEnum.Ok, TaskFilter, false);
                            SetSkipAllAvailability();
                        }
                    });
                }, CanExecuteCommands);

                ListViewLayoutCommand = new Command<object>(SetListViewLayout, CanExecuteCommands);

                FilterCommand = new Command<object>((x) => ExecuteLoadingAction(() =>
                {
                    TaskFilter.Filter(x, useDataSource: false);
                    SetSkipAllAvailability();
                }), CanExecuteCommands);

                SearchTextChangedCommand = new Command((obj) =>
                {
                    if (obj is string searchText)
                        TaskFilter.SearchText = searchText;
                    TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
                    SetSkipAllAvailability();
                });

                DetailCommand = new Command<object>((obj) => ExecuteLoadingAction(async () =>
                await NavigateToDetailCarouselAsync(obj)), CanExecuteCommands);

                DeepLinkCommand = new Command<BasicTaskModel>(async task =>
                {
                    await NavigateToDeepLinkAsync(task);
                }, CanExecuteCommands);

                SearchTextChangedCommand = new Command((obj) =>
                {
                    if (obj is string searchText)
                        TaskFilter.SearchText = searchText;
                    TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
                    SetSkipAllAvailability();
                    TaskHelper.CalculateTaskAmounts(TaskFilter);
                });

                ActionCommand = new Command<BasicTaskModel>((task) => ExecuteLoadingAction(async () =>
                        await OpenPopupOrNavigateToActionsAsync(task)), CanExecuteCommands);

                NavigateToNewActionCommand = new Command(() => ExecuteLoadingAction(async () =>
                        await NavigateToNewActionAsync()), CanExecuteCommands);

                NavigateToNewCommentCommand = new Command(() => ExecuteLoadingAction(async () =>
                        await NavigateToNewCommentAsync()), CanExecuteCommands);

                DropdownTapCommand = new Command<object>(async (obj) =>
                    await ExecuteLoadingActionAsync(async () =>
                    {
                        if (obj is TreeViewNode tree)
                        {
                            await ApplyFilterAsync(tree.Content as FilterModel);
                        }
                        else if (obj is FilterModel filter)
                        {
                            await ApplyFilterAsync(filter);
                        }
                    }
                    ), CanExecuteCommands);

                CloseSelectionCommand = new Command(() =>
                {
                    IsDropdownOpen = false;
                }, CanExecuteCommands);
            });
        }

        #endregion

        private async Task<bool> Validate(BasicTaskModel task)
        {
            if (task.Validate())
                return true;

            await NavigateToDetailCarouselAsync(task, true);
            return false;
        }
        private async Task SetTaskWithPictureProofStatus(BasicTaskModel task, TaskStatusEnum status)
        {
            SelectedTask = task;
            CurrentStatus = status;

            bool owner = (SelectedTask.Signature?.SignedById ?? UserSettings.Id) == UserSettings.Id;
            if (owner)
            {
                if (SelectedTask.FilterStatus == status)
                {
                    if (status == TaskStatusEnum.Skipped)
                    {
                        await TaskHelper.UploadTaskStatusAsync(SelectedTask, TaskStatusEnum.Todo, false, null);
                        FilterAndRecalculateTasks();
                    }
                    else
                    {
                        await OpenUntapTaskDialogAsync();
                    }
                }
                else
                {
                    if (status == TaskStatusEnum.Skipped)
                    {
                        OpenSkipTaskPopup();
                    }
                    else if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped || SelectedTask.FilterStatus == TaskStatusEnum.Todo)
                    {
                        await NavigateToNewPictureProof(SelectedTask, status);
                        CurrentStatus = null;
                    }
                    else
                        OpenChangeStatusPopup();
                }
            }
            else
            {
                if (SelectedTask.FilterStatus == status)
                {
                    if (status == TaskStatusEnum.Skipped)
                        return;

                    await NavigateToPictureProofDetails(task);
                    return;
                }
                if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped || SelectedTask.FilterStatus == TaskStatusEnum.Todo)
                {
                    await NavigateToNewPictureProof(SelectedTask, status);
                    CurrentStatus = null;
                    return;
                }
                await OpenCantTapDialogAsync();
            }
            SetSkipAllAvailability();
        }

        private void FilterAndRecalculateTasks()
        {
            if (TaskFilter != null && !TaskFilter.StatusFilters.IsNullOrEmpty())
            {
                TaskFilter.Filter(TaskFilter.StatusFilters, resetIfTheSame: false, useDataSource: false);
            }
            TaskHelper.CalculateTaskAmounts(TaskFilter);
            SetSkipAllAvailability();
        }

        private async Task NavigateToNewPictureProof(BasicTaskModel task, TaskStatusEnum status)
        {
            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.SelectedTask = task;
            pictureProofViewModel.TaskStatus = status;
            pictureProofViewModel.IsNew = true;
            pictureProofViewModel.SupportsEditing = true;
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
        }

        private async Task NavigateToPictureProofDetails(BasicTaskModel task, TaskStatusEnum? status = null)
        {
            bool isOwner = (task.Signature?.SignedById ?? UserSettings.Id) == UserSettings.Id;

            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.MainMediaElement = task.PictureProofMediaItems?.FirstOrDefault();

            if (task.PictureProofMediaItems?.Count > 1)
                pictureProofViewModel.MediaElements = new System.Collections.ObjectModel.ObservableCollection<MediaItem>(task.PictureProofMediaItems?.Skip(1));

            pictureProofViewModel.IsNew = false;
            pictureProofViewModel.EditingEnabled = false;
            pictureProofViewModel.SupportsEditing = false;

            if (isOwner)
            {
                pictureProofViewModel.SelectedTask = task;
                pictureProofViewModel.TaskStatus = status ?? task.FilterStatus;
                pictureProofViewModel.SupportsEditing = true;
            }
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
        }

        public async override Task SubmitSkipCommandAsync()
        {
            await TaskHelper.SetTaskStatusAsync(SelectedTask, TaskStatusEnum.Skipped, TaskFilter, false);
            await base.SubmitSkipCommandAsync();
            SetSkipAllAvailability();
        }

        public void SetSkipAllAvailability()
        {
            var list = TaskFilter?.FilteredList;
            if (list is null)
            {
                IsSkipAllAvailable = false;
                return;
            }

            IsSkipAllAvailable = list.Any(t => t.FilterStatus == TaskStatusEnum.Todo && t.Score == null);
        }

        public override async Task SubmitSkipAllTasksCommandAsync()
        {
            MultiTaskStatusWithReason.TaskIds = new List<int>();

            var tasksToSkip = TaskFilter.FilteredList
                .Where(t => t.FilterStatus == TaskStatusEnum.Todo && t.Score == null)
                .ToList();

            bool owner = (SelectedTask.Signature?.SignedById ?? UserSettings.Id) == UserSettings.Id;

            tasksToSkip
                .ForEach(t => MultiTaskStatusWithReason.TaskIds.Add((int)t.Id));

            FilterAndRecalculateTasks();

            tasksToSkip.ForEach
            (
                async t =>
                {
                    t.Comment = ReasonInput;
                    await TaskHelper.SetTaskStatusAsync(t, TaskStatusEnum.Skipped, TaskFilter, false, false);
                    FilterAndRecalculateTasks();
                }
            );

            MultiTaskStatusWithReason.Comment = ReasonInput;

            MultiTaskStatusWithReason.Status = 0;

            MultiTaskStatusWithReason.SignedAtUtc = DateTime.UtcNow;

            if (await InternetHelper.HasInternetConnection())
                await _taskService.PostTaskStatusWithReasonAsync(MultiTaskStatusWithReason);
            else
                await _taskService.AddLocalMultiskipTaskAsync(MultiTaskStatusWithReason);

            SetSkipAllAvailability();

            ReasonInput = "";

            await base.SubmitSkipAllTasksCommandAsync();
        }

        public async override Task KeepButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                await TaskHelper.SetTaskStatusAsync(SelectedTask, CurrentStatus.Value, TaskFilter, false);
            }
            await base.KeepButtonChangeStatusPopupCommandAsync();
        }


        public async override Task RemoveButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                await NavigateToPictureProofDetails(SelectedTask, CurrentStatus.Value);
            }
        }

        public async override Task UntapTaskAsync()
        {
            await TaskHelper.UploadTaskStatusAsync(SelectedTask, TaskStatusEnum.Todo, false, null);
            FilterAndRecalculateTasks();
            SetSkipAllAvailability();
        }

        public async override Task SeePicturesAsync()
        {
            await NavigateToPictureProofDetails(SelectedTask);
        }



        public override async Task Init()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            Debug.WriteLine("Started Init of TaskListViewModel");

            var periodTasks = AllTasks.GetByPeriod(Period);
            await Task.Run(() =>
            {
                TaskFilter.SetUnfilteredItems(periodTasks);
                TaskHelper.CalculateTaskAmounts(TaskFilter);
                var filters = new List<FilterModel>()
                {
                    new FilterModel(shift),
                    new FilterModel(today),
                    new FilterModel(week)
                };

                if (OverdueIsVisible)
                    filters.Add(new FilterModel(overdue));

                TaskFilter.AddFilters(filters.ToArray());
                SetSkipAllAvailability();
                SetFilter();
                RegisterMessagingCenter();
                Settings.SubpageTasks = MenuLocation.TasksList;
                Title = $"{Settings.AreaSettings.WorkAreaName}";
                SetListViewLayout(Settings.ListViewLayout);
            }).ConfigureAwait(false);
            st.Stop();
            Debug.WriteLine($"Finished initialization: {st.ElapsedMilliseconds} ms");
            await base.Init();
        }

        private void RegisterMessagingCenter()
        {
            MessagingCenter.Subscribe<TaskViewModel>(this, Constants.RecalculateAmountsMessage, mv =>
            {
                TaskFilter.RefreshStatusFilter(false);
                SetSkipAllAvailability();
                TaskHelper.CalculateTaskAmounts(TaskFilter);
            });

            MessagingCenter.Subscribe<TaskSlideViewModel>(this, Constants.RecalculateAmountsMessage, mv =>
            {
                TaskFilter.RefreshStatusFilter(false, false);
                SetSkipAllAvailability();
                TaskHelper.CalculateTaskAmounts(TaskFilter);
            });

            MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionsChanged, async _ =>
            {
                // Load action counts if needed
                await _taskService.LoadOpenActionCountForTasksAsync(AllTasks.GetAllTasks(), refresh: false);
            });

            MessagingCenter.Subscribe<MessageService, BasicTaskModel>(this, Constants.LinkedChecklistSigned, async (_, taskFromDeepLink) =>
            {
                if (!OnlineShiftCheck.IsShiftChangeAllowed)
                    TaskFilter.SetUnfilteredItems(new List<BasicTaskModel> { taskFromDeepLink });

                var pageT = ViewFactory.GetView(typeof(TaskSlideViewModel));
                var isInNavigationStack = NavigationService.IsInNavigationStack(pageT);

                if (isInNavigationStack)
                {
                    taskFromDeepLink.Validate();
                    return;
                }

                await HandleLinkedItemSigned(taskFromDeepLink, true);
            });

            MessagingCenter.Subscribe<TasksService, MandatoryItemFinishedMessageArgs>(this, Constants.MandatoryItemFinished, async (_, args) =>
            {
                var task = AllTasks.GetAllTasks().FirstOrDefault(x => x.Id == args.TaskId);
                if (task != null)
                {
                    task.CompletedDeeplinkId = args.ItemId;
                    await HandleLinkedItemSigned(task);
                }
            });

            MessagingCenter.Subscribe<PictureProofViewModel>(this, Constants.PictureProofChanged, (_) =>
            {
                FilterAndRecalculateTasks();
            });
        }

        private async Task HandleLinkedItemSigned(BasicTaskModel taskFromDeepLink, bool navigateToDetail = false)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!taskFromDeepLink.Validate())
                {
                    if (navigateToDetail)
                        await NavigateToDetailCarouselAsync(taskFromDeepLink);
                    return;
                }

                if (taskFromDeepLink.FilterStatus == TaskStatusEnum.Ok)
                    return;

                if (taskFromDeepLink.HasPictureProof)
                    await NavigateToNewPictureProof(taskFromDeepLink, TaskStatusEnum.Ok);
                else
                    await TaskHelper.SetTaskStatusAsync(taskFromDeepLink, TaskStatusEnum.Ok, TaskFilter, false);
            });
        }

        private async Task ApplyFilterAsync(FilterModel filter)
        {
            if (filter.Name == shift)
                Period = TaskPeriod.Shift;
            else if (filter.Name == today)
                Period = TaskPeriod.Today;
            else if (filter.Name == week)
                Period = TaskPeriod.Week;
            else if (filter.Name == overdue)
                Period = TaskPeriod.OverDue;

            await Task.Run(() =>
            {
                TaskFilter.SetUnfilteredItems(AllTasks.GetByPeriod(Period));
                TaskFilter.Filter(filter, useDataSource: false);
                TaskHelper.CalculateTaskAmounts(TaskFilter);
            });

            IsDropdownOpen = false;
            SetSkipAllAvailability();
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<TaskViewModel>(this, Constants.RecalculateAmountsMessage);
                MessagingCenter.Unsubscribe<TaskSlideViewModel>(this, Constants.RecalculateAmountsMessage);
                MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionsChanged);
                MessagingCenter.Unsubscribe<MessageService, BasicTaskModel>(this, Constants.LinkedChecklistSigned);
                MessagingCenter.Unsubscribe<PictureProofViewModel>(this, Constants.PictureProofChanged);
                MessagingCenter.Unsubscribe<TasksService, MandatoryItemFinishedMessageArgs>(this, Constants.MandatoryItemFinished);
            });

            _taskService.Dispose();
            _taskTemplateService.Dispose();
            TaskFilter?.Dispose();
            AllTasks = null;
            TaskFilter = null;
            base.Dispose(disposing);
        }

        private async Task LoadTasks(bool isRefreshing = false)
        {
            await AsyncAwaiter.AwaitAsync(nameof(TaskListViewModel), async () =>
            {
                LocalDateTime? refreshDate = null;

                // If we don't check for Internet connection the refresh date will be different than the cached one resulting in no
                // data being displayed
                if (await InternetHelper.HasInternetConnection() && isRefreshing)
                    refreshDate = DateTimeHelper.Now;

                List<BasicTaskModel> allTasks = await _taskService.GetTasksForPeriodsAsync(tasksTimestamp: refreshDate, refresh: isRefreshing);
                List<BasicTaskModel> overdue = await _taskService.GetTasksForPeriodsAsync(period: TaskPeriod.OverDue, tasksTimestamp: refreshDate, refresh: isRefreshing);

                //workaround because backend sometimes returns wrong overdue data
                SetTasksOverdue(allTasks, overdue);

                if (AllTasks != null)
                {
                    AllTasks.Shift = allTasks.Where(x => x.TaskPeriods.HasFlag(TaskPeriodTypes.Shift)).ToList();
                    AllTasks.Today = allTasks.Where(x => x.TaskPeriods.HasFlag(TaskPeriodTypes.Today)).ToList();
                    AllTasks.Week = allTasks.Where(x => x.TaskPeriods.HasFlag(TaskPeriodTypes.Week)).ToList();
                    AllTasks.Overdue = overdue;
                    AllTasks.EnsureNoPastTasks();
                    AllTasks.EnsureNoOverdueDuplcates();

                    await _taskTemplateService.GetAllTemplatesForCurrentAreaAsync(isRefreshing);

                    if (TaskFilter != null)
                    {
                        TaskFilter.SetUnfilteredItems(AllTasks.GetByPeriod(Period));
                        TaskFilter.Filter(TaskFilter.StatusFilters, false, false);
                    }
                }
            });
            TaskHelper.CalculateTaskAmounts(TaskFilter);
            SetSkipAllAvailability();
        }

        protected override async Task RefreshAsync()
        {
            await LoadTasks(isRefreshing: true);
        }

        private void SetTasksOverdue(List<BasicTaskModel> tasks, List<BasicTaskModel> overdue)
        {
            var overdueTasks = tasks.Where(task => task.DueAt < DateTime.Now && task.TaskPeriods.HasFlag(TaskPeriodTypes.Week));
            var pastCompletedTasks = overdueTasks.Where(o => o.IsTaskMarked && o.TaskMarkedDate.Day <= DateTimeHelper.Today.AddDays(-1).Day);
            //don't display completed tasks older than yesterday
            overdueTasks = overdueTasks.Where(o => !pastCompletedTasks.Contains(o)).ToList();
            foreach (var task in overdueTasks)
            {
                task.IsOverdue = true;
                task.TaskPeriods = TaskPeriodTypes.OverDue;
                if (overdue.FirstOrDefault(o => o.Id == task.Id) == null)
                    overdue.Add(task);
            }
        }

        /// <summary>
        /// Sets the ListView layout.
        /// </summary>
        /// <param name="listViewLayout">The list view layout.</param>
        private void SetListViewLayout(object obj)
        {
            if (obj is ListViewLayout listViewLayout)
            {
                if (listViewLayout == ListViewLayout.Grid)
                    IsListVisible = false;
                else
                    IsListVisible = true;

                ListViewLayout = listViewLayout;
                Settings.ListViewLayout = listViewLayout;
            }
            SetSkipAllAvailability();
        }

        private void SetFilter()
        {
            switch (Period)
            {
                case TaskPeriod.Shift:
                    TaskFilter.SetSelectedFilter(shift);
                    break;
                case TaskPeriod.Today:
                    TaskFilter.SetSelectedFilter(today);
                    break;
                case TaskPeriod.Week:
                    TaskFilter.SetSelectedFilter(week);
                    break;
                case TaskPeriod.OverDue:
                    TaskFilter.SetSelectedFilter(overdue);
                    break;
                default:
                    break;
            }
            SetSkipAllAvailability();
        }

        #region Navigation

        /// <summary>
        /// Navigates to detail carousel asynchronous.
        /// </summary>
        /// <param name="obj">The object.</param>
        private async Task NavigateToDetailCarouselAsync(object obj, bool validate = false)
        {
            BasicTaskModel TappedTask = null;

            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is BasicTaskModel task)
                TappedTask = task;

            if (obj is BasicTaskModel taskModel)
                TappedTask = taskModel;

            if (TappedTask != null)
            {
                int index = TaskFilter.FilteredList.IndexOf(TappedTask);

                using var scope = App.Container.CreateScope();
                var taskSlideViewModel = scope.ServiceProvider.GetService<TaskSlideViewModel>();

                taskSlideViewModel.TaskFilterControl = TaskFilter;
                taskSlideViewModel.TaskPeriod = Period;
                taskSlideViewModel.SelectedTask = TappedTask;
                taskSlideViewModel.CurrentIndex = index;

                if (validate)
                    taskSlideViewModel.SelectedTask.Validate();

                await NavigationService.NavigateAsync(viewModel: taskSlideViewModel);
            }
        }

        private async Task OpenPopupOrNavigateToActionsAsync(BasicTaskModel task)
        {
            if (task.ActionBubbleCount > 0)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskId = task.Id;
                actionOpenActionsViewModel.TaskTemplateId = task.TemplateId;
                actionOpenActionsViewModel.ActionType = ActionType.Task;
                actionOpenActionsViewModel.TaskTitle = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forTaskItem)} {task.Name}";

                await NavigationService.NavigateAsync(viewModel: actionOpenActionsViewModel);
            }
            else
            {
                lastTappedActionTask = task;
                IsActionPopupOpen = !IsActionPopupOpen;
            }
        }

        private async Task NavigateToNewCommentAsync()
        {
            using var scope = App.Container.CreateScope();
            var taskCommentEditViewModel = scope.ServiceProvider.GetService<TaskCommentEditViewModel>();
            taskCommentEditViewModel.TaskId = lastTappedActionTask.Id;
            taskCommentEditViewModel.TaskTemplateId = lastTappedActionTask.TemplateId;
            taskCommentEditViewModel.Type = ActionType.Task;
            taskCommentEditViewModel.IsNew = true;
            taskCommentEditViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forTaskItem)} {lastTappedActionTask.Name}";

            await NavigationService.NavigateAsync(viewModel: taskCommentEditViewModel);
        }

        private async Task NavigateToNewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.TaskId = lastTappedActionTask.Id;
            actionNewViewModel.TaskTemplateId = lastTappedActionTask.TemplateId;
            actionNewViewModel.ActionType = ActionType.Task;
            actionNewViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forTaskItem)} {lastTappedActionTask.Name}";

            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }

        #endregion
    }
}
