using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.ViewModels.Checklists;

namespace EZGO.Maui.Views.Checklists;

public partial class TaskTemplatesPage : ContentPage, IViewResizer
{
    public TaskTemplatesPage()
    {
        InitializeComponent();
    }

    public void RecalculateViewElementsPositions()
    {
        FilterLayout.Padding = new Thickness();
        FilterLayout.VerticalOptions = LayoutOptions.CenterAndExpand;
    }

    /// <summary>
    /// Method that is called when the binding context changes.
    /// </summary>
    protected override void OnBindingContextChanged()
    {
        TaskTemplatesViewModel bindingContext = BindingContext as TaskTemplatesViewModel;

        if (bindingContext?.OpenedFromDeepLink ?? false)
            BottomRowGrid.ColumnDefinitions.First().Width = 0;

        base.OnBindingContextChanged();
    }
}

public partial class StageDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate ItemDataTemplate { get; set; }
    public DataTemplate SignGridItemDataTemplate { get; set; }


    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var taskTemplateModel = item as BasicTaskTemplateModel;
        if (taskTemplateModel == null)
            return null;
        return taskTemplateModel.Id == -1 ? SignGridItemDataTemplate : ItemDataTemplate;
    }
}
