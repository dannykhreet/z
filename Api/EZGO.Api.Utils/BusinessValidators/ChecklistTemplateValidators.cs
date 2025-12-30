using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class ChecklistTemplateValidators
    {
        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.ChecklistTemplate> checklistTemplates, int companyId)
        {
            return !(checklistTemplates.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.ChecklistTemplate checklistTemplate, int companyId)
        {
            return (checklistTemplate.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.ChecklistTemplate checklistTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (checklistTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Checklist is not valid or empty;");
                }
                if (succes && checklistTemplate.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(checklistTemplate: checklistTemplate, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && checklistTemplate.TaskTemplates != null && checklistTemplate.TaskTemplates.Count > 0)
                {
                    foreach (var item in checklistTemplate.TaskTemplates)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && checklistTemplate.HasStages)
                {
                    foreach (StageTemplate stage in checklistTemplate.StageTemplates)
                    {
                        succes = stage.ValidateAndClean(companyId, userId, out var possibleMessages, validUserIds);
                        if (!succes) messageBuilder.Append(possibleMessages);
                    }
                }

                if (succes && checklistTemplate.Properties != null && checklistTemplate.Properties.Count > 0)
                {
                    foreach (PropertyChecklistTemplate prop in checklistTemplate.Properties)
                    {
                        if (succes)
                        {
                            succes = prop.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && checklistTemplate.OpenFieldsProperties != null && checklistTemplate.OpenFieldsProperties.Count > 0)
                {
                    foreach (PropertyChecklistTemplate prop in checklistTemplate.OpenFieldsProperties)
                    {
                        if (succes)
                        {
                            succes = prop.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }
                if (succes && !string.IsNullOrEmpty(checklistTemplate.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(checklistTemplate.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", checklistTemplate.Picture));
                }

                checklistTemplate.Name = TextValidator.StripRogueDataFromText(checklistTemplate.Name);
                if (!string.IsNullOrEmpty(checklistTemplate.Description)) checklistTemplate.Description = TextValidator.StripRogueDataFromText(checklistTemplate.Description);
                if (!string.IsNullOrEmpty(checklistTemplate.Role)) checklistTemplate.Role = TextValidator.StripRogueDataFromText(checklistTemplate.Role);

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
