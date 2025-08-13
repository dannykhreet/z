using EZGO.Api.Models.Skills;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.ModelInterfaces;
using NodaTime;

namespace EZGO.Maui.Core.Models.Assessments
{
    public class AssessmentsModel : Assessment, IBase<BasicAssessmentModel>
    {
        public new List<AssessmentSkillInstructionModel> SkillInstructions { get; set; }

        public BasicAssessmentModel ToBasic()
        {
            return new BasicAssessmentModel
            {
                Picture = this.Picture,
                Name = this.Name,
                Description = this.Description,
                CompletedFor = this.CompletedFor,
                CompletedForPicture = this.CompletedForPicture,
                Id = this.Id,
                SkillInstructions = this.SkillInstructions,
                IsCompleted = this.IsCompleted,
                CompanyId = this.CompanyId,
                CreatedAt = Settings.ConvertDateTimeToLocal(this.CreatedAt ?? DateTime.MinValue),
                CreatedBy = this.CreatedBy,
                CreatedById = this.CreatedById,
                ModifiedAt = Settings.ConvertDateTimeToLocal(this.ModifiedAt ?? DateTime.MinValue),
                CompletedAt = Settings.ConvertDateTimeToLocal(this.CompletedAt ?? DateTime.MinValue),
                CompletedForId = this.CompletedForId,
                TemplateId = this.TemplateId,
                Signatures = this.Signatures,
                SignatureRequired = this.SignatureRequired,
                Assessors = this.Assessors,
                StartDate = this.StartDate,
                EndDate = this.EndDate
            };
        }
        public new List<SignatureModel> Signatures { get; set; }

        public LocalDateTime LocalCompletedAt
        {
            get
            {
                if (this.CompletedAt.HasValue)
                    return Settings.ConvertDateTimeToLocal(this.CompletedAt.Value.ToLocalTime());
                else return Settings.ConvertDateTimeToLocal(DateTime.MinValue);
            }
        }

        public double AvarageScore => SkillInstructions?.SelectMany(x => x.InstructionItems).Average(x => x.Score) ?? 0;
        public string AverageScoreString => AvarageScore.ToString("0.00");
    }
}
