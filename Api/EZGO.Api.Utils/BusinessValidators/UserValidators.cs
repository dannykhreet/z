using EZGO.Api.Models.Users;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// UserValidators; contains all validation methods for validating users and values of the users.
    /// </summary>
    public static class UserValidators
    {
        public const string MESSAGE_USER_ID_IS_NOT_VALID = "UserId is not valid";
        public const string MESSAGE_USER_TOKEN_IS_NOT_VALID = "User is not valid";
        public static bool UserIdIsValid(int userid)
        {
            if (userid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool UserTokenIsValid(string usertoken)
        {
            if (!string.IsNullOrEmpty(usertoken)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// ValidateUserLogin; Checks if username and password are not empty.
        /// </summary>
        /// <param name="username">Username string</param>
        /// <param name="password">Password string</param>
        /// <returns>true when both not empty.</returns>
        public static bool ValidateUserLogin(string username, string password)
        {
            if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty (password))
            {
                return true;
            }
            return false;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.UserProfile> userProfiles, int companyId)
        {
            return !(userProfiles.Where(x => x.Company?.Id != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.UserProfile userProfile, int companyId)
        {
            return (userProfile.Company?.Id == companyId);
        }

        /// <summary>
        /// ValidateAndClean; 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static bool ValidateAndClean(this EZGO.Api.Models.UserProfile user, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (user == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("User is not valid or empty;");
                }
                if (succes && user.Company?.Id > 0)
                {
                    succes = CompanyConnectionIsValid(userProfile: user, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && !string.IsNullOrEmpty(user.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(user.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", user.Picture));
                }

                if (succes && !string.IsNullOrEmpty(user.Email)) { user.Email = TextValidator.StripRogueDataFromText(user.Email, ignoreEmailChar: true); }
                if (succes && !string.IsNullOrEmpty(user.FirstName)) { user.FirstName = TextValidator.StripRogueDataFromText(user.FirstName); }
                if (succes && !string.IsNullOrEmpty(user.LastName)) { user.LastName = TextValidator.StripRogueDataFromText(user.LastName); }
                if (succes && !string.IsNullOrEmpty(user.Role)) { user.Role = TextValidator.StripRogueDataFromText(user.Role); }
                if (succes && !string.IsNullOrEmpty(user.UPN)) { user.UPN = TextValidator.StripRogueDataFromText(user.UPN, ignoreEmailChar:true, ignoreMinusChar: true); }
                if (succes && !string.IsNullOrEmpty(user.UserName)) { user.UserName = TextValidator.StripRogueDataFromText(user.UserName, ignoreEmailChar: true, ignoreMinusChar: true); }
                if (succes && !string.IsNullOrEmpty(user.SapPmUsername)) { user.SapPmUsername = TextValidator.StripRogueDataFromText(user.SapPmUsername, ignoreMinusChar: true); }
            }
            catch (Exception ex)
            {
                succes = false;
                messageBuilder.AppendLine(string.Format("Error occured [{0}].", ex.Message));
            }

            messages = messageBuilder.ToString();

            messageBuilder.Clear();
            messageBuilder = null;

            return succes;
        }

        /// <summary>
        /// Validates the provided <see cref="UserExtendedDetails"/> object and cleans its text fields by removing
        /// invalid or rogue data.
        /// </summary>
        /// <remarks>This method performs the following operations: <list type="bullet">
        /// <item><description>Validates that the <paramref name="userExtendedDetails"/> object is not <see
        /// langword="null"/> and has a valid <c>UserId</c>.</description></item> <item><description>Strips rogue or
        /// invalid data from the <c>Bio</c>, <c>Description</c>, <c>EmployeeFunction</c>, and <c>EmployeeId</c> fields,
        /// if they are not <see langword="null"/> or empty.</description></item> <item><description>Populates the
        /// <paramref name="messages"/> output parameter with any validation errors encountered.</description></item>
        /// </list> If an exception occurs during processing, the method returns <see langword="false"/> and appends the
        /// exception message to <paramref name="messages"/>.</remarks>
        /// <param name="userExtendedDetails">The <see cref="UserExtendedDetails"/> object to validate and clean. Cannot be <see langword="null"/>.</param>
        /// <param name="companyId">The ID of the company associated with the user. This parameter is currently unused but reserved for future
        /// use.</param>
        /// <param name="userId">The ID of the user associated with the <paramref name="userExtendedDetails"/> object.</param>
        /// <param name="messages">When the method returns, contains a semicolon-separated list of validation error messages if validation
        /// fails, or an empty string if validation succeeds.</param>
        /// <returns><see langword="true"/> if the <paramref name="userExtendedDetails"/> object is valid and its text fields
        /// have been cleaned; otherwise, <see langword="false"/>.</returns>
        public static bool ValidateAndClean(this UserExtendedDetails userExtendedDetails, int companyId, int userId, out string messages)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (userExtendedDetails == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("User extended details is not valid or empty;");
                }

                if (succes && userExtendedDetails.UserId <= 0)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("User extended details user connection is not valid;");
                }

                if (succes && !string.IsNullOrEmpty(userExtendedDetails.Bio)) { userExtendedDetails.Bio = TextValidator.StripRogueDataFromText(userExtendedDetails.Bio); }
                if (succes && !string.IsNullOrEmpty(userExtendedDetails.Description)) { userExtendedDetails.Description = TextValidator.StripRogueDataFromText(userExtendedDetails.Description); }
                if (succes && !string.IsNullOrEmpty(userExtendedDetails.EmployeeFunction)) { userExtendedDetails.EmployeeFunction = TextValidator.StripRogueDataFromText(userExtendedDetails.EmployeeFunction); }    
                if (succes && !string.IsNullOrEmpty(userExtendedDetails.EmployeeId)) { userExtendedDetails.EmployeeId = TextValidator.StripRogueDataFromText(userExtendedDetails.EmployeeId, ignoreMinusChar: true); }
            }
            catch (Exception ex)
            {
                succes = false;
                messageBuilder.AppendLine(string.Format("Error occured [{0}].", ex.Message));
            }

            messages = messageBuilder.ToString();

            messageBuilder.Clear();
            messageBuilder = null;

            return succes;
        }

    }
}
