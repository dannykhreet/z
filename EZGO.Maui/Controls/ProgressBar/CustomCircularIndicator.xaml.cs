namespace EZGO.Maui.Controls.ProgressBar;

public partial class CustomCircularIndicator : ContentView
{
    public CustomCircularIndicator()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusyProperty), typeof(bool), typeof(CustomCircularIndicator), propertyChanged: OnIsBusyPropertyChanged);

    private static void OnIsBusyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CustomCircularIndicator;
        obj.IsBusy = (bool)newValue;
    }

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set
        {
            SetValue(IsBusyProperty, value);
            OnPropertyChanged();
        }
    }
}
