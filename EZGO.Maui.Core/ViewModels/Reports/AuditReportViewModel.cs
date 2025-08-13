using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Audits;
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
using System.Collections.ObjectModel;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class AuditReportViewModel : BaseViewModel, IReportArea
    {
        private readonly IReportService _reportService;
        private readonly IWorkAreaService _workAreaService;
        private readonly IAuditsService _auditService;

        #region Status

        string deviations
        {
            get
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsDeviationsFilter);
                return result;
            }
        }
        string skipped
        {
            get
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsSkippedFilter);
                return result;
            }
        }

        public ObservableCollection<string> Status { get { return new ObservableCollection<string>(new List<string> { deviations, skipped }); } }

        private string selectedStatus;
        public String SelectedStatus
        {
            get => selectedStatus;
            set
            {
                selectedStatus = value;

                OnPropertyChanged();
            }
        }

        private TaskStatusEnum taskStatus = TaskStatusEnum.NotOk;
        public TaskStatusEnum TaskStatus
        {
            get => taskStatus;
            set
            {
                taskStatus = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Audits

        private List<ReportsCount> auditsResult = new List<ReportsCount>();
        private List<ReportsAverage> auditsAverageResult = new List<ReportsAverage>();



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

        private bool isBusyQ2 = true;
        public bool IsBusyQ2
        {
            get => isBusyQ2;
            set
            {
                isBusyQ2 = value;

                OnPropertyChanged();
            }
        }

        private bool hasItemsQ1 = true;
        public bool HasItemsQ1
        {
            get => hasItemsQ1;
            set
            {
                hasItemsQ1 = value;

                OnPropertyChanged();
            }
        }

        private bool hasItemsQ2 = true;
        public bool HasItemsQ2
        {
            get => hasItemsQ2;
            set
            {
                hasItemsQ2 = value;

                OnPropertyChanged();
            }
        }

        private List<ReportsCount> _audits;
        public List<ReportsCount> Audits
        {
            get => _audits;
            set
            {
                _audits = value;
                SpanCountAudits = Audits.Count;

                OnPropertyChanged();
            }
        }

        private int _spanCountAudits = 0;
        public int SpanCountAudits
        {
            get => _spanCountAudits;
            set
            {
                _spanCountAudits = value;
                OnPropertyChanged();
            }
        }

        private List<ReportsCount> _auditsAvg;
        public List<ReportsCount> AuditsAvg
        {
            get => _auditsAvg;
            set
            {
                _auditsAvg = value;
                SpanCountAuditsAvg = AuditsAvg.Count;
                OnPropertyChanged();
            }
        }
        private int _spanCountAuditsAvg = 0;
        public int SpanCountAuditsAvg
        {
            get => _spanCountAuditsAvg;
            set
            {
                _spanCountAuditsAvg = value;
                OnPropertyChanged();
            }
        }

        #endregion

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

        #region Deviations

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

        private ReportAuditDeviationsModel auditDeviations;
        public List<ReportDeviationItemModel> AuditDeviations { get; set; }

        #endregion

        public string Interval { get; set; }

        public string SelectedAuditName { get; set; }

        public bool IsAuditNameVisible { get; set; }

        public ICommand NavigateToReportFilterCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToReportFilterAsync();
            });
        }, CanExecuteCommands);

        public ICommand ChangeStatusCommand => new Command<object>(ChangeStatus);

        public ICommand DropdownTapCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () => { await DropdownTap(obj); });
        }, CanExecuteCommands);

        public ICommand NavigateToAuditActionsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToAuditActionsAsync(obj);
            });
        }, CanExecuteCommands);

        public ICommand NavigateToCompletedAuditsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToCompletedAuditsAsync(obj);
            });
        }, CanExecuteCommands);

        public AuditReportViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IReportService reportService,
            IAuditsService auditsService,
            IWorkAreaService workAreaService) : base(navigationService, userService, messageService, actionsService)
        {
            _workAreaService = workAreaService;
            _reportService = reportService;
            _auditService = auditsService;

        }

        public override async Task Init()
        {
            Settings.SubpageReporting = MenuLocation.ReportAudits;

            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.noInternetConnectionReportsUnavailable);
                _messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, false, true, MessageTypeEnum.Connection);
            }
            else
            {
                await LoadWorkAreas();

                SelectedStatus = Status.FirstOrDefault() ?? "";
                await Task.Run(async () => await LoadAll());
            }

            await Task.Run(async () => await base.Init());

            MessagingCenter.Subscribe<MessageService, Message>(this, Constants.MessageCenterMessage, async (formsApp, message) =>
            {
                if (message.MessageType == MessageTypeEnum.Clear)
                {
                    await LoadWorkAreas();

                    SelectedStatus = Status.FirstOrDefault() ?? "";

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

            MessagingCenter.Subscribe<ReportFilterViewModel>(this, Constants.ReportPeriodChanged, async (sender) =>
            {
                IsBusyQ1 = true;
                IsBusyQ2 = true;
                IsBusyQ3 = true;
                IsBusyQ4 = true;
                await LoadAll();
            });

            MessagingCenter.Subscribe<ReportFilterViewModel>(this, Constants.ReportAuditIdChanged, async (sender) =>
            {
                IsBusyQ1 = true;
                IsBusyQ2 = true;
                IsBusyQ3 = true;
                IsBusyQ4 = true;
                await LoadAll();
            });

        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<MessageService, Message>(this, Constants.MessageCenterMessage);
                MessagingCenter.Unsubscribe<ReportFilterViewModel>(this, Constants.ReportPeriodChanged);
                MessagingCenter.Unsubscribe<ReportFilterViewModel>(this, Constants.ReportAuditIdChanged);
            });
            _reportService.Dispose();
            _auditService.Dispose();
            _workAreaService.Dispose();
            base.Dispose(disposing);
        }

        private async Task LoadAll()
        {
            auditsResult = await _reportService.GetAuditsCountPerDate(audittemplateid: Settings.ReportAuditId, refresh: true);
            auditsAverageResult = await _reportService.GetAuditsAveragePerDate(audittemplateid: Settings.ReportAuditId, refresh: true);

            LoadAudits();


            if (Settings.ReportAuditId != 0)
            {
                var audits = await _auditService.GetReportAuditTemplatesAsync();
                SelectedAuditName = audits.FirstOrDefault(a => a.Id == Settings.ReportAuditId).Name;
                IsAuditNameVisible = true;
            }
            else
                IsAuditNameVisible = false;

            auditDeviations = await _reportService.GetAuditDeviationsAsync(period: Settings.ReportInterval, audittemplateid: Settings.ReportAuditId, refresh: true);

            LoadDeviations();
        }

        protected override async Task RefreshAsync()
        {
            await LoadAll();
        }

        private void LoadAudits()
        {
            Audits = new List<ReportsCount>();
            AuditsAvg = new List<ReportsCount>();

            List<ReportsCount> auditsAverageResult2 = new List<ReportsCount>();

            List<ReportsCount> intervalResult = _reportService.GetIntervalCollection(Settings.ReportInterval, DateTime.Today);
            List<ReportsCount> groupedResult = new List<ReportsCount>();

            List<ReportsCount> mergedResult = new List<ReportsCount>();
            List<ReportsCount> mergedAverageResult = new List<ReportsCount>();

            int maxcount = 0;

            if (auditsResult.Any())
            {
                if (auditsAverageResult.Any())
                {
                    auditsAverageResult2 = auditsAverageResult.Select(x => new ReportsCount
                    {
                        Name = x.Name,
                        Day = x.Day,
                        Week = x.Week,
                        Month = x.Month,
                        Year = x.Year,
                        AverageNr = (decimal)(x.AverageNr ?? 0)
                    }).ToList();
                }

                switch (Settings.ReportInterval)
                {
                    case TimespanTypeEnum.LastTwelveDays:

                        // Quadrant 1
                        auditsResult.ForEach(x => x.ReportDate = new DateTime(x.Year, x.Month, x.Day));
                        mergedResult = (from x in intervalResult
                                        join y in auditsResult on (x.ReportDate.Date) equals (y.ReportDate.Date) into xy
                                        from z in xy.DefaultIfEmpty()
                                        select new ReportsCount
                                        {
                                            CountNr = z?.CountNr ?? 0,
                                            Day = x.Day,
                                            DayOfYear = x.DayOfYear,
                                            Month = x.Month,
                                            Name = x.Name,
                                            ReportDate = x.ReportDate,
                                            Subscript = x.Subscript,
                                            Week = x.Week,
                                            Year = x.Year
                                        }).ToList();

                        maxcount = mergedResult.Max(x => x.CountNr);
                        mergedResult.ForEach(x => x.MaxCountNr = maxcount);

                        // Quadrant 2
                        auditsAverageResult2.ForEach(x => x.ReportDate = new DateTime(x.Year, x.Month, x.Day));

                        mergedAverageResult = (from x in intervalResult
                                               join y in auditsAverageResult2 on (x.ReportDate.Date) equals (y.ReportDate.Date) into xy
                                               from z in xy.DefaultIfEmpty()
                                               select new ReportsCount
                                               {
                                                   AverageNr = (int)Math.Round(z?.AverageNr ?? 0),
                                                   Day = x.Day,
                                                   DayOfYear = x.DayOfYear,
                                                   Month = x.Month,
                                                   Name = x.Name,
                                                   ReportDate = x.ReportDate,
                                                   Subscript = x.Subscript,
                                                   Week = x.Week,
                                                   Year = x.Year
                                               }).ToList();

                        break;
                    case TimespanTypeEnum.LastTwelveWeeks:

                        // Quadrant 1
                        auditsResult.ForEach(x => x.Name = String.Format("count per week of year {0}-{1}", x.Year, x.Week.ToString("D2")));
                        groupedResult = auditsResult.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Name = g.Key,
                            CountNr = g.Sum(c => c.CountNr)
                        }).ToList();

                        mergedResult = (from x in intervalResult
                                        join y in groupedResult on (x.Name) equals (y.Name) into xy
                                        from z in xy.DefaultIfEmpty()
                                        select new ReportsCount
                                        {
                                            CountNr = z?.CountNr ?? 0,
                                            Day = x.Day,
                                            DayOfYear = x.DayOfYear,
                                            Month = x.Month,
                                            Name = x.Name,
                                            ReportDate = x.ReportDate,
                                            Subscript = x.Subscript,
                                            Week = x.Week,
                                            Year = x.Year
                                        }).ToList();

                        maxcount = mergedResult.Max(x => x.CountNr);
                        mergedResult.ForEach(x => x.MaxCountNr = maxcount);

                        // Quadrant 2
                        auditsAverageResult2.ForEach(x => x.Name = String.Format("count per week of year {0}-{1}", x.Year, x.Week.ToString("D2")));
                        groupedResult = auditsAverageResult2.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Name = g.Key,
                            AverageNr = g.Average(a => a.AverageNr),
                        }).ToList();

                        mergedAverageResult = (from x in intervalResult
                                               join y in groupedResult on (x.Name) equals (y.Name) into xy
                                               from z in xy.DefaultIfEmpty()
                                               select new ReportsCount
                                               {
                                                   AverageNr = (int)Math.Round(z?.AverageNr ?? 0),
                                                   Day = x.Day,
                                                   DayOfYear = x.DayOfYear,
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

                        // Quadrant 1
                        auditsResult.ForEach(x => x.Name = String.Format("count per month of year {0}-{1}", x.Year, x.Month.ToString("D2")));
                        groupedResult = auditsResult.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Name = g.Key,
                            CountNr = g.Sum(c => c.CountNr)
                        }).ToList();

                        mergedResult = (from x in intervalResult
                                        join y in groupedResult on (x.Name) equals (y.Name) into xy
                                        from z in xy.DefaultIfEmpty()
                                        select new ReportsCount
                                        {
                                            CountNr = z?.CountNr ?? 0,
                                            Day = x.Day,
                                            DayOfYear = x.DayOfYear,
                                            Month = x.Month,
                                            Name = x.Name,
                                            ReportDate = x.ReportDate,
                                            Subscript = x.Subscript,
                                            Week = x.Week,
                                            Year = x.Year
                                        }).ToList();

                        maxcount = mergedResult.Max(x => x.CountNr);
                        mergedResult.ForEach(x => x.MaxCountNr = maxcount);

                        // Quadrant 2
                        auditsAverageResult2.ForEach(x => x.Name = String.Format("count per month of year {0}-{1}", x.Year, x.Month.ToString("D2")));
                        groupedResult = auditsAverageResult2.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Name = g.Key,
                            AverageNr = g.Average(a => a.AverageNr),
                        }).ToList();

                        mergedAverageResult = (from x in intervalResult
                                               join y in groupedResult on (x.Name) equals (y.Name) into xy
                                               from z in xy.DefaultIfEmpty()
                                               select new ReportsCount
                                               {
                                                   AverageNr = (int)Math.Round(z?.AverageNr ?? 0),
                                                   Day = x.Day,
                                                   DayOfYear = x.DayOfYear,
                                                   Month = x.Month,
                                                   Name = x.Name,
                                                   ReportDate = x.ReportDate,
                                                   Subscript = x.Subscript,
                                                   Week = x.Week,
                                                   Year = x.Year
                                               }).ToList();

                        break;
                }

                Audits = mergedResult;
                AuditsAvg = mergedAverageResult;
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

            HasItemsQ1 = Audits.Any();
            OnPropertyChanged(nameof(Audits));
            IsBusyQ1 = false;

            HasItemsQ2 = AuditsAvg.Any();
            OnPropertyChanged(nameof(AuditsAvg));
            IsBusyQ2 = false;
        }

        private void LoadDeviations()
        {
            double maxcount = 0;

            List<ReportDeviationItemModel> auditdeviationitems = new List<ReportDeviationItemModel>();

            if (auditDeviations != null)
            {
                switch (taskStatus)
                {
                    case TaskStatusEnum.NotOk:
                        var temp = auditDeviations.Deviations?.ToBasicList<ReportDeviationItemModel, ReportAuditDeviationItemModel>();
                        auditdeviationitems = temp ?? auditdeviationitems;
                        break;
                    case TaskStatusEnum.Skipped:
                        auditdeviationitems = auditDeviations.DeviationsSkipped?.ToList() ?? auditdeviationitems;
                        break;
                }
            }

            if (auditdeviationitems.Any())
            {
                auditdeviationitems = auditdeviationitems.OrderByDescending(x => x.Percentage).Take(5).ToList();
                maxcount = auditdeviationitems.Max(x => x.Percentage);
                auditdeviationitems.ForEach(x =>
                {

                    x.MaxPercentage = maxcount;
                    if (maxcount > 0)
                    {
                        x.CalculatedPercentage = Math.Round(((double)x.Percentage / x.MaxPercentage) * 100, 2);
                    }

                    if (x.ActionDoneCount > x.ActionCount)
                        x.PercentageActionDone = 100;
                    else
                        x.PercentageActionDone = x.ActionCount > 0 ? (int)Math.Round(((double)x.ActionDoneCount / x.ActionCount) * 100, 0) : 0;
                });

            }

            HasItemsQ3 = auditdeviationitems.Any();
            HasItemsQ4 = HasItemsQ3;

            AuditDeviations = auditdeviationitems;
            OnPropertyChanged(nameof(AuditDeviations));

            IsBusyQ3 = false;
            IsBusyQ4 = false;
        }

        private async Task NavigateToReportFilterAsync()
        {
            await NavigationService.NavigateAsync<ReportFilterViewModel>();
        }

        private async Task NavigateToAuditActionsAsync(object obj)
        {
            using var scope = App.Container.CreateScope();
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is ReportDeviationItemModel deviation)
            {
                if (deviation != null)
                {
                    if (deviation.ActionCount > 0)
                    {
                        var actionReportActionsViewModel = scope.ServiceProvider.GetService<ActionReportActionsViewModel>();
                        actionReportActionsViewModel.TaskTemplateId = deviation.Id;
                        actionReportActionsViewModel.FilterByTimespan = false;
                        actionReportActionsViewModel.ReportActionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.auditActionsTitle);
                        await NavigationService.NavigateAsync(viewModel: actionReportActionsViewModel);
                    }
                }
            }
            else if (obj is ItemTappedEventArgs eventArgs2 && eventArgs2.DataItem is ReportAuditDeviationItemModel deviation2)
            {
                if (deviation2 != null)
                {
                    if (deviation2.ActionCount > 0)
                    {
                        var actionReportActionsViewModel = scope.ServiceProvider.GetService<ActionReportActionsViewModel>();
                        actionReportActionsViewModel.TaskTemplateId = deviation2.TaskTemplateId;
                        actionReportActionsViewModel.FilterByTimespan = false;
                        await NavigationService.NavigateAsync(viewModel: actionReportActionsViewModel);
                    }
                }
            }
        }

        private async Task NavigateToCompletedAuditsAsync(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is ReportsCount report)
            {
                if (report.CountNr > 0)
                {
                    using var scope = App.Container.CreateScope();
                    var completedAuditViewModel = scope.ServiceProvider.GetService<CompletedAuditViewModel>();
                    completedAuditViewModel.Period = Settings.ReportInterval;
                    completedAuditViewModel.ReportAuditTemplateId = Settings.ReportAuditId;
                    completedAuditViewModel.Report = report;
                    await NavigationService.NavigateAsync(viewModel: completedAuditViewModel);
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
                    Settings.ReportAuditId = 0;

                    IsBusyQ1 = true;
                    IsBusyQ2 = true;
                    IsBusyQ3 = true;
                    IsBusyQ4 = true;

                    await LoadAll();

                    await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ReportAreaChanged); });
                }
            }
        }
        private void SetTitle()
        {
            if (SelectedWorkArea != null)
            {
                string result = "{0} ";
                Title = string.Format(result.ReplaceLanguageVariablesCumulative(), SelectedWorkArea.Name);
            }
        }

        private void ChangeStatus(object obj)
        {
            if (obj is SelectionChangedEventArgs eventArgs && eventArgs.AddedItems.FirstOrDefault() is string)
            {
                SelectedStatus = eventArgs.AddedItems.FirstOrDefault().ToString();

                if (SelectedStatus == skipped)
                {
                    TaskStatus = TaskStatusEnum.Skipped;
                }
                else
                {
                    TaskStatus = TaskStatusEnum.NotOk;
                }

                LoadDeviations();
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
