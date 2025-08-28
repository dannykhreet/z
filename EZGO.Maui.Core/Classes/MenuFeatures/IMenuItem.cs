using EZGO.Maui.Core.Enumerations;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.MenuFeatures
{
    public interface IMenuItem
    {
        public string Name { get; }
        public ImageSource SelectedImage { get; set; }
        public MenuLocation MenuLocation { get; set; }
        public string BadgeText { get; set; }
        public Color SelectedColor { get; set; }
        public void SetTranslatedName();
    }
}
