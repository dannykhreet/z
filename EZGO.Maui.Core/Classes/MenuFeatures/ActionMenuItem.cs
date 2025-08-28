using System;
using System.Linq;
using Autofac;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Actions;
using EZGO.Maui.Core.Services.Data;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.MenuFeatures
{
    public class ActionMenuItem : NotifyPropertyChanged, IMenuItem
    {
        public string Name { get; private set; }
        public string NameKey { get; set; }
        public ImageSource SelectedImage { get; set; }
        public MenuLocation MenuLocation { get; set; }
        public string BadgeText { get; set; }
        public Color SelectedColor { get; set; } = Colors.White;


        public ActionMenuItem(string nameKey, MenuLocation menuLocation, ImageSource selectedImage = null)
        {
            NameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            SelectedImage = selectedImage;
            MenuLocation = menuLocation;

            SetUserActions();

            SetTranslatedName();

            MessagingCenter.Subscribe<SyncService, int>(this, Constants.MyActionsChanged, (service, value) =>
            {
                SetUserActions(value);
            });
        }

        private async void SetUserActions(int? value = null)
        {
            if (value.HasValue)
            {
                BadgeText = value.Value.ToString();
            }
            else
            {
                using var scope = App.Container.CreateScope();
                var actionService = scope.ServiceProvider.GetService<IActionsService>();
                var actionCount = await actionService.GetActionsCount();
                BadgeText = (actionCount.IsOverdueCount + actionCount.IsUnresolvedCount).ToString();
            }
        }

        public void SetTranslatedName()
        {
            Name = TranslateExtension.GetValueFromDictionary(NameKey);
        }
    }
}
