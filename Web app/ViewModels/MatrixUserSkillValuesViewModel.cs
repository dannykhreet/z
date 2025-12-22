using EZGO.Api.Models;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Users;
using System.Collections.Generic;

namespace WebApp.ViewModels
{
    public class MatrixUserSkillValuesViewModel : BaseViewModel
    {
        public int MatrixId { get; set; }
        public UserProfile User { get; set; }
        public List<UserSkillValue> UserSkillValues { get; set; }
        public List<SkillsMatrixItem> UserSkills { get; set; }
        public List<UserSkillCustomTargetApplicability> Applicabilities { get; set; }
    }
}
