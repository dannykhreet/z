namespace EZGO.Maui.Views.Checklists;

public partial class ChecklistPdfPage : ContentPage
{
    public ChecklistPdfPage()
    {
        InitializeComponent();
        var item = PdfViewer.Toolbars?.GetByName("MoreOptionToolbar");
        if (item != null)
        {
            item.IsVisible = false;
        }
    }
}
