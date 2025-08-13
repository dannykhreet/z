using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Buttons;

public partial class ImageButton : SfButton
{
    public static readonly BindableProperty ImageUrlProperty = BindableProperty.Create(nameof(ImageUrl), typeof(string), typeof(ImageButton));

    public string ImageUrl
    {
        get => (string)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public ImageButton()
    {
        InitializeComponent();
    }
}
