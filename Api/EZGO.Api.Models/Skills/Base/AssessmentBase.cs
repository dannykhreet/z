using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills.Base
{
    public class AssessmentBase
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; } //note first item in media collection (db)
        public AssessmentTypeEnum AssessmentType { get; set; }
        public int? AreaId { get; set; }
        public string AreaPath { get; set; }
        public string AreaPathIds { get; set; }
        public List<string> Media { get; set; }
        public int? TotalScore { get; set; }
        public RoleTypeEnum? Role { get; set; }
        public RequiredSignatureTypeEnum SignatureType { get; set; }
        public bool SignatureRequired { get; set; }
        public int NumberOfSkillInstructions { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this assessment (item) template
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// Version; The version of this instance of the object. Format VyyyyMMddHHmmsss (e.g. V202403131215347 when generated on 13 march 2024 at 13:15:34.7)
        /// </summary>
        public string Version { get; set; }
    }
}
