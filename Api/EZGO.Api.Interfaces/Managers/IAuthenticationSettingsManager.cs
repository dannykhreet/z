using EZGO.Api.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface IAuthenticationSettingsManager
    {
        Task<UserAuthenticationSettings> GetUserAuthenticationSettingsAsync(int companyId, int userId);
        Task<int> CreateAuthenticationSettingsBaseAsync(int companyId, int userId, UserAuthenticationSettings baseUserAuthenticationSettings = null);
        Task<UserAuthenticationSettings> GetEnabledMfaOptionsAsync(int companyId, int userId);
        Task<bool> GetUserCanLoginAsync(int companyId, int userId);
        Task<UserAuthenticationSettings> GetUserSyncDataAsync(int companyId, int userId);
        Task<string> GetOrCreateUserGuidAsync(int companyId, int userId);
        Task<string> GetOrCreateSyncGuidAsync(int companyId, int userId);
        Task<bool> GetUserPasswordMustBeChangedAsync(int companyId, int userId);
        Task<bool> GetUserMustSetMfaTopt(int companyId, int userId);
        Task<bool> GetForceUserMfaAsync(int companyId, int userId);
        Task<bool> ValidateToptMfaAsync(int companyId, int userId, string toptCode);
        Task<bool> ValidateEmailMfaAsync(int companyId, int userId, string emailCode);
        Task<bool> ValidateSmsMfaAsync(int companyId, int userId, string smsCode);
        Task<bool> SetToptMfaAsync(int companyId, int userId, string toptCode);
        Task<bool> SetEmailMfaAsync(int companyId, int userId, string emailCode);
        Task<bool> SetSmsMfaAsync(int companyId, int userId, string smsCode);
        Task<bool> SetSyncGuidAsync(int companyId, int userId, string syncGuid);
        Task<bool> SetCanLoginAsync(int companyId, int userId, bool canLogin);
        Task<bool> SetPasswordMustBeChangedNextLoginAsync(int companyId, int userId, bool mustBeChanged);
        Task<bool> SetMfaToptLastUsedAsync(int companyId, int userId);
        Task<bool> SetMfaEmailLastUsedAsync(int companyId, int userId);
        Task<bool> SetMfaSmslLastUsedAsync(int companyId, int userId);
        Task<bool> SetAddPwdLastHashesAsync(int companyId, int userId, string oldHash);
        List<Exception> GetPossibleExceptions();
    }
}


/*
 
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public bool MfaToptEnabled { get; set; }
        public string MfaToptToken { get; set; }
        public DateTime? MfaToptGenerated { get; set; }
        public DateTime? MfaToptLastUse { get; set; }
        public bool MfaEmailEnabled { get; set; }
        public string MfaEmailEmail { get; set; }
        public string MfaEmailToken { get; set; }
        public DateTime? MfaEmailGenerated { get; set; }
        public DateTime? MfaEmailLastUse { get; set; }
        public bool MfaSmsEnabled { get; set; }
        public bool MfaSmsPhone { get; set; }
        public string MfaSmsToken { get; set; }
        public DateTime? MfaSmsGenerated { get; set; }
        public DateTime? MfaLastUse { get; set; }
        public string MfaGeneralGuid { get; set; }
        public int MfaAfterLoginTimeInMin { get; set; }
        public int PasswordRenewTimeframeDays { get; set; }
        public string PasswordOlderHashes { get; set; }
        public DateTime? PasswordLastChanged { get; set; }
        public bool PasswordMustBeChangedNextLogin { get; set; }
        public bool CanLogin { get; set; }
        public DateTime? AccessToDate { get; set; }
        public int AccessGivenById { get; set; }
        public string SyncGuid { get; set; }
        public DateTime? SyncGuidGeneratedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int LastModifiedById { get; set; }
 
 
 */
