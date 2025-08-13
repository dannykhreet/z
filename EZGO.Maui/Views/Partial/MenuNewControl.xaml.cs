using EZGO.Maui.Core;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.ViewModels.Menu;
using EZGO.Maui.Views.Home;

namespace EZGO.Maui.Views.Partial;

public partial class MenuNewControl : StackLayout
{
    public MenuNewControl()
    {
        using var scope = App.Container.CreateScope();
        BindingContext = scope.ServiceProvider.GetService<MenuViewModel>();

        InitializeComponent();

        var navigationService = scope.ServiceProvider.GetService<INavigationService>();

        if (navigationService.IsInNavigationStack(typeof(HomePage)))
            StartArrowPulseAnimation();
    }

    private async void StartArrowPulseAnimation()
    {
        await Task.Delay(1000);
        for (int counter = 0; counter <= 1; counter++)
        {
            await ShowMoreImage.RelScaleTo(1, easing: Easing.BounceOut);
            await ShowMoreImage.ScaleTo(1, easing: Easing.BounceIn);
            counter++;
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (StackLayoutMenu.Height > 0)
        {
            var context = BindingContext as MenuViewModel;

            if (!context.MenuManager.IsArrowVisible)
            {
                var currentItemsCount = context.MenuManager.CurrentMenuItems.Count;
                if (MenuItemsListView.ItemSize * currentItemsCount < MenuItemsListView.Height)
                {
                    var heightDifference = MenuItemsListView.Height - (MenuItemsListView.ItemSize * currentItemsCount);
                    var spacing = heightDifference / currentItemsCount;
                    MenuItemsListView.ItemSpacing = new Thickness(0, spacing, 0, 0);
                }
                else
                {
                    MenuItemsListView.ItemSpacing = 0;
                }
                MenuItemsListView.RefreshView();
            }
        }
    }
}
