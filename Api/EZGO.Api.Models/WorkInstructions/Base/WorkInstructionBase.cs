using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;

namespace EZGO.Api.Models.WorkInstructions.Base
{
    public class WorkInstructionBase
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; } //first item in media collection / attachments
        //public int? MinScore { get; set; }
        //public int? MaxScore { get; set; }
        public int? AreaId { get; set; }
        public string AreaPath { get; set; }
        public string AreaPathIds { get; set; }
        public int NumberOfInstructionItems { get; set; }
        public List<string> Media { get; set; }
        public InstructionTypeEnum WorkInstructionType { get; set; }
        public bool IsWITemplateLinkedToAssessment { get; set; }
        public RoleTypeEnum? Role { get; set; }
        //public ScoreTypeEnum? ScoreType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this workinstruction template
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// Determines if the work instruction is available to add to checklists audits and tasks for all areas
        /// </summary>
        public bool? IsAvailableForAllAreas { get; set; }
        /// <summary>
        /// Version; The version of this instance of the object. Format VyyyyMMddHHmmsss (e.g. V202403131215347 when generated on 13 march 2024 at 13:15:34.7)
        /// </summary>
        public string Version { get; set; }
    }
}
