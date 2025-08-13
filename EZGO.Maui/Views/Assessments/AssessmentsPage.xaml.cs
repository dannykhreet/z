using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Assessments;

namespace EZGO.Maui.Views.Assessments
{
    public partial class AssessmentsPage : ContentPage
    {
        public AssessmentsPage()
        {
            InitializeComponent();
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
                MessagingCenter.Unsubscribe<AssessmentsViewModel>(this, Constants.ResetParticipantSwipe);
            });
            base.OnDisappearing();
        }

        private void InitializeMessagingCenterSubscriptions()
        {
            MessagingCenter.Subscribe<AssessmentsViewModel>(this, Constants.ResetParticipantSwipe, (viewModel) =>
            {
                ParticipantsListView.ResetSwipeItem();
            });
        }

        void PopupLayout_Opened(System.Object sender, System.EventArgs e)
        {
            PopupLayout.ContentTemplate = new DataTemplate();
            PopupLayout.ContentTemplate = AutocompleteTemplate;
        }
    }
}
