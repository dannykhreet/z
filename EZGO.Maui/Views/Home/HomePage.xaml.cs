using EZGO.Maui.Core.Classes;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Views.Home;

public partial class HomePage : ContentPage
{
    private double pageWidth;
    private double pageHeight;
    private List<SfButton> buttons = new List<SfButton>();
    private const int _numberOfItemsInRow = 3;

    public HomePage()
    {
        InitializeComponent();
        SetButtons();
    }

    private void SetButtons()
    {
        if (CompanyFeatures.CompanyFeatSettings.ChecklistsEnabled)
            buttons.Add(checklistButton);
        if (CompanyFeatures.CompanyFeatSettings.TasksEnabled)
            buttons.Add(tasksButton);
        if (CompanyFeatures.CompanyFeatSettings.AuditsEnabled)
            buttons.Add(auditsButton);
        if (CompanyFeatures.CompanyFeatSettings.ActionsEnabled)
            buttons.Add(actionsButtonButton);
        if (CompanyFeatures.CompanyFeatSettings.ReportsEnabled)
            buttons.Add(reportsButton);
        if (CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled)
            buttons.Add(instructionsButton);
        if (CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled &&
            (UserSettings.RoleType == Api.Models.Enumerations.RoleTypeEnum.Manager || UserSettings.RoleType == Api.Models.Enumerations.RoleTypeEnum.ShiftLeader))
            buttons.Add(assessmentsButton);
        if (CompanyFeatures.CompanyFeatSettings.FactoryFeedEnabled)
            buttons.Add(feedButton);
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
        if (width != pageWidth || height != pageHeight)
        {
            pageWidth = width;
            pageHeight = height;

            pageContent.RowDefinitions.Clear();
            pageContent.ColumnDefinitions.Clear();
            header.HeightRequest = 80;

            double itemWidth;
            double itemHeight;

            if (width > height)
            {
                pageContent.WidthRequest = pageWidth;
                pageContent.HeightRequest = pageHeight - header.HeightRequest;
                itemWidth = (pageContent.WidthRequest - 80) / 3;
                itemHeight = (pageContent.HeightRequest - 60) / 2;
                pageContent.ColumnDefinitions.Add(new ColumnDefinition { Width = itemWidth, });
                pageContent.ColumnDefinitions.Add(new ColumnDefinition { Width = itemWidth, });
                pageContent.ColumnDefinitions.Add(new ColumnDefinition { Width = itemWidth, });

                double numberOfRows = (double)buttons.Count / _numberOfItemsInRow;

                for (int i = 0; i < Math.Ceiling(numberOfRows); i++)
                {
                    pageContent.RowDefinitions.Add(new RowDefinition { Height = itemHeight, });
                }

                int row = 0;
                int column = 0;

                foreach (var button in buttons)
                {
                    if (button == actionsButtonButton)
                    {
                        Grid.SetColumn(actionsButton, column);
                        Grid.SetRow(actionsButton, row);
                        actionsButton.IsVisible = true;
                    }
                    else
                    {
                        Grid.SetColumn(button, column);
                        Grid.SetRow(button, row);
                        button.IsVisible = true;
                    }
                    column++;
                    if (column >= _numberOfItemsInRow)
                    {
                        row++;
                        column = 0;
                    }
                }

                var rowDefinitionsCount = pageContent.RowDefinitions.Count;
                pageContent.HeightRequest = rowDefinitionsCount * itemHeight + (rowDefinitionsCount * 20);

                if (pageContent.HeightRequest > pageHeight - header.HeightRequest)
                {
                    ShowMoreImage.IsVisible = true;
                    StartArrowPulseAnimation();
                }
                else
                    ShowMoreImage.IsVisible = false;
            }
            else
            {
                pageContent.WidthRequest = width;
                pageContent.HeightRequest = height - header.Height;
                itemWidth = (pageContent.WidthRequest - 60) / 2;
                itemHeight = (pageContent.HeightRequest - 80) / 3;
                pageContent.RowDefinitions.Add(new RowDefinition { Height = itemHeight, });
                pageContent.RowDefinitions.Add(new RowDefinition { Height = itemHeight, });
                pageContent.RowDefinitions.Add(new RowDefinition { Height = itemHeight, });
                pageContent.RowDefinitions.Add(new RowDefinition { Height = itemHeight, });
                pageContent.ColumnDefinitions.Add(new ColumnDefinition { Width = itemWidth, });
                pageContent.ColumnDefinitions.Add(new ColumnDefinition { Width = itemWidth, });
                Grid.SetColumn(checklistButton, 0);
                Grid.SetRow(checklistButton, 0);
                Grid.SetColumn(tasksButton, 1);
                Grid.SetRow(tasksButton, 0);
                Grid.SetColumn(auditsButton, 0);
                Grid.SetRow(auditsButton, 1);
                Grid.SetColumn(reportsButton, 1);
                Grid.SetRow(reportsButton, 1);
                Grid.SetColumn(actionsButton, 0);
                Grid.SetRow(actionsButton, 2);
                Grid.SetColumn(instructionsButton, 1);
                Grid.SetRow(instructionsButton, 2);
                Grid.SetColumn(assessmentsButton, 0);
                Grid.SetRow(assessmentsButton, 3);
                Grid.SetColumn(feedButton, 1);
                Grid.SetRow(feedButton, 3);
            }
        }
    }

    void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        var yValue = pageContent.RowDefinitions.First().Height.Value + 20;
        ScrollViewContent.ScrollToAsync(0, yValue, true);
    }

    protected override void OnDisappearing()
    {
        buttons = null;
        base.OnDisappearing();
    }
}
