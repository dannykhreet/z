using System.Windows.Input;

namespace EZGO.Maui.Controls.ViewCells;

public partial class CompletedInstructionItemViewCell : ViewCell
{
    public static readonly BindableProperty PictureDetailCommandProperty = BindableProperty.Create(nameof(PictureDetailCommand), typeof(ICommand), typeof(CompletedInstructionItemViewCell));

    public ICommand PictureDetailCommand
    {
        get => (ICommand)GetValue(PictureDetailCommandProperty);
        set => SetValue(PictureDetailCommandProperty, value);
    }

    public CompletedInstructionItemViewCell()
    {
        InitializeComponent();
    }
}
