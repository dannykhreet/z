using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    public struct UserGroupFilters
    {
        public GroupTypeEnum? UserGroupType;
        public int? Limit;
        public int? Offset;

        public bool HasFilters()
        {
            return (UserGroupType.HasValue || Limit.HasValue || Offset.HasValue);
        }
    }
}
