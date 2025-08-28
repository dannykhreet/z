using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Controls.ViewCells;

public partial class CompletedTemplateViewCell : ViewCell
{
    public List<Brush> CustomBrushes { get; set; }
    private Color RedColor => ResourceHelper.GetApplicationResource<Color>("RedColor");
    private Color GreenColor => ResourceHelper.GetApplicationResource<Color>("GreenColor");
    private Color SkippedColor => ResourceHelper.GetApplicationResource<Color>("SkippedColor");

    public CompletedTemplateViewCell()
    {
        CustomBrushes = new List<Brush>
        {
            GreenColor,
            RedColor,
            SkippedColor,
        };

        InitializeComponent();
    }
}
