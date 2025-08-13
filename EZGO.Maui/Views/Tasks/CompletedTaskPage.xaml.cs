using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Views.Tasks;

public partial class CompletedTaskPage : ContentPage
{
    public CompletedTaskPage()
    {
        InitializeComponent();
        if (Settings.IsRightToLeftLanguage)
            ArabicConversion();
    }

    protected override void OnAppearing()
    {
        customPicker.HookEvents();
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        customPicker.Dispose();
        base.OnDisappearing();
    }

    private void ArabicConversion()
    {
        //nextButtonImage.Rotation = 180;
        //previousButtonImage.Rotation = 180;
    }
}
