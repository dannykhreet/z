using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    /// <summary>
    /// Will possible be removed
    /// </summary>
    public class AuditRelationAction
    {
        public int AuditTemplateId { get; set; }
        public int? AuditId { get; set; }
        public int TaskTemplateId { get; set; }
        public long? TaskId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int? ActionId { get; set; }
    }
}
