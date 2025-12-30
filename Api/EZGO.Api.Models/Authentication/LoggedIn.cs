using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Authentication
{
    /// <summary>
    /// LoggedIn; LoggedIn object used after authentication and further login procedures.
    /// </summary>
    public class LoggedIn
    {
        /// <summary>
        /// CompanyId; Company id of the user logged in.
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// UserId; Userid of the user.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Token; token used for accessing the system.
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// IsValidLogin, basic check if fields are filled so authentication can continue. 
        /// </summary>
        public bool IsValidLogin { get { return this.CompanyId > 0 && this.UserId > 0 && !string.IsNullOrEmpty(this.Token) && this.Token.Length > 10; } }
    }
}
