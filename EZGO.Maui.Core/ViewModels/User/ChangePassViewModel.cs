using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.ValidationRules;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Core.ViewModels
{
    public class ChangePassViewModel : BaseViewModel
    {
        public ValidatableObject<string> CurrentPassword { get; set; } = new ValidatableObject<string>();

        public ValidatablePair<string> NewPassword { get; set; } = new ValidatablePair<string>(string.Empty, string.Empty);

        public ICommand ChangePassCommand => new Command(() =>
        {
            ExecuteLoadingAction(ChangePassAsync);
        }, CanExecuteCommands);

        public ChangePassViewModel(INavigationService navigationService, IUserService userService, IMessageService messageService, IActionsService actionsService) : base(navigationService, userService, messageService, actionsService)
        {
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            AddValidationRules();

            await base.Init();
        }

        private void AddValidationRules()
        {
            string passwordEmptyMessage = TranslateExtension.GetValueFromDictionary(LanguageConstants.editProfileEmptyCurrentPassword);
            string newPasswordInvalid = TranslateExtension.GetValueFromDictionary(LanguageConstants.editProfileInvalidPassword);
            string newPasswordsDoNotMatch = TranslateExtension.GetValueFromDictionary(LanguageConstants.editProfileNotMatchPassword);

            CurrentPassword.Validations.Add(new IsNullOrEmptyValidationRule<string>(passwordEmptyMessage));
            NewPassword.Item1.Validations.Add(new PasswordValidationRule<string>(newPasswordInvalid));
            NewPassword.Item2.Validations.Add(new PasswordValidationRule<string>(newPasswordInvalid));
            NewPassword.Validations.Add(new MatchPairValidationRule<string>(newPasswordsDoNotMatch));
        }

        private async Task ChangePassAsync()
        {
            if (!CurrentPassword.Validatate() || !NewPassword.Validate()) return;

            if (await InternetHelper.HasInternetConnection())
            {
                // Update the password
                bool success = false;

                HttpResponseMessage response = await _userService.UpdatePasswordAsync(CurrentPassword.Value, NewPassword.Item1.Value, NewPassword.Item2.Value);

                try
                {
                    var result = await response.Content.ReadAsStringAsync();
                    //TODO Change to result.IsSuccessfullStatusCode when api will be updated
                    success = bool.Parse(result);
                }
                catch { }

                if (success)
                    await CancelAsync();
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    if (bool.TryParse(error.RemoveSpecialCharacters(), out bool result))
                    {
                        error = "You can't change password to the current one";
                    }
                    else
                    {
                        error = await response.Content.ReadAsJsonAsync<string>();
                    }

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Page page = NavigationService.GetCurrentPage();
                        string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);
                        _messageService.SendMessage("Clear Bad Request Message", Colors.Green, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.Clear);
                        await page.DisplayActionSheet(error, null, ok);
                    }

                    _statusBarService.HideStatusBar();
                }
            }
        }
    }
}
