using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// ChecklistFilters; ChecklistFilters are used within the Controllers to set certain properties for filtering within the managers.
    /// The ChecklistFilters are used for Checklists and ChecklistTemplates functionality.
    /// </summary>
    public struct ChecklistFilters
    {
        public int? AreaId;
        public FilterAreaTypeEnum? FilterAreaType;
        public bool? IsCompleted;
        public RoleTypeEnum? RoleType;
        public int? SignedById;
        public int? TemplateId;
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

        public int? TaskId;

        public bool? SortByModifiedAt;

        public bool HasFilters()
        {
            return (AreaId.HasValue || IsCompleted.HasValue || RoleType.HasValue || SignedById.HasValue || TemplateId.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue || Timestamp.HasValue || TimespanType.HasValue || StartTimestamp.HasValue || EndTimestamp.HasValue || (TagIds != null && TagIds.Length > 0));
        }
    }
}
