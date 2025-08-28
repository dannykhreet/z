using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Views.Bookmarks;

public partial class BookmarkPage : ContentPage, IBookmarkPage
{
    public BookmarkPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        scanner.CameraEnabled = true;
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        scanner.CameraEnabled = false;
        base.OnDisappearing();
    }
}
