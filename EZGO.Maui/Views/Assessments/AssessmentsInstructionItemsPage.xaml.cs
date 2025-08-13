using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Assessments;

namespace EZGO.Maui.Views.Assessments;

public partial class AssessmentsInstructionItemsPage : ContentPage
{
    public AssessmentsInstructionItemsPage()
    {
        InitializeComponent();
        if (Settings.IsRightToLeftLanguage)
            ArabicConversion();
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
            MessagingCenter.Unsubscribe<AssessmentInstructionItemsViewModel>(this, Constants.ScorePopupMessage);
            MessagingCenter.Unsubscribe<AssessmentInstructionItemsViewModel>(this, Constants.HideScorePopupMessage);
        });
        base.OnDisappearing();
    }

    private void InitializeMessagingCenterSubscriptions()
    {
        MessagingCenter.Subscribe<AssessmentInstructionItemsViewModel>(this, Constants.ScorePopupMessage, (viewModel) =>
        {
            ScorePopup.IsOpen = true;
        });

        MessagingCenter.Subscribe<AssessmentInstructionItemsViewModel>(this, Constants.HideScorePopupMessage, (viewModel) =>
        {
            ScorePopup.IsOpen = false;
        });
    }

    private void ArabicConversion()
    {
        leftButton.Rotation = 180;
        rightButton.Rotation = 180;
    }

}
