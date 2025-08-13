using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.MenuFeatures
{
    public class MenuItem : NotifyPropertyChanged, IMenuItem
    {
        public MenuItem(string nameKey, MenuLocation menuLocation, ImageSource selectedImage = null)
        {
            NameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            SelectedImage = selectedImage;
            MenuLocation = menuLocation;
            SetTranslatedName();
        }

        public string NameKey { get; set; }
        public string Name { get; private set; }
        public ImageSource SelectedImage { get; set; }
        public MenuLocation MenuLocation { get; set; }
        public string BadgeText { get; set; } = null;
        public Color SelectedColor { get; set; } = Colors.White;


        public void SetTranslatedName()
        {
            Name = TranslateExtension.GetValueFromDictionary(NameKey);
        }
    }
}
