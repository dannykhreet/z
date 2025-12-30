using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class ActionRelation
    {
        public int ActionId { get; set; }
        public int? TaskId { get; set; }
        public int? TaskTemplateId { get; set; }
        public int? ChecklistId { get; set; }
        public int? AuditId { get; set; }
    }
}
