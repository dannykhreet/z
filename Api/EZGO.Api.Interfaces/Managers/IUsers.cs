using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    //TODO add mutation methods (add, change, set active etc.)
    public interface IUsers
    {
        Task<List<UserGroup>> GetUserGroupsAsync(int companyId, int? userId = null, UserGroupFilters? filters = null, string include = null);
        Task<UserGroup> GetUserGroupAsync(int companyId, int userGroupId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<UserSkill>> GetUserSkillsAsync(int companyId, int? userId = null, UserSkillFilters? filters = null, string include = null);
        Task<UserSkill> GetUserSkillAsync(int companyId, int userSkillId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<UserSkillValue>> GetUserSkillValuesAsync(int companyId, int? userId = null, UserSkillFilters? filters = null, string include = null);
        Task<UserSkillValue> GetUserSkillValueAsync(int companyId, int userSkillValueId, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        List<Exception> GetPossibleExceptions();
    }
}
