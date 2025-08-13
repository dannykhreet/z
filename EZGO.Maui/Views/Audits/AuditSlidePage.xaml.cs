using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Views.Audits;

public partial class AuditSlidePage : ContentPage, IViewResizer
{
    public AuditSlidePage()
    {
        InitializeComponent();
        if (Settings.IsRightToLeftLanguage)
            ArabicConversion();
    }

    private void SfButton_Clicked(object sender, EventArgs e)
    {
        ScorePopup.Show();
    }

    private void Score_Clicked(object sender, EventArgs e)
    {
        ScorePopup.IsOpen = false;
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
