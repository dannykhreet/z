using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    public class CommentRelation
    {
        public int CommentId { get; set; }
        public int? TaskId { get; set; }
        public int? TaskTemplateId { get; set; }
        public int? ChecklistId { get; set; }
        public int? AuditId { get; set; }
    }
}
