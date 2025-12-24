using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class ActionCommentValidators
    {
        #region - messages -
        public const string MESSAGE_ACTION_ID_IS_NOT_VALID = "ActionId is not valid";
        public const string MESSAGE_COMMENT_ID_IS_NOT_VALID = "CommentId is not valid";
        public const string MESSAGE_COMMENT_TEXT_IS_NOT_VALID = "CommentText is not valid";
        public const string MESSAGE_ACTION_IS_NOT_VALID = "Action data is not valid";
        #endregion

        public static bool ActionIdIsValid(int actionid)
        {
            if (actionid > 0)
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
            if (!string.IsNullOrEmpty(commenttext))
            {
                return true;
            }

            return false;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.ActionComment> actionComments, int companyId)
        {
            return !(actionComments.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.ActionComment actionComment, int companyId)
        {
            return (actionComment.CompanyId == companyId);
        }

        /// <summary>
        /// ValidateAndCleanActionComment; Cleans and validated action.
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
        public static bool ValidateAndClean(this EZGO.Api.Models.ActionComment actionComment, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int>validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (succes && actionComment.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(actionComment: actionComment, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && actionComment.UserId > 0 && !ignoreCreatedByCheck)
                {
                    succes = actionComment.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(actionComment.UserId);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
                }
                if (succes && actionComment.Images != null && actionComment.Images.Count > 0)
                {
                    foreach (string item in actionComment.Images)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Image Uri [{0}] is not valid.", item));
                        }
                    }
                }
                if (succes && !string.IsNullOrEmpty(actionComment.VideoThumbnail))
                {
                    succes = UriValidator.MediaUrlPartIsValid(actionComment.VideoThumbnail);
                    if (!succes) messageBuilder.AppendLine(string.Format("Video thumbnail Uri [{0}] is not valid.", actionComment.VideoThumbnail));
                }
                if (succes && !string.IsNullOrEmpty(actionComment.Video))
                {
                    succes = UriValidator.MediaUrlPartIsValid(actionComment.Video);
                    if (!succes) messageBuilder.AppendLine(string.Format("Video Uri [{0}] is not valid.", actionComment.Video));
                }

                actionComment.Comment = TextValidator.StripRogueDataFromText(actionComment.Comment);

            } catch(Exception ex)
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
