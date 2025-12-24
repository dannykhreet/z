using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// 
    /// DB: assessment_templates
    /// </summary>
    public class AssessmentTemplate : Base.AssessmentBase
    {
        public List<AssessmentTemplateSkillInstruction> SkillInstructions { get; set; }
        public int NumberOfAssessments { get; set; }
        public int NumberOfOpenAssessments { get; set; }
        public DateTime? LastActivityDate { get; set; }


    }
}
