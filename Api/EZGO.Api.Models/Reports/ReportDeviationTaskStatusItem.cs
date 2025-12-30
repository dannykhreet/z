using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// ReportDeviationTaskStatusItem; contains a set of percentages and counts per template (tasktemplate or tasktemplate and a parent checklist/audit template)
    /// </summary>
    public class ReportDeviationTaskStatusItem
    {
        public double? PercentageOk { get; set; }
        public double? PercentageNotOk { get; set; }
        public double? PercentageSkipped { get; set; }
        public double? PercentageTodo { get; set; }
        public int? CountOk { get; set; }
        public int? CountNotOk { get; set; }
        public int? CountSkipped { get; set; }
        public int? CountTodo { get; set; }
        public int? CountNr { get; set; }
        public int? TaskTemplateId { get; set; } //will be the task template id
        public string TaskTemplateName { get; set; } //will be the task name
        public string Status { get; set; }
        public int? ParentTemplateId { get; set; } //depending on item, audit or checklist template id (in case of tasks not available)
        public string ParentTemplateName { get; set; } //depending on item, audit or checklist template name (in case of tasks not available)
        public int ActionCount { get; set; }
        public int ActionDoneCount { get; set; }
    }
}
