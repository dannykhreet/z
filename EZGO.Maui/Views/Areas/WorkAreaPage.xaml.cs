using System.ComponentModel;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.ViewModels;

namespace EZGO.Maui.Views.Areas;

public partial class WorkAreaPage : ContentPage
{
    private WorkAreaViewModel VM => BindingContext as WorkAreaViewModel;

    public WorkAreaPage()
    {
        InitializeComponent();
        InitializePageElements(DeviceSettings.ScreenWidth, DeviceSettings.ScreenHeight);

        Header.Padding = new Thickness(8, 5);
    }

    private double width;
    private double height;

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (this.width != width || this.height != height)
        {
            this.width = width;
            this.height = height;
            InitializePageElements(width, height);
        }
    }

    private void InitializePageElements(double width, double height)
    {
        WorkAreaContent.RowDefinitions.Clear();

        if (width > height)
        {
            WorkAreaContent.RowDefinitions.Add(new RowDefinition { Height = 80 });
            WorkAreaContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            WorkAreaContent.RowDefinitions.Add(new RowDefinition { Height = 80 });

            Header.HeightRequest = 80;
        }
        else
        {
            var screenHeight = DeviceSettings.ScreenHeight;
            var headerHeight = screenHeight * 0.1;
            WorkAreaContent.RowDefinitions.Add(new RowDefinition { Height = headerHeight * 2 });
            WorkAreaContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            WorkAreaContent.RowDefinitions.Add(new RowDefinition { Height = headerHeight });

            var screenWidth = DeviceSettings.ScreenWidth;
            var upperRow = screenWidth / 2;
        }

        Grid.SetRow(Header, 0);
    }
}
