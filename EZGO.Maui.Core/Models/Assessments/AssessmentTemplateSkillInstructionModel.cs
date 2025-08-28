using System;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Assessments
{
    public class AssessmentTemplateSkillInstructionModel : EZGO.Api.Models.Skills.AssessmentTemplateSkillInstruction, IItemFilter<SkillTypeEnum>
    {
        public SkillTypeEnum FilterStatus { get; set; } = SkillTypeEnum.Mandatory;

    }
}
