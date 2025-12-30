using EZGO.Api.Models.Authentication;
using EZGO.Api.Models.Enumerations;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Security.Interfaces
{
    /// <summary>
    /// IApplicationUser; Used for Core3.1 DI structure.
    /// </summary>
    public interface IApplicationUser
    {
        Task<int> GetAndSetCompanyIdAsync();
        Task<int> GetAndSetCompanyIdByDjangoTokenAsync();
        Task<int> GetAndSetUserIdAsync();
        Task<string> LoginAndGetDjangoSecurityToken(string username, string password);
        Task<LoggedIn> LoginAndGetSecurityToken(string username, string password, bool? isCmsLogin = false, bool? isAppLogin = false, string ips = null);
        Task<LoggedIn> LoginAndGetSecurityToken(ExternalLogin external, bool? isCmsLogin = false, bool? isAppLogin = false, string ips = null);
        Task<string> GetCheckAndGetExternalLogin(string username);
        Task<bool> CheckObjectRights(int objectId, ObjectTypeEnum objectType, [System.Runtime.CompilerServices.CallerMemberName] string referrer = "");
        Task<bool> CheckObjectCompanyRights(int objectCompanyId, ObjectTypeEnum objectType, [System.Runtime.CompilerServices.CallerMemberName] string referrer = "");
        Task<bool> AddLoginSecurityLogEvent(string message, string description, int eventId = 0, string type = "INFORMATION", string source = null);
        Task<bool> AddApplicationLogEvent(int companyId, int userId, string userAgent, string appVersion, string appOs, string app, string ip, string language, string type = "APP");
        Task<bool> CheckAreaRightsForWorkinstruction(int workInstructionId);
        Task<bool> ValidateIpForLoginAuthentication(int companyId, string possibleIps);

    }
}
