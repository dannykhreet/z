using EZGO.Maui.Core;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Signatures;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Controls.Signatures;

public partial class SignatureControl2 : ContentView, IDisposable
{
    private StreamImageSource _firstImageSource;
    private StreamImageSource _secondImageSource;
    private ISignatureChangedEventSender sender;
    private string placeholder;

    public SignatureControl2()
    {
        InitializeComponent();

        MessagingCenter.Subscribe<SignatureHelperControl>(this, Constants.ResetSignatureMessage, source =>
        {
            _firstImageSource = null;
            SignaturePadEdit.Clear();
        });
        MessagingCenter.Subscribe<SignatureHelperControl>(this, Constants.ResetSignature2Message, source =>
        {
            _secondImageSource = null;
            SignaturePad2Edit.Clear();
        });
        placeholder = TranslateExtension.GetValueFromDictionary(LanguageConstants.signChecklistScreenNamePlaceholderText);
    }

    #region Commands

    public static readonly BindableProperty CancelOperactionCommandProperty = BindableProperty.Create(nameof(CancelOperactionCommand), typeof(ICommand), typeof(SignatureControl2), propertyChanged: ONCancelOperationCommandChanged);
    private static void ONCancelOperationCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.CancelOperactionCommand = (ICommand)newValue;
    }

    public static readonly BindableProperty ResetSignatureCommandProperty = BindableProperty.Create(nameof(ResetSignatureCommand), typeof(ICommand), typeof(SignatureControl2), propertyChanged: OnResetSignatureCommandPropertyChanged);
    private static void OnResetSignatureCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.ResetSignatureCommand = (ICommand)newValue;
    }

    public static readonly BindableProperty ResetSignature2CommandProperty = BindableProperty.Create(nameof(ResetSignature2Command), typeof(ICommand), typeof(SignatureControl2), propertyChanged: OnResetSignature2CommandPropertyChanged);
    private static void OnResetSignature2CommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.ResetSignature2Command = (ICommand)newValue;
    }

    public static readonly BindableProperty OpenUserPopupCommandProperty = BindableProperty.Create(nameof(OpenUserPopupCommand), typeof(ICommand), typeof(SignatureControl2), propertyChanged: OnOpenUserCommandPropertyChanged);
    private static void OnOpenUserCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.OpenUserPopupCommand = (ICommand)newValue;
    }

    public ICommand CancelOperactionCommand { get => (ICommand)GetValue(CancelOperactionCommandProperty); set { SetValue(CancelOperactionCommandProperty, value); OnPropertyChanged(); } }
    public ICommand ResetSignatureCommand { get => (ICommand)GetValue(ResetSignatureCommandProperty); set { SetValue(ResetSignatureCommandProperty, value); OnPropertyChanged(); } }
    public ICommand ResetSignature2Command { get => (ICommand)GetValue(ResetSignature2CommandProperty); set { SetValue(ResetSignature2CommandProperty, value); OnPropertyChanged(); } }
    public ICommand OpenUserPopupCommand { get => (ICommand)GetValue(OpenUserPopupCommandProperty); set { SetValue(OpenUserPopupCommandProperty, value); OnPropertyChanged(); } }

    #endregion

    #region PropertyStrings

    public static readonly BindableProperty FullnameProperty = BindableProperty.Create(nameof(Fullname), typeof(string), typeof(SignatureControl2), propertyChanged: OnFullnamePropertyChanged);
    private static void OnFullnamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.Fullname = (string)newValue;
    }

    public static readonly BindableProperty CoSignerNameProperty = BindableProperty.Create(nameof(CoSignerName), typeof(string), typeof(SignatureControl2), propertyChanged: OnCoSignerNamePropertyChanged);
    private static void OnCoSignerNamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.CoSignerName = (string)newValue;
    }

    public static readonly BindableProperty ShiftNotesProperty = BindableProperty.Create(nameof(ShiftNotes), typeof(string), typeof(SignatureControl2), propertyChanged: OnShiftNotesPropertyChanged);
    private static void OnShiftNotesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.ShiftNotes = (string)newValue;
    }

    public string Fullname { get => (string)GetValue(FullnameProperty); set { SetValue(FullnameProperty, value); OnPropertyChanged(); } }
    public string CoSignerName { get => (string)GetValue(CoSignerNameProperty); set { SetValue(CoSignerNameProperty, value); OnPropertyChanged(); } }
    public string ShiftNotes { get => (string)GetValue(ShiftNotesProperty); set { SetValue(ShiftNotesProperty, value); OnPropertyChanged(); } }
    #endregion

    #region Booleans

    public static readonly BindableProperty IsDoubleSignatureRequiredProperty = BindableProperty.Create(nameof(IsDoubleSignatureRequired), typeof(bool), typeof(SignatureControl2), propertyChanged: OnIsDoubleSignatureRequiredPropertyChanged);
    private static void OnIsDoubleSignatureRequiredPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.IsDoubleSignatureRequired = (bool)newValue;
    }

    public static readonly BindableProperty ButtonEnabledProperty = BindableProperty.Create(nameof(ButtonEnabled), typeof(bool), typeof(SignatureControl2), propertyChanged: OnButtonEnabledPropertyChanged);
    private static void OnButtonEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.ButtonEnabled = (bool)newValue;
    }

    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(SignatureControl2), propertyChanged: OnIsBusyPropertyChanged);
    private static void OnIsBusyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.IsBusy = (bool)newValue;
    }

    public static readonly BindableProperty UseShiftNotesProperty = BindableProperty.Create(nameof(UseShiftNotes), typeof(bool), typeof(SignatureControl2), propertyChanged: OnUseShiftNotesPropertyChanged, defaultValue: false);
    private static void OnUseShiftNotesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.UseShiftNotes = (bool)newValue;
    }

    public static readonly BindableProperty AreSignaturesVisibleProperty = BindableProperty.Create(nameof(AreSignaturesVisible), typeof(bool), typeof(SignatureControl2), propertyChanged: OnAreSignaturesVisiblePropertyChanged, defaultValue: true);

    private static void OnAreSignaturesVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl2;
        bind.AreSignaturesVisible = (bool)newValue;
        if (!bind.AreSignaturesVisible)
        {
            bind.NotesEditor.IsVisible = true;
            bind.ExpandIconNotes.Rotation = 180;
        }
        else
        {
            bind.NotesEditor.IsVisible = false;
            bind.ExpandIconNotes.Rotation = 0;
        }
    }

    public static readonly BindableProperty NumberOfSignatureRequiredProperty = BindableProperty.Create(nameof(NumberOfSignatureRequired), typeof(int), typeof(SignatureControl2));


    public bool IsDoubleSignatureRequired
    {
        get => (bool)GetValue(IsDoubleSignatureRequiredProperty);
        set { SetValue(IsDoubleSignatureRequiredProperty, value); OnPropertyChanged(nameof(IsDoubleSignatureRequired)); }
    }
    public bool ButtonEnabled
    {
        get => (bool)GetValue(ButtonEnabledProperty);
        set { SetValue(ButtonEnabledProperty, value); OnPropertyChanged(); }
    }
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set
        {
            SetValue(IsBusyProperty, value);
            OnPropertyChanged();
        }
    }

    public bool UseShiftNotes
    {
        get => (bool)GetValue(UseShiftNotesProperty);
        set { SetValue(UseShiftNotesProperty, value); OnPropertyChanged(); }
    }

    public int NumberOfSignatureRequired
    {
        get => (int)GetValue(NumberOfSignatureRequiredProperty);
        set { SetValue(NumberOfSignatureRequiredProperty, value); OnPropertyChanged(); }
    }

    public bool AreSignaturesVisible
    {
        get => (bool)GetValue(AreSignaturesVisibleProperty);
        set { SetValue(AreSignaturesVisibleProperty, value); OnPropertyChanged(nameof(AreSignaturesVisible)); }
    }

    #endregion

    private async void SubmitAudit(object obj, EventArgs args)
    {
        if (IsBusy) return;

        IsBusy = true;
        ButtonEnabled = false;
        await Task.Delay(10);

        if (await DisplayDialog())
        {
            IsBusy = false;
            ButtonEnabled = true;
            return;
        }
        sender = new SaveSignatureEventSender(firstSignature: _firstImageSource, secnodSignature: _secondImageSource);

        sender.Send(Constants.SignTemplateMessage);

    }

    private async Task<bool> DisplayDialog()
    {
        if (NumberOfSignatureRequired == 0)
            return false;

        using var scope = App.Container.CreateScope();
        var navigationService = scope.ServiceProvider.GetService<INavigationService>();
        var statusBarService = DependencyService.Resolve<IStatusBarService>();
        Page page = navigationService.GetCurrentPage();

        string nosignature = TranslateExtension.GetValueFromDictionary(LanguageConstants.signAuditScreenNoSignatureError);
        string noname = TranslateExtension.GetValueFromDictionary(LanguageConstants.signAuditScreenNoNamesError);
        string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);

        bool result = false;

        // Timeout used to ensure DisplayActionSheet does not hang indefinitely in edge cases
        // such as when user closes it by spamming to ok button.
        // This allows the app to continue execution.
        const int timeoutMs = 4000;

        if (CoSignerName == placeholder)
        {
            var task = page.DisplayActionSheet(noname, null, ok);
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));

            result = true;
        }
        else if (!CheckConditions())
        {
            var task = page.DisplayActionSheet(nosignature, null, ok);
            var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));

            result = true;
        }

        statusBarService.HideStatusBar();
        return result;
    }

    private bool CheckConditions()
    {
        bool result = false;

        if (_firstImageSource != null)
        {
            result = true;

            if (IsDoubleSignatureRequired)
            {
                if (_secondImageSource == null) result = false;
            }
        }

        return result;
    }

    public void Dispose()
    {
        MessagingCenter.Unsubscribe<SignatureHelperControl>(this, Constants.ResetSignatureMessage);
        MessagingCenter.Unsubscribe<SignatureHelperControl>(this, Constants.ResetSignature2Message);

        _firstImageSource = null;
        _secondImageSource = null;
        sender = null;
    }

    void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        Signature1Pad.IsVisible = !Signature1Pad.IsVisible;
        if (Signature1Pad.IsVisible)
        {
            ExpandIconPad1.Rotation = 0;
            NotesEditor.IsVisible = false;
            ExpandIconNotes.Rotation = 0;
        }
        else
            ExpandIconPad1.Rotation = 180;
    }

    void TapGestureRecognizer_Tapped_1(System.Object sender, System.EventArgs e)
    {
        Signature2Pad.IsVisible = !Signature2Pad.IsVisible;
        if (Signature2Pad.IsVisible)
        {
            ExpandIconPad2.Rotation = 0;
            NotesEditor.IsVisible = false;
            ExpandIconNotes.Rotation = 0;
        }
        else
            ExpandIconPad2.Rotation = 180;
    }
    private void OnMainLayoutTapped(object sender, EventArgs e)
    {
        NotesEditor.Unfocus();
        NotesEditor.IsHintAlwaysFloated = false;
        NotesEditor.IsHintAlwaysFloated = true;
    }

    void TapGestureRecognizer_Tapped_2(System.Object sender, System.EventArgs e)
    {
        NotesEditor.IsVisible = !NotesEditor.IsVisible;
        if (NotesEditor.IsVisible)
        {
            ExpandIconNotes.Rotation = 180;
            Signature1Pad.IsVisible = false;
            Signature2Pad.IsVisible = false;
            ExpandIconPad1.Rotation = ExpandIconPad2.Rotation = 180;
        }
        else
        {
            ExpandIconNotes.Rotation = 0;

            Signature1Pad.IsVisible = true;
            Signature2Pad.IsVisible = true;
            ExpandIconPad1.Rotation = ExpandIconPad2.Rotation = 0;
        }
    }

    private void SfSignaturePad_DrawCompleted(System.Object sender, System.EventArgs e)
    {
        if (SignaturePadEdit?.ToImageSource() is StreamImageSource source)
        {
            if (source != null && !source.IsEmpty) _firstImageSource = source;
        }
    }

    private void SfSignaturePad2_DrawCompleted(System.Object sender, System.EventArgs e)
    {
        if (SignaturePad2Edit?.ToImageSource() is StreamImageSource secondSource)
        {
            if (secondSource != null && !secondSource.IsEmpty) _secondImageSource = secondSource;
        }
    }
}