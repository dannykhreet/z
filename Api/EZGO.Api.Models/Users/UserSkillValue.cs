using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;

namespace EZGO.Api.Models.Users
{
    public class UserSkillValue
    {
        public int Id { get; set; }
        public int UserSkillId { get; set; }
        public int UserId { get; set; }
        public decimal Score { get; set; }
        public bool IsDynamic { get; set; } //e.g. score from skill assessment, else forced input. 
        public List<string> Attachments { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime ValueExpirationDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public ScoringMethodEnum? ScoringMethod { get; set; }
    }
}
