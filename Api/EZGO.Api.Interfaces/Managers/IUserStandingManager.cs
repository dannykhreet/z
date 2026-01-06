using EZGO.Api.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IUserStandingManager
    {
        Task<List<UserGroup>> GetUserGroupsAsync(int companyId);
        Task<UserGroup> GetUserGroupAsync(int companyId, int userGroupId);
        Task<int> AddUserGroupAsync(int companyId, int userId, UserGroup userGroup);
        Task<int> ChangeUserGroupAsync(int companyId, int userId, int userGroupId, UserGroup userGroup);
        Task<bool> AddUserToUserGroup(int userProfileId, int userGroupId);
        Task<bool> RemoveUserFromUserGroup(int id, int userProfileId, int userGroupId);
        Task<bool> SetUserGroupActiveAsync(int companyId, int userId, int userGroupId, bool isActive);

        Task<List<UserSkill>> GetUserSkills(int companyId);
        Task<UserSkill> GetUserSkill(int companyId, int userSkillId);
        Task<int> AddUserSkill(int companyId, int userId, UserSkill userSkill);
        Task<int> ChangeUserSkill(int companyId, int userId, int userSkillId, UserSkill userSkill, bool deleteOldUserSkillValues);
        Task<bool> SetUserSkillActiveAsync(int companyId, int userId, int userSkillId, bool isActive);
        Task<bool> RemoveUserSkillRelationsAsync(int userSkillId);

        Task<List<UserSkillValue>> GetUserSkillValues(int companyId, int userId, int? limit, int? offset);
        Task<List<UserSkillCustomTargetApplicability>> GetUserSkillsCustomTargetApplicabilitiesForUser(int companyId, int? userId = null);
        Task<int> SetUserSkillCustomTargetApplicability(int companyId, int userId, UserSkillCustomTargetApplicability userSkillCustomTargetApplicability);
        Task<int> RemoveCustomTarget(int companyId, int userId, int userSkillId);
        Task<UserSkillValue> GetUserSkillValue(int companyId, int id);
        Task<UserSkillValue> GetUserSkillValue(int companyId, int userSkillId, int userId);
        Task<int> RemoveUserSkillValueForUserWithSkill(int companyId, int userId, int userSkillId);
        Task<int> UpdateUserSkillValuesWithAssessmentAsync(int companyId, int userId, int assessmentId, int assessmentCompletedForId);
        List<Exception> GetPossibleExceptions();
    }
}
