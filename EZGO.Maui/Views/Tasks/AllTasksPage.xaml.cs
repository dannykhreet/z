using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Views.Tasks;

public partial class AllTasksPage : ContentPage, IViewResizer
{
    public AllTasksPage()
    {
        InitializeComponent();
    }

    public bool IsNavigationEnabled { get; set; } = true;

    public void RecalculateViewElementsPositions()
    {
        IsNavigationEnabled = false;
    }
}
