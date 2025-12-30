using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// ReportAuditDeviationItem; Deviation item for use within the deviation reports. This item is specific for Audits seeing they have an calculation structure within the report.
    /// Normal deviation collections will use the ReportDeviationItem or ReportDeviationTaskStatusItem for display.
    /// </summary>
    public class ReportAuditDeviationItem
    {
        public int? NumberOfQuestions { get; set; }
        public int AuditTemplateId { get; set; }
        public string AuditTemplateName { get; set; }
        public int TaskTemplateId { get; set; }
        public string TaskTemplateName { get; set; }
        public int ActionCount { get; set; }
        public int ActionDoneCount { get; set; }
        public double? DeviationScore { get; set; }
        public double? DeviationPercentage { get; set; }

    }
}
