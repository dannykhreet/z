using EZGO.Api.Models;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Signatures;
using EZGO.Maui.Core.Classes.Stages;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Interfaces.Message;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Utils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;

namespace EZGO.Maui.Core.ViewModels.Shared;

public class StageSignViewModel : BaseViewModel
{
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

    public bool AreSignaturesVisible { get; set; }

    public SignatureHelperControl SignatureHelper { get; set; }

    public DateTime StartedAt { get; set; }

    public StageTemplateModel Stage { get; internal set; }

    public StagesControl StagesControl { get; internal set; }

    public Signature Signature1 => Stage?.Signatures?[0] != null ? Stage.Signatures[0] : new Signature();
    public Signature Signature2 => Stage?.Signatures?[1] != null ? Stage.Signatures[1] : new Signature();

    public bool CanEdit { get; set; } = true;

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

    public StageSignViewModel(
        INavigationService navigationService,
        IUserService userService,
        IMessageService messageService,
        IActionsService actionsService
        ) : base(navigationService, userService, messageService, actionsService)
    {
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
            if (Stage.NumberOfSignaturesRequired > 0 && !SignatureHelper.SaveSignatureStreams(sender.FirstSignature, sender.SecondSignature)) return;

            await Submit();
        });

        bool isDoubleSignature = Stage.NumberOfSignaturesRequired > 1;

        if (isDoubleSignature && CanEdit)
        {
            List<UserProfileModel> users = await _userService.GetCompanyUsersAsync();
            users.Remove(users.Where(x => x.Id == UserSettings.Id).FirstOrDefault());
            Users = new ObservableCollection<UserProfileModel>(users);

            string placeholder = TranslateExtension.GetValueFromDictionary(LanguageConstants.signChecklistScreenNamePlaceholderText);
            CoSignerName = placeholder;
        }

        SignatureHelper = new SignatureHelperControl() { IsDoubleSignatureRequired = isDoubleSignature };

        AreSignaturesVisible = Stage.NumberOfSignaturesRequired > 0;

        if (!CanEdit)
        {
            Fullname = Stage?.Signatures[0]?.SignedBy;
            CoSignerName = Stage?.Signatures.Count > 1 ? Stage?.Signatures[1]?.SignedBy : null;
        }

        IsBusy = false;
        await Task.Run(async () => await base.Init());
    }

    private async Task Submit()
    {
        SignatureHelper.CoSignerName = CoSignerName;
        SignatureHelper.CoSignerId = CoSignerId;

        ButtonEnabled = false;

        await SubmitAndContinueAsync();

        IsBusy = false;
    }

    private async Task SubmitAndContinueAsync()
    {
        Debug.WriteLine($"Submiting: {DateTimeHelper.Now.Millisecond}");

        var savedSignatures = await SignatureHelper.SaveAndGetSignatures();
        StagesControl.SaveStage(Stage.Id, savedSignatures);
        await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.StageSigned); });
        await NavigationService.CloseAsync();
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
        base.Dispose(disposing);
    }
}
