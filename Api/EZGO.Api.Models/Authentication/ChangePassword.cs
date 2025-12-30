using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Authentication
{
    /// <summary>
    /// ChangePassword; Change the current password model. For use with change password functionality.
    /// </summary>
    public class ChangePassword
    {
        /// <summary>
        /// Normally only a user can change it's current password. This Id's will be loaded from the authentication token. And only needs to be filled based if used from a management portal.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// CompanyId only a user can change it's current password. This Id's will be loaded from the authentication token. And only needs to be filled based if used from a management portal.
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// CurrentPassword; Current password of the user. 
        /// </summary>
        public string CurrentPassword { get; set; }
        /// <summary>
        /// NewPassword; New password of the user
        /// </summary>
        public string NewPassword { get; set; }
        /// <summary>
        /// NewPasswordValidation; New password validator, which should be the same as NewPassword.
        /// </summary>
        public string NewPasswordValidation { get; set; }
    }
}
