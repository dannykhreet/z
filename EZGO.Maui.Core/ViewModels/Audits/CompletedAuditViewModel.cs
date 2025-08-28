using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Audits;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Audits;
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
    public class CompletedAuditViewModel : BaseViewModel
    {
        private const string _cat = "[CompletedAudits]\n\t";
        private readonly IAuditsService _auditService;
        private readonly ITaskCommentService _commentService;
        private List<AuditsModel> audits;
        private DateTime? startTimestamp, endTimesttamp;
        private CancellationTokenSource generatePdfCancelationTokenSource;
        private LocalDateTime currentTimeStamp => Settings.AppSettings.CompletedAudtisTimestamp;
        private int currentOffset = 0;
        public const int Limit = 10;

        #region Public Properties

        /// <summary>
        /// Filter option from reporting
        /// </summary>
        public TimespanTypeEnum? Period;

        /// <summary>
        /// Template Id from reports
        /// </summary>
        public int ReportAuditTemplateId { private get; set; }

        /// <summary>
        /// Custom option to determine the reach coming from reporting
        /// </summary>
        public ReportsCount Report;

        public ScoreTypeEnum ScoreType { get; set; }

        public bool Thumbs { get; set; }

        public bool Score { get; set; }

        public AuditsModel SelectedAudit { get; set; }

        public List<TasksTaskModel> SelectedAuditTasks { get; set; }

        public List<SignatureModel> SelectedAuditSignatures { get; set; }

        public int SelectedAuditTotalScore { get; set; }

        public ObservableCollection<AuditsModel> CompletedAudits { get; set; }

        /// <summary>
        /// Indicates if PDF loading action is in progress
        /// </summary>
        public bool IsLoadingPdf { get; set; }

        /// <summary>
        /// Indicates if the load more is busy
        /// </summary>
        public bool LoadMoreIsBusy { get; set; }


        /// <summary>
        /// Indicated if the load more option should be available.
        /// In other words if there are more item available to load.
        /// </summary>
        public bool CanLoadMore { get; set; }

        public ChecklistOpenFields OpenFields { get; set; }

        public bool ContainsTags => SelectedAudit?.Tags?.Count > 0;

        [DependsOn(nameof(SelectedAudit))]
        public bool HasItem => SelectedAudit != null;

        public bool IsFromDeepLink { get; set; } = false;
        public int? AuditIdFromDeepLink { get; set; } = null;
        public long? LinkedTaskId { get; set; } = null;

        #endregion

        #region Commands

        public ICommand AuditSelectedCommand => new Command<object>(obj => ExecuteLoadingAction(() => AuditSelected(obj)), CanExecuteCommands);

        public ICommand NavigateToActionsCommand => new Command<object>(obj => ExecuteLoadingAction(async () => await NavigateToActionsAsync(obj)), CanExecuteCommands);

        public ICommand NavigateToDetailsCommand => new Command<object>(
           obj => ExecuteLoadingAction(async () => await NavigateToDetailsAsync(obj)),
           (obj) => !IsRefreshing);

        public ICommand NavigateToPictureProofDetailsCommand => new Command<object>(
      obj => ExecuteLoadingAction(async () => await NavigateToPictureProofDetailsAsync(obj)),
      (obj) => !IsRefreshing);


        public IAsyncRelayCommand GeneratePdfCommand => new AsyncRelayCommand(async () =>
        {
            IsLoadingPdf = true;
            await GeneratePdfAsync(generatePdfCancelationTokenSource.Token);
        }, () => !IsLoadingPdf && !LoadMoreIsBusy);

        public ICommand LoadMoreCommand => new Command(async () => await LoadMoreAsync(), () => !LoadMoreIsBusy && !IsLoadingPdf);

        protected override void RefreshCanExecute()
        {
            (AuditSelectedCommand as Command)?.ChangeCanExecute();
            (NavigateToActionsCommand as Command)?.ChangeCanExecute();
            (NavigateToDetailsCommand as Command)?.ChangeCanExecute();
            base.RefreshCanExecute();
        }

        #endregion

        public CompletedAuditViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IAuditsService auditsService,
            ITaskCommentService taskCommentService) : base(navigationService, userService, messageService, actionsService)
        {
            _auditService = auditsService;
            _commentService = taskCommentService;
        }

        public override async Task Init()
        {
            generatePdfCancelationTokenSource = new CancellationTokenSource();
            OpenFields = new ChecklistOpenFields(fieldsReadonly: true, 30);
            if (IsFromDeepLink)
            {
                await Task.Run(async () => await LoadAuditFromDeepLinkAsync(AuditIdFromDeepLink));
            }
            else
            {
                await Task.Run(async () => await LoadCompletedAuditsAsync());
            }
            SelectAudit(CompletedAudits?.FirstOrDefault());
            await base.Init();
        }

        private async Task LoadAuditFromDeepLinkAsync(int? id)
        {
            if (!id.HasValue)
                return;

            // Local audit - not yet on backend
            if (id == -1 && LinkedTaskId != null)
            {
                audits = await _auditService.GetAuditsAsync(isComplete: true, refresh: false, limit: Limit, offset: currentOffset, timeStamp: currentTimeStamp);
                audits = audits.Where(x => x.LinkedTaskId == LinkedTaskId).ToList();
            }
            else
            {
                audits = await _auditService.GetAuditAsync(id.Value);
            }

            CompletedAudits ??= new ObservableCollection<AuditsModel>(audits);
        }

        private async Task LoadCompletedAuditsAsync()
        {
#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
            Debug.WriteLine("Started loading Completed Audits", _cat);
#endif
            if (Period == null)
            {
                if (await InternetHelper.HasInternetConnection())
                {
                    Settings.CompletedAudtisTimestamp = DateTimeHelper.Now;
                    audits = await _auditService.GetAuditsAsync(isComplete: true, refresh: true, limit: Limit, offset: currentOffset, timeStamp: currentTimeStamp);
                }
                else
                {
                    audits = await _auditService.GetAuditsAsync(isComplete: true, refresh: IsRefreshing, limit: Limit, offset: currentOffset, timeStamp: currentTimeStamp);
                }
            }
            else
            {
                SetStartAndEndTimeStamps();
                audits = await _auditService.GetReportAuditsAsync(startTimeStamp: Settings.ConvertDateTimeToLocal(startTimestamp.Value), endTimeStamp: Settings.ConvertDateTimeToLocal(endTimesttamp.Value), refresh: IsRefreshing, limit: Limit, offset: currentOffset, timeStamp: currentTimeStamp, auditTemplateId: ReportAuditTemplateId);
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[Loaded {audits?.Count} completed audtis], {st.ElapsedMilliseconds} ms", _cat);
            var lastElapsed = st.ElapsedMilliseconds;
#endif
            CanLoadMore = audits.Count >= Limit;

            if (!audits.IsNullOrEmpty())
            {

                audits = audits.OrderByDescending(item => item.LocalSignedAt).ToList();
                CompletedAudits = new ObservableCollection<AuditsModel>(audits);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"[Created new Observable Collection of Audits], {st.ElapsedMilliseconds - lastElapsed} ms", _cat);
                lastElapsed = st.ElapsedMilliseconds;
#endif
            }
        }

        private void SelectAudit(AuditsModel selected)
        {
            if (selected != null)
            {
                SelectedAudit = selected;
                SetTasksSignaturesAndTotalScore();
            }
        }

        private async Task LoadMoreAsync()
        {
            LoadMoreIsBusy = true;
            currentOffset += Limit;

            List<AuditsModel> newAudtis;
            if (Period == null)
            {
                newAudtis = await _auditService.GetAuditsAsync(isComplete: true, offset: currentOffset, limit: Limit, timeStamp: currentTimeStamp);
            }
            else
            {
                newAudtis = await _auditService.GetReportAuditsAsync(startTimeStamp: Settings.ConvertDateTimeToLocal(startTimestamp.Value), endTimeStamp: Settings.ConvertDateTimeToLocal(endTimesttamp.Value), limit: Limit, offset: currentOffset, timeStamp: currentTimeStamp, auditTemplateId: ReportAuditTemplateId);
            }

            newAudtis = newAudtis.OrderByDescending(item => item.ModifiedAt).ToList();
            audits.AddRange(newAudtis);
            newAudtis.ForEach(x => CompletedAudits.Add(x));
            CanLoadMore = newAudtis.Count >= Limit;
            LoadMoreIsBusy = false;

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[Load more completed]: Loaded {newAudtis?.Count} new audtis and currently total count is {audits?.Count}");
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

        private void AuditSelected(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is AuditsModel audit)
            {
                if (audit == SelectedAudit || IsLoadingPdf) return;
                SelectAudit(audit);
            }
        }

        private async void SetTasksSignaturesAndTotalScore()
        {
            OpenFields.SetPropertyValues(SelectedAudit);

            ScoreType = (ScoreTypeEnum)Enum.Parse(typeof(ScoreTypeEnum), SelectedAudit.ScoreType ?? "thumbs", true);

            Thumbs = ScoreType == ScoreTypeEnum.Thumbs;
            Score = ScoreType == ScoreTypeEnum.Score;

            int maxAuditScore = 0;
            int minAuditScore = 0;
            if (ScoreType == ScoreTypeEnum.Score)
            {
                maxAuditScore = SelectedAudit.MaxTaskScore ?? 10;
                minAuditScore = SelectedAudit.MinTaskScore ?? 1;
            }

            SelectedAudit.Tasks?.ForEach(task =>
            {
                if (ScoreType == ScoreTypeEnum.Score)
                {
                    int percentage = (int)Math.Round((double)(100 * (((task.Score) > 0) ? task.Score : minAuditScore)) / maxAuditScore);
                    task.Percentage = percentage;
                    task.NewScore = new ScoreModel { Number = task.Score ?? 0, MinimalScore = minAuditScore, NumberOfScores = Math.Abs(maxAuditScore - minAuditScore + 1) };
                }
                task.ScoreType = ScoreType;
            });

            ScoreType = ScoreType;
            SelectedAuditTasks = SelectedAudit.Tasks;
            SelectedAuditSignatures = SelectedAudit.Signatures;
            SelectedAuditTotalScore = SelectedAudit.TotalScore;

            SelectedAuditTasks ??= new List<TasksTaskModel>();
            HasItems = SelectedAuditTasks.Any();
        }

        protected override async Task RefreshAsync()
        {
            Settings.CompletedAudtisTimestamp = currentTimeStamp;
            currentOffset = 0;

            await LoadCompletedAuditsAsync();

            SelectedAudit = null;
            SelectedAuditTasks = null;
            SelectedAuditSignatures = null;
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

        private async Task GeneratePdfAsync(CancellationToken token)
        {
            //Record for AppCenter analytics
            // Analytics.TrackEvent("Complete audit PDF", new Dictionary<string, string>() {
            //     { "Company", string.Format("{0} ({1})", UserSettings.CompanyName.ToString(), UserSettings.CompanyId.ToString()) },
            //     { "Role", UserSettings.Role.ToString() }
            // });

            foreach (var task in SelectedAudit.Tasks)
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
                if (DeviceInfo.Platform.Equals(DevicePlatform.Android))
                {
                    bool isAllowed = await CheckPermissions();

                    if (!isAllowed)
                    {
                        IsLoadingPdf = false;
                        return;
                    }
                }

                var template = new CompletedAuditTemplate { Model = SelectedAudit };
                string html = template.GenerateString();

                IPdfService pdfService = DependencyService.Get<IPdfService>();

                string pdfFilename = string.Empty;

                string datetimepart = DateTimeHelper.Now.ToString(Constants.PdfNameDateTimeFormat, null);
                string idpart = SelectedAudit.Id.ToString();
                pdfFilename = pdfService.SaveHtmlToPdf(html, $"audit_{datetimepart}_{idpart}", async () =>
                {
                    if (token.IsCancellationRequested)
                        return;
                    using var scope = App.Container.CreateScope();
                    var checklistPdfViewModel = scope.ServiceProvider.GetService<ChecklistPdfViewModel>();
                    checklistPdfViewModel.PdfFilename = pdfFilename;
                    await NavigationService.NavigateAsync(viewModel: checklistPdfViewModel);
                    IsLoading = false;
                    IsLoadingPdf = false;
                    RefreshCanExecute();

                });
            });
        }

        private static void SetTaskLocalComments(TasksTaskModel task)
        {
            var commentsList = new List<Comment>();

            task.LocalComments.ForEach(c => commentsList.Add(c.ToApiModel()));

            task.Comments = commentsList;
        }

        private static void SetTaskLocalActions(TasksTaskModel task)
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

        private async Task NavigateToDetailsAsync(object obj)
        {
            if (obj is ItemTappedEventArgs args && args.DataItem is TasksTaskModel item)
            {
                using var scope = App.Container.CreateScope();
                var taskInfoViewModel = scope.ServiceProvider.GetService<TaskInfoViewModel>();
                taskInfoViewModel.Task = new Tasks.CompletedTasks.CompletedTaskListItemViewModel(item);
                taskInfoViewModel.ActionType = ActionType.CompletedChecklistOrAudit;


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
            _auditService.Dispose();
            base.Dispose(disposing);
        }
    }
}
