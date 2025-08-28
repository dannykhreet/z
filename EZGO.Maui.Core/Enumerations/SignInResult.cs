using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Enumerations
{
    public enum SignInResult
    {
        /// <summary>
        /// Sign in successful
        /// </summary>
        Ok,

        /// <summary>
        /// Incorrect credentials
        /// </summary>
        IncorrectCredentials,

        /// <summary>
        /// TBD
        /// </summary>
        LinkedAccountNotFound,

        /// <summary>
        /// Sign in failed for different reason
        /// </summary>
        Failed,

        /// <summary>
        /// Sign in cancelled
        /// </summary>
        Canceled,
    }
}
