using EZGO.Api.Models;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// ActionValidators; contains all validation methods for validating properties and values of the actions (are part of tasks and related objects).
    /// </summary>
    public static class ActionValidators
    {
        #region - messages -
        public const string MESSAGE_ACTION_ID_IS_NOT_VALID = "ActionId is not valid";
        public const string MESSAGE_COMMENT_ID_IS_NOT_VALID = "CommentId is not valid";
        public const string MESSAGE_COMMENT_TEXT_IS_NOT_VALID = "CommentText is not valid";
        public const string MESSAGE_ACTION_IS_NOT_VALID = "Action data is not valid";
        #endregion

        public static bool ActionIdIsValid(int actionid)
        {
            if(actionid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool CommentIdIsValid(int commentid)
        {
            if (commentid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool CommentTextIsValid(string commenttext)
        {
            if(!string.IsNullOrEmpty(commenttext))
            {
                return true;
            }

            return false;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.ActionsAction> actions, int companyId)
        {
            return !(actions.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.ActionsAction action, int companyId)
        {
            return (action.CompanyId == companyId);
        }

        /// <summary>
        /// ValidateAndCleanAction; Cleans and validated action.
        /// The following rules must be met:
        /// = The company that is connected to the action must be the company that executes the check.
        /// - The userId that created the item is the same user that executes the check (unless ignoreUserIdCheck is true).
        /// - The Comment can not contain script related tags and filters out html tags
        /// - The Description can not contain script related tags and filters out html tags
        /// - The Images should contain valid media UriParts
        /// - The Videos should contain valid media UriParts
        /// - The VideoThumbnails should contain valid media UriParts
        /// </summary>
        /// <param name="action">action object containing all data</param>
        /// <param name="companyId">company of user that is executing the validation</param>
        /// <param name="userId">userId of the user that is executing the validation</param>
        /// <param name="ignoreCreatedByCheck">ignore the created by user check (for changed items)</param>
        /// <returns>true/false</returns>
        public static bool ValidateAndClean(this EZGO.Api.Models.ActionsAction action, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (action == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Action is not valid or empty;");
                }

                if (succes && action.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(action: action, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                else if (succes && action.CompanyId <= 0)
                {
                    action.CompanyId = companyId;
                }

                if (succes && action.CreatedById > 0 && !ignoreCreatedByCheck)
                {
                    succes = action.CreatedById == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(action.CreatedById);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
                }
                else if (succes && action.CreatedById <= 0)
                {
                    action.CreatedById = userId;
                }

                if (succes && action.Images != null && action.Images.Count > 0)
                {
                    foreach (string item in action.Images)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Image Uri [{0}] is not valid.", item));
                        }
                    }
                }

                if (succes && action.VideoThumbNails != null && action.VideoThumbNails.Count > 0)
                {
                    foreach (string item in action.VideoThumbNails)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Video thumbnail Uri [{0}] is not valid.", item));
                        }
                    }
                }

                if (succes && action.Videos != null && action.Videos.Count > 0)
                {
                    foreach (string item in action.Videos)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Video Uri [{0}] is not valid.", item));
                        }
                    }
                }

                if(succes && action.SapPmNotificationConfig != null)
                {
                    if (!string.IsNullOrEmpty(action.SapPmNotificationConfig.NotificationTitle)) action.SapPmNotificationConfig.NotificationTitle = TextValidator.StripRogueDataFromText(action.SapPmNotificationConfig.NotificationTitle);
                }

                action.Comment = TextValidator.StripRogueDataFromText(action.Comment);
                if (!string.IsNullOrEmpty(action.Description)) action.Description = TextValidator.StripRogueDataFromText(action.Description);

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

        //TODO add validation of Action or Comment object (e.g. what is are the rules)

        public static bool ValidateMutation(this ActionsAction action, ActionsAction currentAction, out string messages)
        {
            bool success = true;
            StringBuilder messageBuilder = new();

            if (currentAction.Id != action.Id)
            {
                success = false;
                messageBuilder.AppendLine("Id can not be changed.");
            }

            if (success && currentAction.CompanyId != action.CompanyId)
            {
                success = false;
                messageBuilder.AppendLine("Company id can not be changed.");
            }

            if (success && action.CreatedById > 0 && currentAction.CreatedById != action.CreatedById)
            {
                success = false;
                messageBuilder.AppendLine("Created by id can not be changed.");
            }

            if (success && action.TaskId.HasValue && action.TaskId.Value > 0 && action.TaskId.Value != currentAction.TaskId.Value)
            {
                success = false;
                messageBuilder.AppendLine("Task id can not be changed.");
            }

            if (success && action.TaskTemplateId.HasValue && action.TaskTemplateId.Value > 0 && currentAction.TaskTemplateId.HasValue && action.TaskTemplateId.Value != currentAction.TaskTemplateId.Value)
            {
                success = false;
                messageBuilder.AppendLine("Task template id can not be changed.");
            }

            messages = messageBuilder.ToString();
            return success;
        }

        public static bool ValidateResolve(this ActionsAction existingAction, bool isResolved, out string messages)
        {
            bool success = true;
            StringBuilder messageBuilder = new();

            if (existingAction != null && existingAction.IsResolved.HasValue && existingAction.IsResolved.Value == true && isResolved)
            {
                success = false;
                messageBuilder.AppendLine("Action has already been resolved, and can't be resolved again.");
            }

            messages = messageBuilder.ToString();
            return success;
        }
    }
}
