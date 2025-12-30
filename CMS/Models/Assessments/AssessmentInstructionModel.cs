using System;
using System.Collections.Generic;

namespace WebApp.Models.Assessments
{
    public class AssessmentInstructionModel
    {

        public int WorkInstructionTemplateId { get; set; }
        public int AssessmentTemplateSkillInstructionId { get; set; }
        public int CompletedForId { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalScore { get; set; }
        public List<AssessmentInstructionItemModel> InstructionItems { get; set; }

    }
}
