using System.Windows.Input;
using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls;

public partial class TagButton : SfButton
{
    private static Color ResourcesBackgroundColor => ResourceHelper.GetApplicationResource<Color>("GreyColor");

    public static BindableProperty TagTextProperty = BindableProperty.Create(nameof(TagText), typeof(string), declaringType: typeof(TagButton));
    public static BindableProperty TagIconNameProperty = BindableProperty.Create(nameof(TagIconName), typeof(string), declaringType: typeof(TagButton));
    public static BindableProperty TagIconStyleProperty = BindableProperty.Create(nameof(TagIconStyle), typeof(string), declaringType: typeof(TagButton));

    public static BindableProperty TagBackgroundColorProperty = BindableProperty.Create(nameof(TagBackgroundColor), typeof(Color), declaringType: typeof(TagButton), defaultValue: ResourcesBackgroundColor);
    public Color TagBackgroundColor
    {
        get => (Color)GetValue(TagBackgroundColorProperty);
        set => SetValue(TagBackgroundColorProperty, value);
    }

    public static BindableProperty IsActiveProperty = BindableProperty.Create(nameof(IsActive), typeof(bool), declaringType: typeof(TagButton), defaultBindingMode: BindingMode.TwoWay);
    public static BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), declaringType: typeof(TagButton), propertyChanged: OnIsExpandedPropertyChanged, defaultBindingMode: BindingMode.TwoWay);

    private static void OnIsExpandedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as TagButton;
        obj.IsExpanded = (bool)newValue;
    }

    public bool IsIconVisible { get; set; }

    public string TagText
    {
        get => (string)GetValue(TagTextProperty);
        set => SetValue(TagTextProperty, value);
    }

    public string TagIconName
    {
        get => (string)GetValue(TagIconNameProperty);
        set => SetValue(TagIconNameProperty, value);
    }

    public string TagIconStyle
    {
        get => (string)GetValue(TagIconStyleProperty);
        set => SetValue(TagIconStyleProperty, value);
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

    public ICommand TapCommand { get; set; }

    public TagButton()
    {
        TapCommand = new Command(() =>
        {
            IsActive = !IsActive;
        });

        InitializeComponent();
    }
}
