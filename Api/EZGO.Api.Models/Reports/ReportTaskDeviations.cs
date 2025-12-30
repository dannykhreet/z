using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Reports
{
    /// <summary>
    /// ReportTaskDeviations; Deviation report containing deviation data for a set of tasks.
    /// A report contains a collection of ReportDeviationItem containing skipped items (percentages and total numbers) and
    /// a collection of ReportDeviationItem containing NotOk items (percentages and total numbers) and
    /// a collection of ReportDeviationItem containing Todo items (percentages and total numbers).
    /// </summary>
    public class ReportTaskDeviations
    {
        public List<ReportDeviationItem> DeviationsSkipped { get; set; }
        public List<ReportDeviationItem> DeviationsNotOk { get; set; }
        public List<ReportDeviationItem> DeviationsTodo { get; set; }

        public ReportTaskDeviations()
        {
            DeviationsSkipped = new List<ReportDeviationItem>();
            DeviationsNotOk = new List<ReportDeviationItem>();
            DeviationsTodo = new List<ReportDeviationItem>();
        }
    }
}
