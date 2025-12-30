using EZGO.Api.Models.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Skills;

namespace WebApp.Logic.Converters
{
    public static class AssessmentConverter
    {
        #region - local to API -
        public static AssessmentTemplate ToApiAssessmentTemplate(this SkillAssessmentTemplate assessmentTemplate)
        {
            if (assessmentTemplate != null)
            {
                AssessmentTemplate output = (AssessmentTemplate)assessmentTemplate;

                if (assessmentTemplate.IsDoubleSignatureRequired)
                {
                    output.SignatureType = EZGO.Api.Models.Enumerations.RequiredSignatureTypeEnum.TwoSignatureRequired;
                    output.SignatureRequired = true;
                }
                else if (assessmentTemplate.IsSignatureRequired)
                {
                    output.SignatureType = EZGO.Api.Models.Enumerations.RequiredSignatureTypeEnum.OneSignatureRequired;
                    output.SignatureRequired = true;
                }
                else
                {
                    output.SignatureType = EZGO.Api.Models.Enumerations.RequiredSignatureTypeEnum.NoSignaturedRequired;
                    output.SignatureRequired = false;
                }

                if (assessmentTemplate.TaskTemplates != null)
                {
                    output.SkillInstructions = new List<AssessmentTemplateSkillInstruction>();
                    foreach (var item in assessmentTemplate.TaskTemplates)
                    {
                        output.SkillInstructions.Add(item.ToApiAssessmentTemplateSkillInstruction());
                    }
                    assessmentTemplate.TaskTemplates.Clear();
                    assessmentTemplate.TaskTemplates = null;
                }

                return output;
            }
            return null;
        }
        //SkillAssessmentTemplateSkillInstruction
        public static Assessment ToApiAssessment(this SkillAssessment assessment)
        {
            if (assessment != null)
            {
                Assessment output = (Assessment)assessment;
                return output;
            }
            return null;
        }

        public static List<AssessmentTemplate> ToApiAssessmentTemplates(this List<SkillAssessmentTemplate> assessmentTemplates)
        {
            if (assessmentTemplates != null)
            {
                var output = new List<AssessmentTemplate>();
                foreach (var assessmentTemplate in assessmentTemplates)
                {
                    output.Add(assessmentTemplate.ToApiAssessmentTemplate());
                }
                return output;
            }
            return null;
        }

        public static AssessmentTemplateSkillInstruction ToApiAssessmentTemplateSkillInstruction(this SkillAssessmentTemplateSkillInstruction assessmentTemplateSkillInstruction)
        {
            if(assessmentTemplateSkillInstruction != null)
            {
                AssessmentTemplateSkillInstruction skillInstruction = (AssessmentTemplateSkillInstruction)assessmentTemplateSkillInstruction;
                return skillInstruction;
            }
            return null;
        }

        #endregion

        #region - api to local -
        public static SkillAssessmentTemplate ToLocalAssessmentTemplate(this AssessmentTemplate assessmentTemplate)
        {
            SkillAssessmentTemplate output = new SkillAssessmentTemplate();
            if (assessmentTemplate != null)
            {
                output.AreaId = assessmentTemplate.AreaId;
                output.AreaPath = assessmentTemplate.AreaPath;
                output.AreaPathIds = assessmentTemplate.AreaPathIds;
                output.AssessmentType = assessmentTemplate.AssessmentType;
                output.CompanyId = assessmentTemplate.CompanyId;
                output.CreatedAt = assessmentTemplate.CreatedAt;
                output.CreatedBy = assessmentTemplate.CreatedBy;
                output.CreatedById = assessmentTemplate.CreatedById;
                output.Description = assessmentTemplate.Description;
                output.Id = assessmentTemplate.Id;
                output.IsDoubleSignatureRequired = (output.SignatureType == EZGO.Api.Models.Enumerations.RequiredSignatureTypeEnum.TwoSignatureRequired);
                output.IsSignatureRequired = output.SignatureRequired;
                output.Media = assessmentTemplate.Media;
                output.ModifiedAt = assessmentTemplate.ModifiedAt;
                output.ModifiedBy = assessmentTemplate.ModifiedBy;
                output.ModifiedById = assessmentTemplate.ModifiedById;
                output.Name = assessmentTemplate.Name;
                output.NumberOfAssessments = assessmentTemplate.NumberOfAssessments;
                output.NumberOfSkillInstructions = assessmentTemplate.NumberOfSkillInstructions;
                output.Picture = assessmentTemplate.Picture;
                output.Role = assessmentTemplate.Role;
                output.SignatureRequired = assessmentTemplate.SignatureRequired;
                output.SignatureType = assessmentTemplate.SignatureType;
                output.Tags = assessmentTemplate.Tags;
                output.IsSignatureRequired = assessmentTemplate.SignatureRequired;
                output.IsDoubleSignatureRequired = assessmentTemplate.SignatureType == EZGO.Api.Models.Enumerations.RequiredSignatureTypeEnum.TwoSignatureRequired ? true : false;
                output.SkillInstructions = assessmentTemplate.SkillInstructions;
                output.NumberOfOpenAssessments = assessmentTemplate.NumberOfOpenAssessments;
                if(output.SkillInstructions != null && output.SkillInstructions.Any())
                {
                    output.TaskTemplates = output.SkillInstructions.OrderBy(y => y.Index).Select(x => x.ToLocalAssessmentTemplateSkillInstruction()).ToList();
                } 
                output.TotalScore = assessmentTemplate.TotalScore;
            }
            return output;
        }

        public static SkillAssessmentTemplate ToLocalAssessment(this AssessmentTemplate assessmentTemplate)
        {
            SkillAssessmentTemplate output = new SkillAssessmentTemplate();
            if (assessmentTemplate != null)
            {
                output.AreaPathIds = assessmentTemplate.AreaPathIds;
                output.AssessmentType = assessmentTemplate.AssessmentType;
                output.CompanyId = assessmentTemplate.CompanyId;
                output.Tags = assessmentTemplate.Tags;
            }
            return output;
        }

        public static List<SkillAssessmentTemplate> ToLocalAssessmentTemplates(this List<AssessmentTemplate> assessmentTemplates)
        {
            var output = new List<SkillAssessmentTemplate>();
            if (assessmentTemplates != null)
            {
                foreach (var assessmentTemplate in assessmentTemplates)
                {
                    output.Add(assessmentTemplate.ToLocalAssessmentTemplate());
                }
            }
            return output;
        }

        public static SkillAssessmentTemplateSkillInstruction ToLocalAssessmentTemplateSkillInstruction(this AssessmentTemplateSkillInstruction skillInstruction)
        {
            SkillAssessmentTemplateSkillInstruction output = new SkillAssessmentTemplateSkillInstruction();
            if (skillInstruction != null)
            {
                output.AreaId = skillInstruction.AreaId;
                output.AreaPathIds = skillInstruction.AreaPathIds;
                output.AreaPath = skillInstruction.AreaPath;
                output.AssessmentTemplateId = skillInstruction.AssessmentTemplateId;
                output.CompanyId = skillInstruction.CompanyId;
                output.CreatedAt = skillInstruction.CreatedAt;
                output.CreatedBy = skillInstruction.CreatedBy;
                output.CreatedById = skillInstruction.CreatedById;
                output.Description = skillInstruction.Description;
                output.Id = skillInstruction.Id;
                output.Index = skillInstruction.Index;
                output.InstructionItems = skillInstruction.InstructionItems;
                output.Media = skillInstruction.Media;
                output.Picture = skillInstruction.Picture;
                output.ModifiedAt = skillInstruction.ModifiedAt;
                output.ModifiedBy = skillInstruction.ModifiedBy;
                output.ModifiedById = skillInstruction.ModifiedById;
                output.Name = skillInstruction.Name;
                output.NumberOfInstructionItems = skillInstruction.NumberOfInstructionItems;
                output.Role = skillInstruction.Role.ToString();
                output.Tags = skillInstruction.Tags;
                output.WorkInstructionTemplateId = skillInstruction.WorkInstructionTemplateId;
                output.WorkInstructionType = skillInstruction.WorkInstructionType;
            }
            return output;
        }

        public static SkillAssessment ToLocalAssessment(this Assessment assessment)
        {
            SkillAssessment output = new SkillAssessment();
            if (assessment != null)
            {
                output.AreaId = assessment.AreaId;
                output.AreaPath = assessment.AreaPath;
                output.AreaPathIds = assessment.AreaPathIds;
                output.AssessmentType = assessment.AssessmentType;
                output.CompanyId = assessment.CompanyId;
                output.CompletedAt = assessment.CompletedAt;
                output.CompletedFor = assessment.CompletedFor;
                output.CompletedForId = assessment.CompletedForId;
                output.CompletedForPicture = assessment.CompletedForPicture;
                output.CreatedAt = assessment.CreatedAt;
                output.CreatedBy = assessment.CreatedBy;
                output.CreatedById = assessment.CreatedById;
                output.Description = assessment.Description;
                output.Id = assessment.Id;
                output.IsCompleted = assessment.IsCompleted;
                output.Media = assessment.Media;
                output.ModifiedAt = assessment.ModifiedAt;
                output.ModifiedBy = assessment.ModifiedBy;
                output.ModifiedById = assessment.ModifiedById;
                output.Name = assessment.Name;
                output.NumberOfSignatures = assessment.NumberOfSignatures;
                output.NumberOfSkillInstructions = assessment.NumberOfSkillInstructions;
                output.Picture = assessment.Picture;
                output.Role = assessment.Role;
                output.SignatureRequired = assessment.SignatureRequired;
                output.Signatures = assessment.Signatures;
                output.SignatureType = assessment.SignatureType;
                output.SkillInstructions = assessment.SkillInstructions;
                output.Tags = assessment.Tags;
                output.TemplateId = assessment.TemplateId;
                output.TotalScore = assessment.TotalScore;
                output.Assessor = assessment.Assessor;
                output.AssessorId = assessment.AssessorId;
                output.AssessorPicture = assessment.AssessorPicture;
                output.StartDate = assessment.StartDate;
                output.EndDate = assessment.EndDate;
                output.Assessors = assessment.Assessors;
            }
            return output;
        }

        public static List<SkillAssessment> ToLocalAssessments(this List<Assessment> assessments)
        {
            List<SkillAssessment> output = new List<SkillAssessment>();
            if (assessments != null)
            {
                foreach(var assessment in assessments)
                {
                    output.Add(assessment.ToLocalAssessment());
                }
            }
            return output;
        }
        #endregion
    }
}
