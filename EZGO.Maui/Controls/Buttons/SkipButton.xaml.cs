using System.Windows.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Classes;
using EZGO.Maui.Core.Classes;
using ResourceHelper = EZGO.Maui.Core.Classes.ResourceHelper;

namespace EZGO.Maui.Controls.Buttons;

public partial class SkipButton : StackLayout
{
    public static readonly BindableProperty IsSkippedTextVisibleProperty = BindableProperty.Create(nameof(IsSkippedTextVisible), typeof(bool), typeof(SkipButton));

    public bool IsSkippedTextVisible
    {
        get => (bool)GetValue(IsSkippedTextVisibleProperty);
        set => SetValue(IsSkippedTextVisibleProperty, value);
    }

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SkipButton));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty FilterStatusProperty = BindableProperty.Create(nameof(FilterStatus), typeof(TaskStatusEnum?), typeof(SkipButton), propertyChanged: OnStatusPropertyChanged);

    public TaskStatusEnum? FilterStatus
    {
        get => (TaskStatusEnum?)GetValue(FilterStatusProperty);
        set => SetValue(FilterStatusProperty, value);
    }

    private static void OnStatusPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var button = bindable as SkipButton;

        button.SetBlockedState();
    }

    public static readonly BindableProperty ButtonColorProperty = BindableProperty.Create(nameof(ButtonColor), typeof(Color), typeof(SkipButton), defaultValue: Colors.White, propertyChanged: OnButtonColorPropertyChanged);

    private static void OnButtonColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var obj = bindable as SkipButton;
            if (obj.FilterStatus != TaskStatusEnum.Skipped)
            {
                obj.button.Stroke = obj.ButtonColor;
            }
            else
            {
                obj.button.Stroke = Classes.ResourceHelper.GetValueFromResources<Color>("SkippedColor");
            }
        });
    }

    public Color ButtonColor
    {
        get => (Color)GetValue(ButtonColorProperty);
        set
        {
            SetValue(ButtonColorProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty ButtonInitColorProperty = BindableProperty.Create(nameof(ButtonInitColor), typeof(Color), typeof(SkipButton), defaultValue: Colors.White);

    public Color ButtonInitColor
    {
        get => (Color)GetValue(ButtonInitColorProperty);
        set => SetValue(ButtonInitColorProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SkipButton));

    public object CommandParameter
    {
        get => (object)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public SkipButton()
    {
        InitializeComponent();

        if (Settings.IsRightToLeftLanguage)
            ArabicConversion();
    }

    private void ArabicConversion()
    {
        button.Rotation = 180;
    }

    public static readonly BindableProperty IsBlockedProperty =
        BindableProperty.Create(
            nameof(IsBlocked),
            typeof(bool),
            typeof(SkipButton),
            propertyChanged: OnIsBlockedPropertyChanged);

    public bool IsBlocked
    {
        get => (bool)GetValue(IsBlockedProperty);
        set
        {
            SetValue(IsBlockedProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty IconColorProperty =
    BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(SkipButton), defaultValue: Colors.Black);

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    private static void OnIsBlockedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var button = bindable as SkipButton;

        button.SetBlockedState();
    }

    private void SetBlockedState()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var skipButton = button;

            if (FilterStatus == TaskStatusEnum.Skipped)
                return;

            if (IsBlocked)
            {
                skipButton.Background = ResourceHelper.GetApplicationResource<Color>("LockedButtonBackgroundColor");
                skipButton.Stroke = ResourceHelper.GetApplicationResource<Color>("LockedButtonBackgroundColor");
                IconColor = ResourceHelper.GetApplicationResource<Color>("LockedButtonIconColor");
            }
            else
            {
                skipButton.Background = Colors.Transparent;
                skipButton.Stroke = ButtonInitColor;
                IconColor = ButtonInitColor;
            }
        });
    }
}
