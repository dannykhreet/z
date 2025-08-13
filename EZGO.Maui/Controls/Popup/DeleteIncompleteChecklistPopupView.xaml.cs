using System.Windows.Input;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class DeleteIncompleteChecklistPopupView : SfPopup
{
	public static readonly BindableProperty CancelButtonCommandProperty = BindableProperty.Create(nameof(CancelButtonCommand), typeof(ICommand), typeof(DeleteIncompleteChecklistPopupView));

	public ICommand CancelButtonCommand
	{
		get => (ICommand)GetValue(CancelButtonCommandProperty);
		set
		{
			SetValue(CancelButtonCommandProperty, value);
			OnPropertyChanged();
		}
	}

	public static readonly BindableProperty DeleteButtonCommandProperty = BindableProperty.Create(nameof(DeleteButtonCommand), typeof(ICommand), typeof(DeleteIncompleteChecklistPopupView));

	public ICommand DeleteButtonCommand
	{
		get => (ICommand)GetValue(DeleteButtonCommandProperty);
		set
		{
			SetValue(DeleteButtonCommandProperty, value);
			OnPropertyChanged();
		}
	}

	public DeleteIncompleteChecklistPopupView()
	{
		InitializeComponent();
	}
}