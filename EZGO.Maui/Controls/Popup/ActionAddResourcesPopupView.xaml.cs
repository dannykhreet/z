using Syncfusion.Maui.Popup;


namespace EZGO.Maui.Controls.Popup
{
	public partial class ActionAddResourcesPopupView : SfPopup
	{
		public ActionAddResourcesPopupView()
		{
			InitializeComponent();
		}


		void SfPopup_Opened(System.Object sender, System.EventArgs e)
		{
			AddResources.ContentTemplate = new DataTemplate();
			AddResources.ContentTemplate = AutocompleteTemplate;
		}
	}
}