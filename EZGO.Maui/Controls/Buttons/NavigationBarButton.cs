using EZGO.Maui.Classes;

namespace EZGO.Maui.Controls.Buttons;

public class NavigationBarButton : Button
{
    public readonly static BindableProperty IconProperty = BindableProperty.Create(nameof(IconProperty), typeof(IconFont), typeof(NavigationBarButton), propertyChanged: OnIconPropertyChanged);
    private static void OnIconPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as NavigationBarButton;
        obj.Text = $"{obj.Icon} {obj.Text}";
    }

    public NavigationBarButton()
    {
        TextColor = ResourceHelper.GetValueFromResources<Color>("GreenColor");
        FontFamily = ResourceHelper.GetValueFromResources<string>("RobotoLight");
        //VerticalOptions = LayoutOptions.CenterAndExpand;
        //HorizontalOptions = LayoutOptions.EndAndExpand;
        BackgroundColor = Colors.Transparent;
        Margin = new Thickness(0, 0, 5, 0);
        FontSize = 17;
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set
        {
            SetValue(IconProperty, value);
            OnPropertyChanged();
        }
    }
}
