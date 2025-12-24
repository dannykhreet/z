using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.UI
{
    /// <summary>
    /// MainMenu; MainMenu is used for getting the primary menu items for display within the application. These menu items are based on the tiles in the EZGO Application.
    /// For now these will be more or less static items (not changing depending on settings). But in the future these will be dynamic.
    /// </summary>
    public class MainMenu
    {
        public List<MainMenuItem> MenuItems { get; set; }
        public MainMenu()
        {
            this.MenuItems = new List<MainMenuItem>();
        }
 

    }
}
