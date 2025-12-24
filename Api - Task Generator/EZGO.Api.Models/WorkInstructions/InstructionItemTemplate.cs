using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.WorkInstructions
{
    /// <summary>
    /// WorkInstructionItemTemplate;
    /// Template of a Work Instruction Item. WorkInstructionItemTemplates are part of a WorkInstructionTemplate. 
    /// Based on a item a <see cref="EZGO.Api.Models.WorkInstructions.InstructionItem">WorkInstructionItem</see> is created containing user data.
    /// Depending on type of parent object this will be used with <see cref="EZGO.Api.Models.Skills.AssessmentTemplateSkillInstruction">SkillInstructionTemplate</see>.
    /// DB: workinstruction_template_items
    /// </summary>
    public class InstructionItemTemplate : Base.InstructionItemBase
    {
        public int InstructionTemplateId { get; set; } //TODO rename to workinstrucitontemplateid seeing it is a workinstruction template id
        public int AssessmentTemplateId { get; set; } //TODO needs a better soluition, AssessmentTemplateId added for specific logic reasons but place of this seems wrong seeing a workinstruciton item does not have a assessment template ? And if so, why is it not nullable?
    }
}
