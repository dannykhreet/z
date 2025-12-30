using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    public class SkillsMatrixBehaviourItemValue
    {
        public int ScoreOrNumber { get; set; }
        public int UserId { get; set; }
        public string TechnicalUid { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
