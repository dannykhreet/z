using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Authentication
{
    /// <summary>
    /// Login; Used for login submitting. Used when switching user or logging in.
    /// Both UserName and Password are required for the login procedure. 
    /// </summary>
    public class Login
    {
        /// <summary>
        /// UserName; Username as filled in by user on login.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Password; Password as filled in by user on login.
        /// </summary>
        public string Password { get; set; }
    }
}
