using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models.Action
{
    public class ActionParentModel// : EZGO.Api.Models.Basic.ActionParentBasic
    {
        public int? ActionId { get; set; }
        public int? AuditId { get; set; }
        public int? ChecklistId { get; set; }
        public int? AuditTemplateId { get; set; }
        public int? ChecklistTemplateId { get; set; }
        public int? TaskId { get; set; }
        public int? TaskTemplateId { get; set; }
        public string TaskName { get; set; }
        public string ChecklistTemplateName { get; set; }
        public string AuditTemplateName { get; set; }
        public string Type { get; set; }

    }
}
