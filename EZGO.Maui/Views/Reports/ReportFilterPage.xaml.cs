namespace EZGO.Maui.Views.Reports
{
    public partial class ReportFilterPage : ContentPage
    {
        public ReportFilterPage()
        {
            InitializeComponent();
        }

        public void RecalculateViewElementsPositions()
        {
            Q1Grid.RowDefinitions[0].Height = 30;
            HeaderButtonsStackLayout.StyleClass.Clear();
            HeaderButtonsStackLayout.StyleClass.Add("p-15");
        }
    }
}
