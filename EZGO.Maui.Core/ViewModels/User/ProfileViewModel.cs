using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.LanguageResources;
using EZGO.Maui.Core.Classes.MenuFeatures;
using EZGO.Maui.Core.Classes.ValidationRules;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    /// <summary>
    /// Profile view model.
    /// </summary>
    /// <seealso cref="EZGO.Maui.Core.ViewModels.BaseViewModel" />
    public class ProfileViewModel : BaseViewModel
    {
        private AvailableLanguages availableLanguages;

        #region Public Properties

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public ValidatableObject<string> Email { get; set; } = new ValidatableObject<string>(UserSettings.Email);

        /// <summary>
        /// Gets or sets the picture.
        /// </summary>
        /// <value>
        /// The picture.
        /// </value>
        public MediaItem ProfilePicture { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public ValidatableObject<string> FirstName { get; set; } = new ValidatableObject<string>(UserSettings.Firstname);

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public ValidatableObject<string> LastName { get; set; } = new ValidatableObject<string>(UserSettings.Lastname);

        /// <summary>
        /// Gets the default profile picture
        /// </summary>
        private static MediaItem DefaultProfilePicure => MediaItem.OnlinePicture(Constants.NoProfilePicture);

        /// <summary>
        /// Gets or sets the preferred language.
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// Gets available languages
        /// </summary>
        public List<string> AvailableLanguages { get; private set; }

        #endregion

        #region Commands

        /// <summary>
        /// Shows popup for profile, logout navigation
        /// </summary>
        public ICommand UpdateCommand => new Command(() =>
        {
            ExecuteLoadingAction(UpdateAsync);
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the password command.
        /// </summary>
        /// <value>
        /// The password command.
        /// </value>
        public ICommand PasswordCommand => new Command(() =>
        {
            ExecuteLoadingAction(GoPasswordAsync);
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the change picture command.
        /// </summary>
        /// <value>
        /// The change picture command.
        /// </value>
        public ICommand ChangePictureCommand => new Command(() =>
        {
            ExecuteLoadingAction(ChangePictureAsync);
        }, CanExecuteCommands);

        #endregion
        //TODO Add Snack Bar information about status change
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileViewModel"/> class.
        /// </summary>
        public ProfileViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            SetInitialValues();
            InitializeValidationRules();
            await SetAvailableLanguages();
            await base.Init();
        }

        private async Task SetAvailableLanguages()
        {
            using var scope = App.Container.CreateScope();
            var settingsService = scope.ServiceProvider.GetService<ISettingsService>();

            availableLanguages = await settingsService.GetAvailableLanguagesAsync();
            AvailableLanguages = availableLanguages.SupportedLanguages.Values.ToList();

            var currentLanguageTag = Settings.CurrentLanguageTag.ToLower();
            PreferredLanguage = availableLanguages.SupportedLanguages.GetValue(currentLanguageTag);
        }

        private void SetInitialValues()
        {
            string title = TranslateExtension.GetValueFromDictionary(LanguageConstants.userProfileScreenTitle);
            Title = string.Format(title.ReplaceLanguageVariablesCumulative(), UserSettings.Fullname);
            ProfilePicture = GetCurrentProfilePicture();
            FirstName.Value = UserSettings.Firstname;
            LastName.Value = UserSettings.Lastname;
            Email.Value = UserSettings.Email;
        }

        private void InitializeValidationRules()
        {
            FirstName.Validations.Add(new IsNullOrEmptyValidationRule<string>(TranslateExtension.GetValueFromDictionary("FIRSTNAME_NOT_EMPTY")));
            FirstName.Validations.Add(new MaxLengthValidationRule<string>(250, TranslateExtension.GetValueFromDictionary("FIRSTNAME_TOO_LONG")));
            LastName.Validations.Add(new IsNullOrEmptyValidationRule<string>(TranslateExtension.GetValueFromDictionary("LASTNAME_NOT_EMPTY")));
            LastName.Validations.Add(new MaxLengthValidationRule<string>(250, TranslateExtension.GetValueFromDictionary("LASTNAME_TOO_LONG")));
            Email.Validations.Add(new EmailValidationRule<string>(TranslateExtension.GetValueFromDictionary("EMAIL_FORMAT")));
        }

        /// <summary>
        /// Updates the profile asynchronous.
        /// </summary>
        private async Task UpdateAsync()
        {
            if (FirstName.Validatate() && LastName.Validatate() && Email.Validatate())
            {

                if (await InternetHelper.HasInternetConnection())
                {
                    UserProfile profile = new UserProfile
                    {
                        FirstName = FirstName.Value,
                        LastName = LastName.Value,
                        Email = Email.Value,
                        Id = UserSettings.Id
                    };

                    if (!ProfilePicture.IsEmpty)
                    {
                        using var scope = App.Container.CreateScope();
                        var mediaService = scope.ServiceProvider.GetService<IMediaService>();
                        // Upload the image
                        await mediaService.UploadMediaItemAsync(ProfilePicture, MediaStorageTypeEnum.ProfileImage, UserSettings.Id);
                        // Set the URL
                        profile.Picture = ProfilePicture.PictureUrl;
                    }

                    bool result = await _userService?.UpdateProfileAsync(profile);

                    if (result)
                    {
                        UserSettings.UserPictureUrl = ProfilePicture.PictureUrl;
                        UserSettings.Firstname = profile.FirstName;
                        UserSettings.Lastname = profile.LastName;
                        UserSettings.Fullname = $"{profile.FirstName} {profile.LastName}";
                        UserSettings.Email = Email.Value;

                        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.ReloadUserDataMessage); });

                        //TODO Uncomment when there will be language selection on backend.

                        //if (await SetPreferredLanguage())
                        //{
                        //    var viewModel = StartupHelper.GetStartupViewModel();
                        //    await NavigationService.NavigateAsync(noHistory: true, viewModel: viewModel);
                        //}
                        //else
                        //{
                        await CancelAsync();
                        //}
                    }
                }
            }
        }

        private async Task<bool> SetPreferredLanguage()
        {
            var preferredLanguageTag = availableLanguages.SupportedLanguages.FirstOrDefault(x => x.Value == PreferredLanguage).Key;

            if (preferredLanguageTag != Settings.CurrentLanguageTag)
            {
                UserSettings.PreferredLanguage = preferredLanguageTag;
                Settings.CurrentLanguageTag = preferredLanguageTag;
                Language.SetCultureInfo();
                var language = new Language();
                await language.GetResourcesAsync(false);

                using var scope = App.Container.CreateScope();
                var menuManager = scope.ServiceProvider.GetService<IMenuManager>();
                menuManager.ReloadMenuItemTranslations();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Navigates to the change password view asynchronous.
        /// </summary>
        private async Task GoPasswordAsync()
        {
            if (await InternetHelper.HasInternetConnection())
                await NavigationService.NavigateAsync<ChangePassViewModel>();
        }

        /// <summary>
        /// Changes the picture asynchronous from camera or gallery.
        /// </summary>
        private async Task ChangePictureAsync()
        {
            using var scope = App.Container.CreateScope();
            var mediaHelper = scope.ServiceProvider.GetService<IMediaHelper>();
            List<MediaOption> mediaOptions = new List<MediaOption> { MediaOption.TakePhoto, MediaOption.PhotoGallery };

            if (ProfilePicture.PictureUrl != Constants.NoProfilePicture)
                mediaOptions.Add(MediaOption.RemoveMedia);

            var dialogResult = await mediaHelper.PickMediaAsync(mediaOptions);

            if (dialogResult.IsCanceled)
                return;

            ProfilePicture = dialogResult.Result ?? DefaultProfilePicure;
        }

        /// <summary>
        /// Gets the current profile picture from settings
        /// </summary>
        /// <returns>Media item representing the picture</returns>
        private MediaItem GetCurrentProfilePicture()
        {
            if (string.IsNullOrEmpty(UserSettings.UserPictureUrl) || UserSettings.UserPictureUrl == Constants.NoProfilePicture2)
                return DefaultProfilePicure;

            return MediaItem.OnlinePicture(UserSettings.UserPictureUrl);
        }

        protected override void Dispose(bool disposing)
        {
            FirstName.Dispose();
            LastName.Dispose();
            Email.Dispose();

            base.Dispose(disposing);
        }
    }
}
