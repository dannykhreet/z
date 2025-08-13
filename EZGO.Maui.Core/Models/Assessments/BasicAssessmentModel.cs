using EZGO.Api.Models.Basic;
using EZGO.Maui.Core.Classes;
using NodaTime;
using System.Text.Json.Serialization;

namespace EZGO.Maui.Core.Models.Assessments
{
    public class BasicAssessmentModel : NotifyPropertyChanged
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? CompletedForId { get; set; }
        public int TemplateId { get; set; }
        public int? CreatedById { get; set; }
        public string Picture { get; set; }
        public string DisplayPicture { get => Picture; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CompletedFor { get; set; }
        public string CreatedBy { get; set; }
        public string CompletedForPicture { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool SignatureRequired { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsSignButtonEnabled => SkillInstructions != null && !SkillInstructions.Any(i => !i.IsCompleted);
        public List<AssessmentSkillInstructionModel> SkillInstructions { get; set; }
        public LocalDateTime CreatedAt { get; set; }
        public LocalDateTime ModifiedAt { get; set; }
        public LocalDateTime CompletedAt { get; set; }
        public List<SignatureModel> Signatures { get; set; }
        public int? TotalScore { get => SkillInstructions?.Sum(i => i.TotalScore); }
        public string Version { get; set; }
        public List<UserBasic> Assessors { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [JsonIgnore]
        public bool HasAssessors => Assessors?.Any() == true;

        [JsonIgnore]
        public List<int> EditedByUsersId =>
            Assessors?.Where(a => a != null).Select(a => a.Id).Distinct().ToList() ?? new();

        [JsonIgnore]
        public string ReturnListOfUsers =>
            Assessors == null
                ? string.Empty
                : string.Join(", ",
                    Assessors
                        .Where(a => !string.IsNullOrWhiteSpace(a?.Name))
                        .Select(a => a.Name!.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase));
        public AssessmentsModel ToModel()
        {
            AssessmentsModel result = new AssessmentsModel
            {
                Picture = this.Picture,
                Name = this.Name,
                Description = this.Description,
                CompletedFor = this.CompletedFor,
                CompletedForPicture = this.CompletedForPicture,
                Id = this.Id,
                SkillInstructions = this.SkillInstructions,
                CompanyId = this.CompanyId,
                ModifiedAt = this.ModifiedAt.ToDateTimeUnspecified(),
                CompletedAt = this.CompletedAt.ToDateTimeUnspecified(),
                IsCompleted = this.IsCompleted,
                CompletedForId = this.CompletedForId,
                TemplateId = this.TemplateId,
                Signatures = this.Signatures,
                SignatureRequired = this.SignatureRequired,
                TotalScore = this.TotalScore,
                Version = this.Version,
                Assessors = this.Assessors,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
            };
            return result;
        }
    }
}
