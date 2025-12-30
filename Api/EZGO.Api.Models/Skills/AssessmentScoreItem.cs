using System;

namespace EZGO.Api.Models.Skills
{
    public class AssessmentScoreItem
    {
        public int AssessmentId { get; set; }

        public int AssessmentTemplateId { get; set; }
        public decimal Score { get; set; }
        public int UserId { get; set; }
        public DateTime ScoreDate { get; set; }
    }
}
