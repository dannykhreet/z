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
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;


namespace EZGO.Maui.Core.ViewModels
{
    public class ActionReportViewModel : BaseViewModel, IReportArea
    {
        private readonly IReportService _reportService;
        private readonly IWorkAreaService _workAreaService;

        #region Areas

        private List<BasicWorkAreaModel> workAreas;
        public List<BasicWorkAreaModel> WorkAreas
        {
            get => workAreas;
            set
            {
                workAreas = value;

                OnPropertyChanged();
            }
        }

        private List<BasicWorkAreaModel> flattenedWorkAreas;
        public List<BasicWorkAreaModel> FlattenedWorkAreas
        {
            get => flattenedWorkAreas;
            set
            {
                flattenedWorkAreas = value;

                OnPropertyChanged();
            }
        }


        private Rect _rect = new Rect(129, .2, .4, .6);
        public Rect Rect
        {
            get { return _rect; }
            set
            {
                _rect = value;
                OnPropertyChanged();
            }
        }

        private BasicWorkAreaModel selectedWorkArea;
        public BasicWorkAreaModel SelectedWorkArea
        {
            get => selectedWorkArea;
            set
            {
                selectedWorkArea = value;

                SetTitle();

                OnPropertyChanged();
            }
        }

        #endregion

        #region Q1

        private bool isBusyQ1 = true;
        public bool IsBusyQ1
        {
            get => isBusyQ1;
            set
            {
                isBusyQ1 = value;

                OnPropertyChanged();
            }
        }

        private List<ReportsCount> _actions = new List<ReportsCount>();
        public List<ReportsCount> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                SpanCount = Actions.Count;
                OnPropertyChanged();
            }
        }

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



        #region Q2

        private bool isBusyQ2;
        public bool IsBusyQ2
        {
            get => isBusyQ2;
            set
            {
                isBusyQ2 = value;

                OnPropertyChanged();
            }
        }

        private ReportsCount allActions;
        public ReportsCount AllActions
        {
            get => allActions;
            set
            {
                allActions = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Q3

        private bool hasItemsQ3 = true;
        public bool HasItemsQ3
        {
            get => hasItemsQ3;
            set
            {
                hasItemsQ3 = value;

                OnPropertyChanged();
            }
        }

        private bool isBusyQ3 = true;
        public bool IsBusyQ3
        {
            get => isBusyQ3;
            set
            {
                isBusyQ3 = value;

                OnPropertyChanged();
            }
        }


        private List<ReportsCount> actionsTop;
        public List<ReportsCount> ActionsTop
        {
            get => actionsTop;
            set
            {
                actionsTop = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Q4

        private bool hasItemsQ4 = true;
        public bool HasItemsQ4
        {
            get => hasItemsQ4;
            set
            {
                hasItemsQ4 = value;

                OnPropertyChanged();
            }
        }

        private bool isBusyQ4 = true;
        public bool IsBusyQ4
        {
            get => isBusyQ4;
            set
            {
                isBusyQ4 = value;

                OnPropertyChanged();
            }
        }

        private List<ReportsCount> actionsInvolved;
        public List<ReportsCount> ActionsInvolved
        {
            get => actionsInvolved;
            set
            {
                actionsInvolved = value;

                OnPropertyChanged();
            }
        }

        #endregion

        public string Interval { get; set; }

        public ICommand NavigateToReportFilterCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToReportFilterAsync();
            });
        }, CanExecuteCommands);

        public ICommand DropdownTapCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () => { await DropdownTap(obj); });
        }, CanExecuteCommands);

        public ICommand NavigateToUserActionsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToUserActionsAsync(obj);
            });
        }, CanExecuteCommands);

        public ICommand NavigateToAssignedUserActionsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToAssignedUserActionsAsync(obj);
            });
        }, CanExecuteCommands);

        public ActionReportViewModel(
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
            Settings.SubpageReporting = MenuLocation.ReportActions;


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
                    await LoadAll();
                });
            }

            await Task.Run(async () => await base.Init());


            MessagingCenter.Subscribe<ReportFilterViewModel>(this, Constants.ReportPeriodChanged, async (sender) =>
            {
                IsBusyQ1 = true;
                IsBusyQ3 = true;
                IsBusyQ4 = true;
                await LoadAll();
            });

            MessagingCenter.Subscribe<MessageService, Message>(this, Constants.MessageCenterMessage, async (formsApp, message) =>
            {
                if (message.MessageType == MessageTypeEnum.Clear)
                {
                    await LoadWorkAreas();

                    await LoadAll();
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
            base.Dispose(disposing);
        }

        protected override async Task RefreshAsync()
        {
            await LoadAll();
        }

        private async Task LoadAll()
        {
            _actions = await _reportService.GetActionsCountStartedResolvedPerDateAsync(refresh: true);
            allActions = await _reportService.GetActionsCountAsync(period: Settings.ReportInterval, refresh: true);

            LoadActions();

            actionsTop = await _reportService.GetActionsCountPerUserAsync(period: Settings.ReportInterval, refresh: true);
            actionsInvolved = await _reportService.GetActionsCountPerAssignedUserAsync(period: Settings.ReportInterval, refresh: true);

            LoadCompetitives();
        }

        private void LoadActions()
        {
            List<ReportsCount> intervalResult = _reportService.GetIntervalCollection(Settings.ReportInterval, DateTime.Today);
            List<ReportsCount> groupedResult = new List<ReportsCount>();

            List<ReportsCount> mergedResult = new List<ReportsCount>();

            int maxcount = 0;

            if (_actions.Any())
            {
                switch (Settings.ReportInterval)
                {
                    case TimespanTypeEnum.LastTwelveDays:

                        _actions.ForEach(x => x.ReportDate = new DateTime(x.Year, x.Month, x.Day));

                        groupedResult = _actions.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            ReportDate = g.FirstOrDefault()?.ReportDate ?? DateTime.Parse(g.Key),
                            CountNrResolved = g.Where(d => d.Status == "resolved").Sum(d => d.CountNr),
                            CountNr = g.Sum(d => d.CountNr)
                        }).ToList();

                        mergedResult = (from x in intervalResult
                                        join y in groupedResult on (x.ReportDate.Date) equals (y.ReportDate.Date) into xy
                                        from z in xy.DefaultIfEmpty()
                                        select new ReportsCount
                                        {
                                            CountNr = z?.CountNr ?? 0,
                                            CountNrResolved = z?.CountNrResolved ?? 0,
                                            Day = x.Day,
                                            Month = x.Month,
                                            Name = x.Name,
                                            ReportDate = x.ReportDate,
                                            Subscript = x.Subscript,
                                            Week = x.Week,
                                            Year = x.Year
                                        }).ToList();

                        break;
                    case TimespanTypeEnum.LastTwelveWeeks:

                        _actions.ForEach(x => x.Name = String.Format("count per week of year {0}-{1}", x.Year, x.Week.ToString("D2")));
                        groupedResult = _actions.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Name = g.Key,
                            CountNrResolved = g.Where(d => d.Status == "resolved").Sum(d => d.CountNr),
                            CountNr = g.Sum(d => d.CountNr)
                        }).ToList();

                        mergedResult = (from x in intervalResult
                                        join y in groupedResult on (x.Name) equals (y.Name) into xy
                                        from z in xy.DefaultIfEmpty()
                                        select new ReportsCount
                                        {
                                            CountNr = z?.CountNr ?? 0,
                                            CountNrResolved = z?.CountNrResolved ?? 0,
                                            Day = x.Day,
                                            Month = x.Month,
                                            Name = x.Name,
                                            ReportDate = x.ReportDate,
                                            Subscript = x.Subscript,
                                            Week = x.Week,
                                            Year = x.Year
                                        }).ToList();

                        break;
                    case TimespanTypeEnum.LastTwelveMonths:
                    case TimespanTypeEnum.ThisYear:

                        _actions.ForEach(x => x.Name = String.Format("count per month of year {0}-{1}", x.Year, x.Month.ToString("D2")));
                        groupedResult = _actions.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Name = g.Key,
                            CountNrResolved = g.Where(d => d.Status == "resolved").Sum(d => d.CountNr),
                            CountNr = g.Sum(d => d.CountNr)
                        }).ToList();

                        mergedResult = (from x in intervalResult
                                        join y in groupedResult on (x.Name) equals (y.Name) into xy
                                        from z in xy.DefaultIfEmpty()
                                        select new ReportsCount
                                        {
                                            CountNr = z?.CountNr ?? 0,
                                            CountNrResolved = z?.CountNrResolved ?? 0,
                                            Day = x.Day,
                                            Month = x.Month,
                                            Name = x.Name,
                                            ReportDate = x.ReportDate,
                                            Subscript = x.Subscript,
                                            Week = x.Week,
                                            Year = x.Year
                                        }).ToList();

                        break;
                }

                maxcount = mergedResult.Max(x => x.CountNr);
                mergedResult.ForEach(x =>
                {
                    x.MaxCountNr = maxcount;
                    if (x.MaxCountNr > 0)
                        x.PercentageRelative = Math.Round(((double)x.CountNr / x.MaxCountNr) * 100, 2);
                    if (x.CountNr > 0)
                        x.PercentageOk = Math.Round(((double)x.CountNrResolved / x.CountNr) * 100, 2);
                    x.CountNrUnresolved = x.CountNr - x.CountNrResolved;
                });

                Actions = mergedResult;
            }

            OnPropertyChanged(nameof(Actions));
            HasItems = Actions.Any();
            IsBusyQ1 = false;

            if (allActions != null)
            {
                allActions = new ReportsCount
                {
                    CountNr = allActions.CountNr,
                    CountNrOverdue = allActions.CountNrOverdue,
                    CountNrResolved = allActions.CountNrResolved,
                    CountNrUnresolved = allActions.CountNrUnresolved,
                    CountNrUnresolvedNotOverdue = allActions.CountNrUnresolved - allActions.CountNrOverdue,
                    PercentageResolved = (allActions.CountNr > 0) ? Math.Round(((double)allActions.CountNrResolved / allActions.CountNr) * 100, 2) : 0,
                    PercentageOverdue = (allActions.CountNrUnresolved != 0) ? Math.Round(((double)allActions.CountNrOverdue / allActions.CountNrUnresolved) * 100, 2) : 0
                };
                AllActions = allActions;
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

            OnPropertyChanged(nameof(AllActions));
            IsBusyQ2 = false;
        }

        private void LoadCompetitives()
        {
            int maxcount = 0;

            if (actionsTop.Any())
            {
                actionsTop = actionsTop.Take(10).OrderByDescending(x => x.CountNr).ToList();
                maxcount = actionsTop.Max(x => x.CountNr);
                actionsTop.ForEach(x =>
                {
                    x.MaxCountNr = maxcount;
                    if (x.MaxCountNr > 0)
                        x.PercentageRelative = Math.Round(((double)x.CountNr / x.MaxCountNr) * 100, 2);
                });

                ActionsTop = actionsTop;
            }

            IsBusyQ3 = false;
            OnPropertyChanged(nameof(ActionsTop));
            HasItemsQ3 = ActionsTop.Any();

            if (actionsInvolved.Any())
            {
                actionsInvolved = actionsInvolved.Take(10).OrderByDescending(x => x.CountNr).ToList();
                maxcount = actionsInvolved.Max(x => x.CountNr);
                actionsInvolved.ForEach(x =>
                {
                    x.MaxCountNr = maxcount;
                    if (x.MaxCountNr > 0)
                        x.PercentageRelative = Math.Round(((double)x.CountNr / x.MaxCountNr) * 100, 2);
                });

                ActionsInvolved = actionsInvolved;
            }

            IsBusyQ4 = false;
            OnPropertyChanged(nameof(ActionsInvolved));
            HasItemsQ4 = ActionsInvolved.Any();
        }

        private async Task NavigateToReportFilterAsync()
        {
            await NavigationService.NavigateAsync<ReportFilterViewModel>();
        }

        private async Task NavigateToUserActionsAsync(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is ReportsCount report)
            {
                if (report != null)
                {
                    using var scope = App.Container.CreateScope();
                    var actionReportActionsViewModel = scope.ServiceProvider.GetService<ActionReportActionsViewModel>();
                    actionReportActionsViewModel.UserId = report.Id;
                    actionReportActionsViewModel.ReportActionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.userActionsTitle);
                    await NavigationService.NavigateAsync(viewModel: actionReportActionsViewModel);
                }
            }
        }

        private async Task NavigateToAssignedUserActionsAsync(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is ReportsCount report)
            {
                if (report != null)
                {
                    using var scope = App.Container.CreateScope();
                    var actionReportActionsViewModel = scope.ServiceProvider.GetService<ActionReportActionsViewModel>();
                    actionReportActionsViewModel.ResourceId = report.Id;
                    actionReportActionsViewModel.ReportActionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.userActionsTitle);
                    await NavigationService.NavigateAsync(viewModel: actionReportActionsViewModel);
                }
            }

            await Task.CompletedTask;
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

                    await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ReportAreaChanged); });
                }
            }
            await Task.CompletedTask;
        }
        private void SetTitle()
        {
            if (SelectedWorkArea != null)
            {
                string result = "{0} ";
                Title = string.Format(result.ReplaceLanguageVariablesCumulative(), SelectedWorkArea.Name);
            }
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
