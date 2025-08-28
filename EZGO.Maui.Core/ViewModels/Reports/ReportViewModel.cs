using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Reports;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Areas;
using EZGO.Maui.Core.Models.Messaging;
using EZGO.Maui.Core.Models.Reports;
using EZGO.Maui.Core.Services.Message;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Reports.Stats;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private readonly IWorkAreaService _workAreaService;
        private readonly IReportService _reportService;

        private List<TaskStats> statsList = new List<TaskStats>();

        public Rect Rect { get; set; } = new Rect(113, .2, .4, .6);

        public bool IsBusy { get; set; } = true;

        public TasksStatsViewModel TasksStats { get; set; }

        public ChecklistStatsViewModel ChecklistStats { get; set; }

        public AuditStatsViewModel AuditStats { get; set; }

        public ActionStatsViewModel ActionStats { get; set; }

        #region Areas
        public List<BasicWorkAreaModel> WorkAreas { get; set; }

        public List<BasicWorkAreaModel> FlattenedWorkAreas { get; set; }

        public BasicWorkAreaModel SelectedWorkArea { get; set; }

        #endregion

        #region Commands
        public ICommand NavigateToTaskReportCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToAsync<TaskReportViewModel>();
            });
        }, CanExecuteCommands);

        public ICommand NavigateToChecklistReportCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToAsync<ChecklistReportViewModel>();
            });
        }, CanExecuteCommands);

        public ICommand NavigateToAuditReportCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToAsync<AuditReportViewModel>();
            });
        }, CanExecuteCommands);

        public ICommand NavigateToActionReportCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToAsync<ActionReportViewModel>();
            });
        }, CanExecuteCommands);

        public ICommand DropdownTapCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () => { await DropdownTapAsync(obj); });
        }, CanExecuteCommands);

        #endregion

        public ReportViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IReportService reportService,
            IWorkAreaService workAreaService) : base(navigationService, userService, messageService, actionsService)
        {
            _reportService = reportService;
            _workAreaService = workAreaService;
        }

        ~ReportViewModel()
        { // Breakpoint here
        }

        public override async Task Init()
        {
            InitReportModels();

            Settings.SubpageReporting = MenuLocation.None;

            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionReportsUnavailable);
                _messageService?.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
            }
            else
            {
                await Task.Run(async () =>
                {
                    await LoadWorkAreasAsync();
                    await FillStats();
                });
            }

            MessagingCenter.Subscribe<MessageService, Message>(this, Constants.MessageCenterMessage, async (formsApp, message) =>
            {
                if (message?.MessageType == MessageTypeEnum.Clear)
                {
                    await LoadWorkAreasAsync();
                    await FillStats();
                }
                else
                {
                    IsBusy = true;
                }
            });

            SubscribeToDoAction<TaskReportViewModel>(Constants.ReportAreaChanged);
            SubscribeToDoAction<ChecklistReportViewModel>(Constants.ReportAreaChanged);
            SubscribeToDoAction<AuditReportViewModel>(Constants.ReportAreaChanged);
            SubscribeToDoAction<ActionReportViewModel>(Constants.ReportAreaChanged);

            await base.Init();
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<MessageService, Message>(this, Constants.MessageCenterMessage);
            });
            UnsubscribeToDoAction<TaskReportViewModel>(Constants.ReportAreaChanged);
            UnsubscribeToDoAction<ChecklistReportViewModel>(Constants.ReportAreaChanged);
            UnsubscribeToDoAction<AuditReportViewModel>(Constants.ReportAreaChanged);
            UnsubscribeToDoAction<ActionReportViewModel>(Constants.ReportAreaChanged);
            _reportService.Dispose();
            _workAreaService.Dispose();
            base.Dispose(disposing);
        }

        private void InitReportModels()
        {
            AuditStats = new AuditStatsViewModel(true);
            ChecklistStats = new ChecklistStatsViewModel(true);
            ActionStats = new ActionStatsViewModel(true);
            TasksStats = new TasksStatsViewModel(true);
        }

        private async Task ForwardToSubpage()
        {
            switch (Settings.SubpageReporting)
            {
                case MenuLocation.ReportActions:
                    await NavigateToAsync<ActionReportViewModel>();
                    return;
                case MenuLocation.ReportAudits:
                    await NavigateToAsync<AuditReportViewModel>();
                    return;
                case MenuLocation.ReportChecklists:
                    await NavigateToAsync<ChecklistReportViewModel>();
                    return;
                case MenuLocation.ReportTasks:
                    await NavigateToAsync<TaskReportViewModel>();
                    return;
            }
        }

        private void SubscribeToDoAction<T>(string messageType) where T : class
        {
            MessagingCenter.Subscribe<T>(this, messageType, async (sender) =>
            {
                GetSelectedArea();

                await FillStats();
            });
        }

        private void UnsubscribeToDoAction<T>(string messageType) where T : class
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<T>(this, messageType);
            });
        }

        protected override async Task RefreshAsync()
        {
            await FillStats(false);
        }

        #region Parallel Tasks

        private List<ReportsCount> myresults;

        private async Task GetMyStatisticsAsync() => myresults = await _reportService.GetMyStatisticsAsync(refresh: true);

        private async Task RunTasks()
        {
            var tasks = new List<Task>
            {
                ChecklistStats.FillStats(statsList, myresults),
                AuditStats.FillStats(statsList, myresults),
                TasksStats.FillStats(statsList, myresults),
                ActionStats.FillStats(statsList, myresults),
            };

            await Task.WhenAll(tasks);
        }

        #endregion

        private async Task FillStats(bool busy = true)
        {
            IsBusy = busy;
            await GetMyStatisticsAsync();
            await RunTasks();

            CalculateStats();
            IsBusy = false;
        }

        private void CalculateStats()
        {
            statsList.ForEach(x =>
            {
                var total = (x.Ok + x.NotOk + x.Skipped + x.Todo);
                x.Total = total;
                if (total > 0)
                {
                    x.PercentageOk = Math.Round(((double)x.Ok / x.Total) * 100, 2);
                    x.PercentageNotOk = Math.Round(((double)x.NotOk / x.Total) * 100, 2);
                    x.EndNotOk = Math.Round(x.PercentageOk + x.PercentageNotOk, 2);
                    x.PercentageSkipped = Math.Round(((double)x.Skipped / x.Total) * 100, 2);
                    x.EndSkipped = Math.Round(x.EndNotOk + x.PercentageSkipped, 2);
                    x.PercentageTodo = Math.Round(((double)x.Todo / x.Total) * 100, 2);
                }
                else
                {
                    x.PercentageTodo = 100;
                }

                SetTitle(x);
            });
        }

        private void SetTitle(TaskStats x)
        {
            switch (x.Title)
            {
                case "ActionsOpen":
                    ActionStats.ActionsOpen = x;
                    break;
                case "Checklists30Day":
                    ChecklistStats.Checklists30Day = x;
                    break;
                case "Checklists7Day":
                    ChecklistStats.Checklists7Day = x;
                    break;
                case "ChecklistsToday":
                    ChecklistStats.ChecklistsToday = x;
                    break;
                case "Tasks30Day":
                    TasksStats.Tasks30Day = x;
                    break;
                case "Tasks7Day":
                    TasksStats.Tasks7Day = x;
                    break;
                case "TasksToday":
                    TasksStats.TasksToday = x;
                    break;
            }
        }

        #region Navigation

        private async Task NavigateToAsync<T>() where T : BaseViewModel
        {
            using var scope = App.Container.CreateScope();
            var reportSection = scope.ServiceProvider.GetService<T>();
            var areaRaport = reportSection as IReportArea;
            areaRaport.WorkAreas = WorkAreas;
            areaRaport.FlattenedWorkAreas = FlattenedWorkAreas;

            await NavigationService.NavigateAsync(viewModel: areaRaport as T);
        }

        //private async Task NavigateToChecklistReportAsync()
        //{
        //    using var scope = App.Container.CreateScope();
        //    var checklistReportViewModel = scope.ServiceProvider.GetService<ChecklistReportViewModel>();
        //    checklistReportViewModel.WorkAreas = WorkAreas;
        //    checklistReportViewModel.FlattenedWorkAreas = FlattenedWorkAreas;
        //    await NavigationService.NavigateAsync(viewModel: checklistReportViewModel);
        //}

        //private async Task NavigateToAuditReportAsync()
        //{
        //    using var scope = App.Container.CreateScope();
        //    var actionNewViewModel = scope.ServiceProvider.GetService<AuditReportViewModel>();
        //    await NavigationService.NavigateAsync(viewModel: new AuditReportViewModel
        //    {
        //        WorkAreas = WorkAreas,
        //        FlattenedWorkAreas = FlattenedWorkAreas
        //    });
        //}

        //private async Task NavigateToActionReportAsync()
        //{
        //    using var scope = App.Container.CreateScope();
        //    var actionNewViewModel = scope.ServiceProvider.GetService<ActionReportViewModel>();
        //    await NavigationService.NavigateAsync(viewModel: new ActionReportViewModel
        //    {
        //        WorkAreas = WorkAreas,
        //        FlattenedWorkAreas = FlattenedWorkAreas
        //    });
        //}

        #endregion

        private async Task LoadWorkAreasAsync()
        {
            WorkAreas ??= await _workAreaService.GetBasicWorkAreasAsync();

            FlattenedWorkAreas ??= _workAreaService.GetFlattenedBasicWorkAreas(WorkAreas);

            if (WorkAreas.Count() > 6)
                Rect = new Rect(113, .8, .4, .9);
            else
                Rect = new Rect(113, .2, .4, .6);

            OnPropertyChanged(nameof(WorkAreas));

            GetSelectedArea();
        }

        private void GetSelectedArea()
        {
            SelectedWorkArea = FlattenedWorkAreas.FirstOrDefault(x => x.Id == ((Settings.ReportWorkAreaId != 0) ? Settings.ReportWorkAreaId : Settings.WorkAreaId));
            SelectedWorkArea ??= WorkAreas.FirstOrDefault();
        }

        private async Task DropdownTapAsync(object obj)
        {
            IsDropdownOpen = false;
            if ((obj as Syncfusion.TreeView.Engine.TreeViewNode).Content is BasicWorkAreaModel workAreaModel)
            {
                if (workAreaModel.Id != Settings.ReportWorkAreaId)
                {
                    Settings.ReportWorkAreaId = workAreaModel.Id;
                    SelectedWorkArea = workAreaModel;

                    //Without these calls the audits report bars are not aligned correctly
                    await _reportService.GetAuditsCountAsync(refresh: IsRefreshing);
                    await _reportService.GetAuditsAverageAsync(refresh: IsRefreshing);

                    InitReportModels();
                    await FillStats();
                }

            }
        }

        /// <summary>
        /// Called by Fody when <see cref="SelectedWorkArea"/> changes.
        /// </summary>
        private void OnSelectedWorkAreaChanged()
        {
            if (SelectedWorkArea != null)
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.reportOnArea);
                Title = string.Format(result.ReplaceLanguageVariablesCumulative(), SelectedWorkArea.Name);
            }
        }
    }
}
