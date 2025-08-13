using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using Newtonsoft.Json;
using NodaTime;

namespace EZGO.Maui.Core.Models.Assessments
{
    public class AssessmentSkillInstructionModel : EZGO.Api.Models.Skills.AssessmentSkillInstruction, IItemFilter<SkillTypeEnum>
    {
        [JsonIgnore]
        public SkillTypeEnum FilterStatus { get; set; } = SkillTypeEnum.Mandatory;
        public new List<BasicAssessmentInstructionItemModel> InstructionItems { get; set; }
        public new bool IsCompleted => InstructionItems != null && !InstructionItems.Any(i => !(i.IsCompleted ?? false));

        public new int? TotalScore { get => InstructionItems.Sum(i => i.Score); }
        public bool IsStarted { get => InstructionItems.Any(i => i.Score > 0 && IsCompleted == false); }
        public bool IsStartedOrCompleted { get => InstructionItems.Any(i => i.Score > 0 | i.IsCompleted ?? true); }
        public double AvarageScore => InstructionItems.Average(x => x.Score) ?? 0;
        public string AverageScoreString => AvarageScore.ToString("0.00");

        [JsonIgnore]
        public LocalDateTime? LocalStartedAt => StartDate.HasValue ? Settings.ConvertDateTimeToLocal(StartDate.Value) : (LocalDateTime?)null;

        [JsonIgnore]
        public LocalDateTime? LocalCompletedAt => EndDate.HasValue ? Settings.ConvertDateTimeToLocal(EndDate.Value) : (LocalDateTime?)null;

        [JsonIgnore]
        public List<int> EditedByUsersId =>
               Assessors?
                   .Where(a => a != null)
                   .Select(a => a.Id)
                   .Distinct()
                   .ToList() ?? new List<int>();

        [JsonIgnore]
        public List<string> EditedByUsersName =>
            Assessors?
                .Where(a => !string.IsNullOrWhiteSpace(a?.Name))
                .Select(a => a.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
        public List<UserBasic> Assessors { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        [JsonIgnore]
        public string ReturnListOfUsers => string.Join(", ", EditedByUsersName);

    }
}
