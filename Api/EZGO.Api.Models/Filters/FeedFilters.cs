using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// FeedFilters; Filters for filtering feed requests.
    /// </summary>
    public struct FeedFilters
    {
        public FeedTypeEnum? FeedType;
        public int? Limit;
        public int? Offset;
        public int? FactoryFeedId;
        public int? FeedMessageId;
        public int? UserId;
        public DateTime? Timestamp;

        public bool HasFilters()
        {
            return (FeedType.HasValue || Limit.HasValue || Offset.HasValue || FactoryFeedId.HasValue || UserId.HasValue || Timestamp.HasValue);
        }
    }
}
