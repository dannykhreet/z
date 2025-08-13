using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Filters;

public partial class FilterButton : SfButton
{
	public bool IsButtonChecked { get; set; }

	private static Color ResourcesBackgroundColor => ResourceHelper.GetApplicationResource<Color>("GreyColor");

	public static BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), declaringType: typeof(FilterButton));
	public static BindableProperty IconNameProperty = BindableProperty.Create(nameof(IconName), typeof(string), declaringType: typeof(FilterButton));
	public static BindableProperty IsIconVisibleProperty = BindableProperty.Create(nameof(IsIconVisible), typeof(bool), declaringType: typeof(FilterButton));
	public static BindableProperty IconStyleProperty = BindableProperty.Create(nameof(IconStyle), typeof(string), declaringType: typeof(FilterButton));

	public static BindableProperty ButtonColorProperty = BindableProperty.Create(nameof(ButtonColor), typeof(Color), declaringType: typeof(FilterButton), defaultValue: ResourcesBackgroundColor);

	public Color ButtonColor
	{
		get => (Color)GetValue(ButtonColorProperty);
		set => SetValue(ButtonColorProperty, value);
	}

	public static BindableProperty IsActiveProperty = BindableProperty.Create(nameof(IsActive), typeof(bool), declaringType: typeof(FilterButton), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnIsActivePropertyChanged);

	private static void OnIsActivePropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as FilterButton;
		obj.IsActive = (bool)newValue;
		if (obj.IsActive)
			obj.IsChecked = true;
		else
			obj.IsChecked = false;
	}

	public static BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), declaringType: typeof(FilterButton), propertyChanged: OnIsExpandedPropertyChanged, defaultBindingMode: BindingMode.TwoWay);

	private static void OnIsExpandedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var obj = bindable as FilterButton;
		obj.IsExpanded = (bool)newValue;
	}

	public bool IsIconVisible
	{
		get => (bool)GetValue(IsIconVisibleProperty);
		set => SetValue(IsIconVisibleProperty, value);
	}

	public string Name
	{
		get => (string)GetValue(NameProperty);
		set => SetValue(NameProperty, value);
	}

	public string IconName
	{
		get => (string)GetValue(IconNameProperty);
		set => SetValue(IconNameProperty, value);
	}

	public string IconStyle
	{
		get => (string)GetValue(IconStyleProperty);
		set => SetValue(IconStyleProperty, value);
	}

	public bool IsActive
	{
		get => (bool)GetValue(IsActiveProperty);
		set { SetValue(IsActiveProperty, value); OnPropertyChanged("IsActive"); }
	}

	public bool IsExpanded
	{
		get => (bool)GetValue(IsExpandedProperty);
		set => SetValue(IsExpandedProperty, value);
	}

	public FilterButton()
	{
		InitializeComponent();
	}
}