using System;
using EZGO.Maui.Classes;
using EZGO.Maui.Core.Extensions;
using TranslateExtension = EZGO.Maui.Core.Extensions.TranslateExtension;

namespace EZGO.Maui.Controls
{
    public class EmptyView : Grid
    {
        public EmptyView()
        {
            var content = new StackLayout();
            content.HorizontalOptions = LayoutOptions.Center;
            content.VerticalOptions = LayoutOptions.Center;

            BackgroundColor = Colors.White;

            content.Children.Add(new Image
            {
                Source = "nolcn.png"
            });

            content.Children.Add(new Label
            {
                Text = TranslateExtension.GetValueFromDictionary("EMPTY_SCREEN_TEXT"),
                FontSize = 22
            });

            Children.Add(content);
        }
    }
}

