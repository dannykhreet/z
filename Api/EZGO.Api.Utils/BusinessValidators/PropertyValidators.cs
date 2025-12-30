using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class PropertyValidators
    {
        public static bool CompanyConnectionIsValid(List<Models.PropertyValue.PropertyUserValue> propertyUserValues, int companyId)
        {
            return !(propertyUserValues.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(Models.PropertyValue.PropertyUserValue propertyUserValue, int companyId)
        {
            return (propertyUserValue.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.PropertyValue.PropertyUserValue propertyUserValue, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (propertyUserValue == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("PropertyUserValue is not valid or empty;");
                }
                if (succes && propertyUserValue.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(propertyUserValue: propertyUserValue, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && propertyUserValue.UserId > 0 && !ignoreCreatedByCheck)
                {
                    succes = propertyUserValue.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(propertyUserValue.UserId);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
                }
                if (!string.IsNullOrEmpty(propertyUserValue.UserValueString)) propertyUserValue.UserValueString = TextValidator.StripRogueDataFromText(propertyUserValue.UserValueString);
                if (!string.IsNullOrEmpty(propertyUserValue.UserValueTime)) propertyUserValue.UserValueTime = TextValidator.StripRogueDataFromText(propertyUserValue.UserValueTime);
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

        public static bool ValidateAndClean(this PropertyDTO property, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (property == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("PropertyUserValue is not valid or empty;");
                }
                if (succes && property.UserValue.User.Id > 0 && !ignoreCreatedByCheck)
                {
                    succes = property.UserValue.User.Id == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(userId);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
                }
                if (!string.IsNullOrEmpty(property.UserValue.UserValue))
                {
                    property.UserValue.UserValue = TextValidator.StripRogueDataFromText(property.UserValue.UserValue);
                }
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

        public static bool ValidateAndClean(this EZGO.Api.Models.PropertyValue.PropertyTaskTemplate propertyTaskTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (propertyTaskTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("PropertyTaskTemplate is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(propertyTaskTemplate.CustomDisplayType)) propertyTaskTemplate.CustomDisplayType = TextValidator.StripRogueDataFromText(propertyTaskTemplate.CustomDisplayType);
                if (!string.IsNullOrEmpty(propertyTaskTemplate.PrimaryStringValue)) propertyTaskTemplate.PrimaryStringValue = TextValidator.StripRogueDataFromText(propertyTaskTemplate.PrimaryStringValue);
                if (!string.IsNullOrEmpty(propertyTaskTemplate.SecondaryStringValue)) propertyTaskTemplate.SecondaryStringValue = TextValidator.StripRogueDataFromText(propertyTaskTemplate.SecondaryStringValue);
                if (!string.IsNullOrEmpty(propertyTaskTemplate.PrimaryTimeValue)) propertyTaskTemplate.PrimaryTimeValue = TextValidator.StripRogueDataFromText(propertyTaskTemplate.PrimaryTimeValue);
                if (!string.IsNullOrEmpty(propertyTaskTemplate.SecondaryTimeValue)) propertyTaskTemplate.SecondaryTimeValue = TextValidator.StripRogueDataFromText(propertyTaskTemplate.SecondaryTimeValue);
                if (!string.IsNullOrEmpty(propertyTaskTemplate.TitleDisplay)) propertyTaskTemplate.TitleDisplay = TextValidator.StripRogueDataFromText(propertyTaskTemplate.TitleDisplay);
                if (!string.IsNullOrEmpty(propertyTaskTemplate.PropertyValueDisplay)) propertyTaskTemplate.PropertyValueDisplay = TextValidator.StripRogueDataFromText(propertyTaskTemplate.PropertyValueDisplay);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.PropertyValue.PropertyChecklistTemplate propertyChecklistTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (propertyChecklistTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("PropertyChecklistTemplate is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(propertyChecklistTemplate.CustomDisplayType)) propertyChecklistTemplate.CustomDisplayType = TextValidator.StripRogueDataFromText(propertyChecklistTemplate.CustomDisplayType);
                if (!string.IsNullOrEmpty(propertyChecklistTemplate.PrimaryStringValue)) propertyChecklistTemplate.PrimaryStringValue = TextValidator.StripRogueDataFromText(propertyChecklistTemplate.PrimaryStringValue);
                if (!string.IsNullOrEmpty(propertyChecklistTemplate.SecondaryStringValue)) propertyChecklistTemplate.SecondaryStringValue = TextValidator.StripRogueDataFromText(propertyChecklistTemplate.SecondaryStringValue);
                if (!string.IsNullOrEmpty(propertyChecklistTemplate.PrimaryTimeValue)) propertyChecklistTemplate.PrimaryTimeValue = TextValidator.StripRogueDataFromText(propertyChecklistTemplate.PrimaryTimeValue);
                if (!string.IsNullOrEmpty(propertyChecklistTemplate.SecondaryTimeValue)) propertyChecklistTemplate.SecondaryTimeValue = TextValidator.StripRogueDataFromText(propertyChecklistTemplate.SecondaryTimeValue);
                if (!string.IsNullOrEmpty(propertyChecklistTemplate.TitleDisplay)) propertyChecklistTemplate.TitleDisplay = TextValidator.StripRogueDataFromText(propertyChecklistTemplate.TitleDisplay);
                if (!string.IsNullOrEmpty(propertyChecklistTemplate.PropertyValueDisplay)) propertyChecklistTemplate.PropertyValueDisplay = TextValidator.StripRogueDataFromText(propertyChecklistTemplate.PropertyValueDisplay);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.PropertyValue.PropertyAuditTemplate propertyAuditTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (propertyAuditTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("propertyAuditTemplate is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(propertyAuditTemplate.CustomDisplayType)) propertyAuditTemplate.CustomDisplayType = TextValidator.StripRogueDataFromText(propertyAuditTemplate.CustomDisplayType);
                if (!string.IsNullOrEmpty(propertyAuditTemplate.PrimaryStringValue)) propertyAuditTemplate.PrimaryStringValue = TextValidator.StripRogueDataFromText(propertyAuditTemplate.PrimaryStringValue);
                if (!string.IsNullOrEmpty(propertyAuditTemplate.SecondaryStringValue)) propertyAuditTemplate.SecondaryStringValue = TextValidator.StripRogueDataFromText(propertyAuditTemplate.SecondaryStringValue);
                if (!string.IsNullOrEmpty(propertyAuditTemplate.PrimaryTimeValue)) propertyAuditTemplate.PrimaryTimeValue = TextValidator.StripRogueDataFromText(propertyAuditTemplate.PrimaryTimeValue);
                if (!string.IsNullOrEmpty(propertyAuditTemplate.SecondaryTimeValue)) propertyAuditTemplate.SecondaryTimeValue = TextValidator.StripRogueDataFromText(propertyAuditTemplate.SecondaryTimeValue);
                if (!string.IsNullOrEmpty(propertyAuditTemplate.TitleDisplay)) propertyAuditTemplate.TitleDisplay = TextValidator.StripRogueDataFromText(propertyAuditTemplate.TitleDisplay);
                if (!string.IsNullOrEmpty(propertyAuditTemplate.PropertyValueDisplay)) propertyAuditTemplate.PropertyValueDisplay = TextValidator.StripRogueDataFromText(propertyAuditTemplate.PropertyValueDisplay);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.PropertyValue.Property property, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (property == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Property is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(property.Name)) property.Name = TextValidator.StripRogueDataFromText(property.Name);
                if (!string.IsNullOrEmpty(property.Description)) property.Description = TextValidator.StripRogueDataFromText(property.Description);
                if (!string.IsNullOrEmpty(property.ResourceKeyName)) property.ResourceKeyName = TextValidator.StripRogueDataFromText(property.ResourceKeyName);
                if (!string.IsNullOrEmpty(property.ShortName)) property.ShortName = TextValidator.StripRogueDataFromText(property.ShortName);
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
