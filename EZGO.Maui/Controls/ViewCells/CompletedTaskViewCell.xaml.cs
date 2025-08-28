using System.Windows.Input;

namespace EZGO.Maui.Controls.ViewCells;

public partial class CompletedTaskViewCell : ViewCell
{
    public CompletedTaskViewCell()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ActionsCommandProperty = BindableProperty.Create(nameof(ActionsCommand), typeof(ICommand), typeof(CompletedTaskViewCell));

    public ICommand ActionsCommand
    {
        get => (ICommand)GetValue(ActionsCommandProperty);
        set => SetValue(ActionsCommandProperty, value);
    }

    public static readonly BindableProperty TapStatusCommandProperty = BindableProperty.Create(nameof(TapStatusCommand), typeof(ICommand), typeof(CompletedTaskViewCell));

    public ICommand TapStatusCommand
    {
        get => (ICommand)GetValue(TapStatusCommandProperty);
        set => SetValue(TapStatusCommandProperty, value);
    }
}
