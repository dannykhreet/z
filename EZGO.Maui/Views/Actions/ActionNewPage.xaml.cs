using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using Microsoft.Maui.Handlers;

namespace EZGO.Maui.Views.Actions;

public partial class ActionNewPage : ContentPage, IViewResizer
{
    public ActionNewPage()
    {
        InitializeComponent();
        SetLayout();

    }

    private void SetLayout()
    {
        if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
        {
            Tags.IsVisible = true;
            ResourceButtons.Margin = 0;
            actionCommentEditor.HeightRequest = 150;
            actionNameEditor.HeightRequest = 150;
        }
    }

    private void DueDate_Tapped(object sender, System.EventArgs e)
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            DatePicker1.Focus();
        }
#if ANDROID
        var handler = DatePicker1.Handler as IDatePickerHandler;
        handler.PlatformView.PerformClick();
#endif
    }

    private void DatePicker1_OnUnfocused(object sender, FocusEventArgs e)
    {
        IStatusBarService statusBarService = DependencyService.Get<IStatusBarService>();

        statusBarService.HideStatusBar();
    }

    public void RecalculateViewElementsPositions()
    {
        actionCommentEditor.HeightRequest = 90;
        actionNameEditor.HeightRequest = 90;
    }

}
