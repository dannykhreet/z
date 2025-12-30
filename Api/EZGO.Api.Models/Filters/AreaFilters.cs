using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// AreaFilters; AreaFilters are used within the Controllers to set certain properties for filtering within the managers.
    /// The AreaFilters are used for Areas functionality.
    /// </summary>
    public struct AreaFilters
    {
        public int? MaxLevel;
        public bool? AllowedOnly;

        public bool HasFilters()
        {
            return (MaxLevel.HasValue || AllowedOnly.HasValue);
        }
    }
}
