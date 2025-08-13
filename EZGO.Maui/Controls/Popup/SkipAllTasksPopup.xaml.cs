using System.Windows.Input;
using EZGO.Maui.Core.Extensions;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class SkipAllTasksPopup : SfPopup
{
    public static readonly BindableProperty CancelButtonCommandProperty = BindableProperty.Create(
          nameof(CancelButtonCommand),
          typeof(ICommand),
          typeof(SkipAllTasksPopup));

    public static readonly BindableProperty SubmitButtonCommandProperty = BindableProperty.Create(
      nameof(SubmitButtonCommand),
      typeof(ICommand),
      typeof(SkipAllTasksPopup));

    public readonly static BindableProperty InputTextProperty = BindableProperty.Create(
        nameof(InputTextProperty),
        typeof(string),
        typeof(SkipAllTasksPopup),
        defaultBindingMode: BindingMode.TwoWay);

    public readonly static BindableProperty IsInputValidProperty = BindableProperty.Create(
        nameof(IsInputValidProperty),
        typeof(bool),
        typeof(SkipAllTasksPopup));

    public bool IsInputValid
    {
        get => (bool)GetValue(IsInputValidProperty);
        set
        {
            SetValue(IsInputValidProperty, value);
            OnPropertyChanged();
        }
    }

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set
        {
            SetValue(InputTextProperty, value);
            IsInputValid = !value.IsNullOrWhiteSpace();
            OnPropertyChanged();
        }
    }

    public ICommand CancelButtonCommand
    {
        get => (ICommand)GetValue(CancelButtonCommandProperty);
        set
        {
            SetValue(CancelButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand SubmitButtonCommand
    {
        get => (ICommand)GetValue(SubmitButtonCommandProperty);
        set
        {
            SetValue(SubmitButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public string CancelButtonText { get => TranslateExtension.GetValueFromDictionary("BASE_TEXT_CANCEL"); }
    public string SubmitButtonText { get => TranslateExtension.GetValueFromDictionary("BASE_TEXT_OK"); }

    public SkipAllTasksPopup()
    {
        InitializeComponent();
    }
}
