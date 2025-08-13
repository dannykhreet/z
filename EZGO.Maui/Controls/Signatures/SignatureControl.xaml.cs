using EZGO.Maui.Core;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Signatures;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using System.Windows.Input;

namespace EZGO.Maui.Controls.Signatures;

public partial class SignatureControl : ContentView
{
    private StreamImageSource _firstImageSource;
    private StreamImageSource _secondImageSource;
    private ISignatureChangedEventSender sender;
    private string placeholder;

    public SignatureControl()
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

    public static readonly BindableProperty CancelOperactionCommandProperty = BindableProperty.Create(nameof(CancelOperactionCommand), typeof(ICommand), typeof(SignatureControl), propertyChanged: ONCancelOperationCommandChanged);
    private static void ONCancelOperationCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.CancelOperactionCommand = (ICommand)newValue;
    }

    public static readonly BindableProperty ResetSignatureCommandProperty = BindableProperty.Create(nameof(ResetSignatureCommand), typeof(ICommand), typeof(SignatureControl), propertyChanged: OnResetSignatureCommandPropertyChanged);
    private static void OnResetSignatureCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.ResetSignatureCommand = (ICommand)newValue;
    }

    public static readonly BindableProperty ResetSignature2CommandProperty = BindableProperty.Create(nameof(ResetSignature2Command), typeof(ICommand), typeof(SignatureControl), propertyChanged: OnResetSignature2CommandPropertyChanged);
    private static void OnResetSignature2CommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.ResetSignature2Command = (ICommand)newValue;
    }

    public static readonly BindableProperty OpenUserPopupCommandProperty = BindableProperty.Create(nameof(OpenUserPopupCommand), typeof(ICommand), typeof(SignatureControl), propertyChanged: OnOpenUserCommandPropertyChanged);
    private static void OnOpenUserCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.OpenUserPopupCommand = (ICommand)newValue;
    }

    public ICommand CancelOperactionCommand { get => (ICommand)GetValue(CancelOperactionCommandProperty); set { SetValue(CancelOperactionCommandProperty, value); OnPropertyChanged(); } }
    public ICommand ResetSignatureCommand { get => (ICommand)GetValue(ResetSignatureCommandProperty); set { SetValue(ResetSignatureCommandProperty, value); OnPropertyChanged(); } }
    public ICommand ResetSignature2Command { get => (ICommand)GetValue(ResetSignature2CommandProperty); set { SetValue(ResetSignature2CommandProperty, value); OnPropertyChanged(); } }
    public ICommand OpenUserPopupCommand { get => (ICommand)GetValue(OpenUserPopupCommandProperty); set { SetValue(OpenUserPopupCommandProperty, value); OnPropertyChanged(); } }

    #endregion

    #region PropertyStrings

    public static readonly BindableProperty FullnameProperty = BindableProperty.Create(nameof(Fullname), typeof(string), typeof(SignatureControl), propertyChanged: OnFullnamePropertyChanged);
    private static void OnFullnamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.Fullname = (string)newValue;
    }

    public static readonly BindableProperty CoSignerNameProperty = BindableProperty.Create(nameof(CoSignerName), typeof(string), typeof(SignatureControl), propertyChanged: OnCoSignerNamePropertyChanged);
    private static void OnCoSignerNamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.CoSignerName = (string)newValue;
    }
    public string Fullname { get => (string)GetValue(FullnameProperty); set { SetValue(FullnameProperty, value); OnPropertyChanged(); } }
    public string CoSignerName { get => (string)GetValue(CoSignerNameProperty); set { SetValue(CoSignerNameProperty, value); OnPropertyChanged(); } }

    #endregion

    #region Booleans

    public static readonly BindableProperty IsDoubleSignatureRequiredProperty = BindableProperty.Create(nameof(IsDoubleSignatureRequired), typeof(bool), typeof(SignatureControl), propertyChanged: OnIsDoubleSignatureRequiredPropertyChanged);
    private static void OnIsDoubleSignatureRequiredPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.IsDoubleSignatureRequired = (bool)newValue;
    }

    public static readonly BindableProperty ButtonEnabledProperty = BindableProperty.Create(nameof(ButtonEnabled), typeof(bool), typeof(SignatureControl), propertyChanged: OnButtonEnabledPropertyChanged);
    private static void OnButtonEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.ButtonEnabled = (bool)newValue;
    }

    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(SignatureControl), propertyChanged: OnIsBusyPropertyChanged);
    private static void OnIsBusyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bind = bindable as SignatureControl;
        bind.IsBusy = (bool)newValue;
    }

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

        await sender.Send(Constants.SignTemplateMessage);

    }

    private async Task<bool> DisplayDialog()
    {
        using var scope = App.Container.CreateScope();
        var navigationService = scope.ServiceProvider.GetService<INavigationService>();
        var statusBarService = DependencyService.Resolve<IStatusBarService>();
        Page page = navigationService.GetCurrentPage();

        string nosignature = TranslateExtension.GetValueFromDictionary(LanguageConstants.signAuditScreenNoSignatureError);
        string noname = TranslateExtension.GetValueFromDictionary(LanguageConstants.signAuditScreenNoNamesError);
        string ok = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextOk);

        bool result = false;

        // Timeout used to ensure DisplayActionSheet does not hang indefinitely in edge cases
        // such as when user closes it by tapping outside, or platform-specific dialog issues occur.
        // This allows the app to continue execution even if no option was selected.
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
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MessagingCenter.Unsubscribe<SignatureHelperControl>(this, Constants.ResetSignatureMessage);
            MessagingCenter.Unsubscribe<SignatureHelperControl>(this, Constants.ResetSignature2Message);
        });

        _firstImageSource = null;
        _secondImageSource = null;
        sender = null;
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
