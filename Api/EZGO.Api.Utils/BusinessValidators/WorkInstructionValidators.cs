using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class WorkInstructionValidators
    {
        public const string MESSAGE_WORKINSTRUCTION_ID_IS_NOT_VALID = "WorkInstructionId is not valid";
        public const string MESSAGE_TEMPLATE_ID_IS_NOT_VALID = "TemplateId is not valid";
        public const string MESSAGE_FILENAME_IS_NOT_VALID = "FileName is not valid";
        public const string MESSAGE_WORKINSTRUCTION_CHANGE_NOTIFICATION_ID_IS_NOT_VALID = "WorkInstructionChangeNotificationId is not valid";

        public static bool WorkInstructionIdIsValid(int workInstructionId) => workInstructionId > 0;
        public static bool WorkInstructionChangeNotificationIdIsValid(int changeNotificationId) => changeNotificationId > 0;

        public static bool TemplateIdIsValid(int workInstructionTemplateId) => workInstructionTemplateId > 0;


        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate> workInstructionTemplates, int companyId)
        {
            return !(workInstructionTemplates.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate workInstructionTemplate, int companyId)
        {
            return (workInstructionTemplate.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.WorkInstructions.InstructionItemTemplate> instructionTemplates, int companyId)
        {
            return !(instructionTemplates.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.WorkInstructions.InstructionItemTemplate instructionTemplate, int companyId)
        {
            return (instructionTemplate.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.WorkInstructions.WorkInstructionTemplate workInstructionTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (workInstructionTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("WorkInstructionTemplate is not valid or empty;");
                }
                if (succes && workInstructionTemplate.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(workInstructionTemplate: workInstructionTemplate, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && workInstructionTemplate.InstructionItems != null && workInstructionTemplate.InstructionItems.Count > 0)
                {
                    foreach (var item in workInstructionTemplate.InstructionItems)
                    {
                        succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                        if (!succes) messageBuilder.Append(possibleMessages);
                    }
                }
                if (succes && !string.IsNullOrEmpty(workInstructionTemplate.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(workInstructionTemplate.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", workInstructionTemplate.Picture));
                }
                if (succes && workInstructionTemplate.Media != null && workInstructionTemplate.Media.Count > 0)
                {
                    foreach (string item in workInstructionTemplate.Media)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Media Uri [{0}] is not valid.", item));
                        }
                    }
                }

                workInstructionTemplate.Name = TextValidator.StripRogueDataFromText(workInstructionTemplate.Name);
                if (!string.IsNullOrEmpty(workInstructionTemplate.Description)) workInstructionTemplate.Description = TextValidator.StripRogueDataFromText(workInstructionTemplate.Description);

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


        public static bool ValidateAndClean(this EZGO.Api.Models.WorkInstructions.InstructionItemTemplate instructionTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try {
                if (instructionTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("InstructionTemplate is not valid or empty;");
                }
                if (succes && instructionTemplate.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(instructionTemplate: instructionTemplate, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && !string.IsNullOrEmpty(instructionTemplate.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(instructionTemplate.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", instructionTemplate.Picture));
                }
                if (succes && !string.IsNullOrEmpty(instructionTemplate.Video))
                {
                    succes = UriValidator.MediaUrlPartIsValid(instructionTemplate.Video);
                    if (!succes) messageBuilder.AppendLine(string.Format("Video Uri [{0}] is not valid.", instructionTemplate.Video));
                }
                if (succes && !string.IsNullOrEmpty(instructionTemplate.VideoThumbnail))
                {
                    succes = UriValidator.MediaUrlPartIsValid(instructionTemplate.VideoThumbnail);
                    if (!succes) messageBuilder.AppendLine(string.Format("VideoThumbnail Uri [{0}] is not valid.", instructionTemplate.VideoThumbnail));
                }
                if (succes && instructionTemplate.Media != null && instructionTemplate.Media.Count > 0)
                {
                    foreach (string item in instructionTemplate.Media)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Media Uri [{0}] is not valid.", item));
                        }
                    }
                }

                instructionTemplate.Name = TextValidator.StripRogueDataFromText(instructionTemplate.Name);
                if (!string.IsNullOrEmpty(instructionTemplate.Description)) instructionTemplate.Description = TextValidator.StripRogueDataFromText(instructionTemplate.Description);
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
