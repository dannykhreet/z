namespace EZGO.Maui.Views.Audits;

public partial class AuditSignPage : ContentPage
{
	public AuditSignPage()
	{
		InitializeComponent();
	}

    void PopupLayout_Opened(System.Object sender, System.EventArgs e)
    {
        if (AutocompleteTemplate != null)
        {
            PopupLayout.ContentTemplate = AutocompleteTemplate;
        }
    }
}
