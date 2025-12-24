using EZGO.Api.Data.Enumerations;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    /// <summary>
    /// IUserManager, Interface for use with the UserManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IUserManager
    {
        Task<List<UserProfile>> GetUserProfilesAsync(int companyId, UserFilters? filters = null, string include = null);
        Task<UserProfile> GetUserProfileAsync(int companyId, int userId, string include = null);
        Task<UserProfile> GetUserProfileByTokenAsync(int companyId, string userToken, bool tokenIsEncrypted = false, string include = null, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<List<UserBasic>> GetUsersBasicAsync(int companyId);
        Task<List<int>> GetUsersIdsAsync(int companyId, string currentUserGuid = null, string userGuid = null, string userSyncGuid = null);
        Task<int> RetrieveSystemUserId(int companyId, int? holdingId = null);
        Task<UserBasic> GetUserBasicAsync(int companyId, int userId);
        Task<int> AddUserProfileAsync(int companyId, int userId,  UserProfile userProfile);
        Task<bool> ChangeUserProfileAsync(int companyId, int userId, UserProfile userProfile);
        Task<bool> ChangeUserProfileIncludingAreasAsync(int companyId, int userId, int userProfileId, UserProfile userProfile);
        Task<bool> ChangeUserPasswordAsync(int companyId, int userId, int userProfileId, string userPassword, string userPasswordConfirmation);
        Task<bool> ChangeUserPasswordAsync(int companyId, int userId, int userProfileId, string currentUserPassword, string userPassword, string userPasswordConfirmation);
        Task<bool> AddOrChangeExtendedUserProfileDetails(int companyId, int userId, int userProfileId, UserExtendedDetails details);
        Task<bool> AddOrChangeUserSettings(int companyId, int userId, int userProfileId, UserSettings userSettings);
        Task<bool> AddOrChangeUserAppPreferences(int companyId, int userId, int userProfileId, UserAppPreferences userAppPreferences);
        Task<UserExtendedDetails> GetExtendedUserProfileDetails(int companyId, int userId, int userProfileId);
        Task<UserAppPreferencesWithMetadata> GetUserProfileAppPreferences(int companyId, int userId, int userProfileId);
        Task<bool> SetUserProfileActiveAsync(int companyId, int userProfileId, bool isActive = true);
        Task<string> GenerateUserPassword(string password);
        Task<bool> ResetOrCreateAuthenticationDbToken(string userName, string encryptedPassword);
        Task<bool> ResetOrCreateAuthenticationDbTokenIfExpired(string userName, string encryptedPassword);
        Task<bool> SetLastLoggedInDate(int userId);
        Task<bool> CheckEmail(string email, int? userId = null, int? companyId = null);
        Task<bool> CheckUserName(string userName, int? userId = null, int? companyId = null);
        Task<bool> CheckUPN(string upn, int? userId = null, int? companyId = null);
        Task<bool> SetSuccessor(int companyid, int userid, int successorid);
        Task<bool> AddLoginSecurityLogEvent(string message, string description, int eventId = 0, string type = "INFORMATION", string source = null);
        Task<bool> ResetOrCreateAuthenticationDbTokenByUserName(string userName);
        Task<bool> ResetOrCreateAuthenticationDbTokenIfExpiredByUserName(string userName);
        Task<string> GetUserNameByUPN(string upn, int companyId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<int> GetUserIdByUPN(string upn, int companyId, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<int> GetCompanyIdByUPN(string upn, ConnectionKind connectionKind = ConnectionKind.Reader);
        Task<string> GetTenantByCompanyId(int companyId, ConnectionKind connectionKind = ConnectionKind.Reader);
        List<Exception> GetPossibleExceptions();

    }
}
