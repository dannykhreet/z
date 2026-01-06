using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;

namespace EZGO.Api.Models.Users
{
    public class UserSkillCustomTargetApplicability
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int UserSkillId { get; set; }
        public int UserId { get; set; }

        public int? CustomTarget { get; set; }
        public bool IsApplicable { get; set; }

        public int CreatedById { get; set; }
        public UserBasic CreatedBy { get; set; }

        public int ModifiedById { get; set; }
        public UserBasic ModifiedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
