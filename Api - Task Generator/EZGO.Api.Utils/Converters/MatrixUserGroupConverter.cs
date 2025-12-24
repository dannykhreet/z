using EZGO.Api.Models.Skills;
using EZGO.Api.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class MatrixUserGroupConverter
    {
        public static UserGroup ToUserGroup(this SkillsMatrixUserGroup matrixUserGroup)
        {
            if (matrixUserGroup != null)
            {
                UserGroup userGroup = new UserGroup();
                userGroup.Id = matrixUserGroup.UserGroupId;
                userGroup.Name = matrixUserGroup.Name;
                userGroup.Description = matrixUserGroup.Description;
                userGroup.GroupType = Models.Enumerations.GroupTypeEnum.General;
                return userGroup;

            }

            return null;
        }
    }
}
