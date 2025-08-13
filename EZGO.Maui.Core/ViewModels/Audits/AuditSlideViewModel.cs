using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Core.ViewModels.Tasks;
using PropertyChanged;
using Syncfusion.Maui.DataSource.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace EZGO.Maui.Core.ViewModels
{
    public class AuditSlideViewModel : BasicTaskViewModel, IHasPopup, IHasTaskPropertiesEditViewModel
    {
        #region Public Properties

        public bool HasButtons { get; set; }

        public AuditTemplateModel AuditTemplate { get; set; }

        public IScoreColorCalculator ColorCalculator => AuditTemplate.ScoreColorCalculator;

        private int maxAuditScore => AuditTemplate.MaxScore;

        private decimal maxScore;

        public ScoreTypeEnum ScoreType { get; set; }

        public bool IsPopupOpen { get; set; }

        public List<ScoreModel> Scores { get; set; }

        public double ScoreWidth { get; set; } = 605;

        public BasicTaskModel TaskFromDeepLink { get; set; }

        private BasicTaskTemplateModel selectedTask;

        public BasicTaskTemplateModel SelectedTask
        {
            get { return selectedTask; }
            set
            {
                selectedTask = value;
                FilteredTaskTemplates.ForEach(task => task.MediaSource = null);
                UpdateSelected();
                ToggleButtons();
                OnPropertyChanged();
            }
        }

        //public bool CanSignAudit { get; set; }

        public List<BasicTaskTemplateModel> TaskTemplates { get; set; }


        public ObservableCollection<BasicTaskTemplateModel> FilteredTaskTemplates { get; set; }

        public int CurrentIndex { get; set; }

        /// <summary>
        /// Gets or sets the status filter.
        /// </summary>
        /// <value>
        /// The status filter.
        /// </value>
        public TaskStatusEnum? StatusFilter { get; set; }

        public int Score { get; set; } = 0;

        [DoNotNotify]
        public int PagesFromDeepLink { get; set; }

        public bool IsSignatureRequired { get; set; }

        public bool IsBusy { get; set; }

        /// <summary>
        /// Indicates if the action/comment popup is open
        /// </summary>
        public bool IsActionPopupOpen { get; set; }

        public BaseTaskPropertyEditViewModel PropertyEditViewModel { get; set; }

        public ChecklistOpenFields OpenFields { get; set; }

        public bool IsScorePopupOpen { get; set; } = false;

        public int? CurrentScore { get; set; }

        public bool DeepLinkCompletionIsRequired { get; set; } = false;

        #endregion

        #region Public Commands

        public ICommand StepsCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () => await NavigateToMoreInfoAsync());
        });

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

        /// <summary>
        /// Gets the filter command.
        /// </summary>
        /// <value>
        /// The filter command.
        /// </value>
        public ICommand FilterCommand => new Command<TaskStatusEnum>(FilterTaskTemplates);

        public ICommand SignCommand => new Command(() => ExecuteLoadingAction(NavigateToSignPageOrFinishAuditAsync), CanExecuteCommands);

        public ICommand OpenScoreCommand => new Command(() => ExecuteLoadingAction(() =>
        {
            if (!Validate())
                return;

            IsScorePopupOpen = !IsScorePopupOpen;
        }), CanExecuteCommands);

        public ICommand TaskScoreCommand => new Command<object>(str => ExecuteLoadingAction(async () => await SetTaskScore(str)), CanExecuteCommands);

        public ICommand DetailCommand => new Command<BasicTaskTemplateModel>((task) => ExecuteLoadingAction(async () => await NavigateToDetailAsync(task)), CanExecuteCommands);

        public ICommand ActionCommand => new Command(() => ExecuteLoadingAction(OpenPopupOrNavigateToActionsAsync), CanExecuteCommands);

        public ICommand NavigateToNewActionCommand => new Command(() => ExecuteLoadingAction(NavigateToNewActionAsync), CanExecuteCommands);

        public ICommand NavigateToNewCommentCommand => new Command(() => ExecuteLoadingAction(NavigateToNewCommentAsync), CanExecuteCommands);

        public ICommand OpenPopupCommand => new Command<BasicTaskPropertyModel>((prop) => ExecuteLoadingAction(() => OpenFeaturePopup(prop)), CanExecuteCommands);

        public ICommand SubmitPopupCommand => new Command(() => ExecuteLoadingAction(async () => await SubmitPopupAsync()), CanExecuteCommands);

        public ICommand ClosePopupCommand => new Command(CloseFeaturePopup);

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
            (OpenPopupCommand as Command)?.ChangeCanExecute();
            (SubmitPopupCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        #endregion

        #region Services

        private readonly IAuditsService _auditService;
        private readonly ITasksService _taskService;

        #endregion

        #region Construction

        public AuditSlideViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAuditsService auditsService,
            ITasksService tasksService) : base(navigationService, userService, messageService, actionsService)
        {
            _auditService = auditsService;
            _taskService = tasksService;
        }

        public override async Task Init()
        {
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // So that the carousel can init, otherwise there is some flickering 
                await Task.Delay(150);
            }

            LoadAudit();

            IsSignatureRequired = AuditTemplate.IsSignatureRequired;

            Title = AuditTemplate.Name;

            maxScore = TaskTemplates.Select(x => (x.Weight * AuditTemplate.MaxScore)).Sum();

            SetTotalScore();

            MessagingCenter.Subscribe<string, int>(this, Constants.UpdateSlideIndex, (senderClassName, index) =>
            {
                if (senderClassName != nameof(AuditSlideViewModel))
                    return;

                if (!AuditTemplate.OpenFieldsProperties.IsNullOrEmpty())
                    index++;

                CurrentIndex = index;
                SelectedTask = FilteredTaskTemplates.ElementAt(CurrentIndex);
                UpdateSelected();
            });

            MessagingCenter.Subscribe<PictureProofViewModel>(this, Constants.PictureProofChanged, async (_) =>
            {
                await AdaptChanges().ConfigureAwait(false);
                SetTotalScore();

            });

            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<string, int>(this, Constants.UpdateSlideIndex);
            MessagingCenter.Unsubscribe<PictureProofViewModel>(this, Constants.PictureProofChanged);

            OpenFields = null;
            _auditService.Dispose();
            _taskService.Dispose();

            base.Dispose(disposing);
        }

        public override async void OnDisappearing(object sender, EventArgs e)
        {
            if (!IsBusy) await OpenFields.AdaptChanges(_auditService);
            base.OnDisappearing(sender, e);
        }

        #endregion

        #region Command Methods

        private async void LoadAudit()
        {
            //sometimes, when you change a score in listview the carousel is empty(not showing image)
            //await Task.Delay(200);

            TaskTemplates ??= new List<BasicTaskTemplateModel>();

            var filteredTask = new LinkedList<BasicTaskTemplateModel>(TaskTemplates);

            AddSignButtonToTaskTemplates(filteredTask);

            if (!AuditTemplate.OpenFieldsProperties.IsNullOrEmpty())
                AddOpenFieldsToChecklist(filteredTask);

            FilteredTaskTemplates = new ObservableCollection<BasicTaskTemplateModel>(filteredTask);
        }

        private void UpdateSelected()
        {
            lock (TaskTemplates)
            {
                TaskTemplates.ForEach(x => x.IsSelected = false);
                if (SelectedTask != null)
                {
                    SelectedTask.IsSelected = true;
                    SelectedTask.UpdateActionBubbleCount();
                }
            }
        }

        private async Task ReloadAuditAsync()
        {
            await _taskService.LoadOpenActionCountForTaskTemplatesAsync(TaskTemplates);

            LoadAudit();
        }

        private void AddSignButtonToTaskTemplates(LinkedList<BasicTaskTemplateModel> basicTaskTemplates)
        {
            if (!basicTaskTemplates.Any(x => x.IsSignButton))
            {
                basicTaskTemplates.AddLast(new BasicTaskTemplateModel
                {
                    IsSignButton = true,
                    Name = AuditTemplate.Name,
                    Id = -1
                });
            }
        }


        private void AddOpenFieldsToChecklist(LinkedList<BasicTaskTemplateModel> basicTaskTemplates)
        {
            if (!basicTaskTemplates.Any(x => x.IsPropertyButton) && !AuditTemplate.OpenFieldsProperties.IsNullOrEmpty())
            {
                basicTaskTemplates.AddFirst(new BasicTaskTemplateModel
                {
                    IsPropertyButton = true,
                    IsSignButton = false,
                    Name = "Button",
                    Id = -2
                });
            }
        }

        /// <summary>
        /// Toggles the task status.
        /// </summary>
        /// <param name="status">The status.</param>
        private async Task ToggleTaskStatus(TaskStatusEnum status)
        {
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
                    pictureProofViewModel.SelectedTaskTemplate.IsLocalMedia = true;
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
                SetSelectedTaskStatus(status);

                await AdaptChanges();
                SetTotalScore();
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
            SelectedTask.PictureProofMediaItems ??= new List<MediaItem>();
            using var scope = App.Container.CreateScope();
            var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
            pictureProofViewModel.SelectedTaskTemplate = SelectedTask;
            pictureProofViewModel.TaskStatus = shouldChangeTaskStatus ? CurrentStatus : null;
            pictureProofViewModel.Score = shouldChangeTaskStatus ? CurrentScore : null;
            pictureProofViewModel.SelectedTaskTemplate.IsLocalMedia = true;
            pictureProofViewModel.MainMediaElement = SelectedTask.PictureProofMediaItems?.FirstOrDefault();

            if (SelectedTask.PictureProofMediaItems?.Count > 1)
                pictureProofViewModel.MediaElements = new ObservableCollection<MediaItem>(SelectedTask.PictureProofMediaItems?.Skip(1));

            pictureProofViewModel.IsNew = false;
            pictureProofViewModel.SupportsEditing = supportsEditing;
            await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
        }

        public async override Task UntapTaskAsync()
        {
            SelectedTask.FilterStatus = TaskStatusEnum.Todo;
            SelectedTask.PictureProofMediaItems = new List<MediaItem>();
            if (ScoreType == ScoreTypeEnum.Score)
            {
                SelectedTask.Score = null;
                SelectedTask.NewScore = null;
            }
            SetTotalScore();
            await AdaptChanges();
            await base.UntapTaskAsync();
        }

        /// <summary>
        /// Set the task sore and sets status to Todo.
        /// </summary>
        /// <param name="score">The score value.</param>
        private async Task SetTaskScore(object score)
        {
            IsScorePopupOpen = false;

            if (SelectedTask == null)
            {
                SelectedTask = TaskTemplates.FirstOrDefault();
                CurrentIndex = 0;
            }

            if (score is int result)
            {
                CurrentScore = result;
                if (SelectedTask.HasPictureProof)
                {
                    if (SelectedTask.Score == CurrentScore)
                    {
                        await OpenUntapTaskDialogAsync();
                        return;
                    }
                    else if (SelectedTask.Score.HasValue && SelectedTask.Score != CurrentScore)
                    {
                        OpenChangeStatusPopup();
                        return;
                    }

                    using var scope = App.Container.CreateScope();
                    var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();
                    pictureProofViewModel.SelectedTaskTemplate = SelectedTask;
                    pictureProofViewModel.Score = result;
                    pictureProofViewModel.IsNew = true;
                    await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
                    return;
                }

                await SetSelectedTaskScore(result);
            }
        }

        private async Task SetSelectedTaskScore(int score)
        {
            TaskHelper.SetTaskScore(SelectedTask, score);

            // Audits currently don't support task signatures - REMOVE WHEN ADDED SUPPORT ON BACKEND!
            SelectedTask.Signature = null;

            ScoreModel NewScore = new ScoreModel { Number = score, MinimalScore = AuditTemplate.MinScore, NumberOfScores = Math.Abs(AuditTemplate.MaxScore - AuditTemplate.MinScore + 1) };
            SelectedTask.NewScore = NewScore;

            if (OpenFields != null)
                await OpenFields.AdaptChanges(_auditService);

            SetTotalScore();
            CurrentScore = null;
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
                    score = (int)Math.Round(TaskTemplates.Select(x => (x.Weight * x.Score ?? 0)).Sum());
                    break;
                default: //thumbs
                    score = (int)Math.Round(TaskTemplates.Where(x => x.FilterStatus == (TaskStatusEnum.Ok)).Select(x => (x.Weight * maxAuditScore)).Sum());
                    break;
            }
            if (maxScore > 0) { Score = (int)Math.Round((decimal)(100 * score) / maxScore); }
        }

        /// <summary>
        /// Filters the task templates.
        /// </summary>
        /// <param name="status">Status to filter on.</param>
        private void FilterTaskTemplates(TaskStatusEnum status)
        {
            LinkedList<BasicTaskTemplateModel> filteredTasks;

            if (StatusFilter != null && StatusFilter == status)
            {
                FilteredTaskTemplates.Clear();
                filteredTasks = new LinkedList<BasicTaskTemplateModel>(TaskTemplates);
                StatusFilter = null;
            }
            else
            {
                filteredTasks = new LinkedList<BasicTaskTemplateModel>(TaskTemplates.Where(item => item.FilterStatus == status));
                FilteredTaskTemplates.Clear();
                StatusFilter = status;
            }

            AddSignButtonToTaskTemplates(filteredTasks);
            AddOpenFieldsToChecklist(filteredTasks);

            FilteredTaskTemplates = new ObservableCollection<BasicTaskTemplateModel>(filteredTasks);

            OnPropertyChanged(nameof(FilteredTaskTemplates));
        }

        #endregion

        public override async Task SubmitSkipCommandAsync()
        {
            SelectedTask.PictureProofMediaItems = new List<MediaItem>();
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

        private void SetSelectedTaskStatus(TaskStatusEnum status)
        {
            TaskHelper.SetTaskStatusAsync(SelectedTask, status, null);

            // Audits currently don't support task signatures - REMOVE WHEN ADDED SUPPORT ON BACKEND!
            SelectedTask.Signature = null;

            if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped)
                SelectedTask.ResetValidation();

            if (SelectedTask.FilterStatus == TaskStatusEnum.Skipped && (CurrentIndex + 1) < FilteredTaskTemplates.Count)
                CurrentIndex++;

            CurrentStatus = null;

            // set Score to null
            SelectedTask.Score = null;

            //SetCanSignAudit();
            SetTotalScore();
        }

        private async Task AdaptChanges()
        {
            await OpenFields.AdaptChanges(_auditService);
        }

        #region Navigate

        private async Task NavigateToSignPageOrFinishAuditAsync()
        {
            if (OpenFields.TasksDone)
            {
                if (IsSignatureRequired)
                {
                    using var scope = App.Container.CreateScope();
                    var auditSignViewModel = scope.ServiceProvider.GetService<AuditSignViewModel>();

                    auditSignViewModel.AuditTemplateId = AuditTemplate.Id;
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

                    var model = new PostTemplateModel(AuditTemplate.Id, AuditTemplate.Name, OpenFields.PropertyList, TaskTemplates, DeepLinkCompletionIsRequired, TaskFromDeepLink?.Id, version: AuditTemplate.Version);
                    model.StartedAt = OpenFields.StartedAt;

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

                    await _auditService.PostAuditAsync(model);

                    IsBusy = false;
                }
            }
            else
            {
                OpenFields.ShowInfoDialog(NavigationService.GetCurrentPage());
            }
        }

        private async Task NavigateToDetailAsync(BasicTaskTemplateModel selectedTask)
        {
            using var scope = App.Container.CreateScope();
            var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();

            // Items without openfields and sign button
            var items = FilteredTaskTemplates.Where(x => x.Id > 0).ToList();
            itemsDetailViewModel.Items = new List<Interfaces.Utils.IDetailItem>(items);
            itemsDetailViewModel.SelectedItem = selectedTask;
            itemsDetailViewModel.SenderClassName = nameof(AuditSlideViewModel);
            await NavigationService.NavigateAsync(viewModel: itemsDetailViewModel);
        }

        private async Task NavigateToMoreInfoAsync()
        {
            using var scope = App.Container.CreateScope();
            if (SelectedTask.HasWorkInstructions)
                await NavigateToWorkInstructions(SelectedTask.WorkInstructionRelations);

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
        }

        private async Task OpenPopupOrNavigateToActionsAsync()
        {
            if (SelectedTask.ActionBubbleCount > 0)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskTemplateId = SelectedTask.Id;
                actionOpenActionsViewModel.ActionType = ActionType.Audit;
                actionOpenActionsViewModel.LocalTask = SelectedTask;
                actionOpenActionsViewModel.TaskTitle = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forAuditItem)} {SelectedTask.Name}";

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
            actionNewViewModel.ActionType = ActionType.Audit;
            actionNewViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forAuditItem)} {SelectedTask.Name}";
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
            taskCommentEditViewModel.Title = $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.forAuditItem)} {SelectedTask.Name}";
            taskCommentEditViewModel.Type = ActionType.Audit;

            await NavigationService.NavigateAsync(viewModel: taskCommentEditViewModel);
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

        #endregion

        #region Popup

        private void OpenFeaturePopup(BasicTaskPropertyModel prop)
        {
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
                    propertyValue.CreatedAt = DateTime.UtcNow;
                    propertyValue.ModifiedAt = propertyValue.CreatedAt;
                    SelectedTask.PropertyValues.Add(propertyValue);
                }
                else
                {
                    propertyValue.ModifiedAt = DateTime.UtcNow;
                }

                await OpenFields.SaveLocalChanges(_auditService);

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

        public override Task CancelAsync()
        {
            MainThread.BeginInvokeOnMainThread(() => { MessagingCenter.Send(this, Constants.RecalculateAmountsMessage); });

            return base.CancelAsync();
        }

    }
}
