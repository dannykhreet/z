using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using Syncfusion.Maui.Accordion;

namespace EZGO.Maui.Views.Checklists;

public partial class CompletedChecklistsPage : ContentPage, IViewResizer
{
    public CompletedChecklistsPage()
    {
        InitializeComponent();
    }

    public void RecalculateViewElementsPositions()
    {
        checklistDetailList.IsStickyFooter = false;
    }

    void checklistDetailList_QueryItemSize(System.Object sender, Syncfusion.Maui.ListView.QueryItemSizeEventArgs e)
    {
        if (e.DataItem is TasksTaskModel model && e.ItemType == Syncfusion.Maui.ListView.ItemType.Record)
        {
            e.ItemSize = model.Tags?.Count > 0 ? 180 : 115;
            e.Handled = true;
        }
    }

    void AccordionItem_PropertyChanged(System.Object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsExpanded" && sender is AccordionItem accordionItem)
        {
            if (accordionItem.Parent is SfAccordion accordion)
            {
                double expandedSize = 0;
                var stageModel = accordion.BindingContext as StageModel;
                if (stageModel == null)
                    return;

                if (!stageModel.HasNotesOrSignature)
                {
                    return;
                }

                if (!stageModel.HasNotesOrSignature || !accordionItem.IsExpanded)
                {
                    accordion.HeightRequest = 110;
                    accordionItem.Content.HeightRequest = 0;
                    stageModel.ExpandedAccordionHeight = 0;
                    stageModel.IsAccordionExpanded = false;
                    checklistDetaiStagelList.RefreshItem();//(-1);
                    return;
                }
                var accordionGrid = accordionItem.Content as Grid;

                if (stageModel.FirstSignature.SignatureImage != null)
                {
                    expandedSize += 160;
                }

                if (!string.IsNullOrEmpty(stageModel.ShiftNotes))
                {
                    var textMetterService = DependencyService.Get<ITextMeter>();
                    expandedSize += textMetterService.MeasureTextSize(stageModel.ShiftNotes, checklistDetailList.Width - 20, 14, "RobotoRegular").Item2;
                    expandedSize += 20;//padding
                }

                expandedSize = Math.Ceiling(expandedSize);

                accordion.HeightRequest = 110 + expandedSize;
                accordionGrid.HeightRequest = expandedSize;
                stageModel.ExpandedAccordionHeight = (int)expandedSize;
                stageModel.IsAccordionExpanded = true;
                checklistDetaiStagelList.RefreshItem();//(-1);
            }
        }
    }

    void checklistDetaiStagelList_QueryItemSize(System.Object sender, Syncfusion.Maui.ListView.QueryItemSizeEventArgs e)
    {
        if (e.DataItem is StageModel stage)
        {
            int itemSize = 0;
            if (stage.IsHeaderVisible)
                itemSize += 50 + 110;
            if (stage.ContainsTags)
                itemSize += 75;
            if (stage.Tasks != null)
            {
                foreach (var task in stage.Tasks)
                {
                    itemSize += task.Tags?.Count > 0 ? 180 : 115;
                    itemSize += 6;
                }
            }
            itemSize += stage.ExpandedAccordionHeight;

            itemSize += 20;

            e.ItemSize = itemSize;
            e.Handled = true;
        }
    }
}
