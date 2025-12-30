using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// CommentFilters; Filters for filtering comment collections.
    /// </summary>
    public struct CommentFilters
    {
        public int? UserId;
        public int? TaskId;
        public int? TaskTemplateId;
        public int? Limit;
        public int? Offset;
        public DateTime? Timestamp;
        public int[] TagIds;

        public string FilterText { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public bool HasFilters()
        {
            return (UserId.HasValue || TaskId.HasValue || Limit.HasValue || Offset.HasValue || Timestamp.HasValue || (TagIds != null && TagIds.Length > 0));
        }
    }
}
