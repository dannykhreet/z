using System.Windows.Input;
using EZGO.Maui.Core.Models.Feed;

namespace EZGO.Maui.Controls.Popups.Content
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedAddCommentPopupContent : ContentView
    {
        public static readonly BindableProperty NewItemProperty = BindableProperty.Create(nameof(NewItem), typeof(AddFeedItemModel), typeof(FeedAddCommentPopupContent));

        public AddFeedItemModel NewItem
        {
            get => (AddFeedItemModel)GetValue(NewItemProperty);
            set => SetValue(NewItemProperty, value);
        }

        public static readonly BindableProperty SubmitButtonCommandProperty = BindableProperty.Create(nameof(SubmitButtonCommand), typeof(ICommand), typeof(FeedAddCommentPopupContent));

        public ICommand SubmitButtonCommand
        {
            get => (ICommand)GetValue(SubmitButtonCommandProperty);
            set
            {
                SetValue(SubmitButtonCommandProperty, value);
                OnPropertyChanged();
            }
        }

        public FeedAddCommentPopupContent()
        {
            InitializeComponent();
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            titleEntry.Unfocus();
            descEntry.Unfocus();
        }
    }
}

