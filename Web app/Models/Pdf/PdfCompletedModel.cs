using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Logic.Interfaces;
using WebApp.Models.Shared;
using WebApp.Models.Properties;
using EZGO.Api.Models.Tags;

namespace WebApp.Models.Pdf
{
    public class PdfCompletedModel
    {
        private readonly Lazy<IScoreColorCalculator> scoreColorCalculator;
        public IScoreColorCalculator ScoreColorCalculator => scoreColorCalculator.Value;

        public PdfCompletedModel()
        {
            scoreColorCalculator = new Lazy<IScoreColorCalculator>(() => ScoreColorCalculatorFactory.Default(MinTaskScore, MaxTaskScore));
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string ScoreType { get; set; } = "thumbs";
        public string AreaPathIds { get; set; }
        public List<SharedSignatureModel> Signatures { get; set; }
        public List<SharedTaskModel> Tasks { get; set; }
        public List<OpenFieldModel> OpenFieldsProperties { get; set; }
        public List<OpenFieldModel> OpenFieldsPropertyUserValues { get; set; }
        public List<Tag> Tags { get; set; }
        public int TotalScore { get; set; }

        public int MaxTaskScore { get; set; } = 10;
        public int MinTaskScore { get; set; }
    }
}
