using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    /// <summary>
    /// ChecklistTaskIdStatus; Relational object containing all necessary relational id's for creating certain objects within the database.
    /// This object for use with creating a new checklist based on a ChecklistTemplate.
    /// </summary>
    public class ChecklistRelationStatus
    {
        public int ChecklistTemplateId { get; set; }
        public int? ChecklistId { get; set; }
        public int TaskTemplateId { get; set; }
        public long? TaskId { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// Specific date for posting / creating / updating. If supplied this date will be used for the modified_at data in the database.
        /// </summary>
        public DateTime? UserModifiedUtcAt { get; set; }
        public TaskStatusEnum TaskStatus { get; set; }
    }
}
