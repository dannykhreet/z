using EZGO.Api.Models.Basic;
using System;
using System.Collections.Generic;

namespace EZGO.Api.Models.Skills
{
    /// <summary>
    /// 
    /// DB: assessments
    /// </summary>
    public class Assessment : Base.AssessmentBase
    {
        public List<Signature> Signatures { get; set; }
        public List<AssessmentSkillInstruction> SkillInstructions { get; set; }
        public int NumberOfSignatures { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public int? CompletedForId { get; set; }
        public string CompletedFor { get; set; }
        public string CompletedForPicture { get; set; }
        public int? AssessorId { get; set; }
        public string Assessor { get; set; }
        public List<UserBasic> Assessors { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AssessorPicture { get; set; }
        public int TemplateId { get; set; }
    }
}
