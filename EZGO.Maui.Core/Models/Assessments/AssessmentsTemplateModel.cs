using System.Collections.Generic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Core.Models.Assessments
{
    public class AssessmentsTemplateModel : Api.Models.Skills.AssessmentTemplate, IItemFilter<SkillTypeEnum>
    {
        public List<AssessmentTemplateSkillInstructionModel> SkillInstructions { get; set; }
        public SkillTypeEnum FilterStatus { get; set; }
    }
}
