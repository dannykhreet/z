using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// AuditTempalteValidators; contains all validation methods for validating audittemplates and values of the audittemplates.
    /// </summary>
    public static class AuditTemplateValidators
    {
        public const string MESSAGE_AUDITTEMPLATE_IS_NOT_VALID = "AuditTemplate is not valid. ScoreType is not correct.";

        public static bool AuditTemplateIsValid(AuditTemplate auditTemplate)
        {
            bool output = true;

            if (auditTemplate.ScoreType != ScoreTypeEnum.Score.ToString().ToLower() && auditTemplate.ScoreType != ScoreTypeEnum.Thumbs.ToString().ToLower()
                ) output = false;



            return output;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.AuditTemplate> auditTemplates, int companyId)
        {
            return !(auditTemplates.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.AuditTemplate auditTemplate, int companyId)
        {
            return (auditTemplate.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.AuditTemplate auditTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (auditTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("AuditTemplate is not valid or empty;");
                }
                if (succes && auditTemplate.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(auditTemplate: auditTemplate, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes)
                {
                    succes = auditTemplate.MaxScore > 0 && auditTemplate.MaxScore > auditTemplate.MinScore;
                    if (!succes) messageBuilder.AppendLine($"{nameof(auditTemplate.MaxScore)} must be greater than zero and greater than {nameof(auditTemplate.MinScore)}.");
                }
                if (succes && auditTemplate.TaskTemplates != null && auditTemplate.TaskTemplates.Count > 0)
                {
                    foreach (var item in auditTemplate.TaskTemplates)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }
                if (succes && auditTemplate.Properties != null && auditTemplate.Properties.Count > 0)
                {
                    foreach (PropertyAuditTemplate prop in auditTemplate.Properties)
                    {
                        if (succes)
                        {
                            succes = prop.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }
                if (succes && auditTemplate.OpenFieldsProperties != null && auditTemplate.OpenFieldsProperties.Count > 0)
                {
                    foreach (PropertyAuditTemplate prop in auditTemplate.OpenFieldsProperties)
                    {
                        if (succes)
                        {
                            succes = prop.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }
                if (succes && !string.IsNullOrEmpty(auditTemplate.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(auditTemplate.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", auditTemplate.Picture));
                }
                auditTemplate.Name = TextValidator.StripRogueDataFromText(auditTemplate.Name);
                if (!string.IsNullOrEmpty(auditTemplate.Description)) auditTemplate.Description = TextValidator.StripRogueDataFromText(auditTemplate.Description);
                if (!string.IsNullOrEmpty(auditTemplate.Role)) auditTemplate.Role = TextValidator.StripRogueDataFromText(auditTemplate.Role);
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
