using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Data
{
    public interface IUserDataManager
    {
        Task<int> GetCompanyIdByUserAuthenticationTokenAsync(string token);
        Task<int> GetUserIdByUserAuthenticationTokenAsync(string token);

        Task<UserRelationCompany> GetUserCompanyRelationByAuthenticationTokenAsync(string token);
        Task<string> GetTokenByUserNameAndPassword(string username, string hashedpassword);
        Task<string> GetTokenByUserName(string username, int companyId);
        Task<string> GetUserPasswordByUserName(string username);
        Task<string> GetUserPasswordByUserId(int userId);
        Task<bool> CheckRecentlyExpiredToken(string token);
    }
}
