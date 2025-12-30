using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    public struct MatrixFilters
    {
        public int? AreaId;
        public RoleTypeEnum? Role;
        public DateTime? Timestamp;
        public DateTime? StartTimestamp;
        public DateTime? EndTimestamp;
        public TimespanTypeEnum? TimespanType;
        public int? CreatedById;
        public int? ModifiedById;
        public int? Limit;
        public int? Offset;
        public bool? AllowedOnly;

        public bool HasFilters()
        {
            return (AreaId.HasValue || Role.HasValue || Timestamp.HasValue || StartTimestamp.HasValue || EndTimestamp.HasValue || TimespanType.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue);
        }
    }
}
