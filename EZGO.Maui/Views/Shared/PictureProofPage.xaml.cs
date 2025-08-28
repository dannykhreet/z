using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Views.Shared;

public partial class PictureProofPage : ContentPage
{
	public PictureProofPage()
	{
		InitializeComponent();

        if (Settings.IsRightToLeftLanguage)
            PicutureList.FlowDirection = FlowDirection.RightToLeft;
    }
}
