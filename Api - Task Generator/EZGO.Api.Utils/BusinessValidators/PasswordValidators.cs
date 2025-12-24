using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// PasswordValidators; Contains validation methods for password.
    /// </summary>
    public static class PasswordValidators
    {
        public const string MESSAGE_PASSWORD_IS_NOT_VALID = "Password is not valid.";
        public const string MESSAGE_NEW_PASSWORD_IS_NOT_VALID = "New password is not valid.";
        public const string MESSAGE_PASSWORD_AND_NEW_PASSWORD_SAME = "The current password and new password are the same.";
        public const string MESSAGE_NEW_PASSWORD_AND_NEW_PASSWORD_VALIDATION_NOT_SAME = "The new password and the password confirmation do not match.";
        public static bool PasswordIsValid(string password)
        {
            return !string.IsNullOrEmpty(password) && Regex.IsMatch(password, Settings.AuthenticationSettings.PASSWORD_VALIDATION_REGEX);
        }

        public static bool PasswordNewPasswordIsValid(string password, string newPassword)
        {
            return password != newPassword;
        }

        public static bool NewPasswordNewPasswordConfirmationIsValid(string newPassword, string newPasswordConfirmation) {
            return newPassword == newPasswordConfirmation;
        }


    }
}
