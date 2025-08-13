using EZGO.Maui.Core.Models.Users;
using Syncfusion.Maui.Popup;
using System.Collections.ObjectModel;

namespace EZGO.Maui.Controls.Popups
{
    public partial class FeedLikedPopupView : SfPopup
    {
        public static readonly BindableProperty LikedByUsersProperty = BindableProperty.Create(nameof(LikedByUsers), typeof(ObservableCollection<UserProfileModel>), typeof(FeedLikedPopupView));

        public ObservableCollection<UserProfileModel> LikedByUsers
        {
            get => (ObservableCollection<UserProfileModel>)GetValue(LikedByUsersProperty);
            set => SetValue(LikedByUsersProperty, value);
        }

        public FeedLikedPopupView()
        {
            InitializeComponent();
        }
    }
}