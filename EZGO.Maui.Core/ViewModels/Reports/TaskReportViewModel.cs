using Autofac;
using EZGO.Api.Models.Enumerations;
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
using EZGO.Maui.Core.ViewModels.Reports;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;
using NodaTime;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Command = Microsoft.Maui.Controls.Command;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class TaskReportViewModel : BaseViewModel, IReportArea
    {
        private readonly IReportService _reportService;

        private readonly IWorkAreaService _workAreaService;

        #region Status

        string notOk => TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsNotOKFilter);

        string skipped => TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsSkippedFilter);

        string todo => TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsNotDoneFilter);

        public ObservableCollection<string> Status => new ObservableCollection<string>(new List<string> { notOk, todo, skipped });

        public string SelectedStatus { get; set; }

        public TaskStatusEnum TaskStatus { get; set; }

        #endregion

        #region Areas

        public List<BasicWorkAreaModel> WorkAreas { get; set; }

        public List<BasicWorkAreaModel> FlattenedWorkAreas { get; set; }

        public Rect Rect { get; set; } = new Rect(113, .2, .4, .6);

        public BasicWorkAreaModel SelectedWorkArea { get; set; }

        #endregion

        #region Tasks

        private List<ReportsCount> tasksresult;

        public bool IsBusyQ1 { get; set; } = true;

        public bool IsBusyQ2 { get; set; } = true;

        public bool HasItemsQ1 { get; set; } = true;

        public bool HasItemsQ2 { get; set; } = true;

        public List<ReportsCount> Tasks { get; set; }

        private int _spanCount = 0;
        public int SpanCount
        {
            get => _spanCount;
            set
            {
                _spanCount = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Deviations

        private ReportTaskDeviationsModel taskDeviations;

        public ObservableCollection<BasicReportDeviationItemModel> Deviations { get; set; }

        public bool ShowPercentage { get; set; }

        public bool IsBusyQ3 { get; set; } = true;

        public bool IsBusyQ4 { get; set; } = true;

        public bool HasItemsQ3 { get; set; } = true;

        public bool HasItemsQ4 { get; set; } = true;

        #endregion

        public string Interval { get; set; }

        #region Commands
        public ICommand NavigateToReportFilterCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToReportFilterAsync();
            });
        }, CanExecuteCommands);

        public ICommand DropdownTapCommand => new Microsoft.Maui.Controls.Command<object>(obj =>
        {
            ExecuteLoadingAction(async () => { await DropdownTap(obj); });
        }, CanExecuteCommands);

        public ICommand ChangeStatusCommand => new Microsoft.Maui.Controls.Command<object>(obj =>
        {
            ExecuteLoadingAction(async () => { await ChangeStatus(obj); });
        }, CanExecuteCommands);

        public ICommand ChangePercentageCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () => { await ChangePercentage(); });
        }, CanExecuteCommands);

        public ICommand NavigateToActionsCommand => new Microsoft.Maui.Controls.Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToActionsAsync(obj);
            });
        }, CanExecuteCommands);

        public IAsyncCommand<object> TapCommand => new AsyncCommand<object>(async (obj) =>
        {
            await NavigateToCompletedTasks(obj);
        });

        private async Task NavigateToCompletedTasks(object obj)
        {
            if (obj is ItemTappedEventArgs)
            {
                var report = ((ItemTappedEventArgs)obj).DataItem as ReportsCount;

                using var scope = App.Container.CreateScope();
                var completedTaskViewModel = scope.ServiceProvider.GetService<CompletedTaskViewModel>();
                completedTaskViewModel.CurrentInterval = report.TimespanType;
                completedTaskViewModel.IsFromDeepLink = true;
                completedTaskViewModel.PickerMode = ViewModels.Tasks.CompletedTasks.DatePickerMode.GoToDate;
                completedTaskViewModel.PickedDate = LocalDateTime.FromDateTime(report.ReportDate);

                await NavigationService.NavigateAsync(viewModel: completedTaskViewModel);
            }
        }

        #endregion

        public TaskReportViewModel(
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

        public override async Task Init()
        {
            Settings.SubpageReporting = MenuLocation.ReportTasks;

            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionReportsUnavailable);
                _messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
            }
            else
            {
                await Task.Run(async () =>
                {
                    await LoadWorkAreas();
                    SelectedStatus = Status.FirstOrDefault() ?? "";
                    TaskStatus = TaskStatusEnum.NotOk;

                    await PreLoadPrivates();
                    await LoadTasks();
                    await LoadDeviations();
                });
            }

            await base.Init();

            MessagingCenter.Subscribe<ReportFilterViewModel>(this, Constants.ReportPeriodChanged, async (sender) =>
            {
                await PreLoadPrivates();
                await LoadTasks();
                await LoadDeviations();
            });

            MessagingCenter.Subscribe<MessageService, Message>(this, Constants.MessageCenterMessage, async (formsApp, message) =>
            {
                if (message.MessageType == MessageTypeEnum.Clear)
                {
                    await LoadWorkAreas();

                    SelectedStatus = Status.FirstOrDefault() ?? "";
                    TaskStatus = TaskStatusEnum.NotOk;

                    await PreLoadPrivates();
                    await LoadTasks();
                    await LoadDeviations();
                }
                else
                {
                    IsBusyQ1 = true;
                    IsBusyQ2 = true;
                    IsBusyQ3 = true;
                    IsBusyQ4 = true;
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<ReportFilterViewModel>(this, Constants.ReportPeriodChanged);
                MessagingCenter.Unsubscribe<MessageService, Message>(this, Constants.MessageCenterMessage);
            });
            _workAreaService.Dispose();
            _reportService.Dispose();
            base.Dispose(disposing);
        }

        private async Task PreLoadPrivates()
        {
            tasksresult = await _reportService.GetTasksCountPerStatePerDateAsync(refresh: true);
            taskDeviations = await _reportService.GetTaskDeviationsAsync(period: Settings.ReportInterval, refresh: true);
        }

        private async Task LoadTasks()
        {
            Stopwatch sw = new Stopwatch();

            Tasks = new List<ReportsCount>();

            List<ReportsCount> intervalResult = _reportService.GetIntervalCollection(Settings.ReportInterval, DateTime.Today);
            List<ReportsCount> tasks = new List<ReportsCount>();
            List<ReportsCount> mergedResult = new List<ReportsCount>();

            if (tasksresult.Any())
            {
                tasksresult = tasksresult.Select(x => new ReportsCount
                {
                    CountNr = x.CountNr,
                    ReportDate = new DateTime(x.Year, x.Month, x.Day),
                    Day = x.Day,
                    DayOfYear = new DateTime(x.Year, x.Month, x.Day).DayOfYear,
                    Month = x.Month,
                    Name = x.Name,
                    Status = x.Status,
                    Week = x.Week,
                    Year = x.Year
                }).ToList();

                switch (Settings.ReportInterval)
                {
                    case TimespanTypeEnum.LastTwelveDays:
                        tasks = tasksresult.GroupBy(x => x.ReportDate.Date).Select((g, i) => new ReportsCount
                        {
                            NrNotOk = g.ToList().Where(x => x.Status == "not ok").Sum(x => x.CountNr),
                            NrOk = g.Where(x => x.Status == "ok").Sum(x => x.CountNr),
                            NrSkipped = g.Where(x => x.Status == "skipped").Sum(x => x.CountNr),
                            NrTodo = g.Where(x => x.Status == "todo").Sum(x => x.CountNr),
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Week = g.FirstOrDefault()?.Week ?? 0,
                            Day = g.Key.Day,
                            ReportDate = g.Key,
                            DayOfYear = g.Key.DayOfYear,
                            Name = String.Format("count per day of year {0}-{1}", g.Key.Year, g.Key.DayOfYear),
                            TimespanType = AggregationTimeInterval.Day
                        }).OrderBy(t => t.DayOfYear).ToList();

                        break;
                    case TimespanTypeEnum.LastTwelveWeeks:

                        tasksresult.ForEach(x => x.Name = String.Format("count per week of year {0}-{1}", x.Year, x.Week.ToString("D2")));
                        tasks = tasksresult.GroupBy(x => x.Name).Select((g, i) => new ReportsCount
                        {
                            NrNotOk = g.ToList().Where(x => x.Status == "not ok").Sum(x => x.CountNr),
                            NrOk = g.Where(x => x.Status == "ok").Sum(x => x.CountNr),
                            NrSkipped = g.Where(x => x.Status == "skipped").Sum(x => x.CountNr),
                            NrTodo = g.Where(x => x.Status == "todo").Sum(x => x.CountNr),
                            Name = g.Key,
                            TimespanType = AggregationTimeInterval.Week
                        }).ToList();

                        break;
                    case TimespanTypeEnum.LastTwelveMonths:
                    case TimespanTypeEnum.ThisYear:

                        tasksresult.ForEach(x => x.Name = String.Format("count per month of year {0}-{1}", x.Year, x.Month.ToString("D2")));

                        tasks = tasksresult.GroupBy(x => x.Name).Select((g, i) => new ReportsCount
                        {
                            NrNotOk = g.ToList().Where(x => x.Status == "not ok").Sum(x => x.CountNr),
                            NrOk = g.Where(x => x.Status == "ok").Sum(x => x.CountNr),
                            NrSkipped = g.Where(x => x.Status == "skipped").Sum(x => x.CountNr),
                            NrTodo = g.Where(x => x.Status == "todo").Sum(x => x.CountNr),
                            Name = g.Key,
                            TimespanType = AggregationTimeInterval.Month
                        }).ToList();

                        break;
                }

                mergedResult = (from x in intervalResult
                                join y in tasks on (x.Name) equals (y.Name) into xy
                                from z in xy.DefaultIfEmpty()
                                select new ReportsCount
                                {
                                    CountNr = z?.CountNr ?? 0,
                                    Day = x.Day,
                                    DayOfYear = x.DayOfYear,
                                    MaxCountNr = z?.MaxCountNr ?? 0,
                                    Month = x.Month,
                                    Name = x.Name,
                                    NrDone = z?.NrDone ?? 0,
                                    NrNotOk = z?.NrNotOk ?? 0,
                                    NrOk = z?.NrOk ?? 0,
                                    NrSkipped = z?.NrSkipped ?? 0,
                                    NrTodo = z?.NrTodo ?? 0,
                                    ReportDate = x.ReportDate,
                                    Status = z?.Status,
                                    Subscript = x.Subscript,
                                    Week = x.Week,
                                    Year = x.Year,
                                    TimespanType = z?.TimespanType
                                }).ToList();

                if (mergedResult.Any())
                {
                    mergedResult.ForEach(x =>
                    {
                        x.NrDone = (x.NrSkipped + x.NrOk + x.NrNotOk);
                        x.CountNr = (x.NrTodo + x.NrSkipped + x.NrOk + x.NrNotOk);
                    });

                    var maxcountnr = mergedResult.Max(x => x.CountNr);
                    mergedResult.ForEach(x => x.MaxCountNr = maxcountnr);
                }

                Tasks = mergedResult;
                SpanCount = Tasks.Count;
            }

            switch (Settings.ReportInterval)
            {
                case TimespanTypeEnum.LastTwelveDays:
                    Interval = TranslateExtension.GetValueFromDictionary(LanguageConstants.reports12DaysPeriod);
                    break;
                case TimespanTypeEnum.LastTwelveWeeks:
                    Interval = TranslateExtension.GetValueFromDictionary(LanguageConstants.reports12WeeksPeriod);
                    break;
                case TimespanTypeEnum.LastTwelveMonths:
                    Interval = TranslateExtension.GetValueFromDictionary(LanguageConstants.reports12MonthsPeriod);
                    break;
                case TimespanTypeEnum.ThisYear:
                    Interval = TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsThisYearPeriod);
                    break;
                default:
                    Interval = "";
                    break;
            }

            HasItemsQ1 = Tasks.Any();
            HasItemsQ2 = HasItemsQ1;
            OnPropertyChanged(nameof(Tasks));
            SpanCount = Tasks.Count;
            IsBusyQ1 = false;
            IsBusyQ2 = IsBusyQ1;

            await Task.CompletedTask;
        }

        private async Task LoadDeviations()
        {
            var mydeviations = new List<ReportDeviationItemModel>();
            var mybasicdeviations = new List<BasicReportDeviationItemModel>();
            double maxcount = 0;

            if (taskDeviations != null)
            {
                switch (TaskStatus)
                {
                    case TaskStatusEnum.NotOk:
                        mydeviations = taskDeviations.DeviationsNotOk;
                        break;
                    case TaskStatusEnum.Skipped:
                        mydeviations = taskDeviations.DeviationsSkipped;
                        break;
                    case TaskStatusEnum.Todo:
                        mydeviations = taskDeviations.DeviationsTodo;
                        break;
                }
            }

            if (mydeviations.Any())
            {
                mybasicdeviations = mydeviations.ToBasicList<BasicReportDeviationItemModel, ReportDeviationItemModel>();
                if (ShowPercentage)
                {
                    mybasicdeviations = mybasicdeviations.OrderByDescending(x => x.Percentage).Take(5).ToList();
                    maxcount = mybasicdeviations.Max(x => x.Percentage);
                    mybasicdeviations.ForEach(x =>
                    {
                        x.MaxPercentage = maxcount;
                        if (maxcount > 0)
                            x.CalculatedPercentage = Math.Round(((double)x.Percentage / x.MaxPercentage) * 100, 2);

                        x.DisplayAmount = string.Format("{0:N1}", x.Percentage);
                        if (x.ActionDoneCount > x.ActionCount)
                            x.PercentageActionDone = 100;
                        else
                            x.PercentageActionDone = x.ActionCount > 0 ? (int)Math.Round(((double)x.ActionDoneCount / x.ActionCount) * 100, 0) : 0;
                    });
                }
                else
                {
                    mybasicdeviations = mybasicdeviations.OrderByDescending(x => x.CountNr).Take(5).ToList();
                    maxcount = mybasicdeviations.Max(x => x.CountNr);
                    mybasicdeviations.ForEach(x =>
                    {
                        x.MaxPercentage = maxcount;
                        if (maxcount > 0)
                            x.CalculatedPercentage = Math.Round(((double)x.CountNr / x.MaxPercentage) * 100, 2);

                        x.DisplayAmount = x.CountNr.ToString();
                        if (x.ActionDoneCount > x.ActionCount)
                            x.PercentageActionDone = 100;
                        else
                            x.PercentageActionDone = x.ActionCount > 0 ? (int)Math.Round(((double)x.ActionDoneCount / x.ActionCount) * 100, 0) : 0;
                    });
                }
            }

            HasItemsQ3 = mybasicdeviations.Any();
            HasItemsQ4 = HasItemsQ3;

            Deviations = new ObservableCollection<BasicReportDeviationItemModel>(mybasicdeviations);
            OnPropertyChanged(nameof(Deviations));

            IsBusyQ3 = false;
            IsBusyQ4 = IsBusyQ3;
            await Task.CompletedTask;
        }

        protected override async Task RefreshAsync()
        {
            await PreLoadPrivates();
            await LoadTasks();
            await LoadDeviations();
        }

        private async Task NavigateToReportFilterAsync()
        {
            await NavigationService.NavigateAsync<ReportFilterViewModel>();
        }

        private async Task NavigateToActionsAsync(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is BasicReportDeviationItemModel deviation)
            {
                if (deviation != null)
                {
                    if (deviation.ActionCount > 0)
                    {
                        using var scope = App.Container.CreateScope();
                        var actionReportActionsViewModel = scope.ServiceProvider.GetService<ActionReportActionsViewModel>();
                        actionReportActionsViewModel.TaskTemplateId = deviation.Id;
                        actionReportActionsViewModel.FilterByTimespan = false;
                        actionReportActionsViewModel.ReportActionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskActionsTitle);
                        await NavigationService.NavigateAsync(viewModel: actionReportActionsViewModel);
                    }
                }
            }
        }

        private async Task LoadWorkAreas()
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
            SelectedWorkArea = FlattenedWorkAreas?.FirstOrDefault(x => x.Id == ((Settings.ReportWorkAreaId != 0) ? Settings.ReportWorkAreaId : Settings.WorkAreaId));
            SelectedWorkArea ??= WorkAreas.FirstOrDefault();
        }

        private async Task DropdownTap(object obj)
        {
            IsDropdownOpen = false;
            if ((obj as Syncfusion.TreeView.Engine.TreeViewNode).Content is BasicWorkAreaModel workAreaModel)
            {
                if (workAreaModel.Id != Settings.ReportWorkAreaId)
                {
                    Settings.ReportWorkAreaId = workAreaModel.Id;
                    SelectedWorkArea = workAreaModel;

                    IsBusyQ1 = true;
                    IsBusyQ2 = true;
                    IsBusyQ3 = true;
                    IsBusyQ4 = true;

                    tasksresult = await _reportService.GetTasksCountPerStatePerDateAsync();
                    await LoadTasks();

                    taskDeviations = await _reportService.GetTaskDeviationsAsync(period: Settings.ReportInterval);
                    await LoadDeviations();

                    await MainThread.InvokeOnMainThreadAsync(
                        () => MessagingCenter.Send(this, Constants.ReportAreaChanged)
                        );
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
                string result = "{0} ";
                Title = string.Format(result.ReplaceLanguageVariablesCumulative(), SelectedWorkArea.Name);
            }
        }

        private async Task ChangeStatus(object obj)
        {
            if (obj is SelectionChangedEventArgs eventArgs && eventArgs.AddedItems.FirstOrDefault() is string)
            {
                SelectedStatus = eventArgs.AddedItems.FirstOrDefault().ToString();
                if (SelectedStatus == todo)
                {
                    TaskStatus = TaskStatusEnum.Todo;
                }
                else if (SelectedStatus == skipped)
                {
                    TaskStatus = TaskStatusEnum.Skipped;
                }
                else
                {
                    TaskStatus = TaskStatusEnum.NotOk;
                }
                await LoadDeviations();
            }
        }

        private async Task ChangePercentage()
        {
            ShowPercentage = !ShowPercentage;
            await LoadDeviations();
        }


        /// <summary>
        /// Cancels.
        /// </summary>
        public async override Task CancelAsync()
        {
            if (Settings.MenuLocation == MenuLocation.Report && NavigationService.GetNavigationStackCount() > 1)
            {
                Settings.SubpageReporting = MenuLocation.Report;
                await base.CancelAsync();
            }
            else
            {
                Settings.MenuLocation = MenuLocation.Report;
                await NavigationService.NavigateAsync<ReportViewModel>(noHistory: true, animated: false);
            }
        }

    }
}
