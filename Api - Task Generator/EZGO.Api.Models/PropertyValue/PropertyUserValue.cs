using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// PropertyUserValue; Property items for checklists/audits and tasks; Contains the user information that a user has filled in;
    /// Depending on implementation this will directly link to the checklisttemplateproperty/audittemplateproperty and tasktemplateproperty structures.
    /// </summary>
    public class PropertyUserValue
    {
        /// <summary>
        /// Id; Specific id for this item
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// PropertyId; used for direct referencing properties, is needed for specific queries. (mostly for performance reasons)
        /// </summary>
        public int PropertyId { get; set; }

        public int? PropertyGroupId { get; set; }

        public int? ChecklistId { get; set; }

        public int? AuditId { get; set; }
        /// <summary>
        /// TaskId; Linked TaskId
        /// </summary>
        public int? TaskId { get; set;  }
        /// <summary>
        /// CompanyId;
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// UserId;
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// TemplatePropertyId; Directly references template property id (e.g. tasks_tasktemplate_properties.id or checklists_checklisttemplate_properties.id or audits_auditstemplate_properties.id).
        /// </summary>
        public int TemplatePropertyId { get; set; }
        public int? UserValueInt { get; set; }
        public string UserValueString { get; set; }
        public decimal? UserValueDecimal { get; set; }
        public string UserValueTime { get; set; }
        public DateTime? UserValueDate { get; set; }
        public bool? UserValueBool { get; set; }
        public DateTime CreatedAt {get; set;}
        public DateTime ModifiedAt { get; set; }
        /// <summary>
        /// RegisteredAt; the datetime when the property was entered
        /// </summary>
        public DateTime? RegisteredAt { get; set; }
        public string ModifiedBy { get; set; }

    }
}
