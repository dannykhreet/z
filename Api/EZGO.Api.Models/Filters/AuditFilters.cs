using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// AuditFilters; AuditFilters are used within the Controllers to set certain properties for filtering within the managers.
    /// The AuditFilters are used for Audits and AuditTemplate functionality.
    /// </summary>
    public struct AuditFilters
    {
        public int? SignedById;
        public bool? IsCompleted;
        public int? TemplateId;
        public int? AreaId;
        public FilterAreaTypeEnum? FilterAreaType;
        public ScoreTypeEnum? ScoreType;
        public RoleTypeEnum? RoleType;
        public int? Limit;
        public int? Offset;
        public bool? AllowedOnly;
        public DateTime? Timestamp;
        public DateTime? StartTimestamp;
        public DateTime? EndTimestamp;
        public TimespanTypeEnum? TimespanType;
        public int[] TagIds;

        public string FilterText;
        public List<RoleTypeEnum> Roles;
        public bool? InstructionsAdded;
        public bool? ImagesAdded;

        public bool HasFilters()
        {
            return (AreaId.HasValue || IsCompleted.HasValue || SignedById.HasValue || TemplateId.HasValue || ScoreType.HasValue || RoleType.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue || Timestamp.HasValue || TimespanType.HasValue || StartTimestamp.HasValue || EndTimestamp.HasValue || (TagIds != null && TagIds.Length > 0));
        }
    }
}
