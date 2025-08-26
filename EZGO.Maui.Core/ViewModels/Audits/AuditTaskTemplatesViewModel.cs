using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Local;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using PropertyChanged;
using Syncfusion.Maui.DataSource;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;


namespace EZGO.Maui.Core.ViewModels.Audits
{
    public class AuditTaskTemplatesViewModel : BasicTaskViewModel
    {
        #region Properties

        private BasicTaskTemplateModel taskTemplate;

        private int maxAuditScore;

        private int minAuditScore;

        private decimal maxScore;

        public AuditTemplateModel auditTemplate;

        public BasicAuditTemplateModel SelectedAudit { get; set; }

        public IScoreColorCalculator ColorCalculator { get; set; }

        public bool IsSignatureRequired { get; set; }

        public bool IsBusy { get; set; }

        public ScoreTypeEnum ScoreType { get; set; }
        public List<ScoreModel> Scores { get; set; } = new List<ScoreModel>();

        public double ScoreWidth { get; set; } = 615;

        public BasicTaskModel TaskFromDeepLink { get; set; }

        /// <summary>
        /// Gets or sets the grid data source.
        /// </summary>
        /// <value>
        /// The grid data source.
        /// </value>
        public DataSource GridDataSource { get; set; }

        /// <summary>
        /// Gets or sets the list data source.
        /// </summary>
        /// <value>
        /// The list data source.
        /// </value>
        public DataSource ListDataSource { get; set; }

        /// <summary>
        /// Gets or sets the checklist template identifier.
        /// </summary>
        /// <value>
        /// The checklist template identifier.
        /// </value>
        public int AuditTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the task templates.
        /// </summary>
        /// <value>
        /// The task templates.
        /// </value>
        public List<BasicTaskTemplateModel> TaskTemplates { get; set; }

        /// <summary>
        /// Gets or sets the name of the checklist.
        /// </summary>
        /// <value>
        /// The name of the checklist.
        /// </value>
        public string AuditName { get; set; }

        [DoNotCheckEquality]
        public int Score { get; set; } = 0;

        public int CurrentProgress => Score;

        public List<BasicAuditTemplateModel> AuditTemplates { get; set; }

        public Rect Rect { get; set; } = new Rect(113, .2, .4, .6);

        public string SearchText { get; set; }

        public bool IsSearchBarVisible { get; set; }

        public ListViewLayout ListViewLayout { get; set; }

        public bool IsGridVisible { get; set; }

        public bool IsListVisible { get; set; }

        public bool OpenedFromDeepLink { get; set; }

        public int PagesFromDeepLink { get; set; }

        public bool IsActionPopupOpen { get; set; }

        public ChecklistOpenFields OpenFields { get; set; }

        public FilterControl<BasicTaskTemplateModel, TaskStatusEnum> TaskFilter { get; set; } = new FilterControl<BasicTaskTemplateModel, TaskStatusEnum>(null);

        public int? CurrentScore { get; set; }

        public bool IsFromBookmark { get; set; } = false;

        public bool ContainsTags => SelectedAudit?.Tags?.Count > 0;

        public bool DeepLinkCompletionIsRequired { get; set; } = false;

        public bool IsChangingAudit { get; set; } = false;

        #endregion

        #region Commands

        public ICommand ActionCommand => new Command<BasicTaskTemplateModel>(task => ExecuteLoadingAction(() => OpenPopupOrNavigateToActionsAsync(task)), CanExecuteCommands);

        public ICommand NavigateToNewActionCommand => new Command(() => ExecuteLoadingAction(NavigateToNewActionAsync), CanExecuteCommands);

        public ICommand NavigateToNewCommentCommand => new Command(() => ExecuteLoadingAction(NavigateToNewCommentAsync), CanExecuteCommands);

        /// <summary>
        /// Gets the task skipped command.
        /// </summary>
        /// <value>
        /// The task skipped command.
        /// </value>
        public ICommand TaskSkippedCommand => new Command<BasicTaskTemplateModel>(taskTemplate => ExecuteLoadingAction(async () => await ToggleTaskStatus(taskTemplate, TaskStatusEnum.Skipped)), CanExecuteCommands);

        /// <summary>
        /// Gets the task not ok command.
        /// </summary>
        /// <value>
        /// The task not ok command.
        /// </value>
        public ICommand TaskNotOkCommand => new Command<BasicTaskTemplateModel>(taskTemplate => ExecuteLoadingAction(async () => await ToggleTaskStatus(taskTemplate, TaskStatusEnum.NotOk)), CanExecuteCommands);


        /// <summary>
        /// Gets the task ok command.
        /// </summary>
        /// <value>
        /// The task ok command.
        /// </value>
        public ICommand TaskOkCommand => new Command<BasicTaskTemplateModel>(taskTemplate => ExecuteLoadingAction(async () => await ToggleTaskStatus(taskTemplate, TaskStatusEnum.Ok)), CanExecuteCommands);

        /// <summary>
        /// Gets the task ok command.
        /// </summary>
        /// <value>
        /// The task ok command.
        /// </value>
        public ICommand TaskScoreCommand => new Command<object>(str => ExecuteLoadingAction(async () => await SetTaskScore(taskTemplate, str)), CanExecuteCommands);

        public ICommand ListViewLayoutCommand => new Command<object>((listview) => ExecuteLoadingAction(() => SetListViewLayout(listview)), CanExecuteCommands);

        /// <summary>
        /// Gets the detail command.
        /// </summary>
        /// <value>
        /// The detail command.
        /// </value>
        public ICommand DetailCommand => new Command<object>(obj => ExecuteLoadingAction(async () => await NavigateToDetailCarouselAsync(obj)), CanExecuteCommands);

        /// <summary>
        /// Gets the dropdown tap command.
        /// </summary>
        /// <value>
        /// The dropdown tap command.
        /// </value>
        public IAsyncRelayCommand DropdownTapCommand => new AsyncRelayCommand<object>((obj) => ExecuteLoadingActionAsync(async () => await DropdownTapAsync(obj)), CanExecuteCommands);

        /// <summary>
        /// Gets the search text changed command.
        /// </summary>
        /// <value>
        /// The search text changed command.
        /// </value>
        public ICommand SearchTextChangedCommand { get; private set; }

        public ICommand SignCommand => new Command(() => ExecuteLoadingAction(NavigateToSignPageOrFinishAuditAsync), CanExecuteCommands);

        public ICommand OpenScoreCommand => new Command<BasicTaskTemplateModel>(taskTemplate =>
        {
            ExecuteLoadingAction(async () =>
            {
                this.taskTemplate = taskTemplate;
                if (!await Validate(taskTemplate))
                    return;

                MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.ScorePopupMessage); });
            });
        });

        public ICommand DeleteTagCommand => new Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>((obj) =>
        {
            if (obj.DataItem is TagModel tag)
            {
                TaskFilter.SearchedTags.Remove(tag);
                tag.IsActive = !tag.IsActive;
                TaskFilter.Filter(false, false);
            }
        }, CanExecuteCommands);

        public ICommand StepsCommand => new Command<BasicTaskTemplateModel>(obj => ExecuteLoadingAction(async () => await NavigateToMoreInfoAsync(obj)), CanExecuteCommands);

        protected override void RefreshCanExecute()
        {
            (ActionCommand as Command)?.ChangeCanExecute();
            (NavigateToNewActionCommand as Command)?.ChangeCanExecute();
            (NavigateToNewCommentCommand as Command)?.ChangeCanExecute();
            (TaskSkippedCommand as Command)?.ChangeCanExecute();
            (TaskNotOkCommand as Command)?.ChangeCanExecute();
            (TaskOkCommand as Command)?.ChangeCanExecute();
            (TaskScoreCommand as Command)?.ChangeCanExecute();
            (DetailCommand as Command)?.ChangeCanExecute();
            (SignCommand as Command)?.ChangeCanExecute();
            (StepsCommand as Command)?.ChangeCanExecute();
            (OpenScoreCommand as Command)?.ChangeCanExecute();
            (DropdownTapCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        #endregion

        #region Construction and initialization

        private readonly IAuditsService _auditService;
        private readonly ITasksService _taskService;
        private readonly IPropertyService _propertyService;

        /// <summary>
        /// Stores the task on which the action button was pressed
        /// </summary>
        private BasicTaskTemplateModel lastTappedActionTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTemplatesViewModel"/> class.
        /// </summary>
        public AuditTaskTemplatesViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAuditsService auditsService,
            ITasksService tasksService,
            IPropertyService propertyService) : base(navigationService, userService, messageService, actionsService)
        {
            _auditService = auditsService;
            _taskService = tasksService;
            _propertyService = propertyService;


            SearchTextChangedCommand = new Command((obj) =>
            {
                if (obj is string searchText)
                    TaskFilter.SearchText = searchText;
                TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
            });
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            if (PagesFromDeepLink > 0 || IsFromBookmark)
                OpenedFromDeepLink = true;
            await LoadTaskTemplatesAsync();

            OpenFields = new ChecklistOpenFields(TaskTemplates, AuditTemplateId);

            if (auditTemplate == null)
            {
                HasItems = false;
            }
            else
            {
                // create the Scorebuttons
                LoadScores();

                CalculateTaskScores();

                MessagingCenter.Subscribe<AuditSlideViewModel>(this, Constants.RecalculateAmountsMessage, (settingsViewModel) =>
                {
                    CalculateTaskScores();
                });

                MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionsChanged, async actionService =>
                {
                    await _taskService.LoadOpenActionCountForTaskTemplatesAsync(TaskTemplates);
                });

                MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionChanged, async actionService =>
                {
                    await _taskService.LoadOpenActionCountForTaskTemplatesAsync(TaskTemplates);
                });

                MessagingCenter.Subscribe<TaskCommentEditViewModel>(this, Constants.TaskTemplateCommentAdded, async _ =>
                {
                    await OpenFields.SaveLocalChanges(_auditService);
                });

                MessagingCenter.Subscribe<SyncService>(this, Constants.AuditTemplateChanged, (sender) =>
                {
                    RefreshCommand.Execute(sender);
                });

                MessagingCenter.Subscribe<PictureProofViewModel>(this, Constants.PictureProofChanged, async (_) =>
                {
                    await AdaptChanges().ConfigureAwait(false);
                    SetTotalScore();
                });

                if (AuditTemplates != null && AuditTemplates.Count > 6)
                    Rect = new Rect(113, .8, .4, .9);

                SetListViewLayout(Settings.ListViewLayout);

                if (!OpenedFromDeepLink)
                    SelectedAudit = AuditTemplates?.FirstOrDefault(x => auditTemplate.Id == x.Id);
            }

            OpenFields.CalculateTasksDone();
            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<AuditSlideViewModel>(this, Constants.RecalculateAmountsMessage);
            MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionsChanged);
            MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionChanged);
            MessagingCenter.Unsubscribe<TaskCommentEditViewModel>(this, Constants.TaskTemplateCommentAdded);
            MessagingCenter.Unsubscribe<SyncService>(this, Constants.AuditTemplateChanged);
            MessagingCenter.Unsubscribe<PictureProofViewModel>(this, Constants.PictureProofChanged);

            OpenFields?.Dispose();
            TaskFilter?.Dispose();
            TaskFilter = null;
            OpenFields = null;
            TaskTemplates = null;
            base.Dispose(disposing);
        }

        public override bool CanToggleDropdown()
        {
            return !OpenedFromDeepLink && base.CanToggleDropdown();
        }

        public override async void OnDisappearing(object sender, EventArgs e)
        {
            if (!IsBusy && _auditService != null) await OpenFields?.AdaptChanges(_auditService);
            base.OnDisappearing(sender, e);
        }

        #endregion

        /// <summary>
        /// Loads the task templates asynchronous.
        /// </summary>
        private async Task LoadTaskTemplatesAsync()
        {
            if (!IsFromBookmark)
                auditTemplate = await _auditService?.GetAuditTemplateAsync(id: AuditTemplateId, refresh: IsRefreshing);

            if (auditTemplate != null)
            {
                if (AuditTemplates != null && TaskFilter != null)
                {
                    TaskFilter.FilterCollection = AuditTemplates?.Select(x => new FilterModel(x.Name, x.Id)).ToList();
                    TaskFilter.SetSelectedFilter(filterName: auditTemplate.Name, id: auditTemplate.Id);
                }
                else if (TaskFilter != null)
                    TaskFilter.SelectedFilter = new FilterModel(auditTemplate.Name, auditTemplate.Id);

                auditTemplate.TaskTemplates ??= new List<TaskTemplateModel>();
                // set some audittemplate related settings
                AuditName = auditTemplate.Name;

                IsSignatureRequired = auditTemplate.IsSignatureRequired;

                maxAuditScore = auditTemplate.MaxScore;
                minAuditScore = auditTemplate.MinScore;

                ScoreType = (ScoreTypeEnum)Enum.Parse(typeof(ScoreTypeEnum), auditTemplate.ScoreType ?? "thumbs", true);

                if (auditTemplate.TaskTemplates != null)
                {
                    List<BasicTaskTemplateModel> taskTemplates = auditTemplate.TaskTemplates.ToBasicList<BasicTaskTemplateModel, TaskTemplateModel>();

                    List<LocalTemplateModel> localAuditTemplates = await _auditService.GetLocalAuditTemplates();

                    LocalTemplateModel localAuditTemplate = localAuditTemplates?.LastOrDefault(item => item.Id == AuditTemplateId && item.UserId == UserSettings.Id);

                    if (localAuditTemplate != null)
                    {
                        List<LocalTaskTemplateModel> localTaskTemplates = localAuditTemplate.TaskTemplates ?? new List<LocalTaskTemplateModel>();

                        foreach (BasicTaskTemplateModel basicTask in taskTemplates)
                        {
                            LocalTaskTemplateModel localTaskTemplate = localTaskTemplates?.LastOrDefault(item => item.Id == basicTask.Id);

                            if (localTaskTemplate != null)
                            {
                                basicTask.FilterStatus = localTaskTemplate.Status ?? TaskStatusEnum.Todo;
                                basicTask.Score = localTaskTemplate.Score;
                                basicTask.PictureProofMediaItems = localTaskTemplate.PictureProofMediaItems;
                                basicTask.NewScore = localTaskTemplate.NewScore;
                                basicTask.HasPictureProof = localTaskTemplate.HasPictureProof;
                                basicTask.PropertyValues = localTaskTemplate.PropertyUserValues ?? new List<Api.Models.PropertyValue.PropertyUserValue>();
                                basicTask.LocalComments = localTaskTemplate.Comments ?? new List<Models.Comments.CommentModel>();
                            }
                        }
                    }

                    await _taskService.LoadOpenActionCountForTaskTemplatesAsync(taskTemplates, IsRefreshing);
                    await _propertyService.LoadTaskTemplatesPropertiesAsync(taskTemplates, refresh: IsRefreshing);

                    taskTemplates?.ForEach(x => x?.UpdateActionBubbleCount());

                    SetColorCalculator();
                    TaskTemplates = taskTemplates;

                    if (OpenFields != null)
                        OpenFields.TaskTemplates = TaskTemplates;

                    TaskFilter?.SetUnfilteredItems(TaskTemplates);

                    if (auditTemplate.OpenFieldsProperties != null)
                    {
                        IOpenTextFields open = OpenFields?.CheckAndUpdateOpenFields(localAuditTemplate, auditTemplate);
                        OpenFields?.SetPropertyValues(open);
                    }

                    // calculate total maxscore by weight
                    maxScore = TaskTemplates.Select(x => (x.Weight * maxAuditScore)).Sum();
                }
                else
                {
                    TaskFilter.SetHasItems();
                }
            }
            SelectedAudit = auditTemplate?.ToBasic();
        }

        private void SetColorCalculator() => ColorCalculator = auditTemplate.ScoreColorCalculator;
        /// <summary>
        /// Toggles the task status.
        /// </summary>
        /// <param name="taskTemplate">The task template.</param>
        /// <param name="status">The status.</param>
        private async Task ToggleTaskStatus(BasicTaskTemplateModel taskTemplate, TaskStatusEnum status)
        {
            if ((status == TaskStatusEnum.Ok || status == TaskStatusEnum.NotOk) && !await Validate(taskTemplate))
                return;

            this.taskTemplate = taskTemplate;
            CurrentStatus = status;

            //picture proof
            if (this.taskTemplate.HasPictureProof)
            {
                if (status == TaskStatusEnum.Skipped)
                {
                    if (this.taskTemplate.FilterStatus == TaskStatusEnum.Skipped)
                        await UntapTaskAsync();
                    else
                        OpenSkipTaskPopup();
                }
                else if (this.taskTemplate.FilterStatus == TaskStatusEnum.Todo || this.taskTemplate.FilterStatus == TaskStatusEnum.Skipped)
                {
                    CurrentStatus = null;

                    using var scope = App.Container.CreateScope();
                    var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
                    pictureProofViewModel.SelectedTaskTemplate = this.taskTemplate;
                    pictureProofViewModel.SelectedTaskTemplate.IsLocalMedia = true;
                    pictureProofViewModel.TaskStatus = status;
                    pictureProofViewModel.IsNew = true;
                    await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
                }
                else
                {
                    if (this.taskTemplate.FilterStatus != status)
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
                SetSelectedTaskStatus(status);

                await AdaptChanges();
                SetTotalScore();
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

        public async override Task SeePicturesAsync()
        {
            await NavigateToPictureProofDetails(shouldChangeTaskStatus: false);
            await base.SeePicturesAsync();
        }

        private async Task NavigateToPictureProofDetails(bool supportsEditing = true, bool shouldChangeTaskStatus = true)
        {
            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.SelectedTaskTemplate = this.taskTemplate;
            pictureProofViewModel.TaskStatus = shouldChangeTaskStatus ? CurrentStatus : null;
            pictureProofViewModel.Score = shouldChangeTaskStatus ? CurrentScore : null;
            pictureProofViewModel.MainMediaElement = this.taskTemplate.PictureProofMediaItems?.FirstOrDefault();
            pictureProofViewModel.SelectedTaskTemplate.IsLocalMedia = true;

            if (this.taskTemplate.PictureProofMediaItems?.Count > 1)
                pictureProofViewModel.MediaElements = new ObservableCollection<MediaItem>(taskTemplate.PictureProofMediaItems?.Skip(1));

            pictureProofViewModel.IsNew = false;
            pictureProofViewModel.SupportsEditing = supportsEditing;
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
            await base.SeePicturesAsync();
        }

        public async override Task UntapTaskAsync()
        {
            this.taskTemplate.FilterStatus = TaskStatusEnum.Todo;
            this.taskTemplate.PictureProofMediaItems = new List<MediaItem>();
            if (ScoreType == ScoreTypeEnum.Score)
            {
                this.taskTemplate.Score = null;
                this.taskTemplate.NewScore = null;
                MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.HideScorePopupMessage); });
            }
            SetTotalScore();
            await AdaptChanges();
            await base.UntapTaskAsync();
        }

        private void SetSelectedTaskStatus(TaskStatusEnum status)
        {
            TaskHelper.SetTaskStatusAsync(taskTemplate, status, null);

            CurrentStatus = null;
            SetTotalScore();
        }


        private async Task AdaptChanges()
        {
            await OpenFields.AdaptChanges(_auditService);
        }


        public override async Task SubmitSkipCommandAsync()
        {
            taskTemplate.PictureProofMediaItems = new List<MediaItem>();
            SetSelectedTaskStatus(TaskStatusEnum.Skipped);
            await AdaptChanges();
            await base.SubmitSkipCommandAsync();
        }

        public async override Task KeepButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null)
            {
                SetSelectedTaskStatus(CurrentStatus.Value);
            }
            else if (CurrentScore != null)
            {
                await SetSelectedTaskScore(CurrentScore.Value);
                MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.HideScorePopupMessage); });
            }
            await AdaptChanges();
            await base.KeepButtonChangeStatusPopupCommandAsync();
        }

        public async override Task RemoveButtonChangeStatusPopupCommandAsync()
        {
            if (CurrentStatus != null || CurrentScore != null)
            {
                await NavigateToPictureProofDetails();
            }
        }


        /// <summary>
        /// Set the task sore and sets status to Todo.
        /// </summary>
        /// <param name="taskTemplate">The task template.</param>
        /// <param name="score">The score value.</param>
        private async Task SetTaskScore(BasicTaskTemplateModel taskTemplate, object score)
        {
            this.taskTemplate = taskTemplate;

            if (score is int myscore)
            {
                CurrentScore = myscore;
                if (this.taskTemplate.HasPictureProof)
                {
                    if (this.taskTemplate.Score == CurrentScore)
                    {
                        await OpenUntapTaskDialogAsync();
                        return;
                    }
                    else if (this.taskTemplate.Score.HasValue && this.taskTemplate.Score != CurrentScore)
                    {
                        OpenChangeStatusPopup();
                        return;
                    }

                    using var scope = App.Container.CreateScope();
                    var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
                    pictureProofViewModel.SelectedTaskTemplate = this.taskTemplate;
                    pictureProofViewModel.Score = myscore;
                    pictureProofViewModel.IsNew = true;
                    await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
                    return;
                }

                await SetSelectedTaskScore(myscore);
            }
            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.HideScorePopupMessage); });
        }

        private async Task SetSelectedTaskScore(int score)
        {
            TaskHelper.SetTaskScore(taskTemplate, score);

            // Audits currently don't support task signatures - REMOVE WHEN ADDED SUPPORT ON BACKEND!
            taskTemplate.Signature = null;

            ScoreModel NewScore = new ScoreModel { Number = score, MinimalScore = minAuditScore, NumberOfScores = Math.Abs(maxAuditScore - minAuditScore + 1) };
            this.taskTemplate.NewScore = NewScore;

            await OpenFields.AdaptChanges(_auditService);
            SetTotalScore();
            CurrentScore = null;
        }

        /// <summary>
        /// Set all individual and total scores
        /// </summary>
        private void CalculateTaskScores()
        {
            SetTotalScore();
            OpenFields?.CalculateTasksDone();
        }

        /// <summary>
        /// Create the score buttons
        /// </summary>
        private void LoadScores()
        {
            if (ScoreType == ScoreTypeEnum.Score)
            {
                var scores = new List<ScoreModel>();
                int numberOfScores = Math.Abs(maxAuditScore - minAuditScore + 1);
                for (int i = minAuditScore; i <= maxAuditScore; i++)
                {
                    scores.Add(new ScoreModel
                    {
                        Number = i,
                        NumberOfScores = numberOfScores,
                        MinimalScore = minAuditScore,
                        Color = auditTemplate.ScoreColorCalculator.GetColor(i),
                    });
                }
                if (scores.Any()) { ScoreWidth = ((scores.Count * 60) + 15); }

                Scores = scores;
            }
        }

        /// <summary>
        /// Set page-top (total) score
        /// </summary>
        private void SetTotalScore()
        {
            int score = 0;
            switch (ScoreType)
            {
                case ScoreTypeEnum.Score:
                    score = (int)Math.Round(TaskTemplates?.Select(x => (x.Weight * x.Score)).Sum() ?? 0);
                    break;
                default: //thumbs
                    score = (int)Math.Round(TaskTemplates?.Where(x => x.FilterStatus == (TaskStatusEnum.Ok)).Select(x => (x.Weight * maxAuditScore)).Sum() ?? 0);
                    break;
            }
            if (maxScore > 0)
            {
                Score = (int)Math.Round((decimal)(100 * score) / maxScore);
            }
        }

        public async override Task CancelAsync()
        {
            OnlineShiftCheck.IsShiftChangeAllowed = true;

            await OnlineShiftCheck.CheckCycleChange();

            await base.CancelAsync();
        }

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
                Settings.ListViewLayout = listViewLayout;
            }
        }

        /// <summary>
        /// Handles a tap on the dropdown.
        /// </summary>
        /// <param name="obj">The object.</param>
        private async Task DropdownTapAsync(object obj)
        {
            IsDropdownOpen = false;
            if ((obj as Syncfusion.TreeView.Engine.TreeViewNode).Content is FilterModel filterModel)
            {
                if (AuditTemplateId != filterModel.Id)
                {
                    IsChangingAudit = true;

                    await OpenFields.AdaptChanges(_auditService);

                    AuditTemplateId = filterModel.Id;

                    SelectedAudit = AuditTemplates.FirstOrDefault(x => x.Id == AuditTemplateId);

                    OpenFields.ChecklistTemplateId = SelectedAudit.Id;

                    await LoadTaskTemplatesAsync();

                    LoadScores();

                    SetTotalScore();

                    IsChangingAudit = false;
                }
            }
        }

        #region Navigation

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
                    selectedTaskIndex = TaskTemplates.IndexOf(TappedTask);
                    if (!auditTemplate.OpenFieldsProperties.IsNullOrEmpty() && TaskFilter.StatusFilters != null)
                        selectedTaskIndex += 1;
                }
                else
                {
                    selectedTaskIndex = 0;
                }

                using var scope = App.Container.CreateScope();
                var auditSlideViewModel = scope.ServiceProvider.GetService<AuditSlideViewModel>();

                auditSlideViewModel.ScoreType = ScoreType;
                auditSlideViewModel.Scores = Scores;
                auditSlideViewModel.ScoreWidth = ScoreWidth;
                auditSlideViewModel.AuditTemplate = auditTemplate;
                auditSlideViewModel.CurrentIndex = selectedTaskIndex;
                auditSlideViewModel.TaskTemplates = TaskTemplates;
                auditSlideViewModel.OpenFields = OpenFields;
                auditSlideViewModel.TaskFromDeepLink = TaskFromDeepLink;
                auditSlideViewModel.DeepLinkCompletionIsRequired = DeepLinkCompletionIsRequired;

                if (PagesFromDeepLink > 0)
                    auditSlideViewModel.PagesFromDeepLink = PagesFromDeepLink + 1;

                await NavigationService.NavigateAsync(viewModel: auditSlideViewModel);
            }
        }

        private async Task NavigateToSignPageOrFinishAuditAsync()
        {
            if (OpenFields == null) return;
            if (OpenFields.TasksDone)
            {
                if (IsSignatureRequired)
                {
                    using var scope = App.Container.CreateScope();
                    var auditSignViewModel = scope.ServiceProvider.GetService<AuditSignViewModel>();

                    auditSignViewModel.AuditTemplateId = AuditTemplateId;
                    auditSignViewModel.TaskTemplates = TaskTemplates;
                    auditSignViewModel.OpenFieldsValues = OpenFields.PropertyList;
                    auditSignViewModel.TaskFromDeepLink = TaskFromDeepLink;
                    auditSignViewModel.StartedAt = OpenFields.StartedAt;
                    auditSignViewModel.DeepLinkCompletionIsRequired = DeepLinkCompletionIsRequired;

                    if (PagesFromDeepLink > 0)
                        auditSignViewModel.PagesFromDeepLink = PagesFromDeepLink + 1;

                    await NavigationService.NavigateAsync(viewModel: auditSignViewModel);
                }
                else
                {
                    IsBusy = true;

                    var model = new PostTemplateModel(AuditTemplateId, auditTemplate.Name, OpenFields.PropertyList, TaskTemplates, DeepLinkCompletionIsRequired, TaskFromDeepLink?.Id, version: auditTemplate.Version);
                    model.StartedAt = OpenFields.StartedAt;
                    await _auditService.PostAuditAsync(model);

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
                        await NavigationService.PopOrNavigateToPage<AuditViewModel>(typeof(AuditViewModel));

                    IsBusy = false;
                }
            }

            if (OpenFields != null && NavigationService != null)
                OpenFields.ShowInfoDialog(NavigationService.GetCurrentPage());
        }

        private async Task OpenPopupOrNavigateToActionsAsync(BasicTaskTemplateModel taskTemplate)
        {
            if (taskTemplate.ActionBubbleCount > 0)
            {
                using var scope = App.Container.CreateScope();
                var openActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();

                openActionsViewModel.TaskTemplateId = taskTemplate.Id;
                openActionsViewModel.LocalTask = taskTemplate;
                openActionsViewModel.ActionType = ActionType.Audit;
                openActionsViewModel.TaskTitle = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forAuditItem)} {taskTemplate.Name}";

                await NavigationService.NavigateAsync(viewModel: openActionsViewModel);
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
            taskCommentEditViewModel.Type = ActionType.Audit;
            taskCommentEditViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forAuditItem)} {lastTappedActionTask.Name}";

            await NavigationService.NavigateAsync(viewModel: taskCommentEditViewModel);
        }

        private async Task NavigateToNewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.TaskTemplateId = lastTappedActionTask.Id;
            actionNewViewModel.ActionType = ActionType.Audit;
            actionNewViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forAuditItem)} {lastTappedActionTask.Name}";
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

        #endregion
    }
}
