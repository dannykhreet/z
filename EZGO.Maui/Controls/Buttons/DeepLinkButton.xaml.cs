using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Buttons;

public partial class DeepLinkButton : SfButton
{
    public static readonly BindableProperty InitialBorderColorProperty = BindableProperty.Create(nameof(InitialBorderColor), typeof(Color), typeof(DeepLinkButton));

    public Color InitialBorderColor
    {
        get => (Color)GetValue(InitialBorderColorProperty);
        set => SetValue(InitialBorderColorProperty, value);
    }

    public static readonly BindableProperty IsValidProperty = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(DeepLinkButton), defaultValue: true);

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    public DeepLinkButton()
    {
        InitializeComponent();
    }
}
