using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    /// <summary>
    /// ActionCommentViewedStatsItem; used for internal statistics to get a number of different comment counts with a certain user.
    /// This is an internal stats items (used in some collection). Not used for general statistic reporting or functionality related to reporting.
    /// </summary>
    public class ActionCommentViewedStatsItem
    {
        public int ActionId { get; set; }
        public int CommentsViewedNr { get; set; }
        public int CommentsTotalNr { get; set; }
        public int CommentsNotViewedNr { get; set; }
    }
}
