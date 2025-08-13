using System;
using System.Collections.Generic;
using EZGO.Api.Models;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Core.Models.Audits
{
    public class AddAuditModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int TemplateId { get; set; }
        public bool IsCompleted { get; set; }
        public List<Signature> Signatures { get; set; }
        public List<TasksTemplateAuditTaskStatusModel> Tasks { get; set; }
        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }
        public bool? IsRequiredForLinkedTask { get; set; }
        public long? LinkedTaskId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? CreatedById { get; set; }
        public int? ModifiedById { get; set; }
        public string Version { get; set; }
    }
}
