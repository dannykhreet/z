using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.UI
{
    /// <summary>
    /// SideMenu; SideMenu is used for getting the side menu items for display within the application. These menu items are based on the side bar menu in the EZGO Application.
    /// For now these will be more or less static items (not changing depending on settings). But in the future these will be dynamic.
    /// </summary>
    public class SideMenu
    {
        public List<SideMenuItem> MenuItems { get; set; }
        public SideMenu()
        {
            this.MenuItems = new List<SideMenuItem>();
        }
    }
}
