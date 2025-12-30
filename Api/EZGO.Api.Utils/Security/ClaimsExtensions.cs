using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace EZGO.Api.Utils.Security
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// GetClaim; Get claim based on ClaimType. Will return the value. If it's not available it will return a empty string.
        /// </summary>
        /// <param name="claimsPrincipal">Principal that is being used. (normally the User contained in the controller)</param>
        /// <param name="claimType">ClaimType based on System.Security.ClaimType class and properties.</param>
        /// <returns>Claims value or empty string depending on outcome.</returns>
        public static string GetClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var currentClaim = claimsPrincipal.Claims.Where(c => c.Type == claimType.ToString()).FirstOrDefault();

            if (currentClaim != null)
            {
                return currentClaim.Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// GetClaim; Get claim based on ClaimType. Will return the value. If it's not available it will return a empty string.
        /// </summary>
        /// <param name="jwt">Principal that is being used. (normally the User contained in the controller)</param>
        /// <param name="claimType">ClaimType based on System.Security.ClaimType class and properties.</param>
        /// <returns>Claims value or empty string depending on outcome.</returns>
        public static string GetClaim(this JwtSecurityToken jwt, string claimType)
        {
            var currentClaim = jwt.Claims.Where(c => c.Type == claimType.ToString()).FirstOrDefault();

            if (currentClaim != null)
            {
                return currentClaim.Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// CheckClaim; Get claim based on ClaimType and value. Will return the value. If it's not available it will return a empty string.
        /// </summary>
        /// <param name="jwt">Principal that is being used. (normally the User contained in the controller)</param>
        /// <param name="claimType">ClaimType based on System.Security.ClaimType class and properties.</param>
        /// <returns></returns>
        public static string GetClaim(this JwtSecurityToken jwt, string claimType, string value)
        {
            var currentClaim = jwt.Claims.Where(c => c.Type == claimType.ToString() && c.Value == value).FirstOrDefault();

            if (currentClaim != null)
            {
                return currentClaim.Value;
            }

            return string.Empty;
        }
    }
}
