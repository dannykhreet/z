using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// 
    /// DB: matrices
    /// </summary>
    public class SkillsMatrix
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SkillsMatrixItem> MandatorySkills { get; set; }
        public List<SkillsMatrixItem> OperationalSkills { get; set; }
        public List<SkillsMatrixBehaviourItem> OperationalBehaviours { get; set; }
        public List<SkillsMatrixBehaviourItem> MatrixTotals { get; set; }
        public List<SkillsMatrixUserGroup> UserGroups { get; set; }
        public int NumberOfUserGroups { get; set; }
        public int NumberOfMandatorySkills { get; set; }
        public int NumberOfOperationalSkills { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? AreaId { get; set; }
        public string AreaPath { get; set; }
        public string AreaPathIds { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this matrix
        /// </summary>
        public List<Tag> Tags { get; set; }
    }
}
