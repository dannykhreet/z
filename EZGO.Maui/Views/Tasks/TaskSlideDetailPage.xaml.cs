using MediaManager;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Views.Tasks;

public partial class TaskSlideDetailPage : ContentPage
{
    public TaskSlideDetailPage()
    {
        InitializeComponent();
        if (Settings.IsRightToLeftLanguage)
            BackButton.Rotation = 180;
    }

    private async void Page_Disappearing(object sender, EventArgs e)
    {
        await CrossMediaManager.Current.Pause();
    }
}
