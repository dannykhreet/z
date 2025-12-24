using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    public class SkillsMatrixUserGroup
    {
        public int Id { get; set; } //db: matrix_user_group.id
        public int UserGroupId { get; set; } //db user_group.id
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SkillsMatrixUser> Users { get; set; }
    }
}
