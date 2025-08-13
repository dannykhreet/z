using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks.Properties;
using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class TaskTemplateModel : Api.Models.TaskTemplate, IBase<BasicTaskTemplateModel>
    {
        public int ChecklistId { get; set; }
        public int AuditId { get; set; }

        public new int ActionsCount { get; set; }
        public new int StepsCount { get; set; }

        public int Status { get; set; } = (int)TaskStatusEnum.Todo;
        public new List<StepModel> Steps { get; set; }

        public new List<PropertyTaskTemplateModel> Properties { get; set; }

        public new List<InstructionsModel> WorkInstructions { get; set; }

        public new List<InstructionsModel> WorkInstructionRelations { get; set; }

        public bool IsCompleted { get; set; } = true;

        public BasicTaskTemplateModel ToBasic()
        {
            BasicTaskTemplateModel result = new BasicTaskTemplateModel
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Picture = Picture,
                Video = Video,
                VideoThumbnail = VideoThumbnail,
                OpenActionCount = ActionsCount,
                StepsCount = StepsCount,
                Steps = Steps,
                DescriptionFile = DescriptionFile,
                FilterStatus = TaskStatusEnum.Todo,
                ChecklistTemplateId = ChecklistTemplateId,
                AuditTemplateId = AuditTemplateId,
                PlannedTime = PlannedTime,
                MachineStatus = (MachineStatusEnum)Enum.Parse(typeof(MachineStatusEnum), MachineStatus ?? "running", true),
                Weight = Weight,
                RecurrencyType = RecurrencyType,
                Properties = Properties,
                PropertyValues = PropertyValues,
                WorkInstructionRelations = WorkInstructionRelations,
                HasPictureProof = CompanyFeatures.RequiredProof && HasPictureProof,
                Tags = Tags,
                Attachments = Attachments,
                AuditId = AuditId,
                ChecklistId = ChecklistId,
            };

            return result;
        }
    }
}
