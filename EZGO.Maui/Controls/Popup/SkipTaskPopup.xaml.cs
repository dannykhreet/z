using System.Windows.Input;
using EZGO.Maui.Core.Extensions;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class SkipTaskPopup : SfPopup
{
    public static readonly BindableProperty CancelButtonCommandProperty = BindableProperty.Create(
          nameof(CancelButtonCommand),
          typeof(ICommand),
          typeof(SkipTaskPopup));

    public static readonly BindableProperty SubmitButtonCommandProperty = BindableProperty.Create(
      nameof(SubmitButtonCommand),
      typeof(ICommand),
      typeof(SkipTaskPopup));


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

    public SkipTaskPopup()
	{
		InitializeComponent();
	}
}
