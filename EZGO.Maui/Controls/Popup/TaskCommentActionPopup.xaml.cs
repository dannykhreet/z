using System.Windows.Input;
using EZGO.Maui.Core.Extensions;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class TaskCommentActionPopup : SfPopup
{
    public static readonly BindableProperty NewActionCommandProperty = BindableProperty.Create(
        nameof(NewActionCommand),
        typeof(ICommand),
        typeof(TaskCommentActionPopup));

    public static readonly BindableProperty NewCommentCommandProperty = BindableProperty.Create(
        nameof(NewCommentCommand),
        typeof(ICommand),
        typeof(TaskCommentActionPopup));

    public static readonly BindableProperty CenterOnScreenProperty = BindableProperty.Create(
        nameof(CenterOnScreen),
        typeof(bool),
        typeof(TaskCommentActionPopup),
        false,
        propertyChanged: OnCenterOnScreenChanged);

    public ICommand NewActionCommand
    {
        get => (ICommand)GetValue(NewActionCommandProperty);
        set
        {
            SetValue(NewActionCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand NewCommentCommand
    {
        get => (ICommand)GetValue(NewCommentCommandProperty);
        set
        {
            SetValue(NewCommentCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public bool CenterOnScreen
    {
        get => (bool)GetValue(CenterOnScreenProperty);
        set => SetValue(CenterOnScreenProperty, value);
    }

    public string ActionButtonText { get => TranslateExtension.GetValueFromDictionary("POPUP_ADD_ACTION"); }
    public string CommentButtonText { get => TranslateExtension.GetValueFromDictionary("POPUP_ADD_COMMENT"); }

    public int PopupWidth
    {
        get
        {
            if (!ActionButtonText.IsNullOrEmpty() && !CommentButtonText.IsNullOrEmpty())
            {
                var baseLength = ActionButtonText.Length > CommentButtonText.Length ? ActionButtonText.Length : CommentButtonText.Length;
                return baseLength * (baseLength > 20 ? 10 : 13) * 2;
            }
            return 400;
        }
    }

    public TaskCommentActionPopup()
    {
        InitializeComponent();
        
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        if (CenterOnScreen)
        {
            CenterPopup();
        }
    }

    private static void OnCenterOnScreenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TaskCommentActionPopup popup && (bool)newValue)
        {
            popup.CenterPopup();
        }
    }

    private void CenterPopup()
    {
        if (!CenterOnScreen) return;
        
        try
        {
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            var screenWidth = displayInfo.Width / displayInfo.Density;
            var screenHeight = displayInfo.Height / displayInfo.Density;

            var popupWidth = PopupWidth > 0 ? PopupWidth : 400;
            var popupHeight = HeightRequest > 0 ? HeightRequest : 85;

            StartX = (int)((screenWidth - popupWidth) / 2);
            StartY = (int)((screenHeight - popupHeight) / 2);
            
            if (StartX < 0) StartX = 10;
            if (StartY < 0) StartY = 10;
            if (StartX + popupWidth > screenWidth) StartX = (int)(screenWidth - popupWidth - 10);
            if (StartY + popupHeight > screenHeight) StartY = (int)(screenHeight - popupHeight - 10);
        }
        catch (Exception ex)
        {
            StartX = 0;
            StartY = 0;
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (CenterOnScreen)
        {
            CenterPopup();
        }
    }
}