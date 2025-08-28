using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;

namespace EZGO.Maui.Core.Models.Checklists
{
    public class ChecklistTemplateModel : EZGO.Api.Models.ChecklistTemplate, IBase<BasicChecklistTemplateModel>, IOpenTextFields, IItemFilter<TaskStatusEnum>
    {
        public Guid LocalGuid { get; set; }
        public int SelectedChecklistId { get; set; }
        private List<TaskTemplateModel> _taskTemplates;
        public new List<TaskTemplateModel> TaskTemplates
        {
            get => _taskTemplates;
            set => _taskTemplates = value;
        }
        public string Date { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalTasks { get; set; }
        public int OkTasks { get; set; }
        public int NotOkTasks { get; set; }
        public int SkippedTasks { get; set; }
        public int TodoTasks { get; set; }

        public DateTime? StartedAt { get; set; }

        public List<TemplatePropertyModel> OpenFieldsProperties { get; set; }
        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }

        public BasicChecklistTemplateModel ToBasic()
        {
            var result = new BasicChecklistTemplateModel
            {
                Id = this.Id,
                Name = this.Name,
                Tags = this.Tags
                //TaskTemplates = this.TaskTemplates
            };
            return result;
        }

        public new List<PropertyChecklistTemplateModel> Properties { get; set; }
        public TaskStatusEnum FilterStatus { get; set; }
        public new List<StageTemplateModel> StageTemplates { get; set; }
    }
}
