using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class WorkInstructionRelationParent
    {
        public int? AuditTemplateId { get; set; }
        public int? ChecklistTemplateId { get; set; }
        public int? TaskTemplateId { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string ParentType { 
            get {
                return (AuditTemplateId.HasValue && TaskTemplateId.HasValue ? ObjectTypeEnum.AuditTemplateTaskTemplate :
                    ChecklistTemplateId.HasValue && TaskTemplateId.HasValue ? ObjectTypeEnum.ChecklistTemplateTaskTemplate :
                    ChecklistTemplateId.HasValue && !TaskTemplateId.HasValue ? ObjectTypeEnum.ChecklistTemplate :
                    AuditTemplateId.HasValue && !TaskTemplateId.HasValue ? ObjectTypeEnum.AuditTemplate :
                    ObjectTypeEnum.TaskTemplate).ToString();
            } 
        }

    }
}
