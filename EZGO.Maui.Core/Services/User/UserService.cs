using EZGO.Api.Models;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models.Users;

namespace EZGO.Maui.Core.Services.User
{
    public class UserService : IUserService
    {
        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly IFileService fileService;

        private const string userprefsFilename = "userprefs.json";
        private const string userprefsDirectoryName = "userprefs";

        public UserService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
            fileService = DependencyService.Get<IFileService>();
        }

        public async Task<bool> UpdateProfileAsync(UserProfile model)
        {
            bool result = false;

            HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest("userprofile/change", model).ConfigureAwait(false);

            if (response != null && response.IsSuccessStatusCode)
                result = await response.Content.ReadAsJsonAsync<bool>().ConfigureAwait(false);

            return result;
        }

        public async Task<HttpResponseMessage> UpdatePasswordAsync(string currentPassword, string newPassword, string newPasswordRepeat)
        {
            ChangePasswordModel changePasswordModel = new ChangePasswordModel
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                NewPasswordValidation = newPasswordRepeat
            };

            HttpResponseMessage response = await _apiRequestHandler.HandlePostRequest("userprofile/change/password", changePasswordModel).ConfigureAwait(false);

            return response;
        }

        public async Task<List<UserProfileModel>> GetCompanyUsersAsync(bool isFromSyncService = false)
        {
            List<UserProfileModel> result = await _apiRequestHandler.HandleListRequest<UserProfileModel>("userprofiles", isFromSyncService: isFromSyncService).ConfigureAwait(false);
            return result;
        }

        public async Task<UserProfileModel> GetCompanyUserAsync(int id, bool isFromSyncService = false)
        {
            var users = await GetCompanyUsersAsync(isFromSyncService: isFromSyncService).ConfigureAwait(false);
            var result = users.FirstOrDefault(u => u.Id == id);
            return result ?? new UserProfileModel();
        }

        #region localprefs

        public async Task SaveLocalUserprefsAsync(UserPrefsModel userprefs)
        {
            List<UserPrefsModel> result = await GetLocalUserPrefsAsync().ConfigureAwait(false);

            int index = result.FindIndex(x => x.UserId == userprefs.UserId);
            if (index != -1) { result.RemoveAt(index); }

            result.Add(userprefs);

            string userprefsJson = JsonSerializer.Serialize(result);

            await fileService.SaveFileToInternalStorageAsync(userprefsJson, userprefsFilename, userprefsDirectoryName).ConfigureAwait(false);
        }

        private async Task<List<UserPrefsModel>> GetLocalUserPrefsAsync()
        {
            List<UserPrefsModel> result = new List<UserPrefsModel>();

            string userprefsJson = await fileService.ReadFromInternalStorageAsync(userprefsFilename, userprefsDirectoryName).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(userprefsJson))
                result = JsonSerializer.Deserialize<List<UserPrefsModel>>(userprefsJson) ?? new List<UserPrefsModel>();

            return result;
        }

        public async Task<UserPrefsModel> GetLocalUserPrefsAsync(int id)
        {
            List<UserPrefsModel> result = await GetLocalUserPrefsAsync().ConfigureAwait(false);

            return result?.SingleOrDefault(x => x.UserId == id);
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
        }

        #endregion

    }
}
