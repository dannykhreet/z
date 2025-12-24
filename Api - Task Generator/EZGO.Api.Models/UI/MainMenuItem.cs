using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.UI
{
    /// <summary>
    /// MainMenuItem; For use with <see cref="MainMenu">MainMenu</see>.
    /// Every menu item in the MenuItemCollection within MainMenu will be referencing a tile.
    /// The statistics are generated or calculated based on a database query.
    /// This will be done by external method or functionality when filling the MainMenu.
    /// </summary>
    public class MainMenuItem
    {
        public string Icon { get; set; }
        public string Picture { get; set; }
        public MenuTypeEnum MenuType { get; set; }
        public string Title {
            get {
                return MenuType.ToString();
            }
        }
        public List<StatsItem> Statistics { get; set; }

        public MainMenuItem()
        {
            Statistics = new List<StatsItem>();
        }

    }
}
