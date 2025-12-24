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
        Task<int> ChangeUserSkill(int companyId, int userId, int userSkillId, UserSkill userSkill);
        Task<bool> SetUserSkillActiveAsync(int companyId, int userId, int userSkillId, bool isActive);
        Task<bool> RemoveUserSkillRelationsAsync(int userSkillId);

        Task<List<UserSkillValue>> GetUserSkillValues(int companyId, int userId, int? limit, int? offset);
        Task<UserSkillValue> GetUserSkillValue(int companyId, int id);
        Task<UserSkillValue> GetUserSkillValue(int companyId, int userSkillId, int userId);
        Task<int> AddUserSkillValue(int companyId, int userId, UserSkillValue userSkillValue);
        Task<int> ChangeUserSkillValueById(int companyId, int userId, UserSkillValue userSkillValue);
        Task<int> ChangeUserSkillValueByUserSkill(int companyId, int userId, UserSkillValue userSkillValue);
        List<Exception> GetPossibleExceptions();
    }
}
