using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    /// <summary>
    /// StatsItem; Used for getting certain statistics in menus and like-minded functionalities.
    /// This is an internal stats items (used in some collection). Not used for general statistic reporting or functionality related to reporting.
    /// </summary>
    public class StatsItem
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public int Statistic { get; set; }
    }
}
