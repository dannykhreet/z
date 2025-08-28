using CommunityToolkit.Mvvm.Input;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Areas;
using EZGO.Maui.Core.Interfaces.Cache;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Areas;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Menu;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    /// <summary>
    /// Work area view model.
    /// </summary>
    public class WorkAreaViewModel : BaseViewModel
    {
        private readonly IWorkAreaService _workAreaService;
        private readonly ISyncService _syncService;
        private readonly ICachingService _cachingService;

        public bool IsBusy { get; set; }

        public string NetworkTime { get; set; }

        public bool NetworkTimeIsVisible { get; set; }

        /// <summary>
        /// Work areas.
        /// </summary>
        public ObservableCollection<BasicWorkAreaModel> WorkAreas { get; set; }

        /// <summary>
        /// Boolean indicating if an area is selected.
        /// </summary>
        private bool _isAreaSelected;
        public bool IsAreaSelected
        {
            get => _isAreaSelected;
            set
            {
                if (_isAreaSelected == value) return;

                _isAreaSelected = value;
                OnPropertyChanged(nameof(IsAreaSelected));
            }
        }

        public bool DownloadMedia { get; set; }

        public BasicWorkAreaModel SelectedWorkArea { get; set; }

        /// <summary>
        /// Indicates whether the page should perform sync-related operations
        /// </summary>
        /// <remarks>Used for when there's a need to select an area for e.g. task template editing</remarks>
        public bool ChooseAreaOnly { get; set; }

        /// <summary>
        /// Event to fire when <see cref="ChooseAreaOnly"/> is set to <see langword="true"/> and the user selects an area
        /// </summary>
        public event Action<BasicWorkAreaModel> AreaSelected = (a) => { };

        #region Commands

        /// <summary>
        /// Select work area command.
        /// </summary>
        public ICommand SelectWorkAreaCommand => new Command(workArea =>
        {
            SelectWorkArea((BasicWorkAreaModel)workArea);
        }, CanExecuteCommands);

        /// <summary>
        /// Continue button command.
        /// </summary>
        public IAsyncRelayCommand ContinueButtonCommand => new AsyncRelayCommand(async () =>
         {
             await ContinueAsync();
         }, CanExecuteCommands);
        /// <summary>
        /// Shows popup for profile, logout navigation
        /// </summary>
        public ICommand PopupCommand => new Command(() =>
        {
            ExecuteLoadingAction(PopupAsync);
        }, CanExecuteCommands);

        /// <summary>
        /// Shows popup for profile, logout navigation
        /// </summary>
        public ICommand QRButtonCommand => new Command(() =>
        {
            ExecuteLoadingAction(() =>
            {
                using var scope = App.Container.CreateScope();
                var menuViewModel = scope.ServiceProvider.GetService<MenuViewModel>();
                menuViewModel.NavigateToQRScannerCommand.Execute(null);
            });
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the toggle children command.
        /// </summary>
        /// <value>
        /// The toggle children command.
        /// </value>
        public ICommand ToggleChildrenCommand => new Command(workAreaModel =>
        {
            ToggleChildren((BasicWorkAreaModel)workAreaModel);
        }, CanExecuteCommands);

        public override bool CanExecuteCommands()
        {
            return !IsBusy && !IsLoading && !IsRefreshing;
        }

        public override bool CanExecuteCommands(object commandParameter)
        {
            return !IsBusy && !IsLoading && !IsRefreshing;
        }


        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorkAreaViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IWorkAreaService workAreaService,
            ISyncService syncService) : base(navigationService, userService, messageService, actionsService)
        {
            _workAreaService = workAreaService;
            _syncService = syncService;
            _cachingService = DependencyService.Get<ICachingService>();
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            Settings.AppSettings.MenuLocation = MenuLocation.None;
            _syncService.StopMediaDownload();
            await base.Init();

            await MessageHelper.ErrorMessageIsNotSent(_messageService);

            if (ChooseAreaOnly == false)
            {
                MessagingCenter.Subscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage, (viewModel) =>
                {
                    SetUserData();
                });

                await CompareTimes();
                await MessageHelper.ErrorMessageIsNotSent(_messageService);

                Settings.AreaSettings.WorkAreaId = 0;
            }
            await Task.Run(async () => await LoadWorkAreasAsync());
        }

        protected override void Dispose(bool disposing)
        {
            MessagingCenter.Unsubscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage);
            _workAreaService.Dispose();
            _syncService.Dispose();
            //_cachingService.ClearOutOfDateCache

            base.Dispose(disposing);
        }

        private async Task CompareTimes()
        {
            try
            {
#if DEBUG
                DateTime NetworkTimeUtc = await _workAreaService.GetsServerTimeUtcAsync();
                DateTime deviceTime = DateTime.UtcNow;

                DateTime deviceTimeMinusOne = deviceTime.AddMinutes(-1);
                DateTime deviceTimePlusOne = deviceTime.AddMinutes(1);

                double offset = 0;

                if (deviceTimeMinusOne > NetworkTimeUtc || deviceTimePlusOne < NetworkTimeUtc)
                {
                    //messageService.SendMessage("Time offset error", Color.Red, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.General);
                    offset = deviceTime.Subtract(NetworkTimeUtc).TotalSeconds;
                    // Analytics.TrackEvent("Time offset", new Dictionary<string, string>() {
                    //     { "Company", string.Format("{0} ({1})", UserSettings.CompanyName.ToString(), UserSettings.CompanyId.ToString()) },
                    //     { "Role", UserSettings.Role.ToString() },
                    //     { "Device time", deviceTime.ToString() },
                    //     { "Server time", NetworkTimeUtc.ToString() }
                    // });
                }

                NetworkTime = string.Format("Server Time Utc: {0}, offset: {1}s", NetworkTimeUtc, offset);
                NetworkTimeIsVisible = true;
#endif
            }
            catch { }
        }

        /// <summary>
        /// Loads the work areas asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task LoadWorkAreasAsync()
        {
            var result = await _workAreaService.GetBasicWorkAreasAsync(IsRefreshing);
            WorkAreas = new ObservableCollection<BasicWorkAreaModel>(result);
            WorkAreas ??= new ObservableCollection<BasicWorkAreaModel>();
            HasItems = WorkAreas.Any();
            OnPropertyChanged(nameof(WorkAreas));
        }

        protected override async Task RefreshAsync()
        {
            await LoadWorkAreasAsync();
            await CompareTimes();
        }

        /// <summary>
        /// Selects a work area and deselects the other ones.
        /// </summary>
        /// <param name="workAreaModel">The selected work area model.</param>
        private void SelectWorkArea(BasicWorkAreaModel workAreaModel)
        {
            if (workAreaModel == null)
                return;

            if (SelectedWorkArea != null)
                SelectedWorkArea.IsSelected = false;

            workAreaModel.IsSelected = true;
            SelectedWorkArea = workAreaModel;
            IsAreaSelected = true;
        }

        /// <summary>
        /// Continues to the home view, when a work area is selected.
        /// </summary>
        /// <returns>Task that can be awaited.</returns>
        /// 
        private async Task ContinueAsync()
        {
            IsBusy = true;
            BasicWorkAreaModel selectedWorkArea = FindSelectedWorkArea(WorkAreas.ToList());
            if (await InternetHelper.HasInternetConnection())
            {
                if (ChooseAreaOnly == false)
                {
                    Settings.DownloadMedia = DownloadMedia;
                    if (selectedWorkArea != null)
                    {

                        Settings.WorkAreaId = selectedWorkArea.Id;
                        Settings.WorkAreaName = selectedWorkArea.Name;
                        Settings.ReportWorkAreaId = selectedWorkArea.Id;
                        Settings.AssessmentsWorkAreaId = selectedWorkArea.Id;
                        Settings.AllTaskWorkAreaId = selectedWorkArea.Id;

                        string message = TranslateExtension.GetValueFromDictionary(LanguageConstants.syncStatesViewSynsingMessage);

                        Color greenColor = ResourceHelper.GetApplicationResource<Color>("GreenColor");

                        _messageService?.SendMessage(message, greenColor, MessageIconTypeEnum.Spinner);

                        _cachingService?.ClearCache();
                        await DownloadDataAsync();

                        _messageService?.SendMessage(string.Empty, Colors.Transparent, MessageIconTypeEnum.None, messageType: MessageTypeEnum.Clear);

                        if (!Settings.Token.IsNullOrWhiteSpace())
                        {
                            await NavigationService?.NavigateAsync<HomeViewModel>();
                        }

                    }
                }
                else
                {
                    if (selectedWorkArea != null && AreaSelected != null)
                        AreaSelected.Invoke(selectedWorkArea);
                }
            }

            IsBusy = false;
        }

        /// <summary>
        /// Finds the selected work area.
        /// </summary>
        /// <param name="workAreas">The work areas.</param>
        /// <returns>The selected work area.</returns>
        private static BasicWorkAreaModel FindSelectedWorkArea(List<BasicWorkAreaModel> workAreas)
        {
            BasicWorkAreaModel selectedWorkArea = null;

            foreach (BasicWorkAreaModel basicWorkAreaModel in workAreas)
            {
                if (basicWorkAreaModel.IsSelected)
                {
                    selectedWorkArea = basicWorkAreaModel;
                    break;
                }

                if (basicWorkAreaModel.HasChildren)
                {
                    selectedWorkArea = FindSelectedWorkArea(basicWorkAreaModel.Children.Cast<BasicWorkAreaModel>().ToList());

                    if (selectedWorkArea != null)
                        break;
                }
            }

            return selectedWorkArea;
        }


        private async Task PopupAsync()
        {
            Page page = NavigationService.GetCurrentPage();

            string popupTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenLogoutMessageTitle);
            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            string logout = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenLogoutMessageText);
            string profile = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenEditProfileMessageText);

            string action = await page.DisplayActionSheet(popupTitle, null, cancel, logout, profile);

            _statusBarService.HideStatusBar();

            if (action == logout)
            {
                await UserStatusManager.SignOffAsync(NavigationService);
            }
            else if (action == profile)
            {
                if (!await InternetHelper.HasInternetConnection())
                {
                    string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.onlyOnlineAction);
                    action = await page.DisplayActionSheet(result, null, cancel);
                }
                else
                {
                    await NavigationService.NavigateAsync<ProfileViewModel>();
                }
            }
        }

        /// <summary>
        /// Toggles the children.
        /// </summary>
        /// <param name="workAreaModel">The work area model.</param>
        private static void ToggleChildren(BasicWorkAreaModel workAreaModel)
        {
            workAreaModel.IsRootExpanded = !workAreaModel.IsRootExpanded;
        }

        private async Task DownloadDataAsync()
        {
            await _syncService.GetLocalDataAsync();
        }
    }
}
