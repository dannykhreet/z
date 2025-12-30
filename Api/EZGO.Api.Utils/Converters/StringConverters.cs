using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Converters
{
    public static class StringConverters
    {
        /// <summary>
        /// ConvertAuthorizationHeaderToToken; convert a string from a header to a bare token value.
        /// NOTE! only use with strings that contain tokens.
        /// NOTE! both Token and Bearer are stripped.
        /// </summary>
        /// <param name="authorizationheadervalue">The string contained in the authorization header.</param>
        /// <returns>A stripped string without Token indicator.</returns>
        public static string ConvertAuthorizationHeaderToToken(string authorizationheadervalue)
        {
            return authorizationheadervalue.Replace("Token ", string.Empty).Replace("Bearer ", string.Empty);
        }
    }
}
