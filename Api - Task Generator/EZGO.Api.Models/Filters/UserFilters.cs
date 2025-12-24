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
    public struct UserFilters
    {
        public bool? IsStaff { get; set; }
        public bool? IsSuperUser { get; set; }
        public RoleTypeEnum? RoleType { get; set; }

        public bool HasFilters()
        {
            return (IsStaff.HasValue || IsSuperUser.HasValue || RoleType.HasValue);
        }
    }
}
