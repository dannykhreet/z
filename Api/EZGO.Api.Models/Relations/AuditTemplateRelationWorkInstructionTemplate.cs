using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class AuditTemplateRelationWorkInstructionTemplate : Base.BaseRelationWorkInstruction
    {
        public int AuditTemplateId { get; set; }
        public int WorkInstructionTemplateId { get; set; }
    }
}
