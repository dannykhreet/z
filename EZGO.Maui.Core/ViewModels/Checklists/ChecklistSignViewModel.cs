using Autofac;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.ShiftChecks;
using EZGO.Maui.Core.Classes.Signatures;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Checklists;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Checklists;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace EZGO.Maui.Core.ViewModels
{
    public class ChecklistSignViewModel : BaseViewModel
    {
        private readonly IChecklistService _checklistService;

        private ChecklistTemplateModel checklistTemplate;

        public ChecklistTemplateModel SelectedChecklist { get; set; }

        public Guid IncompleteChecklistLocalGuid { get; internal set; }

        public int ChecklistTemplateId { get; set; }

        public List<BasicTaskTemplateModel> TaskTemplates { get; set; }

        public bool IsBusy { get; set; } = false;

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public ObservableCollection<UserProfileModel> Users { get; set; }

        /// <summary>
        /// Gets or sets the popup users.
        /// </summary>
        /// <value>
        /// The popup users.
        /// </value>
        public ObservableCollection<UserProfileModel> PopupUsers { get; set; }

        /// <summary>
        /// Gets or sets the selected user.
        /// </summary>
        /// <value>
        /// The selected user.
        /// </value>
        public UserProfileModel SelectedUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user popup is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user popup is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserPopupOpen { get; set; }

        /// <summary>
        /// Gets or sets the automatic complete text.
        /// </summary>
        /// <value>
        /// The automatic complete text.
        /// </value>
        public string AutoCompleteText { get; set; }

        public string CoSignerName { get; set; }

        public int CoSignerId { get; set; }

        public bool ButtonEnabled { get; set; } = true;

        public int PagesFromDeepLink { get; set; }

        public SignatureHelperControl SignatureHelper { get; set; }

        public List<UserValuesPropertyModel> OpenFieldsValues { get; set; }

        public BasicTaskModel TaskFromDeepLink { get; set; }

        public DateTime StartedAt { get; set; }

        public bool DeepLinkCompletionIsRequired { get; set; } = false;

        public int IncompleteChecklistId { get; set; }

        public List<StageTemplateModel> Stages { get; internal set; }

        /// <summary>
        /// Gets the open user popup command.
        /// </summary>
        /// <value>
        /// The open user popup command.
        /// </value>
        public ICommand OpenUserPopupCommand => new Command(() =>
            ExecuteLoadingAction(OpenUserPopup), CanExecuteCommands);

        /// <summary>
        /// Gets the close user popup command.
        /// </summary>
        /// <value>
        /// The close user popup command.
        /// </value>
        public ICommand CloseUserPopupCommand => new Command(() =>
            ExecuteLoadingAction(CloseUserPopup), CanExecuteCommands);

        /// <summary>
        /// Gets the auto complete selected command.
        /// </summary>
        /// <value>
        /// The auto complete selected command.
        /// </value>
        public ICommand AutoCompleteSelectedCommand => new Command<SelectionChangedEventArgs>(AutoCompleteSelected);

        /// <summary>
        /// Gets the remove user command.
        /// </summary>
        /// <value>
        /// The remove user command.
        /// </value>
        public ICommand RemoveUserCommand => new Command<UserProfileModel>((userProfile) =>
        {
            ExecuteLoadingAction(() => RemoveUser(userProfile));
        }, CanExecuteCommands);

        /// <summary>
        /// Gets the submit user popup command.
        /// </summary>
        /// <value>
        /// The submit user popup command.
        /// </value>
        public ICommand SubmitUserPopupCommand => new Command(() =>
            ExecuteLoadingAction(SubmitUserPopup), CanExecuteCommands);

        public ChecklistSignViewModel(
            INavigationService navigationService,
            IUserService userService,
            IMessageService messageService,
            IActionsService actionsService,
            IChecklistService checklistService) : base(navigationService, userService, messageService, actionsService)
        {
            _checklistService = checklistService;
        }

        /// <summary>
        /// Initializes this instance.
        /// <para>Sets IsInitialized to true</para>
        /// </summary>
        public override async Task Init()
        {
            IsBusy = true;
            MessagingCenter.Subscribe<SaveSignatureEventSender>(this, Constants.SignTemplateMessage, async (sender) =>
            {
                if (!SignatureHelper.SaveSignatureStreams(sender.FirstSignature, sender.SecondSignature)) return;

                await Submit();
            });
            await Task.Run(async () =>
            {
                if (SelectedChecklist != null)
                    checklistTemplate = SelectedChecklist;
                else
                    checklistTemplate = await _checklistService.GetChecklistTemplateAsync(ChecklistTemplateId);

                if (checklistTemplate.IsDoubleSignatureRequired)
                {
                    List<UserProfileModel> users = await _userService.GetCompanyUsersAsync();
                    users.Remove(users.Where(x => x.Id == UserSettings.Id).FirstOrDefault());
                    Users = new ObservableCollection<UserProfileModel>(users);

                    string placeholder = TranslateExtension.GetValueFromDictionary(LanguageConstants.signChecklistScreenNamePlaceholderText);
                    CoSignerName = placeholder;
                }

                SignatureHelper = new SignatureHelperControl(_checklistService) { IsDoubleSignatureRequired = checklistTemplate.IsDoubleSignatureRequired };
            });
            IsBusy = false;
            await base.Init();
        }

        private async Task Submit()
        {
            SignatureHelper.CoSignerName = CoSignerName;
            SignatureHelper.CoSignerId = CoSignerId;

            ButtonEnabled = false;

            await SubmitChecklistAndContinueAsync();

            IsBusy = false;
        }

        private async Task SubmitChecklistAndContinueAsync()
        {
            Debug.WriteLine($"Submiting: {DateTimeHelper.Now.Millisecond}");

            var model = new PostTemplateModel(ChecklistTemplateId, checklistTemplate.Name, OpenFieldsValues, TaskTemplates, DeepLinkCompletionIsRequired, TaskFromDeepLink?.Id, true, IncompleteChecklistId, Stages, checklistTemplate.Version);
            model.StartedAt = StartedAt;
            model.LocalGuid = IncompleteChecklistLocalGuid;

            if (PagesFromDeepLink > 0)
            {
                var shiftChanged = await ShiftChanged.PerformChangeAsync();

                if (!shiftChanged)
                    OnlineShiftCheck.IsShiftChangeAllowed = true;

                await NavigationService.RemoveLastPagesAsync(PagesFromDeepLink);

                if (TaskFromDeepLink != null)
                {
                    using var scope = App.Container.CreateScope();
                    var messageService = scope.ServiceProvider.GetService<IMessageService>();
                    messageService.SendLinkedItemSignedMessage(TaskFromDeepLink);
                }
            }
            else
                await NavigationService.PopOrNavigateToPage<ChecklistTemplatesViewModel>(typeof(ChecklistTemplatesViewModel));

            await SignatureHelper.Submit(model);
        }

        /// <summary>
        /// Opens the user popup.
        /// </summary>
        private void OpenUserPopup()
        {
            var selectedusers = new List<UserProfileModel>();
            if (SelectedUser != null) { selectedusers.Add(SelectedUser); }
            PopupUsers = new ObservableCollection<UserProfileModel>(selectedusers);

            IsUserPopupOpen = true;
        }

        /// <summary>
        /// Closes the user popup.
        /// </summary>
        private void CloseUserPopup()
        {
            IsUserPopupOpen = false;
        }

        /// <summary>
        /// Removes the user.
        /// </summary>
        /// <param name="userProfile">The user profile.</param>
        private void RemoveUser(UserProfileModel userProfile)
        {
            if (userProfile != null)
            {
                PopupUsers = new ObservableCollection<UserProfileModel>();
                SelectedUser = null;
            }
        }

        /// <summary>
        /// Handles the event when a selection is made in the auto complete control.
        /// </summary>
        /// <param name="args">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void AutoCompleteSelected(SelectionChangedEventArgs args)
        {
            if (args == null)
                return;

            if (args.AddedItems.FirstOrDefault() is UserProfileModel userProfile)
            {
                var selectedusers = new List<UserProfileModel>();

                SelectedUser = userProfile;
                selectedusers.Add(SelectedUser);

                PopupUsers = new ObservableCollection<UserProfileModel>(selectedusers);

                AutoCompleteText = string.Empty;
            }
        }

        /// <summary>
        /// Submits the user popup.
        /// </summary>
        private void SubmitUserPopup()
        {
            if (SelectedUser != null) { CoSignerName = SelectedUser.FullName; OnPropertyChanged(nameof(CoSignerName)); CoSignerId = SelectedUser.Id; }
            else if (!string.IsNullOrWhiteSpace(AutoCompleteText)) { CoSignerName = AutoCompleteText; CoSignerId = 0; }
            else
            {
                string placeholder = TranslateExtension.GetValueFromDictionary(LanguageConstants.signChecklistScreenNamePlaceholderText);
                CoSignerName = placeholder;
                CoSignerId = 0;
            }

            IsUserPopupOpen = false;
        }

        protected override void Dispose(bool disposing)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Unsubscribe<SaveSignatureEventSender>(this, Constants.SignTemplateMessage);
            });
            //_checklistService.Dispose();
            // SignatureHelper = null;
            base.Dispose(disposing);
        }
    }
}
