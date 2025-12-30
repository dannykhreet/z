using System;
using System.Collections.Generic;
using EZGO.Api.Models;
using WebApp.Models.Skills;

namespace WebApp.ViewModels
{
    public class AssessmentViewModel : BaseViewModel
    {

        public SkillAssessment CurrentSkillAssessment { get; set; }
        public List<SkillAssessment> SkillAssessments { get; set; }
        public List<SkillAssessmentTemplate> SkillAssessmentTemplates { get; set; }
        public SkillAssessmentTemplate CurrentSkillAssessmentTemplate { get; set; }
        public SkillsMatrix CurrentSkillsMatrix { get; set; }
        public List<SkillsMatrix> SkillsMatrices { get; set; }
        public List<Area> Areas { get; set; }
        public TagsViewModel Tags { get; set; } = new TagsViewModel();
        public List<SkillAssessmentTemplateWorkInstructionTemplate> WorkInstructionTemplates { get; set; }
        public List<Models.User.UserProfile> Users { get; set; }
        public int SelectedUserId { get; set; }
        public int LoggedInUserId { get; set; }

    }
}
