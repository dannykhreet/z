using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// ReportChecklistDeviations; Deviation report containing deviation data for checklists. 
    /// A report contains a collection of ReportDeviationItem containing skipped items (percentages and total numbers) and
    /// a collection of ReportDeviationItem containing NotOk items (percentages and total numbers) 
    /// </summary>
    public class ReportChecklistDeviations
    {
        public List<ReportDeviationItem> DeviationsSkipped { get; set; }
        public List<ReportDeviationItem> DeviationsNotOk { get; set; }

        public ReportChecklistDeviations()
        {
            DeviationsSkipped = new List<ReportDeviationItem>();
            DeviationsNotOk = new List<ReportDeviationItem>();
        }
    }
}
