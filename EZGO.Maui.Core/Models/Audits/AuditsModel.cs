using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Audits
{
    public class AuditsModel : EZGO.Api.Models.Audit, IBase<BasicAuditsModel>, IOpenTextFields, ICompletedTemplate
    {
        public new DateTime CreatedAt { get; set; }
        public new List<SignatureModel> Signatures { get; set; }
        public new List<TasksTaskModel> Tasks { get; set; }

        public SignatureModel FirstSignature { get => Signatures?.FirstOrDefault() ?? new SignatureModel { SignedBy = "Unknown" }; }
        public SignatureModel SecondSignature { get => Signatures?.ElementAtOrDefault(1) ?? null; }

        public string SignatureString
        {
            get
            {
                StringBuilder signature = new StringBuilder(string.Empty);
                signature.Append("By");
                signature.Append($" {FirstSignature.SignedBy}");

                if (SecondSignature != null)
                {
                    signature.Append(',');
                    signature.Append($" {SecondSignature.SignedBy}");
                }
                return signature.ToString();
            }
        }

        public LocalDateTime LocalSignedAt
        {
            get
            {
                if (FirstSignature.SignedAt.HasValue)
                    return Settings.ConvertDateTimeToLocal(FirstSignature.SignedAt.Value.ToLocalTime());
                else return Settings.ConvertDateTimeToLocal(ModifiedAt.Value);
            }
        }

        private readonly Lazy<IScoreColorCalculator> scoreColorCalculator;
        public IScoreColorCalculator ScoreColorCalculator => scoreColorCalculator.Value;

        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }
        public List<TemplatePropertyModel> OpenFieldsProperties { get; set; }

        public bool IsPieChartVisible { get; set; } = false;

        public List<StatusModel<TaskStatusEnum>> TaskStatuses => new List<StatusModel<TaskStatusEnum>>();

        public AuditsModel()
        {
            scoreColorCalculator = new Lazy<IScoreColorCalculator>(() => ScoreColorCalculatorFactory.Default(MinTaskScore.Value, MaxTaskScore.Value));
        }

        public BasicAuditsModel ToBasic()
        {
            var result = new BasicAuditsModel
            {
                Description = this.Description,
                Id = this.Id,
                Name = this.Name,
                Picture = this.Picture,
                Signatures = this.Signatures,
                //Tasks = this.Tasks.Select(x => x.ToBasic()).ToList(),
                TemplateId = this.TemplateId,
                TotalScore = this.TotalScore,
            };
            return result;
        }

        public List<int> EditedByUsersId => new List<int>();
    }
}
