using EZGO.Api.Models.Basic;
using System;

namespace EZGO.Api.Models.WorkInstructions
{
    /// <summary>
    /// WorkInstructionItem; Currently only used with Assessments as a AssessmentSkillInstruction <see cref="EZGO.Api.Models.Skills.AssessmentSkillInstruction"/>
    /// </summary>
    public class InstructionItem : Base.InstructionItemBase
    {
        //public int InstructionId { get; set; } //

        #region - for assessments -
        public int? Score { get; set; } //Score filled in by user
        public int? AssessmentId { get; set; } //Assessment where this instruction item belongs to
        public int? AssessmentTemplateId { get; set; } //Assessment where this instruction item belongs to
        public int? AssessmentSkillInstructionId { get; set; } //the skill instruction where this instruction item belongs to
        public int? WorkInstructionTemplateId { get; set; } //the workinstruction template where the item is based on belongs to
        public int? WorkInstructionTemplateItemId { get; set; } //the workinstruction template item where the item is based on
        public DateTime? CompletedAt { get; set; } //When completed for user with assessment (UTC)
        public bool? IsCompleted { get; set; } //Always true when date filled, normally always the case
        public string CompletedFor { get; set; }
        public UserBasic Assessor { get; set; }
        public DateTime? ScoredAt { get; set; }
        public int? CompletedForId { get; set; } //UserId of the user where the item is completed for (NOTE! not the assessor!)
        //TODO add assessment skill instruction item fields
        #endregion
    }
}
