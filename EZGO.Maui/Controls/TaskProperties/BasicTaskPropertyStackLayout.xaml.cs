using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Controls.TaskProperties;

public partial class BasicTaskPropertyStackLayout : StackLayout
{
    public BasicTaskPropertyStackLayout()
    {
        InitializeComponent();

        if (DeviceSettings.ScreenDencity < 8)
            RecalculateViewElementsPositions();
    }

    private void RecalculateViewElementsPositions()
    {
        DisplayTitleLabel.FontSize = 10;
        PrimaryValueLabel.FontSize = 14;
        SecondaryValueLabel.FontSize = 14;
        DisplayFooterLabel.FontSize = 10;
    }
}
