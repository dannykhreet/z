using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WebApp.Models.Checklist;
using WebApp.Models.Skills;

namespace WebApp.ViewModels
{
    public class CompletedSkillAssessmentsViewModel : BaseViewModel
    {
        public CompletedSkillAssessmentsViewModel()
        {
        }

        public List<SkillAssessment> CompletedAssessments { get; set; }
        public int TemplateId { get; set; }
        public int AssessmentId { get; set; }

    }
}
