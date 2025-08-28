using EZGO.Maui.Classes;
using Syncfusion.Maui.Buttons;
using Microsoft.Maui.Controls;

namespace EZGO.Maui.Controls.Buttons;

public partial class SkipAllButton : SfButton
{
    public SkipAllButton()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IsAvailableProperty = BindableProperty.Create(nameof(IsAvailable), typeof(bool), typeof(SkipAllButton), propertyChanged: OnButtonIsClickablePropertyChanged);

    public bool IsAvailable
    {
        get => (bool)GetValue(IsAvailableProperty);
        set
        {
            SetValue(IsAvailableProperty, value);
            OnPropertyChanged(nameof(IsAvailable));
        }
    }

    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
        nameof(IconColor),
        typeof(Color),
        typeof(SkipAllButton),
        default(Color));

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public static readonly BindableProperty TextColorButtonProperty = BindableProperty.Create(
        nameof(TextColorButton),
        typeof(Color),
        typeof(SkipAllButton),
        default(Color));

    public Color TextColorButton
    {
        get => (Color)GetValue(TextColorButtonProperty);
        set => SetValue(TextColorButtonProperty, value);
    }

    private static void OnButtonIsClickablePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as SkipAllButton;

        if (!obj.IsAvailable)
        {
            obj.Background = ResourceHelper.GetValueFromResources<Color>("SkippedColor");
            obj.TextColorButton = ResourceHelper.GetValueFromResources<Color>("LightGreyColor");
            obj.IconColor = ResourceHelper.GetValueFromResources<Color>("LightGreyColor");
            return;
        }
        obj.Background = ResourceHelper.GetValueFromResources<Color>("White");
        obj.TextColorButton = ResourceHelper.GetValueFromResources<Color>("SkippedColor");
        obj.IconColor = ResourceHelper.GetValueFromResources<Color>("SkippedColor");
    }
}
