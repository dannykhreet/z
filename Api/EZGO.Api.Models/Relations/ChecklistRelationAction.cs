using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Relations
{
    /// <summary>
    /// Will possible be removed
    /// </summary>
    public class ChecklistRelationAction
    {
        public int ChecklistTemplateId { get; set; }
        public int? ChecklistId { get; set; }
        public int TaskTemplateId { get; set; }
        public long? TaskId { get; set; }
        public int CompanyId { get; set; }
        public int? ActionId { get; set; }
    }
}
