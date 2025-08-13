using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Views.Audits;

public partial class CompletedAuditPage : ContentPage, IViewResizer
{
    public CompletedAuditPage()
    {
        InitializeComponent();
    }

    public void RecalculateViewElementsPositions()
    {
        CompletedAuditList.IsStickyFooter = false;
    }

    void CompletedAuditList_QueryItemSize(System.Object sender, Syncfusion.Maui.ListView.QueryItemSizeEventArgs e)
    {
        if (e.DataItem is TasksTaskModel model && e.ItemType == Syncfusion.Maui.ListView.ItemType.Record)
        {
            e.ItemSize = model.Tags?.Count > 0 ? 150 : 90;
            e.Handled = true;
        }
    }
}
