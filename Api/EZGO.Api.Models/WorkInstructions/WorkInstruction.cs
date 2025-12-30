using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions
{
    /// <summary>
    /// WorkInstructionItem; Currently only used with Assessments as a AssessmentSkillInstruction <see cref="EZGO.Api.Models.Skills.AssessmentSkillInstruction"/>
    /// </summary>
    public class WorkInstruction : Base.WorkInstructionBase
    {
        public int? TotalScore { get; set; }
        public List<InstructionItem> InstructionItems { get; set; }

    }
}
