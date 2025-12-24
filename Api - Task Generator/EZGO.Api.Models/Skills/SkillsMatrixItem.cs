using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    public class SkillsMatrixItem
    {
        public int Id { get; set; } //DB: matrix_user_skills.id
        public int? SkillAssessmentId { get; set; }
        public string SkillAssessmentName { get; set; }
        public int UserSkillId { get; set; } //DB: user_skill.id
        public string Name { get; set; }
        public string Description { get; set; }
        public int Goal { get; set; }
        public int Result { get; set; }
        public int GoalResultDifference { get; set; }
        public int DefaultTarget { get; set; }
        public SkillTypeEnum SkillType { get; set; }
        public int? ExpiryInDays { get; set; }
        public int? NotificationWindowInDays { get; set; }
        public int? Index { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public List<SkillsMatrixItemValue> Values { get; set; }

    }
}
