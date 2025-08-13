using Autofac;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Areas;
using EZGO.Maui.Core.Models.Messaging;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Services.Message;
using EZGO.Maui.Core.Services.Tasks;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Tasks.AllTasks;
using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.TreeView.Engine;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.AllTasks
{

    public class AllTasksViewModel : BaseViewModel
    {
        private List<object> _allAvailableItems;
        private List<string> checkedIntervals;

        #region Public Properties 

        public bool IsBusy { get; set; }

        public List<AllTasksListItemViewModel> AllTasks { get; set; }

        public FilterControl<AllTasksListItemViewModel, TaskStatusEnum> TaskFilter { get; set; } = new FilterControl<AllTasksListItemViewModel, TaskStatusEnum>(null);

        public List<BasicWorkAreaModel> AvailableAreas { get; set; } = new List<BasicWorkAreaModel>();

        public ObservableCollection<object> SelectedAreaFilterItems { get; set; }

        public List<BasicWorkAreaModel> CurrentAreaFilter { get; set; }

        private bool _isAreaSelectionOpen { get; set; }
        public bool IsAreaSelectionOpen
        {
            get => _isAreaSelectionOpen;
            set
            {
                _isAreaSelectionOpen = value;
                OnPropertyChanged();
                if (value)
                    SelectedAreaFilterItems = new ObservableCollection<object>(CurrentAreaFilter);
            }
        }

        public bool IsIntervalSelectionOpen { get; set; }

        public IntervalFilterViewModel IntervalFilter { get; set; } = new IntervalFilterViewModel();

        public bool IsSearchBarVisible { get; set; }

        public string SearchText { get; set; }

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        public DataSource DataSource { get; set; }

        #endregion

        #region Commands
        private bool canNavigate = true;
        public ICommand NavigateToAllTasksSlideCommand => new Command<AllTasksListItemViewModel>(async (selected) =>
        {
            if (canNavigate)
            {
                canNavigate = false;
                await NavigateToAllTasksSlide(selected);
                canNavigate = true;
            }
        }, CanExecuteCommands);

        public ICommand AreaSelectionCommand => new Command(() =>
        {
            IsAreaSelectionOpen ^= true;
        }, CanExecuteCommands);

        public ICommand IntervalSelectionCommand => new Command(() =>
        {
            IsIntervalSelectionOpen ^= true;
        }, CanExecuteCommands);

        public ICommand CloseSelectionCommand => new Command(() =>
        {
            IsIntervalSelectionOpen = false;
            IsAreaSelectionOpen = false;
        }, CanExecuteCommands);

        public ICommand SubmitAreaFilterCommand => new Command(() =>
        {
            IsAreaSelectionOpen = false;
            FilterAreas();
        }, CanExecuteCommands);

        public ICommand AllAreaFilterCommand => new Command(() =>
        {
            SelectedAreaFilterItems.Clear();
            _allAvailableItems.ForEach(x => SelectedAreaFilterItems.Add(x));
        }, CanExecuteCommands);

        public ICommand NoneAreaFilterCommand => new Command(() =>
        {
            SelectedAreaFilterItems.Clear();
        }, CanExecuteCommands);

        public ICommand SubmitIntervalCommand => new Command(() =>
        {
            IsIntervalSelectionOpen = false;
            FilterIntervals();
        }, CanExecuteCommands);

        public ICommand SearchTextChangedCommand => new Command((obj) =>
        {
            if (obj is string searchText)
                TaskFilter.SearchText = searchText;
            TaskFilter.Filter(TaskFilter.StatusFilters, false, useDataSource: false);
        }, CanExecuteCommands);

        public ICommand DeleteTagCommand => new Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>((obj) =>
        {
            if (obj.DataItem is TagModel tag)
            {
                TaskFilter.SearchedTags.Remove(tag);
                tag.IsActive = !tag.IsActive;
                TaskFilter.Filter(false, false);
            }
        }, CanExecuteCommands);

        public ICommand AreaFilterChanged => new Command<Syncfusion.Maui.TreeView.ItemSelectionChangedEventArgs>((args) =>
        {
            UpdateAreaSelection(args);
        }, CanExecuteCommands);


        #endregion

        #region Services 

        private readonly IWorkAreaService _areaService;
        private readonly ITaskTemplatesSerivce _taskTemplates;
        #endregion

        public IWorkAreaFilterControl WorkAreaFilterControl { get; set; }

        public ICommand DropdownTapCommand { get; set; }


        public AllTasksViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IWorkAreaService workAreaService,
            ITaskTemplatesSerivce taskTemplatesSerivce,
            IWorkAreaFilterControl workAreaFilterControl) : base(navigationService, userService, messageService, actionsService)
        {
            _areaService = workAreaService;
            _taskTemplates = taskTemplatesSerivce;
            WorkAreaFilterControl = workAreaFilterControl;

            DropdownTapCommand = new Command<TreeViewNode>((obj) =>
            {
                IsDropdownOpen = false;
                WorkAreaFilterControl.DropdownTapAsync(obj, () => LoadTaskTemplates(), Settings.AllTaskWorkAreaId);
                Settings.AllTaskWorkAreaId = WorkAreaFilterControl.SelectedWorkArea?.Id ?? Settings.WorkAreaId;
            }, CanExecuteCommands);
        }


        #region Initialize

        public override async Task Init()
        {
            Settings.SubpageTasks = MenuLocation.TasksAll;
            await Task.Run(async () => await WorkAreaFilterControl.LoadWorkAreasAsync(Settings.AllTaskWorkAreaId));

            if (!await MessageHelper.ErrorMessageIsNotSent(_messageService))
            {
                AllTasks ??= new List<AllTasksListItemViewModel>();
                checkedIntervals = IntervalFilter.GetChecked();
                HasItems = AllTasks.Count > 0;
            }
            else
            {
                IsBusy = true;
                await LoadTaskTemplates();
                IsBusy = false;
            }


            MessagingCenter.Subscribe<MessageService, Message>(this, Constants.MessageCenterMessage, async (formsApp, message) =>
            {
                if (message.MessageType == MessageTypeEnum.Clear)
                {
                    await LoadTaskTemplates();
                }
                else if (message.MessageType == MessageTypeEnum.Connection)
                {
                    AllTasks ??= new List<AllTasksListItemViewModel>();

                    checkedIntervals = IntervalFilter.GetChecked();
                    HasItems = AllTasks.Count > 0;
                }
            });

            MessagingCenter.Subscribe<TaskTemplatesService>(this, Constants.TaskTemplatesChanged, async (servcie) =>
            {
                IsBusy = true;
                await Task.Run(async () => await ReloadTaskTemplates()).ConfigureAwait(false);
                IsBusy = false;
            });

            await Task.Run(async () => await base.Init());
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<MessageService, Message>(this, Constants.MessageCenterMessage);
                MessagingCenter.Unsubscribe<TaskTemplatesService>(this, Constants.TaskTemplatesChanged);
            });
            _areaService.Dispose();
            _taskTemplates.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        private List<BasicWorkAreaModel> flattenedAreas;

        protected override async Task RefreshAsync()
        {
            await ReloadTaskTemplates();
        }

        private async Task ReloadTaskTemplates()
        {
            try
            {
                var areaId = WorkAreaFilterControl?.SelectedWorkArea?.Id ?? Settings.AllTaskWorkAreaId;
                var tasks = await _taskTemplates.GetAllTemplatesForAreaAsync(areaId: areaId, refresh: IsRefreshing).ConfigureAwait(false);
                var idsFlatened = flattenedAreas.Select(x => x.Id).ToList();
                AllTasks = tasks
                    .Where(x => x.AreaId != null)
                    .Where(x => idsFlatened.Contains(x.AreaId.Value))
                    .Select(task => new AllTasksListItemViewModel(task))
                    .ToList();

                AllTasks.ForEach(task =>
                {
                    var area = flattenedAreas.SingleOrDefault(x => x.Id == task.AssignedAreaId);
                    var topmost = FindTopMostArea(area);
                    task.TopMostAreaName = topmost?.Name;
                    task.AssignedAreaName = area?.Name;
                });

                AllTasks ??= new List<AllTasksListItemViewModel>();
                checkedIntervals = IntervalFilter.GetChecked();

                HasItems = DataSource.Items.Count > 0;
            }
            catch (Exception ex)
            {
                //Crashes.TrackError(ex, new Dictionary<string, string>() { { "areaId", Settings.WorkAreaId.ToString() } });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadTaskTemplates()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var areaId = WorkAreaFilterControl?.SelectedWorkArea?.Id ?? Settings.AllTaskWorkAreaId;
                    var tasks = await _taskTemplates.GetAllTemplatesForAreaAsync(areaId: areaId, refresh: IsRefreshing).ConfigureAwait(false);
                    var area = await _areaService.GetWorkAreaAsync(areaId).ConfigureAwait(false);

                    area ??= new BasicWorkAreaModel();
                    var areas = new List<BasicWorkAreaModel> { area };
                    flattenedAreas = _areaService.GetFlattenedBasicWorkAreas(areas);
                    flattenedAreas ??= new List<BasicWorkAreaModel>();
                    var idsFlatened = flattenedAreas.Select(x => x.Id);
                    idsFlatened ??= new List<int>();

                    _allAvailableItems = flattenedAreas.Cast<object>().ToList();
                    AvailableAreas = areas;
                    SelectedAreaFilterItems = new ObservableCollection<object>(_allAvailableItems);
                    CurrentAreaFilter = new List<BasicWorkAreaModel>(flattenedAreas);


                    AllTasks = tasks
                        .Where(x => x.AreaId != null)
                        .Where(x => idsFlatened.Contains(x.AreaId.Value))
                        .Select(task => new AllTasksListItemViewModel(task))
                        .ToList();

                    AllTasks.ForEach(task =>
                    {
                        var area = flattenedAreas.SingleOrDefault(x => x.Id == task.AssignedAreaId);
                        var topmost = FindTopMostArea(area);
                        task.TopMostAreaName = topmost?.Name;
                        task.AssignedAreaName = area?.Name;
                        task.AssignedAreaFullName = area?.FullDisplayName;
                    });

                    AllTasks ??= new List<AllTasksListItemViewModel>();
                    checkedIntervals = IntervalFilter.GetChecked();
                    HasItems = AllTasks.Count > 0;

                    TaskFilter.SetUnfilteredItems(AllTasks);
                    TaskFilter.Filter(TaskFilter.StatusFilters, false, false);
                }
                catch (Exception ex)
                {
                    //Crashes.TrackError(ex, new Dictionary<string, string>() { { "areaId", Settings.WorkAreaId.ToString() } });
                }
            });
        }

        private BasicWorkAreaModel FindTopMostArea(BasicWorkAreaModel area)
        {
            while (area != null)
            {
                if (area.Parent == null)
                    return area;

                area = area.Parent;
            }

            return null;
        }

        /// <summary>
        /// Filters the task templates using area filter
        /// </summary>
        private void FilterAreas()
        {
            // Get selected items
            CurrentAreaFilter = new List<BasicWorkAreaModel>(SelectedAreaFilterItems.Cast<BasicWorkAreaModel>());

            // Refresh filter
            RefreshFilter();
        }

        /// <summary>
        /// Filters the task templates using interval filter
        /// </summary>
        private void FilterIntervals()
        {
            checkedIntervals = IntervalFilter.GetChecked();
            RefreshFilter();
        }

        /// <summary>
        /// Refreshed filter
        /// </summary>
        private void RefreshFilter()
        {
            DataSource.Filter = obj => obj is AllTasksListItemViewModel task
                && (CurrentAreaFilter == null || CurrentAreaFilter.Where(x => x.Id == task.AssignedAreaId).Any())
                && (checkedIntervals == null || checkedIntervals.Contains(task.RecurrencyType))
                && (SearchText == null || task.Name.ToUpperInvariant().Contains(SearchText.ToUpperInvariant()));

            DataSource.RefreshFilter();
            HasItems = DataSource.Items.Count > 0;
        }

        private async Task NavigateToAllTasksSlide(AllTasksListItemViewModel selected)
        {
            using var scope = App.Container.CreateScope();
            var allTasksSlideViewModel = scope.ServiceProvider.GetService<AllTasksSlideViewModel>();
            allTasksSlideViewModel.AllTasks = new List<BasicTaskModel>();
            List<AllTasksListItemViewModel> allTasks = new();
            DataSource.Items.ForEach(i => allTasks.Add((AllTasksListItemViewModel)i));
            allTasks.ForEach(t => allTasksSlideViewModel.AllTasks.Add(t.ToBasicTask()));
            allTasksSlideViewModel.CurrentIndex = allTasks.IndexOf(selected);
            allTasksSlideViewModel.SelectedTask = selected.ToBasicTask();
            await NavigationService.NavigateAsync(viewModel: allTasksSlideViewModel);
        }

        private void UpdateAreaSelection(Syncfusion.Maui.TreeView.ItemSelectionChangedEventArgs args)
        {
            foreach (var item in args.AddedItems)
            {
                if (item is BasicWorkAreaModel area)
                {
                    _areaService.GetFlattenedBasicWorkAreas(area.Children.Cast<BasicWorkAreaModel>().ToList()).ForEach(x =>
                    {
                        if (SelectedAreaFilterItems.Contains(x) == false)
                            SelectedAreaFilterItems.Add(x);
                    });
                }
            }

            foreach (var item in args.RemovedItems)
            {
                if (item is BasicWorkAreaModel area)
                {
                    _areaService.GetFlattenedBasicWorkAreas(area.Children.Cast<BasicWorkAreaModel>().ToList()).ForEach(x =>
                    {
                        if (SelectedAreaFilterItems.Contains(x) == true)
                            SelectedAreaFilterItems.Remove(x);
                    });
                }
            }
        }
    }
}
