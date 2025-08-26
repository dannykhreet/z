using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DeviceFormats;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Services.Api;
using EZGO.Maui.Core.Utils;
using System.Diagnostics;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    /// <summary>
    /// Base view model.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.Classes.NotifyPropertyChanged" />
    public abstract class BaseViewModel : NotifyPropertyChanged, IDisposable
    {
        private bool disposedValue;

        protected IStatusBarService _statusBarService;
        protected IUserService _userService;
        protected IMessageService _messageService;
        protected IActionsService _actionService;

        /// <summary>
        /// Launches refreshing depending of Internet Connection
        /// </summary>
        public ICommand RefreshCommand { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is navigating.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is navigating; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating a pull to refresh action is busy
        /// </summary>
        public bool IsRefreshing { get; set; }

        /// <summary>
        /// General Dropdown visibility indicator
        /// </summary>
        public bool IsDropdownOpen { get; set; }

        /// <summary>
        /// Indicator of empty content, used to show empty page symbol
        /// </summary>
        public bool HasItems { get; set; } = true;

        /// <summary>
        /// Indicated is drwer is currently open
        /// </summary>
        public bool IsDrawerOpen { get; set; } = false;

        /// <summary>
        /// Gets the navigation service.
        /// </summary>
        /// <value>
        /// The navigation service.
        /// </value>
        public INavigationService NavigationService { get; protected set; }

        /// <summary>
        /// CancelCommand
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Sets IsDropdownOpen to false
        /// </summary>
        public ICommand CloseDropdownCommand { get; protected set; }

        /// <summary>
        /// Toggle IsDropdownOpen
        /// </summary>
        public ICommand ToggleDropdownCommand { get; private set; }
        public ICommand ToggleDrawerCommand { get; private set; }

        public bool IsMenuVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; } = string.Empty;

        public string Picture { get; set; } = UserSettings.userSettingsPrefs.UserPictureUrl != Constants.NoProfilePicture2 ? UserSettings.userSettingsPrefs.UserPictureUrl : null;

        public string Logo { get; set; } = UserSettings.userSettingsPrefs.CompanyLogoUrl;

        public string Fullname { get; set; } = UserSettings.userSettingsPrefs.Fullname;

        public BaseFormat DeviceFormat { get => DeviceSettings.DeviceFormat; }

        protected static bool CanNavigate { get; set; } = true;

        private readonly SemaphoreSlim messagingCenterSemaphore = new SemaphoreSlim(1, 1);


        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewModel"/> class.
        /// </summary>
        protected BaseViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService)
        {
            // Create commands
            CancelCommand = new Command(async () => await ExecuteLoadingActionAsync(CancelAsync), CanExecuteCommands);
            CloseDropdownCommand = new Command(() => ExecuteLoadingAction(() => { IsDropdownOpen = false; }), CanExecuteCommands);
            ToggleDropdownCommand = new Command(() => ExecuteLoadingAction(() => { IsDropdownOpen = !IsDropdownOpen; }), CanToggleDropdown);
            ToggleDrawerCommand = new Command(() => ExecuteLoadingAction(() => { IsDrawerOpen = !IsDrawerOpen; }), CanToggleDropdown);
            RefreshCommand = new Command(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        IsRefreshing = true;
                        IsLoading = true;
                        await MainThread.InvokeOnMainThreadAsync(RefreshCanExecute);
                        if (await InternetHelper.HasInternetAndApiConnectionAsync())
                            await RefreshAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debugger.Break();
                        Debug.WriteLine(ex.StackTrace);
                    }
                    finally
                    {
                        IsRefreshing = false;
                        IsLoading = false;
                        await MainThread.InvokeOnMainThreadAsync(RefreshCanExecute);
                    }
                });
            }, CanExecuteCommands);

            NavigationService = navigationService;
            _userService = userService;
            _messageService = messageService;
            _actionService = actionsService;

            using var scope = App.Container.CreateScope();
            _statusBarService = scope.ServiceProvider.GetService<IStatusBarService>();
        }

        protected virtual void RefreshCanExecute()
        {
            (CancelCommand as Command)?.ChangeCanExecute();
            (CloseDropdownCommand as Command)?.ChangeCanExecute();
            (ToggleDropdownCommand as Command)?.ChangeCanExecute();
            (RefreshCommand as Command)?.ChangeCanExecute();
        }

        /// <summary>
        /// Wraps the given action in the IsLoading property.
        /// </summary>
        /// <param name="execute">The function to execute.</param>
        public async Task ExecuteLoadingAction(Func<Task> execute)
        {
            if (!IsLoading && execute != null)
            {
                try
                {
                    IsLoading = true;
                    CanNavigate = false;
                    RefreshCanExecute();
                    await execute();
                    await Task.Delay(10);
                }
                finally
                {
                    CanNavigate = true;
                    IsLoading = false;
                    RefreshCanExecute();
                }
            }
        }

        public async Task ExecuteLoadingActionAsync(Func<Task> action)
        {
            if (!IsLoading && action != null)
            {
                try
                {
                    IsLoading = true;
                    await action();
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        public virtual Task ApplyFilter(TaskStatusEnum? status = null, bool reset = true)
        {
            return Task.CompletedTask;
        }

        public void ExecuteLoadingAction(Action execute)
        {
            if (!IsLoading && execute != null)
            {
                try
                {
                    IsLoading = true;
                    RefreshCanExecute();
                    execute();
                }
                finally
                {
                    IsLoading = false;
                    RefreshCanExecute();
                }
            }
        }

        protected virtual Task RefreshAsync() { return Task.CompletedTask; }

        /// <summary>
        /// Cancels.
        /// </summary>
        public virtual async Task CancelAsync()
        {
            _statusBarService?.HideStatusBar();
            if (NavigationService != null)
                await NavigationService.CloseAsync();
        }

        /// <summary>
        /// Determines whether this instance [can execute commands].
        /// </summary>
        /// <param name="commandParameter">The command parameter.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can execute commands]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanExecuteCommands(object commandParameter)
        {
            return !IsLoading || !IsRefreshing;
        }

        /// <summary>
        /// Determines whether this instance [can execute commands].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can execute commands]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanExecuteCommands()
        {
            return !IsLoading || !IsRefreshing;
        }

        public virtual bool CanToggleDropdown()
        {
            return CanExecuteCommands();
        }

        public void AppearingHandler(object sender, EventArgs e)
        {
            _ = OnAppearing(sender, e); // fire-and-forget async safely
        }

        public void DisappearingHandler(object sender, EventArgs e)
        {
            OnDisappearing(sender, e); // this is already void
        }

        /// <summary>
        /// Appearing method
        /// <para>This method will be called when the page appears</para>
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        public virtual async Task OnAppearing(object sender, EventArgs e)
        {
            SubscribeToDoAction<ApiClient>(Constants.SignedOff);
            SubscribeToDoAction<ApiClient>(Constants.TokenExpired);
            SubscribeToDoAction<ApiClient>(Constants.LogOff);

            // TODO initialize async
            if (!this.IsInitialized)
                await this.Init();
        }

        /// <summary>
        /// Disappearing method
        /// <para>This method will be called when the page disappears</para>
        /// <para>Sets IsInitialized to false</para>
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        public virtual void OnDisappearing(object sender, EventArgs e)
        {
            try
            {
                messagingCenterSemaphore.Wait();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Unsubscribe<ApiClient>(this, Constants.SignedOff);
                    MessagingCenter.Unsubscribe<ApiClient>(this, Constants.TokenExpired);
                    MessagingCenter.Unsubscribe<ApiClient>(this, Constants.LogOff);
                });
            }
            finally
            {
                messagingCenterSemaphore.Release();
            }
        }

        public virtual async Task<bool> BeforeNavigatingAway() { return true; }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public virtual async Task Init()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            await Task.CompletedTask;
        }

        protected void SetUserData()
        {
            string pictureUrl = Constants.NoProfilePicture;

            if (!UserSettings.UserPictureUrl.IsNullOrWhiteSpace())
                pictureUrl = UserSettings.UserPictureUrl;

            Picture = pictureUrl;
            Logo = UserSettings.CompanyLogoUrl;
            Fullname = UserSettings.Fullname;
        }

        private void SubscribeToDoAction<T>(string messageType) where T : class
        {
            try
            {
                messagingCenterSemaphore.Wait();
                MessagingCenter.Subscribe<T>(this, messageType, async (sender) =>
                {
                    try
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Unsubscribe<T>(this, Constants.TokenExpired);
                            MessagingCenter.Unsubscribe<T>(this, Constants.SignedOff);
                            MessagingCenter.Unsubscribe<T>(this, Constants.LogOff);
                        });
                        Page page = NavigationService.GetCurrentPage();
                        // What is this??????
                        if (!((page ?? new Page()).ToString()).Contains("Login"))
                        {
                            Settings.WorkAreaId = 0;
                            Settings.ReportWorkAreaId = 0;
                            Settings.AllTaskWorkAreaId = 0;
                            Settings.AssessmentsWorkAreaId = 0;
                            Settings.LastCheckedShiftId = -1;

                            if (messageType != Constants.LogOff)
                            {
                                string reason = string.Empty;
                                switch (messageType)
                                {
                                    case Constants.TokenExpired:
                                        reason = TranslateExtension.GetValueFromDictionary("LOG_OF_REASON_TOKEN_EXPIRED_MESSAGE");
                                        break;
                                    default:
                                        reason = TranslateExtension.GetValueFromDictionary("LOG_OF_REASON_USED_ON_ANOTHER_DEVICE_MESSAGE");
                                        break;
                                }

                                string popupTitle = TranslateExtension.GetValueFromDictionary("MAIN_SCREEN_LOGOUT_MESSAGE_TEXT");
                                string ok = TranslateExtension.GetValueFromDictionary("BASE_TEXT_OK");

                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await page.DisplayAlert(popupTitle, reason, ok);
                                    await UserStatusManager.SignOffAsync(NavigationService);
                                });
                            }
                            else
                            {
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                {
                                    await UserStatusManager.SignOffAsync(NavigationService);
                                });
                            }
                        }
                    }
                    catch
                    {

                    }

                });

            }
            finally
            {
                messagingCenterSemaphore.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _actionService?.Dispose();
                    _userService?.Dispose();
                }

                _statusBarService = null;
                _userService = null;
                _messageService = null;
                _actionService = null;
                disposedValue = true;
                CancelCommand = null;
                CloseDropdownCommand = null;
                ToggleDropdownCommand = null;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            PrintGCInfo();
        }

        [Conditional("DEBUG")]
        private void PrintGCInfo()
        {
            Debug.WriteLine($"Max Generation: {GC.MaxGeneration}");
            PrintGCCollectionsCount();
            Debug.WriteLine($"Memory allocated for current thread: {GC.GetAllocatedBytesForCurrentThread()} bytes");
            Debug.WriteLine("Object disposed");
        }

        [Conditional("DEBUG")]
        private void PrintGCCollectionsCount()
        {
            Debug.WriteLine($"CollectionCount gen 0: {GC.CollectionCount(0)}");
            Debug.WriteLine($"CollectionCount gen 1: {GC.CollectionCount(1)}");
        }
    }
}