using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Authentication
{
    /// <summary>
    /// ExternalLogin; External login structure.
    /// Used for external login system (MSAL)
    /// </summary>
    public class ExternalLogin
    {
        /// <summary>
        /// UserName used by user.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// AccessToken that is used.
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Possible ID token as recieved by external party. 
        /// </summary>
        public string IdToken { get; set; }
        /// <summary>
        /// ExternalIdentifier1; Used for validations and reference purposes. 
        /// </summary>
        public string ExternalIdentifier1 { get; set; }
        /// <summary>
        /// ExternalIdentifier2; Used for validations and reference purposes. 
        /// </summary>
        public string ExternalIdentifier2 { get; set; }
    }
}
