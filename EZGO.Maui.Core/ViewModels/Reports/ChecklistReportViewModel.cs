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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class ChecklistReportViewModel : BaseViewModel, IReportArea
    {
        private readonly IReportService _reportService;

        private readonly IWorkAreaService _workAreaService;


        #region Status

        string notOk => TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsNotOKFilter);

        string skipped => TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsSkippedFilter);

        string todo => TranslateExtension.GetValueFromDictionary(LanguageConstants.reportsNotDoneFilter);

        public ObservableCollection<string> Status { get { return new ObservableCollection<string>(new List<string> { notOk, skipped }); } }

        public string SelectedStatus { get; set; }

        public TaskStatusEnum TaskStatus { get; set; } = TaskStatusEnum.NotOk;

        #endregion

        #region Areas

        public List<BasicWorkAreaModel> WorkAreas { get; set; }

        public List<BasicWorkAreaModel> FlattenedWorkAreas { get; set; }

        public Rect Rect { get; set; } = new Rect(113, .2, .4, .6);

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

        #region Checklists

        public bool IsBusyQ1 { get; set; } = true;

        public bool IsBusyQ2 { get; set; } = true;

        public bool ShowPercentage { get; set; }

        private List<ReportsCount> checklistResult = new List<ReportsCount>();
        private List<ReportsCount> checklistItemsResult = new List<ReportsCount>();

        public bool HasItemsQ1 { get; set; } = true;

        public bool HasItemsQ2 { get; set; } = true;

        private List<ReportsCount> _checklists = new List<ReportsCount>();
        public List<ReportsCount> Checklists
        {
            get => _checklists;
            set
            {
                _checklists = value;
                SpanCount = Checklists.Count;
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

        private List<ReportsCount> _checklistItems;
        public List<ReportsCount> ChecklistItems
        {
            get => _checklistItems;
            set
            {
                _checklistItems = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Deviations

        public bool IsBusyQ3 { get; set; } = true;

        public bool IsBusyQ4 { get; set; } = true;

        public bool HasItemsQ3 { get; set; } = true;

        public bool HasItemsQ4 { get; set; } = true;

        private ReportChecklistDeviationsModel checklistDeviations;
        public ObservableCollection<BasicReportDeviationItemModel> Deviations { get; set; }

        #endregion

        public string Interval { get; set; }

        public ICommand NavigateToReportFilterCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToReportFilterAsync();
            });
        }, CanExecuteCommands);

        public ICommand NavigateToActionsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToActionsAsync(obj);
            });
        }, CanExecuteCommands);
        public ICommand NavigateToCompletedChecklistsCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () =>
            {
                await NavigateToCompletedChecklistsAsync(obj);
            });
        }, CanExecuteCommands);

        public ICommand ChangePercentageCommand => new Command(() =>
        {
            ExecuteLoadingAction(async () => { await ChangePercentage(); });
        }, CanExecuteCommands);

        public ICommand DropdownTapCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () => { await DropdownTap(obj); });
        }, CanExecuteCommands);

        public ICommand ChangeStatusCommand => new Command<object>(obj =>
        {
            ExecuteLoadingAction(async () => { await ChangeStatus(obj); });
        }, CanExecuteCommands);

        public ChecklistReportViewModel(
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
            Settings.SubpageReporting = MenuLocation.ReportChecklists;


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
                    await LoadAll();
                });
            }

            await base.Init();

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
                await LoadAll();
            });
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<MessageService, Message>(this, Constants.MessageCenterMessage);
                MessagingCenter.Unsubscribe<ReportFilterViewModel>(this, Constants.ReportPeriodChanged);
            });
            _workAreaService.Dispose();
            _reportService.Dispose();
            base.Dispose(disposing);
        }

        private async Task LoadAll()
        {
            checklistResult = await _reportService.GetChecklistsCountPerDateAsync(refresh: true);
            checklistItemsResult = await _reportService.GetChecklistItemsCountPerStatePerDateAsync(refresh: true);

            LoadChecklists();

            checklistDeviations = await _reportService.GetChecklistDeviationsAsync(period: Settings.ReportInterval, refresh: true);

            await LoadDeviations();
        }

        protected override async Task RefreshAsync()
        {
            await LoadAll();
        }

        private void LoadChecklists()
        {
            Checklists = new List<ReportsCount>();
            ChecklistItems = new List<ReportsCount>();

            List<ReportsCount> intervalResult = _reportService.GetIntervalCollection(Settings.ReportInterval, DateTime.Today);

            List<ReportsCount> groupedResult = new List<ReportsCount>();
            List<ReportsCount> mergedResult = new List<ReportsCount>();
            List<ReportsCount> mergedItemsResult = new List<ReportsCount>();

            int maxcount = 0;

            if (checklistItemsResult.Any())
            {
                checklistItemsResult.ForEach(x =>
                {
                    try
                    {
                        x.ReportDate = new DateTime(x.Year, x.Month, x.Day);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.StackTrace.ToString());
                    }
                });
            }

            if (checklistResult.Any())
            {
                switch (Settings.ReportInterval)
                {
                    case TimespanTypeEnum.LastTwelveDays:
                        checklistResult.ForEach(x =>
                        {
                            if (x.Name.IsNullOrEmpty()) return;
                            x.ReportDate = Convert.ToDateTime(x.Name);
                        });

                        mergedResult = (from x in intervalResult
                                        join y in checklistResult on (x.ReportDate.Date) equals (y.ReportDate.Date) into xy
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
                                            Year = x.Year
                                        }).ToList();

                        maxcount = mergedResult.Max(x => x.CountNr);
                        mergedResult.ForEach(x => x.MaxCountNr = maxcount);

                        groupedResult = checklistItemsResult.GroupBy(x => x.ReportDate).Select(g => new ReportsCount
                        {
                            ReportDate = g.Key,
                            Status = "compiled",
                            CountNr = g.Sum(x => x.CountNr),
                            NrNotOk = g.Where(x => x.Status == "not ok").Sum(x => x.CountNr),
                            NrOk = g.Where(x => x.Status == "ok").Sum(x => x.CountNr),
                            NrSkipped = g.Where(x => x.Status == "skipped").Sum(x => x.CountNr),
                            NrTodo = g.Where(x => x.Status == "todo").Sum(x => x.CountNr),
                        }).ToList();

                        mergedItemsResult = (from x in intervalResult
                                             join y in groupedResult on (x.ReportDate) equals (y.ReportDate) into xy
                                             from z in xy.DefaultIfEmpty()
                                             select new ReportsCount
                                             {
                                                 CountNr = z?.CountNr ?? 0,
                                                 Day = x.Day,
                                                 DayOfYear = x.DayOfYear,
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
                                                 Year = x.Year
                                             }).ToList();

                        break;

                    case TimespanTypeEnum.LastTwelveWeeks:

                        checklistResult.ForEach(x => x.Name = String.Format("count per week of year {0}-{1}", x.Year, x.Week.ToString("D2")));
                        groupedResult = checklistResult.GroupBy(x => x.Name).Select(g => new ReportsCount
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
                                            Name = x.Name,
                                            Subscript = x.Subscript,
                                            Week = x.Week,
                                            Year = x.Year,
                                            ReportDate = x.ReportDate
                                        }).ToList();

                        maxcount = mergedResult.Max(x => x.CountNr);
                        mergedResult.ForEach(x => x.MaxCountNr = maxcount);

                        checklistItemsResult.ForEach(x => x.Name = String.Format("{0}-{1}", x.Year, x.Week.ToString("D2")));
                        groupedResult = checklistItemsResult.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Week = g.FirstOrDefault()?.Week ?? 0,
                            Status = "compiled week",
                            CountNr = g.Sum(x => x.CountNr),
                            NrNotOk = g.Where(x => x.Status == "not ok").Sum(x => x.CountNr),
                            NrOk = g.Where(x => x.Status == "ok").Sum(x => x.CountNr),
                            NrSkipped = g.Where(x => x.Status == "skipped").Sum(x => x.CountNr),
                            NrTodo = g.Where(x => x.Status == "todo").Sum(x => x.CountNr),
                            Name = String.Format("count per week of year {0}", g.Key)
                        }).ToList();

                        mergedItemsResult = (from x in intervalResult
                                             join y in groupedResult on (x.Name) equals (y.Name) into xy
                                             from z in xy.DefaultIfEmpty()
                                             select new ReportsCount
                                             {
                                                 CountNr = z?.CountNr ?? 0,
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
                                                 Year = x.Year
                                             }).ToList();
                        break;

                    case TimespanTypeEnum.LastTwelveMonths:
                    case TimespanTypeEnum.ThisYear:

                        checklistResult.ForEach(x => x.Name = String.Format("count per month of year {0}-{1}", x.Year, x.Month.ToString("D2")));
                        groupedResult = checklistResult.GroupBy(x => x.Name).Select(g => new ReportsCount
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
                                            Name = x.Name,
                                            Subscript = x.Subscript,
                                            Month = z?.Month ?? x.Month,
                                            Year = z?.Year ?? x.Year,
                                            ReportDate = x.ReportDate
                                        }).ToList();

                        maxcount = mergedResult.Max(x => x.CountNr);
                        mergedResult.ForEach(x => x.MaxCountNr = maxcount);

                        checklistItemsResult.ForEach(x => x.Name = String.Format("{0}-{1}", x.Year, x.Month.ToString("D2")));
                        groupedResult = checklistItemsResult.GroupBy(x => x.Name).Select(g => new ReportsCount
                        {
                            Status = "compiled month",
                            CountNr = g.Sum(x => x.CountNr),
                            NrNotOk = g.Where(x => x.Status == "not ok").Sum(x => x.CountNr),
                            NrOk = g.Where(x => x.Status == "ok").Sum(x => x.CountNr),
                            NrSkipped = g.Where(x => x.Status == "skipped").Sum(x => x.CountNr),
                            NrTodo = g.Where(x => x.Status == "todo").Sum(x => x.CountNr),
                            Name = String.Format("count per month of year {0}", g.Key)
                        }).ToList();

                        mergedItemsResult = (from x in intervalResult
                                             join y in groupedResult on (x.Name) equals (y.Name) into xy
                                             from z in xy.DefaultIfEmpty()
                                             select new ReportsCount
                                             {
                                                 CountNr = z?.CountNr ?? 0,
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
                                                 Month = x.Month,
                                                 Year = x.Year
                                             }).ToList();
                        break;
                }

                Checklists = mergedResult;
                ChecklistItems = mergedItemsResult;
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

            HasItemsQ1 = Checklists.Any();
            OnPropertyChanged(nameof(Checklists));
            HasItemsQ2 = ChecklistItems.Any();
            OnPropertyChanged(nameof(ChecklistItems));
            IsBusyQ1 = false;
            IsBusyQ2 = false;
        }

        private async Task LoadDeviations()
        {
            var mydeviations = new List<ReportDeviationItemModel>();
            var mybasicdeviations = new List<BasicReportDeviationItemModel>();
            double maxcount = 0;

            if (checklistDeviations != null)
            {
                switch (TaskStatus)
                {
                    case TaskStatusEnum.NotOk:
                        mydeviations = checklistDeviations.DeviationsNotOk;
                        break;
                    case TaskStatusEnum.Skipped:
                        mydeviations = checklistDeviations.DeviationsSkipped;
                        break;
                }
            }

            if (mydeviations.Any())
            {
                mybasicdeviations = mydeviations.ToBasicList<BasicReportDeviationItemModel, ReportDeviationItemModel>();
                if (ShowPercentage)
                {
                    mybasicdeviations = mybasicdeviations.OrderByDescending(x => x.CountNr).Take(5).ToList();
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

        private async Task ChangePercentage()
        {
            ShowPercentage = !ShowPercentage;
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
                        actionReportActionsViewModel.ReportActionTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.checklistActionsTitle);
                        await NavigationService.NavigateAsync(viewModel: actionReportActionsViewModel);
                    }
                }
            }
        }

        private async Task NavigateToCompletedChecklistsAsync(object obj)
        {
            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is ReportsCount report)
            {
                if (report.CountNr > 0)
                {
                    using var scope = App.Container.CreateScope();
                    var completedChecklistsViewModel = scope.ServiceProvider.GetService<CompletedChecklistsViewModel>();
                    completedChecklistsViewModel.Period = Settings.ReportInterval;
                    completedChecklistsViewModel.Report = report;
                    await NavigationService.NavigateAsync(viewModel: completedChecklistsViewModel);
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

        private async Task ChangeStatus(object obj)
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

                await LoadDeviations();
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
