using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// WorkInstructionFilters; WorkInstruction filters for filtering work instruction collections.
    /// </summary>
    public struct WorkInstructionFilters
    {
        public int? SignedById;
        public int? AreaId;
        public FilterAreaTypeEnum? FilterAreaType;
        public ScoreTypeEnum? ScoreType;
        public RoleTypeEnum? RoleType;
        public InstructionTypeEnum? InstructionType;
        public int? Limit;
        public int? Offset;
        public bool? AllowedOnly;
        public DateTime? Timestamp;
        public DateTime? StartTimestamp;
        public DateTime? EndTimestamp;
        public TimespanTypeEnum? TimespanType;
        public int[] TagIds;
        public bool? IncludeAvailableForAllAreas;
        public string FilterText;

        public bool HasFilters()
        {
            return (AreaId.HasValue || SignedById.HasValue || ScoreType.HasValue || RoleType.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue || Timestamp.HasValue || TimespanType.HasValue || StartTimestamp.HasValue || EndTimestamp.HasValue || InstructionType.HasValue || (TagIds != null && TagIds.Length > 0) || IncludeAvailableForAllAreas.HasValue || !string.IsNullOrEmpty(FilterText));
        }
    }
}
