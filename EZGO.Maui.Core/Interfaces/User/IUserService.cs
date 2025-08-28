using EZGO.Api.Models;
using EZGO.Maui.Core.Models.Users;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.User
{
    public interface IUserService : IDisposable
    {
        Task<bool> UpdateProfileAsync(UserProfile model);

        Task<HttpResponseMessage> UpdatePasswordAsync(string currentPassword, string newPassword, string newPasswordRepeat);

        Task<List<UserProfileModel>> GetCompanyUsersAsync(bool isFromSyncService = false);

        Task<UserProfileModel> GetCompanyUserAsync(int id, bool isFromSyncService = false);

        Task SaveLocalUserprefsAsync(UserPrefsModel userprefs);

        Task<UserPrefsModel> GetLocalUserPrefsAsync(int id);
    }
}
