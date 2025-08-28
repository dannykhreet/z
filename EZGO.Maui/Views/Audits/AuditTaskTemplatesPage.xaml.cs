using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Audits;

namespace EZGO.Maui.Views.Audits;

public partial class AuditTaskTemplatesPage : ContentPage
{

    public AuditTaskTemplatesPage()
    {
        InitializeComponent();
    }

    /// <summary>Method that is called when the binding context changes.</summary>
    /// <remarks>To be added.</remarks>
    protected override void OnBindingContextChanged()
    {
        AuditTaskTemplatesViewModel bindingContext = BindingContext as AuditTaskTemplatesViewModel;

        if (bindingContext?.OpenedFromDeepLink ?? false)
            BottomRowGrid.ColumnDefinitions.First().Width = 0;

        base.OnBindingContextChanged();
    }

    protected override void OnAppearing()
    {
        InitializeMessagingCenterSubscriptions();
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MessagingCenter.Unsubscribe<AuditTaskTemplatesViewModel>(this, Constants.ScorePopupMessage);
            MessagingCenter.Unsubscribe<AuditTaskTemplatesViewModel>(this, Constants.HideScorePopupMessage);
        });

        base.OnDisappearing();
    }

    private void InitializeMessagingCenterSubscriptions()
    {
        MessagingCenter.Subscribe<AuditTaskTemplatesViewModel>(this, Constants.ScorePopupMessage, (viewModel) =>
        {
            ScorePopup.IsOpen = true;
            //ScorePopup.ShowAtTouchPoint();
        });

        MessagingCenter.Subscribe<AuditTaskTemplatesViewModel>(this, Constants.HideScorePopupMessage, (viewModel) =>
        {
            ScorePopup.IsOpen = false;
        });
    }
}
