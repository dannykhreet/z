using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.UI
{
    /// <summary>
    /// SideMenuItem; For use with <see cref="SideMenu">SideMenu</see>.
    /// Every menu item in the SideMenuItemCollection within SideMenu will be referencing a menu button in the side menu.
    /// </summary>
    public class SideMenuItem
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public MenuTypeEnum MenuType { get; set; }
        public string Icon { get; set; }
    }
}
