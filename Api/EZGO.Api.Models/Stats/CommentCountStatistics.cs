using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Stats
{
    public class CommentCountStatistics
    {
        public int TotalCount { get; set; }
        public int IsCreatedByMeCount { get; set; }
        public int IsCreatedTodayCount { get; set; }
        public int IsModifiedTodayCount { get; set; }
        public int IsCommentedToday { get; set; }

    }
}
