using Autofac;
using CommunityToolkit.Mvvm.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Messages;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Areas;
using EZGO.Maui.Core.Models.Statuses;
using EZGO.Maui.Core.Models.Tags;
using EZGO.Maui.Core.Services.Actions;
using EZGO.Maui.Core.Utils;
using Syncfusion.TreeView.Engine;
using System.Windows.Input;
using ItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class ActionViewModel : BaseViewModel
    {
        private readonly IWorkAreaService _workAreaService;

        #region actions dropdown
        string myActions => TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenIAmInvolvedIn);

        string assignedActions => TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenAssignedToMe);

        string startedActions => TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenStartedByMe);

        string allActions => TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenAllActions);

        public FilterControl<BasicActionsModel, ActionStatusEnum> ActionFilterControl { get; set; } = new FilterControl<BasicActionsModel, ActionStatusEnum>(null);
        public List<ITreeDropdownFilterItem> FilterOptions { get; set; }

        /// <summary>
        /// Gets or sets the name of the dropdown selector.
        /// </summary>
        /// <value>
        /// The name of the selected shifttype
        /// </value>
        public ITreeDropdownFilterItem FilterName { get; set; }

        public Rect Rect { get; set; } = new Rect(150, .12, .4, .35);

        #endregion

        #region work areas dropdown

        public List<ITreeDropdownFilterItem> WorkAreas { get; set; }

        public List<BasicWorkAreaModel> FlattenedWorkAreas { get; set; }

        public ITreeDropdownFilterItem SelectedWorkArea { get; set; }

        public Rect AreaRect { get; set; }

        public bool IsWorkAreaDropdownOpen { get; set; }

        #endregion

        #region Public Properties

        private List<ActionsModel> _Actions;

        public List<BasicActionsModel> Actions { get; set; }

        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;

                OnPropertyChanged();
            }
        }

        public bool IsSearchBarVisible { get; set; }

        public bool IsWorkAreaFilterVisible { get; set; } = false;

        public bool ActionOnTheSpotEnabled { get; private set; } = CompanyFeatures.CompanyFeatSettings.ActionOnTheSpotEnabled;

        /// <summary
        /// Indicated if the load more option should be available.
        /// In other words if there are more items available to load.
        /// </summary>
        public bool CanLoadMore { get; set; }

        #endregion

        #region Commands

        public IAsyncRelayCommand<TreeViewNode> DropdownTapCommand { get; private set; }
        public IAsyncRelayCommand<TreeViewNode> WorkAreaDropdownTapCommand { get; private set; }
        public ICommand ToggleWorkAreaDropdownCommand { get; private set; }
        public ICommand ToggleActionFilterDropdownCommand { get; private set; }
        public ICommand CloseWorkAreaDropdownCommand { get; private set; }
        public IAsyncRelayCommand<BasicActionsModel> ActionSolvedCommand { get; private set; }
        public IAsyncRelayCommand<object> NavigateToConversationCommand { get; private set; }
        public IAsyncRelayCommand NavigateToNewActionCommand { get; private set; }
        public ICommand FilterCommand { get; private set; }
        public ICommand SearchTextChangedCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand CloseDropdownCommand { get; private set; }
        public ICommand DeleteTagCommand { get; private set; }

        #endregion

        public ActionViewModel(
          INavigationService navigationService,
          IUserService userService,
          IMessageService messageService,
          IActionsService actionsService,
          IWorkAreaService workAreaService) : base(navigationService, userService, messageService, actionsService)
        {
            _workAreaService = workAreaService;

            DropdownTapCommand = new AsyncRelayCommand<TreeViewNode>(async filter =>
            {
                await ExecuteLoadingActionAsync(async () => await DropdownTapAsync(filter));
            }, CanExecuteCommands);

            ToggleWorkAreaDropdownCommand = new Command(() => IsWorkAreaDropdownOpen = !IsWorkAreaDropdownOpen, CanExecuteCommands);
            CloseWorkAreaDropdownCommand = new Command(() => IsWorkAreaDropdownOpen = false, CanExecuteCommands);
            ToggleActionFilterDropdownCommand = new Command(() => { IsDropdownOpen = !IsDropdownOpen; IsWorkAreaDropdownOpen = false; }, CanExecuteCommands);

            WorkAreaDropdownTapCommand = new AsyncRelayCommand<TreeViewNode>(WorkAreaDropdownTapAsync, CanExecuteCommands);

            ActionSolvedCommand = new AsyncRelayCommand<BasicActionsModel>(async action =>
            {
                await ExecuteLoadingAction(async () => await ToggleActionStatusAsync(action, ActionStatusEnum.Solved));
            }, CanExecuteCommands);

            NavigateToConversationCommand = new AsyncRelayCommand<object>(async obj =>
            {
                await ExecuteLoadingAction(async () => await NavigateToConversationAsync(obj));
            }, CanExecuteCommands);

            NavigateToNewActionCommand = new AsyncRelayCommand(async () => await NavigateToNewActionAsync(), CanExecuteCommands);

            FilterCommand = new Command<object>((status) => ExecuteLoadingAction(() => ActionFilterControl.Filter(status as ActionStatusEnum?, false, useDataSource: false)), CanExecuteCommands);

            SearchTextChangedCommand = new Command((obj) =>
            {
                if (obj is string searchText)
                    ActionFilterControl.SearchText = searchText;
                ActionFilterControl.Filter(ActionFilterControl.StatusFilters, false, useDataSource: false);
                CalculateActionAmounts();
            });

            LoadMoreCommand = new Command(() => { });

            CloseDropdownCommand = new Command(() =>
            {
                IsDropdownOpen = false;
                IsWorkAreaDropdownOpen = false;
            }, CanExecuteCommands);

            DeleteTagCommand = new Command<Syncfusion.Maui.ListView.ItemTappedEventArgs>(obj =>
            {
                if (obj.DataItem is TagModel tag)
                {
                    ActionFilterControl.SearchedTags.Remove(tag);
                    tag.IsActive = !tag.IsActive;
                    ActionFilterControl.Filter(false, false);
                }
            }, CanExecuteCommands);
        }

        public override async Task Init()
        {
            ActionFilterControl.AddFilters(new FilterModel(myActions), new FilterModel(assignedActions), new FilterModel(startedActions));
            if (UserSettings.Role != RoleTypeEnum.Basic.ToString().ToLower()) ActionFilterControl.FilterCollection.Add(new FilterModel(allActions));
            ActionFilterControl.SetSelectedFilter(myActions);

            FilterOptions = new List<ITreeDropdownFilterItem>(ActionFilterControl.FilterCollection);
            FilterName = ActionFilterControl.SelectedFilter;
            Settings.SubpageActions = MenuLocation.Actions;

            _ = Task.Run(RegisterMessagingCenter);
            await Task.Run(async () => await LoadActionsBasedOnFilterAsync());

            await base.Init();
        }

        private void RegisterMessagingCenter()
        {
            MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionsChanged, async (settingsViewModel) =>
            {
                await LoadActionsBasedOnFilterAsync();
            });

            MessagingCenter.Subscribe<ActionsService>(this, Constants.ActionChanged, async (sender) =>
            {
                await ItemChanged();
            });

            MessagingCenter.Subscribe<ActionsService, ActionChangedMessageArgs>(this, Constants.ActionChanged, async (sender, args) =>
            {
                await ItemChanged();
            });

            MessagingCenter.Subscribe<ActionsService, ActionCommentModel>(this, Constants.ChatChanged, async (sender, comment) =>
            {
                await LoadActionsBasedOnFilterAsync();
            });
        }

        // Async version of DropdownTap for AsyncRelayCommand
        private async Task DropdownTapAsync(TreeViewNode node)
        {
            IsDropdownOpen = false;
            IsBusy = true;

            if (node?.Content is FilterModel filterOption)
            {
                if (filterOption.Name != ActionFilterControl.SelectedFilter.Name)
                {
                    ActionFilterControl.SelectedFilter = filterOption;
                    await LoadActionsBasedOnFilterAsync();
                }
            }
            IsBusy = false;
        }

        private async Task WorkAreaDropdownTapAsync(TreeViewNode treeViewNode)
        {
            IsWorkAreaDropdownOpen = false;
            if (treeViewNode.Content is BasicWorkAreaModel workAreaModel)
            {
                SelectedWorkArea = workAreaModel;
                await LoadActionsBasedOnFilterAsync();
            }
        }
        private async Task LoadWorkAreasAsync()
        {
            var workAreas = new List<ITreeDropdownFilterItem>();
            var allWorkAreas = await _workAreaService.GetBasicWorkAreasAsync();
            FlattenedWorkAreas = _workAreaService.GetFlattenedBasicWorkAreas(allWorkAreas);
            var onTheSpotArea = new BasicWorkAreaModel() { Id = 0, Name = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsOnTheSpotName) };
            var allActionsArea = new BasicWorkAreaModel() { Id = 0, Name = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionsScreenAllAreas), Children = new List<ITreeDropdownFilterItem>(allWorkAreas) };

            workAreas.Add(allActionsArea);
            workAreas.Add(onTheSpotArea);

            WorkAreas = workAreas;

            if (WorkAreas.Count > 6)
                AreaRect = new Rect(339, .8, .4, .9);
            else
                AreaRect = new Rect(339, .2, .4, .6);


            SelectedWorkArea = FlattenedWorkAreas.FirstOrDefault(w => w.Id == Settings.WorkAreaId);
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<ActionsService>(this, Constants.ActionsChanged);
            MessagingCenter.Unsubscribe<ActionNewViewModel>(this, Constants.ActionChanged);
            MessagingCenter.Unsubscribe<ActionsService, ActionCommentModel>(this, Constants.ChatChanged);
            MessagingCenter.Unsubscribe<ActionsService, ActionChangedMessageArgs>(this, Constants.ActionChanged);
            Actions = null;
            _Actions = null;
            ActionFilterControl.Dispose();
            ActionFilterControl = null;
            FilterOptions.Clear();
            FilterOptions = null;
            WorkAreas?.Clear();
            WorkAreas = null;
            FlattenedWorkAreas = null;
            SelectedWorkArea = null;
            _workAreaService.Dispose();
            base.Dispose(disposing);
        }

        protected override void RefreshCanExecute()
        {
            base.RefreshCanExecute();
            (FilterCommand as Command)?.ChangeCanExecute();
        }

        private async Task ItemChanged()
        {
            await AsyncAwaiter.AwaitAsync(nameof(ActionViewModel) + nameof(ItemChanged), async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                await Task.Run(async () => await LoadActionsBasedOnFilterAsync());
            });
        }

        protected override async Task RefreshAsync()
        {
            await LoadActionsBasedOnFilterAsync();
            ActionFilterControl.RefreshStatusFilter();
            ActionFilterControl.Filter(ActionFilterControl.StatusFilters, false, useDataSource: false);
        }

        private async Task LoadActionsBasedOnFilterAsync()
        {
            if (_actionService == null) return;

            _Actions = null;

            if (ActionFilterControl.SelectedFilter != null)
            {
                var filterName = ActionFilterControl.SelectedFilter.Name;
                if (filterName == allActions)
                    await LoadActionsAsync();
                else if (filterName == assignedActions)
                    await LoadAssignedActionsAsync();
                else if (filterName == startedActions)
                    await LoadCreatedActionsAsync();
                else
                    await LoadMyActionsAsync();
            }
            else
                await LoadMyActionsAsync();


            CalculateActionAmounts();
            SetActions();
        }

        private void SetActions()
        {
            Actions = _Actions.ToBasicList<BasicActionsModel, ActionsModel>().SortActions();
            if (Actions.Any())
            {
                foreach (BasicActionsModel action in Actions)
                {
                    if (action.LocalMediaItems != null && action.LocalMediaItems.Any())
                    {
                        action.RetrieveImagesOffline = true;

                        IEnumerable<string> imageUrls = action.LocalMediaItems.Select(item => item.PictureUrl);

                        action.Image1 = imageUrls.ElementAtOrDefault(0);
                        action.Image2 = imageUrls.ElementAtOrDefault(1);
                        action.Image3 = imageUrls.ElementAtOrDefault(2);
                        action.Image4 = imageUrls.ElementAtOrDefault(3);
                        action.Image5 = imageUrls.ElementAtOrDefault(4);
                        action.Image6 = imageUrls.ElementAtOrDefault(5);

                        action.MediaCount = imageUrls.Count();
                        action.HasImages = imageUrls.Any();
                    }
                }
            }

            ActionFilterControl ??= new FilterControl<BasicActionsModel, ActionStatusEnum>(null);

            ActionFilterControl.SetUnfilteredItems(Actions);
            ActionFilterControl.RefreshStatusFilter();
            ActionFilterControl.Filter(ActionFilterControl.StatusFilters, false, useDataSource: false);
            CalculateActionAmounts();
        }

        private async Task LoadMyActionsAsync()
        {
            _Actions = await _actionService.GetMyActionsAsync(createdByOrAssignedToMe: true, refresh: IsRefreshing);
            IsWorkAreaFilterVisible = false;
        }

        private async Task LoadAssignedActionsAsync()
        {
            _Actions = await _actionService.GetAssignedUserActionsAsync(id: UserSettings.Id, refresh: IsRefreshing);
            IsWorkAreaFilterVisible = false;
        }

        private async Task LoadCreatedActionsAsync()
        {
            _Actions = await _actionService.GetCreatedActionsAsync(id: UserSettings.Id, refresh: IsRefreshing);
            IsWorkAreaFilterVisible = false;
        }


        private async Task LoadActionsAsync()
        {
            if (_actionService == null)
                return;

            _Actions = await _actionService.GetActionsAsync(refresh: IsRefreshing);
            IsWorkAreaFilterVisible = true;
            if (SelectedWorkArea != null && SelectedWorkArea.Id != 0)
            {
                _Actions = null;
                var actions = await _actionService.GetActionsWithAssignedAreaAsync(refresh: IsRefreshing, SelectedWorkArea.Id);
                _Actions = actions.ToList();
            }
            else
            {
                if (_actionService == null)
                {
                    using var scope = App.Container.CreateScope();
                    _actionService = scope.ServiceProvider.GetRequiredService<IActionsService>();
                }

                _Actions = await _actionService.GetActionsAsync(refresh: IsRefreshing);
                if (SelectedWorkArea?.Id == 0 && SelectedWorkArea?.Children == null)
                {
                    _Actions = _Actions.Where(a => a.Parent.ActionId == a.Id &&
                        a.Parent.AuditId == null &&
                        a.Parent.AuditTemplateId == null &&
                        a.Parent.ChecklistId == null &&
                        a.Parent.ChecklistTemplateId == null &&
                        a.Parent.TaskId == null &&
                        a.Parent.TaskTemplateId == null
                    ).ToList();
                }
            }
            IsWorkAreaFilterVisible = true;
        }

        private void CalculateActionAmounts()
        {
            if (ActionFilterControl == null)
                return;

            var statuses = StatusFactory.CreateStatus<ActionStatusEnum>();

            ActionFilterControl.CountItemsByStatus(ActionStatusEnum.PastDue, statuses);
            ActionFilterControl.CountItemsByStatus(ActionStatusEnum.Unsolved, statuses);
            ActionFilterControl.CountItemsByStatus(ActionStatusEnum.Solved, statuses);
            ActionFilterControl.SetStatusSelected(statuses);
            ActionFilterControl.SetStatusPercentages(statuses);

            ActionFilterControl.TaskStatusList = statuses;
        }

        private async Task ToggleActionStatusAsync(BasicActionsModel action, ActionStatusEnum status)
        {
            if (status == ActionStatusEnum.Solved && action != null && _actionService != null)
            {
                if (action.FilterStatus != status)
                {

                    Page page = NavigationService.GetCurrentPage();

                    string confirm = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertConfirmAction);
                    string yes = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertYesButtonTitle);
                    string no = TranslateExtension.GetValueFromDictionary(LanguageConstants.alertNoButtonTitle);

                    string result = await page.DisplayActionSheet(confirm, null, null, yes, no);

                    if (result == yes && await _actionService?.SetActionResolvedAsync(action))
                    {
                        action.FilterStatus = status;
                        CalculateActionAmounts();
                    }

                    if (_statusBarService != null)
                        _statusBarService.HideStatusBar();
                }
            }
        }

        private async Task NavigateToConversationAsync(object obj)
        {
            BasicActionsModel action = null;

            if (obj is ItemTappedEventArgs eventArgs && eventArgs.DataItem is BasicActionsModel)
                action = (BasicActionsModel)eventArgs.DataItem;
            else if (obj is BasicActionsModel)
                action = (BasicActionsModel)obj;

            ActionConversationViewModel vm = null;

            if (action != null)
            {
                if (vm == null)
                {
                    using var scope = App.Container.CreateScope();
                    vm = scope.ServiceProvider.GetService<ActionConversationViewModel>();
                    vm.SelectedAction = action;
                    vm.Actions = ActionFilterControl.FilteredList;
                }

                await NavigationService.NavigateAsync(viewModel: vm);
            }
        }

        private async Task NavigateToNewActionAsync()
        {
            using var scope = App.Container.CreateScope();
            var actionNewViewModel = scope.ServiceProvider.GetService<ActionNewViewModel>();
            actionNewViewModel.IsFromHomeScreen = true;
            actionNewViewModel.ActionType = ActionType.Task;
            await NavigationService.NavigateAsync(viewModel: actionNewViewModel);
        }
    }
}
