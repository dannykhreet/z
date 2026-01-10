using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Filters
{
    /// <summary>
    /// ActionFilters; ActionFilters are used within the Controllers to set certain properties for filtering within the managers.
    /// The ActionFilters are used for Actions and ActionComments functionality.
    /// </summary>
    public struct ActionFilters
    {
        public string FilterText;
        public int? ActionId;
        public int? UserId;
        public int? CreatedById;
        public int? AssignedAreaId;
        public int[] AssignedAreaIds;
        public FilterAreaTypeEnum? AssignedAreaType;
        public int? TaskId;
        public int? AssignedUserId;
        public int[] AssignedUserIds;
        public int? TaskTemplateId;
        public bool? IsResolved;
        public bool? IsOverdue;
        public bool? IsUnresolved;
        public int? Limit;
        public int? Offset;
        public bool? AllowedOnly;
        public DateTime? Timestamp;
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public int[] TagIds;

        public bool? HasUnviewedComments { get; set; }

        public int? CreatedByOrAssignedTo { get; set; }
        /// <summary>
        /// User id. Return all actions assigned to this user or assigned to an area this user is allowed access to.
        /// Used for the 'assigned to me' filter
        /// </summary>
        public int? AssignedToMeUserId { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public DateTime? ResolvedFrom { get; set; }
        public DateTime? ResolvedTo { get; set; }

        public DateTime? OverdueFrom { get; set; }
        public DateTime? OverdueTo { get; set; }

        public DateTime? ResolvedCutoffDate { get; set; }

        public int? ChecklistId { get; set; }
        public int? ChecklistTemplateId { get; set; }

        public int? AuditId { get; set; }
        public int? AuditTemplateId { get; set; }

        public int? ParentAreaId { get; set; }

        public bool HasFilters()
        {
            return (ActionId.HasValue || UserId.HasValue || CreatedById.HasValue || AssignedAreaId.HasValue || TaskId.HasValue || AssignedUserId.HasValue || TaskTemplateId.HasValue || IsResolved.HasValue || Limit.HasValue || Offset.HasValue || AllowedOnly.HasValue || Timestamp.HasValue || (TagIds != null && TagIds.Length > 0));
        }
    }
}
