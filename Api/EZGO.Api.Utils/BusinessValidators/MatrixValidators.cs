using EZGO.Api.Models.Skills;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class MatrixValidators
    {
        public const string MESSAGE_MATRIX_IS_NOT_VALID = "Matrix is not valid.";
        public const string MESSAGE_MATRIXID_IS_NOT_VALID = "MatrixId is not valid.";
        public const string MESSAGE_USER_SKILL_ALREADY_IN_MATRIX = "User skill is already in matrix, failed to add.";
        public const string MESSAGE_USER_GROUP_ALREADY_IN_MATRIX = "User group is already in matrix, failed to add.";

        public static bool MatrixIdIsValid(int matrixId)
        {
            if (matrixId > 0)
            {
                return true;
            }

            return false;
        }

        public static bool MatrixIsValid(SkillsMatrix matrix)
        {
            bool output = true;

            //TODO fill

            return output;
        }

        public static bool CompanyConnectionIsValid(List<SkillsMatrix> matrices, int companyId)
        {
            return !(matrices.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(SkillsMatrix matrix, int companyId)
        {
            return (matrix.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.SkillsMatrix matrix, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (matrix == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Matrix is not valid or empty;");
                }
                if (succes && matrix.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(matrix: matrix, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && matrix.CreatedById > 0 && !ignoreCreatedByCheck)
                {
                    succes = matrix.CreatedById == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(matrix.CreatedById.Value);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
                }
                if (succes && matrix.ModifiedById > 0)
                {
                    succes = matrix.ModifiedById == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(matrix.ModifiedById.Value);
                    if (!succes) messageBuilder.AppendLine("User modified by identifier is not valid.");
                }

                matrix.Name = TextValidator.StripRogueDataFromText(matrix.Name);
                if (!string.IsNullOrEmpty(matrix.Description)) matrix.Description = TextValidator.StripRogueDataFromText(matrix.Description);

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

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.SkillsMatrixItem skillsMatrixItem, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try {
                if (skillsMatrixItem == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Matrix item is not valid or empty;");
                }

                skillsMatrixItem.Name = TextValidator.StripRogueDataFromText(skillsMatrixItem.Name);
                if (!string.IsNullOrEmpty(skillsMatrixItem.Description)) skillsMatrixItem.Description = TextValidator.StripRogueDataFromText(skillsMatrixItem.Description);
                if (!string.IsNullOrEmpty(skillsMatrixItem.SkillAssessmentName)) skillsMatrixItem.SkillAssessmentName = TextValidator.StripRogueDataFromText(skillsMatrixItem.SkillAssessmentName);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.SkillsMatrixBehaviourItem skillsMatrixItem, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (skillsMatrixItem == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Matrix item is not valid or empty;");
                }

                skillsMatrixItem.Name = TextValidator.StripRogueDataFromText(skillsMatrixItem.Name);
                if (!string.IsNullOrEmpty(skillsMatrixItem.Description)) skillsMatrixItem.Description = TextValidator.StripRogueDataFromText(skillsMatrixItem.Description);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.SkillsMatrixBehaviourItemValue skillsMatrixItemValue, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (skillsMatrixItemValue == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Matrix item value is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(skillsMatrixItemValue.TechnicalUid)) skillsMatrixItemValue.TechnicalUid = TextValidator.StripRogueDataFromText(skillsMatrixItemValue.TechnicalUid);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.SkillsMatrixItemValue skillsMatrixItemValue, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (skillsMatrixItemValue == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Matrix item value is not valid or empty;");
                }

                if (succes && skillsMatrixItemValue.Attachments != null && skillsMatrixItemValue.Attachments.Count > 0)
                {
                    foreach (string item in skillsMatrixItemValue.Attachments)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Attachment Uri [{0}] is not valid.", item));
                        }
                    }
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.SkillsMatrixUserGroup skillsMatrixUserGroup, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try {
                if (skillsMatrixUserGroup == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Matrix user group is not valid or empty;");
                }

                skillsMatrixUserGroup.Name = TextValidator.StripRogueDataFromText(skillsMatrixUserGroup.Name);
                if (!string.IsNullOrEmpty(skillsMatrixUserGroup.Description)) skillsMatrixUserGroup.Description = TextValidator.StripRogueDataFromText(skillsMatrixUserGroup.Description);
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
