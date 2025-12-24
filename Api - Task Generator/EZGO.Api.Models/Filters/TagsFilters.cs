using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// UserFilters; UserFilters are used within the Controllers to set certain properties for filtering within the managers.
    /// The UserFilters are used for UserProfile functionality.
    /// </summary>
    public struct TagsFilters
    {
        public int? AreaId { get; set; }
        public string Type { get; set; }

        public bool HasFilters()
        {
            return (AreaId.HasValue || !string.IsNullOrEmpty(Type));
        }
    }
}
