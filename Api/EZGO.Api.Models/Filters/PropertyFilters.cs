using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// PropertyFilters; Filtering certain property requests.
    /// </summary>
    public struct PropertyFilters
    {
        public int? PropertyGroupId;
        public int[] PropertyGroupIds;
        public int? Limit;
        public int? Offset;

        public bool HasFilters()
        {
            return (PropertyGroupId.HasValue || Limit.HasValue || Offset.HasValue);
        }
    }
}
