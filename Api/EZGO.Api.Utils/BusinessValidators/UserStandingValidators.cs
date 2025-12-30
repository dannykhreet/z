using EZGO.Api.Models.Users;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class UserStandingValidators
    {
        public const string MESSAGE_USER_SKILL_ID_IS_NOT_VALID = "User Skill Id is not valid";
        public const string MESSAGE_USER_GROUP_ID_IS_NOT_VALID = "User Group Id is not valid";

        public const string MESSAGE_USER_SKILL_IN_USE_IN_MATRIX = "User Skill in use in Skills Matrix";
        public const string MESSAGE_USER_GROUP_IN_USE_IN_MATRIX = "User Group in use in Skills Matrix";
        public static bool UserSkillIdIsValid(int userSkillId)
        {
            if (userSkillId > 0)
            {
                return true;
            }

            return false;
        }
        public static bool UserGroupIdIsValid(int userGroupId)
        {
            if (userGroupId > 0)
            {
                return true;
            }

            return false;
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Users.UserGroup userGroup, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (userGroup == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("UserGroup is not valid or empty;");
                }

                userGroup.Name = TextValidator.StripRogueDataFromText(userGroup.Name);
                if (!string.IsNullOrEmpty(userGroup.Description)) userGroup.Description = TextValidator.StripRogueDataFromText(userGroup.Description);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Users.UserSkill userSkill, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (userSkill == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("UserSkill is not valid or empty;");
                }

                userSkill.Name = TextValidator.StripRogueDataFromText(userSkill.Name);

                if (string.IsNullOrEmpty(userSkill.Name))
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("User skill name is required;");
                }

                if (userSkill.SkillType == Models.Enumerations.SkillTypeEnum.Operational && userSkill.DefaultTarget == 0)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Default target is required for operational skills;");
                }

                if (!string.IsNullOrEmpty(userSkill.Description)) userSkill.Description = TextValidator.StripRogueDataFromText(userSkill.Description);

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

        public static bool ValidateAndClean(this EZGO.Api.Models.Users.UserSkillValue userSkillValue, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (userSkillValue == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("UserSkillValue is not valid or empty;");
                }

                if (succes && userSkillValue.Attachments != null && userSkillValue.Attachments.Count > 0)
                {
                    foreach (string item in userSkillValue.Attachments)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Attachment Uri [{0}] is not valid.", item));
                        }
                    }
                }
                if (succes && userSkillValue.UserId > 0 && !ignoreCreatedByCheck)
                {
                    succes = userSkillValue.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(userSkillValue.UserId);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
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

    }


}
