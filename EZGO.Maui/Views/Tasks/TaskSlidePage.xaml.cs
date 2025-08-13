using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Views.Tasks;

public partial class TaskSlidePage : ContentPage, IViewResizer
{
    public TaskSlidePage()
    {
        InitializeComponent();
    }

    public bool IsEditButtonVisible { get; set; } = true;

    public void RecalculateViewElementsPositions()
    {
        IsEditButtonVisible = false;
        MainGrid.RowDefinitions.First().Height = 60;
        MainGrid.RowDefinitions.Last().Height = 110;
        CoverFlow.Margin = new Thickness(0, 5, 0, 0);
        Buttons.Margin = new Thickness(25, 10, 0, 10);
    }
} 
