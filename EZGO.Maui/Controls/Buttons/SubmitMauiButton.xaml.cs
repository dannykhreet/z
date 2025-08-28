using CommunityToolkit.Mvvm.Input;
using MvvmHelpers.Interfaces;

namespace EZGO.Maui.Controls.Buttons;

public partial class SubmitMauiButton : Grid
{
    public event EventHandler Tapped;


    public readonly static BindableProperty IsLoadingProperty = BindableProperty.Create(nameof(IsLoadingProperty), typeof(bool), typeof(SubmitMauiButton), propertyChanged: OnIsLoadingPropertyChanged, defaultValue: false);

    private static void OnIsLoadingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as SubmitMauiButton;
        obj.indicator.IsRunning = obj.IsLoading;
        obj.indicator.IsVisible = obj.IsLoading;
        obj.buttonContent.IsVisible = !obj.IsLoading;
    }

    public readonly static BindableProperty TextSizeProperty = BindableProperty.Create(nameof(TextSize), typeof(string), typeof(SubmitMauiButton), propertyChanged: OnTextSizePropertyChanged, defaultValue: "Small");

    private static void OnTextSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as SubmitMauiButton;
        var fontConverter = new FontSizeConverter();
        double size = (double)fontConverter.ConvertFromInvariantString(obj.TextSize);
        obj.buttonContent.FontSize = size;
    }

    public static readonly BindableProperty ButtonTextProperty = BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(SubmitMauiButton));

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public SubmitMauiButton()
    {
        InitializeComponent();

        buttonContent.IsVisible = !IsLoading;
        indicator.IsVisible = IsLoading;
        indicator.IsRunning = IsLoading;
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set
        {
            SetValue(IsLoadingProperty, value);
            OnPropertyChanged();
        }
    }

    public string TextSize
    {
        get => (string)GetValue(TextSizeProperty);
        set => SetValue(TextSizeProperty, value);
    }

    public readonly static BindableProperty LoadingIndicator = BindableProperty.Create(nameof(LoadingIndicatorSize), typeof(double), typeof(SubmitMauiButton), propertyChanged: OnLoadingIndicatorSizePropertyChanged, defaultValue: 1.5);

    public double LoadingIndicatorSize
    {
        get => (double)GetValue(LoadingIndicator);
        set => SetValue(LoadingIndicator, value);
    }
    private static void OnLoadingIndicatorSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as SubmitMauiButton;
        obj.indicator.Scale = (double)newValue;
    }

    public static readonly BindableProperty ClickedProperty = BindableProperty.Create(nameof(Clicked), typeof(IAsyncRelayCommand), typeof(SubmitMauiButton));

    public IAsyncRelayCommand Clicked
    {
        get => (IAsyncRelayCommand)GetValue(ClickedProperty);
        set { SetValue(ClickedProperty, value); OnPropertyChanged(); }
    }

    void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        if (Tapped != null)
            Tapped(this, new EventArgs());
    }
}
