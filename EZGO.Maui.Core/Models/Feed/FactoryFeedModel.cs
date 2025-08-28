using System;
using EZGO.Api.Models.Feed;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Feed
{
    public class FactoryFeedModel : FactoryFeed
    {
        public new List<FeedMessageItemModel> Items { get; set; }
    }
}

