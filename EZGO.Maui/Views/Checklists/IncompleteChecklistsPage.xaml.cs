using EZGO.Maui.Core.Utils;
using EZGO.Maui.Core.ViewModels.Checklists;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Views.Checklists;

public partial class IncompleteChecklistsPage : ContentPage
{
    public IncompleteChecklistsPage()
    {
        InitializeComponent();

        MessagingCenter.Subscribe<IncompleteChecklistsViewModel>(this, Constants.HideDeletePopup, (viewModel) =>
        {
            DeletePopup.IsOpen = false;
        });
    }

    protected override void OnDisappearing()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MessagingCenter.Unsubscribe<IncompleteChecklistsViewModel>(this, Constants.HideDeletePopup);
        });
        base.OnDisappearing();
    }

    protected override void OnBindingContextChanged()
    {
        IncompleteChecklistsViewModel bindingContext = BindingContext as IncompleteChecklistsViewModel;

        if (bindingContext?.IsFromBookmark ?? false)
            BottomRowGrid.ColumnDefinitions.First().Width = 0;

        base.OnBindingContextChanged();
    }

    void SfButton_Clicked(System.Object sender, System.EventArgs e)
    {
        if (sender is SfButton button)
        {
            var t = button.Parent;
            DeletePopup.ShowRelativeToView(button, Syncfusion.Maui.Popup.PopupRelativePosition.AlignTopLeft);
        }
    }
}