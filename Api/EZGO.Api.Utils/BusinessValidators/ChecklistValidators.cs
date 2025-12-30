using EZGO.Api.Models;
using EZGO.Api.Models.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// ChecklistValidators; contains all validation methods for validating checklists and values of the checklists.
    /// </summary>
    public static class ChecklistValidators
    {
        public const string MESSAGE_CHECKLIST_ID_IS_NOT_VALID = "ChecklistId is not valid";
        public const string MESSAGE_CHECKLIST_IS_NOT_VALID = "Checklist is not valid";
        public const string MESSAGE_TEMPLATE_ID_IS_NOT_VALID = "TemplateId is not valid";
        public const string MESSAGE_FILENAME_IS_NOT_VALID = "FileName is not valid";
        public static bool ChecklistIdIsValid(int checklistid)
        {
            if (checklistid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool TemplateIdIsValid(int templateid)
        {
            if (templateid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool SharedTemplateIdIsValid(int templateid)
        {
            if (templateid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool FileNameIsValid(string filename)
        {
            //Add regex for valid filenames
            if (!string.IsNullOrEmpty(filename) && filename.Length < 255)
            {
                return true;
            }

            return false;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.Checklist> checklists, int companyId)
        {
            return !(checklists.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Checklist checklist, int companyId)
        {
            return (checklist.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Checklist checklist, int companyId, int userId, out string messages, List<int> validUserIds = null, List<StageTemplate> stageTemplates = null, List<TaskTemplate> taskTemplates = null, List<Stage> existingStages = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (checklist == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Checklist is not valid or empty;");
                }

                if (succes && checklist.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(checklist: checklist, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                else if (checklist.CompanyId == 0)
                {
                    succes = false;
                    messageBuilder.AppendLine("Company id is not set.");
                }

                if (succes && checklist.PropertyUserValues != null && checklist.PropertyUserValues.Count > 0)
                {
                    foreach (var item in checklist.PropertyUserValues)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && checklist.OpenFieldsPropertyUserValues != null && checklist.OpenFieldsPropertyUserValues.Count > 0)
                {
                    foreach (var item in checklist.OpenFieldsPropertyUserValues)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && checklist.Tasks != null && checklist.Tasks.Count > 0)
                {
                    foreach (var item in checklist.Tasks)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                        if (succes)
                        {
                            succes = item.Status != null;
                            if (!succes) messageBuilder.Append("Status not set for all tasks.");
                        }
                    }
                }

                if (succes && checklist.Stages != null && checklist.Stages.Count > 0 && stageTemplates != null && stageTemplates.Count > 0)
                {
                    foreach (var stage in checklist.Stages)
                    {
                        if (succes)
                        {
                            var stageTemplate = stageTemplates.Where(t => t.Id == stage.StageTemplateId).FirstOrDefault();
                            var existingStage = existingStages == null ? null : existingStages.Where(t => t.Id == stage.Id).FirstOrDefault();

                            if (stageTemplate == null)
                            {
                                succes = false;
                                messageBuilder.Append("Stage template not found for this stage.");
                            }

                            if (succes)
                            {
                                succes = stage.ValidateAndClean(companyId: companyId, checklist.Tasks, out var possibleMessages, taskTemplates: taskTemplates, stageTemplate: stageTemplate, existingStage: existingStage);
                                if (!succes) messageBuilder.Append(possibleMessages);
                            }
                        }
                    }
                }

                if (succes && checklist.Signatures != null && checklist.Signatures.Count > 0)
                {
                    foreach (var item in checklist.Signatures)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && checklist.Signatures != null && checklist.Signatures.Count > 0)
                {
                    succes = (checklist.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Contains(userId));
                    if (!succes && validUserIds != null)
                    {
                        if (checklist.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Any())
                        {
                            //check if id is not part of valid user ids, if not break, if all ids then continue and all items are part of this collection.
                            foreach (var possibleUserId in checklist.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById))
                            {
                                if (!validUserIds.Contains(possibleUserId.Value)) break;
                            }
                            succes = true;
                        }
                    }
                    if (!succes) messageBuilder.AppendLine("User can not update or add this checklist.");
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

        /// <summary>
        /// Checks if the mutation is allowed by comparing the current data to the incomming data.
        /// Some field are not allowed to change.
        /// </summary>
        /// <param name="checklist"></param>
        /// <param name="currentChecklist"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static bool ValidateMutation(this Checklist checklist, Checklist currentChecklist, out string messages)
        {
            bool success = true;
            StringBuilder messageBuilder = new();

            try
            {
                if (success && currentChecklist.IsCompleted)
                {
                    success = false;
                    messageBuilder.AppendLine("Completed checklist can not be changed.");
                }

                if (success && checklist.Id != currentChecklist.Id)
                {
                    success = false;
                    messageBuilder.AppendLine("Id cannot be changed.");
                }

                if (success && checklist.CompanyId != currentChecklist.CompanyId)
                {
                    success = false;
                    messageBuilder.AppendLine("Company id cannot be changed.");
                }

                if (success && checklist.TemplateId != currentChecklist.TemplateId)
                {
                    success = false;
                    messageBuilder.AppendLine("Template id cannot be changed.");
                }

                if (success && checklist.CreatedById.HasValue && checklist.CreatedById.Value > 0 && currentChecklist.CreatedById.HasValue && checklist.CreatedById.Value != currentChecklist.CreatedById.Value)
                {
                    success = false;
                    messageBuilder.AppendLine("Created by id cannot be changed.");
                }

                if (success && !string.IsNullOrEmpty(checklist.Version) && !string.IsNullOrEmpty(currentChecklist.Version) && checklist.Version != currentChecklist.Version)
                {
                    success = false;
                    messageBuilder.AppendLine("Version can not be changed.");
                }
            }
            catch (Exception ex)
            {
                success = false;
                messageBuilder.AppendLine(string.Format("Error occured [{0}].", ex.Message));
            }

            messages = messageBuilder.ToString();
            return success;
        }

        /// <summary>
        /// Validates whether the provided checklist object exists (is not null).
        /// </summary>
        /// <param name="checklist">The checklist object to check.</param>
        /// <returns>True if the checklist exists; otherwise, false.</returns>
        public static bool ChecklistExists(Checklist checklist) {
            return (checklist != null);
        }
    }
}
