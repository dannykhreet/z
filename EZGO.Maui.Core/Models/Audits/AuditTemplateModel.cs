using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using NodaTime;
using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Audits
{
    public class AuditTemplateModel : EZGO.Api.Models.AuditTemplate, IBase<BasicAuditTemplateModel>, IOpenTextFields, IItemFilter<TaskStatusEnum>
    {
        public new List<TaskTemplateModel> TaskTemplates { get; set; }

        //TODO get score field from API
        public new int Score { get; set; } = 0;

        public new int MaxScore { get; set; } = 10;
        public new int MinScore { get; set; } = 0;

        private readonly Lazy<IScoreColorCalculator> _scoreColorCalculator;
        public IScoreColorCalculator ScoreColorCalculator => _scoreColorCalculator.Value;

        public string Date { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalTasks { get; set; }
        public int OkTasks { get; set; }
        public int NotOkTasks { get; set; }
        public int SkippedTasks { get; set; }
        public int TodoTasks { get; set; }
        public int ScoredTasks { get; set; }
        public List<TemplatePropertyModel> OpenFieldsProperties { get; set; }
        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }
        public LocalDateTime LocalLastSignedAt => Settings.ConvertDateTimeToLocal(LastSignedAt?.ToLocalTime() ?? DateTime.MinValue);

        public AuditTemplateModel()
        {
            _scoreColorCalculator = new Lazy<IScoreColorCalculator>(() => ScoreColorCalculatorFactory.Default(MinScore, MaxScore));
        }

        public BasicAuditTemplateModel ToBasic()
        {
            var result = new BasicAuditTemplateModel
            {
                Id = this.Id,
                Name = this.Name,
                Tags = this.Tags
            };

            return result;
        }

        public TaskStatusEnum FilterStatus { get; set; }
    }
}
