using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Views.Tasks;

public partial class TaskPage : ContentPage, IViewResizer
{
    public TaskPage()
    {
        InitializeComponent();
    }

    public void RecalculateViewElementsPositions()
    {
        PullableGrid.Padding = 5;
        var newMargin = new Thickness(0, 0, 0, 5);
        ShiftBars.GaugeMargin = newMargin;
        TodayBars.GaugeMargin = newMargin;
        WeekBars.GaugeMargin = newMargin;
        OverdueBars.GaugeMargin = newMargin;
    }
}
