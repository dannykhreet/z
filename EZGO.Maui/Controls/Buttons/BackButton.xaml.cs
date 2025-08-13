using System.Windows.Input;
using EZGO.Maui.Core;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Navigation;

namespace EZGO.Maui.Controls.Buttons;

public partial class BackButton : Button
{
    public BackButton()
    {
        InitializeComponent();
        _cancelCommand = new Command(async () => await NavigateBack());
        backButton.Command = _cancelCommand;
        if (Settings.IsRightToLeftLanguage)
            ArabicConversion();
    }

    private async Task NavigateBack()
    {
        using var scope = App.Container.CreateScope();
        var navigationService = scope.ServiceProvider.GetService<INavigationService>();
        await navigationService.CloseAsync();
    }

    private void ArabicConversion()
    {
        backButton.Rotation = 180;
    }

    private readonly ICommand _cancelCommand;
}
