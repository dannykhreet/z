using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using WebApp.Models.Properties;

namespace WebApp.Models.Task
{
    public class TaskTemplateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Role { get; set; }
        public string AreaPath { get; set; }
        public int AreaId { get; set; }
        public int CompanyId { get; set; }
        public string DescriptionFile { get; set; }
        public string Description { get; set; }
        public int StepsCount { get; set; }
        public TaskRecurrencyModel Recurrency { get; set; }
        public int? DeepLinkId { get; set; }
        public string DeepLinkTo { get; set; }
        /// <summary>
        /// DeepLinkCompletionIsRequired; Indicates if completion of the linked object (checklist or audit) is required before the task can be completed
        /// </summary>
        public bool? DeepLinkCompletionIsRequired { get; set; }
        public string Video { get; set; }
        public string VideoThumbnail { get; set; }
        public string AreaPathIds { get; set; }
        public string RecurrencyType { get; set; }
        public bool HasDerivedItems { get; set; }
        public bool HasPictureProof { get; set; }
        public string MachineStatus { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public int? PlannedTime { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<TemplatePropertyModel> Properties { get; set; }
        public List<TaskTaskTemplateModel> TaskTemplates { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public List<TaskTemplateRelationWorkInstructionTemplate> WorkInstructionRelations { get; set; }
        public List<TaskStepModel> Steps { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Attachment> Attachments { get; set; }
        public int? SharedTemplateId { get; set; }
        public int PreviousTasksCount { get; set; }
    }
}
