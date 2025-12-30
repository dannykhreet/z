using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// ReportAuditDeviations; Deviation report containing deviation data for audits. 
    /// The report consists of a collection of ReportAuditDeviationItem, containing calculated data and
    /// a collection of ReportDeviationItem containing skipped items (percentages and total numbers)
    /// </summary>
    public class ReportAuditDeviations
    {
        public List<ReportAuditDeviationItem> Deviations { get; set; }
        public List<ReportDeviationItem> DeviationsSkipped { get; set; }

        public ReportAuditDeviations()
        {
            Deviations = new List<ReportAuditDeviationItem>();
            DeviationsSkipped = new List<ReportDeviationItem>();
        }
    }
}
