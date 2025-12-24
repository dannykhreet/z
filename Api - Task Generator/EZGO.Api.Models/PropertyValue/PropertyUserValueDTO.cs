using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.PropertyValue
{
    /// <summary>
    /// Simplified property object to use as a data transfer object for the user input related data for properties
    /// </summary>
    public class PropertyUserValueDTO
    {
        /// <summary>
        /// Id of the property user value
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// Value filled in by user
        /// </summary>
        public string UserValue { get; set; }
        /// <summary>
        /// User who filled the property value
        /// </summary>
        public UserBasic User { get; set; }
        /// <summary>
        /// Date and time the value was last modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }
        /// <summary>
        /// RegisteredAt; DateTime when the property was registered (can be null)
        /// </summary>
        public DateTime? RegisteredAt { get; set; }
        /// <summary>
        /// Optional. Id of the task this property is related to
        /// </summary>
        public int? TaskId { get; set; }
        /// <summary>
        /// Optional. Id of the audit this property is related to
        /// </summary>
        public int? AuditId { get; set; }
        /// <summary>
        /// Optional. Id of the checklist this property is related to
        /// </summary>
        public int? ChecklistId { get; set; }

    }
}
