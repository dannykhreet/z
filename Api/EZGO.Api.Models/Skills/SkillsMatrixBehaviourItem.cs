using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    public class SkillsMatrixBehaviourItem
    {
        public string TechnicalUid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public List<SkillsMatrixBehaviourItemValue> Values { get; set; }
    }
}
