using EZGO.Maui.Classes;

namespace EZGO.Maui.Controls.CustomLabels;

public class TitleLabel : Label
{
    public TitleLabel()
    {
        VerticalTextAlignment = TextAlignment.Center;
        //HorizontalOptions = LayoutOptions.CenterAndExpand;
        //VerticalOptions = LayoutOptions.CenterAndExpand;
        FontSize = 26;
        HorizontalTextAlignment = TextAlignment.Center;
        TextColor = Color.FromArgb("212121");
        //ResourceHelper.GetValueFromResources<string>("RobotoLight");
    }
}
