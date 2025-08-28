using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Buttons;

public partial class ThumbDownButton : SfButton
{
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(nameof(Status), typeof(TaskStatusEnum), typeof(ThumbDownButton), propertyChanged: OnStatusPropertyChanged);

    public TaskStatusEnum Status
    {
        get => (TaskStatusEnum)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    private static void OnStatusPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var notOkButton = bindable as ThumbDownButton;

        notOkButton.SetBlockedState();
    }

    public static readonly BindableProperty ButtonColorProperty = BindableProperty.Create(nameof(ButtonColor), typeof(Color), typeof(ThumbDownButton), defaultValue: Colors.White);

    public Color ButtonColor
    {
        get => (Color)GetValue(ButtonColorProperty);
        set => SetValue(ButtonColorProperty, value);
    }

    public static readonly BindableProperty ButtonInitColorProperty = BindableProperty.Create(nameof(ButtonInitColor), typeof(Color), typeof(ThumbDownButton), defaultValue: Colors.White);

    public Color ButtonInitColor
    {
        get => (Color)GetValue(ButtonInitColorProperty);
        set => SetValue(ButtonInitColorProperty, value);
    }

    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
    nameof(IconColor), typeof(Color), typeof(ThumbDownButton), defaultValue: Colors.White);

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public ThumbDownButton()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IsBlockedProperty =
            BindableProperty.Create(
                nameof(IsBlocked),
                typeof(bool),
                typeof(ThumbDownButton),
                propertyChanged: OnIsBlockedPropertyChanged);

    public bool IsBlocked
    {
        get => (bool)GetValue(IsBlockedProperty);
        set
        {
            SetValue(IsBlockedProperty, value);
            OnPropertyChanged(nameof(IsBlocked));
        }
    }

    private static void OnIsBlockedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var notOkButton = bindable as ThumbDownButton;

        notOkButton.SetBlockedState();
    }

    private void SetBlockedState()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Status == TaskStatusEnum.NotOk)
                return;

            if (IsBlocked)
            {
                thumbDownButton.Background = ResourceHelper.GetApplicationResource<Color>("LockedButtonBackgroundColor");
                thumbDownButton.Stroke = ResourceHelper.GetApplicationResource<Color>("LockedButtonBackgroundColor");
                IconColor = ResourceHelper.GetApplicationResource<Color>("LockedButtonIconColor");
            }
            else
            {
                thumbDownButton.Background = Colors.Transparent;
                thumbDownButton.Stroke = ButtonInitColor;
                IconColor = ButtonInitColor;
            }
        });
    }
}
