using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// SkillInstruction; Skill instruction is based on a work instruction of the type 'skill instruction'. 
    /// DB: assessments_skillinstructions
    /// </summary>
    public class AssessmentSkillInstruction : WorkInstructions.WorkInstruction
    {
        public int AssessmentTemplateId { get; set; }
        public int WorkInstructionTemplateId { get; set; }
        public int AssessmentTemplateSkillInstructionId { get; set; }
        public int AssessmentId { get; set; }
        public int? CompletedForId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletedFor { get; set; }
        public bool IsCompleted { get; set; }
        public int Index { get; set; }
        public List<UserBasic> Assessors { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
