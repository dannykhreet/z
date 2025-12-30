using EZGO.Api.Models.Skills;
using EZGO.Api.Models.WorkInstructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models.Checklist;
using WebApp.Models.WorkInstructions;
using WebApp.ViewModels;
using static WebApp.Logic.Constants;

namespace WebApp.Logic
{
    public static class ViewModelConverters
    {
        public static ViewModels.CompletedAuditSingleViewModel ConvertToViewModel(Models.Audit.CompletedAuditModel completedAudit)
        {
            var viewModel = new ViewModels.CompletedAuditSingleViewModel();

            viewModel.AreaPathIds = completedAudit.AreaPathIds;
            viewModel.Id = completedAudit.Id;
            viewModel.MaxTaskScore = completedAudit.MaxTaskScore;
            viewModel.MinTaskScore = completedAudit.MinTaskScore;
            viewModel.Name = completedAudit.Name;
            viewModel.OpenFieldsProperties = completedAudit.OpenFieldsProperties;
            viewModel.OpenFieldsPropertyUserValues = completedAudit.OpenFieldsPropertyUserValues;
            viewModel.Picture = completedAudit.Picture;
            viewModel.ScoreType = completedAudit.ScoreType;
            viewModel.Signatures = completedAudit.Signatures;
            viewModel.Tasks = completedAudit.Tasks;
            viewModel.TotalScore = completedAudit.TotalScore;

            return viewModel;
        }


        public static ViewModels.CompletedChecklistSingleViewModel ConvertToViewModel(Models.Checklist.CompletedChecklistModel completedChecklist)
        {
            var viewModel = new ViewModels.CompletedChecklistSingleViewModel();

            viewModel.AreaPathIds = completedChecklist.AreaPathIds;
            viewModel.Id = completedChecklist.Id;
            viewModel.Name = completedChecklist.Name;
            viewModel.OpenFieldsProperties = completedChecklist.OpenFieldsProperties;
            viewModel.OpenFieldsPropertyUserValues = completedChecklist.OpenFieldsPropertyUserValues;
            viewModel.Picture = completedChecklist.Picture;
            viewModel.Signatures = completedChecklist.Signatures;
            viewModel.Tasks = completedChecklist.Tasks;
            viewModel.Stages = completedChecklist.Stages;
            viewModel.EditedByUsers = completedChecklist.EditedByUsers;


            viewModel.IsCompleted = completedChecklist.IsCompleted;
            viewModel.CreatedAt = completedChecklist.CreatedAt;
            viewModel.ModifiedAt = completedChecklist.ModifiedAt;
            viewModel.CreatedBy = completedChecklist.CreatedBy;
            viewModel.ModifiedBy = completedChecklist.ModifiedBy;

            return viewModel;
        }

        public static ViewModels.TaskViewModel ConvertToViewModel(Models.Task.TaskTemplateModel taskTemplate)
        {
            return new ViewModels.TaskViewModel()
            {
                TaskTemplates = new List<Models.Task.TaskTemplateModel>() { taskTemplate },
                CurrentTaskTemplate = taskTemplate,
                Recurrency = new RecurrencyViewModel(taskTemplate.Recurrency)
            };
        }

        public static ViewModels.CompletedSkillAssessmentSingleViewModel ConvertToViewModel(Models.Skills.SkillAssessment completedAssessment)
        {
            var viewModel = new ViewModels.CompletedSkillAssessmentSingleViewModel();

            viewModel.AreaId = completedAssessment.AreaId;
            viewModel.AreaPath = completedAssessment.AreaPath;
            viewModel.AreaPathIds = completedAssessment.AreaPathIds;
            viewModel.AssessmentType = completedAssessment.AssessmentType;
            viewModel.CompanyId = completedAssessment.CompanyId;
            viewModel.CompletedAt = completedAssessment.CompletedAt;
            viewModel.CompletedFor = completedAssessment.CompletedFor;
            viewModel.CompletedForId = completedAssessment.CompletedForId;
            viewModel.CompletedForPicture = completedAssessment.CompletedForPicture;
            viewModel.CreatedAt = completedAssessment.CreatedAt;
            viewModel.CreatedBy = completedAssessment.CreatedBy;
            viewModel.CreatedById = completedAssessment.CreatedById;
            viewModel.Description = completedAssessment.Description;
            viewModel.Id = completedAssessment.Id;
            viewModel.IsCompleted = completedAssessment.IsCompleted;
            viewModel.Media = completedAssessment.Media;
            viewModel.ModifiedAt = completedAssessment.ModifiedAt;
            viewModel.ModifiedBy = completedAssessment.ModifiedBy;
            viewModel.ModifiedById = completedAssessment.ModifiedById;
            viewModel.Name = completedAssessment.Name;
            viewModel.NumberOfSignatures = completedAssessment.NumberOfSignatures;
            viewModel.NumberOfSkillInstructions = completedAssessment.NumberOfSkillInstructions;
            viewModel.Picture = completedAssessment.Picture;
            viewModel.Role = completedAssessment.Role;
            viewModel.SignatureRequired = completedAssessment.SignatureRequired;
            viewModel.Signatures = completedAssessment.Signatures;
            viewModel.SignatureType = completedAssessment.SignatureType;
            viewModel.SkillInstructions = completedAssessment.SkillInstructions;
            viewModel.Tags = completedAssessment.Tags;
            viewModel.TemplateId = completedAssessment.TemplateId;
            viewModel.TotalScore = completedAssessment.TotalScore;
            viewModel.CompletedAt = completedAssessment.CompletedAt;
            viewModel.Assessor = completedAssessment.Assessor;
            viewModel.AssessorId = completedAssessment.AssessorId;
            viewModel.AssessorPicture = completedAssessment.AssessorPicture;
            viewModel.StartDate = completedAssessment.StartDate;
            viewModel.EndDate = completedAssessment.EndDate;
            viewModel.Assessors = completedAssessment.Assessors;

            return viewModel;
        }

        public static WorkInstructionChangeNotificationViewModel ConvertToViewModel(WorkInstructionTemplateChangesNotificationModel wiChangeNotification)
        {
            var viewModel = new WorkInstructionChangeNotificationViewModel();

            viewModel.ChangeNotification = wiChangeNotification;

            return viewModel;
        }
    }
}
