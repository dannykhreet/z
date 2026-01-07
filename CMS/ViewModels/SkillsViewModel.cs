using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Shared;
using WebApp.Models.Skills;
using WebApp.Models.WorkInstructions;

namespace WebApp.ViewModels
{
    public class SkillsViewModel : BaseViewModel
    {
        public SkillAssessment CurrentSkillAssessment { get; set; }
        public List<SkillAssessment> SkillAssessments { get; set; }
        public List<SkillAssessmentTemplate> SkillAssessmentTemplates { get; set; }
        public SkillAssessmentTemplate CurrentSkillAssessmentTemplate { get; set; }
        public List<SkillAssessment> CompletedAssessments { get; set; }

        public IScoreColorCalculator AssessmentScoreColorCalculator { get; set; }

        public List<EZGO.Api.Models.Users.UserSkill> UserSkills { get; set; }
        public List<EZGO.Api.Models.Users.UserGroup> UserGroups { get; set; }
        public List<EZGO.Api.Models.Skills.SkillsMatrixUser> MatrixUsers { get; set; }
        public List<EZGO.Api.Models.Users.UserSkillCustomTargetApplicability> Applicabilities { get; set; }

        public SkillsMatrix CurrentSkillsMatrix { get; set; }
        public List<SkillsMatrix> SkillsMatrices { get; set; }
        public SkillMatrixLegendConfiguration LegendConfiguration { get; set; }
        public List<Area> Areas { get; set; }
        public List<SkillAssessmentTemplateWorkInstructionTemplate> WorkInstructionTemplates { get; set; } 
        public List<Models.User.UserProfile> Users { get; set; }
        public TagsViewModel Tags { get; set; } = new TagsViewModel();
        public UserProfile CurrentUser { get; set; }
        public int SelectedUserId { get; set; }
        public int LoggedInUserId { get; set; }
        public bool IsNewTemplate { get; set; }
        public ExtractionModel ExtractionData { get; set; }

        public IMediaService MediaService { get; set; }
    }
}
