using System;
namespace WebApp.Models.Assessments
{
    public class AssessmentInstructionItemModel
    {
        public int Score { get; set; }
        public int WorkInstructionTemplateItemId { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public int CompletedForId { get; set; }

    }
}
