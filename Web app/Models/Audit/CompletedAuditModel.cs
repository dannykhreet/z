using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models.Properties;

namespace WebApp.Models.Audit
{
    public class CompletedAuditModel
    {
        private readonly Lazy<IScoreColorCalculator> scoreColorCalculator;
        public IScoreColorCalculator ScoreColorCalculator => scoreColorCalculator.Value;

        public CompletedAuditModel()
        {
            scoreColorCalculator = new Lazy<IScoreColorCalculator>(() => ScoreColorCalculatorFactory.Default(MinTaskScore.Value, MaxTaskScore.Value));
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string ScoreType { get; set; }
        public string AreaPathIds { get; set; }
        public List<CompletedAuditSignatureModel> Signatures { get; set; }
        public List<CompletedAuditTaskModel> Tasks { get; set; }
        public int TotalScore { get; set; }
        public int? MaxTaskScore { get; set; }
        public int? MinTaskScore { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public List<OpenFieldModel> OpenFieldsProperties { get; set; }
        public List<OpenFieldModel> OpenFieldsPropertyUserValues { get; set; }
        public PictureProof PictureProof { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
