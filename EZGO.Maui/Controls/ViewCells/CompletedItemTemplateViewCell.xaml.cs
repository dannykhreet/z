using System.Windows.Input;
using EZGO.Maui.Core.Models.Audits;

namespace EZGO.Maui.Controls.ViewCells;

public partial class CompletedItemTemplateViewCell : ViewCell
{
    public CompletedItemTemplateViewCell()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ColorCalculatorProperty = BindableProperty.Create(nameof(ColorCalculator), typeof(IScoreColorCalculator), typeof(CompletedItemTemplateViewCell));

    public IScoreColorCalculator ColorCalculator
    {
        get => (IScoreColorCalculator)GetValue(ColorCalculatorProperty);
        set => SetValue(ColorCalculatorProperty, value);
    }

    public static readonly BindableProperty ActionsCommandProperty = BindableProperty.Create(nameof(ActionsCommand), typeof(ICommand), typeof(CompletedItemTemplateViewCell));

    public ICommand ActionsCommand
    {
        get => (ICommand)GetValue(ActionsCommandProperty);
        set => SetValue(ActionsCommandProperty, value);
    }

    public static readonly BindableProperty TapStatusCommandProperty = BindableProperty.Create(nameof(TapStatusCommand), typeof(ICommand), typeof(CompletedItemTemplateViewCell));

    public ICommand TapStatusCommand
    {
        get => (ICommand)GetValue(TapStatusCommandProperty);
        set => SetValue(TapStatusCommandProperty, value);
    }
}
