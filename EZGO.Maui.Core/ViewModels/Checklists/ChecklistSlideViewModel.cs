using Autofac;
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
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Checklists;
using EZGO.Maui.Core.ViewModels.Checklists.Commands;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using MvvmHelpers.Interfaces;
using PropertyChanged;
using Syncfusion.Maui.DataSource.Extensions;
using System.Diagnostics;
using System.Windows.Input;


namespace EZGO.Maui.Core.ViewModels
{
    public class ChecklistSlideViewModel : BasicTaskViewModel, IHasPopup, IHasTaskPropertiesEditViewModel
    {
        private BasicTaskTemplateModel openFieldsButton;
        private BasicTaskTemplateModel submitButton;
        private bool _reloadFilteredList = true;

        #region Public Properties

        public int ChecklistTemplateId { get; set; }

        public BasicTaskTemplateModel SelectedTask { get; set; }

        public string Pager { get; set; }

        public bool HasButtons { get; set; }

        public int CurrentIndex { get; set; }

        [DoNotNotify]
        public int PagesFromDeepLink { get; set; }

        public bool IsSignatureRequired { get; set; }

        public bool IsBusy { get; set; }

        public bool IsEnabled { get; set; } = false;

        public ChecklistTemplateModel ChecklistTemplate { get; set; }

        public bool IsPopupOpen { get; set; }

        public BasicTaskModel TaskFromDeepLink { get; set; }

        /// <summary>
        /// Indicates if the action/comment popup is open
        /// </summary>
        public bool IsActionPopupOpen { get; set; }

        public BaseTaskPropertyEditViewModel PropertyEditViewModel { get; set; }

        public ChecklistOpenFields ChecklistOpenFields { get; set; }

        public FilterControl<BasicTaskTemplateModel, TaskStatusEnum> TaskFilter { get; set; }

        public ScoreTypeEnum ScoreType { get; set; } = ScoreTypeEnum.Thumbs;

        public int Score { get; set; }

        public bool DeepLinkCompletionIsRequired { get; set; } = false;

        public ChecklistModel IncompleteChecklist { get; set; }

        public bool AnyRemoteChanges { get; set; }

        public bool FirstTimeSaving => IncompleteChecklist == null;

        public StagesControl Stages { get; set; }

        public ICommand SignStageCommand { get; private set; }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the task skipped command.
        /// </summary>
        /// <value>
        /// The task skipped command.
        /// </value>
        public ICommand TaskSkippedCommand => new Command(() => ExecuteLoadingAction(async () => await ToggleTaskStatus(TaskStatusEnum.Skipped)), CanExecuteCommands);

        /// <summary>
        /// Gets the task not ok command.
        /// </summary>
        /// <value>
        /// The task not ok command.
        /// </value>
        public ICommand TaskNotOkCommand => new Command(() => ExecuteLoadingAction(async () => await ToggleTaskStatus(TaskStatusEnum.NotOk)), CanExecuteCommands);

        /// <summary>
        /// Gets the task ok command.
        /// </summary>
        /// <value>
        /// The task ok command.
        /// </value>
        public ICommand TaskOkCommand => new Command(() => ExecuteLoadingAction(async () => await ToggleTaskStatus(TaskStatusEnum.Ok)), CanExecuteCommands);

        public ICommand SelectionChangedCommand => new Command(() =>
        {
            //ExecuteLoadingAction(OnSelectedTaskChanged);
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the filter command.
        /// </summary>
        /// <value>
        /// The filter command.
        /// </value>      
        public ICommand FilterCommand => new FilterCommand(this);

        public ICommand SignCommand => new Command(() => ExecuteLoadingAction(NavigateToSignPageOrFinishChecklistAsync), CanExecuteCommands);

        public ICommand DetailCommand => new Command<BasicTaskTemplateModel>((task) => ExecuteLoadingAction(async () => await NavigateToDetailAsync(task)), CanExecuteCommands);

        public ICommand StepsCommand => new Command(() => ExecuteLoadingAction(NavigateToMoreInfoAsync), CanExecuteCommands);

        public ICommand ActionCommand => new Command(() => ExecuteLoadingAction(OpenPopupOrNavigateToActionsAsync), CanExecuteCommands);

        public ICommand NavigateToNewActionCommand => new Command(() => ExecuteLoadingAction(NavigateToNewActionAsync), CanExecuteCommands);

        public ICommand NavigateToNewCommentCommand => new Command(() => ExecuteLoadingAction(NavigateToNewCommentAsync), CanExecuteCommands);

        public ICommand OpenPopupCommand => new Command<BasicTaskPropertyModel>((prop) => ExecuteLoadingAction(() => OpenFeaturePopup(prop)), CanExecuteCommands);

        public ICommand SubmitPopupCommand => new Command(() => ExecuteLoadingAction(SubmitPopupAsync), CanExecuteCommands);

        public ICommand ClosePopupCommand => new Command(CloseFeaturePopup);

        public IAsyncCommand SaveCommand => new MvvmHelpers.Commands.AsyncCommand(SaveChecklistAsync);

        protected override void RefreshCanExecute()
        {
            (TaskSkippedCommand as Command)?.ChangeCanExecute();
            (TaskNotOkCommand as Command)?.ChangeCanExecute();
            (TaskOkCommand as Command)?.ChangeCanExecute();
            (DetailCommand as Command)?.ChangeCanExecute();
            (SignCommand as Command)?.ChangeCanExecute();
            (ActionCommand as Command)?.ChangeCanExecute();
            (NavigateToNewActionCommand as Command)?.ChangeCanExecute();
            (NavigateToNewCommentCommand as Command)?.ChangeCanExecute();
            (OpenPopupCommand as Command)?.ChangeCanExecute();
            (SubmitPopupCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        #endregion

        #region Construction

        private readonly IChecklistService _checklistService;
        private readonly ITasksService _taskService;

        public ChecklistSlideViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IChecklistService checklistService,
            ITasksService tasksService) : base(navigationService, userService, messageService, actionsService)
        {
            _checklistService = checklistService;
            _taskService = tasksService;


            SignStageCommand = new Command<BasicTaskTemplateModel>(async (taskTemplate) =>
            {
                await NavigateToSignPageOrSaveStage(taskTemplate);
            });
        }

        public override async Task Init()
        {
            _reloadFilteredList = false;
            LoadChecklist();

            IsSignatureRequired = ChecklistTemplate.IsSignatureRequired;

            MessagingCenter.Subscribe<string, IDetailItem>(this, Constants.UpdateSlideIndex, (senderClassName, selectedTask) =>
            {
                if (senderClassName != nameof(ChecklistSlideViewModel))
                    return;

                var task = selectedTask as BasicTaskTemplateModel;

                var foundTask = TaskFilter.FilteredSlideList.FirstOrDefault(x => x.Id == task.Id) ?? TaskFilter.FilteredSlideList.FirstOrDefault();
                var taskIndex = TaskFilter.FilteredSlideList.IndexOf(foundTask);
                CurrentIndex = taskIndex;
                OnSelectedTaskChanged();
            });

            MessagingCenter.Subscribe<PictureProofViewModel>(this, Constants.PictureProofChanged, async (_) =>
            {
                await AdaptChanges().ConfigureAwait(false);
            });

            if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
            {
                MessagingCenter.Subscribe<TaskTemplatesViewModel, bool>(Application.Current, Constants.RemoteChanges, (sender, anyRemoteChanges) =>
                {
                    AnyRemoteChanges = anyRemoteChanges;
                });

                MessagingCenter.Subscribe<TaskTemplatesViewModel, ChecklistModel>(this, Constants.ChecklistAdded, (sender, incompleteChecklist) =>
                {
                    IncompleteChecklist = incompleteChecklist;
                    ChecklistOpenFields.IsSyncing = false;
                    ChecklistOpenFields.CalculateTasksDone();
                    OnPropertyChanged(nameof(FirstTimeSaving));
                    OnPropertyChanged(nameof(IncompleteChecklist));
                });

                MessagingCenter.Subscribe<TaskTemplatesViewModel, string>(this, Constants.ErrorSendingChecklist, async (sender, reason) =>
                {
                    ChecklistOpenFields.IsSyncing = false;
                    ChecklistOpenFields.CalculateTasksDone();
                    OnPropertyChanged(nameof(FirstTimeSaving));
                    OnPropertyChanged(nameof(IncompleteChecklist));
                });
            }

            await base.Init();
            _reloadFilteredList = true;
        }

        public async Task LoadSlideItems()
        {
            if (!TaskFilter.StatusFilters.IsNullOrEmpty())
                await ApplyFilter(TaskFilter.StatusFilters, false);
            else if (TaskFilter.StatusFilter != null)
                await ApplyFilter(TaskFilter.StatusFilter, false);
            else
                LoadTemplates();
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<string, IDetailItem>(this, Constants.UpdateSlideIndex);
                MessagingCenter.Unsubscribe<PictureProofViewModel>(this, Constants.PictureProofChanged);
                if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
                {
                    MessagingCenter.Unsubscribe<TaskTemplatesViewModel, bool>(Application.Current, Constants.RemoteChanges);
                    MessagingCenter.Unsubscribe<TaskTemplatesViewModel, ChecklistModel>(this, Constants.ChecklistAdded);
                    MessagingCenter.Unsubscribe<TaskTemplatesViewModel, string>(this, Constants.ErrorSendingChecklist);
                }
            });
            _checklistService.Dispose();
            _taskService.Dispose();
            openFieldsButton = null;
            submitButton = null;
            SelectedTask = null;
            ChecklistTemplate = null;
            PropertyEditViewModel = null;
            base.Dispose(disposing);
        }

        public async Task<bool> BeforeNavigatingToInstructions()
        {
            if (!Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
                return true;

            if (!CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled)
                return true;

            if (!ChecklistOpenFields.AnyLocalChanges)
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

        public override async void OnDisappearing(object sender, EventArgs e)
        {
            if (!IsBusy) await ChecklistOpenFields.AdaptChanges(_checklistService);
            base.OnDisappearing(sender, e);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called when <see cref="SelectedTask"/> is changed.
        /// </summary>
        /// <remarks>IDE marks this method as unused because the call to this method is injected by Fody at compile time.</remarks>
        public void OnSelectedTaskChanged()
        {
            TaskFilter?.FilteredSlideList?.ForEach(task =>
            {
                if (task != null && task != SelectedTask)
                {
                    task.MediaSource = null;
                    task.IsSelected = false;
                }
            });

            if (SelectedTask != null)
            {
                SelectedTask.IsSelected = true;
                SelectedTask.UpdateActionBubbleCount();
                Stages.SetCurrentStage(SelectedTask.StageTemplateId);
                Stages.SetCurrentStageHeaderVisibility(SelectedTask != openFieldsButton);
            }

            SetPager();
            ToggleButtons();
            SetSelection();
        }

        private void SetSelection()
        {
            if (CurrentIndex < 0 || CurrentIndex >= TaskFilter?.FilteredSlideList?.Count()) return;

            SelectedTask = TaskFilter?.FilteredSlideList?.ElementAt(CurrentIndex);
        }

        public override Task CancelAsync()
        {
            RemoveAdditionalSlides();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(this, Constants.RecalculateAmountsMessage);
                MessagingCenter.Send(this, Constants.RemoteChanges, AnyRemoteChanges);
            });


            Stages.SetCurrentStageHeaderVisibility(true);

            TaskHelper.CalculateTaskAmounts(TaskFilter);

            return base.CancelAsync();
        }

        #endregion

        #region Private Methods

        protected async override Task RefreshAsync()
        {
            var result = await Task.Run(async () => await _checklistService.GetChecklistTemplateAsync(ChecklistTemplateId));

            var taskTemplates = result.TaskTemplates.ToBasicList<BasicTaskTemplateModel, TaskTemplateModel>();

            TaskFilter.SetUnfilteredItems(taskTemplates);
            LoadTemplates();

            SelectedTask = TaskFilter.FilteredSlideList.FirstOrDefault(x => x.Id == SelectedTask.Id);
            SelectedTask.IsSelected = true;
        }

        private void LoadChecklist()
        {
            LinkedList<BasicTaskTemplateModel> taskTemplates = new LinkedList<BasicTaskTemplateModel>(TaskFilter.FilteredSlideList);

            taskTemplates.ForEach(task =>
            {
                if (task != null)
                {
                    task.MediaSource = null;
                }
            });
        }

        private void LoadTemplates()
        {
            if (!ChecklistTemplate.OpenFieldsProperties.IsNullOrEmpty())
                AddOpenFieldsToChecklist();
            AddSignButtonToTaskTemplates();
            if (Stages.HasStages && TaskFilter != null)
            {
                var stageTemplates = Stages.GetAllFilteredTaskTemplates();
                TaskFilter?.AddFilteredItems(openFieldsButton, submitButton, stageTemplates);
                return;
            }

            TaskFilter?.AddFilteredItems(openFieldsButton, submitButton);
        }

        private void SetPager()
        {
            if (SelectedTask != null)
            {
                if (SelectedTask.Id < 0 || CurrentIndex == -1)
                {
                    Pager = string.Format($"{TranslateExtension.GetValueFromDictionary(LanguageConstants.totalTaskCountText)} {CountItems(TaskFilter?.UnfilteredItems)}");
                }
                else
                {
                    string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskPageNumberText);
                    var taskIndex = Stages.HasStages ? TaskFilter?.FilteredSlideList.Where(x => x.Id != -1 && x.Id != -2).ToList().IndexOf(SelectedTask) : TaskFilter?.FilteredList.IndexOf(SelectedTask);
                    var index = TaskFilter?.FilteredSlideList.IndexOf(SelectedTask);

                    if (TaskFilter?.FilteredSlideList != null && TaskFilter?.StatusFilter == null)
                    {
                        Pager = string.Format(result.ReplaceLanguageVariablesCumulative(), taskIndex + 1, CountItems(TaskFilter?.FilteredList));
                    }
                    else
                        Pager = string.Format(result.ReplaceLanguageVariablesCumulative(), openFieldsButton != null ? index : index + 1, CountItems(TaskFilter?.FilteredSlideList));
                }
            }
            else { Pager = string.Empty; }
        }

        private int CountItems(List<BasicTaskTemplateModel> tasks)
        {
            if (tasks == null)
                return 0;

            var result = tasks.Count(x => x != null && x.Id != -1 && x.Id != -2);

            return result;
        }

        private void ToggleButtons()
        {
            if (SelectedTask != null)
            {
                bool result = !(SelectedTask.Id < 0 || CurrentIndex == -1);
                HasButtons = result;
            }
            else { HasButtons = false; }
        }

        private void AddSignButtonToTaskTemplates()
        {
            submitButton = new BasicTaskTemplateModel
            {
                IsSignButton = true,
                Name = TaskFilter.SelectedFilter?.Name,
                Id = -1,
            };
        }

        private void AddOpenFieldsToChecklist()
        {
            openFieldsButton = new BasicTaskTemplateModel
            {
                IsPropertyButton = true,
                IsSignButton = false,
                Name = "Button",
                Id = -2
            };
        }

        /// <summary>
        /// Toggles the task status.
        /// </summary>
        /// <param name="status">The status.</param>
        private async Task ToggleTaskStatus(TaskStatusEnum status)
        {
            if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled && Stages.HasStages)
            {
                if (ChecklistOpenFields.IsSyncing)
                {
                    ChecklistOpenFields.SendSyncingInProgressClosableWarning();
                    return;
                }

                if (Stages.IsTaskLocked(SelectedTask))
                {
                    if (SelectedTask.FilterStatus == status && SelectedTask.PictureProofMediaItems?.Count > 0 && (SelectedTask.FilterStatus == TaskStatusEnum.NotOk || SelectedTask.FilterStatus == TaskStatusEnum.Ok))
                    {
                        CurrentStatus = status;
                        SelectedTask = SelectedTask;
                        await NavigateToPictureProofDetails(false);
                        return;
                    }

                    Stages.SendClosableWarning(SelectedTask);
                    return;
                }
            }

            if ((status == TaskStatusEnum.Ok || status == TaskStatusEnum.NotOk) && !Validate())
                return;

            CurrentStatus = status;

            //picture proof
            if (SelectedTask.HasPictureProof)
            {
                if (status == TaskStatusEnum.Skipped)
                {
                    if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped)
                        await UntapTaskAsync();
                    else
                        OpenSkipTaskPopup();
                }
                else if (SelectedTask.FilterStatus == TaskStatusEnum.Todo || SelectedTask.FilterStatus == TaskStatusEnum.Skipped)
                {
                    CurrentStatus = null;

                    using var scope = App.Container.CreateScope();
                    var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
                    pictureProofViewModel.SelectedTaskTemplate = SelectedTask;
                    pictureProofViewModel.TaskStatus = status;
                    pictureProofViewModel.IsNew = true;
                    await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
                }
                else
                {
                    if (SelectedTask.FilterStatus != status)
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
                await SetSelectedTaskStatus(status);
                // Its delayed in order for device to refresh the view,
                // if status filter is selected and you tap very fast task statuses it might cause to list dissaperance
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    await Task.Delay(10);
            }
        }

        private bool Validate()
        {
            bool isValid = true;
            isValid = SelectedTask?.Validate() ?? false;
            return isValid;
        }

        public async override Task SeePicturesAsync()
        {
            await NavigateToPictureProofDetails(shouldChangeTaskStatus: false);
            await base.SeePicturesAsync();
        }

        private async Task NavigateToPictureProofDetails(bool supportsEditing = true, bool shouldChangeTaskStatus = true)
        {
            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.SelectedTaskTemplate = SelectedTask;
            pictureProofViewModel.TaskStatus = shouldChangeTaskStatus ? CurrentStatus.Value : null;
            pictureProofViewModel.MainMediaElement = SelectedTask.PictureProofMediaItems.FirstOrDefault();

            if (SelectedTask.PictureProofMediaItems?.Count > 1)
                pictureProofViewModel.MediaElements = new System.Collections.ObjectModel.ObservableCollection<MediaItem>(SelectedTask.PictureProofMediaItems.Skip(1));

            pictureProofViewModel.IsNew = false;
            pictureProofViewModel.SupportsEditing = supportsEditing;
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
        }

        public async override Task UntapTaskAsync()
        {
            SelectedTask.FilterStatus = TaskStatusEnum.Todo;
            SelectedTask.PictureProofMediaItems = new List<MediaItem>();
            await AdaptChanges();
            TaskHelper.CalculateTaskAmounts(TaskFilter);
            await base.UntapTaskAsync();
        }

        private async Task SetSelectedTaskStatus(TaskStatusEnum status)
        {
            TaskHelper.SetTaskStatusAsync(SelectedTask, status, null);

            if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped)
                SelectedTask.ResetValidation();

            await AdaptChanges().ConfigureAwait(false);

            if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped && (CurrentIndex + 1) < TaskFilter.FilteredSlideList.Count)
                CurrentIndex++;

            CurrentStatus = null;
        }

        private async Task AdaptChanges(bool isFromStageSign = false)
        {
            if (isFromStageSign)
            {
                ChecklistOpenFields.AddChangedStageSign();
                ChecklistOpenFields.CalculateTasksDone();
                return;
            }

            ChecklistOpenFields.AddChangedTask(SelectedTask);
            await ChecklistOpenFields.AdaptChanges(_checklistService).ConfigureAwait(false);

            if (!TaskFilter.StatusFilters.IsNullOrEmpty())
                await ApplyFilter(TaskFilter.StatusFilters, false).ConfigureAwait(false);
            else if (TaskFilter.StatusFilter != null)
                await ApplyFilter(TaskFilter.StatusFilter, false).ConfigureAwait(false);
            else
            {
                TaskHelper.CalculateTaskAmounts(TaskFilter);
            }
        }


        private void RemoveAdditionalSlides()
        {
            var list = TaskFilter?.FilteredSlideList;
            if (openFieldsButton != null && list != null)
                list.Remove(openFieldsButton);
            if (submitButton != null && list != null)
                list.Remove(submitButton);

            if (TaskFilter != null)
                TaskFilter.FilteredSlideList = list ?? new List<BasicTaskTemplateModel>();

        }

        /// <summary>
        /// Filters the task templates.
        /// </summary>
        /// <param name="status">Status filter to apply.</param>
        public override Task ApplyFilter(TaskStatusEnum? status = null, bool reset = true)
        {
            RemoveAdditionalSlides();
            TaskFilter.Filter(status, useDataSource: false, resetIfTheSame: reset, reloadFileredList: _reloadFilteredList);
            LoadTemplates();
            SetPager();

            if (!ChecklistTemplate.OpenFieldsProperties.IsNullOrEmpty() && TaskFilter.FilteredSlideList.Any() && TaskFilter.StatusFilter != null && CurrentIndex < 0)
                CurrentIndex += 2;

            if (SelectedTask?.FilterStatus != status)
            {
                MainThread.InvokeOnMainThreadAsync(() => CurrentIndex = ChecklistTemplate.OpenFieldsProperties.IsNullOrEmpty() ? 0 : 1);
            }

            TaskHelper.CalculateTaskAmounts(TaskFilter);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Filters the task templates.
        /// </summary>
        /// <param name="status">Status filter to apply.</param>
        public Task ApplyFilter(List<TaskStatusEnum?> statusses = null, bool reset = true)
        {
            RemoveAdditionalSlides();
            TaskFilter.Filter(statusses, useDataSource: false, resetIfTheSame: reset, reloadFileredList: _reloadFilteredList);
            LoadTemplates();
            SetPager();

            if (!ChecklistTemplate.OpenFieldsProperties.IsNullOrEmpty() && TaskFilter.FilteredSlideList.Any() && TaskFilter.StatusFilter != null && CurrentIndex < 0)
                CurrentIndex += 2;

            TaskHelper.CalculateTaskAmounts(TaskFilter);

            return Task.CompletedTask;
        }

        public override async Task SubmitSkipCommandAsync()
        {
            SelectedTask.PictureProofMediaItems = new List<MediaItem>();
            await SetSelectedTaskStatus(TaskStatusEnum.Skipped);
            await base.SubmitSkipCommandAsync().ConfigureAwait(false);
        }

        public async override Task KeepButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                await SetSelectedTaskStatus(CurrentStatus.Value);
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

        private async Task SaveChecklistAsync()
        {
            if (ChecklistOpenFields.IsSyncing)
                return;

            IsBusy = true;
            ChecklistOpenFields.IsSyncing = true;
            if (ChecklistOpenFields.AnyLocalChanges)
            {
                var model = new PostTemplateModel(ChecklistTemplateId, ChecklistTemplate.Name, ChecklistOpenFields.PropertyList, TaskFilter.UnfilteredItems, DeepLinkCompletionIsRequired, TaskFromDeepLink?.Id, false, IncompleteChecklist?.Id ?? 0, Stages.StageTemplates, ChecklistTemplate.Version);
                model.StartedAt = ChecklistOpenFields.StartedAt;
                model.LocalGuid = IncompleteChecklist?.LocalGuid ?? ChecklistTemplate.LocalGuid;

                // When incomplete checklist, send only changed tasks and don't modify started at field and signatures
                if (IncompleteChecklist?.Id > 0)
                {
                    model.Tasks = new List<BasicTaskTemplateModel>(ChecklistOpenFields.GetChangedTasks());
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
                ChecklistOpenFields.ClearChangedTasks();
                await UpdateTaskStatuses(IncompleteChecklist?.Id).ConfigureAwait(false);
            }

            IsBusy = false;
            ChecklistOpenFields.ClearChangedTasks();
            Stages.ClearChangedStages();
            MessagingCenter.Send(this, Constants.NoUnsavedChanges);
        }

        private readonly SemaphoreSlim tasksSemaphore = new SemaphoreSlim(1, 1);

        private async Task UpdateTaskStatuses(int? id)
        {
            try
            {
                if (id == null)
                    await Task.CompletedTask;

                var checklistFromApi = await _checklistService.GetIncompleteChecklistAsync(refresh: true, checklistId: IncompleteChecklist.Id).ConfigureAwait(false);

                await tasksSemaphore.WaitAsync();

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

                        // Update the signature
                        if (pair.change.Signature == null)
                            pair.task.Signature = null;
                        else
                        {
                            pair.task.Signature = new SignatureModel
                            {
                                SignedAt = pair.change.Signature.SignedAt,
                                SignedById = pair.change.Signature.SignedById,
                                SignedBy = pair.change.Signature.SignedBy,
                            };
                        }

                        // Check for picture proof
                        if (CompanyFeatures.RequiredProof && pair.change.HasPictureProof)
                        {
                            pair.task.PictureProofMediaItems = null;
                            pair.task.PictureProof = pair.change.PictureProof;
                            pair.task.SetPictureProofMediaItems();
                            pair.task.HasPictureProof = pair.change.HasPictureProof;
                        }

                        // Properties
                        if (pair.change.PropertyUserValues != null)
                        {
                            foreach (var value in pair.change.PropertyUserValues)
                            {
                                pair.task.AddOrUpdateUserProperty(value);
                            }

                            pair.task.RefreshPropertyValueString();
                        }
                    }
                }

                // Open fields
                if (checklistFromApi.OpenFieldsProperties != null)
                {
                    IOpenTextFields open = ChecklistOpenFields.CheckAndUpdateOpenFields(checklistFromApi, ChecklistTemplate);
                    ChecklistOpenFields.SetPropertyValues(open);
                }

                AnyRemoteChanges = false;
                await ChecklistOpenFields.AdaptChanges(_checklistService).ConfigureAwait(false);
                ChecklistOpenFields.CalculateTasksDone();
                TaskHelper.CalculateTaskAmounts(TaskFilter);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                tasksSemaphore.Release();
                ChecklistOpenFields.ClearChangedTasks();
                ChecklistOpenFields.IsSyncing = false;
            }
        }


        #region Navigation

        private async Task NavigateToSignPageOrFinishChecklistAsync()
        {

            if (ChecklistOpenFields.TasksDone)
            {
                if (IsSignatureRequired)
                {
                    using var scope = App.Container.CreateScope();
                    var checklistSignViewModel = scope.ServiceProvider.GetService<ChecklistSignViewModel>();
                    checklistSignViewModel.ChecklistTemplateId = ChecklistTemplateId;
                    checklistSignViewModel.TaskTemplates = TaskFilter.UnfilteredItems;
                    checklistSignViewModel.OpenFieldsValues = ChecklistOpenFields.PropertyList;
                    checklistSignViewModel.TaskFromDeepLink = TaskFromDeepLink;
                    checklistSignViewModel.StartedAt = ChecklistOpenFields.StartedAt;
                    checklistSignViewModel.DeepLinkCompletionIsRequired = DeepLinkCompletionIsRequired;
                    checklistSignViewModel.IncompleteChecklistLocalGuid = IncompleteChecklist?.LocalGuid ?? ChecklistTemplate.LocalGuid;
                    checklistSignViewModel.IncompleteChecklistId = IncompleteChecklist?.Id ?? 0;
                    checklistSignViewModel.Stages = Stages.StageTemplates;

                    // When incomplete checklist, send only changed tasks and don't modify started at field
                    if (IncompleteChecklist?.Id > 0)
                    {
                        checklistSignViewModel.TaskTemplates = ChecklistOpenFields.GetChangedTasks();
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

                    var model = new PostTemplateModel(ChecklistTemplateId, TaskFilter.SelectedFilter.Name, ChecklistOpenFields.PropertyList, TaskFilter.UnfilteredItems, DeepLinkCompletionIsRequired, TaskFromDeepLink?.Id, true, IncompleteChecklist?.Id ?? 0, Stages.StageTemplates, ChecklistTemplate.Version);
                    model.StartedAt = ChecklistOpenFields.StartedAt;
                    model.LocalGuid = IncompleteChecklist?.LocalGuid ?? ChecklistTemplate.LocalGuid;

                    // When incomplete checklist, send only changed tasks and don't modify started at field
                    if (IncompleteChecklist?.Id > 0)
                    {
                        model.Tasks = ChecklistOpenFields.GetChangedTasks();
                        model.StartedAt = IncompleteChecklist.CreatedAt;
                        model.Stages = new List<Models.Stages.StageTemplateModel>(Stages.GetChangedStage());
                        model.CreatedBy = IncompleteChecklist.CreatedBy;
                        model.CreatedById = IncompleteChecklist.CreatedById;
                    }

                    if (PagesFromDeepLink > 0)
                    {
                        var shiftChanged = await ShiftChanged.PerformChangeAsync().ConfigureAwait(false);

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
            else
            {
                ChecklistOpenFields.ShowInfoDialog(NavigationService.GetCurrentPage());
            }
        }

        private async Task NavigateToDetailAsync(BasicTaskTemplateModel selectedTask)
        {
            using var scope = App.Container.CreateScope();
            var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();

            // Items without openfields and sign button
            var items = TaskFilter.FilteredSlideList.Where(x => x.Id > 0).ToList();
            itemsDetailViewModel.Items = new List<Interfaces.Utils.IDetailItem>(items);
            itemsDetailViewModel.SelectedItem = selectedTask;
            itemsDetailViewModel.SenderClassName = nameof(ChecklistSlideViewModel);
            await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
        }

        private async Task NavigateToMoreInfoAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                using var scope = App.Container.CreateScope();

                if (SelectedTask.HasWorkInstructions)
                {
                    if (!await BeforeNavigatingToInstructions())
                        return;

                    await NavigateToWorkInstructions(SelectedTask.WorkInstructionRelations);
                }

                if (SelectedTask.HasAttachments)
                {
                    var attachement = SelectedTask.Attachments.FirstOrDefault();

                    switch (SelectedTask.AttachmentType)
                    {
                        case AttachmentEnum.Pdf:
                            var pdfViewerViewModel = scope.ServiceProvider.GetService<PdfViewerViewModel>();
                            pdfViewerViewModel.DocumentUri = attachement.Uri;
                            await NavigationService.NavigateAsync(viewModel: pdfViewerViewModel);
                            break;
                        case AttachmentEnum.Link:
                            await Launcher.OpenAsync(new Uri(attachement.Uri as string));
                            break;
                    }
                    return;
                }

                if (SelectedTask.HasSteps)
                {
                    var stepsViewModel = scope.ServiceProvider.GetService<StepsViewModel>();
                    stepsViewModel.Steps = SelectedTask.Steps;
                    stepsViewModel.Name = SelectedTask.Name;
                    await NavigationService.NavigateAsync(viewModel: stepsViewModel);
                    return;
                }
            });
        }

        private async Task OpenPopupOrNavigateToActionsAsync()
        {
            //Uncomment this code in case business logic changes about locking actions in locked stage
            // if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled && Stages.HasStages)
            // {
            //     if (Stages.IsTaskLocked(SelectedTask))
            //     {
            //         Stages.SendClosableWarning(SelectedTask);
            //         return;
            //     }
            // }

            if (SelectedTask.ActionBubbleCount > 0)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskTemplateId = SelectedTask.Id;
                actionOpenActionsViewModel.ActionType = ActionType.Checklist;
                actionOpenActionsViewModel.LocalTask = SelectedTask;
                actionOpenActionsViewModel.TaskTitle = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forChecklistItem)} {SelectedTask.Name}";

                await NavigationService.NavigateAsync(viewModel: actionOpenActionsViewModel);
            }
            else
            {
                IsActionPopupOpen = !IsActionPopupOpen;
            }
        }

        private async Task NavigateToNewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.TaskTemplateId = SelectedTask.Id;
            actionNewViewModel.ActionType = ActionType.Checklist;
            actionNewViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forChecklistItem)} {SelectedTask.Name}";
            actionNewViewModel.LocalTask = SelectedTask;

            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }

        private async Task NavigateToNewCommentAsync()
        {
            using var scope = App.Container.CreateScope();
            var taskCommentEditViewModel = scope.ServiceProvider.GetService<TaskCommentEditViewModel>();
            taskCommentEditViewModel.TaskTemplateId = SelectedTask.Id;
            taskCommentEditViewModel.IsNew = true;
            taskCommentEditViewModel.LocalTask = SelectedTask;
            taskCommentEditViewModel.Type = ActionType.Checklist;
            taskCommentEditViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forChecklistItem)} {SelectedTask.Name}";

            await NavigationService.NavigateAsync(viewModel: taskCommentEditViewModel);
        }

        private async Task NavigateToSignPageOrSaveStage(BasicTaskTemplateModel taskTemplate)
        {
            if (ChecklistOpenFields?.IsSyncing ?? false)
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

        #endregion

        #region Popup

        private void OpenFeaturePopup(BasicTaskPropertyModel prop)
        {
            if (CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled && Stages.HasStages)
            {
                if (Stages.IsTaskLocked(SelectedTask))
                {
                    Stages.SendClosableWarning(SelectedTask);
                    return;
                }
            }

            if (prop == null)
                return;

            PropertyEditViewModel = BaseTaskPropertyEditViewModel.FromPropertyModel(prop);
            IsPopupOpen = true;
            _statusBarService.HideStatusBar();
        }

        private async Task SubmitPopupAsync()
        {
            // If succeeded submitting the value
            if (PropertyEditViewModel.TrySubmit())
            {
                // Get the value
                var propertyValue = PropertyEditViewModel.GetValue();

                // Make sure we have a list
                SelectedTask.PropertyValues ??= new List<PropertyUserValue>();

                // If that value is not already in the list of values
                if (!SelectedTask.PropertyValues.Contains(propertyValue))
                // Add it
                {
                    propertyValue.CreatedAt = propertyValue.CreatedAt;
                    propertyValue.ModifiedAt = DateTime.UtcNow;
                    propertyValue.RegisteredAt = DateTime.UtcNow;
                    propertyValue.ModifiedBy = UserSettings.Fullname;
                    propertyValue.Id = 0;
                    SelectedTask.PropertyValues.Add(propertyValue);
                }
                else
                {
                    propertyValue.ModifiedAt = DateTime.UtcNow;
                    propertyValue.RegisteredAt = DateTime.UtcNow;
                    propertyValue.ModifiedBy = UserSettings.Fullname;
                }

                SelectedTask.AddModifiedProperty(propertyValue);
                await ChecklistOpenFields.AdaptChanges(_checklistService).ConfigureAwait(false);
                ChecklistOpenFields.AddChangedTask(SelectedTask);

                IsPopupOpen = false;
                _statusBarService.HideStatusBar();

                // Make these automatic
                SelectedTask.RefreshPropertyValueString();
                PropertyEditViewModel.Property.UpdatePrimaryDisplayValue();
                PropertyEditViewModel.Property.UpdateDisplayType();
                PropertyEditViewModel.Property.Validate();
            }
        }

        private void CloseFeaturePopup()
        {
            IsPopupOpen = false;
            _statusBarService.HideStatusBar();
        }

        #endregion

        #endregion
    }
}
