#if ANDROID
using Android.Views;
using Microsoft.Maui.Platform;
#endif
using System.Windows.Input;
using EZGO.Maui.Core.Models.Feed;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popups
{
    public partial class FeedAddCommentPopupView : SfPopup
    {
        public static readonly BindableProperty NewPostProperty = BindableProperty.Create(nameof(NewPost), typeof(AddFeedItemModel), typeof(FeedAddCommentPopupView));

        public AddFeedItemModel NewPost
        {
            get => (AddFeedItemModel)GetValue(NewPostProperty);
            set => SetValue(NewPostProperty, value);
        }

        public static readonly BindableProperty SubmitButtonCommandProperty = BindableProperty.Create(nameof(SubmitButtonCommand), typeof(ICommand), typeof(FeedAddCommentPopupView));

        public ICommand SubmitButtonCommand
        {
            get => (ICommand)GetValue(SubmitButtonCommandProperty);
            set
            {
                SetValue(SubmitButtonCommandProperty, value);
                OnPropertyChanged();
            }
        }

        public FeedAddCommentPopupView()
        {
            InitializeComponent();
        }

        private void Page_Opened(object sender, EventArgs e)
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity != null)
            {
                activity.Window.SetSoftInputMode(SoftInput.AdjustNothing);
            }
#endif
        }

        private void Page_Closed(object sender, EventArgs e)
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity != null)
            {
                activity.Window.SetSoftInputMode(SoftInput.AdjustResize);
            }
#endif
        }
    }
}
