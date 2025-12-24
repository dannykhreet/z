using EZGO.Api.Models.Setup;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// CompanyValidators; contains all validation methods for validating companies and values of the companies.
    /// </summary>
    public static class CompanyValidators
    {
        public const string MESSAGE_COMPANY_ID_IS_NOT_VALID = "CompanyId is not valid";
        public const string MESSAGE_COMPANY_ID_AND_ASKED_COMPANY_ID_HAS_ACCESS = "Current user has no access to asked companyid";
        public const string MESSAGE_COMPANY_ALREADY_EXISTS = "Company already exists";

        public static bool CompanyIdIsValid(int companyid)
        {
            if (companyid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool HasAccessToCompanyId(int companyid, int askedcompanyid)
        {
            return (companyid == askedcompanyid);
        }

        /// <summary>
        /// SetupCompanyIsValid; Validate setup company.
        /// </summary>
        /// <param name="company">Company contain all data.</param>
        /// <returns>Returns message if failed</returns>
        public static string SetupCompanyIsValidAndOrSetDefaults(SetupCompany company)
        {
            var messageBuilder = new StringBuilder();

            if (company == null)
            {
                messageBuilder.Append("Company is not valid or empty;");
            }
            else
            {
                if (string.IsNullOrEmpty(company.Name))
                {
                    messageBuilder.AppendLine("Company name is not valid or empty.");
                }
                if (string.IsNullOrEmpty(company.PrimaryUserName))
                {
                    messageBuilder.AppendLine("Company primary username is not valid or empty.");
                }
                if (string.IsNullOrEmpty(company.PrimaryUserPassword) && PasswordValidators.PasswordIsValid(company.PrimaryUserPassword))
                {
                    messageBuilder.AppendLine("Company primary user password is not valid or empty.");
                }
                if (string.IsNullOrEmpty(company.TierLevel))
                {
                    company.TierLevel = "essential";
                }
                if (string.IsNullOrEmpty(company.TimeZone))
                {
                    company.TimeZone = "Europe/Amsterdam";
                }
                if (string.IsNullOrEmpty(company.Locale))
                {
                    company.Locale = "en-us";
                }
            }

            return messageBuilder.ToString(); ;
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.CompanyRoles companyRoles, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (companyRoles == null)
                {
                    succes = false;
                }

                if (!string.IsNullOrEmpty(companyRoles.BasicDisplayName)) companyRoles.BasicDisplayName = TextValidator.StripRogueDataFromText(companyRoles.BasicDisplayName);
                if (!string.IsNullOrEmpty(companyRoles.ManagerDisplayName)) companyRoles.BasicDisplayName = TextValidator.StripRogueDataFromText(companyRoles.ManagerDisplayName);
                if (!string.IsNullOrEmpty(companyRoles.ShiftLeaderDisplayName)) companyRoles.ShiftLeaderDisplayName = TextValidator.StripRogueDataFromText(companyRoles.ShiftLeaderDisplayName);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Setup.SetupCompany setupCompany, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (setupCompany == null)
                {
                    succes = false;
                }
                if (succes && !string.IsNullOrEmpty(setupCompany.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(setupCompany.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", setupCompany.Picture));
                }
                if (!string.IsNullOrEmpty(setupCompany.Description)) setupCompany.Description = TextValidator.StripRogueDataFromText(setupCompany.Description);
                if (!string.IsNullOrEmpty(setupCompany.Locale)) setupCompany.Locale = TextValidator.StripRogueDataFromText(setupCompany.Locale);
                if (!string.IsNullOrEmpty(setupCompany.Name)) setupCompany.Name = TextValidator.StripRogueDataFromText(setupCompany.Name);
                if (!string.IsNullOrEmpty(setupCompany.PrimaryFirstName)) setupCompany.PrimaryFirstName = TextValidator.StripRogueDataFromText(setupCompany.PrimaryFirstName);
                if (!string.IsNullOrEmpty(setupCompany.PrimaryLastName)) setupCompany.PrimaryLastName = TextValidator.StripRogueDataFromText(setupCompany.PrimaryLastName);
                if (!string.IsNullOrEmpty(setupCompany.PrimaryUserName)) setupCompany.PrimaryUserName = TextValidator.StripRogueDataFromText(setupCompany.PrimaryUserName);
                if (!string.IsNullOrEmpty(setupCompany.PrimaryUserPassword)) setupCompany.PrimaryUserPassword = TextValidator.StripRogueDataFromText(setupCompany.PrimaryUserPassword);
                if (!string.IsNullOrEmpty(setupCompany.ShiftDays)) setupCompany.ShiftDays = TextValidator.StripRogueDataFromText(setupCompany.ShiftDays);
                if (!string.IsNullOrEmpty(setupCompany.ShiftStartTime)) setupCompany.ShiftStartTime = TextValidator.StripRogueDataFromText(setupCompany.ShiftStartTime);
                if (!string.IsNullOrEmpty(setupCompany.TierLevel)) setupCompany.TierLevel = TextValidator.StripRogueDataFromText(setupCompany.TierLevel);
                if (!string.IsNullOrEmpty(setupCompany.TimeZone)) setupCompany.TimeZone = TextValidator.StripRogueDataFromText(setupCompany.TimeZone);

                //settings if being processed with company processing (normally settings handled separately):
                if (!string.IsNullOrEmpty(setupCompany.Country)) setupCompany.Country = TextValidator.StripRogueDataFromText(setupCompany.Country);
                if (!string.IsNullOrEmpty(setupCompany.Coords)) setupCompany.Coords = TextValidator.StripRogueDataFromText(setupCompany.Coords);


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

        public static bool ValidateAndClean(this EZGO.Api.Models.Setup.SetupCompanySettings setupCompanySettings, int companyid, out string messages)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (!(companyid > 0))
                {
                    succes = false;
                }

                if (setupCompanySettings == null)
                {
                    succes = false;
                }
                if (!string.IsNullOrEmpty(setupCompanySettings.Locale)) setupCompanySettings.Locale = TextValidator.StripRogueDataFromText(setupCompanySettings.Locale);
                if (!string.IsNullOrEmpty(setupCompanySettings.TierLevel)) setupCompanySettings.TierLevel = TextValidator.StripRogueDataFromText(setupCompanySettings.TierLevel);
                if (!string.IsNullOrEmpty(setupCompanySettings.TimeZone)) setupCompanySettings.TimeZone = TextValidator.StripRogueDataFromText(setupCompanySettings.TimeZone);

                if (!string.IsNullOrEmpty(setupCompanySettings.Country)) setupCompanySettings.Country = TextValidator.StripRogueDataFromText(setupCompanySettings.Country);
                if (!string.IsNullOrEmpty(setupCompanySettings.Coords)) setupCompanySettings.Coords = TextValidator.StripRogueDataFromText(setupCompanySettings.Coords);

                if (!string.IsNullOrEmpty(setupCompanySettings.IpRestrictionList)) setupCompanySettings.IpRestrictionList = TextValidator.StripRogueDataFromText(setupCompanySettings.IpRestrictionList);
                if (!string.IsNullOrEmpty(setupCompanySettings.VirtualTeamLeadModules)) setupCompanySettings.VirtualTeamLeadModules = TextValidator.StripRogueDataFromText(setupCompanySettings.VirtualTeamLeadModules);
                if (!string.IsNullOrEmpty(setupCompanySettings.TranslationModules)) setupCompanySettings.TranslationModules = TextValidator.StripRogueDataFromText(setupCompanySettings.TranslationModules);
                if (!string.IsNullOrEmpty(setupCompanySettings.TranslationLanguages)) setupCompanySettings.TranslationLanguages = TextValidator.StripRogueDataFromText(setupCompanySettings.TranslationLanguages);

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

        public static bool ValidateAndClean(this EZGO.Api.Models.Company company, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (company == null)
                {
                    succes = false;
                }
                if (succes && !string.IsNullOrEmpty(company.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(company.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", company.Picture));
                }
                if (!string.IsNullOrEmpty(company.Description)) company.Description = TextValidator.StripRogueDataFromText(company.Description);
                if (!string.IsNullOrEmpty(company.Name)) company.Name = TextValidator.StripRogueDataFromText(company.Name);
                if (!string.IsNullOrEmpty(company.ManagerName)) company.ManagerName = TextValidator.StripRogueDataFromText(company.ManagerName);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Holding holding, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (holding == null)
                {
                    succes = false;
                }
                if (succes && !string.IsNullOrEmpty(holding.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(holding.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", holding.Picture));
                }
                if (!string.IsNullOrEmpty(holding.Description)) holding.Description = TextValidator.StripRogueDataFromText(holding.Description);
                if (!string.IsNullOrEmpty(holding.Name)) holding.Name = TextValidator.StripRogueDataFromText(holding.Name);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.HoldingUnit holding, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (holding == null)
                {
                    succes = false;
                }
                if (succes && !string.IsNullOrEmpty(holding.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(holding.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", holding.Picture));
                }
                if (!string.IsNullOrEmpty(holding.Description)) holding.Description = TextValidator.StripRogueDataFromText(holding.Description);
                if (!string.IsNullOrEmpty(holding.Name)) holding.Name = TextValidator.StripRogueDataFromText(holding.Name);
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

        /*
         
         
CompanyRoles
SetupCompany
Company
Holding
HoldingUnit
         
         */

    }
}
