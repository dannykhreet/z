using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    public struct AssessmentFilters
    {
        public int? AreaId;
        public RoleTypeEnum? Role;
        public AssessmentTypeEnum? AssessmentType;
        public int? TemplateId;
        public DateTime? Timestamp;
        public DateTime? StartTimestamp; 
        public DateTime? EndTimestamp; 
        public TimespanTypeEnum? TimespanType;
        public int? Limit;
        public int? Offset;
        public bool? AllowedOnly;
        public int? CompletedForId;
        public int? AssessorId;
        public bool? IsCompleted;
        public int[] TagIds;
        public int[] AssessorIds;
        public string FilterText;

        public bool? SortByModifiedAt;
        public bool? SortByCompletedAt;


        public bool HasFilters()
        {
            return (AreaId.HasValue || Role.HasValue || AssessmentType.HasValue || TemplateId.HasValue || Timestamp.HasValue || StartTimestamp.HasValue || EndTimestamp.HasValue || TimespanType.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue || CompletedForId.HasValue || AssessorId.HasValue || IsCompleted.HasValue || (TagIds != null && TagIds.Length > 0));
        }
    }
}
