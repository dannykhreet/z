using System;
using System.Collections.Generic;

namespace WebApp.Models.Assessments
{
    public class AssessmentModel
    {
        public int Id { get; set; }
        public List<AssessmentSignatureModel> Signatures { get; set; }
        public List<AssessmentInstructionModel> SkillInstructions { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public int CompletedForId { get; set; }
        public int AssessorId { get; set; }
        public int TemplateId { get; set; }
        public int CompanyId { get; set; }

    }
}
