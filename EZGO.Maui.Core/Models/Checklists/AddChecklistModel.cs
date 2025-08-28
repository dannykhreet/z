using EZGO.Api.Models;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Models.Checklists
{
    public class AddChecklistModel
    {
        public int? Id { get; set; }
        public int CompanyId { get; set; }
        public int TemplateId { get; set; }
        public bool IsCompleted { get; set; }
        public List<Signature> Signatures { get; set; }
        public List<TasksTemplateTaskStatusModel> Tasks { get; set; }
        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }
        public bool? IsRequiredForLinkedTask { get; set; }
        public long? LinkedTaskId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        public List<StageAddModel> Stages { get; set; }
        public string Version { get; set; }

    }
}
