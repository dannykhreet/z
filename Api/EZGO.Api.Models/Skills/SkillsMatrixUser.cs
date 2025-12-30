using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// 
    /// </summary>
    public class SkillsMatrixUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Picture { get; set; }
        public int UserProfileId { get; set; } //DB profile_users.id
        public int GroupId { get; set; }
        public RoleTypeEnum Role { get; set; }
        public List<SkillsMatrixItem> MandatorySkills { get; set; }
        public List<SkillsMatrixItem> OperationalSkills { get; set; }
        public List<SkillsMatrixBehaviourItem> OperationalBehaviours { get; set; }
        public List<SkillsMatrixItemValue> SkillValues { get; set; }

    }
}
