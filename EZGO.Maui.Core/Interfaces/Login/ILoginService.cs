using System;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Login
{
    public interface ILoginService : IDisposable
    {
        /// <summary>
        /// Signs the user in using internal EZGO account.
        /// </summary>
        /// <param name="username">User's name.</param>
        /// <param name="password">User's password.</param>
        /// <returns>Result of the operation as an enum.</returns>
        Task<SignInResult> SignInWithCredentialsAsync(string username, string password);

        /// <summary>
        /// Signs the user in using MSAL authentication system.
        /// </summary>
        /// <param name="username">User's name.</param>
        /// <returns>Result of the operation as an enum.</returns>
        Task<SignInResult> SignInMsalAsync(string username);

        /// <summary>
        /// Attempts to silently sign the user in using cached MSAL access token.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>Result of the operation as an enum.</returns>
        Task<SignInResult> SilentSignInMsalAsync(string username);

        /// <summary>
        /// Signs the user out asynchronously
        /// </summary>
        /// <param name="userName">Username of the user to be signed out.</param>
        /// <returns>TBD</returns>
        Task<bool> SignOutAsync(string userName);

        /// <summary>
        /// Check if authentication method for this user is MSAL.
        /// </summary>
        /// <param name="username">User's name</param>
        /// <returns>Authentication method for the given user</returns>
        Task<AuthenticationMethod> IsMsalAsync(string username);

        /// <summary>
        /// Gets localy saved names of users that signed in the past.
        /// </summary>
        /// <returns>List of user names.</returns>
        Task<List<string>> GetLocalUsernamesAsync();

        Task<bool> GetUserProfileWithTokenAsync(string jwtToken);

    }
}
