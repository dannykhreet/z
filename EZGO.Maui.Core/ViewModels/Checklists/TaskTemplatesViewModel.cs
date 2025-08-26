using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Classes.Stages;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Local;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Services.Checklists;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using MvvmHelpers.Interfaces;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;


namespace EZGO.Maui.Core.ViewModels.Checklists
{
    /// <summary>
    /// Task templates view model.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.ViewModels.BaseViewModel" />
    public class TaskTemplatesViewModel : BasicTaskViewModel
    {
        private readonly IChecklistService _checklistService;
        private readonly ITasksService _taskService;
        private readonly IUpdateService _updateService;
        private readonly IPropertyService _propertySerice;
        private readonly ITaskCommentService _commentService;

        public ChecklistTemplateModel selectedChecklist;
        public BasicChecklistTemplateModel SelectedChecklist { get; set; }
        /// <summary>
        /// Stores the task on which the action button was pressed
        /// </summary>
        private BasicTaskTemplateModel lastTappedActionTask;

        public BasicTaskTemplateModel SelectedTask { get; set; }

        #region Public Properties

        /// <summary>
        /// Gets or sets the checklist template identifier.
        /// </summary>
        /// <value>
        /// The checklist template identifier.
        /// </value>
        public int ChecklistTemplateId { get; set; }

        public bool IsActionPopupOpen { get; set; }

        public List<BasicChecklistTemplateModel> ChecklistTemplates { get; set; }

        public bool IsSignatureRequired { get; set; }

        public bool IsBusy { get; set; }

        public FilterControl<BasicTaskTemplateModel, TaskStatusEnum> TaskFilter { get; set; } = new FilterControl<BasicTaskTemplateModel, TaskStatusEnum>(null);

        public StagesControl Stages { get; set; } = new StagesControl(null, null, null);

        public Rect Rect { get; set; } = new Rect(113, .2, .4, .6);

        public bool IsSearchBarVisible { get; set; }

        /// <summary>
        /// Gets or sets the ListView layout.
        /// </summary>
        /// <value>
        /// The ListView layout.
        /// </value>
        public ListViewLayout ListViewLayout { get; set; }

        public bool IsListVisible { get; set; }

        public bool OpenedFromDeepLink { get; set; }

        public bool IsChecklistSelectorActive => !OpenedFromDeepLink && !CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled;

        public int PagesFromDeepLink { get; set; }

        public BasicTaskModel TaskFromDeepLink { get; set; }

        public ChecklistOpenFields OpenFields { get; set; }

        public IScoreColorCalculator ScoreColorCalculator { get => ScoreColorCalculatorFactory.Default(0, 1); }

        public bool IsFromBookmark { get; set; } = false;

        public bool ContainsTags => SelectedChecklist?.Tags?.Count > 0;

        public bool DeepLinkCompletionIsRequired { get; set; } = false;

        public ChecklistModel IncompleteChecklist { get; set; }

        public bool AnyRemoteChanges { get; set; }

        public bool FirstTimeSaving => IncompleteChecklist == null;

        public bool ShouldClearStatuses { get; set; } = false;
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
        /// Gets the filter command.
        /// </summary>
        /// <value>
        /// The filter command.
        /// </value>
        public ICommand FilterCommand { get; private set; }

        /// <summary>
        /// Gets the ListView layout command.
        /// </summary>
        /// <value>
        /// The ListView layout command.
        /// </value>
        public ICommand ListViewLayoutCommand { get; private set; }

        /// <summary>
        /// Gets the detail command.
        /// </summary>
        /// <value>
        /// The detail command.
        /// </value>
        public ICommand DetailCommand { get; private set; }

        /// <summary>
        /// Gets the dropdown tap command.
        /// </summary>
        /// <value>
        /// The dropdown tap command.
        /// </value>
        public ICommand DropdownTapCommand { get; private set; }

        /// <summary>
        /// Gets the search text changed command.
        /// </summary>
        /// <value>
        /// The search text changed command.
        /// </value>
        public ICommand SearchTextChangedCommand { get; private set; }

        public ICommand SignCommand { get; private set; }

        public ICommand ActionCommand { get; private set; }

        public ICommand NavigateToNewActionCommand { get; private set; }

        public ICommand NavigateToNewCommentCommand { get; private set; }

        public ICommand StepsCommand { get; private set; }

        public ICommand DeleteTagCommand { get; set; }
        public IAsyncCommand SaveCommand { get; private set; }

        public ICommand SignStageCommand { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTemplatesViewModel"/> class.
        /// </summary>
        public TaskTemplatesViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IChecklistService checklistService,
            ITasksService tasksService,
            IPropertyService propertyService,
            IUpdateService updateService,
            ITaskCommentService commentService
            ) : base(navigationService, userService, messageService, actionsService)
        {
            _checklistService = checklistService;
            _taskService = tasksService;
            _propertySerice = propertyService;
            _updateService = updateService;
            _commentService = commentService;

            ActionCommand = new Command<BasicTaskTemplateModel>(task => ExecuteLoadingAction(async () => await OpenPopupOrNavigateToActionsAsync(task)), CanExecuteCommands);

            TaskSkippedCommand = new Command<BasicTaskTemplateModel>(async taskTemplate =>
                await ExecuteLoadingAction(async () => await ToggleTaskStatus(taskTemplate, TaskStatusEnum.Skipped)), CanExecuteCommands);

            TaskNotOkCommand = new Command<BasicTaskTemplateModel>(async taskTemplate =>
                await ExecuteLoadingAction(async () => await ToggleTaskStatus(taskTemplate, TaskStatusEnum.NotOk)), CanExecuteCommands);

            TaskOkCommand = new Command<BasicTaskTemplateModel>(async taskTemplate =>
               await ExecuteLoadingAction(async () => await ToggleTaskStatus(taskTemplate, TaskStatusEnum.Ok)), CanExecuteCommands);

            FilterCommand = new Command<TaskStatusEnum?>(newStatus => ExecuteLoadingAction(() =>
            {
                TaskFilter.Filter(newStatus, useDataSource: false);
            }));

            ListViewLayoutCommand = new Command<object>(obj => ExecuteLoadingAction(() => SetListViewLayout(obj)), CanExecuteCommands);

            DetailCommand = new Command<object>(async obj =>
                await ExecuteLoadingAction(async () => await NavigateToDetailCarouselAsync(obj)), CanExecuteCommands);

            DeleteTagCommand = new Command<ItemTappedEventArgs>(obj =>
            {
                if (obj.DataItem is TagModel tag)
                {
                    TaskFilter.SearchedTags.Remove(tag);
                    tag.IsActive = !tag.IsActive;
                    TaskFilter.Filter(false, false);
                }

            }, CanExecuteCommands);

            DropdownTapCommand = new Command<object>(obj => ExecuteLoadingAction(() => DropdownTapAsync(obj)), CanExecuteCommands);

            SearchTextChangedCommand = new Command((obj) =>
            {
                if (obj is string searchText)
                    TaskFilter.SearchText = searchText;
                TaskFilter?.Filter(TaskFilter.SelectedFilter, false, useDataSource: false);
                TaskHelper.CalculateTaskAmounts(TaskFilter);


            }, CanExecuteCommands);

            SignCommand = new Command(async () =>
            {
                await ExecuteLoadingAction(NavigateToSignPageOrFinishChecklistAsync);
            }, CanExecuteCommands);

            SaveCommand = new MvvmHelpers.Commands.AsyncCommand(SaveChecklistAsync, CanExecuteCommands);

            NavigateToNewActionCommand = new Command(async () => await ExecuteLoadingAction(async () => await NavigateToNewActionAsync()), CanExecuteCommands);

            NavigateToNewCommentCommand = new Command(async () => await ExecuteLoadingAction(async () => await NavigateToNewCommentAsync()), CanExecuteCommands);

            StepsCommand = new Command<BasicTaskTemplateModel>(async obj =>
            {
                await ExecuteLoadingAction(async () => await NavigateToMoreInfoAsync(obj));
            }, CanExecuteCommands);

            SignStageCommand = new Command<BasicTaskTemplateModel>(async (taskTemplate) =>
            {
                await NavigateToSignPageOrSaveStage(taskTemplate);
            });

            DiscardChangesPopupClosed = Task.CompletedTask;
        }

        protected override void RefreshCanExecute()
        {
            (ActionCommand as Command)?.ChangeCanExecute();
            (TaskSkippedCommand as Command)?.ChangeCanExecute();
            (TaskNotOkCommand as Command)?.ChangeCanExecute();
            (TaskOkCommand as Command)?.ChangeCanExecute();
            (FilterCommand as Command)?.ChangeCanExecute();
            (DetailCommand as Command)?.ChangeCanExecute();
            (DropdownTapCommand as Command)?.ChangeCanExecute();
            (SearchTextChangedCommand as Command)?.ChangeCanExecute();
            (SignCommand as Command)?.ChangeCanExecute();
            (NavigateToNewActionCommand as Command)?.ChangeCanExecute();
            (NavigateToNewCommentCommand as Command)?.ChangeCanExecute();
            (StepsCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        ~TaskTemplatesViewModel()
        { // Breakpoint here
        }

        private readonly SemaphoreSlim FifteenSecondLock = new SemaphoreSlim(1, 1);


        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            if (PagesFromDeepLink > 0 || IsFromBookmark)
                OpenedFromDeepLink = true;

            OpenFields = new ChecklistOpenFields(null, ChecklistTemplateId);

            await LoadTaskTemplatesAsync();

            if (selectedChecklist == null)
            {
                TaskFilter?.SetHasItems();
            }
            else
            {
                selectedChecklist.TaskTemplates ??= new List<TaskTemplateModel>();

                IsSignatureRequired = selectedChecklist?.IsSignatureRequired ?? false;

                TaskHelper.CalculateTaskAmounts(TaskFilter);

                MessagingCenter.Subscribe<ChecklistSlideViewModel>(this, Constants.RecalculateAmountsMessage, viewModel =>
                {
                    TaskFilter.RefreshStatusFilter(false, false);
                    TaskHelper.CalculateTaskAmounts(TaskFilter);
                });

                MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionsChanged, async actionService =>
                {
                    await _taskService.LoadOpenActionCountForTaskTemplatesAsync(TaskFilter.UnfilteredItems).ConfigureAwait(false);
                });

                MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionChanged, async actionService =>
                {
                    await _taskService.LoadOpenActionCountForTaskTemplatesAsync(TaskFilter.UnfilteredItems).ConfigureAwait(false);
                });

                MessagingCenter.Subscribe<TaskCommentEditViewModel, BasicTaskTemplateModel>(this, Constants.TaskTemplateCommentAdded, async (sender, task) =>
                {
                    OpenFields.AddChangedTask(task, false);
                    await OpenFields.SaveLocalChanges(_checklistService).ConfigureAwait(false);
                });

                MessagingCenter.Subscribe<SyncService>(this, Constants.ChecklistTemplateChanged, async (sender) =>
                 {
                     await RefreshAsync().ConfigureAwait(false);
                 });

                MessagingCenter.Subscribe<PictureProofViewModel, BasicTaskTemplateModel>(this, Constants.PictureProofChanged, async (sender, task) =>
                {
                    SelectedTask = task;

                    if (!TaskFilter.StatusFilters.IsNullOrEmpty())
                    {
                        TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
                    }

                    TaskHelper.CalculateTaskAmounts(TaskFilter);
                    await AdaptChanges().ConfigureAwait(false);
                });

                if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
                {
                    MessagingCenter.Subscribe<ChecklistsService, ChecklistModel>(this, Constants.ChecklistAdded, async (sender, checklist) =>
                    {
                        if (selectedChecklist == null)
                            selectedChecklist = await _checklistService.GetChecklistTemplateAsync(ChecklistTemplateId, IsRefreshing).ConfigureAwait(false);

                        if (selectedChecklist.SelectedChecklistId == 0 && selectedChecklist.LocalGuid != checklist.LocalGuid)
                        {
                            if (OpenFields != null)
                            {
                                OpenFields.IsSyncing = false;
                                OpenFields.CalculateTasksDone();
                            }
                            return;
                        }

                        if (selectedChecklist.SelectedChecklistId > 0 && selectedChecklist.SelectedChecklistId != checklist.Id)
                        {
                            if (OpenFields != null)
                            {
                                OpenFields.IsSyncing = false;
                                OpenFields.CalculateTasksDone();
                                return;
                            }
                        }

                        IncompleteChecklist = checklist;
                        if (TaskFilter != null)
                        {
                            foreach (var item in TaskFilter.UnfilteredItems)
                            {
                                var taskFromApi = IncompleteChecklist.Tasks.FirstOrDefault(x => x.TemplateId == item.Id);
                                item.ItemId = taskFromApi?.Id ?? 0;
                                item.PropertyValues = taskFromApi.PropertyUserValues;
                                item.PictureProof = taskFromApi.PictureProof;
                                item.PictureProofMediaItems = null;
                                item.SetPictureProofMediaItems();
                                item.CreatePropertyList();
                                item.HasPictureProof = taskFromApi.HasPictureProof;
                            }
                        }

                        if (IncompleteChecklist.OpenFieldsPropertyUserValues != null)
                        {
                            IOpenTextFields open = OpenFields?.CheckAndUpdateOpenFields(IncompleteChecklist, selectedChecklist);
                            OpenFields?.SetPropertyValues(open);
                        }

                        if (IncompleteChecklist.Stages != null && Stages != null)
                            Stages.UpdateStages(IncompleteChecklist.Stages);

                        OnPropertyChanged(nameof(FirstTimeSaving));
                        OnPropertyChanged(nameof(IncompleteChecklist));
                        MainThread.BeginInvokeOnMainThread(() =>
                       {
                           MessagingCenter.Send(this, Constants.ChecklistAdded, IncompleteChecklist);
                       });
                        if (OpenFields != null)
                        {
                            OpenFields.IsSyncing = false;
                            OpenFields.CalculateTasksDone();
                        }
                    });

                    MessagingCenter.Subscribe<ChecklistsService, string>(this, Constants.ErrorSendingChecklist, async (sender, reason) =>
                    {
                        _messageService.SendClosableWarning($"Error: {reason}");
                        if (OpenFields != null)
                            OpenFields.IsSyncing = false;
                        await UpdateTaskStatuses(IncompleteChecklist?.Id).ConfigureAwait(false);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Send(this, Constants.ErrorSendingChecklist, reason);
                        });
                    });

                    MessagingCenter.Subscribe<Application>(Application.Current, Constants.QuickTimer, async (sender) =>
                    {
                        try
                        {
                            if (await FifteenSecondLock.WaitAsync(0))
                            {
                                await Task.Run(async () =>
                                {
                                    var ids = await _updateService.CheckForUpdatedChecklistsAsync().ConfigureAwait(false);
                                    if (ids.Count > 0 && ids.Contains(IncompleteChecklist?.Id ?? -1))
                                    {
                                        AnyRemoteChanges = true;
                                        LoadStages();
                                        MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.RemoteChanges, AnyRemoteChanges); });
                                    }
                                }).ConfigureAwait(false);
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

                    MessagingCenter.Subscribe<ChecklistSlideViewModel, bool>(this, Constants.RemoteChanges, (sender, anyRemoteChanges) =>
                    {
                        AnyRemoteChanges = anyRemoteChanges;
                    });

                    MessagingCenter.Subscribe<StageSignViewModel>(this, Constants.StageSigned, async (sender) =>
                    {
                        await AdaptChanges(true).ConfigureAwait(false);
                        await SaveChecklistAsync().ConfigureAwait(false);
                    });
                }

                if (ChecklistTemplates != null && ChecklistTemplates.Count > 6)
                    Rect = new Rect(113, .8, .4, .9);

                SetListViewLayout(Settings.AppSettings.ListViewLayout);

                if (!OpenedFromDeepLink)
                {
                    SelectedChecklist = ChecklistTemplates?.FirstOrDefault(x => selectedChecklist.Id == x.Id);
                    if (selectedChecklist.Tags != null)
                        SelectedChecklist.Tags = selectedChecklist.Tags;
                }
            }

            LoadStages();
            OpenFields?.CalculateTasksDone();
            if (selectedChecklist != null)
                selectedChecklist.LocalGuid = Guid.NewGuid();

            await base.Init();
        }

        private void LoadStages()
        {
            bool hasStages = IncompleteChecklist != null ? IncompleteChecklist.HasStages : selectedChecklist?.StageTemplates?.Count > 0;
            if (!hasStages)
                return;

            if (IncompleteChecklist != null)
            {
                IncompleteChecklist.LocalGuid = selectedChecklist.LocalGuid;
                selectedChecklist.SelectedChecklistId = IncompleteChecklist.Id;
            }

            if (IncompleteChecklist != null && IncompleteChecklist.Stages != null)
            {
                foreach (var item in selectedChecklist.StageTemplates)
                {
                    var stageFromApi = IncompleteChecklist.Stages.FirstOrDefault(x => x.StageTemplateId == item.Id);
                    if (stageFromApi != null)
                    {
                        item.Signatures = stageFromApi.Signatures;
                        item.StageId = stageFromApi.Id;
                        item.ShiftNotes = stageFromApi.ShiftNotes;
                        item.BlockNextStagesUntilCompletion = stageFromApi.BlockNextStagesUntilCompletion;
                        item.LockStageAfterCompletion = stageFromApi.LockStageAfterCompletion;
                        item.UseShiftNotes = stageFromApi.UseShiftNotes;
                        item.TaskTemplateIds = stageFromApi.TaskTemplateIds;
                        item.TaskIds = stageFromApi.TaskIds;
                        item.NumberOfSignaturesRequired = stageFromApi.NumberOfSignaturesRequired;
                        item.Name = stageFromApi.Name;
                        item.Description = stageFromApi.Description;
                        item.Tags = stageFromApi.Tags;
                    }
                }
            }
            if (TaskFilter != null)
            {
                Stages = new StagesControl(selectedChecklist.StageTemplates, TaskFilter.UnfilteredItems, TaskFilter.FilteredList);
                Stages.SetStages(TaskFilter.FilteredList);
                TaskFilter.SetStagesControl(Stages);
            }

            if (OpenFields != null)
                OpenFields.StagesControl = Stages;
        }

        private readonly SemaphoreSlim tasksSemaphore = new SemaphoreSlim(1, 1);

        private async Task UpdateTaskStatuses(int? id)
        {
            var acquired = false;

            try
            {
                if (id == null)
                    return;

                var checklistFromApi = await _checklistService.GetIncompleteChecklistAsync(refresh: true, checklistId: IncompleteChecklist.Id).ConfigureAwait(false);

                await tasksSemaphore.WaitAsync();
                acquired = true;

                var allTasks = TaskFilter.UnfilteredItems;

                var tasks = checklistFromApi.Tasks
                    .Join(allTasks, change => change.Id, task => task.ItemId, (change, task) => new
                    {
                        task,
                        change
                    })
                    .ToList();

                if (tasks.Any())
                {
                    foreach (var pair in tasks)
                    {
                        pair.task.FilterStatus = pair.change.TaskStatus;
                        pair.task.Comment = pair.change.Comment;

                        if (pair.change.Signature == null)
                            pair.task.Signature = null;
                        else
                            pair.task.Signature = new SignatureModel
                            {
                                SignedAt = pair.change.Signature.SignedAt,
                                SignedById = pair.change.Signature.SignedById,
                                SignedBy = pair.change.Signature.SignedBy,
                            };

                        if (CompanyFeatures.RequiredProof && pair.change.HasPictureProof)
                        {
                            pair.task.PictureProofMediaItems = null;
                            pair.task.PictureProof = pair.change.PictureProof;
                            pair.task.SetPictureProofMediaItems();
                            pair.task.HasPictureProof = pair.change.HasPictureProof;
                        }

                        if (pair.change.PropertyUserValues != null)
                        {
                            foreach (var value in pair.change.PropertyUserValues)
                                pair.task.AddOrUpdateUserProperty(value);

                            pair.task.RefreshPropertyValueString();
                        }
                    }
                }

                if (checklistFromApi.OpenFieldsPropertyUserValues != null)
                {
                    IOpenTextFields open = OpenFields?.CheckAndUpdateOpenFields(checklistFromApi, selectedChecklist);
                    OpenFields?.SetPropertyValues(open);
                }

                if (checklistFromApi.Stages != null)
                    Stages?.UpdateStages(checklistFromApi.Stages);

                AnyRemoteChanges = false;
                if (OpenFields != null)
                {
                    await OpenFields.AdaptChanges(_checklistService).ConfigureAwait(false);
                    OpenFields.CalculateTasksDone();
                }
                TaskHelper.CalculateTaskAmounts(TaskFilter);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTaskStatuses error: {e}");
            }
            finally
            {
                if (acquired)
                    tasksSemaphore.Release();

                if (OpenFields != null)
                {
                    OpenFields.ClearChangedTasks();
                    OpenFields.IsSyncing = false;
                }
            }
        }

        public override async void OnDisappearing(object sender, EventArgs e)
        {
            if (!IsBusy && OpenFields != null) await OpenFields.AdaptChanges(_checklistService).ConfigureAwait(false);
            base.OnDisappearing(sender, e);
        }

        public override async Task<bool> BeforeNavigatingAway()
        {
            if (!Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
                return true;

            if (!CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
                return true;

            if (!OpenFields.AnyLocalChanges)
                return true;

            IsDiscardChangesPopupOpen = true;
            Task<bool> confirmationTask = WaitForDiscardChangesConfirmation();
            bool confirmed = await confirmationTask;

            IsDiscardChangesPopupOpen = false;
            return confirmed;
        }

        public override async Task SubmitDiscardChangesPopup()
        {
            await SaveChecklistAsync().ConfigureAwait(false);
            await base.SubmitDiscardChangesPopup().ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<ChecklistSlideViewModel>(this, Constants.RecalculateAmountsMessage);
                MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionsChanged);
                MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionChanged);
                MessagingCenter.Unsubscribe<TaskCommentEditViewModel, BasicTaskTemplateModel>(this, Constants.TaskTemplateCommentAdded);
                MessagingCenter.Unsubscribe<SyncService>(this, Constants.ChecklistTemplateChanged);
                MessagingCenter.Unsubscribe<PictureProofViewModel, BasicTaskTemplateModel>(this, Constants.PictureProofChanged);
                if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
                {
                    MessagingCenter.Unsubscribe<ChecklistsService, ChecklistModel>(this, Constants.ChecklistAdded);
                    MessagingCenter.Unsubscribe<ChecklistsService, string>(this, Constants.ErrorSendingChecklist);
                    MessagingCenter.Unsubscribe<Application>(Application.Current, Constants.QuickTimer);
                    MessagingCenter.Unsubscribe<ChecklistSlideViewModel, bool>(this, Constants.RemoteChanges);
                    MessagingCenter.Unsubscribe<StageSignViewModel>(this, Constants.StageSigned);
                }
            });
            OpenFields?.Dispose();
            TaskFilter?.Dispose();
            //_checklistService.Dispose();
            //_propertySerice.Dispose();
            //_taskService.Dispose();
            ChecklistTemplates?.Clear();
            ChecklistTemplates = null;
            OpenFields = null;
            TaskFilter = null;
            selectedChecklist = null;
            SelectedChecklist = null;
            lastTappedActionTask = null;
            base.Dispose(disposing);
        }

        public override bool CanToggleDropdown()
        {
            return IsChecklistSelectorActive && base.CanToggleDropdown();
        }

        /// <summary>
        /// Loads the task templates asynchronous.
        /// </summary>
        private async Task LoadTaskTemplatesAsync()
        {
            if (_checklistService == null)
                return;

            if (!IsFromBookmark)
            {
                selectedChecklist = await _checklistService.GetChecklistTemplateAsync(ChecklistTemplateId, IsRefreshing).ConfigureAwait(false);
            }

            if (IncompleteChecklist != null)
            {
                selectedChecklist.Name = IncompleteChecklist.Name;
                selectedChecklist.Version = IncompleteChecklist.Version;
                selectedChecklist.Tags = IncompleteChecklist.Tags;
                selectedChecklist.OpenFieldsProperties = IncompleteChecklist.OpenFieldsProperties;
                OpenFields.PropertyList = IncompleteChecklist.OpenFieldsPropertyUserValues;
                selectedChecklist.IsSignatureRequired = IncompleteChecklist.IsSignatureRequired;
                selectedChecklist.IsDoubleSignatureRequired = IncompleteChecklist.IsDoubleSignatureRequired;
            }

            if (selectedChecklist != null)
            {
                selectedChecklist.TaskTemplates ??= new List<TaskTemplateModel>();
                TaskFilter ??= new FilterControl<BasicTaskTemplateModel, TaskStatusEnum>(null);
                if (ChecklistTemplates != null)
                {
                    TaskFilter.FilterCollection = ChecklistTemplates?.Select(x => new FilterModel(x.Name, x.Id)).ToList();

                    if (IncompleteChecklist != null)
                        TaskFilter.SelectedFilter = new FilterModel(selectedChecklist.Name, selectedChecklist.Id);
                    else
                        TaskFilter.SetSelectedFilter(filterName: selectedChecklist.Name, id: selectedChecklist.Id);
                }
                else
                    TaskFilter.SelectedFilter = new FilterModel(selectedChecklist.Name, selectedChecklist.Id);

                if (selectedChecklist.TaskTemplates != null)
                {
                    await LoadTaskTemplates().ConfigureAwait(false);
                }
                else
                {
                    TaskFilter.SetHasItems();
                }
                SelectedChecklist = selectedChecklist?.ToBasic() ?? new BasicChecklistTemplateModel();
            }
        }


        private async Task LoadTaskTemplates()
        {
            List<BasicTaskTemplateModel> taskTemplates = selectedChecklist?.TaskTemplates?.Where(t => t != null).ToList().ToBasicList<BasicTaskTemplateModel, TaskTemplateModel>() ?? new List<BasicTaskTemplateModel>();
            LocalTemplateModel localChecklistTemplate = null;

            if (IncompleteChecklist != null)
            {
                var matchingTemplates = selectedChecklist?.TaskTemplates?
                    .Where(t => IncompleteChecklist.Tasks.Any(x => x.TemplateId == t.Id))
                    .ToList() ?? new List<TaskTemplateModel>();

                taskTemplates = matchingTemplates.ToBasicList<BasicTaskTemplateModel, TaskTemplateModel>();

                var allComments = await _commentService.GetAllAsync(refresh: false, true, true).ConfigureAwait(false);
                foreach (var basicTask in taskTemplates)
                {
                    var incompleteTask = IncompleteChecklist.Tasks.FirstOrDefault(x => x.TemplateId == basicTask.Id);
                    if (incompleteTask != null)
                    {
                        var comments = await _commentService.GetCommentsForTaskAsync((int)incompleteTask.Id, refresh: false, allComments: allComments).ConfigureAwait(false);

                        basicTask.FilterStatus = incompleteTask.FilterStatus;
                        basicTask.Name = incompleteTask.Name;
                        basicTask.Description = incompleteTask.Description;
                        basicTask.Picture = incompleteTask.Picture;
                        basicTask.PropertyValues = incompleteTask.PropertyUserValues ?? new List<PropertyUserValue>();
                        basicTask.LocalComments = comments ?? new List<Models.Comments.CommentModel>();
                        basicTask.PictureProof = incompleteTask.PictureProof;
                        basicTask.Tags = incompleteTask.Tags;
                        basicTask.SetPictureProofMediaItems();
                        basicTask.HasPictureProof = incompleteTask.HasPictureProof;
                        basicTask.ItemId = incompleteTask.Id;
                        basicTask.WorkInstructionRelations = incompleteTask.WorkInstructionRelations;
                        basicTask.Attachments = incompleteTask.Attachments;
                        basicTask.Steps = incompleteTask.Steps;
                        if (incompleteTask.Signature != null)
                        {
                            basicTask.Signature = new SignatureModel()
                            {
                                SignatureImage = incompleteTask.Signature.SignatureImage,
                                SignedAt = incompleteTask.Signature.SignedAt,
                                SignedBy = incompleteTask.Signature.SignedBy,
                                SignedById = incompleteTask.Signature.SignedById,
                            };
                        }
                    }
                }
            }
            else if (!ShouldClearStatuses)
            {
                List<LocalTemplateModel> localChecklistTemplates = await _checklistService.GetLocalChecklistTemplatesAsync().ConfigureAwait(false);

                localChecklistTemplate = localChecklistTemplates?.SingleOrDefault(item => item.Id == ChecklistTemplateId && item.UserId == UserSettings.Id);

                if (localChecklistTemplate != null)
                {
                    List<LocalTaskTemplateModel> localTaskTemplates = localChecklistTemplate.TaskTemplates ?? new List<LocalTaskTemplateModel>();

                    foreach (BasicTaskTemplateModel basicTask in taskTemplates)
                    {
                        LocalTaskTemplateModel localTaskTemplate = localTaskTemplates.SingleOrDefault(item => item.Id == basicTask.Id);

                        if (localTaskTemplate != null)
                        {
                            basicTask.FilterStatus = localTaskTemplate.Status ?? TaskStatusEnum.Todo;
                            basicTask.PropertyValues = localTaskTemplate.PropertyUserValues ?? new List<PropertyUserValue>();
                            basicTask.LocalComments = localTaskTemplate.Comments ?? new List<Models.Comments.CommentModel>();
                            basicTask.HasPictureProof = localTaskTemplate.HasPictureProof;
                            basicTask.PictureProofMediaItems = localTaskTemplate.PictureProofMediaItems;
                            basicTask.Signature = localTaskTemplate.Signature;
                        }
                    }

                    selectedChecklist.StartedAt = localChecklistTemplate.StartedAt;
                }
            }

            await _taskService.LoadOpenActionCountForTaskTemplatesAsync(taskTemplates, IsRefreshing).ConfigureAwait(false);
            await _propertySerice.LoadTaskTemplatesPropertiesAsync(taskTemplates, refresh: IsRefreshing).ConfigureAwait(false);

            taskTemplates?.ForEach(x => x?.UpdateActionBubbleCount());
            TaskFilter?.SetUnfilteredItems(taskTemplates ?? new List<BasicTaskTemplateModel>());

            if (OpenFields != null)
                OpenFields.TaskTemplates = taskTemplates;

            if (selectedChecklist?.OpenFieldsProperties != null)
            {
                IOpenTextFields template = IncompleteChecklist != null ? IncompleteChecklist : localChecklistTemplate;

                IOpenTextFields open = OpenFields?.CheckAndUpdateOpenFields(template, selectedChecklist);
                OpenFields?.SetPropertyValues(open, shouldClearPropertyIds: ShouldClearStatuses);
            }

            if (TaskFilter?.StatusFilters != null)
            {
                TaskFilter.Filter(TaskFilter.StatusFilters, resetIfTheSame: false, useDataSource: false);
            }
        }

        /// <summary>
        /// Toggles the task status.
        /// </summary>
        /// <param name="taskTemplate">The task template.</param>
        /// <param name="status">The status.</param>
        private async Task ToggleTaskStatus(BasicTaskTemplateModel taskTemplate, TaskStatusEnum status)
        {
            if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled && Stages.HasStages)
            {
                if (OpenFields.IsSyncing)
                {
                    OpenFields.SendSyncingInProgressClosableWarning();
                    return;
                }

                if (Stages.IsTaskLocked(taskTemplate))
                {
                    if (taskTemplate.FilterStatus == status && taskTemplate.PictureProofMediaItems?.Count > 0 && (taskTemplate.FilterStatus == TaskStatusEnum.NotOk || taskTemplate.FilterStatus == TaskStatusEnum.Ok))
                    {
                        CurrentStatus = status;
                        SelectedTask = taskTemplate;
                        await NavigateToPictureProofDetails(false);
                        return;
                    }

                    Stages.SendClosableWarning(taskTemplate);
                    return;
                }
            }


            if ((status == TaskStatusEnum.Ok || status == TaskStatusEnum.NotOk) && !await Validate(taskTemplate).ConfigureAwait(false))
                return;

            CurrentStatus = status;
            SelectedTask = taskTemplate;

            //picture proof
            if (taskTemplate.HasPictureProof)
            {
                if (status == TaskStatusEnum.Skipped)
                {
                    if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped)
                        await UntapTaskAsync();
                    else
                        OpenSkipTaskPopup();
                }
                else if (taskTemplate.FilterStatus == TaskStatusEnum.Todo || taskTemplate.FilterStatus == TaskStatusEnum.Skipped)
                {
                    CurrentStatus = null;

                    using var scope = App.Container.CreateScope();
                    var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
                    pictureProofViewModel.SelectedTaskTemplate = taskTemplate;
                    pictureProofViewModel.TaskStatus = status;
                    pictureProofViewModel.IsNew = true;
                    await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
                }
                else
                {
                    if (taskTemplate.FilterStatus != status)
                        OpenChangeStatusPopup();
                    else
                    {
                        await OpenUntapTaskDialogAsync();
                        CurrentStatus = null;
                    }
                }
            }
            else
            {
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

                if (!TaskFilter.StatusFilters.IsNullOrEmpty())
                {
                    TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
                }

                TaskHelper.CalculateTaskAmounts(TaskFilter);

                await AdaptChanges();
            }

            if (taskTemplate.FilterStatus == TaskStatusEnum.Skipped)
                taskTemplate.ResetValidation();
        }

        private async Task<bool> Validate(BasicTaskTemplateModel task)
        {
            if (task.Validate())
                return true;

            await NavigateToDetailCarouselAsync(task, true);
            return false;
        }

        private async Task NavigateToPictureProofDetails(bool supportsEditing = true, bool shouldChangeTaskStatus = true)
        {
            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.SelectedTaskTemplate = SelectedTask;
            pictureProofViewModel.TaskStatus = shouldChangeTaskStatus ? CurrentStatus.Value : null;
            pictureProofViewModel.MainMediaElement = SelectedTask.PictureProofMediaItems?.FirstOrDefault();

            if (SelectedTask.PictureProofMediaItems?.Count > 1)
                pictureProofViewModel.MediaElements = new System.Collections.ObjectModel.ObservableCollection<MediaItem>(SelectedTask.PictureProofMediaItems?.Skip(1));

            pictureProofViewModel.IsNew = false;
            pictureProofViewModel.SupportsEditing = supportsEditing;
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
        }

        public async override Task SeePicturesAsync()
        {
            await NavigateToPictureProofDetails(shouldChangeTaskStatus: false);
            await base.SeePicturesAsync();
        }

        public async override Task UntapTaskAsync()
        {
            SelectedTask.FilterStatus = TaskStatusEnum.Todo;
            SelectedTask.PictureProofMediaItems = new List<MediaItem>();
            await AdaptChanges().ConfigureAwait(false);
            TaskHelper.CalculateTaskAmounts(TaskFilter);
            await base.UntapTaskAsync();
        }

        private void SetSelectedTaskStatus(TaskStatusEnum status)
        {
            TaskHelper.SetTaskStatusAsync(SelectedTask, status, TaskFilter);

            CurrentStatus = null;
        }

        private async Task AdaptChanges(bool isFromStageSign = false)
        {
            if (OpenFields == null) return;

            if (isFromStageSign)
            {
                OpenFields.AddChangedStageSign();
                OpenFields.CalculateTasksDone();
                return;
            }

            OpenFields.AddChangedTask(SelectedTask);
            OpenFields.AnyTaskChanges = true;
            await OpenFields.AdaptChanges(_checklistService).ConfigureAwait(false);
        }

        public override async Task SubmitSkipCommandAsync()
        {
            SelectedTask.PictureProofMediaItems = new List<MediaItem>();
            SetSelectedTaskStatus(TaskStatusEnum.Skipped);

            if (!TaskFilter.StatusFilters.IsNullOrEmpty())
            {
                TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
            }

            await AdaptChanges().ConfigureAwait(false);
            await base.SubmitSkipCommandAsync().ConfigureAwait(false);
        }

        public async override Task KeepButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                SetSelectedTaskStatus(CurrentStatus.Value);
                await AdaptChanges().ConfigureAwait(false);
            }
            await base.KeepButtonChangeStatusPopupCommandAsync().ConfigureAwait(false);
        }

        public async override Task RemoveButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                await NavigateToPictureProofDetails();
            }
        }

        public async override Task CancelAsync()
        {
            OnlineShiftCheck.IsShiftChangeAllowed = true;

            await OnlineShiftCheck.CheckCycleChange().ConfigureAwait(false);

            await base.CancelAsync();
        }


        /// <summary>
        /// Sets the ListView layout.
        /// </summary>
        /// <param name="listViewLayout">The list view layout.</param>
        private void SetListViewLayout(object obj)
        {
            if (obj is ListViewLayout listViewLayout)
            {
                if (listViewLayout == ListViewLayout) return;

                if (listViewLayout == ListViewLayout.Grid)
                    IsListVisible = false;
                else
                    IsListVisible = true;

                ListViewLayout = listViewLayout;
                Settings.AppSettings.ListViewLayout = listViewLayout;
            }
        }

        /// <summary>
        /// Navigates to detail carousel asynchronous.
        /// </summary>
        /// <param name="obj">The object.</param>
        private async Task NavigateToDetailCarouselAsync(object obj, bool validate = false)
        {
            BasicTaskTemplateModel TappedTask = null;

            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is BasicTaskTemplateModel task)
                TappedTask = task;

            if (obj is BasicTaskTemplateModel taskModel)
                TappedTask = taskModel;

            if (TappedTask != null)
            {
                int selectedTaskIndex;

                if (TappedTask != null)
                {
                    selectedTaskIndex = TaskFilter.FilteredList.IndexOf(TappedTask);
                    if (Stages.HasStages)
                    {
                        var stagesList = Stages.GetAllFilteredTaskTemplates();
                        selectedTaskIndex = stagesList.IndexOf(TappedTask);
                    }

                    if (!OpenFields?.PropertyList.IsNullOrEmpty() ?? true && TaskFilter.StatusFilters != null)
                        selectedTaskIndex += 1;
                }
                else
                {
                    selectedTaskIndex = -1;
                }

                using var scope = App.Container.CreateScope();
                var checklistSlideViewModel = scope.ServiceProvider.GetService<ChecklistSlideViewModel>();

                checklistSlideViewModel.ChecklistTemplateId = ChecklistTemplateId;
                checklistSlideViewModel.TaskFilter = TaskFilter;
                checklistSlideViewModel.ChecklistTemplate = selectedChecklist;
                checklistSlideViewModel.ChecklistOpenFields = OpenFields;
                checklistSlideViewModel.TaskFromDeepLink = TaskFromDeepLink;
                checklistSlideViewModel.DeepLinkCompletionIsRequired = DeepLinkCompletionIsRequired;
                checklistSlideViewModel.IncompleteChecklist = IncompleteChecklist;
                checklistSlideViewModel.AnyRemoteChanges = AnyRemoteChanges;
                checklistSlideViewModel.CurrentIndex = selectedTaskIndex;
                checklistSlideViewModel.Stages = Stages;

                await checklistSlideViewModel.LoadSlideItems();


                if (PagesFromDeepLink > 0)
                    checklistSlideViewModel.PagesFromDeepLink = PagesFromDeepLink + 1;

                if (validate)
                    checklistSlideViewModel.SelectedTask?.Validate();

                await NavigationService.NavigateAsync(viewModel: checklistSlideViewModel);
            }
        }

        /// <summary>
        /// Handles a tap on the dropdown.
        /// </summary>
        /// <param name="obj">The object.</param>
        private async void DropdownTapAsync(object obj)
        {
            IsDropdownOpen = false;
            if ((obj as Syncfusion.TreeView.Engine.TreeViewNode).Content is FilterModel model)
            {
                if (ChecklistTemplateId != model.Id)
                {
                    await OpenFields.AdaptChanges(_checklistService).ConfigureAwait(false);

                    ChecklistTemplateId = (int)model.Id;
                    OpenFields.ChecklistTemplateId = ChecklistTemplateId;

                    SelectedChecklist = ChecklistTemplates.FirstOrDefault(x => x.Id == ChecklistTemplateId);

                    await LoadTaskTemplatesAsync().ConfigureAwait(false);

                    selectedChecklist.TaskTemplates ??= new List<TaskTemplateModel>();

                    IsSignatureRequired = selectedChecklist?.IsSignatureRequired ?? false;

                    TaskHelper.CalculateTaskAmounts(TaskFilter);

                    OpenFields.ChecklistTemplateId = SelectedChecklist.Id;
                }
            }
        }

        private async Task NavigateToSignPageOrFinishChecklistAsync()
        {
            if (OpenFields.TasksDone)
            {
                if (IsSignatureRequired)
                {
                    using var scope = App.Container.CreateScope();
                    var checklistSignViewModel = scope.ServiceProvider.GetService<ChecklistSignViewModel>();
                    checklistSignViewModel.ChecklistTemplateId = ChecklistTemplateId;
                    checklistSignViewModel.TaskTemplates = TaskFilter.UnfilteredItems;
                    checklistSignViewModel.OpenFieldsValues = OpenFields.PropertyList;
                    checklistSignViewModel.TaskFromDeepLink = TaskFromDeepLink;
                    checklistSignViewModel.StartedAt = OpenFields.StartedAt;
                    checklistSignViewModel.DeepLinkCompletionIsRequired = DeepLinkCompletionIsRequired;
                    checklistSignViewModel.IncompleteChecklistId = IncompleteChecklist?.Id ?? 0;
                    checklistSignViewModel.IncompleteChecklistLocalGuid = IncompleteChecklist?.LocalGuid ?? selectedChecklist.LocalGuid;
                    checklistSignViewModel.Stages = Stages.StageTemplates;
                    if (IncompleteChecklist != null)
                        checklistSignViewModel.SelectedChecklist = selectedChecklist;

                    // When incomplete checklist, send only changed tasks and don't modify started at field
                    if (IncompleteChecklist?.Id > 0)
                    {
                        checklistSignViewModel.TaskTemplates = OpenFields.GetChangedTasks();
                        checklistSignViewModel.StartedAt = IncompleteChecklist.CreatedAt;
                        checklistSignViewModel.Stages = new List<Models.Stages.StageTemplateModel>(Stages.GetChangedStage());
                    }

                    if (PagesFromDeepLink > 0)
                        checklistSignViewModel.PagesFromDeepLink = PagesFromDeepLink + 1;

                    await NavigationService.NavigateAsync(viewModel: checklistSignViewModel);
                }
                else
                {
                    IsBusy = true;

                    var model = new PostTemplateModel(ChecklistTemplateId, selectedChecklist.Name, OpenFields.PropertyList, TaskFilter.UnfilteredItems, DeepLinkCompletionIsRequired, TaskFromDeepLink?.Id, true, IncompleteChecklist?.Id ?? 0, Stages.StageTemplates, selectedChecklist.Version);
                    model.StartedAt = OpenFields.StartedAt;
                    model.LocalGuid = IncompleteChecklist?.LocalGuid ?? selectedChecklist.LocalGuid;

                    // When incomplete checklist, send only changed tasks and don't modify started at field
                    if (IncompleteChecklist?.Id > 0)
                    {
                        model.Tasks = OpenFields.GetChangedTasks();
                        model.StartedAt = IncompleteChecklist.CreatedAt;
                        model.Stages = new List<Models.Stages.StageTemplateModel>(Stages.GetChangedStage());
                        model.CreatedBy = IncompleteChecklist.CreatedBy;
                        model.CreatedById = IncompleteChecklist.CreatedById;
                    }

                    if (PagesFromDeepLink > 0)
                    {
                        var shiftChanged = await ShiftChanged.PerformChangeAsync();

                        if (!shiftChanged)
                            OnlineShiftCheck.IsShiftChangeAllowed = true;

                        await NavigationService.RemoveLastPagesAsync(PagesFromDeepLink);

                        if (TaskFromDeepLink != null)
                        {
                            using var scope = App.Container.CreateScope();
                            var messageService = scope.ServiceProvider.GetService<IMessageService>();
                            messageService.SendLinkedItemSignedMessage(TaskFromDeepLink);
                        }
                    }
                    else
                        await NavigationService.PopOrNavigateToPage<ChecklistTemplatesViewModel>(typeof(ChecklistTemplatesViewModel));

                    await _checklistService.PostChecklistAsync(model).ConfigureAwait(false);
                }
            }

            if (OpenFields != null && NavigationService != null)
                OpenFields.ShowInfoDialog(NavigationService.GetCurrentPage());
        }

        private async Task SaveChecklistAsync()
        {
            if (OpenFields == null)
            {
                _messageService?.SendClosableInfo("No checklist opened. Save skipped.");
                return;
            }

            if (OpenFields.IsSyncing)
                return;

            OpenFields.IsSyncing = true;
            IsBusy = true;
            if (OpenFields.AnyLocalChanges)
            {
                var model = new PostTemplateModel(ChecklistTemplateId, selectedChecklist.Name, OpenFields.PropertyList, TaskFilter.UnfilteredItems, DeepLinkCompletionIsRequired, TaskFromDeepLink?.Id, false, IncompleteChecklist?.Id ?? 0, Stages.StageTemplates, selectedChecklist.Version);
                model.StartedAt = OpenFields.StartedAt;
                model.LocalGuid = IncompleteChecklist?.LocalGuid ?? selectedChecklist.LocalGuid;

                // When incomplete checklist, send only changed tasks and don't modify started at field and signatures
                if (IncompleteChecklist?.Id > 0)
                {
                    model.Tasks = new List<BasicTaskTemplateModel>(OpenFields.GetChangedTasks());
                    model.StartedAt = IncompleteChecklist.CreatedAt;
                    model.LinkedTaskId = null;
                    model.IsRequiredForLinkedTask = null;
                    model.Stages = new List<Models.Stages.StageTemplateModel>(Stages.GetChangedStage());
                    model.CreatedBy = IncompleteChecklist.CreatedBy;
                    model.CreatedById = IncompleteChecklist.CreatedById;
                }

                await _checklistService.PostChecklistAsync(model).ConfigureAwait(false);
            }


            if (IncompleteChecklist?.Id > 0 && AnyRemoteChanges)
            {
                AnyRemoteChanges = false;
                OpenFields.ClearChangedTasks();
                await UpdateTaskStatuses(IncompleteChecklist?.Id).ConfigureAwait(false);
            }

            IsBusy = false;
            OpenFields?.ClearChangedTasks();
            Stages?.ClearChangedStages();
        }

        private async Task OpenPopupOrNavigateToActionsAsync(BasicTaskTemplateModel taskTemplate)
        {
            //Uncomment this code in case business logic changes about locking actions in locked stage
            // if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled && Stages.HasStages)
            // {
            //     if (Stages.IsTaskLocked(taskTemplate))
            //     {
            //         Stages.SendClosableWarning(taskTemplate);
            //         return;
            //     }
            // }

            if (taskTemplate.ActionBubbleCount > 0)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskTemplateId = taskTemplate.Id;
                actionOpenActionsViewModel.ActionType = ActionType.Checklist;
                actionOpenActionsViewModel.LocalTask = taskTemplate;
                actionOpenActionsViewModel.TaskTitle = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forChecklistItem)} {taskTemplate.Name}";

                await NavigationService.NavigateAsync(viewModel: actionOpenActionsViewModel);
            }
            else
            {
                lastTappedActionTask = taskTemplate;
                IsActionPopupOpen = !IsActionPopupOpen;
            }
        }

        private async Task NavigateToNewCommentAsync()
        {
            using var scope = App.Container.CreateScope();
            var taskCommentEditViewModel = scope.ServiceProvider.GetService<TaskCommentEditViewModel>();
            taskCommentEditViewModel.TaskTemplateId = lastTappedActionTask.Id;
            taskCommentEditViewModel.IsNew = true;
            taskCommentEditViewModel.LocalTask = lastTappedActionTask;
            taskCommentEditViewModel.Type = ActionType.Checklist;
            taskCommentEditViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forChecklistItem)} {lastTappedActionTask.Name}";

            await NavigationService.NavigateAsync(viewModel: taskCommentEditViewModel);
        }

        private async Task NavigateToNewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.TaskTemplateId = lastTappedActionTask.Id;
            actionNewViewModel.ActionType = ActionType.Checklist;
            actionNewViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forChecklistItem)} {lastTappedActionTask.Name}";
            actionNewViewModel.LocalTask = lastTappedActionTask;

            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }

        private async Task NavigateToMoreInfoAsync(object obj)
        {
            if (obj is BasicTaskTemplateModel taskTemplate)
            {
                using var scope = App.Container.CreateScope();
                if (taskTemplate.HasDocument)
                {
                    var pdfViewerViewModel = scope.ServiceProvider.GetService<PdfViewerViewModel>();
                    pdfViewerViewModel.DocumentUri = taskTemplate.DescriptionFile;
                    pdfViewerViewModel.Title = taskTemplate.Name;

                    await NavigationService.NavigateAsync(viewModel: pdfViewerViewModel);
                }
                else if (taskTemplate.HasSteps)
                {
                    var stepsViewModel = scope.ServiceProvider.GetService<StepsViewModel>();
                    stepsViewModel.Steps = taskTemplate.Steps;
                    stepsViewModel.Name = taskTemplate.Name;

                    await NavigationService.NavigateAsync(viewModel: stepsViewModel);
                }
            }
        }

        private async Task NavigateToSignPageOrSaveStage(BasicTaskTemplateModel taskTemplate)
        {
            if (OpenFields?.IsSyncing ?? false)
                return;

            var stage = Stages?.GetStageTemplate(taskTemplate.StageTemplateId);
            if (stage == null)
                return;

            if (stage.IsSigned && (stage.NumberOfSignaturesRequired > 0 || !stage.ShiftNotes.IsNullOrEmpty()))
            {
                await NavigateToSaveStage(stage, false);
                return;
            }

            if (!Stages?.CanSaveStage(taskTemplate.StageTemplateId) ?? false)
                return;

            if (stage.NumberOfSignaturesRequired == 0 && !stage.UseShiftNotes)
            {
                var saved = Stages?.SaveStage(taskTemplate.StageTemplateId) ?? false;

                if (saved)
                {
                    await AdaptChanges(true).ConfigureAwait(false);
                    await SaveChecklistAsync().ConfigureAwait(false);
                }
                return;
            }

            await NavigateToSaveStage(stage);
        }

        private async Task NavigateToSaveStage(StageTemplateModel stage, bool canEdit = true)
        {
            using var scope = App.Container.CreateScope();
            var stageSignViewModel = scope.ServiceProvider.GetService<StageSignViewModel>();
            stageSignViewModel.Stage = stage;
            stageSignViewModel.StagesControl = Stages;
            stageSignViewModel.CanEdit = canEdit;

            await NavigationService.NavigateAsync(viewModel: stageSignViewModel);
        }

    }
}
