using Autofac;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.MenuFeatures;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Services.Login;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Assessments;
using EZGO.Maui.Core.ViewModels.Bookmarks;
using EZGO.Maui.Core.ViewModels.Feed;
using System.Diagnostics;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels.Menu
{
    /// <summary>
    /// Menu view model.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.ViewModels.BaseViewModel" />
    public class MenuViewModel : BaseViewModel
    {
        private bool isLoading = false;

        /// <summary>
        /// Gets or sets the menu location.
        /// </summary>
        /// <value>
        /// The menu location.
        /// </value>
        public MenuLocation MenuLocation { get; set; }

        public IMenuManager MenuManager { get; set; }

        public bool IsReporting { get; set; } = false;

        public int PersonalComments { get; set; }

        public int TabIndex { get; set; }

        /// <summary>
        /// Gets the navigate to home command.
        /// </summary>
        /// <value>
        /// The navigate to home command.
        /// </value>
        public ICommand NavigateToHomeCommand { get; private set; }

        /// <summary>
        /// Gets the navigate to checklist templates command.
        /// </summary>
        /// <value>
        /// The navigate to checklist templates command.
        /// </value>
        public ICommand NavigateToChecklistTemplatesCommand { get; private set; }

        /// <summary>
        /// Gets the navigate to tasks command.
        /// </summary>
        /// <value>
        /// The navigate to tasks command.
        /// </value>
        public ICommand NavigateToTasksCommand { get; private set; }

        /// <summary>
        /// Gets the navigate to audits command.
        /// </summary>
        /// <value>
        /// The navigate to audits command.
        /// </value>
        public ICommand NavigateToAuditsCommand { get; private set; }

        /// <summary>
        /// Gets the navigate to report command.
        /// </summary>
        /// <value>
        /// The navigate to report command.
        /// </value>
        public ICommand NavigateToReportCommand { get; private set; }

        /// <summary>
        /// Gets the navigate to actions command.
        /// </summary>
        /// <value>
        /// The navigate to actions command.
        /// </value>
        public ICommand NavigateToActionsCommand { get; private set; }

        public ICommand NavigateToInstructionsCommand { get; private set; }

        public ICommand NavigateToAssessmentsCommand { get; private set; }

        public ICommand NavigateToFeedCommand { get; private set; }

        public ICommand NavigateToQRScannerCommand { get; private set; }

        public ICommand LoadNextPageButtonsCommand { get; set; }

        /// <summary>
        /// Shows popup for profile, logout navigation
        /// </summary>
        public ICommand PopupCommand { get; private set; }


        private IServiceScope scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuViewModel"/> class.
        /// </summary>
        public MenuViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IMenuManager menuManager) : base(navigationService, userService, messageService, actionsService)
        {
            MenuLocation = Settings.MenuLocation;

            MenuManager = menuManager;

            LoadUserData();

            MessagingCenter.Subscribe<SyncService, int>(this, Constants.MyActionsChanged, (service, value) =>
            {
                SetUserActions(value);
            });

            MessagingCenter.Subscribe<LoginService>(this, Constants.UserHasChanged, (sender) =>
            {
                LoadUserData();
            });

            MessagingCenter.Subscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage, (sender) =>
            {
                LoadUserData();
            });

            NavigateToHomeCommand = RegisterCommand(NavigateToHomeAsync);

            NavigateToChecklistTemplatesCommand = RegisterCommand(NavigateToChecklistTemplatesAsync);

            NavigateToTasksCommand = RegisterCommand(NavigateToTasksAsync);

            NavigateToAuditsCommand = RegisterCommand(NavigateToAuditsAsync);

            NavigateToReportCommand = RegisterCommand(NavigateToReportAsync);

            NavigateToActionsCommand = RegisterCommand(NavigateToActionsAsync);

            NavigateToInstructionsCommand = RegisterCommand(NavigateToInstructionsAsync);

            NavigateToAssessmentsCommand = RegisterCommand(NavigateToAssessmentsAsync);

            NavigateToQRScannerCommand = RegisterCommand(NavigateToQRScanner);

            NavigateToFeedCommand = RegisterCommand(NavigateToFeedAsync);

            LoadNextPageButtonsCommand = RegisterCommand(LoadNextPageButtons);

            PopupCommand = RegisterCommand(PopupAsync);
        }

        private Task LoadNextPageButtons()
        {
            return Task.CompletedTask;
        }

        private void LoadUserData()
        {
            Fullname = UserSettings.Fullname;
            Picture = UserSettings.UserPictureUrl != Constants.NoProfilePicture2 ? UserSettings.UserPictureUrl : null;
        }

        private ICommand RegisterCommand(Func<Task> action)
        {
            return new Command<object>((obj) =>
            {
                if (int.TryParse(obj?.ToString(), out int result))
                    TabIndex = result;
                ExecuteLoadingAction(() => NavigateAsync(action));
            });
        }

        public void InitLocation() => MenuLocation = Settings.MenuLocation;

        private async void SetUserActions(int? value = null)
        {
            if (value.HasValue)
            {
                PersonalComments = value.Value;
            }
            else
            {
                using var scope = App.Container.CreateScope();
                var actionService = scope.ServiceProvider.GetService<IActionsService>();
                var actions = await actionService.GetActionsAsync(withIncludes: true, includeLocalActions: true);
                PersonalComments = actions.Where(x => x.CreatedById == UserSettings.Id && x.IsResolved.HasValue && !x.IsResolved.Value).Count();
            }
        }

        private async void ExecuteLoadingAction(Func<Task> execute)
        {
            if (!isLoading && execute != null && CanNavigate || !IsLoading)
            {
                isLoading = true;
                await execute();
                await Task.Delay(100);
                isLoading = false;
            }
        }

        /// <summary>
        /// Navigates to home asynchronous.
        /// </summary>
        private async Task NavigateToHomeAsync()
        {
            // Prevent double clicking
            if (Settings.MenuLocation == MenuLocation.Home)
                return;
            IsReporting = false;
            Settings.MenuLocation = MenuLocation.Home;
            MenuLocation = MenuLocation.Home;

            await NavigationService.NavigateAsync<HomeViewModel>(noHistory: true, animated: false);
        }

        /// <summary>
        /// Navigates to checklist templates asynchronous.
        /// </summary>
        private async Task NavigateToChecklistTemplatesAsync()
        {
            // Prevent double clicking
            if (MenuLocation == MenuLocation.Checklist && IsRootPageTopMost())
                return;

            IsReporting = false;
            Settings.MenuLocation = MenuLocation.Checklist;
            MenuLocation = MenuLocation.Checklist;

            await NavigationService.PopOrNavigateToPage<ChecklistTemplatesViewModel>(typeof(ChecklistTemplatesViewModel));
        }

        /// <summary>
        /// Navigates to tasks asynchronous.
        /// </summary>
        private async Task NavigateToTasksAsync()
        {
            // Prevent double clicking
            if (MenuLocation == MenuLocation.Tasks && IsRootPageTopMost())
                return;

            IsReporting = false;
            if (MenuLocation == MenuLocation.Tasks && Settings.SubpageTasks != MenuLocation.None)
            {
                Settings.SubpageTasks = MenuLocation.None;
                await NavigationService?.CloseAsync();
            }
            else
            {
                Settings.MenuLocation = MenuLocation.Tasks;
                MenuLocation = MenuLocation.Tasks;

                await NavigationService?.PopOrNavigateToPage<TaskViewModel>(typeof(TaskViewModel));
            }
        }

        /// <summary>
        /// Navigates to audits asynchronous.
        /// </summary>
        private async Task NavigateToAuditsAsync()
        {
            // Prevent double clicking
            if (Settings.MenuLocation == MenuLocation.Audits && IsRootPageTopMost())
                return;

            IsReporting = false;
            Settings.MenuLocation = MenuLocation.Audits;
            MenuLocation = MenuLocation.Audits;

            await NavigationService?.PopOrNavigateToPage<AuditViewModel>(typeof(AuditViewModel));
        }

        /// <summary>
        /// Navigates to report asynchronous.
        /// </summary>
        private async Task NavigateToReportAsync()
        {
            // Prevent double clicking
            if (MenuLocation == MenuLocation.Report && IsRootPageTopMost() && Settings.SubpageReporting == MenuLocation.None)
                return;

            if (MenuLocation == MenuLocation.Report)
            {
                if (Settings.SubpageReporting != MenuLocation.None && IsRootPageTopMost())
                    await NavigationService.NavigateAsync<ReportViewModel>(noHistory: true, animated: false);
                else if (Settings.SubpageReporting != MenuLocation.None && !IsRootPageTopMost())
                    await NavigationService?.CloseAsync();
                else
                {
                    MenuLocation = MenuLocation.Report;
                    Settings.MenuLocation = MenuLocation.Report;

                    await NavigationService?.NavigateAsync<ReportViewModel>(noHistory: true, animated: false);
                }
            }
            else
            {
                if (Settings.SubpageReporting != MenuLocation.Report && Settings.MenuLocation != MenuLocation.None)
                {
                    MenuLocation = MenuLocation.Report;
                    Settings.MenuLocation = MenuLocation;
                    switch (Settings.SubpageReporting)
                    {
                        case MenuLocation.ReportActions:
                            await NavigationService?.NavigateAsync<ActionReportViewModel>(noHistory: true, animated: false);
                            break;
                        case MenuLocation.ReportAudits:
                            await NavigationService?.NavigateAsync<AuditReportViewModel>(noHistory: true, animated: false);
                            break;
                        case MenuLocation.ReportChecklists:
                            await NavigationService?.NavigateAsync<ChecklistReportViewModel>(noHistory: true, animated: false);
                            break;
                        case MenuLocation.ReportTasks:
                            await NavigationService?.NavigateAsync<TaskReportViewModel>(noHistory: true, animated: false);
                            break;
                        default:
                            MenuLocation = MenuLocation.Report;
                            Settings.MenuLocation = MenuLocation.Report;
                            Settings.SubpageReporting = MenuLocation.None;


                            await NavigationService?.NavigateAsync<ReportViewModel>(noHistory: true, animated: false);
                            break;
                    }
                    IsReporting = true;
                }
                else
                {
                    MenuLocation = MenuLocation.Report;
                    Settings.MenuLocation = MenuLocation.Report;

                    await NavigationService?.NavigateAsync<ReportViewModel>(noHistory: true, animated: false);
                }
            }
        }

        /// <summary>
        /// Navigates to actions asynchronous.
        /// </summary>
        private async Task NavigateToActionsAsync()
        {
            // Prevent double clicking
            if (MenuLocation == MenuLocation.Actions && IsRootPageTopMost())
                return;

            IsReporting = false;
            MenuLocation = MenuLocation.Actions;
            Settings.MenuLocation = MenuLocation.Actions;
            Settings.SubpageActions = MenuLocation.Actions;

            await NavigationService?.PopOrNavigateToPage<ActionViewModel>(typeof(ActionViewModel));
        }

        /// <summary>
        /// Navigates to work instructions asynchronous.
        /// </summary>
        private async Task NavigateToInstructionsAsync()
        {
            // Prevent double clicking
            if (MenuLocation == MenuLocation.Instructions && IsRootPageTopMost())
                return;

            IsReporting = false;
            MenuLocation = MenuLocation.Instructions;
            Settings.MenuLocation = MenuLocation.Instructions;
            Settings.SubpageActions = MenuLocation.Instructions;

            await NavigationService.PopOrNavigateToPage<InstructionsViewModel>(typeof(InstructionsViewModel));
        }

        /// <summary>
        /// Navigates to assessments asynchronous.
        /// </summary>
        private async Task NavigateToAssessmentsAsync()
        {
            // Prevent double clicking
            if (MenuLocation == MenuLocation.Assessments && IsRootPageTopMost())
                return;

            IsReporting = false;
            MenuLocation = MenuLocation.Assessments;
            Settings.MenuLocation = MenuLocation.Assessments;
            Settings.SubpageActions = MenuLocation.Assessments;

            await NavigationService.PopOrNavigateToPage<AssessmentsTemplatesViewModel>(typeof(AssessmentsTemplatesViewModel));
        }

        /// <summary>
        /// Navigates to assessments asynchronous.
        /// </summary>
        private async Task NavigateToFeedAsync()
        {
            // Prevent double clicking
            if (MenuLocation == MenuLocation.Feed && IsRootPageTopMost())
                return;

            IsReporting = false;
            MenuLocation = MenuLocation.Feed;
            Settings.MenuLocation = MenuLocation.Feed;
            Settings.SubpageActions = MenuLocation.Feed;

            await NavigationService.PopOrNavigateToPage<FeedViewModel>(typeof(FeedViewModel));
        }

        /// <summary>
        /// Triggers popup menu for profile, logout, area selection
        /// </summary>
        /// <returns></returns>
        private async Task PopupAsync()
        {
            using var scope = App.Container.CreateScope();
            var menuViewModel = scope.ServiceProvider.GetService<MenuViewModel>();
            Page page = menuViewModel.NavigationService.GetCurrentPage();

            string popupTitle = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenLogoutMessageTitle);
            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            string logout = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenLogoutMessageText);
            string profile = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenEditProfileMessageText);
            string area = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenChangeAreaText);

            string qrScanner = TranslateExtension.GetValueFromDictionary(LanguageConstants.mainScreenQRScannerText);
            string action = await page.DisplayActionSheet(popupTitle, null, cancel, area, logout, profile, CompanyFeatures.QRCode ? qrScanner : null);

            DependencyService.Get<IStatusBarService>().HideStatusBar();

            if (action == profile)
            {
                if (!await InternetHelper.HasInternetConnection())
                {
                    string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.onlyOnlineAction);
                    await page.DisplayActionSheet(result, null, cancel);
                }
                else
                {
                    await menuViewModel.NavigationService?.NavigateAsync<ProfileViewModel>();
                }
            }
            else if (action == area)
            {
                if (!await InternetHelper.HasInternetConnection())
                {
                    string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.changeAreaNoInternet);
                    await page.DisplayActionSheet(result, null, cancel);
                }
                else
                {
                    Settings.WorkAreaId = 0;
                    Settings.AssessmentsWorkAreaId = 0;
                    Settings.ReportWorkAreaId = 0;
                    Settings.MenuLocation = MenuLocation.None;
                    Settings.SubpageTasks = MenuLocation.None;
                    Settings.SubpageActions = MenuLocation.None;
                    Settings.SubpageReporting = MenuLocation.None;
                    await menuViewModel.NavigationService?.NavigateAsync<WorkAreaViewModel>(noHistory: true);
                }
            }
            else if (action == qrScanner)
            {
                await NavigateToQRScanner();
            }
            else if (action == logout)
            {
                await UserStatusManager.SignOffAsync(menuViewModel.NavigationService);
            }
        }

        private async Task NavigateToQRScanner()
        {
            if (!await InternetHelper.HasInternetConnection())
            {
                Page page = NavigationService?.GetCurrentPage();

                string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.qrScannerNoInternet);
                await page.DisplayActionSheet(result, null, cancel);
            }
            else
            {
                bool isAllowed = await CheckCameraPermissions();

                if (!isAllowed)
                {
                    return;
                }

                await NavigationService?.NavigateAsync<BookmarkViewModel>();
            }
        }


        private async Task<bool> CheckCameraPermissions()
        {
            var isCameraPermissionGranted = await PermissionsHelper.CheckAndRequestPermissionAsync<Permissions.Camera>();

            if (!isCameraPermissionGranted)
            {
                string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.contextMenuChooseMediaDialogCancel);
                string cameraMessage = TranslateExtension.GetValueFromDictionary(LanguageConstants.securityPermissionCamera);

                Page page = NavigationService.GetCurrentPage();
                await page.DisplayAlert("", cameraMessage, cancel);
            }

            return isCameraPermissionGranted;
        }

        public async override Task<bool> BeforeNavigatingAway()
        {
            var currentPage = NavigationService?.GetCurrentPage();
            var continueNavigating = true;
            if (currentPage?.BindingContext is BaseViewModel baseViewModel)
                continueNavigating = await baseViewModel.BeforeNavigatingAway();

            return continueNavigating;
        }


        private async Task NavigateAsync(Func<Task> action)
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Phone && TabIndex != 0 && DeviceSettings.PhoneViewsEnabled)
            {
                if (Settings.MenuLocation == MenuLocation.Menu)
                    return;

                Settings.MenuLocation = MenuLocation.Menu;
                using var scope = App.Container.CreateScope();
                var menuViewModel = scope.ServiceProvider.GetService<MenuViewModel>();
                menuViewModel.TabIndex = TabIndex;
                await menuViewModel.NavigationService?.NavigateAsync(viewModel: menuViewModel, noHistory: true);
            }
            else
            {
                var continueNavigating = await BeforeNavigatingAway();
                if (!continueNavigating)
                    return;

                await action();
                MenuManager.SetSelectedMenuItem();
            }
        }

        /// <summary>
        /// Checks if the current root page is at the top of the stack.
        /// </summary>
        /// <returns><see langword="true"/> if navigation stack count it less or equal than 1.</returns>
        private bool IsRootPageTopMost()
        {
            Debug.WriteLine(NavigationService.GetNavigationStackCount().ToString());
            return NavigationService.GetNavigationStackCount() <= 1;
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<LoginViewModel>(this, Constants.UserHasChanged);
                MessagingCenter.Unsubscribe<ProfileViewModel>(this, Constants.ReloadUserDataMessage);
                MessagingCenter.Unsubscribe<SyncService, int>(this, Constants.MyActionsChanged);
            });
            scope.Dispose();
            base.Dispose(disposing);
        }
    }
}
