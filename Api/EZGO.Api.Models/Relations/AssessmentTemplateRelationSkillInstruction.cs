using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class AssessmentTemplateRelationSkillInstruction
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int AssessmentTemplateId { get; set; }
        public int WorkInstructionTemplateId { get; set; }
    }
}
