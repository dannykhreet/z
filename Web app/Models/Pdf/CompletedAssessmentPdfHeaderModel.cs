using System;

namespace WebApp.Models.Pdf
{
    public class CompletedAssessmentPdfHeaderModel
    {
        public string CompletedAssessmentName { get; set; }
        public string CompletedAssessmentDescription { get; set; }
        public DateTime CompletedAt { get; set; }
        public string CompletedBy { get; set; }
        public string Assessor { get; set; }
        public string AssessmentScore { get; set; }

        public int HeaderHeight
        {
            get { return 30; } //old calucaltion: { return 30 + (int)((CompletedAssessmentName?.Length ?? 1 / 80 * 5.5) + (CompletedAssessmentDescription?.Length ?? 1 / 80 * 5.5)); }
        }
    }
}
