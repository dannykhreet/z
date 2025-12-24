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
    public static class CommentValidators
    {
        #region - messages -
        public const string MESSAGE_ACTION_ID_IS_NOT_VALID = "ActionId is not valid";
        public const string MESSAGE_COMMENT_ID_IS_NOT_VALID = "CommentId is not valid";

        public const string MESSAGE_COMMENT_TEXT_IS_NOT_VALID = "CommentText is not valid";
        #endregion

        public static bool CommentIdIsValid(int actionid)
        {
            if(actionid > 0)
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

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.Comment> comments, int companyId)
        {
            return !(comments.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Comment comment, int companyId)
        {
            return (comment.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Comment comment, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (comment == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Comment is not valid or empty;");
                }
                if (succes && comment.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(comment: comment, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && comment.UserId > 0 && !ignoreCreatedByCheck)
                {
                    succes = comment.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(comment.UserId.Value);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
                }

                if (succes && comment.Attachments != null && comment.Attachments.Count > 0)
                {
                    foreach (string item in comment.Attachments)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Attachment Uri [{0}] is not valid.", item));
                        }
                    }
                }

                if (succes && comment.Media != null && comment.Media.Count > 0)
                {
                    foreach (var item in comment.Media)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item.Uri);
                            if (!string.IsNullOrEmpty(item.VideoThumbnailUri))
                            {
                                succes = UriValidator.MediaUrlPartIsValid(item.VideoThumbnailUri);
                                if (!succes) messageBuilder.AppendLine(string.Format("Media Video Thumbnail Uri [{0}] is not valid.", item.VideoThumbnailUri));
                            }
                            if (!succes) messageBuilder.AppendLine(string.Format("Media Uri [{0}] is not valid.", item));
                        }
                    }
                }

                comment.CommentText = TextValidator.StripRogueDataFromText(comment.CommentText);
                if (!string.IsNullOrEmpty(comment.Description)) comment.Description = TextValidator.StripRogueDataFromText(comment.Description);
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

        public static bool ValidateMutation(this EZGO.Api.Models.Comment comment, EZGO.Api.Models.Comment CurrentComment, out string messages)
        {
            bool success = true;
            StringBuilder messageBuilder = new StringBuilder();

            if (CurrentComment.Id != comment.Id)
            {
                success = false;
                messageBuilder.AppendLine("Id can not be changed.");
            }

            if (success && CurrentComment.CompanyId != comment.CompanyId)
            {
                success = false;
                messageBuilder.AppendLine("Company id can not be changed.");
            }

            if (success && comment.UserId.HasValue && comment.UserId.Value > 0 && CurrentComment.UserId.Value != comment.UserId.Value)
            {
                success = false;
                messageBuilder.AppendLine("Created by id can not be changed.");
            }

            if (success && comment.TaskId.HasValue && comment.TaskId.Value > 0 && comment.TaskId.Value != CurrentComment.TaskId.Value)
            {
                success = false;
                messageBuilder.AppendLine("Task id can not be changed.");
            }

            if (success && comment.TaskTemplateId.HasValue && comment.TaskTemplateId.Value > 0 && comment.TaskTemplateId.Value != CurrentComment.TaskTemplateId.Value)
            {
                success = false;
                messageBuilder.AppendLine("Task template id can not be changed.");
            }

            messages = messageBuilder.ToString();
            return success;
        }

    }
}
