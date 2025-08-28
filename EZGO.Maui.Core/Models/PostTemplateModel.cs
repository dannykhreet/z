using System;
using EZGO.Maui.Core.Models.Tasks;
using System.Collections.Generic;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Api.Models;
using EZGO.Maui.Core.Models.Stages;

namespace EZGO.Maui.Core.Models
{
    public class PostTemplateModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public IEnumerable<UserValuesPropertyModel> UserValues { get; set; }
        public IEnumerable<BasicTaskTemplateModel> Tasks { get; set; }
        public IEnumerable<Signature> Signatures { get; set; }
        public DateTime? StartedAt { get; set; }
        public bool? IsRequiredForLinkedTask { get; set; }
        public long? LinkedTaskId { get; set; }
        public bool IsCompleted { get; set; }
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        public List<StageTemplateModel> Stages { get; set; }
        public Guid LocalGuid { get; internal set; }
        public string Version { get; set; }

        public PostTemplateModel(int templateId, string templateName, IEnumerable<UserValuesPropertyModel> userValues, IEnumerable<BasicTaskTemplateModel> tasks, bool? isRequiredForLinkedTask = null, long? linkedTaskId = null, bool isCompleted = true, int id = 0, List<StageTemplateModel> stages = null, string version = "")
        {
            TemplateId = templateId;
            TemplateName = templateName;
            UserValues = userValues;
            Tasks = tasks;
            IsRequiredForLinkedTask = isRequiredForLinkedTask;
            LinkedTaskId = linkedTaskId;
            IsCompleted = isCompleted;
            Id = id;
            Stages = stages;
            Version = version;
        }
    }
}

