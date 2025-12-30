using EZGO.Api.Models.Skills;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class AssessmentValidators
    {
        public const string MESSAGE_ASSESSMENTTEMPLATEID_IS_NOT_VALID = "AssessmentTemplateId is not valid.";
        public const string MESSAGE_ASSESSMENTID_IS_NOT_VALID = "AssessmentId is not valid.";
        public const string MESSAGE_ASSESSMENT_IS_NOT_VALID = "Assessment is not valid.";
        public const string MESSAGE_ASSESSMENTTEMPLATE_IS_NOT_VALID = "AssessmentTemplate is not valid.";
        public const string MESSAGE_ASSESSMENT_ALREADY_EXISTS = "Assessment already exists for this assessee.";

        public static bool TemplateIdIsValid(int assessmentTemplateId)
        {
            if (assessmentTemplateId > 0)
            {
                return true;
            }

            return false;
        }

        public static bool AssessmentIdIsValid(int assessmentId)
        {
            if (assessmentId > 0)
            {
                return true;
            }

            return false;
        }

        public static bool AssessorIdIsValid(int assessorId)
        {
            if (assessorId > 0)
            {
                return true;
            }

            return false;
        }

        public static bool AssessmentTemplateIsValid(AssessmentTemplate assessmentTemplate, bool isExisting = false)
        {
            bool output = true;

            //TODO fill

            return output;
        }

        public static bool AssessmentIsValid(Assessment assessment, bool isExisting = false)
        {
            bool output = true;

            //TODO fill

            return output;
        }

        public static bool CompanyConnectionIsValid(List<AssessmentTemplate> assessmentTemplates, int companyId)
        {
            return !(assessmentTemplates.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(AssessmentTemplate assessmentTemplate, int companyId)
        {
            return (assessmentTemplate.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.AssessmentTemplate assessmentTemplate, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (assessmentTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("AssessmentTemplate is not valid or empty;");
                }
                if (succes && assessmentTemplate.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(assessmentTemplate: assessmentTemplate, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && !string.IsNullOrEmpty(assessmentTemplate.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(assessmentTemplate.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", assessmentTemplate.Picture));
                }
                if (succes && assessmentTemplate.Media != null && assessmentTemplate.Media.Count > 0)
                {
                    foreach (string item in assessmentTemplate.Media)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Media Uri [{0}] is not valid.", item));
                        }
                    }
                }

                assessmentTemplate.Name = TextValidator.StripRogueDataFromText(assessmentTemplate.Name);
                if (!string.IsNullOrEmpty(assessmentTemplate.Description)) assessmentTemplate.Description = TextValidator.StripRogueDataFromText(assessmentTemplate.Description);

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



        public static bool CompanyConnectionIsValid(List<Assessment> assessment, int companyId)
        {
            return !(assessment.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(Assessment assessment, int companyId)
        {
            return (assessment.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Skills.Assessment assessment, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (assessment == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("AssessmentTemplate is not valid or empty;");
                }
                if (succes && assessment.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(assessment: assessment, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && assessment.CreatedById.HasValue && assessment.CreatedById > 0 && !ignoreCreatedByCheck)
                {
                    succes = assessment.CreatedById == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(assessment.CreatedById.Value);
                    if (!succes) messageBuilder.AppendLine("User created by identifier is not valid.");
                }

                if (succes && assessment.ModifiedById.HasValue && assessment.ModifiedById > 0)
                {
                    succes = assessment.ModifiedById == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(assessment.ModifiedById.Value);
                    if (!succes) messageBuilder.AppendLine("User modified by identifier is not valid.");
                }
                else if (succes && !assessment.ModifiedById.HasValue)
                {
                    assessment.ModifiedById = userId;
                }

                if (succes && assessment.Signatures != null && assessment.Signatures.Count > 0)
                {
                    foreach (var item in assessment.Signatures)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && assessment.Signatures != null && assessment.Signatures.Count > 0)
                {
                    succes = (assessment.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Contains(userId));
                    if (!succes && validUserIds != null)
                    {
                        if (assessment.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Any())
                        {
                            //check if id is not part of valid user ids, if not break, if all ids then continue and all items are part of this collection.
                            foreach (var possibleUserId in assessment.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById))
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

        public static bool ValidateMutation(this Assessment assessment, Assessment currentAssessment, out string messages)
        {
            bool success = true;
            StringBuilder messageBuilder = new();

            if (success && currentAssessment.IsCompleted)
            {
                success = false;
                messageBuilder.AppendLine("Completed assessment can not be changed.");
            }

            if (success && assessment.Id != currentAssessment.Id)
            {
                success = false;
                messageBuilder.AppendLine("Id can not be changed.");
            }

            if (success && assessment.CompanyId != currentAssessment.CompanyId)
            {
                success = false;
                messageBuilder.AppendLine("Company id can not be changed.");
            }

            if (success && assessment.TemplateId != currentAssessment.TemplateId)
            {
                success = false;
                messageBuilder.AppendLine("Template id can not be changed.");
            }

            if (success && assessment.CreatedById.HasValue && assessment.CreatedById.Value > 0 && currentAssessment.CreatedById.HasValue && assessment.CreatedById.Value != currentAssessment.CreatedById.Value)
            {
                success = false;
                messageBuilder.AppendLine("Created by id can not be changed.");
            }

            if (success && !string.IsNullOrEmpty(assessment.Version) && !string.IsNullOrEmpty(currentAssessment.Version) && assessment.Version != currentAssessment.Version)
            {
                success = false;
                messageBuilder.AppendLine("Version can not be changed.");
            }

            messages = messageBuilder.ToString();
            return success;
        }
    }
}
