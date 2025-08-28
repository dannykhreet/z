using ExCSS;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Views.Checklists;

public partial class ChecklistSlidePage : ContentPage, IViewResizer
{
    public ChecklistSlidePage()
    {
        InitializeComponent();
        if (Settings.IsRightToLeftLanguage)
            ArabicConversion();
    }

    public void RecalculateViewElementsPositions()
    {
        MainGrid.RowDefinitions.First().Height = 60;
        MainGrid.RowDefinitions.Last().Height = 110;
        CoverFlow.Margin = new Thickness(0, 5, 0, 0);
        Buttons.Margin = new Thickness(25, 10, 0, 10);
    }

    private void ArabicConversion()
    {
        Buttons.Margin = new Thickness(0, 5, 125, 45);
    }
}
