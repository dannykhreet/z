using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Views.Assessments;

public partial class AssessmentsSlidePage : ContentPage
{
    public AssessmentsSlidePage()
    {
        InitializeComponent();
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
    }
}
