using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.ListView;

namespace EZGO.Maui.Controls.Lists;

public partial class StageList : SfListView
{
	public static readonly BindableProperty IsSignedProperty = BindableProperty.Create(nameof(IsSigned), typeof(bool), typeof(StageList), propertyChanged: OnIsSignedOrLockedPropertyChanged);

	public bool IsSigned
	{
		get => (bool)GetValue(IsSignedProperty);
		set => SetValue(IsSignedProperty, value);
	}

	public static readonly BindableProperty IsLockedProperty = BindableProperty.Create(nameof(IsLocked), typeof(bool), typeof(StageList), propertyChanged: OnIsSignedOrLockedPropertyChanged);

	public bool IsLocked
	{
		get => (bool)GetValue(IsLockedProperty);
		set => SetValue(IsLockedProperty, value);
	}

	private static void OnIsSignedOrLockedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as StageList;

		obj.SetBackgroundColor();
	}

	private void SetBackgroundColor()
	{
		if (IsSigned)
		{
			BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightGreenColor");
		}
		else if (IsLocked)
		{
			BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightGreyColor");
		}
		else
		{
			BackgroundColor = ResourceHelper.GetApplicationResource<Color>("LightBlueColor");
		}
	}

	public StageList()
	{
		InitializeComponent();
	}
}