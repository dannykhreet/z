using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// 
    /// DB: skillassessment_template_skillinstructions and workinstruction_templates
    /// </summary>
    public class AssessmentTemplateSkillInstruction : WorkInstructions.WorkInstructionTemplate
    {
        public int? AssessmentTemplateId { get; set; }
        public int? WorkInstructionTemplateId { get; set; }
        public int Index { get; set; }
    }
}
