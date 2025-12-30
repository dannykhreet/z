using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    /// <summary>
    /// AuthenticationSettings, Contains specific authentication settings related to the API.
    /// </summary>
    public static class AuthenticationSettings
    {
        /// <summary>
        /// MESSAGE_FAILED_LOGING; Basic message that is returned when login fails.
        /// </summary>
        public const string MESSAGE_FAILED_LOGIN = "Login failed. Unknown user or incorrect password.";
        /// <summary>
        /// MESSAGE_FAILED_APP_HAS_NO_ACCESS; Message when the application has no rights to access the API with this specific user or company
        /// </summary>
        public const string MESSAGE_FAILED_APP_HAS_NO_ACCESS = "Login failed. Company or user has access to this application. Please try again later or contact support.";
        /// <summary>
        /// SECURITY_TOKEN_CONFIG_KEY; AppSettings key for the Token (JWT) token encryption.
        /// </summary>
        public const string SECURITY_TOKEN_CONFIG_KEY = "AppSettings:Token";
        /// <summary>
        /// TOKEN_EXPERATION_IN_HOURS; Expiration time in hours when a normal JWT token will expire after creation.
        /// </summary>
        public const int TOKEN_EXPERATION_IN_HOURS = 16;
        /// <summary>
        /// PROTECTION_CONFIG_KEY; AppSettings key location (for use with standard encrypted used with .net core data-protection logic)
        /// </summary>
        public const string PROTECTION_CONFIG_KEY = "AppSettings:ProtectionKey";
        /// <summary>
        /// AUTHORIZATION_ADMINISTRATOR_ROLES; Basic administrator (EZfactory users) rules (for use with AuthorizeTag)
        /// </summary>
        public const string AUTHORIZATION_ADMINISTRATOR_ROLES = "superuser,staff";
        /// <summary>
        /// AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES; Manager role including the Administrator roles
        /// </summary>
        public const string AUTHORIZATION_MANAGER_ADMINISTRATOR_ROLES = "manager,superuser,staff";
        /// <summary>
        /// AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES; Shiftleader and manager rule including the Administrator roles.
        /// </summary>
        public const string AUTHORIZATION_SHIFTLEADER_MANAGER_ADMINISTRATOR_ROLES = "shift_leader,manager,superuser,staff";
        /// <summary>
        /// PASSWORD_VALIDATION_REGEX; Password regex; Only accepts a-z and numbers;
        /// </summary>
        public const string PASSWORD_VALIDATION_REGEX = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{6,}$";
        /// <summary>
        /// Represents the regular expression pattern used to validate passwords according to version 2 requirements.
        /// </summary>
        /// <remarks>The pattern enforces the following rules: 
        /// <list type="bullet"> 
        ///     <item> <description>At least one lowercase letter.</description> </item>
        ///     <item> <description>At least one uppercase letter.</description> </item>
        ///     <item> <description>At least one numeric digit.</description> </item> 
        ///     <item> <description>A minimum length of 12 characters.</description> </item> 
        /// </list> 
        /// This constant can be used to validate passwords in scenarios where strong password policies are required.</remarks>
        public const string PASSWORD_VALIDATION_REGEX_V2 = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{12,}$";
        /// <summary>
        /// Password regex with special characters.
        /// At least one upper case English letter, (?=.*?[A-Z])
        /// At least one lower case English letter, (?=.*?[a - z])
        /// At least one digit, (?=.*?[0 - 9])
        /// At least one special character, (?=.*?[#?!@$%^&*-])
        /// Minimum eight in length.{8,}
        /// </summary>
        public const string PASSWORD_VALIDATION_REGEX_WITH_SPEC_CHARS = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
        /// <summary>
        /// MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN; Message when token expired, is not available and/or possible login on other device.
        /// </summary>
        public const string MESSAGE_UNAUTHORIZED_ACCESS_POSSIBLE_LOGIN = "Unauthorized. User logged in on other device or user session has expired.";
        /// <summary>
        /// MESSAGE_UNAUTHORIZED_ACCESS; Message when token expired or is not available .
        /// </summary>
        public const string MESSAGE_UNAUTHORIZED_ACCESS = "Unauthorized. User session is not valid or expired.";
        /// <summary>
        /// MESSAGE_FORBIDDEN_OBJECT; Message when user tries to access a object that is not part of the users company or has rights to
        /// </summary>
        public const string MESSAGE_FORBIDDEN_OBJECT = "Forbidden. User has no rights to access object.";
        /// <summary>
        /// MESSAGE_FORBIDDENCOMPANY_OBJECT; Message when user tries to update a object and has no rights to the company (or company is 0)
        /// </summary>
        public const string MESSAGE_FORBIDDENCOMPANY_OBJECT = "Forbidden. Company unknown or user has no rights to update company data.";


    }
}
