using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Users
{
    public class UserSkill
    {
        public int Id { get; set; }
        public int? SkillAssessmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Goal { get; set; }
        public int Result { get; set; }
        public int GoalResultDifference { get; set; }
        public int DefaultTarget { get; set; }
        public SkillTypeEnum SkillType { get; set; }
        public int? ExpiryInDays { get; set; }
        public int? NotificationWindowInDays { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public List<UserSkillValue> Values { get; set; }
        /// <summary>
        /// Tags; Tags that are added to this user skill
        /// </summary>
        public List<Tag> Tags { get; set; }
        public bool InUseInMatrix { get; set; }
    }
}
