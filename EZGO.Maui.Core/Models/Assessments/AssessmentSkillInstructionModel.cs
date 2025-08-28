using System.Collections.Generic;
using System;
using System.Linq;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;
using Newtonsoft.Json;

namespace EZGO.Maui.Core.Models.Assessments
{
    public class AssessmentSkillInstructionModel : EZGO.Api.Models.Skills.AssessmentSkillInstruction, IItemFilter<SkillTypeEnum>
    {
        [JsonIgnore]
        public SkillTypeEnum FilterStatus { get; set; } = SkillTypeEnum.Mandatory;
        public new List<BasicAssessmentInstructionItemModel> InstructionItems { get; set; }
        public new bool IsCompleted { get => !InstructionItems.Any(i => !i.IsCompleted ?? false); }
        public new int? TotalScore { get => InstructionItems.Sum(i => i.Score); }

        public double AvarageScore => InstructionItems.Average(x => x.Score) ?? 0;
        public string AverageScoreString => AvarageScore.ToString("0.00");
    }
}
