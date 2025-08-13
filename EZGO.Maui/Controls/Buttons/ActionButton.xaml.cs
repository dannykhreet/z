using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Buttons;

public partial class ActionButton : SfButton
{
    public ActionButton()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ActionBubbleCountProperty = BindableProperty.Create(nameof(ActionBubbleCount), typeof(int), typeof(ActionButton));

    public int ActionBubbleCount
    {
        get => (int)GetValue(ActionBubbleCountProperty);
        set => SetValue(ActionBubbleCountProperty, value);
    }

    public static readonly BindableProperty IconSizeProperty = BindableProperty.Create(nameof(IconSize), typeof(double), typeof(ActionButton), defaultValue: 30.0);

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
}
