namespace EZGO.Maui.Views.Checklists;

public partial class ChecklistSignPage : ContentPage
{
    public ChecklistSignPage()
    {
        InitializeComponent();
    }

    void PopupLayout_Opened(System.Object sender, System.EventArgs e)
    {
        PopupLayout.ContentTemplate = new DataTemplate();
        PopupLayout.ContentTemplate = AutocompleteTemplate;
    }
}
