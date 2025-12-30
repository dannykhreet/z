using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions
{
    /// <summary>
    /// WorkInstructionItemTemplate;
    /// Template of a Work Instruction Item. WorkInstructionItemTemplates are part of a WorkInstructionTemplate. 
    /// Based on a item a <see cref="EZGO.Api.Models.WorkInstructions.InstructionItem">WorkInstructionItem</see> is created containing user data.
    /// Depending on type this will be used with <see cref="EZGO.Api.Models.Skills.AssessmentTemplateSkillInstruction">SkillInstructionTemplate</see>.
    /// DB: workinstruction_templates
    /// </summary>
    public class WorkInstructionTemplate : Base.WorkInstructionBase
    {
        public List<InstructionItemTemplate> InstructionItems { get; set; }
        public List<WorkInstructionRelationParent> ParentRelations { get; set; }
        public int? SharedTemplateId { get; set; }
        public int? UnreadChangesNotificationsCount { get; set; }
    }
}
