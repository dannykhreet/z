using Autofac;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Api;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Login;
using EZGO.Maui.Core.Interfaces.User;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Authentication;
using EZGO.Maui.Core.Models.Users;
using EZGO.Maui.Core.Utils;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using JsonSerializer = EZGO.Maui.Core.Classes.JsonSerializer;

namespace EZGO.Maui.Core.Services.Login
{
    public class LoginService : ILoginService
    {
        #region Constants

        private const string usernamesFilename = "usernames.json";
        private const string usernamesDirectoryName = "usernames";

        private readonly IApiClient _apiClient;
        private readonly IFileService _fileService;
        private readonly IUserService _userService;
        private readonly ISettingsService _settingsService;
        private readonly IPublicClientApplication _pca;

        private readonly string AppId = "nl.ezfactory.ezgoxam";
        private readonly string ClientID = "7f3290a6-2d16-493b-93f5-e29181bfacda";
        private readonly string[] Scopes = { "User.Read" };

        private string RedirectUri
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    return $"msauth://{AppId}/2jmj7l5rSw0yVb%2FvlWAYkK%2FYBwk%3D";
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                    return $"msauth.{AppId}://auth";

                return string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// Android uses this to determine which activity to use to show the login screen dialog from.
        /// </summary>
        public static object ParentWindow { get; set; }

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LoginService(IApiClient apiClient, IUserService userService, ISettingsService settingsService)
        {
            _apiClient = apiClient;
            _fileService = DependencyService.Get<IFileService>();
            _userService = userService;
            _settingsService = settingsService;

            _pca = PublicClientApplicationBuilder
                .Create(ClientID)
                .WithIosKeychainSecurityGroup(AppId)
                .WithRedirectUri(RedirectUri)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .Build();
        }

        #endregion

        #region Interface Implementation

        public async Task<SignInResult> SignInWithCredentialsAsync(string username, string password)
        {
            var jwtToken = await PostCredentialsAsync(username, password);

            if (string.IsNullOrEmpty(jwtToken))
                return SignInResult.IncorrectCredentials;

            var result = await SetupProfileFromTokenAsync(jwtToken, username);

            return result;
        }

        public async Task<SignInResult> SignInMsalAsync(string username)
        {
            var silentLoginResult = await SilentSignInMsalAsync(username);

            // We're already authenticated by MSAL through silent sign in
            if (silentLoginResult == SignInResult.Ok)
            {
                return SignInResult.Ok;
            }
            // Silent login failed
            else
            {
                try
                {
                    var builder = _pca.AcquireTokenInteractive(Scopes)
                                      .WithParentActivityOrWindow(ParentWindow)
                                      .WithUseEmbeddedWebView(true)
                                      .WithPrompt(Prompt.ForceLogin)
                                      .WithLoginHint(username);

                    var authResult = await builder.ExecuteAsync();

                    // Store the access token securely for later use.
                    await Settings.SetMsalAccessTokenAsync(authResult?.AccessToken);

                    var token = await GetEzgoTokenFromMsalTokenAsync(username, authResult.IdToken);

                    if (string.IsNullOrEmpty(token))
                        return SignInResult.LinkedAccountNotFound;

                    var result = await SetupProfileFromTokenAsync(token, authResult.Account.Username);

                    return result;
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == MsalError.AuthenticationCanceledError)
                        return SignInResult.Canceled;

                    return SignInResult.Failed;
                }
            }
        }

        public async Task<SignInResult> SilentSignInMsalAsync(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    throw new ArgumentNullException(nameof(username));

                // Can show the list of accounts here

                var accounts = (await _pca.GetAccountsAsync())
                    .Where(acc => acc.Username == username);

                var firstAccount = accounts.FirstOrDefault();

                var authResult = await _pca.AcquireTokenSilent(Scopes, firstAccount).ExecuteAsync();

                // Store the access token securely for later use.
                await Settings.SetMsalAccessTokenAsync(authResult?.AccessToken);

                var token = await GetEzgoTokenFromMsalTokenAsync(username, authResult.IdToken);

                return await SetupProfileFromTokenAsync(token, authResult.Account.Username);
            }
            catch (MsalUiRequiredException)
            {
                return SignInResult.Failed;
            }
        }

        public async Task<bool> SignOutAsync(string username)
        {
            try
            {
                var account = (await _pca.GetAccountsAsync())
                    .Where(acc => acc.Username == username)
                    .FirstOrDefault();

                if (account != null)
                {
                    await _pca.RemoveAsync(account);
                }

                // Clear our access token from secure storage.
                Settings.RemoveMsalAccessToken();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AuthenticationMethod> IsMsalAsync(string username)
        {
            HttpDelegatingHandlerBase.SendToken = false;
            var response = await _apiClient.PostAsync("authentication/external/check", username);
            HttpDelegatingHandlerBase.SendToken = true;

            if (response.IsSuccessStatusCode)
            {
                var method = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
#if DEBUG
                Debug.WriteLine($"[MSAL] Received authentication method for \"{username}\": {method}\r\n");
#endif
                if (method == "MSAL")
                {
                    return AuthenticationMethod.MSAL;
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotAcceptable) // HTTP 406
            {
                return AuthenticationMethod.Credentials;
            }

            return AuthenticationMethod.Credentials;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Posts credentials to the API to get a token back.
        /// </summary>
        /// <param name="username">User's username.</param>
        /// <param name="password">User's password.</param>
        /// <returns>Bearer token or <see langword="null"/> if the request failed.</returns>
        private async Task<string> PostCredentialsAsync(string username, string password)
        {
            var loginValues = new
            {
                username,
                password
            };

            HttpDelegatingHandlerBase.SendToken = false;
            HttpResponseMessage result = await _apiClient.PostAsync<object>("authentication/login", loginValues);
            HttpDelegatingHandlerBase.SendToken = true;

            string output = null;
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = await result.Content.ReadAsJsonAsync<string>();
                if (token != null)
                {
                    output = token;
                }
            }
            return output;
        }

        /// <summary>
        /// Verifies MSAL token and tries to retrive bearer token from the API. 
        /// </summary>
        /// <returns>Bearer token or <see langword="null"/> if the request failed.</returns>
        private async Task<string> GetEzgoTokenFromMsalTokenAsync(string username, string idToken)
        {
            var msalToken = await Settings.GetMsalAccessTokenAsync();

            HttpDelegatingHandlerBase.SendToken = false;
            var msalTokenAuto = new MsalTokenAuto(username, msalToken, idToken);
            var response = await _apiClient.PostAsync("authentication/external/login/auto", msalTokenAuto);
            HttpDelegatingHandlerBase.SendToken = true;

            if (response.IsSuccessStatusCode)
            {
                var a = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
#if DEBUG
                Debug.WriteLine($"[MSAL] EZGO token received: {token}\r\n");
#endif
                return token;
            }

            return null;
        }

        /// <summary>
        /// Sets user's profile using the provided API token.
        /// </summary>
        /// <param name="jwtToken">JWT token for the API.</param>
        /// <param name="username">Username to cache.</param>
        /// <returns>Result of the proccess.</returns>
        private async Task<SignInResult> SetupProfileFromTokenAsync(string jwtToken, string username)
        {
            bool isUserProfileExist = await GetUserProfileWithTokenAsync(jwtToken);
            if (isUserProfileExist)
            {
                // Clean up last session
                _fileService.ClearInternalStorageFolder(Constants.SessionDataDirectory);

                UserSettings.Username = username;

                await AddLocalUsernameAsync(username);

                await MainThread.InvokeOnMainThreadAsync(() => { MessagingCenter.Send(this, Constants.UserHasChanged); });

                return SignInResult.Ok;
            }
            else
                return SignInResult.Failed;
        }

        public async Task<bool> GetUserProfileWithTokenAsync(string jwtToken)
        {
#if DEBUG
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            System.Diagnostics.Debug.WriteLine("[LoginService::GetUserProfileWithTokenAsync]:: Started retriving user profile");
#endif
            bool result = false;

            if (!jwtToken.IsNullOrEmpty())
            {
                Settings.Token = jwtToken;

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken token = handler.ReadJwtToken(jwtToken);

                string djangoToken = token.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Sid))?.Value;

                UserProfileModel userProfile = await PostUserInfoAsync(djangoToken);

                if (userProfile != null)
                {
                    var settings = Task.Run(() => _settingsService.GetApplicationSettingsAsync());
                    var second = Task.Run(async () =>
                   {
                       UserPrefsModel prefs = await _userService.GetLocalUserPrefsAsync(userProfile.Id);

                       UserSettings.Id = userProfile.Id;
                       UserSettings.Firstname = userProfile.FirstName;
                       UserSettings.Lastname = userProfile.LastName;
                       UserSettings.Fullname = userProfile.FullName;
                       UserSettings.Role = userProfile.Role;
                       UserSettings.RoleType = userProfile.RoleEnum;
                       UserSettings.UserPictureUrl = userProfile.Picture;
                       UserSettings.Email = userProfile.Email;
                       UserSettings.CompanyLogoUrl = userProfile.Company.Picture;
                       UserSettings.CompanyName = userProfile.Company.Name;
                       UserSettings.CompanyId = userProfile.Company.Id;

                       Settings.ReportInterval = prefs?.ReportPeriod ?? Settings.ReportInterval;
                   });

                    await Task.WhenAll(second, settings);

                    result = true;
                }
                else
                    Settings.Token = null;
            }
#if DEBUG
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[LoginService::GetUserProfileWithTokenAsync]:: End {stopwatch.ElapsedMilliseconds}");
#endif
            return result;
        }

        private async Task SaveLocalUsernamesAsync(List<string> usernames)
        {
            string usernamesJson = JsonSerializer.Serialize(usernames);

            await _fileService.SaveFileToInternalStorageAsync(usernamesJson, usernamesFilename, usernamesDirectoryName);
        }

        private async Task<UserProfileModel> PostUserInfoAsync(string token)
        {
#if DEBUG
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            System.Diagnostics.Debug.WriteLine("[LoginService::PostUserInfoAsync]:: Started retriving user info");
#endif
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            HttpResponseMessage result = await _apiClient.PostAsync("userprofile?include=company", token.Trim());

            UserProfileModel output = null;
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var userInfo = JsonSerializer.Deserialize<UserProfileModel>(await result.Content.ReadAsStringAsync());

                if (userInfo != null)
                {
                    output = userInfo;
                }
            }
#if DEBUG
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[LoginService::PostUserInfoAsync]:: Posting userProfile took: {stopwatch.ElapsedMilliseconds}");
#endif
            return output;
        }

        public void Dispose()
        {
            _apiClient.Dispose();
            //_fileService.di
            //_userService.di;
            _settingsService.Dispose();
        }
        #region Saved accounts

        public async Task<List<string>> GetLocalUsernamesAsync()
        {
            List<string> result = new List<string>();

            string usernamesJson = await _fileService.ReadFromInternalStorageAsync(usernamesFilename, usernamesDirectoryName);

            if (!string.IsNullOrWhiteSpace(usernamesJson))
                result = JsonConvert.DeserializeObject<List<string>>(usernamesJson) ?? new List<string>();

            return result;
        }

        public async Task AddLocalUsernameAsync(string username)
        {
            List<string> usernames = await GetLocalUsernamesAsync();

            if (!usernames.Contains(username))
            {
                usernames.Add(username);
                await SaveLocalUsernamesAsync(usernames);
            }
        }

        #endregion 

        #endregion
    }
}
