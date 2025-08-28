using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.MenuFeatures;
using EZGO.Maui.Core.Classes.ValidationRules;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Login;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Authentication;
using EZGO.Maui.Core.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Translations

        private readonly string LoginText = Statics.LanguageDictionary.GetValue("LOGIN_SCREEN_LOGIN_BUTTON_TITLE", "Login");
        private readonly string NextText = Statics.LanguageDictionary.GetValue("BASE_TEXT_NEXT", "Next");
        private readonly string OkResult = Statics.LanguageDictionary.GetValue("BASE_TEXT_OK");
        private readonly string FailedResult = Statics.LanguageDictionary.GetValue("LOGIN_SCREEN_AUTHORIZATION_FAILED");
        private readonly string OfflineResult = Statics.LanguageDictionary.GetValue("ONLY_ONLINE_ACTION");
        private readonly string ErrorResult = Statics.LanguageDictionary.GetValue("SAML_LOGIN_INCORECT_CREDENTIALS_ERROR");

        #endregion

        #region Public Properties

        /// <summary>
        /// Usernames for autocomplete.
        /// </summary>
        public ObservableCollection<string> Usernames { get; set; }

        /// <summary>
        /// Gets or sets user's username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Indicates if the password input should be visible.
        /// </summary>
        public bool IsPasswordVisible { get; set; } = true;

        /// <summary>
        /// Gets the display text for the button depending on the current model state.
        /// </summary>
        /// <value>Translated text as a string.</value>
        public string SubmitButtonText => IsPasswordVisible ? LoginText : NextText;

        /// <summary>
        /// Gets or sets users's password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates if the view model is busy.
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Gets formatted string containing current application version and build number.
        /// </summary>
        public string Version => VersionTracking.CurrentVersion + Constants.EnvironmentIdentifier + VersionTracking.CurrentBuild;

        #endregion

        #region Public Commands

        /// <summary>
        /// Gets the command that proceedes to the next authentication step.
        /// </summary>
        public ICommand SubmitCommand => new Command(() =>
        {
            ExecuteLoadingAction(NextAsync);
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the command that opens the browser and naviagtes to the url that is supplied as the command's parameter.
        /// </summary>
        public ICommand OpenLinkCommand => new Command(url =>
        {
            ExecuteLoadingAction(async () =>
            {
                await Launcher.OpenAsync(new Uri(url as string));
            });
        }, CanExecuteCommands);

        public ICommand UsernameInputChangedCommand => new Command((args) => UsernameInputChanged(args));

        #endregion

        #region Services 

        private readonly ILoginService _loginService;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public LoginViewModel(ILoginService loginService, INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
            _loginService = loginService;
        }

        #endregion

        #region Initilization

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<Services.Api.ApiClient>(this, Constants.SignedOff);
            });
            Settings.LastCheckedShiftId = -1;

            List<string> usernames = await Task.Run(async () => await _loginService.GetLocalUsernamesAsync());
            Usernames = new ObservableCollection<string>(usernames ?? new List<string>());

#if DEBUG
            // Default password for development
            Password = "Ezf2045";
#endif
            if (DeviceInfo.Platform.Equals(DevicePlatform.Android))
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await PermissionsHelper.CheckAndRequestPermissionAsync<Permissions.StorageWrite>();
                });
            }

            await Task.Run(async () => await base.Init());
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Proceeds to the next authentication step.
        /// </summary>
        /// <returns>A task that can be awaited.</returns>
        private async Task NextAsync()
        {
            IsBusy = true;

            // Get the current page to display the action sheet on later
            Page page = NavigationService.GetCurrentPage();

            try
            {
                // Check for API connection
                if (await InternetHelper.HasInternetAndApiConnectionAsync(ignoreToken: true))
                {
                    // Check if we have a user name
                    if (!string.IsNullOrWhiteSpace(Username))
                    {
                        // Check if external authentication method should be used for this user
                        var authenticationMethod = await _loginService.IsMsalAsync(Username);

                        switch (authenticationMethod)
                        {
                            case AuthenticationMethod.MSAL:
                                await HandleMsalAuthentication(page);
                                break;

                            case AuthenticationMethod.Credentials:
                                await HandleCredentialsAuthentication(page);
                                break;
                        }
                    }
                }
                // No internet connection
                else
                {
                    // Show no internet error massage
                    await page.DisplayActionSheet(OfflineResult, null, OkResult);
                }
            }
            catch (Exception exception)
            {
                // Show error message
                await page.DisplayActionSheet(FailedResult, null, OkResult);
                //Crashes.TrackError(exception);
            }

            _statusBarService?.HideStatusBar();

            IsBusy = false;
        }

        private async Task HandleCredentialsAuthentication(Page page)
        {
            // Password entry already visible 
            if (IsPasswordVisible)
            {
                // If password is empty
                if (string.IsNullOrWhiteSpace(Password))
                {
                    // Show error
                    await page.DisplayActionSheet(ErrorResult, null, OkResult);
                }
                // We have a password
                else
                {
                    // Login using credentials
                    var result = await _loginService.SignInWithCredentialsAsync(Username, Password);
                    await HandleSignInResultAsync(result, page);
                }
            }
            // Password entry not visible yet
            else
            {
                // Mark it as visible
                IsPasswordVisible = true;
            }
        }

        private async Task HandleMsalAuthentication(Page page)
        {
            // Clear current flags and password since we won't be using them
            IsPasswordVisible = false;
            Password = null;

            var result = await _loginService.SignInMsalAsync(Username);

            await HandleSignInResultAsync(result, page);
        }

        private async Task HandleSignInResultAsync(SignInResult result, Page page)
        {
            switch (result)
            {
                case SignInResult.Ok:
                    SetMenuItems();
                    Settings.MenuLocation = MenuLocation.None;
                    await NavigationService.NavigateAsync<WorkAreaViewModel>(noHistory: true);
                    break;

                case SignInResult.Failed:
                case SignInResult.LinkedAccountNotFound:
                case SignInResult.IncorrectCredentials:
                    await page.DisplayActionSheet(ErrorResult, null, OkResult);
                    break;
            }
        }

        private void UsernameInputChanged(object args)
        {
            var login = args as Syncfusion.Maui.Inputs.SelectionChangedEventArgs;
            var isEmail = new EmailValidationRule<string>("").Check(login.AddedItems.ToString());
            IsPasswordVisible = !isEmail;
        }

        private void SetMenuItems()
        {
            using var scope = App.Container.CreateScope();
            var menuManager = scope.ServiceProvider.GetService<IMenuManager>();
            menuManager.RegisterMenuItems();
        }

        #endregion
    }
}