using System.Windows.Input;
using EZGO.Maui.Core.Extensions;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class DiscardChangesPopupView : SfPopup
{
	public static readonly BindableProperty CancelButtonCommandProperty = BindableProperty.Create(
	  nameof(CancelButtonCommand),
	  typeof(ICommand),
	  typeof(DiscardChangesPopupView));

	public static readonly BindableProperty SubmitButtonCommandProperty = BindableProperty.Create(
	  nameof(SubmitButtonCommand),
	  typeof(ICommand),
	  typeof(DiscardChangesPopupView));

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

	public string CancelButtonText { get => TranslateExtension.GetValueFromDictionary("BASE_TEXT_DISCARD_PROGRESS"); }
	public string SubmitButtonText { get => TranslateExtension.GetValueFromDictionary("EDIT_SCREEN_SAVE_BUTTON_TITLE"); }

	public DiscardChangesPopupView()
	{
		InitializeComponent();
	}
}