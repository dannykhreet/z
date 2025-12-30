using EZGO.Api.Models;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Users;
using System.Collections.Generic;

namespace WebApp.ViewModels
{
    public class MatrixUserSkillValueDetailsViewModel : BaseViewModel
    {
        public int MatrixId { get; set; }
        public UserProfile User { get; set; }
        public UserSkillValue UserSkillValue { get; set; }
        public SkillsMatrixItem UserSkill { get; set; }
        public UserSkillCustomTargetApplicability Applicability { get; set; }
    }
}
