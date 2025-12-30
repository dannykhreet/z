using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// ReportDeviationItem for use with deviation reports. This normally would be part of a collection. 
    /// </summary>
    public class ReportDeviationItem
    {
        public double? Percentage { get; set; }
        public int? Id { get; set; } //e.g. the task template id
        public int? CountNr { get; set; }
        public string Name { get; set; } //e.g. the task name
        public string Status { get; set; }
        public int? ParentTemplateId { get; set; } //depending on item, audit or checklist template id (in case of tasks not available)
        public string ParentTemplateName { get; set; } //depending on item, audit or checklist template name (in case of tasks not available)
        public int ActionCount { get; set; }
        public int ActionDoneCount { get; set; }
    }
}
