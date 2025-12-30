using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class ChecklistTemplateRelationWorkInstructionTemplate : Base.BaseRelationWorkInstruction
    {
        public int ChecklistTemplateId { get; set; }
        public int WorkInstructionTemplateId { get; set; }
    }
}
