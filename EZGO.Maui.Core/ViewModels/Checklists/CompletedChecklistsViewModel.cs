using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Stages;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.PdfTemplates;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Shared;
using MvvmHelpers;
using NodaTime;
using PropertyChanged;
using Syncfusion.Maui.DataSource.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    /// <summary>
    /// Checklist templates view model.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.ViewModels.BaseViewModel" />
    /// <summary>
    /// Checklist templates view model.
    /// </summary>
    /// <seealso cref="EZGO.Core.ViewModels.BaseViewModel" />
    public class CompletedChecklistsViewModel : BaseViewModel
    {
        private readonly IChecklistService _checklistService;
        private readonly ITaskCommentService _commentService;
        private List<ChecklistModel> checklists;
        private LocalDateTime currentTimeStamp => Settings.AppSettings.CompletedChecklistsTimestamp;
        private DateTime? startTimestamp, endTimesttamp;
        private int currentOffset = 0;
        private CancellationTokenSource generatePdfCancelationTokenSource;
        public const int Limit = 8;

        #region Public Properties

        /// <summary>
        /// Filter option from reporting
        /// </summary>
        public TimespanTypeEnum? Period;

        /// <summary>
        /// Custom option to determine the reach coming from reporting
        /// </summary>
        /// 
        public ReportsCount Report;

        [DependsOn(nameof(SelectedChecklist))]
        public bool HasItem => SelectedChecklist != null;

        public ChecklistModel SelectedChecklist { get; set; }

        public ObservableCollection<ChecklistModel> CompletedChecklists { get; set; }

        public ChecklistOpenFields SelectedOpenFields { get; set; }

        public List<TasksTaskModel> SelectedChecklistTasks { get; set; }

        public List<SignatureModel> SelectedChecklistSignatures { get; set; }

        public CompletedStagesControl Stages { get; set; }

        /// <summary>
        /// Indicates if the load more is busy
        /// </summary>
        public bool LoadMoreIsBusy { get; set; }

        /// <summary>
        /// Indicates if PDF loading action is in progress
        /// </summary>
        public bool IsLoadingPdf { get; set; }

        /// <summary>
        /// Indicated if the load more option should be available.
        /// In other words if there are more items available to load.
        /// </summary>
        public bool CanLoadMore { get; set; }

        public bool ContainsTags => SelectedChecklist?.Tags?.Count > 0;

        public bool IsFromDeepLink { get; set; } = false;
        public int? ChecklistIdFromDeepLink { get; set; } = null;
        public long? LinkedTaskId { get; set; } = null;

        #endregion

        #region Commands 

        public IAsyncRelayCommand GeneratePdfCommand => new AsyncRelayCommand(async () =>
        {
            IsLoadingPdf = true;
            await GeneratePdfAsync(generatePdfCancelationTokenSource.Token);
        }, () => !IsLoadingPdf && !LoadMoreIsBusy);

        public IAsyncRelayCommand<object> ChecklistSelectedCommand { get; }
        public ICommand NavigateToActionsCommand => new Command<object>(
            obj => ExecuteLoadingAction(async () => await NavigateToActionsAsync(obj)),
            (obj) => !IsRefreshing);

        public ICommand NavigateToDetailsCommand => new Command<object>(
            obj => ExecuteLoadingAction(async () => await NavigateToDetailsAsync(obj)),
            (obj) => !IsRefreshing);

        public ICommand NavigateToPictureProofDetailsCommand => new Command<object>(
           obj => ExecuteLoadingAction(async () => await NavigateToPictureProofDetailsAsync(obj)),
           (obj) => !IsRefreshing);

        public ICommand LoadMoreCommand => new Command(async () => await LoadMoreAsync(), () => !LoadMoreIsBusy && !IsLoadingPdf);

        protected override void RefreshCanExecute()
        {
            (ChecklistSelectedCommand as Command)?.ChangeCanExecute();
            (NavigateToActionsCommand as Command)?.ChangeCanExecute();
            (NavigateToDetailsCommand as Command)?.ChangeCanExecute();
            (NavigateToPictureProofDetailsCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        #endregion

        public CompletedChecklistsViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IChecklistService checklistService,
            ITaskCommentService taskCommentService) : base(navigationService, userService, messageService, actionsService)
        {
            _checklistService = checklistService;
            _commentService = taskCommentService;
            ChecklistSelectedCommand = new AsyncRelayCommand<object>(async (obj) => await ExecuteLoadingAction(async () => await ChecklistSelectedAsync(obj)), CanExecuteCommands);
        }

        public override async Task Init()
        {
            generatePdfCancelationTokenSource = new CancellationTokenSource();
            SelectedOpenFields = new ChecklistOpenFields(fieldsReadonly: true, 40);

            if (IsFromDeepLink)
            {
                await Task.Run(async () =>
                {
                    await LoadChecklistFromDeepLinkAsync(ChecklistIdFromDeepLink).ConfigureAwait(false);
                });
            }
            else
            {
                await Task.Run(async () =>
                {
                    await LoadCompletedChecklistsAsync().ConfigureAwait(false);
                });
            }
            SelectChecklist(CompletedChecklists?.FirstOrDefault());
            await base.Init();
        }

        private async Task LoadChecklistFromDeepLinkAsync(int? id)
        {
            if (!id.HasValue)
                return;

            // Local checklist - not yet on backend
            if (id == -1 && LinkedTaskId != null)
            {
                checklists = await _checklistService.GetChecklistsAsync(isComplete: true, limit: Limit, timeStamp: currentTimeStamp, refresh: false);
                checklists = checklists.Where(x => x.LinkedTaskId == LinkedTaskId).ToList();
            }
            else
            {
                var checklist = await _checklistService.GetChecklistAsync(id);
                checklists = new List<ChecklistModel> { checklist };
            }

            CompletedChecklists ??= new ObservableCollection<ChecklistModel>(checklists);
            CompletedChecklists.ForEach(c => c.SetupAfterLoaded());
        }

        private async Task LoadCompletedChecklistsAsync()
        {
            DebugService.Start(nameof(CompletedChecklistsViewModel));

            if (Period == null)
            {
                if (await InternetHelper.HasInternetConnection())
                {
                    Settings.CompletedChecklistsTimestamp = DateTimeHelper.Now;
                    checklists = await _checklistService.GetChecklistsAsync(isComplete: true, limit: Limit, timeStamp: currentTimeStamp, refresh: true, showLocalChecklists: false);
                }
                else
                {
                    checklists = await _checklistService.GetChecklistsAsync(isComplete: true, limit: Limit, timeStamp: currentTimeStamp, refresh: IsRefreshing);
                }
            }
            else
            {
                SetStartAndEndTimeStamps();
                checklists = await _checklistService.GetReportChecklistsAsync(startTimeStamp: startTimestamp, endTimeStamp: endTimesttamp, refresh: IsRefreshing, limit: Limit, offset: currentOffset);
            }

            DebugService.WriteLineWithTime($"[Loaded {checklists?.Count} completed checklists]", nameof(CompletedChecklistsViewModel));

            CanLoadMore = checklists.Count >= Limit;

            if (checklists != null && checklists.Any())
            {
                DebugService.WriteLineWithTime($"[Loaded actions for completed checklists]", nameof(CompletedChecklistsViewModel));

                checklists = checklists.OrderByDescending(item => item.LocalSignedAt).ToList();
                DebugService.WriteLineWithTime($"[Ordered By Descending]", nameof(CompletedChecklistsViewModel));

                CompletedChecklists = new ObservableCollection<ChecklistModel>(checklists);
            }

            CompletedChecklists ??= new ObservableCollection<ChecklistModel>();


            DebugService.WriteLineWithTime($"Finished loading checklist]", nameof(CompletedChecklistsViewModel));
            DebugService.Stop(nameof(CompletedChecklistsViewModel));
            CompletedChecklists.ForEach(c => c.SetupAfterLoaded());
        }

        private async Task LoadMoreAsync()
        {
            LoadMoreIsBusy = true;
            currentOffset += Limit;

            List<ChecklistModel> newChecklists;
            if (Period == null)
                newChecklists = await _checklistService.GetChecklistsAsync(isComplete: true, offset: currentOffset, limit: Limit, timeStamp: currentTimeStamp).ConfigureAwait(false);
            else
                newChecklists = await _checklistService.GetReportChecklistsAsync(startTimeStamp: startTimestamp, endTimeStamp: endTimesttamp, refresh: false, limit: Limit, offset: currentOffset).ConfigureAwait(false);

            newChecklists = newChecklists.OrderByDescending(item => item.ModifiedAt).ToList();
            newChecklists.ForEach(c => c.SetupAfterLoaded());
            checklists.AddRange(newChecklists);
            await CompletedChecklists.AddRange(newChecklists);
            CanLoadMore = newChecklists.Count >= Limit;
            LoadMoreIsBusy = false;

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[Load more completed]: Loaded {newChecklists?.Count} new checklists and currently total count is {checklists?.Count}");
#endif
        }

        private void SetStartAndEndTimeStamps()
        {
            if (Period != null)
            {
                switch (Period)
                {
                    case TimespanTypeEnum.LastTwelveDays:
                        if (Report != null)
                        {
                            startTimestamp = Report.ReportDate.Date;
                            endTimesttamp = Report.ReportDate.Date.AddDays(1).AddSeconds(-1);
                        }
                        else
                            startTimestamp = DateTime.Today.AddDays(-11);
                        break;
                    case TimespanTypeEnum.LastTwelveWeeks:
                        if (Report != null)
                        {
                            startTimestamp = Report.ReportDate.Date;
                            endTimesttamp = Report.ReportDate.Date.AddDays(7);
                        }
                        else
                            startTimestamp = DateTime.Today.AddDays(-83);
                        break;
                    case TimespanTypeEnum.LastTwelveMonths:
                        if (Report != null)
                        {
                            startTimestamp = Report.ReportDate.Date;
                            endTimesttamp = Report.ReportDate.Date.AddMonths(1);
                        }
                        else
                            startTimestamp = DateTime.Today.AddMonths(-11);
                        break;
                    case TimespanTypeEnum.ThisYear:
                        if (Report != null)
                        {
                            startTimestamp = Report.ReportDate.Date;
                            endTimesttamp = Report.ReportDate.Date.AddMonths(1);
                        }
                        else
                            startTimestamp = new DateTime(DateTime.Today.Year, 1, 1);
                        break;
                }
            }
        }

        private void SelectChecklist(ChecklistModel selectedChecklist)
        {
            if (selectedChecklist != null)
            {
                SelectedChecklist = selectedChecklist;
                SetTasksAndSignatures();
                Stages = new CompletedStagesControl(selectedChecklist.Stages, SelectedChecklistTasks);
                Stages.SetStages(addStageSign: false);
            }
        }

        private async Task ChecklistSelectedAsync(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is ChecklistModel checklist)
            {
                if (checklist == SelectedChecklist)
                    return;

                SelectedChecklist = null;
                await Task.Delay(50); // allow UI update

                // Load data off the UI thread
                await Task.Run(() =>
                {
                    checklist.SetupAfterLoaded(); // ensure checklist is initialized
                    var openFields = new ChecklistOpenFields(fieldsReadonly: true, 40);
                    openFields.SetPropertyValues(checklist);

                    var tasks = checklist.Tasks?.ToList() ?? new List<TasksTaskModel>();
                    var signatures = checklist.Signatures?.ToList() ?? new List<SignatureModel>();
                    var stagesControl = new CompletedStagesControl(checklist.Stages, tasks);
                    stagesControl.SetStages(addStageSign: false);

                    // Switch back to UI thread to apply changes
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SelectedChecklist = checklist;
                        SelectedOpenFields = openFields;
                        SelectedChecklistTasks = tasks;
                        SelectedChecklistSignatures = signatures;
                        Stages = stagesControl;
                    });
                });
            }
        }

        private const string _cat = "[PdfGen]:\n\t";

        private async Task GeneratePdfAsync(CancellationToken token)
        {
            SelectedChecklist.Tasks ??= new List<TasksTaskModel>();

            foreach (var task in SelectedChecklist.Tasks)
            {
                if (task.CommentCount > 0 && task.Id != 0)
                {
                    await _commentService.AddCommentsToTasksTaskAsync(task, true);
                }
                if (task.ActionsCount > 0 && task.Id != 0)
                {
                    await _actionService.LoadAssignedAreasForActionsAsync(task.Actions, true);
                    await _actionService.LoadAssignedUsersForActionsAsync(task.Actions, true);
                }

                if (task.Actions == null && task.LocalActions != null)
                    SetTaskLocalActions(task);

                if (task.Comments == null && task.LocalComments != null)
                    SetTaskLocalComments(task);
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    bool isAllowed = await CheckPermissions();

                    if (!isAllowed)
                    {
                        IsLoadingPdf = false;
                        return;
                    }
                }
                Debug.WriteLine("Checked permissions", _cat);
                SelectedChecklist.Stages = Stages.Stages;
                var template = new CompletedChecklistTemplate() { Model = SelectedChecklist };

                Debug.WriteLine("Loaded template", _cat);
                string html = template.GenerateString();

                Debug.WriteLine("Generated html", _cat);
                IPdfService pdfService = DependencyService.Get<IPdfService>();

                string pdfFilename = string.Empty;

                string datetimepart = DateTimeHelper.Now.ToString(Constants.PdfNameDateTimeFormat, null);
                string idpart = SelectedChecklist.Id.ToString();
                Debug.WriteLine("Setted FileName", _cat);
                pdfFilename = pdfService.SaveHtmlToPdf(html, $"checklist_{datetimepart}_{idpart}", async () =>
                        {
                            if (token.IsCancellationRequested)
                                return;
                            Debug.WriteLine("Saving Html To Pdf", _cat);
                            using var scope = App.Container.CreateScope();
                            var checklistPdfViewModel = scope.ServiceProvider.GetService<ChecklistPdfViewModel>();
                            checklistPdfViewModel.PdfFilename = pdfFilename;
                            await NavigationService.NavigateAsync(viewModel: checklistPdfViewModel);

                            Debug.WriteLine("Navigation ended", _cat);
                            IsLoadingPdf = false;
                        });
            });
        }

        private void SetTaskLocalComments(TasksTaskModel task)
        {
            var commentsList = new List<Comment>();

            task.LocalComments.ForEach(c => commentsList.Add(c.ToApiModel()));

            task.Comments = commentsList;
        }

        private void SetTaskLocalActions(TasksTaskModel task)
        {
            task.Actions = task.LocalActions;

            var localMediaUrls = new List<string>();

            foreach (var action in task.Actions)
            {
                action.LocalMediaItems.ForEach(m => localMediaUrls.Add(m.PictureUrl));
            }

            foreach (var action in task.Actions)
            {
                action.Images = localMediaUrls;
            }
        }

        /// <summary>
        /// Check permissions for writing and reading data in storage
        /// If any permission is denied then it displays alert
        /// </summary>
        /// <returns>Returns boolean depending on permission</returns>
        private async Task<bool> CheckPermissions()
        {
            // On android 13 (and possibly next ones as well) we don't need permission for reading storage
            if (DeviceInfo.Version.Major >= 13)
                return true;

            var isStorageReadPermissionGranted = await PermissionsHelper.CheckAndRequestPermissionAsync<Permissions.StorageRead>();

            if (!isStorageReadPermissionGranted)
            {
                string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuChooseMediaDialogCancel);
                string storageReadMessage = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionStorageMessage);
                string storageRead = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionStorage);

                Page page = NavigationService.GetCurrentPage();
                await page.DisplayAlert(storageRead, storageReadMessage, cancel);
            }
            return isStorageReadPermissionGranted;
        }

        private void SetTasksAndSignatures()
        {
            SelectedOpenFields.SetPropertyValues(SelectedChecklist);
            SelectedChecklistTasks = SelectedChecklist.Tasks;
            SelectedChecklistSignatures = SelectedChecklist.Signatures;
        }

        protected override async Task RefreshAsync()
        {
            Settings.CompletedChecklistsTimestamp = DateTimeHelper.Now;
            currentOffset = 0;

            await LoadCompletedChecklistsAsync();

            SelectedChecklist = null;
            SelectedChecklistTasks = null;
            SelectedChecklistSignatures = null;
            SelectedOpenFields.SetPropertyValues(null);

            CanLoadMore = true;
        }

        /// <summary>
        /// Navigates to related actions overview asynchronous.
        /// </summary>
        /// <param name="obj">Command object.</param>
        private async Task NavigateToActionsAsync(object obj)
        {
            if (obj is TasksTaskModel item)
            {
                using var scope = App.Container.CreateScope();
                var actionOpenActionsViewModel = scope.ServiceProvider.GetService<ActionOpenActionsViewModel>();
                actionOpenActionsViewModel.TaskId = item.Id;
                actionOpenActionsViewModel.TaskTemplateId = item.TemplateId;
                actionOpenActionsViewModel.ActionType = ActionType.CompletedChecklistOrAudit;


                if (item.LocalComments?.Any() ?? false)
                    actionOpenActionsViewModel.Comments = new ObservableRangeCollection<Models.Comments.CommentModel>(item.LocalComments);

                if (item.LocalActions?.Any() ?? false)
                    actionOpenActionsViewModel.Actions = new ObservableRangeCollection<Models.Actions.BasicActionsModel>(item.LocalActions.ToBasicList<BasicActionsModel, ActionsModel>());

                await NavigationService.NavigateAsync(viewModel: actionOpenActionsViewModel);
            }
        }

        private async Task NavigateToDetailsAsync(object obj)
        {
            if (obj is ItemTappedEventArgs args && args.DataItem is TasksTaskModel item)
            {
                using var scope = App.Container.CreateScope();
                var taskInfoViewModel = scope.ServiceProvider.GetService<TaskInfoViewModel>();
                taskInfoViewModel.Task = new Tasks.CompletedTasks.CompletedTaskListItemViewModel(item);
                taskInfoViewModel.ActionType = ActionType.CompletedChecklistOrAudit;
                taskInfoViewModel.Stage = Stages?.GetStage(item.StageId);

                if (item.LocalComments?.Any() ?? false)
                    taskInfoViewModel.LocalComments = item.LocalComments;

                if (item.LocalActions?.Any() ?? false)
                    taskInfoViewModel.LocalActions = new List<ActionsModel>(item.LocalActions);

                await NavigationService.NavigateAsync(viewModel: taskInfoViewModel);
            }
        }

        private async Task NavigateToPictureProofDetailsAsync(object obj)
        {
            if (obj is TasksTaskModel item && item.HasPictureProof)
            {
                using var scope = App.Container.CreateScope();
                var pictureProofViewModel = scope.ServiceProvider.GetService<PictureProofViewModel>();

                pictureProofViewModel.MainMediaElement = item.PictureProofMediaItems?.FirstOrDefault();

                if (item.PictureProofMediaItems?.Count > 1)
                    pictureProofViewModel.MediaElements = new ObservableCollection<MediaItem>(item.PictureProofMediaItems?.Skip(1));

                pictureProofViewModel.IsNew = false;
                pictureProofViewModel.EditingEnabled = false;
                pictureProofViewModel.SupportsEditing = false;

                await NavigationService.NavigateAsync(viewModel: pictureProofViewModel);
            }
        }


        protected override void Dispose(bool disposing)
        {
            generatePdfCancelationTokenSource.Cancel();
            generatePdfCancelationTokenSource.Dispose();
            _commentService.Dispose();
            _checklistService.Dispose();
            base.Dispose(disposing);
        }
    }
}
