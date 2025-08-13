using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Buttons;

public partial class CustomFilterButton : SfButton
{
	public static readonly BindableProperty IsButtonClickedProperty = BindableProperty.Create(nameof(IsButtonClicked), typeof(bool), typeof(CustomFilterButton));

	public bool IsButtonClicked
	{
		get => (bool)GetValue(IsButtonClickedProperty);
		set => SetValue(IsButtonClickedProperty, value);
	}

	public CustomFilterButton()
	{
		InitializeComponent();
		Command = new Command(() =>
		{
			IsButtonClicked = !IsButtonClicked;
			OnPropertyChanged(nameof(IsButtonClicked));
		});
	}
}