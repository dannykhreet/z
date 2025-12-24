using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class TaskTemplateRelationWorkInstructionTemplate : Base.BaseRelationWorkInstruction
    {
        public int TaskTemplateId { get; set; }
        public int WorkInstructionTemplateId { get; set; }
        public int? ChecklistTemplateId { get; set; }
        public int? AuditTemplateId { get; set; }
    }
}
