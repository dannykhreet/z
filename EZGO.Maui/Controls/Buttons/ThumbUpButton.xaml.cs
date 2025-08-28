using ExCSS;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.Buttons;
using Color = Microsoft.Maui.Graphics.Color;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace EZGO.Maui.Controls.Buttons;

public partial class ThumbUpButton : SfButton
{
    public ThumbUpButton()
    {
        InitializeComponent();
    }

    private readonly Color buttonTappedColor = Color.FromArgb("#1ed760");

    public static readonly BindableProperty StatusProperty =
        BindableProperty.Create(
            nameof(Status),
            typeof(TaskStatusEnum),
            typeof(ThumbUpButton),
            propertyChanged: OnStatusPropertyChanged);

    public TaskStatusEnum Status
    {
        get => (TaskStatusEnum)GetValue(StatusProperty);
        set
        {
            SetValue(StatusProperty, value);
            OnPropertyChanged(nameof(Status));
        }
    }

    public static readonly BindableProperty ButtonColorProperty = BindableProperty.Create(nameof(ButtonColor), typeof(Color), typeof(ThumbDownButton), defaultValue: Colors.White);

    public Color ButtonColor
    {
        get => (Color)GetValue(ButtonColorProperty);
        set => SetValue(ButtonColorProperty, value);
    }

    public static readonly BindableProperty ButtonInitColorProperty = BindableProperty.Create(nameof(ButtonInitColor), typeof(Color), typeof(ThumbUpButton), defaultValue: Colors.White);

    public Color ButtonInitColor
    {
        get => (Color)GetValue(ButtonInitColorProperty);
        set => SetValue(ButtonInitColorProperty, value);
    }

    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
    nameof(IconColor),
    typeof(Color),
    typeof(ThumbUpButton),
    defaultValue: Colors.White);

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    private static void OnStatusPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {

            var okButton = bindable as ThumbUpButton;

            var thumbUpButton = okButton.okButton;

            if (okButton.Status == TaskStatusEnum.Ok)
            {
                thumbUpButton.Background = okButton.buttonTappedColor;
                thumbUpButton.Stroke = okButton.buttonTappedColor;
                okButton.IconColor = Colors.White;
            }
            else
            {
                thumbUpButton.Background = Colors.Transparent;
                thumbUpButton.Stroke = okButton.Stroke;
                okButton.IconColor = okButton.ButtonInitColor;//okButton.Stroke;
            }

            okButton.SetBlockedState();
        });
    }

    public static readonly BindableProperty IsBlockedProperty =
        BindableProperty.Create(
            nameof(IsBlocked),
            typeof(bool),
            typeof(ThumbUpButton),
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
        var okButton = bindable as ThumbUpButton;

        okButton.SetBlockedState();
    }


    private void SetBlockedState()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var thumbUpButton = okButton;

            if (Status == TaskStatusEnum.Ok)
                return;

            if (IsBlocked)
            {
                thumbUpButton.Background = ResourceHelper.GetApplicationResource<Color>("LockedButtonBackgroundColor");
                thumbUpButton.Stroke = ResourceHelper.GetApplicationResource<Color>("LockedButtonBackgroundColor");
                IconColor = ResourceHelper.GetApplicationResource<Color>("LockedButtonIconColor");
            }
            else
            {
                thumbUpButton.Background = Colors.Transparent;
                thumbUpButton.Stroke = ButtonInitColor;
                IconColor = ButtonInitColor;
            }
        });
    }
}
