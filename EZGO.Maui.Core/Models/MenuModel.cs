using System;
namespace EZGO.Maui.Core.Models
{
    public class MenuModel
    {
        public string MenuItemName { get; set; }
        public string MenuIconName { get; set; }

        public MenuModel(string name, string icon)
        {
            this.MenuItemName = name;
            this.MenuIconName = icon;
        }
    }
}
