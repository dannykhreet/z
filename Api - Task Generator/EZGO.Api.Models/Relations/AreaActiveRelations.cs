using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class AreaActiveRelations
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? NrActiveTaskTemplates { get; set; }
        public int? NrActiveActions { get; set; }
        public int? NrActiveChecklistTemplates { get; set; }
        public int? NrActiveAuditTemplates { get; set; }
        public int? NrActiveShifts { get; set; }
        public int? NrActiveChildren { get; set; }
        public int? NrActivWorkinstructions { get; set; }
        public int? NrActiveAssessmentTemplates { get; set; }
        public int? NrActiveMatrices { get; set; }
        public bool? HasActiveTaskTemplates { get; set; }
        public bool? HasActiveActions { get; set; }
        public bool? HasActiveChecklistTemplates { get; set; }
        public bool? HasActiveAuditTemplates { get; set; }
        public bool? HasActiveShifts { get; set; }
        public bool? HasActiveChildren { get; set; }
        public bool? HasActiveWorkInstructionTemplates { get; set; }
        public bool? HasActiveAssessmentTemplates { get; set; }
        public bool? HasActiveSkillsMatrices { get; set; }
    }
}
