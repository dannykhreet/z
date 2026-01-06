using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class MatrixUserSkillConverter
    {
        public static UserSkill ToUserSkill(this SkillsMatrixItem matrixUserSkill)
        {
            if (matrixUserSkill != null)
            {
                UserSkill userSkill = new UserSkill();
                userSkill.Id = matrixUserSkill.UserSkillId;
                userSkill.Name = matrixUserSkill.Name;
                userSkill.Description = matrixUserSkill.Description;
                userSkill.SkillType = matrixUserSkill.SkillType;
                userSkill.SkillAssessmentId = matrixUserSkill.SkillAssessmentId;
                userSkill.ValidFrom = matrixUserSkill.ValidFrom;
                userSkill.ValidTo = matrixUserSkill.ValidTo;
                userSkill.ExpiryInDays = matrixUserSkill.ExpiryInDays;
                userSkill.NotificationWindowInDays = matrixUserSkill.NotificationWindowInDays;
                userSkill.Goal = matrixUserSkill.Goal;
                userSkill.DefaultTarget = matrixUserSkill.DefaultTarget;
                return userSkill;

            }

            return null;
        }
    }
}
