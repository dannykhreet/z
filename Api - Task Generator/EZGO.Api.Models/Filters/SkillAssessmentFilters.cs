using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// SkillAssessmentFilters; Filtering Skills/Assessment collections.
    /// </summary>
    public struct SkillAssessmentFilters
    {
        public int? AreaId;
        public FilterAreaTypeEnum? FilterAreaType;
        public RoleTypeEnum? RoleType;
        public int? SignedById;
        public int? Limit;
        public int? Offset;
        public bool? AllowedOnly;
        public DateTime? Timestamp;
        public DateTime? StartTimestamp;
        public DateTime? EndTimestamp;
        public TimespanTypeEnum? TimespanType;

        public bool HasFilters()
        {
            return (AreaId.HasValue || RoleType.HasValue || SignedById.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue || Timestamp.HasValue || TimespanType.HasValue || StartTimestamp.HasValue || EndTimestamp.HasValue);
        }
    }
}
